using Microsoft.AspNetCore.Mvc;
using VirtualQueue.Application.Common.Interfaces;
using VirtualQueue.Application.DTOs;

namespace VirtualQueue.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IUserService userService, ILogger<AuthController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserLoginResponse>> Login([FromBody] UserLoginRequest request)
    {
        try
        {
            var response = await _userService.LoginAsync(request);
            _logger.LogInformation("User {Username} logged in successfully", request.Username);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Login failed for user {Username}: {Message}", request.Username, ex.Message);
            return Unauthorized(new { message = "Invalid credentials" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("Login failed for user {Username}: {Message}", request.Username, ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user {Username}", request.Username);
            return StatusCode(500, new { message = "Login error" });
        }
    }

    [HttpPost("refresh")]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var response = await _userService.RefreshTokenAsync(request);
            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning("Token refresh failed: {Message}", ex.Message);
            return Unauthorized(new { message = "Invalid refresh token" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { message = "Token refresh error" });
        }
    }

    [HttpPost("logout")]
    public async Task<ActionResult> Logout([FromBody] LogoutRequest request)
    {
        try
        {
            await _userService.LogoutAsync(request.UserId);
            _logger.LogInformation("User {UserId} logged out successfully", request.UserId);
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout for user {UserId}", request.UserId);
            return StatusCode(500, new { message = "Logout error" });
        }
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        try
        {
            await _userService.ForgotPasswordAsync(request);
            _logger.LogInformation("Password reset requested for email {Email}", request.Email);
            return Ok(new { message = "Password reset email sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing forgot password for email {Email}", request.Email);
            return StatusCode(500, new { message = "Password reset error" });
        }
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        try
        {
            var success = await _userService.ResetPasswordAsync(request);
            if (success)
            {
                _logger.LogInformation("Password reset successfully");
                return Ok(new { message = "Password reset successfully" });
            }
            else
            {
                return BadRequest(new { message = "Invalid or expired reset token" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting password");
            return StatusCode(500, new { message = "Password reset error" });
        }
    }

    [HttpPost("change-password/{userId}")]
    public async Task<ActionResult> ChangePassword(Guid userId, [FromBody] VirtualQueue.Application.Common.Interfaces.ChangePasswordRequest request)
    {
        try
        {
            var success = await _userService.ChangePasswordAsync(userId, request);
            if (success)
            {
                _logger.LogInformation("Password changed successfully for user {UserId}", userId);
                return Ok(new { message = "Password changed successfully" });
            }
            else
            {
                return BadRequest(new { message = "Current password is incorrect" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", userId);
            return StatusCode(500, new { message = "Password change error" });
        }
    }

    [HttpPost("verify-email")]
    public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        try
        {
            var success = await _userService.VerifyEmailAsync(request);
            if (success)
            {
                _logger.LogInformation("Email verified successfully");
                return Ok(new { message = "Email verified successfully" });
            }
            else
            {
                return BadRequest(new { message = "Invalid or expired verification token" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying email");
            return StatusCode(500, new { message = "Email verification error" });
        }
    }

    [HttpPost("resend-email-verification")]
    public async Task<ActionResult> ResendEmailVerification([FromBody] ResendEmailVerificationRequest request)
    {
        try
        {
            await _userService.ResendEmailVerificationAsync(request);
            _logger.LogInformation("Email verification resent to {Email}", request.Email);
            return Ok(new { message = "Verification email sent" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resending email verification to {Email}", request.Email);
            return StatusCode(500, new { message = "Email verification error" });
        }
    }

    [HttpPost("send-phone-verification")]
    public async Task<ActionResult> SendPhoneVerification([FromBody] SendPhoneVerificationRequest request)
    {
        try
        {
            var success = await _userService.SendPhoneVerificationAsync(request);
            if (success)
            {
                _logger.LogInformation("Phone verification sent to {PhoneNumber}", request.PhoneNumber);
                return Ok(new { message = "Verification code sent" });
            }
            else
            {
                return BadRequest(new { message = "Failed to send verification code" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending phone verification to {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new { message = "Phone verification error" });
        }
    }

    [HttpPost("verify-phone")]
    public async Task<ActionResult> VerifyPhone([FromBody] VerifyPhoneRequest request)
    {
        try
        {
            var success = await _userService.VerifyPhoneAsync(request);
            if (success)
            {
                _logger.LogInformation("Phone verified successfully for {PhoneNumber}", request.PhoneNumber);
                return Ok(new { message = "Phone verified successfully" });
            }
            else
            {
                return BadRequest(new { message = "Invalid verification code" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying phone {PhoneNumber}", request.PhoneNumber);
            return StatusCode(500, new { message = "Phone verification error" });
        }
    }

    [HttpPost("enable-2fa/{userId}")]
    public async Task<ActionResult<EnableTwoFactorResponse>> EnableTwoFactor(Guid userId, [FromBody] VirtualQueue.Application.Common.Interfaces.EnableTwoFactorRequest request)
    {
        try
        {
            var response = await _userService.EnableTwoFactorAsync(userId, request);
            _logger.LogInformation("Two-factor authentication enabled for user {UserId}", userId);
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enabling two-factor authentication for user {UserId}", userId);
            return StatusCode(500, new { message = "Two-factor authentication error" });
        }
    }

    [HttpPost("verify-2fa/{userId}")]
    public async Task<ActionResult> VerifyTwoFactor(Guid userId, [FromBody] VirtualQueue.Application.Common.Interfaces.VerifyTwoFactorRequest request)
    {
        try
        {
            var success = await _userService.VerifyTwoFactorAsync(userId, request);
            if (success)
            {
                _logger.LogInformation("Two-factor authentication verified for user {UserId}", userId);
                return Ok(new { message = "Two-factor authentication verified" });
            }
            else
            {
                return BadRequest(new { message = "Invalid verification code" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying two-factor authentication for user {UserId}", userId);
            return StatusCode(500, new { message = "Two-factor authentication verification error" });
        }
    }

    [HttpPost("disable-2fa/{userId}")]
    public async Task<ActionResult> DisableTwoFactor(Guid userId, [FromBody] VirtualQueue.Application.Common.Interfaces.DisableTwoFactorRequest request)
    {
        try
        {
            await _userService.DisableTwoFactorAsync(userId, request);
            _logger.LogInformation("Two-factor authentication disabled for user {UserId}", userId);
            return Ok(new { message = "Two-factor authentication disabled" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling two-factor authentication for user {UserId}", userId);
            return StatusCode(500, new { message = "Two-factor authentication error" });
        }
    }
}

public record LogoutRequest(Guid UserId);
