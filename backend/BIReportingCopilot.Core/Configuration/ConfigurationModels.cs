using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Application settings consolidating all application-level configuration
/// Enhanced with comprehensive application settings from legacy ApplicationSettings.cs
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Configuration section name for backward compatibility
    /// </summary>
    public const string SectionName = "ApplicationSettings";

    // Core Application Properties
    [Required(ErrorMessage = "Application name is required")]
    public string ApplicationName { get; set; } = "BI Reporting Copilot";

    [Required(ErrorMessage = "Environment is required")]
    public string Environment { get; set; } = "Development";

    [Required(ErrorMessage = "Version is required")]
    public string Version { get; set; } = "1.0.0";

    // Web Application Settings
    public bool EnableDetailedErrors { get; set; } = true;
    public bool EnableSwagger { get; set; } = true;
    public bool EnableCors { get; set; } = true;

    [Range(1, 300, ErrorMessage = "Request timeout must be between 1 and 300 seconds")]
    public int RequestTimeoutSeconds { get; set; } = 30;

    public List<string> AllowedOrigins { get; set; } = new();
    public List<string> TrustedProxies { get; set; } = new();

    // Logging and Monitoring (consolidated from ApplicationSettings.cs)
    /// <summary>
    /// Whether to enable detailed logging
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable performance monitoring
    /// </summary>
    public bool EnablePerformanceMonitoring { get; set; } = true;

    /// <summary>
    /// Whether to enable health checks
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Whether to enable metrics collection
    /// </summary>
    public bool EnableMetrics { get; set; } = true;

    /// <summary>
    /// Whether to enable distributed tracing
    /// </summary>
    public bool EnableTracing { get; set; } = true;

    // Performance Settings (consolidated from ApplicationSettings.cs)
    /// <summary>
    /// Default timeout for operations in seconds
    /// </summary>
    [Range(1, 3600, ErrorMessage = "Timeout must be between 1 and 3600 seconds")]
    public int DefaultTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Maximum number of concurrent operations
    /// </summary>
    [Range(1, 1000, ErrorMessage = "Max concurrent operations must be between 1 and 1000")]
    public int MaxConcurrentOperations { get; set; } = 100;

    // Caching Settings (consolidated from ApplicationSettings.cs)
    /// <summary>
    /// Whether to enable caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Cache expiration must be between 1 and 1440 minutes")]
    public int DefaultCacheExpirationMinutes { get; set; } = 60;

    // Background Services (consolidated from ApplicationSettings.cs)
    /// <summary>
    /// Whether to enable background jobs
    /// </summary>
    public bool EnableBackgroundJobs { get; set; } = true;

    /// <summary>
    /// Whether to enable real-time notifications
    /// </summary>
    public bool EnableRealTimeNotifications { get; set; } = true;

    // Security and Audit (consolidated from ApplicationSettings.cs)
    /// <summary>
    /// Whether to enable audit logging
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable security monitoring
    /// </summary>
    public bool EnableSecurityMonitoring { get; set; } = true;

    // Feature Management (consolidated from ApplicationSettings.cs)
    /// <summary>
    /// Feature flags
    /// </summary>
    public Dictionary<string, bool> FeatureFlags { get; set; } = new();

    /// <summary>
    /// Custom application settings
    /// </summary>
    public Dictionary<string, string> CustomSettings { get; set; } = new();

    // Obsolete properties removed during cleanup
}

