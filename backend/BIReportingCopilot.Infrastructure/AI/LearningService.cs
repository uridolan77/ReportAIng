using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.ML;
using BIReportingCopilot.Infrastructure.Data;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI;

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
    public async Task<AnomalyDetectionResult> DetectQueryAnomaliesAsync(string query, QueryMetrics metrics)
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
                Timestamp = DateTime.UtcNow
            };

            // Check execution time anomalies
            var executionTimeAnomaly = await DetectExecutionTimeAnomalyAsync(TimeSpan.FromMilliseconds(metrics.ExecutionTime));
            if (executionTimeAnomaly != null)
            {
                result.DetectedAnomalies.Add(executionTimeAnomaly);
                result.IsAnomalous = true;
            }

            // Check result size anomalies
            var resultSizeAnomaly = await DetectResultSizeAnomalyAsync(metrics.ResultCount);
            if (resultSizeAnomaly != null)
            {
                result.DetectedAnomalies.Add(resultSizeAnomaly);
                result.IsAnomalous = true;
            }

            // Check query complexity anomalies
            var complexityAnomaly = await DetectComplexityAnomalyAsync(query, metrics);
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
            result.AnomalyScore = CalculateOverallAnomalyScore(result.DetectedAnomalies);

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
                Timestamp = DateTime.UtcNow
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
            var recentActivity = await GetUserActivityAsync(userId, startTime, endTime);

            // Check for unusual query frequency
            var frequencyAnomaly = DetectQueryFrequencyAnomaly(recentActivity, userId);
            if (frequencyAnomaly != null)
                anomalies.Add(frequencyAnomaly);

            // Check for unusual query patterns
            var patternAnomaly = await DetectUnusualQueryPatternsAsync(recentActivity, userId);
            if (patternAnomaly != null)
                anomalies.Add(patternAnomaly);

            // Check for unusual error rates
            var errorAnomaly = DetectErrorRateAnomaly(recentActivity, userId);
            if (errorAnomaly != null)
                anomalies.Add(errorAnomaly);

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
    public async Task ProcessFeedbackAsync(UserFeedback feedback)
    {
        try
        {
            _logger.LogDebug("Processing feedback for query: {QueryId}", feedback.QueryId);

            // Store feedback in database
            var feedbackEntry = new AIFeedbackEntry
            {
                // Id will be auto-generated by database
                QueryId = feedback.QueryId,
                UserId = feedback.UserId,
                Rating = feedback.Rating,
                Comments = feedback.Comments,
                Category = feedback.Category,
                Timestamp = DateTime.UtcNow,
                Metadata = JsonSerializer.Serialize(feedback.Metadata ?? new Dictionary<string, object>())
            };

            _context.AIFeedbackEntries.Add(feedbackEntry);
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
                SuccessfulPatterns = await GetSuccessfulPatternsAsync(userId, category),
                CommonMistakes = await GetCommonMistakesAsync(userId, category),
                OptimizationSuggestions = await GetOptimizationSuggestionsAsync(userId, category),
                UserPreferences = await GetUserPreferencesAsync(userId),
                PerformanceInsights = (await GetPerformanceInsightsAsync(userId, category)).ToDictionary(p => p, p => 1.0)
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
    /// Learn from query execution patterns
    /// </summary>
    public async Task LearnFromQueryExecutionAsync(QueryExecutionContext context)
    {
        try
        {
            _logger.LogDebug("Learning from query execution: {QueryId}", context.QueryId);

            // Extract learning patterns from execution
            var patterns = ExtractExecutionPatterns(context);

            // Update performance models
            await UpdatePerformanceModelsAsync(context, patterns);

            // Update success/failure patterns
            await UpdateExecutionPatternsAsync(context, patterns);

            // Update optimization recommendations
            await UpdateOptimizationRecommendationsAsync(context, patterns);

            _logger.LogDebug("Learning completed for query execution: {QueryId}", context.QueryId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error learning from query execution: {QueryId}", context.QueryId);
        }
    }

    /// <summary>
    /// Get personalized recommendations based on learning
    /// </summary>
    public async Task<List<PersonalizedRecommendation>> GetPersonalizedRecommendationsAsync(string userId)
    {
        try
        {
            var recommendations = new List<PersonalizedRecommendation>();

            // Get user's learning insights
            var insights = await GenerateLearningInsightsAsync(userId);

            // Generate query optimization recommendations
            var optimizationRecs = GenerateOptimizationRecommendations(insights);
            recommendations.AddRange(optimizationRecs);

            // Generate pattern-based recommendations
            var patternRecs = await GeneratePatternRecommendationsAsync(userId, insights);
            recommendations.AddRange(patternRecs);

            // Generate performance recommendations
            var performanceRecs = GeneratePerformanceRecommendations(insights);
            recommendations.AddRange(performanceRecs);

            // Sort by relevance and confidence
            recommendations = recommendations
                .OrderByDescending(r => r.Confidence)
                .ThenByDescending(r => r.Relevance)
                .Take(10)
                .ToList();

            return recommendations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting personalized recommendations for user {UserId}", userId);
            return new List<PersonalizedRecommendation>();
        }
    }

    #endregion

    #region Private Helper Methods - Anomaly Detection

    private async Task<DetectedAnomaly?> DetectExecutionTimeAnomalyAsync(TimeSpan executionTime)
    {
        try
        {
            // Get historical execution times for comparison
            var recentExecutions = await _context.QueryExecutionLogs
                .Where(q => q.Timestamp > DateTime.UtcNow.AddDays(-7))
                .Select(q => TimeSpan.FromMilliseconds(q.ExecutionTimeMs))
                .ToListAsync();

            if (recentExecutions.Count < 10) return null; // Need sufficient data

            var avgExecutionTime = TimeSpan.FromMilliseconds(recentExecutions.Average(t => t.TotalMilliseconds));
            var threshold = avgExecutionTime.TotalMilliseconds * 3; // 3x average is anomalous

            if (executionTime.TotalMilliseconds > threshold)
            {
                return new DetectedAnomaly
                {
                    Type = "ExecutionTime",
                    Severity = executionTime.TotalMilliseconds > threshold * 2 ? AnomalySeverity.High : AnomalySeverity.Medium,
                    Description = $"Execution time ({executionTime.TotalSeconds:F2}s) is significantly higher than average ({avgExecutionTime.TotalSeconds:F2}s)",
                    Score = Math.Min(1.0, executionTime.TotalMilliseconds / threshold),
                    Recommendations = new List<string>
                    {
                        "Consider adding indexes to improve query performance",
                        "Review query complexity and optimize if possible",
                        "Check for table locks or blocking queries"
                    }
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error detecting execution time anomaly");
            return null;
        }
    }

    private async Task<DetectedAnomaly?> DetectResultSizeAnomalyAsync(int resultCount)
    {
        try
        {
            // Get historical result sizes
            var recentResults = await _context.QueryExecutionLogs
                .Where(q => q.Timestamp > DateTime.UtcNow.AddDays(-7))
                .Select(q => q.RowCount)
                .ToListAsync();

            if (recentResults.Count < 10) return null;

            var avgResultCount = recentResults.Average();
            var threshold = avgResultCount * 10; // 10x average is anomalous

            if (resultCount > threshold)
            {
                return new DetectedAnomaly
                {
                    Type = "ResultSize",
                    Severity = resultCount > threshold * 2 ? AnomalySeverity.High : AnomalySeverity.Medium,
                    Description = $"Result count ({resultCount:N0}) is significantly higher than average ({avgResultCount:N0})",
                    Score = Math.Min(1.0, resultCount / threshold),
                    Recommendations = new List<string>
                    {
                        "Consider adding WHERE clauses to filter results",
                        "Use LIMIT/TOP to restrict result set size",
                        "Review if all returned data is necessary"
                    }
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error detecting result size anomaly");
            return null;
        }
    }

    private async Task<DetectedAnomaly?> DetectComplexityAnomalyAsync(string query, QueryMetrics metrics)
    {
        try
        {
            var complexity = CalculateQueryComplexity(query);

            if (complexity > 0.8) // High complexity threshold
            {
                return new DetectedAnomaly
                {
                    Type = "QueryComplexity",
                    Severity = complexity > 0.9 ? AnomalySeverity.High : AnomalySeverity.Medium,
                    Description = $"Query complexity ({complexity:P0}) is unusually high",
                    Score = complexity,
                    Recommendations = new List<string>
                    {
                        "Consider breaking complex query into smaller parts",
                        "Review join conditions and optimize where possible",
                        "Consider using views or stored procedures for complex logic"
                    }
                };
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error detecting complexity anomaly");
            return null;
        }
    }

    private async Task<DetectedAnomaly?> DetectPatternAnomalyAsync(string query)
    {
        try
        {
            // Check for unusual query patterns
            var lowerQuery = query.ToLowerInvariant();

            // Check for potentially dangerous patterns
            var dangerousPatterns = new[]
            {
                "select * from",
                "where 1=1",
                "or 1=1",
                "union select",
                "drop table",
                "delete from"
            };

            foreach (var pattern in dangerousPatterns)
            {
                if (lowerQuery.Contains(pattern))
                {
                    return new DetectedAnomaly
                    {
                        Type = "SuspiciousPattern",
                        Severity = AnomalySeverity.High,
                        Description = $"Query contains potentially dangerous pattern: {pattern}",
                        Score = 0.9,
                        Recommendations = new List<string>
                        {
                            "Review query for security implications",
                            "Consider using parameterized queries",
                            "Validate query against security policies"
                        }
                    };
                }
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error detecting pattern anomaly");
            return null;
        }
    }

    private double CalculateOverallAnomalyScore(List<DetectedAnomaly> anomalies)
    {
        if (!anomalies.Any()) return 0.0;

        // Weight anomalies by severity
        var weightedScore = anomalies.Sum(a => a.Score * GetSeverityWeight(a.Severity));
        var totalWeight = anomalies.Sum(a => GetSeverityWeight(a.Severity));

        return Math.Min(1.0, weightedScore / totalWeight);
    }

    private double GetSeverityWeight(AnomalySeverity severity)
    {
        return severity switch
        {
            AnomalySeverity.Low => 1.0,
            AnomalySeverity.Medium => 2.0,
            AnomalySeverity.High => 3.0,
            _ => 1.0
        };
    }

    private double CalculateQueryComplexity(string query)
    {
        var complexity = 0.0;
        var lowerQuery = query.ToLowerInvariant();

        // Count joins
        var joinCount = System.Text.RegularExpressions.Regex.Matches(lowerQuery, @"\bjoin\b").Count;
        complexity += joinCount * 0.2;

        // Count subqueries
        var subqueryCount = System.Text.RegularExpressions.Regex.Matches(query, @"\(.*select.*\)", System.Text.RegularExpressions.RegexOptions.IgnoreCase).Count;
        complexity += subqueryCount * 0.3;

        // Count aggregations
        var aggregationCount = System.Text.RegularExpressions.Regex.Matches(lowerQuery, @"\b(count|sum|avg|max|min|group by)\b").Count;
        complexity += aggregationCount * 0.1;

        // Count conditions
        var conditionCount = System.Text.RegularExpressions.Regex.Matches(lowerQuery, @"\b(and|or)\b").Count;
        complexity += conditionCount * 0.05;

        return Math.Min(1.0, complexity);
    }

    #endregion

    #region Private Helper Methods - Learning

    private async Task<List<UserActivity>> GetUserActivityAsync(string userId, DateTime startTime, DateTime endTime)
    {
        try
        {
            var activities = await _context.QueryExecutionLogs
                .Where(q => q.UserId == userId && q.Timestamp >= startTime && q.Timestamp <= endTime)
                .Select(q => new UserActivity
                {
                    Timestamp = q.Timestamp,
                    Query = q.Query,
                    Success = q.IsSuccessful,
                    ExecutionTime = TimeSpan.FromMilliseconds(q.ExecutionTimeMs),
                    ResultCount = q.RowCount
                })
                .ToListAsync();

            return activities;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting user activity");
            return new List<UserActivity>();
        }
    }

    private BehaviorAnomaly? DetectQueryFrequencyAnomaly(List<UserActivity> activities, string userId)
    {
        var queryCount = activities.Count;
        var timeSpan = activities.Any() ? activities.Max(a => a.Timestamp) - activities.Min(a => a.Timestamp) : TimeSpan.Zero;

        if (timeSpan.TotalHours > 0)
        {
            var queriesPerHour = queryCount / timeSpan.TotalHours;

            // Threshold: more than 100 queries per hour is unusual
            if (queriesPerHour > 100)
            {
                return new BehaviorAnomaly
                {
                    Type = "HighQueryFrequency",
                    Description = $"User executed {queryCount} queries in {timeSpan.TotalHours:F1} hours ({queriesPerHour:F1} queries/hour)",
                    Severity = queriesPerHour > 200 ? (double)AnomalySeverity.High : (double)AnomalySeverity.Medium,
                    UserId = userId,
                    DetectedAt = DateTime.UtcNow
                };
            }
        }

        return null;
    }

    private async Task<BehaviorAnomaly?> DetectUnusualQueryPatternsAsync(List<UserActivity> activities, string userId)
    {
        // Check for unusual error rates
        var errorRate = activities.Any() ? activities.Count(a => !a.Success) / (double)activities.Count : 0;

        if (errorRate > 0.5) // More than 50% errors is unusual
        {
            return new BehaviorAnomaly
            {
                Type = "HighErrorRate",
                Description = $"User has {errorRate:P0} error rate in recent queries",
                Severity = errorRate > 0.8 ? (double)AnomalySeverity.High : (double)AnomalySeverity.Medium,
                UserId = userId,
                DetectedAt = DateTime.UtcNow
            };
        }

        return null;
    }

    private BehaviorAnomaly? DetectErrorRateAnomaly(List<UserActivity> activities, string userId)
    {
        if (!activities.Any()) return null;

        var errorCount = activities.Count(a => !a.Success);
        var errorRate = errorCount / (double)activities.Count;

        if (errorRate > 0.3) // More than 30% errors
        {
            return new BehaviorAnomaly
            {
                Type = "HighErrorRate",
                Description = $"High error rate: {errorCount} errors out of {activities.Count} queries ({errorRate:P0})",
                Severity = errorRate > 0.6 ? (double)AnomalySeverity.High : (double)AnomalySeverity.Medium,
                UserId = userId,
                DetectedAt = DateTime.UtcNow
            };
        }

        return null;
    }

    #endregion

    #region Model Classes

    private class AnomalyModel
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, double> Parameters { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    private class LearningModel
    {
        public string Name { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime LastUpdated { get; set; }
    }

    private class UserActivity
    {
        public DateTime Timestamp { get; set; }
        public string Query { get; set; } = string.Empty;
        public bool Success { get; set; }
        public TimeSpan ExecutionTime { get; set; }
        public int ResultCount { get; set; }
    }

    #endregion

    // Placeholder methods for learning functionality
    private async Task UpdateLearningModelsAsync(UserFeedback feedback) { /* Implementation */ }
    private async Task UpdateQuerySuccessPatternsAsync(UserFeedback feedback) { /* Implementation */ }
    private async Task UpdateOptimizationInsightsAsync(UserFeedback feedback) { /* Implementation */ }
    private async Task<List<string>> GetSuccessfulPatternsAsync(string? userId, string? category) => new();
    private async Task<List<string>> GetCommonMistakesAsync(string? userId, string? category) => new();
    private async Task<List<string>> GetOptimizationSuggestionsAsync(string? userId, string? category) => new();
    private async Task<Dictionary<string, object>> GetUserPreferencesAsync(string? userId) => new();
    private async Task<List<string>> GetPerformanceInsightsAsync(string? userId, string? category) => new();
    private List<ExecutionPattern> ExtractExecutionPatterns(QueryExecutionContext context) => new();
    private async Task UpdatePerformanceModelsAsync(QueryExecutionContext context, List<ExecutionPattern> patterns) { }
    private async Task UpdateExecutionPatternsAsync(QueryExecutionContext context, List<ExecutionPattern> patterns) { }
    private async Task UpdateOptimizationRecommendationsAsync(QueryExecutionContext context, List<ExecutionPattern> patterns) { }
    private List<PersonalizedRecommendation> GenerateOptimizationRecommendations(LearningInsights insights) => new();
    private async Task<List<PersonalizedRecommendation>> GeneratePatternRecommendationsAsync(string userId, LearningInsights insights) => new();
    private List<PersonalizedRecommendation> GeneratePerformanceRecommendations(LearningInsights insights) => new();

    private class ExecutionPattern { }
}