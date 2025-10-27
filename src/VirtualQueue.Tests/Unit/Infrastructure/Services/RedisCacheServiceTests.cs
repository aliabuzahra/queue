using Microsoft.Extensions.Caching.Distributed;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualQueue.Infrastructure.Services;

namespace VirtualQueue.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for the RedisCacheService class.
/// </summary>
[TestClass]
public class RedisCacheServiceTests
{
    #region Test Data
    private Mock<IDistributedCache> _mockCache;
    private RedisCacheService _service;
    private const string ValidKey = "test-key";
    private const string ValidValue = "test-value";
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockCache = new Mock<IDistributedCache>();
        _service = new RedisCacheService(_mockCache.Object);
    }
    #endregion

    #region Constructor Tests
    /// <summary>
    /// Tests that the constructor initializes the service successfully with valid cache.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidCache_InitializesServiceSuccessfully()
    {
        // Arrange
        var cache = new Mock<IDistributedCache>();

        // Act
        var service = new RedisCacheService(cache.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when cache is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullCache_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new RedisCacheService(null!));
    }
    #endregion

    #region GetAsync Tests
    /// <summary>
    /// Tests that GetAsync returns a value successfully when found.
    /// </summary>
    [TestMethod]
    public async Task GetAsync_WithValidKey_ReturnsValueSuccessfully()
    {
        // Arrange
        var expectedValue = new TestObject { Id = 1, Name = "Test" };
        var jsonValue = System.Text.Json.JsonSerializer.Serialize(expectedValue);
        
        _mockCache.Setup(c => c.GetStringAsync(ValidKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(jsonValue);

        // Act
        var result = await _service.GetAsync<TestObject>(ValidKey);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(expectedValue.Id, result.Id);
        Assert.AreEqual(expectedValue.Name, result.Name);
    }

    /// <summary>
    /// Tests that GetAsync returns null when key is not found.
    /// </summary>
    [TestMethod]
    public async Task GetAsync_WithNonExistentKey_ReturnsNull()
    {
        // Arrange
        _mockCache.Setup(c => c.GetStringAsync(ValidKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((string?)null);

        // Act
        var result = await _service.GetAsync<TestObject>(ValidKey);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetAsync returns null when value is empty.
    /// </summary>
    [TestMethod]
    public async Task GetAsync_WithEmptyValue_ReturnsNull()
    {
        // Arrange
        _mockCache.Setup(c => c.GetStringAsync(ValidKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync("");

        // Act
        var result = await _service.GetAsync<TestObject>(ValidKey);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetAsync throws an exception when key is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task GetAsync_WithInvalidKey_ThrowsArgumentException(string key)
    {
        // Arrange & Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.GetAsync<TestObject>(key!));
    }

    /// <summary>
    /// Tests that GetAsync throws an exception when JSON is invalid.
    /// </summary>
    [TestMethod]
    public async Task GetAsync_WithInvalidJson_ThrowsInvalidOperationException()
    {
        // Arrange
        var invalidJson = "invalid-json";
        _mockCache.Setup(c => c.GetStringAsync(ValidKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(invalidJson);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _service.GetAsync<TestObject>(ValidKey));
    }
    #endregion

    #region SetAsync Tests
    /// <summary>
    /// Tests that SetAsync sets a value successfully.
    /// </summary>
    [TestMethod]
    public async Task SetAsync_WithValidParameters_SetsValueSuccessfully()
    {
        // Arrange
        var value = new TestObject { Id = 1, Name = "Test" };
        var expiration = TimeSpan.FromMinutes(30);

        // Act
        await _service.SetAsync(ValidKey, value, expiration);

        // Assert
        _mockCache.Verify(c => c.SetStringAsync(
            ValidKey,
            It.IsAny<string>(),
            It.Is<DistributedCacheEntryOptions>(opt => opt.AbsoluteExpiration.HasValue),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SetAsync sets a value with default expiration when none provided.
    /// </summary>
    [TestMethod]
    public async Task SetAsync_WithNoExpiration_SetsValueWithDefaultExpiration()
    {
        // Arrange
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        await _service.SetAsync(ValidKey, value);

        // Assert
        _mockCache.Verify(c => c.SetStringAsync(
            ValidKey,
            It.IsAny<string>(),
            It.Is<DistributedCacheEntryOptions>(opt => opt.AbsoluteExpiration.HasValue),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SetAsync throws an exception when key is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task SetAsync_WithInvalidKey_ThrowsArgumentException(string key)
    {
        // Arrange
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.SetAsync(key!, value));
    }
    #endregion

    #region RemoveAsync Tests
    /// <summary>
    /// Tests that RemoveAsync removes a value successfully.
    /// </summary>
    [TestMethod]
    public async Task RemoveAsync_WithValidKey_RemovesValueSuccessfully()
    {
        // Arrange & Act
        await _service.RemoveAsync(ValidKey);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync(ValidKey, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RemoveAsync throws an exception when key is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task RemoveAsync_WithInvalidKey_ThrowsArgumentException(string key)
    {
        // Arrange & Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _service.RemoveAsync(key!));
    }
    #endregion

    #region ExistsAsync Tests
    /// <summary>
    /// Tests that ExistsAsync returns true when key exists.
    /// </summary>
    [TestMethod]
    public async Task ExistsAsync_WithExistingKey_ReturnsTrue()
    {
        // Arrange
        _mockCache.Setup(c => c.GetStringAsync(ValidKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync(ValidValue);

        // Act
        var exists = await _service.ExistsAsync(ValidKey);

        // Assert
        Assert.IsTrue(exists);
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when key does not exist.
    /// </summary>
    [TestMethod]
    public async Task ExistsAsync_WithNonExistentKey_ReturnsFalse()
    {
        // Arrange
        _mockCache.Setup(c => c.GetStringAsync(ValidKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync((string?)null);

        // Act
        var exists = await _service.ExistsAsync(ValidKey);

        // Assert
        Assert.IsFalse(exists);
    }

    /// <summary>
    /// Tests that ExistsAsync returns false when value is empty.
    /// </summary>
    [TestMethod]
    public async Task ExistsAsync_WithEmptyValue_ReturnsFalse()
    {
        // Arrange
        _mockCache.Setup(c => c.GetStringAsync(ValidKey, It.IsAny<CancellationToken>()))
                  .ReturnsAsync("");

        // Act
        var exists = await _service.ExistsAsync(ValidKey);

        // Assert
        Assert.IsFalse(exists);
    }
    #endregion

    #region SetUserPositionAsync Tests
    /// <summary>
    /// Tests that SetUserPositionAsync sets the user position successfully.
    /// </summary>
    [TestMethod]
    public async Task SetUserPositionAsync_WithValidParameters_SetsPositionSuccessfully()
    {
        // Arrange
        var queueId = Guid.NewGuid();
        var userIdentifier = "testuser";
        var position = 5;

        // Act
        await _service.SetUserPositionAsync(queueId, userIdentifier, position);

        // Assert
        _mockCache.Verify(c => c.SetStringAsync(
            It.Is<string>(k => k.Contains(queueId.ToString()) && k.Contains(userIdentifier)),
            position.ToString(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region GetUserPositionAsync Tests
    /// <summary>
    /// Tests that GetUserPositionAsync returns the user position successfully.
    /// </summary>
    [TestMethod]
    public async Task GetUserPositionAsync_WithValidParameters_ReturnsPositionSuccessfully()
    {
        // Arrange
        var queueId = Guid.NewGuid();
        var userIdentifier = "testuser";
        var expectedPosition = 5;
        
        _mockCache.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync(expectedPosition.ToString());

        // Act
        var position = await _service.GetUserPositionAsync(queueId, userIdentifier);

        // Assert
        Assert.AreEqual(expectedPosition, position);
    }

    /// <summary>
    /// Tests that GetUserPositionAsync returns null when position is not found.
    /// </summary>
    [TestMethod]
    public async Task GetUserPositionAsync_WithNonExistentPosition_ReturnsNull()
    {
        // Arrange
        var queueId = Guid.NewGuid();
        var userIdentifier = "testuser";
        
        _mockCache.Setup(c => c.GetStringAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ReturnsAsync((string?)null);

        // Act
        var position = await _service.GetUserPositionAsync(queueId, userIdentifier);

        // Assert
        Assert.IsNull(position);
    }
    #endregion

    #region RemoveUserPositionAsync Tests
    /// <summary>
    /// Tests that RemoveUserPositionAsync removes the user position successfully.
    /// </summary>
    [TestMethod]
    public async Task RemoveUserPositionAsync_WithValidParameters_RemovesPositionSuccessfully()
    {
        // Arrange
        var queueId = Guid.NewGuid();
        var userIdentifier = "testuser";

        // Act
        await _service.RemoveUserPositionAsync(queueId, userIdentifier);

        // Assert
        _mockCache.Verify(c => c.RemoveAsync(
            It.Is<string>(k => k.Contains(queueId.ToString()) && k.Contains(userIdentifier)),
            It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region Test Helper Class
    /// <summary>
    /// Test object for serialization testing.
    /// </summary>
    public class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
    #endregion
}
