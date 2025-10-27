using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.ValueObjects;

namespace VirtualQueue.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for the Queue entity.
/// </summary>
[TestClass]
public class QueueTests
{
    #region Test Data
    private Queue _queue;
    private Guid _tenantId;
    private const string ValidName = "Test Queue";
    private const string ValidDescription = "Test Queue Description";
    private const int ValidMaxConcurrentUsers = 10;
    private const int ValidReleaseRatePerMinute = 5;
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _tenantId = Guid.NewGuid();
        _queue = new Queue(
            _tenantId,
            ValidName,
            ValidDescription,
            ValidMaxConcurrentUsers,
            ValidReleaseRatePerMinute);
    }
    #endregion

    #region Constructor Tests
    /// <summary>
    /// Tests that the constructor creates a queue successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_CreatesQueueSuccessfully()
    {
        // Arrange & Act
        var queue = new Queue(
            _tenantId,
            ValidName,
            ValidDescription,
            ValidMaxConcurrentUsers,
            ValidReleaseRatePerMinute);

        // Assert
        Assert.IsNotNull(queue);
        Assert.AreEqual(_tenantId, queue.TenantId);
        Assert.AreEqual(ValidName, queue.Name);
        Assert.AreEqual(ValidDescription, queue.Description);
        Assert.AreEqual(ValidMaxConcurrentUsers, queue.MaxConcurrentUsers);
        Assert.AreEqual(ValidReleaseRatePerMinute, queue.ReleaseRatePerMinute);
        Assert.IsTrue(queue.IsActive);
        Assert.IsNotNull(queue.Users);
        Assert.AreEqual(0, queue.Users.Count);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when tenant ID is empty.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyTenantId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new Queue(Guid.Empty, ValidName, ValidDescription, ValidMaxConcurrentUsers, ValidReleaseRatePerMinute));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when name is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void Constructor_WithInvalidName_ThrowsArgumentException(string name)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new Queue(_tenantId, name, ValidDescription, ValidMaxConcurrentUsers, ValidReleaseRatePerMinute));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when name is too short.
    /// </summary>
    [TestMethod]
    public void Constructor_WithShortName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new Queue(_tenantId, "", ValidDescription, ValidMaxConcurrentUsers, ValidReleaseRatePerMinute));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when name is too long.
    /// </summary>
    [TestMethod]
    public void Constructor_WithLongName_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var longName = new string('a', 101); // 101 characters
        Assert.ThrowsException<ArgumentException>(() =>
            new Queue(_tenantId, longName, ValidDescription, ValidMaxConcurrentUsers, ValidReleaseRatePerMinute));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when description is too long.
    /// </summary>
    [TestMethod]
    public void Constructor_WithLongDescription_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        var longDescription = new string('a', 501); // 501 characters
        Assert.ThrowsException<ArgumentException>(() =>
            new Queue(_tenantId, ValidName, longDescription, ValidMaxConcurrentUsers, ValidReleaseRatePerMinute));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when max concurrent users is invalid.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(10001)]
    public void Constructor_WithInvalidMaxConcurrentUsers_ThrowsArgumentException(int maxConcurrentUsers)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new Queue(_tenantId, ValidName, ValidDescription, maxConcurrentUsers, ValidReleaseRatePerMinute));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when release rate is invalid.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1001)]
    public void Constructor_WithInvalidReleaseRate_ThrowsArgumentException(int releaseRatePerMinute)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new Queue(_tenantId, ValidName, ValidDescription, ValidMaxConcurrentUsers, releaseRatePerMinute));
    }
    #endregion

    #region UpdateName Tests
    /// <summary>
    /// Tests that UpdateName updates the name successfully with valid name.
    /// </summary>
    [TestMethod]
    public void UpdateName_WithValidName_UpdatesNameSuccessfully()
    {
        // Arrange
        var newName = "Updated Queue Name";

        // Act
        _queue.UpdateName(newName);

        // Assert
        Assert.AreEqual(newName, _queue.Name);
    }

    /// <summary>
    /// Tests that UpdateName throws an exception when name is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void UpdateName_WithInvalidName_ThrowsArgumentException(string name)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _queue.UpdateName(name));
    }

    /// <summary>
    /// Tests that UpdateName throws an exception when name is too long.
    /// </summary>
    [TestMethod]
    public void UpdateName_WithLongName_ThrowsArgumentException()
    {
        // Arrange
        var longName = new string('a', 101); // 101 characters

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _queue.UpdateName(longName));
    }
    #endregion

    #region UpdateDescription Tests
    /// <summary>
    /// Tests that UpdateDescription updates the description successfully.
    /// </summary>
    [TestMethod]
    public void UpdateDescription_WithValidDescription_UpdatesDescriptionSuccessfully()
    {
        // Arrange
        var newDescription = "Updated Queue Description";

        // Act
        _queue.UpdateDescription(newDescription);

        // Assert
        Assert.AreEqual(newDescription, _queue.Description);
    }

    /// <summary>
    /// Tests that UpdateDescription throws an exception when description is too long.
    /// </summary>
    [TestMethod]
    public void UpdateDescription_WithLongDescription_ThrowsArgumentException()
    {
        // Arrange
        var longDescription = new string('a', 501); // 501 characters

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _queue.UpdateDescription(longDescription));
    }
    #endregion

    #region UpdateConfiguration Tests
    /// <summary>
    /// Tests that UpdateConfiguration updates the configuration successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public void UpdateConfiguration_WithValidParameters_UpdatesConfigurationSuccessfully()
    {
        // Arrange
        var newMaxConcurrentUsers = 20;
        var newReleaseRatePerMinute = 10;

        // Act
        _queue.UpdateConfiguration(newMaxConcurrentUsers, newReleaseRatePerMinute);

        // Assert
        Assert.AreEqual(newMaxConcurrentUsers, _queue.MaxConcurrentUsers);
        Assert.AreEqual(newReleaseRatePerMinute, _queue.ReleaseRatePerMinute);
    }

    /// <summary>
    /// Tests that UpdateConfiguration throws an exception when max concurrent users is invalid.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(10001)]
    public void UpdateConfiguration_WithInvalidMaxConcurrentUsers_ThrowsArgumentException(int maxConcurrentUsers)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => 
            _queue.UpdateConfiguration(maxConcurrentUsers, ValidReleaseRatePerMinute));
    }

    /// <summary>
    /// Tests that UpdateConfiguration throws an exception when release rate is invalid.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    [DataRow(1001)]
    public void UpdateConfiguration_WithInvalidReleaseRate_ThrowsArgumentException(int releaseRatePerMinute)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => 
            _queue.UpdateConfiguration(ValidMaxConcurrentUsers, releaseRatePerMinute));
    }
    #endregion

    #region Activation Tests
    /// <summary>
    /// Tests that Activate activates the queue successfully.
    /// </summary>
    [TestMethod]
    public void Activate_ActivatesQueueSuccessfully()
    {
        // Arrange
        _queue.Deactivate();
        Assert.IsFalse(_queue.IsActive);

        // Act
        _queue.Activate();

        // Assert
        Assert.IsTrue(_queue.IsActive);
    }

    /// <summary>
    /// Tests that Deactivate deactivates the queue successfully.
    /// </summary>
    [TestMethod]
    public void Deactivate_DeactivatesQueueSuccessfully()
    {
        // Arrange
        Assert.IsTrue(_queue.IsActive);

        // Act
        _queue.Deactivate();

        // Assert
        Assert.IsFalse(_queue.IsActive);
    }
    #endregion

    #region EnqueueUser Tests
    /// <summary>
    /// Tests that EnqueueUser adds a user to the queue successfully.
    /// </summary>
    [TestMethod]
    public void EnqueueUser_WithValidUserIdentifier_AddsUserSuccessfully()
    {
        // Arrange
        var userIdentifier = "testuser123";
        var metadata = "test metadata";

        // Act
        var userSession = _queue.EnqueueUser(userIdentifier, metadata);

        // Assert
        Assert.IsNotNull(userSession);
        Assert.AreEqual(userIdentifier, userSession.UserIdentifier);
        Assert.AreEqual(metadata, userSession.Metadata);
        Assert.AreEqual(1, _queue.Users.Count);
    }

    /// <summary>
    /// Tests that EnqueueUser throws an exception when user identifier is invalid.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void EnqueueUser_WithInvalidUserIdentifier_ThrowsArgumentException(string userIdentifier)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _queue.EnqueueUser(userIdentifier));
    }

    /// <summary>
    /// Tests that EnqueueUser throws an exception when queue is inactive.
    /// </summary>
    [TestMethod]
    public void EnqueueUser_WhenQueueIsInactive_ThrowsInvalidOperationException()
    {
        // Arrange
        _queue.Deactivate();
        var userIdentifier = "testuser123";

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => _queue.EnqueueUser(userIdentifier));
    }
    #endregion

    #region ReleaseUsers Tests
    /// <summary>
    /// Tests that ReleaseUsers releases users successfully.
    /// </summary>
    [TestMethod]
    public void ReleaseUsers_WithValidCount_ReleasesUsersSuccessfully()
    {
        // Arrange
        var user1 = _queue.EnqueueUser("user1");
        var user2 = _queue.EnqueueUser("user2");
        var user3 = _queue.EnqueueUser("user3");

        // Act
        _queue.ReleaseUsers(2);

        // Assert
        Assert.AreEqual(2, _queue.Users.Count(u => u.Status == Domain.Enums.QueueStatus.Released));
        Assert.AreEqual(1, _queue.Users.Count(u => u.Status == Domain.Enums.QueueStatus.Waiting));
    }

    /// <summary>
    /// Tests that ReleaseUsers does nothing when count is zero or negative.
    /// </summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void ReleaseUsers_WithZeroOrNegativeCount_DoesNothing(int count)
    {
        // Arrange
        var user1 = _queue.EnqueueUser("user1");
        var user2 = _queue.EnqueueUser("user2");

        // Act
        _queue.ReleaseUsers(count);

        // Assert
        Assert.AreEqual(0, _queue.Users.Count(u => u.Status == Domain.Enums.QueueStatus.Released));
        Assert.AreEqual(2, _queue.Users.Count(u => u.Status == Domain.Enums.QueueStatus.Waiting));
    }
    #endregion

    #region Utility Method Tests
    /// <summary>
    /// Tests that GetWaitingUsersCount returns the correct count.
    /// </summary>
    [TestMethod]
    public void GetWaitingUsersCount_ReturnsCorrectCount()
    {
        // Arrange
        var user1 = _queue.EnqueueUser("user1");
        var user2 = _queue.EnqueueUser("user2");
        var user3 = _queue.EnqueueUser("user3");

        // Act
        var count = _queue.GetWaitingUsersCount();

        // Assert
        Assert.AreEqual(3, count);
    }

    /// <summary>
    /// Tests that GetServingUsersCount returns the correct count.
    /// </summary>
    [TestMethod]
    public void GetServingUsersCount_ReturnsCorrectCount()
    {
        // Arrange
        var user1 = _queue.EnqueueUser("user1");
        var user2 = _queue.EnqueueUser("user2");
        user1.MarkAsServed();
        user2.MarkAsServed();

        // Act
        var count = _queue.GetServingUsersCount();

        // Assert
        Assert.AreEqual(2, count);
    }
    #endregion

    #region Schedule Tests
    /// <summary>
    /// Tests that SetSchedule sets the schedule successfully.
    /// </summary>
    [TestMethod]
    public void SetSchedule_WithValidSchedule_SetsScheduleSuccessfully()
    {
        // Arrange
        var schedule = new QueueSchedule(
            new BusinessHours(TimeSpan.FromHours(9), TimeSpan.FromHours(17), "UTC"),
            new List<DayOfWeek> { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday });

        // Act
        _queue.SetSchedule(schedule);

        // Assert
        Assert.AreEqual(schedule, _queue.Schedule);
    }
    #endregion

    #region Availability Tests
    /// <summary>
    /// Tests that IsQueueAvailable returns true when queue is active and no schedule is set.
    /// </summary>
    [TestMethod]
    public void IsQueueAvailable_WhenActiveAndNoSchedule_ReturnsTrue()
    {
        // Arrange
        var dateTime = DateTime.UtcNow;

        // Act
        var isAvailable = _queue.IsQueueAvailable(dateTime);

        // Assert
        Assert.IsTrue(isAvailable);
    }

    /// <summary>
    /// Tests that IsQueueAvailable returns false when queue is inactive.
    /// </summary>
    [TestMethod]
    public void IsQueueAvailable_WhenInactive_ReturnsFalse()
    {
        // Arrange
        _queue.Deactivate();
        var dateTime = DateTime.UtcNow;

        // Act
        var isAvailable = _queue.IsQueueAvailable(dateTime);

        // Assert
        Assert.IsFalse(isAvailable);
    }
    #endregion

    #region Domain Events Tests
    /// <summary>
    /// Tests that ClearDomainEvents clears all domain events.
    /// </summary>
    [TestMethod]
    public void ClearDomainEvents_ClearsAllDomainEvents()
    {
        // Arrange
        _queue.UpdateName("New Name");
        Assert.IsTrue(_queue.DomainEvents.Count > 0);

        // Act
        _queue.ClearDomainEvents();

        // Assert
        Assert.AreEqual(0, _queue.DomainEvents.Count);
    }
    #endregion
}
