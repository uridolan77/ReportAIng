using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Configuration;

/// <summary>
/// Application-wide settings configuration
/// </summary>
public class ApplicationSettings
{
    /// <summary>
    /// Configuration section name
    /// </summary>
    public const string SectionName = "ApplicationSettings";

    /// <summary>
    /// Application name
    /// </summary>
    public string ApplicationName { get; set; } = "BI Reporting Copilot";

    /// <summary>
    /// Application version
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// Application environment
    /// </summary>
    public string Environment { get; set; } = "Development";

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

    /// <summary>
    /// Whether to enable caching
    /// </summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>
    /// Default cache expiration in minutes
    /// </summary>
    [Range(1, 1440, ErrorMessage = "Cache expiration must be between 1 and 1440 minutes")]
    public int DefaultCacheExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Whether to enable background jobs
    /// </summary>
    public bool EnableBackgroundJobs { get; set; } = true;

    /// <summary>
    /// Whether to enable real-time notifications
    /// </summary>
    public bool EnableRealTimeNotifications { get; set; } = true;

    /// <summary>
    /// Whether to enable audit logging
    /// </summary>
    public bool EnableAuditLogging { get; set; } = true;

    /// <summary>
    /// Whether to enable security monitoring
    /// </summary>
    public bool EnableSecurityMonitoring { get; set; } = true;

    /// <summary>
    /// Feature flags
    /// </summary>
    public Dictionary<string, bool> FeatureFlags { get; set; } = new();

    /// <summary>
    /// Custom application settings
    /// </summary>
    public Dictionary<string, string> CustomSettings { get; set; } = new();
}
