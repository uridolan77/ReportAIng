namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Unified AI feedback entry for tracking user feedback on AI-generated content
/// </summary>
public class UnifiedAIFeedbackEntry
{
    /// <summary>
    /// Unique identifier for the feedback entry
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// User ID who provided the feedback
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// Reference to the AI generation attempt
    /// </summary>
    public string GenerationAttemptId { get; set; } = string.Empty;

    /// <summary>
    /// Feedback type (rating, correction, suggestion, etc.)
    /// </summary>
    public string FeedbackType { get; set; } = string.Empty;

    /// <summary>
    /// Feedback rating (1-5 scale)
    /// </summary>
    public int? Rating { get; set; }

    /// <summary>
    /// Feedback comments
    /// </summary>
    public string? Comments { get; set; }

    /// <summary>
    /// Corrected output (if user provided a correction)
    /// </summary>
    public string? CorrectedOutput { get; set; }

    /// <summary>
    /// Feedback category (accuracy, relevance, completeness, etc.)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Feedback sentiment (positive, negative, neutral)
    /// </summary>
    public string? Sentiment { get; set; }

    /// <summary>
    /// Whether this feedback was helpful for model improvement
    /// </summary>
    public bool IsHelpful { get; set; }

    /// <summary>
    /// Whether this feedback has been processed for model training
    /// </summary>
    public bool IsProcessed { get; set; }

    /// <summary>
    /// Additional metadata as JSON
    /// </summary>
    public string? Metadata { get; set; }

    /// <summary>
    /// Created timestamp
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Updated timestamp
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
