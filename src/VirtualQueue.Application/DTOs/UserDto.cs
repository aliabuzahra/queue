using VirtualQueue.Domain.Enums;

namespace VirtualQueue.Application.DTOs;

public record UserDto(
    Guid Id,
    Guid TenantId,
    string Username,
    string Email,
    string FirstName,
    string LastName,
    string? PhoneNumber,
    UserRole Role,
    UserStatus Status,
    DateTime? LastLoginAt,
    DateTime? EmailVerifiedAt,
    DateTime? PhoneVerifiedAt,
    bool IsTwoFactorEnabled,
    string? ProfileImageUrl,
    Dictionary<string, string> Metadata,
    DateTime CreatedAt,
    DateTime? LastModifiedAt);

public record CreateUserRequest(
    string Username,
    string Email,
    string Password,
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    UserRole Role = UserRole.User);

public record UpdateUserProfileRequest(
    string FirstName,
    string LastName,
    string? PhoneNumber = null,
    string? ProfileImageUrl = null);

public record UpdateUserEmailRequest(string Email);
public record UpdateUserPasswordRequest(string CurrentPassword, string NewPassword);
public record UpdateUserRoleRequest(UserRole Role);
public record UserStatusUpdateRequest(UserStatus Status, string? Reason = null);

public record UserLoginRequest(string Username, string Password, string? TwoFactorCode = null);
public record UserLoginResponse(
    string AccessToken,
    string? RefreshToken,
    DateTime ExpiresAt,
    UserDto User);

public record RefreshTokenRequest(string RefreshToken);
public record RefreshTokenResponse(
    string AccessToken,
    string? RefreshToken,
    DateTime ExpiresAt);

public record EnableTwoFactorRequest(string Password);
public record EnableTwoFactorResponse(string Secret, string QrCodeUrl);
public record VerifyTwoFactorRequest(string Code);
public record DisableTwoFactorRequest(string Password, string Code);

public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string Token, string NewPassword);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);

public record VerifyEmailRequest(string Token);
public record ResendEmailVerificationRequest(string Email);
public record VerifyPhoneRequest(string PhoneNumber, string Code);
public record SendPhoneVerificationRequest(string PhoneNumber);
