using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Multi-Factor Authentication controller for user security management
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MfaController : ControllerBase
{
    private readonly IMfaService _mfaService;
    private readonly ILogger<MfaController> _logger;

    public MfaController(
        IMfaService mfaService,
        ILogger<MfaController> logger)
    {
        _mfaService = mfaService;
        _logger = logger;
    }

    /// <summary>
    /// Get MFA status for the current user
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<MfaStatusResponse>> GetMfaStatus()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");            var status = await _mfaService.GetMfaStatusAsync(userId);
            if (status == null)
                return NotFound("MFA status not found");            return Ok(new MfaStatusResponse
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
    [HttpPost("setup")]
    public async Task<ActionResult<MfaSetupResult>> SetupMfa([FromBody] MfaSetupRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");            var result = await _mfaService.SetupMfaAsync(userId, request.Method);

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
    [HttpPost("verify-setup")]
    public async Task<ActionResult<MfaVerificationResult>> VerifyMfaSetup([FromBody] MfaVerificationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");            var isValid = await _mfaService.VerifyMfaSetupAsync(userId, request.Code);

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
    [HttpPost("validate")]
    public async Task<ActionResult<MfaValidationResult>> ValidateMfa([FromBody] MfaValidationRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");            // Override the UserId from the token for security
            request.UserId = userId;

            var challengeRequest = new Core.Models.MfaChallengeRequest
            {
                UserId = Guid.Parse(userId),
                ChallengeId = request.Code, // This might need proper mapping based on your use case
                Code = request.Code
            };

            var result = await _mfaService.ValidateMfaAsync(challengeRequest);

            if (!result)
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
    [HttpPost("backup-codes/generate")]
    public async Task<ActionResult<BackupCodesResponse>> GenerateBackupCodes()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");

            var backupCodes = await _mfaService.GenerateBackupCodesAsync(userId);

            return Ok(new BackupCodesResponse
            {
                BackupCodes = backupCodes,
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
    [HttpGet("backup-codes/count")]
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
    [HttpPost("disable")]
    public async Task<ActionResult<MfaDisableResult>> DisableMfa([FromBody] MfaDisableRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User not found");            var isDisabled = await _mfaService.DisableMfaAsync(userId, request.ConfirmationCode);

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
    [HttpPost("challenge")]
    [AllowAnonymous] // This might be called during login before full authentication
    public async Task<ActionResult<MfaChallengeResponse>> SendMfaChallenge([FromBody] MfaChallengeRequest request)
    {
        try
        {            var challenge = await _mfaService.SendMfaChallengeAsync(request.UserId, request.Method);

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
    [HttpPost("test-sms")]
    public async Task<ActionResult<TestSmsResponse>> TestSms([FromBody] TestSmsRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))                return Unauthorized("User not found");

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
