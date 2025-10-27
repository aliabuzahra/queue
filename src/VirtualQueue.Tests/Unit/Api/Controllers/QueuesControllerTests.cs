using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualQueue.Api.Controllers;
using VirtualQueue.Application.Commands.Queues;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.Queues;

namespace VirtualQueue.Tests.Unit.Api.Controllers;

/// <summary>
/// Unit tests for the QueuesController class.
/// </summary>
[TestClass]
public class QueuesControllerTests
{
    #region Test Data
    private Mock<IMediator> _mockMediator;
    private QueuesController _controller;
    private Guid _tenantId;
    private QueueDto _testQueue;
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new QueuesController(_mockMediator.Object);
        _tenantId = Guid.NewGuid();
        
        _testQueue = new QueueDto
        {
            Id = Guid.NewGuid(),
            TenantId = _tenantId,
            Name = "Test Queue",
            Description = "Test Queue Description",
            MaxConcurrentUsers = 10,
            ReleaseRatePerMinute = 5,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
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
        var controller = new QueuesController(mediator.Object);

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
        Assert.ThrowsException<ArgumentNullException>(() => new QueuesController(null!));
    }
    #endregion

    #region CreateQueue Tests
    /// <summary>
    /// Tests that CreateQueue creates a queue successfully with valid request.
    /// </summary>
    [TestMethod]
    public async Task CreateQueue_WithValidRequest_CreatesQueueSuccessfully()
    {
        // Arrange
        var request = new CreateQueueRequest("Test Queue", "Test Description", 10, 5);
        var command = new CreateQueueCommand(_tenantId, request.Name, request.Description, request.MaxConcurrentUsers, request.ReleaseRatePerMinute);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateQueueCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testQueue);

