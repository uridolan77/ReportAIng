using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Review;

/// <summary>
/// Phase 4: Human-in-Loop Review Service Interfaces
/// </summary>

/// <summary>
/// Service for managing human review workflows
/// </summary>
public interface IHumanReviewService
{
    /// <summary>
    /// Submit a query for human review
    /// </summary>
    Task<ReviewRequest> SubmitForReviewAsync(
        string originalQuery,
        string generatedSql,
        ReviewType type,
        string requestedBy,
        ReviewPriority priority = ReviewPriority.Normal,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get review request by ID
    /// </summary>
    Task<ReviewRequest?> GetReviewRequestAsync(string reviewId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending reviews for a user
    /// </summary>
    Task<List<ReviewQueueItem>> GetPendingReviewsAsync(
        string? userId = null,
        ReviewType? type = null,
        ReviewPriority? priority = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assign review to a user
    /// </summary>
    Task<bool> AssignReviewAsync(string reviewId, string assignedTo, CancellationToken cancellationToken = default);

    /// <summary>
    /// Complete a review with feedback
    /// </summary>
    Task<bool> CompleteReviewAsync(
        string reviewId,
        HumanFeedback feedback,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get review analytics
    /// </summary>
    Task<ReviewAnalytics> GetReviewAnalyticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a review request
    /// </summary>
    Task<bool> CancelReviewAsync(string reviewId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Escalate a review to higher authority
    /// </summary>
    Task<bool> EscalateReviewAsync(string reviewId, string reason, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for managing approval workflows
/// </summary>
public interface IApprovalWorkflowService
{
    /// <summary>
    /// Start an approval workflow
    /// </summary>
    Task<ApprovalWorkflow> StartWorkflowAsync(
        string reviewRequestId,
        string workflowName,
        List<ApprovalStep> steps,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow by ID
    /// </summary>
    Task<ApprovalWorkflow?> GetWorkflowAsync(string workflowId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Process approval step
    /// </summary>
    Task<bool> ProcessApprovalStepAsync(
        string workflowId,
        string stepId,
        string decision,
        string? comments = null,
        string? processedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get active workflows for a user
    /// </summary>
    Task<List<ApprovalWorkflow>> GetActiveWorkflowsAsync(
        string? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a workflow
    /// </summary>
    Task<bool> CancelWorkflowAsync(string workflowId, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get workflow templates
    /// </summary>
    Task<List<WorkflowTemplate>> GetWorkflowTemplatesAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for collecting and processing human feedback
/// </summary>
public interface IHumanFeedbackService
{
    /// <summary>
    /// Submit feedback for a review
    /// </summary>
    Task<HumanFeedback> SubmitFeedbackAsync(
        string reviewRequestId,
        string reviewerId,
        FeedbackType type,
        FeedbackAction action,
        string? correctedSql = null,
        string? comments = null,
        int qualityRating = 3,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback for a review
    /// </summary>
    Task<List<HumanFeedback>> GetFeedbackAsync(
        string reviewRequestId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get feedback analytics
    /// </summary>
    Task<FeedbackAnalytics> GetFeedbackAnalyticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? reviewerId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Learn from human feedback to improve AI
    /// </summary>
    Task<bool> LearnFromFeedbackAsync(
        HumanFeedback feedback,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get common feedback patterns
    /// </summary>
    Task<List<FeedbackPattern>> GetFeedbackPatternsAsync(
        ReviewType? type = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for review notifications
/// </summary>
public interface IReviewNotificationService
{
    /// <summary>
    /// Send review assignment notification
    /// </summary>
    Task<bool> SendReviewAssignmentNotificationAsync(
        string reviewId,
        string assignedTo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send review reminder notification
    /// </summary>
    Task<bool> SendReviewReminderAsync(
        string reviewId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Send escalation notification
    /// </summary>
    Task<bool> SendEscalationNotificationAsync(
        string reviewId,
        string reason,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get notifications for a user
    /// </summary>
    Task<List<ReviewNotification>> GetNotificationsAsync(
        string userId,
        bool unreadOnly = false,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Mark notification as read
    /// </summary>
    Task<bool> MarkNotificationAsReadAsync(string notificationId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get notification settings for a user
    /// </summary>
    Task<NotificationSettings> GetNotificationSettingsAsync(
        string userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update notification settings
    /// </summary>
    Task<bool> UpdateNotificationSettingsAsync(
        string userId,
        NotificationSettings settings,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for review configuration management
/// </summary>
public interface IReviewConfigurationService
{
    /// <summary>
    /// Get review configuration
    /// </summary>
    Task<ReviewConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Update review configuration
    /// </summary>
    Task<bool> UpdateConfigurationAsync(
        ReviewConfiguration configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get configuration for specific review type
    /// </summary>
    Task<ReviewTypeConfig> GetTypeConfigurationAsync(
        ReviewType type,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Update configuration for specific review type
    /// </summary>
    Task<bool> UpdateTypeConfigurationAsync(
        ReviewType type,
        ReviewTypeConfig configuration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate configuration
    /// </summary>
    Task<ConfigurationValidationResult> ValidateConfigurationAsync(
        ReviewConfiguration configuration,
        CancellationToken cancellationToken = default);
}

// Additional models for interfaces
public class WorkflowTemplate
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<ApprovalStep> Steps { get; set; } = new();
    public ReviewType ApplicableType { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string CreatedBy { get; set; } = string.Empty;
}

public class FeedbackAnalytics
{
    public string Period { get; set; } = string.Empty;
    public int TotalFeedback { get; set; }
    public double AverageQualityRating { get; set; }
    public Dictionary<FeedbackType, int> FeedbackByType { get; set; } = new();
    public Dictionary<FeedbackAction, int> ActionsByType { get; set; } = new();
    public List<string> CommonIssues { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public Dictionary<string, double> ReviewerRatings { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

public class FeedbackPattern
{
    public string Pattern { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public ReviewType Type { get; set; }
    public List<string> CommonIssues { get; set; } = new();
    public string SuggestedImprovement { get; set; } = string.Empty;
    public double ImpactScore { get; set; }
}

public class NotificationSettings
{
    public string UserId { get; set; } = string.Empty;
    public bool EmailNotifications { get; set; } = true;
    public bool InAppNotifications { get; set; } = true;
    public bool SlackNotifications { get; set; } = false;
    public TimeSpan ReminderInterval { get; set; } = TimeSpan.FromHours(4);
    public List<ReviewType> NotificationTypes { get; set; } = new();
    public List<ReviewPriority> NotificationPriorities { get; set; } = new();
    public bool WeekendNotifications { get; set; } = false;
    public TimeSpan QuietHoursStart { get; set; } = TimeSpan.FromHours(18);
    public TimeSpan QuietHoursEnd { get; set; } = TimeSpan.FromHours(8);
}

public class ConfigurationValidationResult
{
    public bool IsValid { get; set; }
    public List<string> Errors { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public List<string> Suggestions { get; set; } = new();
}
