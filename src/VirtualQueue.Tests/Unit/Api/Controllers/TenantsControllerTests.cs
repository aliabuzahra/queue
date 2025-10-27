using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VirtualQueue.Api.Controllers;
using VirtualQueue.Application.Commands.Tenants;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Application.Queries.Tenants;

namespace VirtualQueue.Tests.Unit.Api.Controllers;

/// <summary>
/// Unit tests for the TenantsController class.
/// </summary>
[TestClass]
public class TenantsControllerTests
{
    #region Test Data
    private Mock<IMediator> _mockMediator;
    private TenantsController _controller;
    private TenantDto _testTenant;
    #endregion

    #region Test Setup
    /// <summary>
    /// Sets up test data before each test.
    /// </summary>
    [TestInitialize]
    public void Setup()
    {
        _mockMediator = new Mock<IMediator>();
        _controller = new TenantsController(_mockMediator.Object);
        
        _testTenant = new TenantDto
        {
            Id = Guid.NewGuid(),
            Name = "Test Tenant",
            Domain = "test.example.com",
            ApiKey = "test-api-key",
            CreatedAt = DateTime.UtcNow,
            IsActive = true
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
        var controller = new TenantsController(mediator.Object);

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
        Assert.ThrowsException<ArgumentNullException>(() => new TenantsController(null!));
    }
    #endregion

    #region CreateTenant Tests
    /// <summary>
    /// Tests that CreateTenant creates a tenant successfully with valid request.
    /// </summary>
    [TestMethod]
    public async Task CreateTenant_WithValidRequest_CreatesTenantSuccessfully()
    {
        // Arrange
        var request = new CreateTenantRequest("Test Tenant", "test.example.com");
        var command = new CreateTenantCommand(request.Name, request.Domain);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateTenantCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testTenant);

        // Act
        var result = await _controller.CreateTenant(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(CreatedAtActionResult));
        
        var createdAtResult = (CreatedAtActionResult)result.Result!;
        Assert.AreEqual(201, createdAtResult.StatusCode);
        Assert.AreEqual(nameof(_controller.GetTenant), createdAtResult.ActionName);
        Assert.AreEqual(_testTenant, createdAtResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<CreateTenantCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that CreateTenant returns bad request when request is null.
    /// </summary>
    [TestMethod]
    public async Task CreateTenant_WithNullRequest_ReturnsBadRequest()
    {
        // Arrange
        CreateTenantRequest? nullRequest = null;

        // Act
        var result = await _controller.CreateTenant(nullRequest!);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Request cannot be null", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that CreateTenant returns bad request when validation fails.
    /// </summary>
    [TestMethod]
    public async Task CreateTenant_WithValidationError_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateTenantRequest("", "test.example.com");
        var command = new CreateTenantCommand(request.Name, request.Domain);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateTenantCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new ArgumentException("Name cannot be empty"));

        // Act
        var result = await _controller.CreateTenant(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Name cannot be empty", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that CreateTenant returns internal server error when unexpected error occurs.
    /// </summary>
    [TestMethod]
    public async Task CreateTenant_WithUnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        var request = new CreateTenantRequest("Test Tenant", "test.example.com");
        
        _mockMediator.Setup(m => m.Send(It.IsAny<CreateTenantCommand>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.CreateTenant(request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        
        var objectResult = (ObjectResult)result.Result!;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("An error occurred while creating the tenant", objectResult.Value);
    }
    #endregion

    #region GetTenant Tests
    /// <summary>
    /// Tests that GetTenant returns tenant successfully when found.
    /// </summary>
    [TestMethod]
    public async Task GetTenant_WithValidId_ReturnsTenantSuccessfully()
    {
        // Arrange
        var tenantId = _testTenant.Id;
        var query = new GetTenantByIdQuery(tenantId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testTenant);

        // Act
        var result = await _controller.GetTenant(tenantId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(_testTenant, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that GetTenant returns not found when tenant is not found.
    /// </summary>
    [TestMethod]
    public async Task GetTenant_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var query = new GetTenantByIdQuery(nonExistentId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync((TenantDto?)null);

        // Act
        var result = await _controller.GetTenant(nonExistentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(NotFoundObjectResult));
        
        var notFoundResult = (NotFoundObjectResult)result.Result!;
        Assert.AreEqual(404, notFoundResult.StatusCode);
        Assert.AreEqual($"Tenant with ID {nonExistentId} not found", notFoundResult.Value);
    }

    /// <summary>
    /// Tests that GetTenant returns bad request when ID is empty.
    /// </summary>
    [TestMethod]
    public async Task GetTenant_WithEmptyId_ReturnsBadRequest()
    {
        // Arrange
        var emptyId = Guid.Empty;

        // Act
        var result = await _controller.GetTenant(emptyId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(BadRequestObjectResult));
        
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.AreEqual(400, badRequestResult.StatusCode);
        Assert.AreEqual("Invalid tenant ID", badRequestResult.Value);
    }

    /// <summary>
    /// Tests that GetTenant returns internal server error when unexpected error occurs.
    /// </summary>
    [TestMethod]
    public async Task GetTenant_WithUnexpectedError_ReturnsInternalServerError()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetTenantByIdQuery>(), It.IsAny<CancellationToken>()))
                    .ThrowsAsync(new Exception("Unexpected error"));

        // Act
        var result = await _controller.GetTenant(tenantId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(ObjectResult));
        
        var objectResult = (ObjectResult)result.Result!;
        Assert.AreEqual(500, objectResult.StatusCode);
        Assert.AreEqual("An error occurred while retrieving the tenant", objectResult.Value);
    }
    #endregion

    #region GetAllTenants Tests
    /// <summary>
    /// Tests that GetAllTenants returns all tenants successfully.
    /// </summary>
    [TestMethod]
    public async Task GetAllTenants_ReturnsAllTenantsSuccessfully()
    {
        // Arrange
        var tenants = new List<TenantDto> { _testTenant };
        var query = new GetAllTenantsQuery();
        
        _mockMediator.Setup(m => m.Send(It.IsAny<GetAllTenantsQuery>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(tenants);

        // Act
        var result = await _controller.GetAllTenants();

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(tenants, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<GetAllTenantsQuery>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region UpdateTenant Tests
    /// <summary>
    /// Tests that UpdateTenant updates tenant successfully with valid request.
    /// </summary>
    [TestMethod]
    public async Task UpdateTenant_WithValidRequest_UpdatesTenantSuccessfully()
    {
        // Arrange
        var tenantId = _testTenant.Id;
        var request = new UpdateTenantRequest("Updated Tenant", "updated.example.com");
        var command = new UpdateTenantCommand(tenantId, request.Name, request.Domain);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<UpdateTenantCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(_testTenant);

        // Act
        var result = await _controller.UpdateTenant(tenantId, request);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result.Result, typeof(OkObjectResult));
        
        var okResult = (OkObjectResult)result.Result!;
        Assert.AreEqual(200, okResult.StatusCode);
        Assert.AreEqual(_testTenant, okResult.Value);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<UpdateTenantCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }
    #endregion

    #region DeleteTenant Tests
    /// <summary>
    /// Tests that DeleteTenant deletes tenant successfully when found.
    /// </summary>
    [TestMethod]
    public async Task DeleteTenant_WithValidId_DeletesTenantSuccessfully()
    {
        // Arrange
        var tenantId = _testTenant.Id;
        var command = new DeleteTenantCommand(tenantId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<DeleteTenantCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteTenant(tenantId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NoContentResult));
        
        var noContentResult = (NoContentResult)result;
        Assert.AreEqual(204, noContentResult.StatusCode);
        
        _mockMediator.Verify(m => m.Send(It.IsAny<DeleteTenantCommand>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    /// <summary>
    /// Tests that DeleteTenant returns not found when tenant is not found.
    /// </summary>
    [TestMethod]
    public async Task DeleteTenant_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();
        var command = new DeleteTenantCommand(nonExistentId);
        
        _mockMediator.Setup(m => m.Send(It.IsAny<DeleteTenantCommand>(), It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        // Act
        var result = await _controller.DeleteTenant(nonExistentId);

        // Assert
        Assert.IsNotNull(result);
        Assert.IsInstanceOfType(result, typeof(NotFoundResult));
        
        var notFoundResult = (NotFoundResult)result;
        Assert.AreEqual(404, notFoundResult.StatusCode);
    }
    #endregion

    #region Request Model Tests
    /// <summary>
    /// Tests that CreateTenantRequest initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void CreateTenantRequest_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var name = "Test Tenant";
        var domain = "test.example.com";

        // Act
        var request = new CreateTenantRequest(name, domain);

        // Assert
        Assert.AreEqual(name, request.Name);
        Assert.AreEqual(domain, request.Domain);
    }

    /// <summary>
    /// Tests that UpdateTenantRequest initializes with valid parameters.
    /// </summary>
    [TestMethod]
    public void UpdateTenantRequest_WithValidParameters_InitializesSuccessfully()
    {
        // Arrange
        var name = "Updated Tenant";
        var domain = "updated.example.com";

        // Act
        var request = new UpdateTenantRequest(name, domain);

        // Assert
        Assert.AreEqual(name, request.Name);
        Assert.AreEqual(domain, request.Domain);
    }
    #endregion
}
