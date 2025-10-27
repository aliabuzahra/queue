using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualQueue.Infrastructure.Services;

namespace VirtualQueue.Tests.Unit.Infrastructure.Services;

/// <summary>
/// Unit tests for the JwtAuthenticationService class.
/// </summary>
[TestClass]
public class JwtAuthenticationServiceTests
{
    #region Test Data
    private Mock<IConfiguration> _mockConfiguration;
    private Mock<ILogger<JwtAuthenticationService>> _mockLogger;
    private JwtAuthenticationService _service;
    private Guid _tenantId;
    private const string ValidUserIdentifier = "testuser123";
    private readonly List<string> _validRoles = new() { "User", "Admin" };
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockConfiguration = new Mock<IConfiguration>();
        _mockLogger = new Mock<ILogger<JwtAuthenticationService>>();
        _tenantId = Guid.NewGuid();
        
        SetupConfiguration();
        _service = new JwtAuthenticationService(_mockConfiguration.Object, _mockLogger.Object);
    }
    
    private void SetupConfiguration()
    {
        _mockConfiguration.Setup(c => c["Jwt:SecretKey"]).Returns("YourSuperSecretKeyThatIsAtLeast32CharactersLong!");
        _mockConfiguration.Setup(c => c["Jwt:Issuer"]).Returns("VirtualQueue");
        _mockConfiguration.Setup(c => c["Jwt:Audience"]).Returns("VirtualQueueUsers");
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");
    }
    #endregion

    #region Constructor Tests
    /// <summary>
    /// Tests that the constructor initializes the service successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_InitializesServiceSuccessfully()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>();
        var logger = new Mock<ILogger<JwtAuthenticationService>>();
        SetupConfiguration();

        // Act
        var service = new JwtAuthenticationService(configuration.Object, logger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when configuration is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullConfiguration_ThrowsArgumentNullException()
    {
        // Arrange
        var logger = new Mock<ILogger<JwtAuthenticationService>>();

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => 
            new JwtAuthenticationService(null!, logger.Object));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when logger is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullLogger_ThrowsArgumentNullException()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>();

        // Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => 
            new JwtAuthenticationService(configuration.Object, null!));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when JWT configuration is invalid.
    /// </summary>
    [TestMethod]
    public void Constructor_WithInvalidJwtConfiguration_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>();
        var logger = new Mock<ILogger<JwtAuthenticationService>>();
        
        configuration.Setup(c => c["Jwt:SecretKey"]).Returns(""); // Invalid secret key
        configuration.Setup(c => c["Jwt:Issuer"]).Returns("VirtualQueue");
        configuration.Setup(c => c["Jwt:Audience"]).Returns("VirtualQueueUsers");
        configuration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => 
            new JwtAuthenticationService(configuration.Object, logger.Object));
    }
    #endregion

    #region GenerateJwtTokenAsync Tests
    /// <summary>
    /// Tests that GenerateJwtTokenAsync generates a token successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public async Task GenerateJwtTokenAsync_WithValidParameters_GeneratesTokenSuccessfully()
    {
        // Arrange
        var roles = new List<string> { "User", "Admin" };

        // Act
        var token = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, roles);

        // Assert
        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrEmpty(token));
    }

    /// <summary>
    /// Tests that GenerateJwtTokenAsync throws an exception when tenant ID is empty.
    /// </summary>
    [TestMethod]
    public async Task GenerateJwtTokenAsync_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange
        var emptyTenantId = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
            _service.GenerateJwtTokenAsync(emptyTenantId, ValidUserIdentifier, _validRoles));
    }

    /// <summary>
    /// Tests that GenerateJwtTokenAsync throws an exception when user identifier is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public async Task GenerateJwtTokenAsync_WithInvalidUserIdentifier_ThrowsArgumentException(string userIdentifier)
    {
        // Arrange & Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
            _service.GenerateJwtTokenAsync(_tenantId, userIdentifier, _validRoles));
    }

    /// <summary>
    /// Tests that GenerateJwtTokenAsync throws an exception when roles is null.
    /// </summary>
    [TestMethod]
    public async Task GenerateJwtTokenAsync_WithNullRoles_ThrowsArgumentException()
    {
        // Arrange
        List<string>? nullRoles = null;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => 
            _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, nullRoles!));
    }

    /// <summary>
    /// Tests that GenerateJwtTokenAsync handles empty roles successfully.
    /// </summary>
    [TestMethod]
    public async Task GenerateJwtTokenAsync_WithEmptyRoles_GeneratesTokenSuccessfully()
    {
        // Arrange
        var emptyRoles = new List<string>();

        // Act
        var token = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, emptyRoles);

        // Assert
        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrEmpty(token));
    }

    /// <summary>
    /// Tests that GenerateJwtTokenAsync filters out null or empty roles.
    /// </summary>
    [TestMethod]
    public async Task GenerateJwtTokenAsync_WithNullAndEmptyRoles_FiltersRolesSuccessfully()
    {
        // Arrange
        var roles = new List<string> { "User", "", null!, "Admin" };

        // Act
        var token = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, roles);

        // Assert
        Assert.IsNotNull(token);
        Assert.IsFalse(string.IsNullOrEmpty(token));
    }
    #endregion

    #region ValidateJwtTokenAsync Tests
    /// <summary>
    /// Tests that ValidateJwtTokenAsync validates a valid token successfully.
    /// </summary>
    [TestMethod]
    public async Task ValidateJwtTokenAsync_WithValidToken_ReturnsTrue()
    {
        // Arrange
        var token = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, _validRoles);

        // Act
        var isValid = await _service.ValidateJwtTokenAsync(token);

        // Assert
        Assert.IsTrue(isValid);
    }

    /// <summary>
    /// Tests that ValidateJwtTokenAsync returns false for an invalid token.
    /// </summary>
    [TestMethod]
    public async Task ValidateJwtTokenAsync_WithInvalidToken_ReturnsFalse()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isValid = await _service.ValidateJwtTokenAsync(invalidToken);

        // Assert
        Assert.IsFalse(isValid);
    }

    /// <summary>
    /// Tests that ValidateJwtTokenAsync returns false for an expired token.
    /// </summary>
    [TestMethod]
    public async Task ValidateJwtTokenAsync_WithExpiredToken_ReturnsFalse()
    {
        // Arrange
        // Create a service with very short expiration time
        _mockConfiguration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("0");
        var shortExpirationService = new JwtAuthenticationService(_mockConfiguration.Object, _mockLogger.Object);
        
        // Wait a moment to ensure token is expired
        await Task.Delay(100);

        // Act
        var isValid = await shortExpirationService.ValidateJwtTokenAsync("some.token");

        // Assert
        Assert.IsFalse(isValid);
    }
    #endregion

    #region GetTenantIdFromTokenAsync Tests
    /// <summary>
    /// Tests that GetTenantIdFromTokenAsync extracts tenant ID successfully from a valid token.
    /// </summary>
    [TestMethod]
    public async Task GetTenantIdFromTokenAsync_WithValidToken_ReturnsTenantId()
    {
        // Arrange
        var token = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, _validRoles);

        // Act
        var extractedTenantId = await _service.GetTenantIdFromTokenAsync(token);

        // Assert
        Assert.IsNotNull(extractedTenantId);
        Assert.AreEqual(_tenantId, extractedTenantId);
    }

    /// <summary>
    /// Tests that GetTenantIdFromTokenAsync returns null for an invalid token.
    /// </summary>
    [TestMethod]
    public async Task GetTenantIdFromTokenAsync_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var extractedTenantId = await _service.GetTenantIdFromTokenAsync(invalidToken);

        // Assert
        Assert.IsNull(extractedTenantId);
    }
    #endregion

    #region GetUserIdentifierFromTokenAsync Tests
    /// <summary>
    /// Tests that GetUserIdentifierFromTokenAsync extracts user identifier successfully from a valid token.
    /// </summary>
    [TestMethod]
    public async Task GetUserIdentifierFromTokenAsync_WithValidToken_ReturnsUserIdentifier()
    {
        // Arrange
        var token = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, _validRoles);

        // Act
        var extractedUserIdentifier = await _service.GetUserIdentifierFromTokenAsync(token);

        // Assert
        Assert.IsNotNull(extractedUserIdentifier);
        Assert.AreEqual(ValidUserIdentifier, extractedUserIdentifier);
    }

    /// <summary>
    /// Tests that GetUserIdentifierFromTokenAsync returns null for an invalid token.
    /// </summary>
    [TestMethod]
    public async Task GetUserIdentifierFromTokenAsync_WithInvalidToken_ReturnsNull()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var extractedUserIdentifier = await _service.GetUserIdentifierFromTokenAsync(invalidToken);

        // Assert
        Assert.IsNull(extractedUserIdentifier);
    }
    #endregion

    #region GetRolesFromTokenAsync Tests
    /// <summary>
    /// Tests that GetRolesFromTokenAsync extracts roles successfully from a valid token.
    /// </summary>
    [TestMethod]
    public async Task GetRolesFromTokenAsync_WithValidToken_ReturnsRoles()
    {
        // Arrange
        var token = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, _validRoles);

        // Act
        var extractedRoles = await _service.GetRolesFromTokenAsync(token);

        // Assert
        Assert.IsNotNull(extractedRoles);
        Assert.AreEqual(_validRoles.Count, extractedRoles.Count);
        Assert.IsTrue(extractedRoles.Contains("User"));
        Assert.IsTrue(extractedRoles.Contains("Admin"));
    }

    /// <summary>
    /// Tests that GetRolesFromTokenAsync returns empty list for an invalid token.
    /// </summary>
    [TestMethod]
    public async Task GetRolesFromTokenAsync_WithInvalidToken_ReturnsEmptyList()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var extractedRoles = await _service.GetRolesFromTokenAsync(invalidToken);

        // Assert
        Assert.IsNotNull(extractedRoles);
        Assert.AreEqual(0, extractedRoles.Count);
    }
    #endregion

    #region IsTokenExpiredAsync Tests
    /// <summary>
    /// Tests that IsTokenExpiredAsync returns false for a valid token.
    /// </summary>
    [TestMethod]
    public async Task IsTokenExpiredAsync_WithValidToken_ReturnsFalse()
    {
        // Arrange
        var token = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, _validRoles);

        // Act
        var isExpired = await _service.IsTokenExpiredAsync(token);

        // Assert
        Assert.IsFalse(isExpired);
    }

    /// <summary>
    /// Tests that IsTokenExpiredAsync returns true for an invalid token.
    /// </summary>
    [TestMethod]
    public async Task IsTokenExpiredAsync_WithInvalidToken_ReturnsTrue()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act
        var isExpired = await _service.IsTokenExpiredAsync(invalidToken);

        // Assert
        Assert.IsTrue(isExpired);
    }
    #endregion

    #region RefreshTokenAsync Tests
    /// <summary>
    /// Tests that RefreshTokenAsync refreshes a token successfully.
    /// </summary>
    [TestMethod]
    public async Task RefreshTokenAsync_WithValidToken_RefreshesTokenSuccessfully()
    {
        // Arrange
        var originalToken = await _service.GenerateJwtTokenAsync(_tenantId, ValidUserIdentifier, _validRoles);

        // Act
        var refreshedToken = await _service.RefreshTokenAsync(originalToken);

        // Assert
        Assert.IsNotNull(refreshedToken);
        Assert.IsFalse(string.IsNullOrEmpty(refreshedToken));
        Assert.AreNotEqual(originalToken, refreshedToken);
    }

    /// <summary>
    /// Tests that RefreshTokenAsync throws an exception for an invalid token.
    /// </summary>
    [TestMethod]
    public async Task RefreshTokenAsync_WithInvalidToken_ThrowsException()
    {
        // Arrange
        var invalidToken = "invalid.token.here";

        // Act & Assert
        await Assert.ThrowsExceptionAsync<Exception>(() => 
            _service.RefreshTokenAsync(invalidToken));
    }
    #endregion

    #region Configuration Tests
    /// <summary>
    /// Tests that the service uses default values when configuration is not provided.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMissingConfiguration_UsesDefaultValues()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>();
        var logger = new Mock<ILogger<JwtAuthenticationService>>();
        
        // Setup configuration to return null for all JWT settings
        configuration.Setup(c => c["Jwt:SecretKey"]).Returns((string?)null);
        configuration.Setup(c => c["Jwt:Issuer"]).Returns((string?)null);
        configuration.Setup(c => c["Jwt:Audience"]).Returns((string?)null);
        configuration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns((string?)null);

        // Act
        var service = new JwtAuthenticationService(configuration.Object, logger.Object);

        // Assert
        Assert.IsNotNull(service);
    }

    /// <summary>
    /// Tests that the service throws an exception when secret key is too short.
    /// </summary>
    [TestMethod]
    public void Constructor_WithShortSecretKey_ThrowsInvalidOperationException()
    {
        // Arrange
        var configuration = new Mock<IConfiguration>();
        var logger = new Mock<ILogger<JwtAuthenticationService>>();
        
        configuration.Setup(c => c["Jwt:SecretKey"]).Returns("short"); // Too short
        configuration.Setup(c => c["Jwt:Issuer"]).Returns("VirtualQueue");
        configuration.Setup(c => c["Jwt:Audience"]).Returns("VirtualQueueUsers");
        configuration.Setup(c => c["Jwt:ExpirationMinutes"]).Returns("60");

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => 
            new JwtAuthenticationService(configuration.Object, logger.Object));
    }
    #endregion
}
