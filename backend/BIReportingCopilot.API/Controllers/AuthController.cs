using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly ILogger<AuthController> _logger;
    private readonly IAuthenticationService _authService;
    private readonly IMfaService _mfaService;

    public AuthController(
        ILogger<AuthController> logger,
        IAuthenticationService authService,
        IMfaService mfaService)
    {
        _logger = logger;
        _authService = authService;
        _mfaService = mfaService;
    }

    /// <summary>
    /// Authenticate user with username and password
    /// </summary>
    /// <param name="request">Login credentials</param>
    /// <returns>Authentication result with JWT token</returns>
    [HttpPost("login")]
    public async Task<ActionResult<AuthenticationResult>> Login([FromBody] LoginRequest request)
    {
        try
        {
            _logger.LogInformation("Login attempt for user: {Username}", request.Username);

            var result = await _authService.AuthenticateAsync(request);

            if (result.Success)
            {
                _logger.LogInformation("Successful login for user: {Username}", request.Username);

                // Debug: Log the token details
                if (!string.IsNullOrEmpty(result.AccessToken))
                {
                    try
                    {
                        var tokenParts = result.AccessToken.Split('.');
                        if (tokenParts.Length == 3)
                        {
                            var payload = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(
                                System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(tokenParts[1] + "==")));
                            _logger.LogInformation("Generated token - Issuer: {Issuer}, Audience: {Audience}",
                                payload?.GetValueOrDefault("iss"), payload?.GetValueOrDefault("aud"));
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to decode token for debugging");
                    }
                }

                return Ok(result);
            }
            else
            {
                _logger.LogWarning("Failed login attempt for user: {Username}", request.Username);
                return Unauthorized(new { error = "Invalid username or password" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login for user: {Username}", request.Username);
            return StatusCode(500, new { error = "An error occurred during authentication" });
        }
    }

    /// <summary>
    /// Register a new user account (placeholder - not implemented in interface)
    /// </summary>
    /// <param name="request">Registration details</param>
    /// <returns>Registration result</returns>
    [HttpPost("register")]
    public ActionResult<AuthenticationResult> Register([FromBody] User request)
    {
        try
        {
            _logger.LogInformation("Registration attempt for user: {Username}", request.Username);

            // Registration not implemented in current interface
            return BadRequest(new { error = "Registration not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration for user: {Username}", request.Username);
            return StatusCode(500, new { error = "An error occurred during registration" });
        }
    }

    /// <summary>
    /// Refresh JWT token
    /// </summary>
    /// <param name="request">Refresh token request</param>
    /// <returns>New JWT token</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult<AuthenticationResult>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Token refresh attempt");

            var result = await _authService.RefreshTokenAsync(request);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(new { error = "Invalid refresh token" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
            return StatusCode(500, new { error = "An error occurred during token refresh" });
        }
    }

    /// <summary>
    /// Logout user and invalidate tokens
    /// </summary>
    /// <param name="request">Logout request</param>
    /// <returns>Success status</returns>
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
            _logger.LogInformation("Logout attempt for user: {UserId}", userId);

            await _authService.RevokeTokenAsync(request.RefreshToken);

            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, new { error = "An error occurred during logout" });
        }
    }

    /// <summary>
    /// Validate JWT token
    /// </summary>
    /// <returns>Token validation result</returns>
    [HttpGet("validate")]
    [Authorize]
    public ActionResult ValidateToken()
    {
        try
        {
            var userId = User.FindFirst("sub")?.Value ?? User.FindFirst("user_id")?.Value;
            var username = User.FindFirst("username")?.Value ?? User.Identity?.Name;

            return Ok(new
            {
                valid = true,
                userId = userId,
                username = username,
                expires = User.FindFirst("exp")?.Value
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating token");
            return StatusCode(500, new { error = "An error occurred during token validation" });
        }
    }

    /// <summary>
    /// Request password reset (placeholder - not implemented in interface)
    /// </summary>
    /// <param name="request">Password reset request</param>
    /// <returns>Success status</returns>
    [HttpPost("forgot-password")]
    public ActionResult ForgotPassword([FromBody] User request)
    {
        try
        {
            _logger.LogInformation("Password reset request for email: {Email}", request.Email);

            // Password reset not implemented in current interface
            return Ok(new { message = "Password reset not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request");
            return StatusCode(500, new { error = "An error occurred during password reset request" });
        }
    }

    /// <summary>
    /// Reset password with token (placeholder - not implemented in interface)
    /// </summary>
    /// <param name="request">Password reset confirmation</param>
    /// <returns>Success status</returns>
    [HttpPost("reset-password")]
    public ActionResult ResetPassword([FromBody] RefreshTokenRequest request)
    {
        try
        {
            _logger.LogInformation("Password reset confirmation attempt");

            // Password reset not implemented in current interface
            return Ok(new { message = "Password reset not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return StatusCode(500, new { error = "An error occurred during password reset" });
        }
    }

    #region Multi-Factor Authentication

    /// <summary>
    /// Get MFA status for the current user
    /// </summary>
    [HttpGet("mfa/status")]
    [Authorize]
    public async Task<ActionResult<MfaStatusResponse>> GetMfaStatus()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var status = await _mfaService.GetMfaStatusAsync(userId);
            if (status == null)
                return NotFound("MFA status not found");

            return Ok(new MfaStatusResponse
            {
                IsEnabled = status.IsEnabled,
                Method = status.Method,
                IsPhoneNumberVerified = status.IsPhoneNumberVerified,
                HasBackupCodes = status.HasBackupCodes,
                LastValidationDate = status.LastValidationDate,
                BackupCodesCount = status.BackupCodesCount,
                HasPhoneNumber = status.HasPhoneNumber,
                MaskedPhoneNumber = status.MaskedPhoneNumber,
                MaskedEmail = status.MaskedEmail
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA status for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Setup MFA for the current user
    /// </summary>
    [HttpPost("mfa/setup")]
    [Authorize]
    public async Task<ActionResult<MfaSetupResult>> SetupMfa([FromBody] MfaSetupRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var result = await _mfaService.SetupMfaAsync(userId, request.Method);

            if (result == null || !result.Success)
                return BadRequest(result?.ErrorMessage ?? "Failed to setup MFA");

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up MFA for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Verify MFA setup with a test code
    /// </summary>
    [HttpPost("mfa/verify-setup")]
    [Authorize]
    public async Task<ActionResult<MfaVerificationResult>> VerifyMfaSetup([FromBody] MfaVerificationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var isValid = await _mfaService.VerifyMfaSetupAsync(userId, request.Code);

            if (!isValid)
                return BadRequest("Invalid verification code");

            return Ok(new MfaVerificationResult
            {
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA setup for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Validate MFA code
    /// </summary>
    [HttpPost("mfa/validate")]
    [Authorize]
    public async Task<ActionResult<MfaValidationResult>> ValidateMfa([FromBody] MfaValidationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            // Override the UserId from the token for security
            request.UserId = userId;

            var challengeRequest = new Core.Models.MfaChallengeRequest
            {
                UserId = Guid.Parse(userId),
                ChallengeId = request.Code, // This might need proper mapping based on your use case
                Code = request.Code
            };

            var result = await _mfaService.ValidateMfaAsync(challengeRequest);

            if (result == null || !result.Success)
                return BadRequest("Invalid MFA code");

            return Ok(new MfaValidationResult
            {
                Success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MFA code for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Generate new backup codes
    /// </summary>
    [HttpPost("mfa/backup-codes/generate")]
    [Authorize]
    public async Task<ActionResult<BackupCodesResponse>> GenerateBackupCodes()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var backupCodesResult = await _mfaService.GenerateBackupCodesAsync(userId);

            return Ok(new BackupCodesResponse
            {
                BackupCodes = backupCodesResult.BackupCodes.ToArray(),
                Message = "New backup codes generated. Please store them securely."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get remaining backup codes count
    /// </summary>
    [HttpGet("mfa/backup-codes/count")]
    [Authorize]
    public async Task<ActionResult<BackupCodesCountResponse>> GetBackupCodesCount()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var count = await _mfaService.GetRemainingBackupCodesCountAsync(userId);

            return Ok(new BackupCodesCountResponse
            {
                RemainingCount = count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting backup codes count for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Disable MFA for the current user
    /// </summary>
    [HttpPost("mfa/disable")]
    [Authorize]
    public async Task<ActionResult<MfaDisableResult>> DisableMfa([FromBody] MfaDisableRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var isDisabled = await _mfaService.DisableMfaAsync(userId, request.ConfirmationCode);

            if (!isDisabled)
                return BadRequest("Failed to disable MFA");

            return Ok(new MfaDisableResult
            {
                Success = true,
                Message = "MFA has been disabled successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling MFA for user");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Send MFA challenge (for SMS/Email methods)
    /// </summary>
    [HttpPost("mfa/challenge")]
    [AllowAnonymous] // This might be called during login before full authentication
    public async Task<ActionResult<MfaChallengeResponse>> SendMfaChallenge([FromBody] MfaChallengeRequest request)
    {
        try
        {
            var challenge = await _mfaService.SendMfaChallengeAsync(request.UserId, request.Method);

            if (challenge == null)
                return BadRequest("Unable to send MFA challenge");

            return Ok(new MfaChallengeResponse
            {
                ChallengeId = challenge.ChallengeId,
                Method = challenge.Method,
                ExpiresAt = challenge.ExpiresAt,
                DeliveryTarget = challenge.MaskedDeliveryAddress ?? "****"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending MFA challenge");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Test phone number for SMS MFA
    /// </summary>
    [HttpPost("mfa/test-sms")]
    [Authorize]
    public async Task<ActionResult<TestSmsResponse>> TestSms([FromBody] TestSmsRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var result = await _mfaService.TestSmsDeliveryAsync(request.PhoneNumber);

            return Ok(new TestSmsResponse
            {
                Success = result,
                Message = result ? "Test SMS sent successfully" : "Failed to send test SMS"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SMS delivery for user");
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion
}

// DTOs for MFA endpoints
public class MfaStatusResponse
{
    public bool IsEnabled { get; set; }
    public MfaMethod Method { get; set; }
    public bool IsPhoneNumberVerified { get; set; }
    public bool HasBackupCodes { get; set; }
    public DateTime? LastValidationDate { get; set; }
    public int BackupCodesCount { get; set; }
    public bool HasPhoneNumber { get; set; }
    public string? MaskedPhoneNumber { get; set; }
    public string? MaskedEmail { get; set; }
}

public class MfaVerificationRequest
{
    [Required(ErrorMessage = "Verification code is required")]
    public string Code { get; set; } = string.Empty;
}

public class MfaVerificationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string[]? BackupCodes { get; set; }
}

public class BackupCodesResponse
{
    public string[] BackupCodes { get; set; } = Array.Empty<string>();
    public string Message { get; set; } = string.Empty;
}

public class BackupCodesCountResponse
{
    public int RemainingCount { get; set; }
}

public class MfaDisableRequest
{
    [Required(ErrorMessage = "Confirmation code is required")]
    public string ConfirmationCode { get; set; } = string.Empty;
}

public class MfaDisableResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class MfaSetupRequest
{
    [Required(ErrorMessage = "MFA method is required")]
    public MfaMethod Method { get; set; }
}

public class MfaSetupResult
{
    public MfaMethod Method { get; set; }
    public string? TotpSecret { get; set; }
    public string? QrCodeUrl { get; set; }
    public string[] BackupCodes { get; set; } = Array.Empty<string>();
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
}

public class MfaChallengeRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Method is required")]
    public MfaMethod Method { get; set; }
}

public class MfaChallengeResponse
{
    public string ChallengeId { get; set; } = string.Empty;
    public MfaMethod Method { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string DeliveryTarget { get; set; } = string.Empty;
}

public class TestSmsRequest
{
    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Invalid phone number format")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class TestSmsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}
