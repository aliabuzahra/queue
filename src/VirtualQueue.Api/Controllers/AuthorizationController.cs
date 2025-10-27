using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthorizationController : ControllerBase
{
    private readonly IAuthorizationService _authorizationService;
    private readonly ILogger<AuthorizationController> _logger;

    public AuthorizationController(IAuthorizationService authorizationService, ILogger<AuthorizationController> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    [HttpPost("authorize")]
    public async Task<ActionResult<AuthorizationResponse>> Authorize([FromBody] AuthorizationRequest request)
    {
        try
        {
            var isAuthorized = await _authorizationService.AuthorizeAsync(
                request.TenantId, 
                request.UserRole, 
                "resource", // Default resource
                request.RequiredRole);
            
            var response = new AuthorizationResponse(isAuthorized, DateTime.UtcNow);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authorization check for tenant {TenantId}", request.TenantId);
            return StatusCode(500, new { message = "Authorization service error" });
        }
    }

    [HttpPost("permission")]
    public async Task<ActionResult<PermissionResponse>> CheckPermission([FromBody] PermissionRequest request)
    {
        try
        {
            var hasPermission = await _authorizationService.HasPermissionAsync(
                request.TenantId, 
                request.UserRole, 
                request.Permission);
            
            var response = new PermissionResponse(hasPermission, request.Permission, DateTime.UtcNow);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for tenant {TenantId}", request.TenantId);
            return StatusCode(500, new { message = "Permission check error" });
        }
    }

    [HttpGet("permissions/{role}")]
    public async Task<ActionResult<RolePermissionsResponse>> GetRolePermissions(string role)
    {
        try
        {
            var permissions = await _authorizationService.GetPermissionsForRoleAsync(role);
            var response = new RolePermissionsResponse(role, permissions, DateTime.UtcNow);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for role {Role}", role);
            return StatusCode(500, new { message = "Permission retrieval error" });
        }
    }
}

public record AuthorizationRequest(Guid TenantId, string UserRole, string RequiredRole);
public record AuthorizationResponse(bool IsAuthorized, DateTime CheckedAt);
public record PermissionRequest(Guid TenantId, string UserRole, string Permission);
public record PermissionResponse(bool HasPermission, string Permission, DateTime CheckedAt);
public record RolePermissionsResponse(string Role, List<string> Permissions, DateTime RetrievedAt);
