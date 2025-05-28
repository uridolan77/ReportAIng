using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Core user domain model - consolidated from multiple user classes
/// </summary>
public class User
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    public bool IsActive { get; set; } = true;

    // Profile information (consolidated from UserProfile)
    public string? ProfilePictureUrl { get; set; }

    // MFA-related properties
    public bool IsMfaEnabled { get; set; } = false;
    public string? MfaSecret { get; set; } = null;
    public MfaMethod MfaMethod { get; set; } = MfaMethod.None;
    public string? PhoneNumber { get; set; } = null;
    public bool IsPhoneNumberVerified { get; set; } = false;
    public DateTime? LastMfaValidationDate { get; set; }
    public string[] BackupCodes { get; set; } = Array.Empty<string>();

    // User preferences and settings
    public UserPreferences Preferences { get; set; } = new();

    // Activity tracking
    public UserActivitySummary ActivitySummary { get; set; } = new();
}

/// <summary>
/// User information for API responses - lightweight version of User
/// </summary>
public class UserInfo
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string[] Roles { get; set; } = Array.Empty<string>();
    public List<string> Permissions { get; set; } = new();
    public UserPreferences Preferences { get; set; } = new();
    public DateTime? LastLogin { get; set; }
    public string? ProfilePictureUrl { get; set; }
}

public class UserPreferences
{
    public string DefaultChartType { get; set; } = "bar";
    public string[] PreferredChartTypes { get; set; } = new[] { "bar", "line", "pie" };
    public string DefaultDateRange { get; set; } = "last_30_days";
    public int MaxRowsPerQuery { get; set; } = 1000;
    public bool EnableNotifications { get; set; } = true;
    public bool EnableQuerySuggestions { get; set; } = true;
    public bool EnableAutoVisualization { get; set; } = true;
    public string Timezone { get; set; } = "UTC";
    public string DateFormat { get; set; } = "yyyy-MM-dd";
    public NotificationSettings NotificationSettings { get; set; } = new();
    public Dictionary<string, object> CustomSettings { get; set; } = new();
}

public class NotificationSettings
{
    public bool Email { get; set; } = true;
    public bool Slack { get; set; } = false;
    public bool Teams { get; set; } = false;
    public bool InApp { get; set; } = true;
    public string[] EventTypes { get; set; } = Array.Empty<string>();
}

public class LoginRequest
{
    [Required(ErrorMessage = "Username is required")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password is required")]
    public string Password { get; set; } = string.Empty;

    public bool RememberMe { get; set; } = false;
}

public class AuthenticationResult
{
    public bool Success { get; set; }
    public string? AccessToken { get; set; }
    public string? RefreshToken { get; set; }
    public int ExpiresIn { get; set; }
    public UserInfo? User { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? ExpiresAt { get; set; }

    // MFA-related properties
    public bool RequiresMfa { get; set; } = false;
    public MfaChallenge? MfaChallenge { get; set; }
}

public class RefreshTokenRequest
{
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}

public class TokenInfo
{
    public string Token { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRevoked { get; set; } = false;
    public string? DeviceInfo { get; set; }
    public string? IpAddress { get; set; }
}

public class UserSession
{
    public string SessionId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime StartTime { get; set; } = DateTime.UtcNow;
    public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public bool IsActive { get; set; } = true;
    public Dictionary<string, object> SessionData { get; set; } = new();
}

public class UserActivity
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? EntityType { get; set; }
    public string? EntityId { get; set; }
    public Dictionary<string, object>? Details { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string SessionId { get; set; } = string.Empty;
}

public enum MfaMethod
{
    None,
    TOTP,
    SMS,
    Email
}

// MFA-related models
public class MfaChallenge
{
    public string ChallengeId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public MfaMethod Method { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;
    public string? Challenge { get; set; } // For SMS/Email, this contains the code
}

public class MfaSetupRequest
{
    [Required(ErrorMessage = "MFA method is required")]
    public MfaMethod Method { get; set; }

    public string? PhoneNumber { get; set; } // Required for SMS
}

public class MfaSetupResult
{
    public bool Success { get; set; }
    public string? Secret { get; set; } // For TOTP
    public string? QrCode { get; set; } // For TOTP
    public string[]? BackupCodes { get; set; }
    public string? ErrorMessage { get; set; }
}

public class MfaValidationRequest
{
    [Required(ErrorMessage = "User ID is required")]
    public string UserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "MFA code is required")]
    public string Code { get; set; } = string.Empty;

    public bool TrustDevice { get; set; } = false;
}

public class MfaValidationResult
{
    public bool Success { get; set; }
    public bool IsBackupCode { get; set; } = false;
    public string? ErrorMessage { get; set; }
}

public class LoginWithMfaRequest : LoginRequest
{
    public string? MfaCode { get; set; }
    public string? ChallengeId { get; set; }
}
