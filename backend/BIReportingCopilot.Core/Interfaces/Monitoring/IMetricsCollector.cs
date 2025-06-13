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
    Task<PerformanceMetrics> GetPerformanceMetricsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<List<MetricDataPoint>> GetMetricHistoryAsync(string metricName, DateTime from, DateTime to, CancellationToken cancellationToken = default);
    Task<Dictionary<string, object>> GetSystemHealthAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Performance metrics summary
/// </summary>
public class PerformanceMetrics
{
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public int TotalQueries { get; set; }
    public double AverageQueryTime { get; set; }
    public double MedianQueryTime { get; set; }
    public double P95QueryTime { get; set; }
    public double P99QueryTime { get; set; }
    public int TotalAIRequests { get; set; }
    public double AverageAIResponseTime { get; set; }
    public int CacheHits { get; set; }
    public int CacheMisses { get; set; }
    public double CacheHitRate => (CacheHits + CacheMisses) > 0 ? (double)CacheHits / (CacheHits + CacheMisses) : 0;
    public int TotalErrors { get; set; }
    public double ErrorRate => TotalQueries > 0 ? (double)TotalErrors / TotalQueries : 0;
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public Dictionary<string, double> ResponseTimesByEndpoint { get; set; } = new();
}

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
