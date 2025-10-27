using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Infrastructure.Data;
using VirtualQueue.Infrastructure.Repositories;

namespace VirtualQueue.Tests.Unit.Infrastructure.Repositories;

/// <summary>
/// Unit tests for the BaseRepository class.
/// </summary>
[TestClass]
public class BaseRepositoryTests
{
    #region Test Data
    private Mock<VirtualQueueDbContext> _mockContext;
    private Mock<DbSet<User>> _mockDbSet;
    private TestRepository _repository;
    private User _testUser;
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockContext = new Mock<VirtualQueueDbContext>();
        _mockDbSet = new Mock<DbSet<User>>();
        _repository = new TestRepository(_mockContext.Object);
        
        _testUser = new User(
            Guid.NewGuid(),
            "testuser",
            "test@example.com",
            "hashedpassword",
            "Test",
            "User");
    }
    #endregion

    #region Constructor Tests
    /// <summary>
    /// Tests that the constructor initializes the repository successfully with valid context.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidContext_InitializesRepositorySuccessfully()
    {
        // Arrange
        var context = new Mock<VirtualQueueDbContext>();

        // Act
        var repository = new TestRepository(context.Object);

        // Assert
        Assert.IsNotNull(repository);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when context is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullContext_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new TestRepository(null!));
    }
    #endregion

    #region GetByIdAsync Tests
    /// <summary>
    /// Tests that GetByIdAsync returns the entity when found.
    /// </summary>
    [TestMethod]
    public async Task GetByIdAsync_WithValidId_ReturnsEntity()
    {
        // Arrange
        var userId = _testUser.Id;
        var users = new List<User> { _testUser }.AsQueryable();
        
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        
        _mockContext.Setup(c => c.Set<User>()).Returns(_mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testUser.Id, result.Id);
    }

    /// <summary>
    /// Tests that GetByIdAsync returns null when entity is not found.
    /// </summary>
    [TestMethod]
    public async Task GetByIdAsync_WithNonExistentId_ReturnsNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var users = new List<User>().AsQueryable();
        
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        
        _mockContext.Setup(c => c.Set<User>()).Returns(_mockDbSet.Object);

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        Assert.IsNull(result);
    }

    /// <summary>
    /// Tests that GetByIdAsync throws an exception when ID is empty.
    /// </summary>
    [TestMethod]
    public async Task GetByIdAsync_WithEmptyId_ThrowsArgumentException()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentException>(() => _repository.GetByIdAsync(emptyId));
    }
    #endregion

    #region GetAllAsync Tests
    /// <summary>
    /// Tests that GetAllAsync returns all entities.
    /// </summary>
    [TestMethod]
    public async Task GetAllAsync_ReturnsAllEntities()
    {
        // Arrange
        var users = new List<User> { _testUser }.AsQueryable();
        
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.Provider).Returns(users.Provider);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.Expression).Returns(users.Expression);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.ElementType).Returns(users.ElementType);
        _mockDbSet.As<IQueryable<User>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());
        
        _mockContext.Setup(c => c.Set<User>()).Returns(_mockDbSet.Object);

        // Act
        var result = await _repository.GetAllAsync();

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(1, result.Count());
        Assert.AreEqual(_testUser.Id, result.First().Id);
    }
    #endregion

    #region AddAsync Tests
    /// <summary>
    /// Tests that AddAsync adds the entity successfully.
    /// </summary>
    [TestMethod]
    public async Task AddAsync_WithValidEntity_AddsEntitySuccessfully()
    {
        // Arrange
        _mockContext.Setup(c => c.Set<User>()).Returns(_mockDbSet.Object);
        _mockDbSet.Setup(s => s.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                 .Returns(ValueTask.FromResult((EntityEntry<User>)null!));

        // Act
        var result = await _repository.AddAsync(_testUser);

        // Assert
        Assert.IsNotNull(result);
        Assert.AreEqual(_testUser.Id, result.Id);
        _mockDbSet.Verify(s => s.AddAsync(_testUser, It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that AddAsync throws an exception when entity is null.
    /// </summary>
    [TestMethod]
    public async Task AddAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        User? nullEntity = null;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _repository.AddAsync(nullEntity!));
    }
    #endregion

    #region UpdateAsync Tests
    /// <summary>
    /// Tests that UpdateAsync updates the entity successfully.
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithValidEntity_UpdatesEntitySuccessfully()
    {
        // Arrange
        _mockContext.Setup(c => c.Set<User>()).Returns(_mockDbSet.Object);

        // Act
        await _repository.UpdateAsync(_testUser);

        // Assert
        _mockDbSet.Verify(s => s.Update(_testUser), Times.Once);
    }

    /// <summary>
    /// Tests that UpdateAsync throws an exception when entity is null.
    /// </summary>
    [TestMethod]
    public async Task UpdateAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        User? nullEntity = null;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _repository.UpdateAsync(nullEntity!));
    }
    #endregion

    #region DeleteAsync Tests
    /// <summary>
    /// Tests that DeleteAsync deletes the entity successfully.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_WithValidEntity_DeletesEntitySuccessfully()
    {
        // Arrange
        _mockContext.Setup(c => c.Set<User>()).Returns(_mockDbSet.Object);

        // Act
        await _repository.DeleteAsync(_testUser);

        // Assert
        _mockDbSet.Verify(s => s.Remove(_testUser), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteAsync throws an exception when entity is null.
    /// </summary>
    [TestMethod]
    public async Task DeleteAsync_WithNullEntity_ThrowsArgumentNullException()
    {
        // Arrange
        User? nullEntity = null;

        // Act & Assert
        await Assert.ThrowsExceptionAsync<ArgumentNullException>(() => _repository.DeleteAsync(nullEntity!));
    }
    #endregion

    #region SaveChangesAsync Tests
    /// <summary>
    /// Tests that SaveChangesAsync saves changes successfully.
    /// </summary>
    [TestMethod]
    public async Task SaveChangesAsync_SavesChangesSuccessfully()
    {
        // Arrange
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ReturnsAsync(1);

        // Act
        await _repository.SaveChangesAsync();

        // Assert
        _mockContext.Verify(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that SaveChangesAsync throws an exception when database update fails.
    /// </summary>
    [TestMethod]
    public async Task SaveChangesAsync_WhenDatabaseUpdateFails_ThrowsInvalidOperationException()
    {
        // Arrange
        _mockContext.Setup(c => c.SaveChangesAsync(It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new DbUpdateException("Database update failed"));

        // Act & Assert
        await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => _repository.SaveChangesAsync());
    }
    #endregion

    #region Test Helper Class
    /// <summary>
    /// Test implementation of BaseRepository for testing purposes.
    /// </summary>
    private class TestRepository : BaseRepository<User>
    {
        public TestRepository(VirtualQueueDbContext context) : base(context)
        {
        }
    }
    #endregion
}
