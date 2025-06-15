using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Models;
using MLQueryMetrics = BIReportingCopilot.Core.Models.QueryMetrics;
using MLUserFeedback = BIReportingCopilot.Core.Models.UserFeedback;
using BIReportingCopilot.Infrastructure.Data;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI.Core;

/// <summary>
/// Unified learning service combining ML anomaly detection and feedback learning
/// Consolidates functionality from MLAnomalyDetector and FeedbackLearningEngine
/// </summary>
public class LearningService
{
    private readonly BICopilotContext _context;
    private readonly ICacheService _cacheService;
    private readonly ILogger<LearningService> _logger;
    private readonly Dictionary<string, AnomalyModel> _anomalyModels;
    private readonly Dictionary<string, LearningModel> _learningModels;

    public LearningService(
        BICopilotContext context,
        ICacheService cacheService,
        ILogger<LearningService> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
        _anomalyModels = new Dictionary<string, AnomalyModel>();
        _learningModels = new Dictionary<string, LearningModel>();
    }

    #region Anomaly Detection Methods

    /// <summary>
    /// Detect anomalies in query patterns
    /// </summary>
    public async Task<AnomalyDetectionResult> DetectQueryAnomaliesAsync(string query, MLQueryMetrics metrics)
    {
        try
        {
            _logger.LogDebug("Detecting anomalies for query: {Query}", query);

            var result = new AnomalyDetectionResult
            {
                Query = query,
                IsAnomalous = false,
                AnomalyScore = 0.0,
                DetectedAnomalies = new List<DetectedAnomaly>(),
                AnalyzedAt = DateTime.UtcNow
            };

            // Check execution time anomalies
            var executionTimeAnomaly = await DetectExecutionTimeAnomalyAsync(TimeSpan.FromMilliseconds(metrics.ExecutionTimeMs));
            if (executionTimeAnomaly != null)
            {
                result.DetectedAnomalies.Add(executionTimeAnomaly);
                result.IsAnomalous = true;
            }

            // Check result size anomalies
            var resultSizeAnomaly = await DetectResultSizeAnomalyAsync(metrics.RowCount);
            if (resultSizeAnomaly != null)
            {
                result.DetectedAnomalies.Add(resultSizeAnomaly);
                result.IsAnomalous = true;
            }

            // Check query complexity anomalies
            var complexityAnomaly = await DetectComplexityAnomalyAsync("medium"); // Default complexity
            if (complexityAnomaly != null)
            {
                result.DetectedAnomalies.Add(complexityAnomaly);
                result.IsAnomalous = true;
            }

            // Check pattern anomalies
            var patternAnomaly = await DetectPatternAnomalyAsync(query);
            if (patternAnomaly != null)
            {
                result.DetectedAnomalies.Add(patternAnomaly);
                result.IsAnomalous = true;
            }

            // Calculate overall anomaly score
            result.AnomalyScore = CalculateOverallAnomalyScore(result.DetectedAnomalies.ToArray());

            _logger.LogDebug("Anomaly detection completed. Score: {Score}, Anomalous: {IsAnomalous}",
                result.AnomalyScore, result.IsAnomalous);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting query anomalies");
            return new AnomalyDetectionResult
            {
                Query = query,
                IsAnomalous = false,
                AnomalyScore = 0.0,
                DetectedAnomalies = new List<DetectedAnomaly>(),
                AnalyzedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Detect anomalies in user behavior patterns
    /// </summary>
    public async Task<List<BehaviorAnomaly>> DetectBehaviorAnomaliesAsync(string userId, TimeSpan timeWindow)
    {
        try
        {
            var anomalies = new List<BehaviorAnomaly>();
            var endTime = DateTime.UtcNow;
            var startTime = endTime.Subtract(timeWindow);

            // Get user's recent activity
            var recentActivity = await GetUserActivityAsync(userId);

            // Check for unusual query frequency
            var frequencyAnomalyScore = DetectQueryFrequencyAnomaly(recentActivity);
            if (frequencyAnomalyScore > 0.5)
            {
                anomalies.Add(new BehaviorAnomaly
                {
                    UserId = userId,
                    AnomalyType = "QueryFrequency",
                    Severity = frequencyAnomalyScore,
                    Description = "Unusual query frequency detected",
                    DetectedAt = DateTime.UtcNow
                });
            }

            // Check for unusual query patterns
            var patternAnomalies = await DetectUnusualQueryPatternsAsync(recentActivity);
            if (patternAnomalies.Any())
            {
                anomalies.Add(new BehaviorAnomaly
                {
                    UserId = userId,
                    AnomalyType = "QueryPattern",
                    Severity = 0.7,
                    Description = string.Join(", ", patternAnomalies),
                    DetectedAt = DateTime.UtcNow
                });
            }

            // Check for unusual error rates
            var errorAnomalyScore = DetectErrorRateAnomaly(recentActivity);
            if (errorAnomalyScore > 0.3)
            {
                anomalies.Add(new BehaviorAnomaly
                {
                    UserId = userId,
                    AnomalyType = "ErrorRate",
                    Severity = errorAnomalyScore,
                    Description = "Unusual error rate detected",
                    DetectedAt = DateTime.UtcNow
                });
            }

            return anomalies;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error detecting behavior anomalies for user {UserId}", userId);
            return new List<BehaviorAnomaly>();
        }
    }

    #endregion

    #region Feedback Learning Methods

    /// <summary>
    /// Process user feedback and update learning models
    /// </summary>
    public async Task ProcessFeedbackAsync(MLUserFeedback feedback)
    {
        try
        {
            _logger.LogDebug("Processing feedback for query: {QueryId}", feedback.QueryId);

            // Store feedback in database using unified model
            var unifiedFeedbackEntry = new UnifiedAIFeedbackEntry
            {
                // Id will be auto-generated by database
                QueryId = feedback.QueryId,
                UserId = feedback.UserId,
                OriginalQuery = feedback.QueryId, // Use QueryId as placeholder for OriginalQuery
                Rating = feedback.Rating,
                Comments = feedback.Comments,
                Category = feedback.Category,
                FeedbackType = "User Feedback",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = feedback.UserId,
                UpdatedBy = feedback.UserId,
                CreatedDate = DateTime.UtcNow,
                LastUpdated = DateTime.UtcNow,
                IsActive = true,
                Metadata = JsonSerializer.Serialize(feedback.Metadata ?? new Dictionary<string, object>())
            };

            _context.AIFeedbackEntries.Add(unifiedFeedbackEntry);
            await _context.SaveChangesAsync();

            // Update learning models based on feedback
            await UpdateLearningModelsAsync(feedback);

            // Update query success patterns
            await UpdateQuerySuccessPatternsAsync(feedback);

            // Update optimization insights
            await UpdateOptimizationInsightsAsync(feedback);

            _logger.LogDebug("Feedback processed successfully for query: {QueryId}", feedback.QueryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing feedback for query {QueryId}", feedback.QueryId);
        }
    }

    /// <summary>
    /// Generate learning insights based on accumulated feedback
    /// </summary>
    public async Task<LearningInsights> GenerateLearningInsightsAsync(string? userId = null, string? category = null)
    {
        try
        {
            _logger.LogDebug("Generating learning insights for user: {UserId}, category: {Category}", userId, category);

            var insights = new LearningInsights
            {
                GeneratedAt = DateTime.UtcNow,
                SuccessfulPatterns = await GetSuccessfulPatternsAsync(userId),
                CommonMistakes = await GetCommonMistakesAsync(userId),
                OptimizationSuggestions = await GetOptimizationSuggestionsAsync(userId),
                UserPreferences = await GetUserPreferencesAsync(userId),
                PerformanceInsights = await GetPerformanceInsightsAsync(userId)
            };

            // Cache insights for quick access
            var cacheKey = $"learning_insights:{userId ?? "global"}:{category ?? "all"}";
            await _cacheService.SetAsync(cacheKey, insights, TimeSpan.FromHours(2));

            _logger.LogDebug("Learning insights generated with {SuccessfulPatterns} patterns and {Mistakes} mistakes",
                insights.SuccessfulPatterns.Count, insights.CommonMistakes.Count);

            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating learning insights");
            return new LearningInsights
            {
                GeneratedAt = DateTime.UtcNow,
                SuccessfulPatterns = new List<string>(),
                CommonMistakes = new List<string>(),
                OptimizationSuggestions = new List<string>(),
                UserPreferences = new Dictionary<string, object>(),
                PerformanceInsights = new Dictionary<string, double>()
            };
        }
    }

    /// <summary>
    /// Get learning insights for AI service
    /// </summary>
    public async Task<LearningInsights> GetLearningInsightsAsync(string? userId = null)
    {
        return await GenerateLearningInsightsAsync(userId);
    }

    #endregion

    #region Private Helper Methods

    private async Task<DetectedAnomaly?> DetectExecutionTimeAnomalyAsync(TimeSpan executionTime)
    {
        // Simple threshold-based detection
        if (executionTime.TotalSeconds > 30)
        {
            return new DetectedAnomaly
            {
                Type = "ExecutionTime",
                Severity = AnomalySeverity.High,
                Description = $"Query execution time ({executionTime.TotalSeconds:F2}s) exceeds threshold",
                DetectedAt = DateTime.UtcNow,
                Score = 0.8
            };
        }
        return null;
    }

    private async Task<DetectedAnomaly?> DetectResultSizeAnomalyAsync(int resultSize)
    {
        // Simple threshold-based detection
        if (resultSize > 100000)
        {
            return new DetectedAnomaly
            {
                Type = "ResultSize",
                Severity = AnomalySeverity.Medium,
                Description = $"Result size ({resultSize} rows) is unusually large",
                DetectedAt = DateTime.UtcNow,
                Score = 0.6
            };
        }
        return null;
    }

    private async Task<DetectedAnomaly?> DetectComplexityAnomalyAsync(string queryComplexity)
    {
        // Simple complexity check
        if (queryComplexity?.ToLower() == "high")
        {
            return new DetectedAnomaly
            {
                Type = "Complexity",
                Severity = AnomalySeverity.Medium,
                Description = "Query complexity is high",
                DetectedAt = DateTime.UtcNow,
                Score = 0.5
            };
        }
        return null;
    }

    private async Task<DetectedAnomaly?> DetectPatternAnomalyAsync(string queryPattern)
    {
        // Simple pattern check
        if (string.IsNullOrEmpty(queryPattern))
        {
            return new DetectedAnomaly
            {
                Type = "Pattern",
                Severity = AnomalySeverity.Low,
                Description = "No recognizable query pattern detected",
                DetectedAt = DateTime.UtcNow,
                Score = 0.3
            };
        }
        return null;
    }

    private double CalculateOverallAnomalyScore(params DetectedAnomaly?[] anomalies)
    {
        var validAnomalies = anomalies.Where(a => a != null).ToList();
        if (!validAnomalies.Any()) return 0.0;

        var score = validAnomalies.Count * 0.25; // Base score per anomaly
        return Math.Min(score, 1.0); // Cap at 1.0
    }

    private async Task<List<UserActivity>> GetUserActivityAsync(string userId)
    {
        // Return mock data for now
        return new List<UserActivity>
        {
            new UserActivity
            {
                UserId = userId,
                Action = "Query",
                Timestamp = DateTime.UtcNow.AddHours(-1),
                Details = new Dictionary<string, object> { { "Description", "Sample query activity" } }
            }
        };
    }

    private double DetectQueryFrequencyAnomaly(List<UserActivity> activities)
    {
        // Simple frequency check
        var recentActivities = activities.Where(a => a.Timestamp > DateTime.UtcNow.AddHours(-1)).Count();
        return recentActivities > 100 ? 0.8 : 0.0;
    }

    private async Task<List<string>> DetectUnusualQueryPatternsAsync(List<UserActivity> activities)
    {
        // Return mock patterns for now
        return new List<string> { "Unusual SELECT pattern detected" };
    }

    private double DetectErrorRateAnomaly(List<UserActivity> activities)
    {
        // Simple error rate check
        return 0.1; // 10% error rate
    }

    private async Task UpdateLearningModelsAsync(UserFeedback feedback)
    {
        // Mock implementation
        _logger.LogInformation("Updating learning models with feedback");
    }

    private async Task UpdateQuerySuccessPatternsAsync(UserFeedback feedback)
    {
        // Mock implementation
        _logger.LogInformation("Updating query success patterns");
    }

    private async Task UpdateOptimizationInsightsAsync(UserFeedback feedback)
    {
        // Mock implementation
        _logger.LogInformation("Updating optimization insights");
    }

    private async Task<List<string>> GetSuccessfulPatternsAsync(string userId)
    {
        return new List<string> { "Common aggregation patterns", "Effective filtering strategies" };
    }

    private async Task<List<string>> GetCommonMistakesAsync(string userId)
    {
        return new List<string> { "Missing WHERE clauses", "Inefficient JOINs" };
    }

    private async Task<List<string>> GetOptimizationSuggestionsAsync(string userId)
    {
        return new List<string> { "Use indexes for better performance", "Consider query caching" };
    }

    private async Task<Dictionary<string, object>> GetUserPreferencesAsync(string userId)
    {
        return new Dictionary<string, object>
        {
            { "PreferredVisualization", "BarChart" },
            { "DefaultRowLimit", 1000 }
        };
    }

    private async Task<Dictionary<string, double>> GetPerformanceInsightsAsync(string userId)
    {
        return new Dictionary<string, double>
        {
            { "AverageQueryTime", 2.5 },
            { "CacheHitRate", 0.75 }
        };
    }

    #endregion
}
