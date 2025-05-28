using System.ComponentModel.DataAnnotations;
using BIReportingCopilot.Core.Constants;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// JWT configuration settings with validation
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    [Required(ErrorMessage = "JWT Secret is required")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters long")]
    public string Secret { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required")]
    public string Issuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required")]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "Access token expiration must be between 1 and 1440 minutes")]
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    [Range(1, 43200, ErrorMessage = "Refresh token expiration must be between 1 and 43200 minutes (30 days)")]
    public int RefreshTokenExpirationMinutes { get; set; } = 10080; // 7 days

    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;
    public bool ValidateIssuerSigningKey { get; set; } = true;
}

/// <summary>
/// OpenAI service configuration with validation
/// </summary>
public class OpenAISettings
{
    public const string SectionName = "OpenAI";

    // API Key is not required to allow for testing/development scenarios
    public string ApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "OpenAI Model is required")]
    public string Model { get; set; } = "gpt-4";

    [Range(0.0, 2.0, ErrorMessage = "Temperature must be between 0.0 and 2.0")]
    public double Temperature { get; set; } = 0.1;

    [Range(1, 4096, ErrorMessage = "Max tokens must be between 1 and 4096")]
    public int MaxTokens { get; set; } = 2000;

    [Range(1, 30, ErrorMessage = "Timeout must be between 1 and 30 seconds")]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(1, 5, ErrorMessage = "Max retries must be between 1 and 5")]
    public int MaxRetries { get; set; } = 3;

    public bool EnableCaching { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// Rate limiting configuration with validation
/// </summary>
public class RateLimitSettings
{
    public const string SectionName = "RateLimit";

    [Range(1, 10000, ErrorMessage = "Requests per window must be between 1 and 10000")]
    public int RequestsPerWindow { get; set; } = ApplicationConstants.Defaults.DefaultRateLimitRequests;

    [Range(1, 60, ErrorMessage = "Window size must be between 1 and 60 minutes")]
    public int WindowSizeMinutes { get; set; } = ApplicationConstants.Defaults.DefaultRateLimitWindowMinutes;

    public bool EnableRateLimiting { get; set; } = true;
    public bool EnablePerUserLimits { get; set; } = true;
    public bool EnablePerEndpointLimits { get; set; } = true;

    public Dictionary<string, EndpointRateLimit> EndpointLimits { get; set; } = new();
}

/// <summary>
/// Per-endpoint rate limiting configuration
/// </summary>
public class EndpointRateLimit
{
    [Range(1, 10000, ErrorMessage = "Requests per window must be between 1 and 10000")]
    public int RequestsPerWindow { get; set; }

    [Range(1, 60, ErrorMessage = "Window size must be between 1 and 60 minutes")]
    public int WindowSizeMinutes { get; set; }
}

/// <summary>
/// Query processing configuration with validation
/// </summary>
public class QuerySettings
{
    public const string SectionName = "QuerySettings";

    [Range(1, 300, ErrorMessage = "Default timeout must be between 1 and 300 seconds")]
    public int DefaultTimeoutSeconds { get; set; } = ApplicationConstants.Defaults.DefaultQueryTimeoutSeconds;

    [Range(1, 1000000, ErrorMessage = "Max rows per query must be between 1 and 1,000,000")]
    public int MaxRowsPerQuery { get; set; } = ApplicationConstants.Defaults.DefaultMaxRowsPerQuery;

    [Range(1, 3600, ErrorMessage = "Cache expiration must be between 1 and 3600 seconds")]
    public int CacheExpirationSeconds { get; set; } = ApplicationConstants.Defaults.DefaultCacheExpirationMinutes * 60;

    public bool EnableQueryCaching { get; set; } = true;
    public bool EnableQueryValidation { get; set; } = true;
    public bool EnableQueryLogging { get; set; } = true;
    public bool EnablePerformanceMetrics { get; set; } = true;

    public List<string> AllowedDatabases { get; set; } = new();
    public List<string> BlockedKeywords { get; set; } = new() { "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "CREATE", "TRUNCATE" };
}

/// <summary>
/// Security configuration with validation
/// </summary>
public class SecuritySettings
{
    public const string SectionName = "Security";

    [Range(1, 10, ErrorMessage = "Max login attempts must be between 1 and 10")]
    public int MaxLoginAttempts { get; set; } = 5;

    [Range(1, 1440, ErrorMessage = "Lockout duration must be between 1 and 1440 minutes")]
    public int LockoutDurationMinutes { get; set; } = 15;

    [Range(8, 128, ErrorMessage = "Min password length must be between 8 and 128")]
    public int MinPasswordLength { get; set; } = ApplicationConstants.Validation.MinPasswordLength;

    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public bool EnableTwoFactorAuthentication { get; set; } = false;
    public bool EnablePasswordHistory { get; set; } = true;

    [Range(1, 24, ErrorMessage = "Password history count must be between 1 and 24")]
    public int PasswordHistoryCount { get; set; } = 5;

    // MFA-related settings
    [Range(1, 60, ErrorMessage = "MFA code expiration must be between 1 and 60 minutes")]
    public int MfaCodeExpirationMinutes { get; set; } = 5;

    [Range(1, 10, ErrorMessage = "Max MFA attempts must be between 1 and 10")]
    public int MaxMfaAttempts { get; set; } = 3;

    [Range(1, 1440, ErrorMessage = "MFA lockout duration must be between 1 and 1440 minutes")]
    public int MfaLockoutDurationMinutes { get; set; } = 30;

    public bool EnableMfaRememberDevice { get; set; } = true;

    [Range(1, 365, ErrorMessage = "MFA remember device duration must be between 1 and 365 days")]
    public int MfaRememberDeviceDays { get; set; } = 30;

    public List<string> AllowedOrigins { get; set; } = new();
    public List<string> TrustedProxies { get; set; } = new();
}

/// <summary>
/// Cache configuration with validation
/// </summary>
public class CacheSettings
{
    public const string SectionName = "Cache";

    [Required(ErrorMessage = "Redis connection string is required")]
    public string RedisConnectionString { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "Default expiration must be between 1 and 1440 minutes")]
    public int DefaultExpirationMinutes { get; set; } = ApplicationConstants.Defaults.DefaultCacheExpirationMinutes;

    [Range(1, 10080, ErrorMessage = "Schema cache expiration must be between 1 and 10080 minutes (7 days)")]
    public int SchemaCacheExpirationMinutes { get; set; } = 1440; // 24 hours

    [Range(1, 1440, ErrorMessage = "Query cache expiration must be between 1 and 1440 minutes")]
    public int QueryCacheExpirationMinutes { get; set; } = 60;

    public bool EnableDistributedCache { get; set; } = true;
    public bool EnableMemoryCache { get; set; } = true;
    public bool EnableCompression { get; set; } = true;

    [Range(1, 1000, ErrorMessage = "Memory cache size limit must be between 1 and 1000 MB")]
    public int MemoryCacheSizeLimitMB { get; set; } = 100;
}

/// <summary>
/// Background job configuration with validation
/// </summary>
public class BackgroundJobSettings
{
    public const string SectionName = "BackgroundJobs";

    [Range(1, 1440, ErrorMessage = "Schema refresh interval must be between 1 and 1440 minutes")]
    public int SchemaRefreshIntervalMinutes { get; set; } = 60;

    [Range(1, 1440, ErrorMessage = "Cleanup interval must be between 1 and 1440 minutes")]
    public int CleanupIntervalMinutes { get; set; } = 360; // 6 hours

    [Range(1, 365, ErrorMessage = "Retention days must be between 1 and 365")]
    public int RetentionDays { get; set; } = ApplicationConstants.Defaults.DefaultCleanupRetentionDays;

    public bool EnableSchemaRefresh { get; set; } = true;
    public bool EnableCleanupJobs { get; set; } = true;
    public bool EnablePerformanceMonitoring { get; set; } = true;
}

/// <summary>
/// Feature flags configuration
/// </summary>
public class FeatureFlagSettings
{
    public const string SectionName = "FeatureFlags";

    public bool EnableQuerySuggestions { get; set; } = true;
    public bool EnableAutoVisualization { get; set; } = true;
    public bool EnableRealTimeUpdates { get; set; } = true;
    public bool EnableAdvancedAnalytics { get; set; } = false;
    public bool EnableDataExport { get; set; } = true;
    public bool EnableSchemaInference { get; set; } = false;
    public bool EnableAuditLogging { get; set; } = true;
    public bool EnablePerformanceMetrics { get; set; } = true;
    public bool EnableHealthChecks { get; set; } = true;
}

/// <summary>
/// Email service configuration with validation
/// </summary>
public class EmailSettings
{
    public const string SectionName = "Email";

    [Required(ErrorMessage = "SMTP server is required")]
    public string SmtpServer { get; set; } = string.Empty;

    [Range(1, 65535, ErrorMessage = "SMTP port must be between 1 and 65535")]
    public int SmtpPort { get; set; } = 587;

    [Required(ErrorMessage = "SMTP username is required")]
    public string SmtpUsername { get; set; } = string.Empty;

    [Required(ErrorMessage = "SMTP password is required")]
    public string SmtpPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "From email is required")]
    [EmailAddress(ErrorMessage = "From email must be a valid email address")]
    public string FromEmail { get; set; } = string.Empty;

    [Required(ErrorMessage = "From name is required")]
    public string FromName { get; set; } = "BI Reporting Copilot";

    public bool EnableSsl { get; set; } = true;

    [Range(5, 300, ErrorMessage = "Timeout must be between 5 and 300 seconds")]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(1, 10, ErrorMessage = "Max retry attempts must be between 1 and 10")]
    public int MaxRetryAttempts { get; set; } = 3;

    [Range(1, 60, ErrorMessage = "Retry delay must be between 1 and 60 seconds")]
    public int RetryDelaySeconds { get; set; } = 5;
}

/// <summary>
/// SMS service configuration with validation
/// </summary>
public class SmsSettings
{
    public const string SectionName = "SMS";

    [Required(ErrorMessage = "Twilio Account SID is required")]
    public string TwilioAccountSid { get; set; } = string.Empty;

    [Required(ErrorMessage = "Twilio Auth Token is required")]
    public string TwilioAuthToken { get; set; } = string.Empty;

    [Required(ErrorMessage = "Twilio phone number is required")]
    [Phone(ErrorMessage = "Twilio phone number must be a valid phone number")]
    public string TwilioPhoneNumber { get; set; } = string.Empty;

    [Range(1, 10, ErrorMessage = "Max retry attempts must be between 1 and 10")]
    public int MaxRetryAttempts { get; set; } = 3;

    [Range(1, 60, ErrorMessage = "Retry delay must be between 1 and 60 seconds")]
    public int RetryDelaySeconds { get; set; } = 5;

    [Range(5, 300, ErrorMessage = "Timeout must be between 5 and 300 seconds")]
    public int TimeoutSeconds { get; set; } = 30;

    public bool EnableDeliveryReports { get; set; } = true;
    public bool EnableTestMode { get; set; } = false;
}
