using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models.ML;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Adapter that implements IAnomalyDetector using LearningService
/// </summary>
public class AnomalyDetectorAdapter : IAnomalyDetector
{
    private readonly LearningService _learningService;
    private readonly ILogger<AnomalyDetectorAdapter> _logger;

    public AnomalyDetectorAdapter(LearningService learningService)
    {
        _learningService = learningService;
        _logger = new LoggerFactory().CreateLogger<AnomalyDetectorAdapter>();
    }

    public async Task<AnomalyDetectionResult> AnalyzeQueryAsync(string userId, string naturalLanguageQuery, string generatedSQL)
    {
        try
        {
            // Create basic query metrics for analysis
            var metrics = new QueryMetrics
            {
                ExecutionTime = 100, // Default value
                ResultCount = 0,
                IsSuccessful = true
            };

            // Use LearningService to detect anomalies
            var result = await _learningService.DetectQueryAnomaliesAsync(generatedSQL, metrics);

            // Map to expected format
            return new AnomalyDetectionResult
            {
                QueryId = Guid.NewGuid().ToString(),
                UserId = userId,
                Query = naturalLanguageQuery,
                AnomalyScore = result.AnomalyScore,
                RiskLevel = result.IsAnomalous ? RiskLevel.Medium : RiskLevel.Low,
                IsAnomalous = result.IsAnomalous,
                DetectedAnomalies = result.DetectedAnomalies,
                AnalyzedAt = DateTime.UtcNow,
                Timestamp = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query for anomalies");
            return new AnomalyDetectionResult
            {
                QueryId = Guid.NewGuid().ToString(),
                UserId = userId,
                Query = naturalLanguageQuery,
                AnomalyScore = 0.0,
                RiskLevel = RiskLevel.Low,
                IsAnomalous = false,
                DetectedAnomalies = new List<DetectedAnomaly>(),
                AnalyzedAt = DateTime.UtcNow,
                Timestamp = DateTime.UtcNow
            };
        }
    }

    public async Task<BehaviorAnomaly> AnalyzeBehaviorAsync(string userId, QueryMetrics metrics)
    {
        try
        {
            var timeWindow = TimeSpan.FromHours(1); // Default time window
            var anomalies = await _learningService.DetectBehaviorAnomaliesAsync(userId, timeWindow);

            // Return the first anomaly or create a default one
            return anomalies.FirstOrDefault() ?? new BehaviorAnomaly
            {
                UserId = userId,
                AnomalyType = "None",
                Severity = 0.0,
                Description = "No behavioral anomalies detected",
                DetectedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing behavior for user {UserId}", userId);
            return new BehaviorAnomaly
            {
                UserId = userId,
                AnomalyType = "Error",
                Severity = 0.0,
                Description = "Error occurred during behavior analysis",
                DetectedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<Dictionary<string, object>> GetAnomalyStatisticsAsync()
    {
        try
        {
            // Return basic statistics
            return new Dictionary<string, object>
            {
                ["TotalAnalyzed"] = 0,
                ["AnomaliesDetected"] = 0,
                ["AverageAnomalyScore"] = 0.0,
                ["LastAnalysis"] = DateTime.UtcNow,
                ["Status"] = "Active"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting anomaly statistics");
            return new Dictionary<string, object>
            {
                ["Status"] = "Error",
                ["LastError"] = ex.Message
            };
        }
    }
}
