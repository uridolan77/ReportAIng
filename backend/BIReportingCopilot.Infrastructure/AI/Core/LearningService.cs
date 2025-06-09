using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using MLQueryMetrics = BIReportingCopilot.Core.Models.ML.QueryMetrics;
using MLUserFeedback = BIReportingCopilot.Core.Models.ML.UserFeedback;
using BIReportingCopilot.Core.Models.ML;
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
    public async Task ProcessFeedbackAsync(MLUserFeedback feedback)
    {
        try
        {
            _logger.LogDebug("Processing feedback for query: {QueryId}", feedback.QueryId);

            // Store feedback in database using unified model
            var unifiedFeedbackEntry = new Core.Models.UnifiedAIFeedbackEntry
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
