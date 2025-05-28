using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Main application settings configuration
/// </summary>
public class ApplicationSettings
{
    public const string SectionName = "Application";

    /// <summary>
    /// Application name and version information
    /// </summary>
    public ApplicationInfo Info { get; set; } = new();

    /// <summary>
    /// Feature flags for enabling/disabling functionality
    /// </summary>
    public FeatureFlags Features { get; set; } = new();

    /// <summary>
    /// Performance and resource limits
    /// </summary>
    public PerformanceLimits Limits { get; set; } = new();

    /// <summary>
    /// Security-related settings
    /// </summary>
    public SecuritySettings Security { get; set; } = new();

    /// <summary>
    /// AI/ML service configuration
    /// </summary>
    public AISettings AI { get; set; } = new();

    /// <summary>
    /// Caching configuration
    /// </summary>
    public CachingSettings Caching { get; set; } = new();

    /// <summary>
    /// Database configuration
    /// </summary>
    public DatabaseSettings Database { get; set; } = new();

    /// <summary>
    /// Monitoring and observability settings
    /// </summary>
    public MonitoringSettings Monitoring { get; set; } = new();

    /// <summary>
    /// Event bus configuration
    /// </summary>
    public EventBusSettings EventBus { get; set; } = new();
}

/// <summary>
/// Application information
/// </summary>
public class ApplicationInfo
{
    [Required]
    public string Name { get; set; } = "BI Reporting Copilot";

    [Required]
    public string Version { get; set; } = "1.0.0";

    public string Environment { get; set; } = "Development";
    public string Description { get; set; } = "AI-powered Business Intelligence reporting and query generation";
    public string SupportEmail { get; set; } = "support@bireportingcopilot.com";
    public string DocumentationUrl { get; set; } = "https://docs.bireportingcopilot.com";
}

/// <summary>
/// Feature flags for enabling/disabling functionality
/// </summary>
public class FeatureFlags
{
    /// <summary>
    /// Enable AI-powered query generation
    /// </summary>
    public bool EnableAIQueryGeneration { get; set; } = true;

    /// <summary>
    /// Enable semantic caching
    /// </summary>
    public bool EnableSemanticCaching { get; set; } = true;

    /// <summary>
    /// Enable anomaly detection
    /// </summary>
    public bool EnableAnomalyDetection { get; set; } = true;

    /// <summary>
    /// Enable adaptive learning from user feedback
    /// </summary>
    public bool EnableAdaptiveLearning { get; set; } = true;

    /// <summary>
    /// Enable distributed caching with Redis
    /// </summary>
    public bool EnableDistributedCaching { get; set; } = false;

    /// <summary>
    /// Enable event-driven architecture
    /// </summary>
    public bool EnableEventDrivenArchitecture { get; set; } = true;

    /// <summary>
    /// Enable sharded schema support
    /// </summary>
    public bool EnableShardedSchema { get; set; } = false;

    /// <summary>
    /// Enable advanced SQL validation
    /// </summary>
    public bool EnableAdvancedSQLValidation { get; set; } = true;

    /// <summary>
    /// Enable query explanation generation
    /// </summary>
    public bool EnableQueryExplanation { get; set; } = true;

    /// <summary>
    /// Enable real-time query streaming
    /// </summary>
    public bool EnableQueryStreaming { get; set; } = true;
}

/// <summary>
/// Performance and resource limits
/// </summary>
public class PerformanceLimits
{
    /// <summary>
    /// Maximum number of rows to return in a single query
    /// </summary>
    [Range(1, 100000)]
    public int MaxQueryRows { get; set; } = 10000;

    /// <summary>
    /// Maximum query execution timeout in seconds
    /// </summary>
    [Range(1, 600)]
    public int MaxQueryTimeoutSeconds { get; set; } = 300;

    /// <summary>
    /// Maximum length of natural language queries
    /// </summary>
    [Range(1, 10000)]
    public int MaxQueryLength { get; set; } = 2000;

