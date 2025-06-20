namespace BIReportingCopilot.Core.Interfaces.AI;

/// <summary>
/// Interface for AI learning and feedback processing
/// </summary>
public interface IAILearningService
{
    /// <summary>
    /// Submit feedback for AI learning
    /// </summary>
    Task<bool> SubmitFeedbackAsync(
        string sessionId,
        string userId,
        Dictionary<string, object> feedbackData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Learn from user corrections
    /// </summary>
    Task<bool> LearnFromCorrectionAsync(
        string originalQuery,
        string correctedQuery,
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get learning metrics
    /// </summary>
    Task<LearningMetrics> GetLearningMetricsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Apply learned patterns to improve AI responses
    /// </summary>
    Task<bool> ApplyLearningPatternsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Learning metrics for AI improvement tracking
/// </summary>
public class LearningMetrics
{
    public int TotalFeedbackItems { get; set; }
    public int PositiveFeedbackCount { get; set; }
    public int NegativeFeedbackCount { get; set; }
    public double AverageAccuracyScore { get; set; }
    public Dictionary<string, int> FeedbackByCategory { get; set; } = new();
    public List<string> CommonIssues { get; set; } = new();
    public List<string> ImprovementAreas { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
