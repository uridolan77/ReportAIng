using System.Diagnostics.Metrics;

namespace BIReportingCopilot.Core.Interfaces;



/// <summary>
/// Interface for collecting and recording application metrics
/// </summary>
public interface IMetricsCollector
{
    /// <summary>
    /// Record a query execution metric
    /// </summary>
    void RecordQueryExecution(string queryType, long executionTimeMs, bool isSuccessful, int rowCount);

    /// <summary>
    /// Record a histogram value
    /// </summary>
    void RecordHistogram(string name, double value);

    /// <summary>
    /// Increment a counter
    /// </summary>
    void IncrementCounter(string name, System.Diagnostics.TagList? tags = null);

    /// <summary>
    /// Set a gauge value
    /// </summary>
    void SetGaugeValue(string name, double value);

    /// <summary>
    /// Record execution time
    /// </summary>
    void RecordExecutionTime(string operationName, TimeSpan duration);

    /// <summary>
    /// Record cache hit/miss
    /// </summary>
    void RecordCacheOperation(string cacheType, bool isHit);

    /// <summary>
    /// Record error occurrence
    /// </summary>
    void RecordError(string errorType, string? details = null);
    void RecordError(string errorType, string? details, Exception? exception);

    /// <summary>
    /// Record user activity
    /// </summary>
    void RecordUserActivity(string userId, string activityType);

    /// <summary>
    /// Get current metrics snapshot
    /// </summary>
    Task<Dictionary<string, object>> GetMetricsSnapshotAsync();



    /// <summary>
    /// Record histogram value
    /// </summary>
    void RecordHistogram(string name, double value, System.Diagnostics.TagList? tags = null);

    /// <summary>
    /// Record a value
    /// </summary>
    void RecordValue(string name, double value);
}


