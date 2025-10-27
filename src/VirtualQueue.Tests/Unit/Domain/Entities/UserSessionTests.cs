using Microsoft.VisualStudio.TestTools.UnitTesting;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Tests.Unit.Domain.Entities;

/// <summary>
/// Unit tests for the UserSession entity.
/// </summary>
[TestClass]
public class UserSessionTests
{
    #region Test Data
    private UserSession _userSession;
    private Guid _queueId;
    private const string ValidUserIdentifier = "testuser123";
    private const string ValidMetadata = "test metadata";
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _queueId = Guid.NewGuid();
        _userSession = new UserSession(
            _queueId,
            ValidUserIdentifier,
            ValidMetadata,
            QueuePriority.Normal);
    }
    #endregion

    #region Constructor Tests
    /// <summary>
    /// Tests that the constructor creates a user session successfully with valid parameters.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidParameters_CreatesUserSessionSuccessfully()
    {
        // Arrange & Act
        var userSession = new UserSession(
            _queueId,
            ValidUserIdentifier,
            ValidMetadata,
            QueuePriority.High);

        // Assert
        Assert.IsNotNull(userSession);
        Assert.AreEqual(_queueId, userSession.QueueId);
        Assert.AreEqual(ValidUserIdentifier, userSession.UserIdentifier);
        Assert.AreEqual(ValidMetadata, userSession.Metadata);
        Assert.AreEqual(QueuePriority.High, userSession.Priority);
        Assert.AreEqual(QueueStatus.Waiting, userSession.Status);
        Assert.IsNotNull(userSession.EnqueuedAt);
        Assert.IsNull(userSession.ReleasedAt);
        Assert.IsNull(userSession.ServedAt);
        Assert.AreEqual(0, userSession.Position);
    }

    /// <summary>
    /// Tests that the constructor creates a user session with default values.
    /// </summary>
    [TestMethod]
    public void Constructor_WithMinimalParameters_CreatesUserSessionWithDefaults()
    {
        // Arrange & Act
        var userSession = new UserSession(_queueId, ValidUserIdentifier);

        // Assert
        Assert.IsNotNull(userSession);
        Assert.AreEqual(_queueId, userSession.QueueId);
        Assert.AreEqual(ValidUserIdentifier, userSession.UserIdentifier);
        Assert.IsNull(userSession.Metadata);
        Assert.AreEqual(QueuePriority.Normal, userSession.Priority);
        Assert.AreEqual(QueueStatus.Waiting, userSession.Status);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when queue ID is empty.
    /// </summary>
    [TestMethod]
    public void Constructor_WithEmptyQueueId_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new UserSession(Guid.Empty, ValidUserIdentifier));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when user identifier is null.
    /// </summary>
    [TestMethod]
    [DataRow("")]
    [DataRow(" ")]
    [DataRow(null)]
    public void Constructor_WithInvalidUserIdentifier_ThrowsArgumentException(string userIdentifier)
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new UserSession(_queueId, userIdentifier));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when user identifier is too short.
    /// </summary>
    [TestMethod]
    public void Constructor_WithShortUserIdentifier_ThrowsArgumentException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new UserSession(_queueId, ""));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when user identifier is too long.
    /// </summary>
    [TestMethod]
    public void Constructor_WithLongUserIdentifier_ThrowsArgumentException()
    {
        // Arrange
        var longUserIdentifier = new string('a', 256); // 256 characters

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new UserSession(_queueId, longUserIdentifier));
    }

    /// <summary>
    /// Tests that the constructor throws an exception when metadata is too long.
    /// </summary>
    [TestMethod]
    public void Constructor_WithLongMetadata_ThrowsArgumentException()
    {
        // Arrange
        var longMetadata = new string('a', 1001); // 1001 characters

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() =>
            new UserSession(_queueId, ValidUserIdentifier, longMetadata));
    }
    #endregion

    #region MarkAsServing Tests
    /// <summary>
    /// Tests that MarkAsServing marks the user session as serving successfully.
    /// </summary>
    [TestMethod]
    public void MarkAsServing_WhenWaiting_MarksAsServingSuccessfully()
    {
        // Arrange
        Assert.AreEqual(QueueStatus.Waiting, _userSession.Status);

        // Act
        _userSession.MarkAsServing();

        // Assert
        Assert.AreEqual(QueueStatus.Serving, _userSession.Status);
        Assert.IsNotNull(_userSession.ServedAt);
    }

    /// <summary>
    /// Tests that MarkAsServing throws an exception when user session is not waiting.
    /// </summary>
    [TestMethod]
    public void MarkAsServing_WhenNotWaiting_ThrowsInvalidOperationException()
    {
        // Arrange
        _userSession.MarkAsServing(); // Already serving
        Assert.AreEqual(QueueStatus.Serving, _userSession.Status);

        // Act & Assert
        Assert.ThrowsException<InvalidOperationException>(() => _userSession.MarkAsServing());
    }
    #endregion

    #region MarkAsReleased Tests
    /// <summary>
    /// Tests that MarkAsReleased marks the user session as released successfully.
    /// </summary>
    [TestMethod]
    public void MarkAsReleased_WhenWaiting_MarksAsReleasedSuccessfully()
    {
        // Arrange
        Assert.AreEqual(QueueStatus.Waiting, _userSession.Status);

        // Act
        _userSession.MarkAsReleased();

        // Assert
        Assert.AreEqual(QueueStatus.Released, _userSession.Status);
        Assert.IsNotNull(_userSession.ReleasedAt);
    }

    /// <summary>
    /// Tests that MarkAsReleased does nothing when already released.
    /// </summary>
    [TestMethod]
    public void MarkAsReleased_WhenAlreadyReleased_DoesNothing()
    {
        // Arrange
        _userSession.MarkAsReleased();
        var releasedAt = _userSession.ReleasedAt;

        // Act
        _userSession.MarkAsReleased();

        // Assert
        Assert.AreEqual(QueueStatus.Released, _userSession.Status);
        Assert.AreEqual(releasedAt, _userSession.ReleasedAt);
    }
    #endregion

    #region MarkAsDropped Tests
    /// <summary>
    /// Tests that MarkAsDropped marks the user session as dropped successfully.
    /// </summary>
    [TestMethod]
    public void MarkAsDropped_WhenWaiting_MarksAsDroppedSuccessfully()
    {
        // Arrange
        Assert.AreEqual(QueueStatus.Waiting, _userSession.Status);

        // Act
        _userSession.MarkAsDropped();

        // Assert
        Assert.AreEqual(QueueStatus.Dropped, _userSession.Status);
    }

    /// <summary>
    /// Tests that MarkAsDropped does nothing when already dropped.
    /// </summary>
    [TestMethod]
    public void MarkAsDropped_WhenAlreadyDropped_DoesNothing()
    {
        // Arrange
        _userSession.MarkAsDropped();
        Assert.AreEqual(QueueStatus.Dropped, _userSession.Status);

        // Act
        _userSession.MarkAsDropped();

        // Assert
        Assert.AreEqual(QueueStatus.Dropped, _userSession.Status);
    }
    #endregion

    #region UpdatePosition Tests
    /// <summary>
    /// Tests that UpdatePosition updates the position successfully with valid position.
    /// </summary>
    [TestMethod]
    public void UpdatePosition_WithValidPosition_UpdatesPositionSuccessfully()
    {
        // Arrange
        var newPosition = 5;

        // Act
        _userSession.UpdatePosition(newPosition);

        // Assert
        Assert.AreEqual(newPosition, _userSession.Position);
    }

    /// <summary>
    /// Tests that UpdatePosition throws an exception when position is negative.
    /// </summary>
    [TestMethod]
    public void UpdatePosition_WithNegativePosition_ThrowsArgumentException()
    {
        // Arrange
        var negativePosition = -1;

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _userSession.UpdatePosition(negativePosition));
    }

    /// <summary>
    /// Tests that UpdatePosition updates the position successfully with zero position.
    /// </summary>
    [TestMethod]
    public void UpdatePosition_WithZeroPosition_UpdatesPositionSuccessfully()
    {
        // Arrange
        var zeroPosition = 0;

        // Act
        _userSession.UpdatePosition(zeroPosition);

        // Assert
        Assert.AreEqual(zeroPosition, _userSession.Position);
    }
    #endregion

    #region UpdateMetadata Tests
    /// <summary>
    /// Tests that UpdateMetadata updates the metadata successfully.
    /// </summary>
    [TestMethod]
    public void UpdateMetadata_WithValidMetadata_UpdatesMetadataSuccessfully()
    {
        // Arrange
        var newMetadata = "Updated metadata";

        // Act
        _userSession.UpdateMetadata(newMetadata);

        // Assert
        Assert.AreEqual(newMetadata, _userSession.Metadata);
    }

    /// <summary>
    /// Tests that UpdateMetadata updates the metadata to null successfully.
    /// </summary>
    [TestMethod]
    public void UpdateMetadata_WithNullMetadata_UpdatesMetadataSuccessfully()
    {
        // Arrange
        Assert.IsNotNull(_userSession.Metadata);

        // Act
        _userSession.UpdateMetadata(null);

        // Assert
        Assert.IsNull(_userSession.Metadata);
    }

    /// <summary>
    /// Tests that UpdateMetadata throws an exception when metadata is too long.
    /// </summary>
    [TestMethod]
    public void UpdateMetadata_WithLongMetadata_ThrowsArgumentException()
    {
        // Arrange
        var longMetadata = new string('a', 1001); // 1001 characters

        // Act & Assert
        Assert.ThrowsException<ArgumentException>(() => _userSession.UpdateMetadata(longMetadata));
    }
    #endregion

    #region UpdatePriority Tests
    /// <summary>
    /// Tests that UpdatePriority updates the priority successfully.
    /// </summary>
    [TestMethod]
    public void UpdatePriority_WithValidPriority_UpdatesPrioritySuccessfully()
    {
        // Arrange
        var newPriority = QueuePriority.High;

        // Act
        _userSession.UpdatePriority(newPriority);

        // Assert
        Assert.AreEqual(newPriority, _userSession.Priority);
    }

    /// <summary>
    /// Tests that UpdatePriority updates the priority to VIP successfully.
    /// </summary>
    [TestMethod]
    public void UpdatePriority_WithVipPriority_UpdatesPrioritySuccessfully()
    {
        // Arrange
        var vipPriority = QueuePriority.VIP;

        // Act
        _userSession.UpdatePriority(vipPriority);

        // Assert
        Assert.AreEqual(vipPriority, _userSession.Priority);
    }
    #endregion

    #region Domain Events Tests
    /// <summary>
    /// Tests that MarkAsServing adds a domain event.
    /// </summary>
    [TestMethod]
    public void MarkAsServing_AddsDomainEvent()
    {
        // Arrange
        Assert.AreEqual(0, _userSession.DomainEvents.Count);

        // Act
        _userSession.MarkAsServing();

        // Assert
        Assert.AreEqual(1, _userSession.DomainEvents.Count);
    }

    /// <summary>
    /// Tests that MarkAsReleased adds a domain event.
    /// </summary>
    [TestMethod]
    public void MarkAsReleased_AddsDomainEvent()
    {
        // Arrange
        Assert.AreEqual(0, _userSession.DomainEvents.Count);

        // Act
        _userSession.MarkAsReleased();

        // Assert
        Assert.AreEqual(1, _userSession.DomainEvents.Count);
    }

    /// <summary>
    /// Tests that MarkAsDropped adds a domain event.
    /// </summary>
    [TestMethod]
    public void MarkAsDropped_AddsDomainEvent()
    {
        // Arrange
        Assert.AreEqual(0, _userSession.DomainEvents.Count);

        // Act
        _userSession.MarkAsDropped();

        // Assert
        Assert.AreEqual(1, _userSession.DomainEvents.Count);
    }

    /// <summary>
    /// Tests that ClearDomainEvents clears all domain events.
    /// </summary>
    [TestMethod]
    public void ClearDomainEvents_ClearsAllDomainEvents()
    {
        // Arrange
        _userSession.MarkAsServing();
        Assert.AreEqual(1, _userSession.DomainEvents.Count);

        // Act
        _userSession.ClearDomainEvents();

        // Assert
        Assert.AreEqual(0, _userSession.DomainEvents.Count);
    }
    #endregion

    #region Status Transition Tests
    /// <summary>
    /// Tests the complete status transition from waiting to serving to released.
    /// </summary>
    [TestMethod]
    public void StatusTransition_WaitingToServingToReleased_TransitionsSuccessfully()
    {
        // Arrange
        Assert.AreEqual(QueueStatus.Waiting, _userSession.Status);

        // Act - Mark as serving
        _userSession.MarkAsServing();
        Assert.AreEqual(QueueStatus.Serving, _userSession.Status);
        Assert.IsNotNull(_userSession.ServedAt);

        // Act - Mark as released
        _userSession.MarkAsReleased();
        Assert.AreEqual(QueueStatus.Released, _userSession.Status);
        Assert.IsNotNull(_userSession.ReleasedAt);
    }

    /// <summary>
    /// Tests the status transition from waiting to dropped.
    /// </summary>
    [TestMethod]
    public void StatusTransition_WaitingToDropped_TransitionsSuccessfully()
    {
        // Arrange
        Assert.AreEqual(QueueStatus.Waiting, _userSession.Status);

        // Act
        _userSession.MarkAsDropped();

        // Assert
        Assert.AreEqual(QueueStatus.Dropped, _userSession.Status);
    }
    #endregion

    #region Priority Tests
    /// <summary>
    /// Tests that the constructor sets the priority correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_WithHighPriority_SetsPriorityCorrectly()
    {
        // Arrange & Act
        var userSession = new UserSession(_queueId, ValidUserIdentifier, null, QueuePriority.High);

        // Assert
        Assert.AreEqual(QueuePriority.High, userSession.Priority);
    }

    /// <summary>
    /// Tests that the constructor sets the priority correctly for VIP.
    /// </summary>
    [TestMethod]
    public void Constructor_WithVipPriority_SetsPriorityCorrectly()
    {
        // Arrange & Act
        var userSession = new UserSession(_queueId, ValidUserIdentifier, null, QueuePriority.VIP);

        // Assert
        Assert.AreEqual(QueuePriority.VIP, userSession.Priority);
    }
    #endregion

    #region Timestamp Tests
    /// <summary>
    /// Tests that the constructor sets the enqueued timestamp correctly.
    /// </summary>
    [TestMethod]
    public void Constructor_SetsEnqueuedTimestampCorrectly()
    {
        // Arrange
        var beforeEnqueue = DateTime.UtcNow;

        // Act
        var userSession = new UserSession(_queueId, ValidUserIdentifier);

        // Assert
        Assert.IsTrue(userSession.EnqueuedAt >= beforeEnqueue);
        Assert.IsTrue(userSession.EnqueuedAt <= DateTime.UtcNow);
    }

    /// <summary>
    /// Tests that MarkAsServing sets the served timestamp correctly.
    /// </summary>
    [TestMethod]
    public void MarkAsServing_SetsServedTimestampCorrectly()
    {
        // Arrange
        var beforeServe = DateTime.UtcNow;

        // Act
        _userSession.MarkAsServing();

        // Assert
        Assert.IsTrue(_userSession.ServedAt >= beforeServe);
        Assert.IsTrue(_userSession.ServedAt <= DateTime.UtcNow);
    }

    /// <summary>
    /// Tests that MarkAsReleased sets the released timestamp correctly.
    /// </summary>
    [TestMethod]
    public void MarkAsReleased_SetsReleasedTimestampCorrectly()
    {
        // Arrange
        var beforeRelease = DateTime.UtcNow;

        // Act
        _userSession.MarkAsReleased();

        // Assert
        Assert.IsTrue(_userSession.ReleasedAt >= beforeRelease);
        Assert.IsTrue(_userSession.ReleasedAt <= DateTime.UtcNow);
    }
    #endregion
}
