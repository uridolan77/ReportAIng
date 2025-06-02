using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Unified application settings consolidating all configuration models
/// </summary>
public class UnifiedApplicationSettings
{
    [Required(ErrorMessage = "Application name is required")]
    public string ApplicationName { get; set; } = "BI Reporting Copilot";

    [Required(ErrorMessage = "Environment is required")]
    public string Environment { get; set; } = "Development";

    [Required(ErrorMessage = "Version is required")]
    public string Version { get; set; } = "1.0.0";

    public bool EnableDetailedErrors { get; set; } = true;
    public bool EnableSwagger { get; set; } = true;
    public bool EnableCors { get; set; } = true;

    [Range(1, 300, ErrorMessage = "Request timeout must be between 1 and 300 seconds")]
    public int RequestTimeoutSeconds { get; set; } = 30;

    public List<string> AllowedOrigins { get; set; } = new();
    public List<string> TrustedProxies { get; set; } = new();
}

/// <summary>
/// AI configuration consolidating OpenAI and AI-related settings
/// </summary>
public class AIConfiguration
{
    // OpenAI Settings
    public string OpenAIApiKey { get; set; } = string.Empty;

    [Required(ErrorMessage = "OpenAI Model is required")]
    public string OpenAIModel { get; set; } = "gpt-4";

    [Range(0.0, 2.0, ErrorMessage = "Temperature must be between 0.0 and 2.0")]
    public double Temperature { get; set; } = 0.1;

    [Range(1, 4096, ErrorMessage = "Max tokens must be between 1 and 4096")]
    public int MaxTokens { get; set; } = 2000;

    [Range(1, 30, ErrorMessage = "Timeout must be between 1 and 30 seconds")]
    public int TimeoutSeconds { get; set; } = 30;

    [Range(1, 5, ErrorMessage = "Max retries must be between 1 and 5")]
    public int MaxRetries { get; set; } = 3;

    // AI Features
    public bool EnableCaching { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public bool EnableSemanticAnalysis { get; set; } = true;
    public bool EnableQueryClassification { get; set; } = true;
    public bool EnableContextManagement { get; set; } = true;
    public bool EnableLearning { get; set; } = true;
    public bool EnableAnomalyDetection { get; set; } = true;
    public bool EnablePromptOptimization { get; set; } = true;

    // AI Performance
    [Range(1, 1440, ErrorMessage = "Cache expiration must be between 1 and 1440 minutes")]
    public int CacheExpirationMinutes { get; set; } = 60;

    [Range(1, 100, ErrorMessage = "Max concurrent requests must be between 1 and 100")]
    public int MaxConcurrentRequests { get; set; } = 10;
}

/// <summary>
/// AI Service configuration for provider-specific settings
/// </summary>
public class AIServiceConfiguration
{
    public string Provider { get; set; } = "OpenAI";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4";
    public double Temperature { get; set; } = 0.1;
    public int MaxTokens { get; set; } = 2000;
    public int TimeoutSeconds { get; set; } = 30;
    public bool EnableCaching { get; set; } = true;
    public Dictionary<string, object> ProviderSettings { get; set; } = new();

    // Provider selection properties
    public bool PreferAzureOpenAI { get; set; } = false;
    public OpenAIConfiguration OpenAI { get; set; } = new();
    public AzureOpenAIConfiguration AzureOpenAI { get; set; } = new();
}

/// <summary>
/// OpenAI-specific configuration
/// </summary>
public class OpenAIConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4";
    public string Organization { get; set; } = string.Empty;
    public double Temperature { get; set; } = 0.1;
    public int MaxTokens { get; set; } = 2000;
    public int TimeoutSeconds { get; set; } = 30;
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";

    public bool IsConfigured => !string.IsNullOrEmpty(ApiKey);
}

/// <summary>
/// Azure OpenAI-specific configuration
/// </summary>
public class AzureOpenAIConfiguration
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = string.Empty;
    public string DeploymentName { get; set; } = string.Empty;
    public string ApiVersion { get; set; } = "2023-12-01-preview";
    public double Temperature { get; set; } = 0.1;
    public int MaxTokens { get; set; } = 2000;
    public int TimeoutSeconds { get; set; } = 30;

    public bool IsConfigured => !string.IsNullOrEmpty(ApiKey) && !string.IsNullOrEmpty(Endpoint);
}

