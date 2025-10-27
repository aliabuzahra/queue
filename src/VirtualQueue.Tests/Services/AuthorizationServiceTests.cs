using Microsoft.Extensions.Logging;
using Moq;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Infrastructure.Services;
using FluentAssertions;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Tests.Services;

/// <summary>
/// Unit tests for the AuthorizationService
/// </summary>
public class AuthorizationServiceTests
{
    #region Fields
    private readonly Mock<VirtualQueue.Infrastructure.Data.VirtualQueueDbContext> _mockContext;
    private readonly Mock<ILogger<AuthorizationService>> _mockLogger;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly AuthorizationService _authorizationService;
    #endregion

    #region Constructor
    public AuthorizationServiceTests()
    {
        _mockContext = new Mock<VirtualQueue.Infrastructure.Data.VirtualQueueDbContext>();
        _mockLogger = new Mock<ILogger<AuthorizationService>>();
        _mockCacheService = new Mock<ICacheService>();
        _authorizationService = new AuthorizationService(_mockContext.Object, _mockLogger.Object, _mockCacheService.Object);
    }
    #endregion

    #region HasPermissionAsync Tests
    [Fact]
    public async Task HasPermissionAsync_WithValidAdminUser_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userIdentifier = "admin@test.com";
        var permission = "queue.manage";

        // Act
        var result = await _authorizationService.HasPermissionAsync(tenantId, userIdentifier, permission);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_WithInvalidUser_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userIdentifier = "nonexistent@test.com";
        var permission = "queue.manage";

        // Act
        var result = await _authorizationService.HasPermissionAsync(tenantId, userIdentifier, permission);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_WithNullUserIdentifier_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        string? userIdentifier = null;
        var permission = "queue.manage";

        // Act
        var result = await _authorizationService.HasPermissionAsync(tenantId, userIdentifier!, permission);

        // Assert
        result.Should().BeFalse();
    }
    #endregion

    #region HasRoleAsync Tests
    [Fact]
    public async Task HasRoleAsync_WithValidUserAndRole_ShouldReturnTrue()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userIdentifier = "admin@test.com";
        var role = "Admin";

        // Act
        var result = await _authorizationService.HasRoleAsync(tenantId, userIdentifier, role);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasRoleAsync_WithInvalidRole_ShouldReturnFalse()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userIdentifier = "admin@test.com";
        var role = "InvalidRole";

        // Act
        var result = await _authorizationService.HasRoleAsync(tenantId, userIdentifier, role);

        // Assert
        result.Should().BeFalse();
    }
    #endregion

    #region GetUserPermissionsAsync Tests
    [Fact]
    public async Task GetUserPermissionsAsync_WithValidUser_ShouldReturnPermissions()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userIdentifier = "admin@test.com";

        // Act
        var result = await _authorizationService.GetUserPermissionsAsync(tenantId, userIdentifier);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<string>>();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithInvalidUser_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userIdentifier = "nonexistent@test.com";

        // Act
        var result = await _authorizationService.GetUserPermissionsAsync(tenantId, userIdentifier);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    #endregion

    #region GetUserRolesAsync Tests
    [Fact]
    public async Task GetUserRolesAsync_WithValidUser_ShouldReturnRoles()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userIdentifier = "admin@test.com";

        // Act
        var result = await _authorizationService.GetUserRolesAsync(tenantId, userIdentifier);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<List<string>>();
    }

    [Fact]
    public async Task GetUserRolesAsync_WithInvalidUser_ShouldReturnEmptyList()
    {
        // Arrange
        var tenantId = Guid.NewGuid();
        var userIdentifier = "nonexistent@test.com";

        // Act
        var result = await _authorizationService.GetUserRolesAsync(tenantId, userIdentifier);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
    #endregion
}