/// <summary>
/// AI configuration consolidating OpenAI and AI-related settings
/// Enhanced with advanced AI features from EnhancedAIConfiguration.cs
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

    // Basic AI Features
    public bool EnableCaching { get; set; } = true;
    public bool EnableLogging { get; set; } = true;
    public bool EnableSemanticAnalysis { get; set; } = true;
    public bool EnableQueryClassification { get; set; } = true;
    public bool EnableContextManagement { get; set; } = true;
    public bool EnableLearning { get; set; } = true;
    public bool EnableAnomalyDetection { get; set; } = true;
    public bool EnablePromptOptimization { get; set; } = true;

    // Enhanced AI Features (consolidated from EnhancedAIConfiguration.cs)
    /// <summary>
    /// Enable conversation context tracking
    /// </summary>
    public bool EnableConversationContext { get; set; } = true;

    /// <summary>
    /// Enable query decomposition for complex queries
    /// </summary>
    public bool EnableQueryDecomposition { get; set; } = true;

    /// <summary>
    /// Enable entity linking to schema elements
    /// </summary>
    public bool EnableEntityLinking { get; set; } = true;

    /// <summary>
    /// Enable multi-dimensional confidence scoring
    /// </summary>
    public bool EnableAdvancedConfidenceScoring { get; set; } = true;

    /// <summary>
    /// Enable AI-powered intent classification
    /// </summary>
    public bool EnableAIIntentClassification { get; set; } = true;

    /// <summary>
    /// Enable schema relationship analysis
    /// </summary>
    public bool EnableSchemaRelationshipAnalysis { get; set; } = true;

    /// <summary>
    /// Enable SQL optimization
    /// </summary>
    public bool EnableSQLOptimization { get; set; } = true;

    // Enhanced AI Configuration Parameters
    /// <summary>
    /// Maximum number of queries to keep in conversation context
    /// </summary>
    [Range(1, 50, ErrorMessage = "Max context queries must be between 1 and 50")]
    public int MaxContextQueries { get; set; } = 10;

    /// <summary>
    /// Time-to-live for conversation context in hours
    /// </summary>
    [Range(1, 24, ErrorMessage = "Context TTL must be between 1 and 24 hours")]
    public int ContextTtlHours { get; set; } = 2;

    /// <summary>
    /// Minimum confidence threshold for query execution
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Confidence threshold must be between 0.0 and 1.0")]
    public double MinimumConfidenceThreshold { get; set; } = 0.6;

    /// <summary>
    /// Minimum similarity threshold for semantic caching
    /// </summary>
    [Range(0.0, 1.0, ErrorMessage = "Similarity threshold must be between 0.0 and 1.0")]
    public double MinimumSimilarityThreshold { get; set; } = 0.85;

    /// <summary>
    /// Maximum complexity score before triggering decomposition
    /// </summary>
    [Range(1, 20, ErrorMessage = "Max complexity must be between 1 and 20")]
    public int MaxComplexityBeforeDecomposition { get; set; } = 7;

    // AI Performance (enhanced)
    [Range(1, 1440, ErrorMessage = "Cache expiration must be between 1 and 1440 minutes")]
    public int CacheExpirationMinutes { get; set; } = 60;

    [Range(1, 100, ErrorMessage = "Max concurrent requests must be between 1 and 100")]
    public int MaxConcurrentRequests { get; set; } = 10;

    /// <summary>
    /// Enable parallel processing for sub-queries
    /// </summary>
    public bool EnableParallelProcessing { get; set; } = true;

    /// <summary>
    /// Maximum number of parallel sub-queries
    /// </summary>
    [Range(1, 10, ErrorMessage = "Max parallel sub-queries must be between 1 and 10")]
    public int MaxParallelSubQueries { get; set; } = 3;

    /// <summary>
    /// Timeout for individual sub-query processing in seconds
    /// </summary>
    [Range(5, 300, ErrorMessage = "Sub-query timeout must be between 5 and 300 seconds")]
    public int SubQueryTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable caching of decomposition results
    /// </summary>
    public bool EnableDecompositionCaching { get; set; } = true;

    /// <summary>
    /// Cache TTL for decomposition results in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Decomposition cache TTL must be between 1 and 1440 minutes")]
    public int DecompositionCacheTtlMinutes { get; set; } = 60;

    // Confidence Weights (consolidated from EnhancedAIConfiguration.cs)
    /// <summary>
    /// Confidence weights for multi-dimensional scoring
    /// </summary>
    public AIConfidenceWeights ConfidenceWeights { get; set; } = new();

    // Enhanced Logging Settings
    /// <summary>
    /// Enable detailed logging for semantic analysis
    /// </summary>
    public bool EnableSemanticAnalysisLogging { get; set; } = true;

    /// <summary>
    /// Enable detailed logging for SQL generation
    /// </summary>
    public bool EnableSQLGenerationLogging { get; set; } = true;

    /// <summary>
    /// Enable detailed logging for confidence scoring
    /// </summary>
    public bool EnableConfidenceLogging { get; set; } = true;

    /// <summary>
    /// Enable performance metrics logging
    /// </summary>
    public bool EnablePerformanceLogging { get; set; } = true;

    /// <summary>
    /// Log conversation context changes
    /// </summary>
    public bool LogConversationContext { get; set; } = false;

    /// <summary>
    /// Log query decomposition details
    /// </summary>
    public bool LogQueryDecomposition { get; set; } = true;
}

/// <summary>
/// Confidence scoring weights for multi-dimensional AI confidence calculation
/// Consolidated from EnhancedAIConfiguration.cs
/// </summary>
public class AIConfidenceWeights
{
    [Range(0.0, 1.0, ErrorMessage = "Model confidence weight must be between 0.0 and 1.0")]
    public float ModelConfidence { get; set; } = 0.3f;

    [Range(0.0, 1.0, ErrorMessage = "Schema alignment weight must be between 0.0 and 1.0")]
    public float SchemaAlignment { get; set; } = 0.25f;

    [Range(0.0, 1.0, ErrorMessage = "Execution validity weight must be between 0.0 and 1.0")]
    public float ExecutionValidity { get; set; } = 0.25f;

    [Range(0.0, 1.0, ErrorMessage = "Historical performance weight must be between 0.0 and 1.0")]
    public float HistoricalPerformance { get; set; } = 0.2f;

    /// <summary>
    /// Validate that all weights sum to approximately 1.0
    /// </summary>
    public bool IsValid()
    {
        var sum = ModelConfidence + SchemaAlignment + ExecutionValidity + HistoricalPerformance;
        return Math.Abs(sum - 1.0f) < 0.01f; // Allow small floating point differences
    }
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

