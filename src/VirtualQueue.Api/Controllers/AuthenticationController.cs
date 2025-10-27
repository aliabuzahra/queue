using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var authResult = await _authenticationService.AuthenticateAsync(request.Username, request.Password);
            var response = new AuthenticationResponse(authResult.Token, DateTime.UtcNow.AddMinutes(60));
            
            _logger.LogInformation("User {Username} authenticated successfully", request.Username);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            _logger.LogWarning("Authentication failed for user {Username}", request.Username);
            return Unauthorized(new { message = "Invalid credentials" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during authentication for user {Username}", request.Username);
            return StatusCode(500, new { message = "Authentication service error" });
        }
    }

    [HttpPost("validate")]
    public async Task<ActionResult<ValidationResponse>> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        try
        {
            var isValid = await _authenticationService.ValidateTokenAsync(request.Token);
            var tenantId = await _authenticationService.GetTenantIdFromTokenAsync(request.Token);
            var role = await _authenticationService.GetRoleFromTokenAsync(request.Token);
            
            var response = new ValidationResponse(isValid, tenantId, role);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { message = "Token validation error" });
        }
    }

    [HttpPost("api-key")]
    public async Task<ActionResult<ApiKeyResponse>> GenerateApiKey([FromBody] GenerateApiKeyRequest request)
    {
        try
        {
            var apiKey = await _authenticationService.GenerateApiKeyAsync(request.TenantId, request.Role);
            var response = new ApiKeyResponse(apiKey, DateTime.UtcNow.AddDays(365));
            
            _logger.LogInformation("API key generated for tenant {TenantId} with role {Role}", request.TenantId, request.Role);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating API key for tenant {TenantId}", request.TenantId);
            return StatusCode(500, new { message = "API key generation error" });
        }
    }
}

public record LoginRequest(string Username, string Password);
public record AuthenticationResponse(string Token, DateTime ExpiresAt);
public record ValidateTokenRequest(string Token);
public record ValidationResponse(bool IsValid, Guid? TenantId, string? Role);
public record GenerateApiKeyRequest(Guid TenantId, string Role);
public record ApiKeyResponse(string ApiKey, DateTime ExpiresAt);
