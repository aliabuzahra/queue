using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Application.Common.Interfaces;

public interface IUserService
{
    // User Management
    Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByUsernameAsync(Guid tenantId, string username, CancellationToken cancellationToken = default);
    Task<UserDto?> GetUserByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetUsersByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<UserDto>> SearchUsersAsync(Guid tenantId, string searchTerm, CancellationToken cancellationToken = default);
    Task UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserEmailAsync(Guid userId, UpdateUserEmailRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserPasswordAsync(Guid userId, UpdateUserPasswordRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
    Task UpdateUserStatusAsync(Guid userId, UserStatusUpdateRequest request, CancellationToken cancellationToken = default);
    Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);

    // Authentication
    Task<UserLoginResponse> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default);
    Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateCredentialsAsync(Guid tenantId, string username, string password, CancellationToken cancellationToken = default);

    // Two-Factor Authentication
    Task<EnableTwoFactorResponse> EnableTwoFactorAsync(Guid userId, EnableTwoFactorRequest request, CancellationToken cancellationToken = default);
    Task<bool> VerifyTwoFactorAsync(Guid userId, VerifyTwoFactorRequest request, CancellationToken cancellationToken = default);
    Task DisableTwoFactorAsync(Guid userId, DisableTwoFactorRequest request, CancellationToken cancellationToken = default);

    // Password Management
    Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken cancellationToken = default);

    // Email Verification
    Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default);
    Task ResendEmailVerificationAsync(ResendEmailVerificationRequest request, CancellationToken cancellationToken = default);

    // Phone Verification
    Task<bool> SendPhoneVerificationAsync(SendPhoneVerificationRequest request, CancellationToken cancellationToken = default);
    Task<bool> VerifyPhoneAsync(VerifyPhoneRequest request, CancellationToken cancellationToken = default);

    // User Statistics
    Task<int> GetUserCountAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<List<UserDto>> GetRecentlyActiveUsersAsync(Guid tenantId, DateTime since, CancellationToken cancellationToken = default);
    Task<Dictionary<string, int>> GetUserStatisticsAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
