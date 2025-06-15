using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Security;

/// <summary>
/// Multi-factor authentication service interface
/// </summary>
public interface IMfaService
{
    Task<MfaSetupResult> SetupMfaAsync(string userId, MfaMethod method, CancellationToken cancellationToken = default);
    Task<MfaChallengeResult> InitiateChallengeAsync(string userId, MfaMethod method, CancellationToken cancellationToken = default);
    Task<MfaVerificationResult> VerifyChallengeAsync(string userId, string challengeId, string code, CancellationToken cancellationToken = default);
    Task<bool> DisableMfaAsync(string userId, string verificationCode, CancellationToken cancellationToken = default);
    Task<List<MfaMethod>> GetEnabledMethodsAsync(string userId, CancellationToken cancellationToken = default);
    Task<MfaStatus> GetMfaStatusAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> IsMfaRequiredAsync(string userId, CancellationToken cancellationToken = default);
    Task<MfaBackupCodesResult> GenerateBackupCodesAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> VerifyBackupCodeAsync(string userId, string backupCode, CancellationToken cancellationToken = default);
    Task<bool> ResetMfaAsync(string userId, string adminToken, CancellationToken cancellationToken = default);
    Task<bool> VerifyMfaSetupAsync(string userId, string code, CancellationToken cancellationToken = default);
    Task<MfaValidationResult> ValidateMfaAsync(MfaChallengeRequest challengeRequest, CancellationToken cancellationToken = default);
    Task<int> GetRemainingBackupCodesCountAsync(string userId, CancellationToken cancellationToken = default);    Task<MfaChallengeResult> SendMfaChallengeAsync(string userId, MfaMethod method, CancellationToken cancellationToken = default);
    Task<bool> TestSmsDeliveryAsync(string phoneNumber, CancellationToken cancellationToken = default);
    
    // Additional methods referenced in Infrastructure
    Task<bool> ValidateTotpAsync(string userId, string code, CancellationToken cancellationToken = default);
    Task<string> GenerateSmsCodeAsync(string userId, CancellationToken cancellationToken = default);
    Task SendSmsAsync(string phoneNumber, string code, CancellationToken cancellationToken = default);
    Task<string> GenerateEmailCodeAsync(string userId, CancellationToken cancellationToken = default);
    Task SendEmailCodeAsync(string email, string code, CancellationToken cancellationToken = default);
    Task<string> GenerateSecretAsync(string userId, CancellationToken cancellationToken = default);
    Task<string> GenerateQrCodeAsync(string userId, string secret, CancellationToken cancellationToken = default);
}

/// <summary>
/// MFA setup result
/// </summary>
public class MfaSetupResult
{
    public bool Success { get; set; }
    public string SecretKey { get; set; } = string.Empty;
    public string QrCodeUrl { get; set; } = string.Empty;
    public List<string> BackupCodes { get; set; } = new();
    public string Message { get; set; } = string.Empty;
    
    // Additional properties referenced in Infrastructure
    public string ErrorMessage { get; set; } = string.Empty;
    public string Secret
    {
        get => SecretKey;
        set => SecretKey = value;
    }
    public string QrCode
    {
        get => QrCodeUrl;
        set => QrCodeUrl = value;
    }
}

/// <summary>
/// MFA challenge result
/// </summary>
public class MfaChallengeResult
{
    public bool Success { get; set; }
    public string ChallengeId { get; set; } = string.Empty;
    public MfaMethod Method { get; set; }
    public string DeliveryTarget { get; set; } = string.Empty; // Email or phone number (masked)
    public DateTime ExpiresAt { get; set; }
    public string Message { get; set; } = string.Empty;
    
    // Additional properties referenced in Infrastructure
    public string ErrorMessage { get; set; } = string.Empty;
    public string MaskedDeliveryAddress
    {
        get => DeliveryTarget;
        set => DeliveryTarget = value;
    }
}

/// <summary>
/// MfaBackupCodesResult
/// </summary>
public class MfaBackupCodesResult
{
    public bool Success { get; set; }
    public List<string> BackupCodes { get; set; } = new();
    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// MFA verification result
/// </summary>
public class MfaVerificationResult
{
    public bool Success { get; set; }
    public bool IsBackupCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime? NextAttemptAllowedAt { get; set; }
    public int RemainingAttempts { get; set; }
}

/// <summary>
/// MFA validation result  
/// </summary>
public class MfaValidationResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public bool IsBackupCode { get; set; }
    public DateTime? NextAttemptAllowedAt { get; set; }
    public int RemainingAttempts { get; set; }
    
    // Additional properties referenced in Infrastructure
    public string ErrorMessage { get; set; } = string.Empty;
}
