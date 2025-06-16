using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Monitoring;

/// <summary>
/// Metrics collection service interface for performance monitoring
/// </summary>
public interface IMetricsCollector
{
    Task RecordQueryExecutionAsync(string queryId, TimeSpan executionTime, bool success, CancellationToken cancellationToken = default);
    Task RecordAIRequestAsync(string requestId, string provider, TimeSpan responseTime, int tokenCount, bool success, CancellationToken cancellationToken = default);
    Task RecordCacheHitAsync(string cacheKey, string cacheType, CancellationToken cancellationToken = default);
    Task RecordCacheMissAsync(string cacheKey, string cacheType, CancellationToken cancellationToken = default);
    Task RecordUserActionAsync(string userId, string action, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default);
    Task RecordErrorAsync(string errorId, string errorType, string message, string? stackTrace = null, CancellationToken cancellationToken = default);
    Task<Models.PerformanceMetrics> GetPerformanceMetricsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<List<MetricDataPoint>> GetMetricHistoryAsync(string metricName, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetSystemHealthAsync(CancellationToken cancellationToken = default);

    // Additional methods expected by Infrastructure services
    /// <summary>
    /// Increment a counter
    /// </summary>
    void IncrementCounter(string name, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Record a histogram value synchronously
    /// </summary>
    void RecordHistogram(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Set a gauge value
    /// </summary>
    void SetGaugeValue(string name, double value, Dictionary<string, string>? tags = null);

    /// <summary>
    /// Record query execution synchronously
    /// </summary>
    void RecordQueryExecution(string queryType, TimeSpan duration, bool success);

    /// <summary>
    /// Record value (for compatibility)
    /// </summary>
    void RecordValue(string name, double value);


}

// PerformanceMetrics moved to Core.Models.PerformanceModels.cs to eliminate duplicates
// Use: BIReportingCopilot.Core.Models.PerformanceMetrics instead

/// <summary>
/// Individual metric data point
/// </summary>
public class MetricDataPoint
{
    public DateTime Timestamp { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public double Value { get; set; }
    public Dictionary<string, object> Tags { get; set; } = new();
    public string Unit { get; set; } = string.Empty;
}