    /// <summary>
    /// Maximum number of concurrent queries per user
    /// </summary>
    [Range(1, 100)]
    public int MaxConcurrentQueriesPerUser { get; set; } = 5;

    /// <summary>
    /// Maximum file upload size in MB
    /// </summary>
    [Range(1, 1000)]
    public int MaxFileUploadSizeMB { get; set; } = 100;

    /// <summary>
    /// Maximum number of schema tables to process
    /// </summary>
    [Range(1, 10000)]
    public int MaxSchemaTables { get; set; } = 1000;

    /// <summary>
    /// Maximum cache entry size in KB
    /// </summary>
    [Range(1, 10240)]
    public int MaxCacheEntrySizeKB { get; set; } = 1024;
}

/// <summary>
/// Security-related settings
/// </summary>
public class SecuritySettings
{
    /// <summary>
    /// Minimum confidence score required for query execution
    /// </summary>
    [Range(0.0, 1.0)]
    public double MinimumConfidenceScore { get; set; } = 0.7;

    /// <summary>
    /// Enable SQL injection detection
    /// </summary>
    public bool EnableSQLInjectionDetection { get; set; } = true;

    /// <summary>
    /// Enable rate limiting
    /// </summary>
    public bool EnableRateLimiting { get; set; } = true;

    /// <summary>
    /// Maximum requests per minute per user
    /// </summary>
    [Range(1, 1000)]
    public int MaxRequestsPerMinute { get; set; } = 60;

    /// <summary>
    /// Maximum requests per hour per user
    /// </summary>
    [Range(1, 10000)]
    public int MaxRequestsPerHour { get; set; } = 1000;

    /// <summary>
    /// Enable audit logging
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Allowed database schemas (empty means all allowed)
    /// </summary>
    public List<string> AllowedSchemas { get; set; } = new();

    /// <summary>
    /// Blocked table patterns (regex patterns)
    /// </summary>
    public List<string> BlockedTablePatterns { get; set; } = new()
    {
        @"^sys\w*",
        @"^information_schema\w*",
        @"^master\w*",
        @"^msdb\w*"
    };
}

/// <summary>
/// AI/ML service configuration
/// </summary>
public class AISettings
{
    /// <summary>
    /// AI provider (OpenAI, AzureOpenAI, Mock)
    /// </summary>
    [Required]
    public string Provider { get; set; } = "OpenAI";

    /// <summary>
    /// Default model to use for query generation
    /// </summary>
    [Required]
    public string DefaultModel { get; set; } = "gpt-3.5-turbo";

    /// <summary>
    /// Maximum tokens for AI responses
    /// </summary>
    [Range(100, 8000)]
    public int MaxTokens { get; set; } = 2000;

    /// <summary>
    /// Temperature for AI responses (0.0 to 1.0)
    /// </summary>
    [Range(0.0, 1.0)]
    public double Temperature { get; set; } = 0.1;

    /// <summary>
    /// Maximum retries for AI service calls
    /// </summary>
    [Range(0, 10)]
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Timeout for AI service calls in seconds
    /// </summary>
    [Range(1, 300)]
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Enable prompt optimization
    /// </summary>
    public bool EnablePromptOptimization { get; set; } = true;

    /// <summary>
    /// Enable confidence scoring
    /// </summary>
    public bool EnableConfidenceScoring { get; set; } = true;

    /// <summary>
    /// Minimum similarity threshold for semantic caching
    /// </summary>
    [Range(0.0, 1.0)]
    public double SemanticSimilarityThreshold { get; set; } = 0.75;
}

/// <summary>
/// Caching configuration
/// </summary>
public class CachingSettings
{
    /// <summary>
    /// Default cache expiry in minutes
    /// </summary>
    [Range(1, 10080)] // 1 minute to 1 week
    public int DefaultExpiryMinutes { get; set; } = 60;

    /// <summary>
    /// Schema cache expiry in minutes
    /// </summary>
    [Range(1, 10080)]
    public int SchemaExpiryMinutes { get; set; } = 30;

    /// <summary>
    /// Query result cache expiry in minutes
    /// </summary>
    [Range(1, 1440)] // 1 minute to 1 day
    public int QueryResultExpiryMinutes { get; set; } = 15;

