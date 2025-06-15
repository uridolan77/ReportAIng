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

// RefreshTokenRequest moved to PerformanceModels.cs - removed duplicate

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

// UserSession moved to PerformanceModels.cs - removed duplicate

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

// MfaChallenge moved to PerformanceModels.cs - removed duplicate

public class MfaSetupRequest
{
    [Required(ErrorMessage = "MFA method is required")]
    public MfaMethod Method { get; set; }

    public string? PhoneNumber { get; set; } // Required for SMS
}

// MfaSetupResult definition moved to IMfaService.cs interface - removed duplicate
// MfaValidationRequest moved to PerformanceModels.cs - removed duplicate

// MfaValidationResult moved to PerformanceModels.cs - removed duplicate

public class LoginWithMfaRequest : LoginRequest
{
    public string? MfaCode { get; set; }
    public string? ChallengeId { get; set; }
}

// Additional models from UserModels.cs
public class DailyActivity
{
    public DateTime Date { get; set; }
    public int QueryCount { get; set; }
    public double AverageResponseTime { get; set; }
}

/// <summary>
/// User permissions and access control
/// </summary>
public class UserPermissions
{
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    public Dictionary<string, bool> FeatureAccess { get; set; } = new();
    public List<string> AllowedDatabases { get; set; } = new();
}
