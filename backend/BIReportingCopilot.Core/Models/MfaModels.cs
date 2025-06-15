namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Represents the current MFA status for a user
/// </summary>
public class MfaStatus
{
    public bool IsEnabled { get; set; } = false;
    public MfaMethod Method { get; set; } = MfaMethod.None;
    public int BackupCodesCount { get; set; } = 0;
    public bool HasPhoneNumber { get; set; } = false;
    public bool IsPhoneNumberVerified { get; set; } = false;
    public bool HasBackupCodes { get; set; } = false;
    public string? MaskedPhoneNumber { get; set; }
    public string? MaskedEmail { get; set; }
    public DateTime? LastValidationDate { get; set; }
    
    // Additional properties referenced in Infrastructure
    public bool TotpEnabled { get; set; } = false;
    public bool SmsEnabled { get; set; } = false;
    public bool EmailEnabled { get; set; } = false;
}

/// <summary>
/// Information returned when setting up MFA
/// </summary>
public class MfaSetupInfo
{
    public MfaMethod Method { get; set; }
    public string? TotpSecret { get; set; }
    public string? QrCodeUrl { get; set; }
    public string[] BackupCodes { get; set; } = Array.Empty<string>();
    public bool Success { get; set; } = true;
    public string? ErrorMessage { get; set; }
    
    // Additional properties referenced in Infrastructure
    public string? SecretKey
    {
        get => TotpSecret;
        set => TotpSecret = value;
    }
}

/// <summary>
/// Request to send an MFA challenge (SMS/Email)
/// </summary>
public class MfaChallengeRequest
{
    public Guid UserId { get; set; }
    public MfaMethod Method { get; set; }
    public string? PhoneNumber { get; set; } // For SMS
    public string? Email { get; set; } // For Email
    public string? ChallengeId { get; set; } // For validation
    public string? Code { get; set; } // For validation
}

/// <summary>
/// Response after sending an MFA challenge
/// </summary>
public class MfaChallengeResponse
{
    public string ChallengeId { get; set; } = string.Empty;
    public MfaMethod Method { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string? MaskedDeliveryAddress { get; set; } // Masked phone/email
    
    // Additional properties referenced in Infrastructure
    public string? DeliveryTarget
    {
        get => MaskedDeliveryAddress;
        set => MaskedDeliveryAddress = value;
    }
    
    public string? Message
    {
        get => ErrorMessage;
        set => ErrorMessage = value;
    }
}