    /// <summary>
    /// Semantic cache expiry in hours
    /// </summary>
    [Range(1, 168)] // 1 hour to 1 week
    public int SemanticCacheExpiryHours { get; set; } = 24;

    /// <summary>
    /// Maximum number of cache entries
    /// </summary>
    [Range(100, 1000000)]
    public int MaxCacheEntries { get; set; } = 10000;

    /// <summary>
    /// Cache key prefix
    /// </summary>
    public string KeyPrefix { get; set; } = "bi-reporting";
}

/// <summary>
/// Database configuration
/// </summary>
public class DatabaseSettings
{
    /// <summary>
    /// Command timeout in seconds
    /// </summary>
    [Range(1, 600)]
    public int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Connection pool size
    /// </summary>
    [Range(1, 1000)]
    public int MaxPoolSize { get; set; } = 100;

    /// <summary>
    /// Enable connection pooling
    /// </summary>
    public bool EnableConnectionPooling { get; set; } = true;

    /// <summary>
    /// Enable query logging
    /// </summary>
    public bool EnableQueryLogging { get; set; } = false;

    /// <summary>
    /// Enable sensitive data logging (development only)
    /// </summary>
    public bool EnableSensitiveDataLogging { get; set; } = false;

    /// <summary>
    /// Retry policy settings
    /// </summary>
    public RetryPolicySettings RetryPolicy { get; set; } = new();
}

/// <summary>
/// Retry policy settings
/// </summary>
public class RetryPolicySettings
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Base delay between retries in milliseconds
    /// </summary>
    [Range(100, 10000)]
    public int BaseDelayMs { get; set; } = 1000;

    /// <summary>
    /// Maximum delay between retries in milliseconds
    /// </summary>
    [Range(1000, 60000)]
    public int MaxDelayMs { get; set; } = 30000;

    /// <summary>
    /// Enable exponential backoff
    /// </summary>
    public bool UseExponentialBackoff { get; set; } = true;
}

/// <summary>
/// Monitoring and observability settings
/// </summary>
public class MonitoringSettings
{
    /// <summary>
    /// Enable detailed performance metrics
    /// </summary>
    public bool EnableDetailedMetrics { get; set; } = true;

    /// <summary>
    /// Enable distributed tracing
    /// </summary>
    public bool EnableDistributedTracing { get; set; } = true;

    /// <summary>
    /// Enable health checks
    /// </summary>
    public bool EnableHealthChecks { get; set; } = true;

    /// <summary>
    /// Metrics collection interval in seconds
    /// </summary>
    [Range(1, 3600)]
    public int MetricsIntervalSeconds { get; set; } = 60;

    /// <summary>
    /// Log level for application logs
    /// </summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>
    /// Enable structured logging
    /// </summary>
    public bool EnableStructuredLogging { get; set; } = true;
}

/// <summary>
/// Event bus configuration
/// </summary>
public class EventBusSettings
{
    /// <summary>
    /// Event bus provider (InMemory, RabbitMQ, ServiceBus)
    /// </summary>
    public string Provider { get; set; } = "InMemory";

    /// <summary>
    /// Maximum concurrent event handlers
    /// </summary>
    [Range(1, 100)]
    public int MaxConcurrentHandlers { get; set; } = 10;

    /// <summary>
    /// Enable event retries
    /// </summary>
    public bool EnableRetries { get; set; } = true;

    /// <summary>
    /// Maximum retry attempts for failed events
    /// </summary>
    [Range(0, 10)]
    public int MaxRetryAttempts { get; set; } = 3;

    /// <summary>
    /// Retry delay in milliseconds
    /// </summary>
    [Range(100, 60000)]
    public int RetryDelayMs { get; set; } = 5000;

    /// <summary>
    /// Enable dead letter queue
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;

    /// <summary>
    /// Message timeout in minutes
    /// </summary>
    [Range(1, 60)]
    public int MessageTimeoutMinutes { get; set; } = 5;
}
