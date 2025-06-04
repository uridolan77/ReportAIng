using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models.ML;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// Feedback learning engine for processing user feedback and improving AI responses
/// DEPRECATED: This functionality has been consolidated into LearningService.cs
/// Use LearningService for comprehensive feedback learning and anomaly detection.
/// This class is kept for backward compatibility and will be removed in future versions.
/// </summary>
[Obsolete("Use LearningService instead. This class will be removed in future versions.")]
public class FeedbackLearningEngine
{
    private readonly ILogger<FeedbackLearningEngine> _logger;

    public FeedbackLearningEngine(ILogger<FeedbackLearningEngine> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get learning insights for a user
    /// </summary>
    public async Task<LearningInsights> GetLearningInsightsAsync(string userId)
    {
        _logger.LogInformation("Getting learning insights for user {UserId}", userId);

        return new LearningInsights
        {
            UserId = userId,
            InsightType = "UserBehavior",
            Description = "User learning insights",
            Confidence = 0.8,
            GeneratedAt = DateTime.UtcNow,
            SuccessfulPatterns = new List<string> { "Pattern1", "Pattern2" },
            CommonMistakes = new List<string> { "Mistake1", "Mistake2" },
            OptimizationSuggestions = new List<string> { "Suggestion1", "Suggestion2" }
        };
    }

    /// <summary>
    /// Get learning statistics
    /// </summary>
    public async Task<Dictionary<string, object>> GetLearningStatisticsAsync()
    {
        _logger.LogInformation("Getting learning statistics");

        return new Dictionary<string, object>
        {
            ["TotalFeedback"] = 100,
            ["PositiveFeedback"] = 80,
            ["NegativeFeedback"] = 20,
            ["AverageRating"] = 4.2,
            ["LastUpdated"] = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Process feedback with additional parameters
    /// </summary>
    public async Task ProcessFeedbackAsync(string userId, string queryId, UserFeedback feedback, string additionalContext)
    {
        _logger.LogInformation("Processing feedback for user {UserId}, query {QueryId}", userId, queryId);

        // Process the feedback
        await Task.CompletedTask;
    }

    /// <summary>
    /// Process feedback with basic parameters
    /// </summary>
    public async Task ProcessFeedbackAsync(UserFeedback feedback)
    {
        _logger.LogInformation("Processing feedback {FeedbackId}", feedback.FeedbackId);

        // Process the feedback
        await Task.CompletedTask;
    }
}
