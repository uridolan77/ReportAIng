namespace BIReportingCopilot.Infrastructure.AI.Core.Models;

/// <summary>
/// AI service performance metrics
/// </summary>
public class AIServiceMetrics
{
    /// <summary>
    /// Total number of requests processed
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Number of successful requests
    /// </summary>
    public long SuccessfulRequests { get; set; }

    /// <summary>
    /// Number of failed requests
    /// </summary>
    public long FailedRequests { get; set; }

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTimeMs { get; set; }

    /// <summary>
    /// Total tokens consumed
    /// </summary>
    public long TotalTokensConsumed { get; set; }

    /// <summary>
    /// Total cost in USD
    /// </summary>
    public decimal TotalCostUsd { get; set; }

    /// <summary>
    /// Cache hit rate (0.0 to 1.0)
    /// </summary>
    public double CacheHitRate { get; set; }

    /// <summary>
    /// Success rate (0.0 to 1.0)
    /// </summary>
    public double SuccessRate => TotalRequests > 0 ? (double)SuccessfulRequests / TotalRequests : 0.0;

    /// <summary>
    /// Error rate (0.0 to 1.0)
    /// </summary>
    public double ErrorRate => TotalRequests > 0 ? (double)FailedRequests / TotalRequests : 0.0;

    /// <summary>
    /// Last updated timestamp
    /// </summary>
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Provider-specific metrics
    /// </summary>
    public Dictionary<string, object> ProviderMetrics { get; set; } = new();

    /// <summary>
    /// Performance trends over time
    /// </summary>
    public List<MetricDataPoint> PerformanceTrends { get; set; } = new();

    // Properties expected by Infrastructure services
    /// <summary>
    /// Whether the AI service is available
    /// </summary>
    public bool IsAvailable { get; set; } = true;

    /// <summary>
    /// Average response time in milliseconds
    /// </summary>
    public double AverageResponseTime { get; set; }

    /// <summary>
    /// Last health check timestamp
    /// </summary>
    public DateTime LastHealthCheck { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Provider name
    /// </summary>
    public string ProviderName { get; set; } = string.Empty;

    /// <summary>
    /// Provider version
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// Error message if service is unavailable
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// Metric data point for trend analysis
/// </summary>
public class MetricDataPoint
{
    /// <summary>
    /// Timestamp of the metric
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Metric value
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Metric name/type
    /// </summary>
    public string MetricName { get; set; } = string.Empty;

    /// <summary>
    /// Additional metadata
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
}
