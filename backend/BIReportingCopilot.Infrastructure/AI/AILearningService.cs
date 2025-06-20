using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.AI;
using System.Collections.Concurrent;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Simple implementation of AI Learning Service for Phase 4
/// </summary>
public class AILearningService : IAILearningService
{
    private readonly ILogger<AILearningService> _logger;
    
    // In-memory storage for demo (would be database in production)
    private readonly ConcurrentDictionary<string, List<FeedbackItem>> _feedbackData = new();
    private readonly ConcurrentDictionary<string, LearningPattern> _learningPatterns = new();

    public AILearningService(ILogger<AILearningService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Submit feedback for AI learning
    /// </summary>
    public async Task<bool> SubmitFeedbackAsync(
        string sessionId,
        string userId,
        Dictionary<string, object> feedbackData,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🧠 Submitting AI learning feedback for session: {SessionId}", sessionId);

            var feedbackItem = new FeedbackItem
            {
                SessionId = sessionId,
                UserId = userId,
                Data = feedbackData,
                Timestamp = DateTime.UtcNow
            };

            if (!_feedbackData.ContainsKey(sessionId))
            {
                _feedbackData[sessionId] = new List<FeedbackItem>();
            }
            
            _feedbackData[sessionId].Add(feedbackItem);

            // Process feedback for learning patterns
            await ProcessFeedbackForLearningAsync(feedbackItem, cancellationToken);

            _logger.LogInformation("✅ AI learning feedback submitted successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error submitting AI learning feedback");
            return false;
        }
    }