    // Rate Limiting (cleaned up redundant properties)
    public bool EnableRateLimit { get; set; } = true;
    public bool EnableSqlValidation { get; set; } = true;
    public int MaxQueryLength { get; set; } = 10000;
    public int MaxQueryComplexity { get; set; } = 50;
    public Dictionary<string, int>? RateLimits { get; set; }

    // Obsolete properties removed during cleanup

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
    [CustomValidation(typeof(DatabaseConfiguration), nameof(ValidateConnectionString))]
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

    /// <summary>
    /// Custom validation for connection string that allows Azure Key Vault placeholders
    /// </summary>
    public static ValidationResult? ValidateConnectionString(string connectionString, ValidationContext context)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return new ValidationResult("Connection string is required");
        }

        // Allow connection strings with Azure Key Vault placeholders
        if (connectionString.Contains("{azurevault:"))
        {
            return ValidationResult.Success;
        }

        // For regular connection strings, perform basic validation
        if (connectionString.Length < 10)
        {
            return new ValidationResult("Connection string appears to be too short");
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Cache configuration consolidating Redis and memory cache settings
/// Enhanced with comprehensive Redis configuration from legacy RedisConfiguration.cs
/// </summary>
public class CacheConfiguration
{
    // Redis Connection Settings
    public string RedisConnectionString { get; set; } = string.Empty;
    public bool EnableRedis { get; set; } = true;

    // Detailed Redis Settings (consolidated from RedisConfiguration)
    public string RedisHost { get; set; } = "localhost";

    [Range(1, 65535, ErrorMessage = "Redis port must be between 1 and 65535")]
    public int RedisPort { get; set; } = 6379;

    public string? RedisPassword { get; set; }

    [Range(0, 15, ErrorMessage = "Redis database must be between 0 and 15")]
    public int RedisDatabase { get; set; } = 0;

    [Range(1000, 30000, ErrorMessage = "Redis connection timeout must be between 1000 and 30000 milliseconds")]
    public int RedisConnectionTimeoutMs { get; set; } = 5000;

    [Range(1000, 30000, ErrorMessage = "Redis command timeout must be between 1000 and 30000 milliseconds")]
    public int RedisCommandTimeoutMs { get; set; } = 5000;

    public bool RedisUseSsl { get; set; } = false;
    public string? RedisSslHost { get; set; }
    public bool RedisAbortOnConnectFail { get; set; } = false;
    public string RedisKeyPrefix { get; set; } = "bi-copilot";

    [Range(1, 1000, ErrorMessage = "Redis max connections must be between 1 and 1000")]
    public int RedisMaxConnections { get; set; } = 100;

    public bool RedisEnableClustering { get; set; } = false;
    public List<string> RedisClusterEndpoints { get; set; } = new();

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

    // Advanced Cache Features (consolidated from Infrastructure CacheConfiguration)
    [Range(1, 300, ErrorMessage = "Write behind delay must be between 1 and 300 seconds")]
    public int WriteBehindDelaySeconds { get; set; } = 5;

    public bool EnableRefreshAhead { get; set; } = true;

    [Range(1, 60, ErrorMessage = "Refresh ahead threshold must be between 1 and 60 minutes")]
    public int RefreshAheadThresholdMinutes { get; set; } = 5;

    /// <summary>
    /// Get Redis connection string with options (consolidated from RedisConfiguration)
    /// </summary>
    /// <returns>Formatted Redis connection string</returns>
    public string GetRedisConnectionStringWithOptions()
    {
        if (!string.IsNullOrEmpty(RedisConnectionString))
        {
            return RedisConnectionString;
        }

        var connectionStringBuilder = new List<string>();

        // Add host and port
        connectionStringBuilder.Add($"{RedisHost}:{RedisPort}");

        // Add password if provided
        if (!string.IsNullOrEmpty(RedisPassword))
        {
            connectionStringBuilder.Add($"password={RedisPassword}");
        }

        // Add database
        if (RedisDatabase != 0)
        {
            connectionStringBuilder.Add($"defaultDatabase={RedisDatabase}");
        }

        // Add timeouts
        connectionStringBuilder.Add($"connectTimeout={RedisConnectionTimeoutMs}");
        connectionStringBuilder.Add($"syncTimeout={RedisCommandTimeoutMs}");

        // Add SSL settings
        if (RedisUseSsl)
        {
            connectionStringBuilder.Add("ssl=true");
            if (!string.IsNullOrEmpty(RedisSslHost))
            {
                connectionStringBuilder.Add($"sslHost={RedisSslHost}");
            }
        }

        // Add other options
        connectionStringBuilder.Add($"abortConnect={RedisAbortOnConnectFail.ToString().ToLower()}");

        return string.Join(",", connectionStringBuilder);
    }

    // Obsolete properties removed during cleanup
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
