namespace VirtualQueue.Application.Common.Interfaces;

public interface IAuthenticationService
{
    // Methods expected by controllers
    Task<AuthenticationResult> AuthenticateAsync(string username, string password, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string?> GetRoleFromTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string> GenerateApiKeyAsync(Guid tenantId, string description, CancellationToken cancellationToken = default);
    
    // Original methods
    Task<string> GenerateJwtTokenAsync(Guid tenantId, string userIdentifier, List<string> roles, CancellationToken cancellationToken = default);
    Task<bool> ValidateJwtTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<Guid?> GetTenantIdFromTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<string?> GetUserIdentifierFromTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<List<string>> GetRolesFromTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> IsTokenExpiredAsync(string token, CancellationToken cancellationToken = default);
    Task<string> RefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}

public interface IAuthorizationService
{
    // Methods expected by controllers
    Task<bool> AuthorizeAsync(Guid tenantId, string userIdentifier, string resource, string action, CancellationToken cancellationToken = default);
    Task<List<string>> GetPermissionsForRoleAsync(string role, CancellationToken cancellationToken = default);
    
    // Original methods
    Task<bool> HasPermissionAsync(Guid tenantId, string userIdentifier, string permission, CancellationToken cancellationToken = default);
    Task<bool> HasRoleAsync(Guid tenantId, string userIdentifier, string role, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserPermissionsAsync(Guid tenantId, string userIdentifier, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(Guid tenantId, string userIdentifier, CancellationToken cancellationToken = default);
}

public interface IApiKeyService
{
    Task<string> GenerateApiKeyAsync(Guid tenantId, string description, DateTime? expiresAt = null, CancellationToken cancellationToken = default);
    Task<bool> ValidateApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<Guid?> GetTenantIdFromApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task RevokeApiKeyAsync(string apiKey, CancellationToken cancellationToken = default);
    Task<List<ApiKeyDto>> GetApiKeysAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public record ApiKeyDto(
    string Key,
    string Description,
    DateTime CreatedAt,
    DateTime? ExpiresAt,
    bool IsActive
);

// Additional types needed by controllers
public record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword,
    string ConfirmPassword
);

public record EnableTwoFactorRequest(
    string SecretKey,
    string VerificationCode
);

public record VerifyTwoFactorRequest(
    string VerificationCode
);

public record DisableTwoFactorRequest(
    string VerificationCode
);