    /// <summary>
    /// Learn from user corrections
    /// </summary>
    public async Task<bool> LearnFromCorrectionAsync(
        string originalQuery,
        string correctedQuery,
        string userId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📝 Learning from user correction by: {UserId}", userId);

            var correctionData = new Dictionary<string, object>
            {
                ["type"] = "correction",
                ["original_query"] = originalQuery,
                ["corrected_query"] = correctedQuery,
                ["correction_length"] = correctedQuery.Length - originalQuery.Length,
                ["has_structural_changes"] = !originalQuery.Equals(correctedQuery, StringComparison.OrdinalIgnoreCase)
            };

            // Analyze the correction for patterns
            await AnalyzeCorrectionPatternsAsync(originalQuery, correctedQuery, userId, cancellationToken);

            _logger.LogInformation("✅ Learning from correction completed");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error learning from correction");
            return false;
        }
    }

    /// <summary>
    /// Get learning metrics
    /// </summary>
    public async Task<LearningMetrics> GetLearningMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async operation

            var allFeedback = _feedbackData.Values
                .SelectMany(f => f)
                .Where(f => startDate == null || f.Timestamp >= startDate)
                .Where(f => endDate == null || f.Timestamp <= endDate)
                .ToList();

            var metrics = new LearningMetrics
            {
                TotalFeedbackItems = allFeedback.Count,
                GeneratedAt = DateTime.UtcNow
            };

            // Calculate positive/negative feedback
            metrics.PositiveFeedbackCount = allFeedback.Count(f => 
                f.Data.ContainsKey("rating") && 
                int.TryParse(f.Data["rating"].ToString(), out var rating) && 
                rating >= 4);

            metrics.NegativeFeedbackCount = allFeedback.Count(f => 
                f.Data.ContainsKey("rating") && 
                int.TryParse(f.Data["rating"].ToString(), out var rating) && 
                rating <= 2);

            // Calculate average accuracy
            var accuracyScores = allFeedback
                .Where(f => f.Data.ContainsKey("accuracy") && 
                           double.TryParse(f.Data["accuracy"].ToString(), out _))
                .Select(f => double.Parse(f.Data["accuracy"].ToString()!))
                .ToList();

            metrics.AverageAccuracyScore = accuracyScores.Any() ? accuracyScores.Average() : 0.0;

            // Feedback by category
            metrics.FeedbackByCategory = allFeedback
                .Where(f => f.Data.ContainsKey("category"))
                .GroupBy(f => f.Data["category"].ToString() ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count());

            // Common issues (simulated)
            metrics.CommonIssues = new List<string>
            {
                "Incorrect table joins",
                "Missing WHERE clauses",
                "Suboptimal query structure",
                "Business logic misalignment"
            };

            // Improvement areas (simulated)
            metrics.ImprovementAreas = new List<string>
            {
                "Natural language understanding",
                "Schema relationship inference",
                "Query optimization",
                "Business context awareness"
            };

            _logger.LogDebug("📊 Generated learning metrics: {TotalItems} items", metrics.TotalFeedbackItems);
            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating learning metrics");
            throw;
        }
    }

    /// <summary>
    /// Apply learned patterns to improve AI responses
    /// </summary>
    public async Task<bool> ApplyLearningPatternsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔄 Applying learning patterns to improve AI responses");

            await Task.Delay(1, cancellationToken); // Simulate async operation

            // In production, this would:
            // 1. Analyze accumulated feedback patterns
            // 2. Update AI model weights or prompts
            // 3. Adjust query generation strategies
            // 4. Update business context understanding

            var patternCount = _learningPatterns.Count;
            _logger.LogInformation("✅ Applied {PatternCount} learning patterns", patternCount);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error applying learning patterns");
            return false;
        }
    }

    // Private helper methods
    private async Task ProcessFeedbackForLearningAsync(FeedbackItem feedback, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async processing

            // Extract learning patterns from feedback
            if (feedback.Data.ContainsKey("type"))
            {
                var feedbackType = feedback.Data["type"].ToString();
                var patternKey = $"{feedbackType}_{feedback.UserId}";

                if (_learningPatterns.TryGetValue(patternKey, out var existingPattern))
                {
                    existingPattern.Frequency++;
                    existingPattern.LastSeen = DateTime.UtcNow;
                }
                else
                {
                    _learningPatterns[patternKey] = new LearningPattern
                    {
                        Type = feedbackType ?? "unknown",
                        UserId = feedback.UserId,
                        Frequency = 1,
                        FirstSeen = DateTime.UtcNow,
                        LastSeen = DateTime.UtcNow
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error processing feedback for learning");
        }
    }

    private async Task AnalyzeCorrectionPatternsAsync(
        string originalQuery, 
        string correctedQuery, 
        string userId, 
        CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(1, cancellationToken); // Simulate async analysis

            // Simple pattern analysis (in production, would use NLP/ML)
            var patterns = new List<string>();

            if (correctedQuery.Contains("JOIN") && !originalQuery.Contains("JOIN"))
            {
                patterns.Add("missing_join");
            }

            if (correctedQuery.Contains("WHERE") && !originalQuery.Contains("WHERE"))
            {
                patterns.Add("missing_where_clause");
            }

            if (correctedQuery.Length > originalQuery.Length * 1.5)
            {
                patterns.Add("significant_expansion");
            }

            // Store patterns for future learning
            foreach (var pattern in patterns)
            {
                var patternKey = $"correction_{pattern}_{userId}";
                
                if (_learningPatterns.TryGetValue(patternKey, out var existingPattern))
                {
                    existingPattern.Frequency++;
                    existingPattern.LastSeen = DateTime.UtcNow;
                }
                else
                {
                    _learningPatterns[patternKey] = new LearningPattern
                    {
                        Type = pattern,
                        UserId = userId,
                        Frequency = 1,
                        FirstSeen = DateTime.UtcNow,
                        LastSeen = DateTime.UtcNow
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error analyzing correction patterns");
        }
    }

    // Helper classes
    private class FeedbackItem
    {
        public string SessionId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public Dictionary<string, object> Data { get; set; } = new();
        public DateTime Timestamp { get; set; }
    }

    private class LearningPattern
    {
        public string Type { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public int Frequency { get; set; }
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