/// <summary>
/// Security configuration consolidating JWT, authentication, and security settings
/// </summary>
public class SecurityConfiguration
{
    // JWT Settings
    [Required(ErrorMessage = "JWT Secret is required")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters long")]
    public string JwtSecret { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Issuer is required")]
    public string JwtIssuer { get; set; } = string.Empty;

    [Required(ErrorMessage = "JWT Audience is required")]
    public string JwtAudience { get; set; } = string.Empty;

    [Range(1, 1440, ErrorMessage = "Access token expiration must be between 1 and 1440 minutes")]
    public int AccessTokenExpirationMinutes { get; set; } = 60;
    public int JwtAccessTokenExpirationMinutes => AccessTokenExpirationMinutes; // Alias

    [Range(1, 43200, ErrorMessage = "Refresh token expiration must be between 1 and 43200 minutes")]
    public int RefreshTokenExpirationMinutes { get; set; } = 10080;
    public int JwtRefreshTokenExpirationMinutes => RefreshTokenExpirationMinutes; // Alias

    // Authentication Settings
    [Range(1, 10, ErrorMessage = "Max login attempts must be between 1 and 10")]
    public int MaxLoginAttempts { get; set; } = 5;

    [Range(1, 1440, ErrorMessage = "Lockout duration must be between 1 and 1440 minutes")]
    public int LockoutDurationMinutes { get; set; } = 15;

    [Range(8, 128, ErrorMessage = "Min password length must be between 8 and 128")]
    public int MinPasswordLength { get; set; } = 8;

    public bool RequireDigit { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireNonAlphanumeric { get; set; } = true;
    public bool EnableTwoFactorAuthentication { get; set; } = false;

    [Range(1, 60, ErrorMessage = "MFA code expiration must be between 1 and 60 minutes")]
    public int MfaCodeExpirationMinutes { get; set; } = 5;

    // JWT Validation Settings
    public bool ValidateIssuerSigningKey { get; set; } = true;
    public bool ValidateIssuer { get; set; } = true;
    public bool ValidateAudience { get; set; } = true;
    public bool ValidateLifetime { get; set; } = true;

    // Rate Limiting
    public bool EnableRateLimit { get; set; } = true;
    public bool EnableRateLimiting { get; set; } = true; // Alias for EnableRateLimit
    public bool EnableSqlValidation { get; set; } = true;
    public int MaxQueryLength { get; set; } = 10000;
    public int MaxQueryComplexity { get; set; } = 50;
    public Dictionary<string, int>? RateLimits { get; set; }

    // Security Features
    public bool EnableHttpsRedirection { get; set; } = true;
    public bool EnableSecurityHeaders { get; set; } = true;
    public bool EnableAuditLogging { get; set; } = true;
}

/// <summary>
/// Performance configuration consolidating query, caching, and performance settings
/// </summary>
public class PerformanceConfiguration
{
    // Query Performance
    [Range(1, 300, ErrorMessage = "Default timeout must be between 1 and 300 seconds")]
    public int DefaultQueryTimeoutSeconds { get; set; } = 30;

    [Range(1, 1000000, ErrorMessage = "Max rows per query must be between 1 and 1,000,000")]
    public int MaxRowsPerQuery { get; set; } = 10000;

    [Range(1, 100, ErrorMessage = "Max concurrent queries must be between 1 and 100")]
    public int MaxConcurrentQueries { get; set; } = 10;

    // Streaming Performance
    [Range(100, 10000, ErrorMessage = "Streaming batch size must be between 100 and 10000")]
    public int StreamingBatchSize { get; set; } = 1000;

    [Range(1, 1000, ErrorMessage = "Streaming delay must be between 1 and 1000 milliseconds")]
    public int StreamingDelayMs { get; set; } = 10;

    // Background Jobs
    [Range(1, 1440, ErrorMessage = "Schema refresh interval must be between 1 and 1440 minutes")]
    public int SchemaRefreshIntervalMinutes { get; set; } = 60;

    [Range(1, 1440, ErrorMessage = "Cleanup interval must be between 1 and 1440 minutes")]
    public int CleanupIntervalMinutes { get; set; } = 360;

    [Range(1, 365, ErrorMessage = "Retention days must be between 1 and 365")]
    public int RetentionDays { get; set; } = 30;

    // Performance Features
    public bool EnableQueryCaching { get; set; } = true;
    public bool EnableQueryValidation { get; set; } = true;
    public bool EnableQueryLogging { get; set; } = true;
    public bool EnablePerformanceMetrics { get; set; } = true;
    public bool EnableStreaming { get; set; } = true;
    public bool EnableBackgroundJobs { get; set; } = true;
}

/// <summary>
/// Database configuration
/// </summary>
public class DatabaseConfiguration
{
    [Required(ErrorMessage = "Connection string is required")]
    public string ConnectionString { get; set; } = string.Empty;

    [Range(1, 300, ErrorMessage = "Command timeout must be between 1 and 300 seconds")]
    public int CommandTimeoutSeconds { get; set; } = 30;

    [Range(1, 1000, ErrorMessage = "Max pool size must be between 1 and 1000")]
    public int MaxPoolSize { get; set; } = 100;

    public bool EnableRetryOnFailure { get; set; } = true;
    public bool EnableSensitiveDataLogging { get; set; } = false;
    public bool EnableDetailedErrors { get; set; } = true;

    public List<string> AllowedDatabases { get; set; } = new();
    public List<string> BlockedKeywords { get; set; } = new() { "DROP", "DELETE", "UPDATE", "INSERT", "ALTER", "CREATE", "TRUNCATE" };
}

/// <summary>
/// Cache configuration consolidating Redis and memory cache settings
/// </summary>
public class CacheConfiguration
{
    // Redis Settings
    public string RedisConnectionString { get; set; } = string.Empty;
    public bool EnableRedis { get; set; } = true;

    // Cache Expiration
    [Range(1, 1440, ErrorMessage = "Default expiration must be between 1 and 1440 minutes")]
    public int DefaultExpirationMinutes { get; set; } = 30;

    [Range(1, 10080, ErrorMessage = "Schema cache expiration must be between 1 and 10080 minutes")]
    public int SchemaCacheExpirationMinutes { get; set; } = 1440;

    [Range(1, 1440, ErrorMessage = "Query cache expiration must be between 1 and 1440 minutes")]
    public int QueryCacheExpirationMinutes { get; set; } = 60;

    // Cache Features
    public bool EnableDistributedCache { get; set; } = true;
    public bool EnableMemoryCache { get; set; } = true;
    public bool EnableCompression { get; set; } = true;

    [Range(1, 1000, ErrorMessage = "Memory cache size limit must be between 1 and 1000 MB")]
    public int MemoryCacheSizeLimitMB { get; set; } = 100;
}

/// <summary>
/// Monitoring configuration consolidating health checks and metrics
/// </summary>
public class MonitoringConfiguration
{
    // Health Checks
    public bool EnableHealthChecks { get; set; } = true;
    public bool EnableDetailedHealthChecks { get; set; } = true;
    public bool EnableStartupValidation { get; set; } = true;

    [Range(1, 300, ErrorMessage = "Health check timeout must be between 1 and 300 seconds")]
    public int HealthCheckTimeoutSeconds { get; set; } = 30;

    // Metrics
    public bool EnableMetrics { get; set; } = true;
    public bool EnablePerformanceCounters { get; set; } = true;
    public bool EnableCustomMetrics { get; set; } = true;

    // Logging
    public bool EnableStructuredLogging { get; set; } = true;
    public bool EnableRequestLogging { get; set; } = true;
    public bool EnableErrorLogging { get; set; } = true;

    [Required(ErrorMessage = "Log level is required")]
    public string LogLevel { get; set; } = "Information";
}

/// <summary>
/// Feature configuration consolidating all feature flags
/// </summary>
public class FeatureConfiguration
{
    // Core Features
    public bool EnableQuerySuggestions { get; set; } = true;
    public bool EnableAutoVisualization { get; set; } = true;
    public bool EnableRealTimeUpdates { get; set; } = true;
    public bool EnableDataExport { get; set; } = true;

    // Advanced Features
    public bool EnableAdvancedAnalytics { get; set; } = false;
    public bool EnableSchemaInference { get; set; } = false;
    public bool EnableMachineLearning { get; set; } = true;
    public bool EnableAIOptimization { get; set; } = true;

    // Experimental Features
    public bool EnableExperimentalFeatures { get; set; } = false;
    public bool EnableBetaFeatures { get; set; } = false;
    public bool EnableDebugFeatures { get; set; } = false;
}

/// <summary>
/// Notification configuration for unified notification management
/// </summary>
public class NotificationConfiguration
{
    public bool EnableEmail { get; set; } = true;
    public bool EnableSms { get; set; } = true;
    public bool EnableRealTime { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public int SimulatedDelay { get; set; } = 100;
}
