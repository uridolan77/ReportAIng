using BIReportingCopilot.Core.Models.ML;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for anomaly detection services
/// </summary>
public interface IAnomalyDetector
{
    /// <summary>
    /// Analyze a query for potential anomalies
    /// </summary>
    Task<AnomalyDetectionResult> AnalyzeQueryAsync(string userId, string naturalLanguageQuery, string generatedSQL);

    /// <summary>
    /// Analyze user behavior for anomalies
    /// </summary>
    Task<BehaviorAnomaly> AnalyzeBehaviorAsync(string userId, QueryMetrics metrics);

    /// <summary>
    /// Get anomaly detection statistics
    /// </summary>
    Task<Dictionary<string, object>> GetAnomalyStatisticsAsync();
}
