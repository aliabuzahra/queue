using Microsoft.Extensions.Logging;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;
using VirtualQueue.Domain.Entities;
using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IAuthenticationService authenticationService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _authenticationService = authenticationService;
        _logger = logger;
    }

    public async Task<UserDto> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating user {Username}", request.Username);

        // Hash password
        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

        // Create user entity
        var user = new User(
            Guid.Empty, // TenantId will be set by the calling context
            request.Username,
            request.Email,
            passwordHash,
            request.FirstName,
            request.LastName,
            request.Role);

        if (!string.IsNullOrEmpty(request.PhoneNumber))
        {
            user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber);
        }

        // Save user
        var savedUser = await _userRepository.AddAsync(user, cancellationToken);

        _logger.LogInformation("User {Username} created successfully with ID {UserId}", request.Username, savedUser.Id);

        return MapToDto(savedUser);
    }

    public async Task<UserDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByUsernameAsync(Guid tenantId, string username, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameAsync(tenantId, username, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<UserDto?> GetUserByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(tenantId, email, cancellationToken);
        return user != null ? MapToDto(user) : null;
    }

    public async Task<List<UserDto>> GetUsersByTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        return users.Select(MapToDto).ToList();
    }

    public async Task<List<UserDto>> SearchUsersAsync(Guid tenantId, string searchTerm, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.SearchUsersAsync(tenantId, searchTerm, cancellationToken);
        return users.Select(MapToDto).ToList();
    }

    public async Task UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        user.UpdateProfile(request.FirstName, request.LastName, request.PhoneNumber, request.ProfileImageUrl);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User profile updated for {UserId}", userId);
    }

    public async Task UpdateUserEmailAsync(Guid userId, UpdateUserEmailRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        user.UpdateEmail(request.Email);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User email updated for {UserId}", userId);
    }

    public async Task UpdateUserPasswordAsync(Guid userId, UpdateUserPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // Verify current password
        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect");

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User password updated for {UserId}", userId);
    }

    public async Task UpdateUserRoleAsync(Guid userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        user.UpdateRole(request.Role);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User role updated for {UserId} to {Role}", userId, request.Role);
    }

    public async Task UpdateUserStatusAsync(Guid userId, UserStatusUpdateRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        switch (request.Status)
        {
            case UserStatus.Active:
                user.Activate();
                break;
            case UserStatus.Inactive:
                user.Deactivate();
                break;
            case UserStatus.Suspended:
                user.Suspend(request.Reason ?? "No reason provided");
                break;
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User status updated for {UserId} to {Status}", userId, request.Status);
    }

    public async Task DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        await _userRepository.DeleteAsync(user, cancellationToken);

        _logger.LogInformation("User deleted: {UserId}", userId);
    }

    public async Task<UserLoginResponse> LoginAsync(UserLoginRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(Guid.Empty, request.Username, cancellationToken);
        if (user == null || !user.CanLogin())
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        // Check two-factor authentication if enabled
        if (user.IsTwoFactorEnabled)
        {
            if (string.IsNullOrEmpty(request.TwoFactorCode))
                throw new InvalidOperationException("Two-factor authentication code required");

            // In a real implementation, you would verify the TOTP code here
            // For now, we'll just check if the code is not empty
            if (request.TwoFactorCode.Length != 6)
                throw new UnauthorizedAccessException("Invalid two-factor authentication code");
        }

        // Generate tokens
        var accessToken = await _authenticationService.GenerateJwtTokenAsync(user.TenantId, user.Id.ToString(), new List<string> { user.Role.ToString() }, cancellationToken);
        var refreshToken = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        // Update user
        user.RecordLogin();
        user.SetRefreshToken(refreshToken, expiresAt.AddDays(7));
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("User {Username} logged in successfully", user.Username);

        return new UserLoginResponse(
            accessToken,
            refreshToken,
            expiresAt,
            MapToDto(user));
    }

    public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(Guid.Empty, cancellationToken); // In real implementation, find by refresh token
        if (user == null || !user.HasValidRefreshToken())
            throw new UnauthorizedAccessException("Invalid refresh token");

        var accessToken = await _authenticationService.GenerateJwtTokenAsync(user.TenantId, user.Id.ToString(), new List<string> { user.Role.ToString() }, cancellationToken);
        var newRefreshToken = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddMinutes(60);

        user.SetRefreshToken(newRefreshToken, expiresAt.AddDays(7));
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new RefreshTokenResponse(accessToken, newRefreshToken, expiresAt);
    }

    public async Task LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user != null)
        {
            user.RevokeRefreshToken();
            await _userRepository.UpdateAsync(user, cancellationToken);
        }

        _logger.LogInformation("User {UserId} logged out", userId);
    }

    public async Task<bool> ValidateCredentialsAsync(Guid tenantId, string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByUsernameOrEmailAsync(tenantId, username, cancellationToken);
        return user != null && BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<EnableTwoFactorResponse> EnableTwoFactorAsync(Guid userId, VirtualQueue.Application.Common.Interfaces.EnableTwoFactorRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // Note: EnableTwoFactorRequest doesn't have Password property in the interface
        // In a real implementation, you would validate the user's current password separately
        // For now, we'll skip password validation

        var secret = Guid.NewGuid().ToString("N")[..32]; // Generate a secret
        var qrCodeUrl = $"otpauth://totp/VirtualQueue:{user.Username}?secret={secret}&issuer=VirtualQueue";

        user.EnableTwoFactor(secret);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Two-factor authentication enabled for user {UserId}", userId);

        return new EnableTwoFactorResponse(secret, qrCodeUrl);
    }

    public async Task<bool> VerifyTwoFactorAsync(Guid userId, VirtualQueue.Application.Common.Interfaces.VerifyTwoFactorRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null || !user.IsTwoFactorEnabled)
            return false;

        // In a real implementation, you would verify the TOTP code using the secret
        // For now, we'll just check if the code is 6 digits
        return request.VerificationCode.Length == 6 && request.VerificationCode.All(char.IsDigit);
    }

    public async Task DisableTwoFactorAsync(Guid userId, VirtualQueue.Application.Common.Interfaces.DisableTwoFactorRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            throw new InvalidOperationException($"User with ID {userId} not found");

        // Note: DisableTwoFactorRequest doesn't have Password property in the interface
        // In a real implementation, you would validate the user's current password separately
        // For now, we'll skip password validation

        if (!await VerifyTwoFactorAsync(userId, new VirtualQueue.Application.Common.Interfaces.VerifyTwoFactorRequest(request.VerificationCode), cancellationToken))
            throw new UnauthorizedAccessException("Invalid two-factor authentication code");

        user.DisableTwoFactor();
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Two-factor authentication disabled for user {UserId}", userId);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        // In a real implementation, you would generate a reset token and send an email
        _logger.LogInformation("Password reset requested for email {Email}", request.Email);
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        // In a real implementation, you would validate the token and reset the password
        _logger.LogInformation("Password reset attempted with token {Token}", request.Token);
        return true; // Mock implementation
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, VirtualQueue.Application.Common.Interfaces.ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user == null)
            return false;

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            return false;

        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatePassword(newPasswordHash);
        await _userRepository.UpdateAsync(user, cancellationToken);

        _logger.LogInformation("Password changed for user {UserId}", userId);
        return true;
    }

    public async Task<bool> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        // In a real implementation, you would validate the token and mark email as verified
        _logger.LogInformation("Email verification attempted with token {Token}", request.Token);
        return true; // Mock implementation
    }

    public async Task ResendEmailVerificationAsync(ResendEmailVerificationRequest request, CancellationToken cancellationToken = default)
    {
        // In a real implementation, you would send a verification email
        _logger.LogInformation("Email verification resent to {Email}", request.Email);
    }

    public async Task<bool> SendPhoneVerificationAsync(SendPhoneVerificationRequest request, CancellationToken cancellationToken = default)
    {
        // In a real implementation, you would send an SMS
        _logger.LogInformation("Phone verification sent to {PhoneNumber}", request.PhoneNumber);
        return true; // Mock implementation
    }

    public async Task<bool> VerifyPhoneAsync(VerifyPhoneRequest request, CancellationToken cancellationToken = default)
    {
        // In a real implementation, you would verify the SMS code
        _logger.LogInformation("Phone verification attempted for {PhoneNumber} with code {Code}", request.PhoneNumber, request.Code);
        return true; // Mock implementation
    }

    public async Task<int> GetUserCountAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await _userRepository.GetUserCountAsync(tenantId, cancellationToken);
    }

    public async Task<List<UserDto>> GetRecentlyActiveUsersAsync(Guid tenantId, DateTime since, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetRecentlyActiveUsersAsync(tenantId, since, cancellationToken);
        return users.Select(MapToDto).ToList();
    }

    public async Task<Dictionary<string, int>> GetUserStatisticsAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        
        return new Dictionary<string, int>
        {
            ["Total"] = users.Count,
            ["Active"] = users.Count(u => u.Status == UserStatus.Active),
            ["Inactive"] = users.Count(u => u.Status == UserStatus.Inactive),
            ["Suspended"] = users.Count(u => u.Status == UserStatus.Suspended),
            ["EmailVerified"] = users.Count(u => u.IsEmailVerified()),
            ["PhoneVerified"] = users.Count(u => u.IsPhoneVerified()),
            ["TwoFactorEnabled"] = users.Count(u => u.IsTwoFactorEnabled)
        };
    }

    private static UserDto MapToDto(User user)
    {
        return new UserDto(
            user.Id,
            user.TenantId,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.PhoneNumber,
            user.Role,
            user.Status,
            user.LastLoginAt,
            user.EmailVerifiedAt,
            user.PhoneVerifiedAt,
            user.IsTwoFactorEnabled,
            user.ProfileImageUrl,
            user.Metadata,
            user.CreatedAt,
            user.UpdatedAt);
    }
}
