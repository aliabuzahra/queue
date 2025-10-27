using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualQueue.Api.Controllers;
using VirtualQueue.Application.Commands.UserSessions;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.UserSessions;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Tests.Unit.Api.Controllers;

/// <summary>
/// Unit tests for the UserSessionsController class.
/// </summary>
[TestClass]
public class UserSessionsControllerTests
{
    #region Test Data
    private Mock<IMediator> _mockMediator;
    private UserSessionsController _controller;
    private Guid _tenantId;
    private Guid _queueId;
    private UserSessionDto _testUserSession;
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new UserSessionsController(_mockMediator.Object);
        _tenantId = Guid.NewGuid();
        _queueId = Guid.NewGuid();
        
        _testUserSession = new UserSessionDto
        {
            Id = Guid.NewGuid(),
            QueueId = _queueId,
            UserIdentifier = "testuser123",
            Status = QueueStatus.Waiting,
            Priority = QueuePriority.Normal,
            EnqueuedAt = DateTime.UtcNow,
            Position = 1
        };
    }
    #endregion

    #region Constructor Tests
    /// <summary>
    /// Tests that the constructor initializes the controller successfully with valid mediator.
    /// </summary>
    [TestMethod]
    public void Constructor_WithValidMediator_InitializesControllerSuccessfully()
    {
        // Arrange
        var mediator = new Mock<IMediator>();

        // Act
        var controller = new UserSessionsController(mediator.Object);

        // Assert
        Assert.IsNotNull(controller);
    }

    /// <summary>
    /// Tests that the constructor throws an exception when mediator is null.
    /// </summary>
    [TestMethod]
    public void Constructor_WithNullMediator_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        Assert.ThrowsException<ArgumentNullException>(() => new UserSessionsController(null!));
    }
    #endregion

    #region EnqueueUser Tests
    /// <summary>
    /// Tests that EnqueueUser enqueues a user successfully with valid request.
    /// </summary>
    [TestMethod]
    public async Task EnqueueUser_WithValidRequest_EnqueuesUserSuccessfully()
    {
        // Arrange
        var request = new EnqueueUserRequest("testuser123", "test metadata", QueuePriority.High);
        var command = new EnqueueUserCommand(_tenantId, _queueId, request.UserIdentifier, request.Metadata, request.Priority);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<EnqueueUserCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testUserSession);

        // Act
        var result = await _controller.EnqueueUser(_tenantId, _queueId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        
        var createdAtResult = (CreatedAtActionResult)result.Result!;
        Assert.AreEqual(201, createdAtResult.StatusCode);
        Assert.AreEqual(nameof(_controller.GetUserStatus), createdAtResult.ActionName);
        Assert.AreEqual(_testUserSession, createdAtResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<EnqueueUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that EnqueueUser returns bad request when tenant ID is empty.
    /// </summary>
    [TestMethod]
    public async Task EnqueueUser_WithEmptyTenantId_ReturnsBadRequest()
    {
        // Arrange
        var request = new EnqueueUserRequest("testuser123");
        var emptyTenantId = Guid.Empty;

        // Act
        var result = await _controller.EnqueueUser(emptyTenantId, _queueId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Invalid tenant ID", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that EnqueueUser returns bad request when queue ID is empty.
    /// </summary>
    [TestMethod]
    public async Task EnqueueUser_WithEmptyQueueId_ReturnsBadRequest()
    {
        // Arrange
        var request = new EnqueueUserRequest("testuser123");
        var emptyQueueId = Guid.Empty;

        // Act
        var result = await _controller.EnqueueUser(_tenantId, emptyQueueId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Invalid queue ID", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that EnqueueUser returns bad request when request is null.
    /// </summary>
    [TestMethod]
    public async Task EnqueueUser_WithNullRequest_ReturnsBadRequest()
    {
        // Arrange
        EnqueueUserRequest? nullRequest = null;

        // Act
        var result = await _controller.EnqueueUser(_tenantId, _queueId, nullRequest!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Request cannot be null", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that EnqueueUser returns bad request when validation fails.
    /// </summary>
    [TestMethod]
    public async Task EnqueueUser_WithValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new EnqueueUserRequest("", "test metadata", QueuePriority.High);
        var command = new EnqueueUserCommand(_tenantId, _queueId, request.UserIdentifier, request.Metadata, request.Priority);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<EnqueueUserCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ArgumentException("User identifier cannot be empty"));

        // Act
        var result = await _controller.EnqueueUser(_tenantId, _queueId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("User identifier cannot be empty", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that EnqueueUser returns internal server error when unexpected error occurs.
    /// </summary>
    [TestMethod]
    public async Task EnqueueUser_WithUnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        var request = new EnqueueUserRequest("testuser123");
        
        _mockMediator.Setup(m => m.Send(It.IsAny<EnqueueUserCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.EnqueueUser(_tenantId, _queueId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        
        var objectResult = (ObjectResult)result.Result!;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("An error occurred while enqueuing the user", objectResult.Value);
    }
    #endregion

    #region GetUserStatus Tests
    /// <summary>
    /// Tests that GetUserStatus returns user status successfully when found.
    /// </summary>
    [TestMethod]
    public async Task GetUserStatus_WithValidParameters_ReturnsStatusSuccessfully()
    {
        // Arrange
        var userId = "testuser123";
        var status = new QueueStatusDto
        {
            UserIdentifier = userId,
            QueueId = _queueId,
            Status = QueueStatus.Waiting,
            Position = 1,
            EstimatedWaitTime = TimeSpan.FromMinutes(5)
        };
        var query = new GetUserQueueStatusQuery(_tenantId, _queueId, userId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetUserQueueStatusQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(status);

        // Act
        var result = await _controller.GetUserStatus(_tenantId, _queueId, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(status, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<GetUserQueueStatusQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetUserStatus returns not found when user is not found.
    /// </summary>
    [TestMethod]
    public async Task GetUserStatus_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "nonexistentuser";
        var query = new GetUserQueueStatusQuery(_tenantId, _queueId, userId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetUserQueueStatusQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((QueueStatusDto?)null);

        // Act
        var result = await _controller.GetUserStatus(_tenantId, _queueId, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        
        var notFoundResult = (NotFoundResult)result.Result!;
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }
    #endregion

    #region ReleaseUsers Tests
    /// <summary>
    /// Tests that ReleaseUsers releases users successfully with valid request.
    /// </summary>
    [TestMethod]
    public async Task ReleaseUsers_WithValidRequest_ReleasesUsersSuccessfully()
    {
        // Arrange
        var request = new ReleaseUsersRequest(5);
        var command = new ReleaseUsersCommand(_tenantId, _queueId, request.Count);
        var releasedCount = 3;
        
        _mockMediator.Setup(m => m.Send(It.IsAny<ReleaseUsersCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(releasedCount);

        // Act
        var result = await _controller.ReleaseUsers(_tenantId, _queueId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.IsInstanceOfType(okResult.Value, typeof(ReleaseUsersResponse));
        
        var response = (ReleaseUsersResponse)okResult.Value!;
        Assert.AreEqual(releasedCount, response.ReleasedCount);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<ReleaseUsersCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region GetUserSessions Tests
    /// <summary>
    /// Tests that GetUserSessions returns user sessions successfully for a queue.
    /// </summary>
    [TestMethod]
    public async Task GetUserSessions_WithValidParameters_ReturnsSessionsSuccessfully()
    {
        // Arrange
        var sessions = new List<UserSessionDto> { _testUserSession };
        var query = new GetUserSessionsByQueueIdQuery(_tenantId, _queueId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetUserSessionsByQueueIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(sessions);

        // Act
        var result = await _controller.GetUserSessions(_tenantId, _queueId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(sessions, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<GetUserSessionsByQueueIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region RemoveUser Tests
    /// <summary>
    /// Tests that RemoveUser removes a user successfully when found.
    /// </summary>
    [TestMethod]
    public async Task RemoveUser_WithValidParameters_RemovesUserSuccessfully()
    {
        // Arrange
        var userId = "testuser123";
        var command = new RemoveUserCommand(_tenantId, _queueId, userId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<RemoveUserCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act
        var result = await _controller.RemoveUser(_tenantId, _queueId, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        
        var noContentResult = (NoContentResult)result;
        Assert.AreEqual(204, noContentResult.StatusCode);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<RemoveUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that RemoveUser returns not found when user is not found.
    /// </summary>
    [TestMethod]
    public async Task RemoveUser_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "nonexistentuser";
        var command = new RemoveUserCommand(_tenantId, _queueId, userId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<RemoveUserCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        // Act
        var result = await _controller.RemoveUser(_tenantId, _queueId, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        
        var notFoundResult = (NotFoundResult)result;
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }
    #endregion

    #region DropUser Tests
    /// <summary>
    /// Tests that DropUser drops a user successfully when found.
    /// </summary>
    [TestMethod]
    public async Task DropUser_WithValidParameters_DropsUserSuccessfully()
    {
        // Arrange
        var userId = "testuser123";
        var command = new DropUserCommand(_tenantId, _queueId, userId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<DropUserCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testUserSession);

        // Act
        var result = await _controller.DropUser(_tenantId, _queueId, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(_testUserSession, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<DropUserCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DropUser returns not found when user is not found.
    /// </summary>
    [TestMethod]
    public async Task DropUser_WithNonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "nonexistentuser";
        var command = new DropUserCommand(_tenantId, _queueId, userId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<DropUserCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((UserSessionDto?)null);

        // Act
        var result = await _controller.DropUser(_tenantId, _queueId, userId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        
        var notFoundResult = (NotFoundResult)result.Result!;
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }
    #endregion

    #region Request Model Tests
    /// <summary>
    /// Tests that EnqueueUserRequest initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void EnqueueUserRequest_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var userIdentifier = "testuser123";
        var metadata = "test metadata";
        var priority = QueuePriority.High;

        // Act
        var request = new EnqueueUserRequest(userIdentifier, metadata, priority);

        // Assert
        Assert.AreEqual(userIdentifier, request.UserIdentifier);
        Assert.AreEqual(metadata, request.Metadata);
        Assert.AreEqual(priority, request.Priority);
    }

    /// <summary>
    /// Tests that EnqueueUserRequest initializes with default values.
    /// </summary>
    [TestMethod]
    public void EnqueueUserRequest_WithMinimalParameters_InitializesWithDefaults()
    {
        // Arrange
        var userIdentifier = "testuser123";

        // Act
        var request = new EnqueueUserRequest(userIdentifier);

        // Assert
        Assert.AreEqual(userIdentifier, request.UserIdentifier);
        Assert.IsNull(request.Metadata);
        Assert.AreEqual(QueuePriority.Normal, request.Priority);
    }

    /// <summary>
    /// Tests that ReleaseUsersRequest initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void ReleaseUsersRequest_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var count = 5;

        // Act
        var request = new ReleaseUsersRequest(count);

        // Assert
        Assert.AreEqual(count, request.Count);
    }

    /// <summary>
    /// Tests that ReleaseUsersResponse initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void ReleaseUsersResponse_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var releasedCount = 3;

        // Act
        var response = new ReleaseUsersResponse(releasedCount);

        // Assert
        Assert.AreEqual(releasedCount, response.ReleasedCount);
    }
    #endregion
}
