using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Phase 4: Human-in-Loop Review Models
/// </summary>

/// <summary>
/// Review request for human validation
/// </summary>
public class ReviewRequest
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string OriginalQuery { get; set; } = string.Empty;
    public string GeneratedSql { get; set; } = string.Empty;
    public string? CorrectedSql { get; set; }
    public ReviewType Type { get; set; }
    public ReviewStatus Status { get; set; } = ReviewStatus.Pending;
    public ReviewPriority Priority { get; set; } = ReviewPriority.Normal;
    public string RequestedBy { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewNotes { get; set; }
    public List<ValidationIssue> ValidationIssues { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
    public double? ConfidenceScore { get; set; }
    public bool RequiresApproval { get; set; } = true;
}

/// <summary>
/// Human feedback on AI-generated content
/// </summary>
public class HumanFeedback
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReviewRequestId { get; set; } = string.Empty;
    public string ReviewerId { get; set; } = string.Empty;
    public FeedbackType Type { get; set; }
    public FeedbackAction Action { get; set; }
    public string? CorrectedSql { get; set; }
    public string? Comments { get; set; }
    public List<string> IssuesIdentified { get; set; } = new();
    public List<string> SuggestedImprovements { get; set; } = new();
    public int QualityRating { get; set; } // 1-5 scale
    public bool IsApproved { get; set; }
    public DateTime ProvidedAt { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> AdditionalData { get; set; } = new();
}

/// <summary>
/// Approval workflow for sensitive queries
/// </summary>
public class ApprovalWorkflow
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReviewRequestId { get; set; } = string.Empty;
    public string WorkflowName { get; set; } = string.Empty;
    public List<ApprovalStep> Steps { get; set; } = new();
    public int CurrentStepIndex { get; set; } = 0;
    public WorkflowStatus Status { get; set; } = WorkflowStatus.InProgress;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public string? FinalDecision { get; set; }
    public Dictionary<string, object> Context { get; set; } = new();
}

/// <summary>
/// Individual step in approval workflow
/// </summary>
public class ApprovalStep
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public string? AssignedRole { get; set; }
    public ApprovalStepStatus Status { get; set; } = ApprovalStepStatus.Pending;
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Decision { get; set; }
    public string? Comments { get; set; }
    public bool IsRequired { get; set; } = true;
    public int Order { get; set; }
    public TimeSpan? Timeout { get; set; }
}

/// <summary>
/// Review analytics and metrics
/// </summary>
public class ReviewAnalytics
{
    public string Period { get; set; } = string.Empty;
    public int TotalReviews { get; set; }
    public int ApprovedReviews { get; set; }
    public int RejectedReviews { get; set; }
    public int PendingReviews { get; set; }
    public double AverageReviewTime { get; set; } // in minutes
    public double ApprovalRate { get; set; }
    public Dictionary<string, int> ReviewsByType { get; set; } = new();
    public Dictionary<string, int> ReviewsByPriority { get; set; } = new();
    public Dictionary<string, double> ReviewerPerformance { get; set; } = new();
    public List<string> CommonIssues { get; set; } = new();
    public List<string> ImprovementSuggestions { get; set; } = new();
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Validation issue identified during review
/// </summary>
public class ValidationIssue
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IssueSeverity Severity { get; set; }
    public string? Location { get; set; }
    public string? SuggestedFix { get; set; }
    public bool IsResolved { get; set; } = false;
    public string? ResolvedBy { get; set; }
    public DateTime? ResolvedAt { get; set; }
}

/// <summary>
/// Review queue item for dashboard
/// </summary>
public class ReviewQueueItem
{
    public string Id { get; set; } = string.Empty;
    public string OriginalQuery { get; set; } = string.Empty;
    public string GeneratedSql { get; set; } = string.Empty;
    public ReviewType Type { get; set; }
    public ReviewPriority Priority { get; set; }
    public ReviewStatus Status { get; set; }
    public string RequestedBy { get; set; } = string.Empty;
    public string? AssignedTo { get; set; }
    public DateTime CreatedAt { get; set; }
    public TimeSpan? WaitingTime { get; set; }
    public int ValidationIssueCount { get; set; }
    public double? ConfidenceScore { get; set; }
    public bool IsUrgent { get; set; }
}

/// <summary>
/// Review configuration settings
/// </summary>
public class ReviewConfiguration
{
    public bool AutoReviewEnabled { get; set; } = true;
    public double AutoApprovalThreshold { get; set; } = 0.9;
    public double ManualReviewThreshold { get; set; } = 0.7;
    public TimeSpan DefaultReviewTimeout { get; set; } = TimeSpan.FromHours(24);
    public List<string> RequiredApprovalRoles { get; set; } = new();
    public Dictionary<ReviewType, ReviewTypeConfig> TypeConfigurations { get; set; } = new();
    public bool NotificationsEnabled { get; set; } = true;
    public TimeSpan NotificationInterval { get; set; } = TimeSpan.FromHours(4);
}

/// <summary>
/// Configuration for specific review types
/// </summary>
public class ReviewTypeConfig
{
    public bool RequiresApproval { get; set; } = true;
    public List<string> RequiredRoles { get; set; } = new();
    public TimeSpan Timeout { get; set; } = TimeSpan.FromHours(24);
    public ReviewPriority DefaultPriority { get; set; } = ReviewPriority.Normal;
    public bool AutoAssignEnabled { get; set; } = true;
    public List<string> AutoAssignToRoles { get; set; } = new();
}

// Enums
public enum ReviewType
{
    SqlValidation,
    SemanticAlignment,
    BusinessLogic,
    SecurityReview,
    PerformanceReview,
    ComplianceReview,
    DataAccess,
    SensitiveData
}

public enum ReviewStatus
{
    Pending,
    InReview,
    Approved,
    Rejected,
    RequiresChanges,
    Escalated,
    Cancelled,
    Expired
}

public enum ReviewPriority
{
    Low,
    Normal,
    High,
    Critical,
    Urgent
}

public enum FeedbackType
{
    Approval,
    Rejection,
    Correction,
    Suggestion,
    Question,
    Escalation
}

public enum FeedbackAction
{
    Approve,
    Reject,
    RequestChanges,
    Escalate,
    Defer,
    Cancel
}

public enum WorkflowStatus
{
    InProgress,
    Completed,
    Cancelled,
    Failed,
    Expired
}

public enum ApprovalStepStatus
{
    Pending,
    InProgress,
    Approved,
    Rejected,
    Skipped,
    Expired
}



/// <summary>
/// Review notification
/// </summary>
public class ReviewNotification
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ReviewRequestId { get; set; } = string.Empty;
    public string RecipientId { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationPriority Priority { get; set; } = NotificationPriority.Normal;
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReadAt { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
}

public enum NotificationType
{
    ReviewAssigned,
    ReviewReminder,
    ReviewEscalated,
    ReviewCompleted,
    ReviewExpired,
    WorkflowStarted,
    WorkflowCompleted
}

public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}