        // Act
        var result = await _controller.CreateQueue(_tenantId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        
        var createdAtResult = (CreatedAtActionResult)result.Result!;
        Assert.AreEqual(201, createdAtResult.StatusCode);
        Assert.AreEqual(nameof(_controller.GetQueues), createdAtResult.ActionName);
        Assert.AreEqual(_testQueue, createdAtResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateQueueCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateQueue returns bad request when tenant ID is empty.
    /// </summary>
    [TestMethod]
    public async Task CreateQueue_WithEmptyTenantId_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateQueueRequest("Test Queue", "Test Description", 10, 5);
        var emptyTenantId = Guid.Empty;

        // Act
        var result = await _controller.CreateQueue(emptyTenantId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Invalid tenant ID", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that CreateQueue returns bad request when request is null.
    /// </summary>
    [TestMethod]
    public async Task CreateQueue_WithNullRequest_ReturnsBadRequest()
    {
        // Arrange
        CreateQueueRequest? nullRequest = null;

        // Act
        var result = await _controller.CreateQueue(_tenantId, nullRequest!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Request cannot be null", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that CreateQueue returns bad request when validation fails.
    /// </summary>
    [TestMethod]
    public async Task CreateQueue_WithValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateQueueRequest("", "Test Description", 10, 5);
        var command = new CreateQueueCommand(_tenantId, request.Name, request.Description, request.MaxConcurrentUsers, request.ReleaseRatePerMinute);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateQueueCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ArgumentException("Name cannot be empty"));

        // Act
        var result = await _controller.CreateQueue(_tenantId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Name cannot be empty", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that CreateQueue returns internal server error when unexpected error occurs.
    /// </summary>
    [TestMethod]
    public async Task CreateQueue_WithUnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        var request = new CreateQueueRequest("Test Queue", "Test Description", 10, 5);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateQueueCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.CreateQueue(_tenantId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        
        var objectResult = (ObjectResult)result.Result!;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("An error occurred while creating the queue", objectResult.Value);
    }
    #endregion

    #region GetQueues Tests
    /// <summary>
    /// Tests that GetQueues returns all queues successfully for a tenant.
    /// </summary>
    [TestMethod]
    public async Task GetQueues_WithValidTenantId_ReturnsQueuesSuccessfully()
    {
        // Arrange
        var queues = new List<QueueDto> { _testQueue };
        var query = new GetQueuesByTenantIdQuery(_tenantId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetQueuesByTenantIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(queues);

        // Act
        var result = await _controller.GetQueues(_tenantId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(queues, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<GetQueuesByTenantIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region GetQueue Tests
    /// <summary>
    /// Tests that GetQueue returns a queue successfully when found.
    /// </summary>
    [TestMethod]
    public async Task GetQueue_WithValidIds_ReturnsQueueSuccessfully()
    {
        // Arrange
        var queueId = _testQueue.Id;
        var query = new GetQueueByIdQuery(_tenantId, queueId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetQueueByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testQueue);

        // Act
        var result = await _controller.GetQueue(_tenantId, queueId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(_testQueue, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<GetQueueByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetQueue returns not found when queue is not found.
    /// </summary>
    [TestMethod]
    public async Task GetQueue_WithNonExistentQueue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentQueueId = Guid.NewGuid();
        var query = new GetQueueByIdQuery(_tenantId, nonExistentQueueId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetQueueByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((QueueDto?)null);

        // Act
        var result = await _controller.GetQueue(_tenantId, nonExistentQueueId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundResult));
        
        var notFoundResult = (NotFoundResult)result.Result!;
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }
    #endregion

    #region UpdateQueue Tests
    /// <summary>
    /// Tests that UpdateQueue updates a queue successfully with valid request.
    /// </summary>
    [TestMethod]
    public async Task UpdateQueue_WithValidRequest_UpdatesQueueSuccessfully()
    {
        // Arrange
        var queueId = _testQueue.Id;
        var request = new UpdateQueueRequest("Updated Queue", "Updated Description", 20, 10);
        var command = new UpdateQueueCommand(_tenantId, queueId, request.Name, request.Description, request.MaxConcurrentUsers, request.ReleaseRatePerMinute);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<UpdateQueueCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testQueue);

        // Act
        var result = await _controller.UpdateQueue(_tenantId, queueId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(_testQueue, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<UpdateQueueCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region DeleteQueue Tests
    /// <summary>
    /// Tests that DeleteQueue deletes a queue successfully when found.
    /// </summary>
    [TestMethod]
    public async Task DeleteQueue_WithValidIds_DeletesQueueSuccessfully()
    {
        // Arrange
        var queueId = _testQueue.Id;
        var command = new DeleteQueueCommand(_tenantId, queueId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<DeleteQueueCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteQueue(_tenantId, queueId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        
        var noContentResult = (NoContentResult)result;
        Assert.AreEqual(204, noContentResult.StatusCode);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<DeleteQueueCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteQueue returns not found when queue is not found.
    /// </summary>
    [TestMethod]
    public async Task DeleteQueue_WithNonExistentQueue_ReturnsNotFound()
    {
        // Arrange
        var nonExistentQueueId = Guid.NewGuid();
        var command = new DeleteQueueCommand(_tenantId, nonExistentQueueId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<DeleteQueueCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteQueue(_tenantId, nonExistentQueueId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        
        var notFoundResult = (NotFoundResult)result;
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }
    #endregion

    #region ActivateQueue Tests
    /// <summary>
    /// Tests that ActivateQueue activates a queue successfully.
    /// </summary>
    [TestMethod]
    public async Task ActivateQueue_WithValidIds_ActivatesQueueSuccessfully()
    {
        // Arrange
        var queueId = _testQueue.Id;
        var command = new ActivateQueueCommand(_tenantId, queueId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<ActivateQueueCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testQueue);

        // Act
        var result = await _controller.ActivateQueue(_tenantId, queueId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(_testQueue, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<ActivateQueueCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region DeactivateQueue Tests
    /// <summary>
    /// Tests that DeactivateQueue deactivates a queue successfully.
    /// </summary>
    [TestMethod]
    public async Task DeactivateQueue_WithValidIds_DeactivatesQueueSuccessfully()
    {
        // Arrange
        var queueId = _testQueue.Id;
        var command = new DeactivateQueueCommand(_tenantId, queueId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<DeactivateQueueCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testQueue);

        // Act
        var result = await _controller.DeactivateQueue(_tenantId, queueId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(_testQueue, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<DeactivateQueueCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region SetQueueSchedule Tests
    /// <summary>
    /// Tests that SetQueueSchedule sets the schedule successfully.
    /// </summary>
    [TestMethod]
    public async Task SetQueueSchedule_WithValidRequest_SetsScheduleSuccessfully()
    {
        // Arrange
        var queueId = _testQueue.Id;
        var schedule = new QueueScheduleDto();
        var request = new SetQueueScheduleRequest(schedule);
        var command = new SetQueueScheduleCommand(_tenantId, queueId, schedule);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<SetQueueScheduleCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testQueue);

        // Act
        var result = await _controller.SetQueueSchedule(_tenantId, queueId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(_testQueue, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<SetQueueScheduleCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region GetQueueAvailability Tests
    /// <summary>
    /// Tests that GetQueueAvailability returns availability information successfully.
    /// </summary>
    [TestMethod]
    public async Task GetQueueAvailability_WithValidIds_ReturnsAvailabilitySuccessfully()
    {
        // Arrange
        var queueId = _testQueue.Id;
        var checkTime = DateTime.UtcNow;
        var availability = new QueueAvailabilityDto
        {
            IsAvailable = true,
            NextActivationTime = DateTime.UtcNow.AddHours(1),
            PreviousActivationTime = DateTime.UtcNow.AddHours(-1)
        };
        var query = new GetQueueAvailabilityQuery(_tenantId, queueId, checkTime);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetQueueAvailabilityQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(availability);

        // Act
        var result = await _controller.GetQueueAvailability(_tenantId, queueId, checkTime);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(availability, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<GetQueueAvailabilityQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region Request Model Tests
    /// <summary>
    /// Tests that CreateQueueRequest initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void CreateQueueRequest_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var name = "Test Queue";
        var description = "Test Description";
        var maxConcurrentUsers = 10;
        var releaseRatePerMinute = 5;

        // Act
        var request = new CreateQueueRequest(name, description, maxConcurrentUsers, releaseRatePerMinute);

        // Assert
        Assert.AreEqual(name, request.Name);
        Assert.AreEqual(description, request.Description);
        Assert.AreEqual(maxConcurrentUsers, request.MaxConcurrentUsers);
        Assert.AreEqual(releaseRatePerMinute, request.ReleaseRatePerMinute);
    }

    /// <summary>
    /// Tests that UpdateQueueRequest initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void UpdateQueueRequest_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var name = "Updated Queue";
        var description = "Updated Description";
        var maxConcurrentUsers = 20;
        var releaseRatePerMinute = 10;

        // Act
        var request = new UpdateQueueRequest(name, description, maxConcurrentUsers, releaseRatePerMinute);

        // Assert
        Assert.AreEqual(name, request.Name);
        Assert.AreEqual(description, request.Description);
        Assert.AreEqual(maxConcurrentUsers, request.MaxConcurrentUsers);
        Assert.AreEqual(releaseRatePerMinute, request.ReleaseRatePerMinute);
    }

    /// <summary>
    /// Tests that SetQueueScheduleRequest initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void SetQueueScheduleRequest_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var schedule = new QueueScheduleDto();

        // Act
        var request = new SetQueueScheduleRequest(schedule);

        // Assert
        Assert.AreEqual(schedule, request.Schedule);
    }
    #endregion
}
