using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces.Review;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using BIReportingCopilot.Core.Interfaces.Security;

namespace BIReportingCopilot.Infrastructure.Review;

/// <summary>
/// Phase 4: Human-in-Loop Review Service Implementation
/// </summary>
public class HumanReviewService : IHumanReviewService
{
    private readonly ILogger<HumanReviewService> _logger;
    private readonly BICopilotContext _context;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly IReviewNotificationService _notificationService;
    private readonly IReviewConfigurationService _configurationService;
    private readonly IAuditService _auditService;

    public HumanReviewService(
        ILogger<HumanReviewService> logger,
        BICopilotContext context,
        IApprovalWorkflowService workflowService,
        IReviewNotificationService notificationService,
        IReviewConfigurationService configurationService,
        IAuditService auditService)
    {
        _logger = logger;
        _context = context;
        _workflowService = workflowService;
        _notificationService = notificationService;
        _configurationService = configurationService;
        _auditService = auditService;
    }

    /// <summary>
    /// Submit a query for human review
    /// </summary>
    public async Task<ReviewRequest> SubmitForReviewAsync(
        string originalQuery,
        string generatedSql,
        ReviewType type,
        string requestedBy,
        ReviewPriority priority = ReviewPriority.Normal,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📝 Submitting query for human review: {Type} - {Priority}", type, priority);

            var reviewRequest = new ReviewRequest
            {
                OriginalQuery = originalQuery,
                GeneratedSql = generatedSql,
                Type = type,
                Priority = priority,
                RequestedBy = requestedBy,
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            // Get configuration for this review type
            var typeConfig = await _configurationService.GetTypeConfigurationAsync(type, cancellationToken);
            reviewRequest.RequiresApproval = typeConfig.RequiresApproval;

            // Auto-assign if enabled
            if (typeConfig.AutoAssignEnabled && typeConfig.AutoAssignToRoles.Any())
            {
                var assignedUser = await FindAvailableReviewerAsync(typeConfig.AutoAssignToRoles, cancellationToken);
                if (assignedUser != null)
                {
                    reviewRequest.AssignedTo = assignedUser;
                    reviewRequest.Status = ReviewStatus.InReview;
                }
            }

            // Store review request (in-memory for now, would be database in production)
            await StoreReviewRequestAsync(reviewRequest, cancellationToken);

            // Start approval workflow if required
            if (reviewRequest.RequiresApproval)
            {
                await StartApprovalWorkflowAsync(reviewRequest, typeConfig, cancellationToken);
            }

            // Send notification if assigned
            if (!string.IsNullOrEmpty(reviewRequest.AssignedTo))
            {
                await _notificationService.SendReviewAssignmentNotificationAsync(
                    reviewRequest.Id, reviewRequest.AssignedTo, cancellationToken);
            }

            // Log audit event
            await _auditService.LogAsync(
                "ReviewSubmitted",
                requestedBy,
                "ReviewRequest",
                reviewRequest.Id,
                new { Type = type, Priority = priority, AssignedTo = reviewRequest.AssignedTo });

            _logger.LogInformation("✅ Review request submitted: {ReviewId}", reviewRequest.Id);
            return reviewRequest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error submitting review request");
            throw;
        }
    }

    /// <summary>
    /// Get review request by ID
    /// </summary>
    public async Task<ReviewRequest?> GetReviewRequestAsync(string reviewId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🔍 Getting review request: {ReviewId}", reviewId);
            
            // In production, this would query the database
            return await GetStoredReviewRequestAsync(reviewId, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting review request: {ReviewId}", reviewId);
            return null;
        }
    }

    /// <summary>
    /// Get pending reviews for a user
    /// </summary>
    public async Task<List<ReviewQueueItem>> GetPendingReviewsAsync(
        string? userId = null,
        ReviewType? type = null,
        ReviewPriority? priority = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📋 Getting pending reviews for user: {UserId}", userId ?? "All");

            // In production, this would query the database with proper filtering
            var reviews = await GetStoredReviewQueueAsync(userId, type, priority, page, pageSize, cancellationToken);

            _logger.LogDebug("✅ Found {Count} pending reviews", reviews.Count);
            return reviews;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting pending reviews");
            return new List<ReviewQueueItem>();
        }
    }

    /// <summary>
    /// Assign review to a user
    /// </summary>
    public async Task<bool> AssignReviewAsync(string reviewId, string assignedTo, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("👤 Assigning review {ReviewId} to {AssignedTo}", reviewId, assignedTo);

            var reviewRequest = await GetReviewRequestAsync(reviewId, cancellationToken);
            if (reviewRequest == null)
            {
                _logger.LogWarning("⚠️ Review request not found: {ReviewId}", reviewId);
                return false;
            }

            reviewRequest.AssignedTo = assignedTo;
            reviewRequest.Status = ReviewStatus.InReview;

            await UpdateReviewRequestAsync(reviewRequest, cancellationToken);

            // Send assignment notification
            await _notificationService.SendReviewAssignmentNotificationAsync(reviewId, assignedTo, cancellationToken);

            // Log audit event
            await _auditService.LogAsync(
                "ReviewAssigned",
                assignedTo,
                "ReviewRequest",
                reviewId,
                new { AssignedTo = assignedTo });

            _logger.LogInformation("✅ Review assigned successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error assigning review");
            return false;
        }
    }

    /// <summary>
    /// Complete a review with feedback
    /// </summary>
    public async Task<bool> CompleteReviewAsync(
        string reviewId,
        HumanFeedback feedback,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("✅ Completing review {ReviewId} with action: {Action}", reviewId, feedback.Action);

            var reviewRequest = await GetReviewRequestAsync(reviewId, cancellationToken);
            if (reviewRequest == null)
            {
                _logger.LogWarning("⚠️ Review request not found: {ReviewId}", reviewId);
                return false;
            }

            // Update review status based on feedback action
            reviewRequest.Status = feedback.Action switch
            {
                FeedbackAction.Approve => ReviewStatus.Approved,
                FeedbackAction.Reject => ReviewStatus.Rejected,
                FeedbackAction.RequestChanges => ReviewStatus.RequiresChanges,
                FeedbackAction.Escalate => ReviewStatus.Escalated,
                FeedbackAction.Cancel => ReviewStatus.Cancelled,
                _ => ReviewStatus.InReview
            };

            reviewRequest.ReviewedAt = DateTime.UtcNow;
            reviewRequest.ReviewNotes = feedback.Comments;

            // Apply corrections if provided
            if (!string.IsNullOrEmpty(feedback.CorrectedSql))
            {
                reviewRequest.CorrectedSql = feedback.CorrectedSql;
            }

            await UpdateReviewRequestAsync(reviewRequest, cancellationToken);
            await StoreFeedbackAsync(feedback, cancellationToken);

            // Process workflow if exists
            var workflow = await _workflowService.GetWorkflowAsync(reviewId, cancellationToken);
            if (workflow != null)
            {
                await _workflowService.ProcessApprovalStepAsync(
                    workflow.Id,
                    workflow.Steps[workflow.CurrentStepIndex].Id,
                    feedback.Action.ToString(),
                    feedback.Comments,
                    feedback.ReviewerId,
                    cancellationToken);
            }

            // Log audit event
            await _auditService.LogAsync(
                "ReviewCompleted",
                feedback.ReviewerId,
                "ReviewRequest",
                reviewId,
                new { Action = feedback.Action, Status = reviewRequest.Status });

            _logger.LogInformation("✅ Review completed successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error completing review");
            return false;
        }
    }

    /// <summary>
    /// Get review analytics
    /// </summary>
    public async Task<ReviewAnalytics> GetReviewAnalyticsAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📊 Generating review analytics");

            var analytics = new ReviewAnalytics
            {
                Period = $"{startDate?.ToString("yyyy-MM-dd") ?? "All"} to {endDate?.ToString("yyyy-MM-dd") ?? "All"}",
                GeneratedAt = DateTime.UtcNow
            };

            // In production, this would query the database for actual metrics
            analytics.TotalReviews = 150;
            analytics.ApprovedReviews = 120;
            analytics.RejectedReviews = 20;
            analytics.PendingReviews = 10;
            analytics.AverageReviewTime = 45.5; // minutes
            analytics.ApprovalRate = 0.8;

            analytics.ReviewsByType = new Dictionary<string, int>
            {
                ["SqlValidation"] = 80,
                ["SemanticAlignment"] = 40,
                ["BusinessLogic"] = 20,
                ["SecurityReview"] = 10
            };

            analytics.ReviewsByPriority = new Dictionary<string, int>
            {
                ["Normal"] = 100,
                ["High"] = 30,
                ["Critical"] = 15,
                ["Urgent"] = 5
            };

            analytics.CommonIssues = new List<string>
            {
                "Incorrect table joins",
                "Missing WHERE clauses",
                "Performance concerns",
                "Business logic misalignment"
            };

            _logger.LogDebug("✅ Review analytics generated");
            return analytics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating review analytics");
            throw;
        }
    }

    /// <summary>
    /// Cancel a review request
    /// </summary>
    public async Task<bool> CancelReviewAsync(string reviewId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("❌ Cancelling review {ReviewId}: {Reason}", reviewId, reason);

            var reviewRequest = await GetReviewRequestAsync(reviewId, cancellationToken);
            if (reviewRequest == null)
            {
                return false;
            }

            reviewRequest.Status = ReviewStatus.Cancelled;
            reviewRequest.ReviewNotes = $"Cancelled: {reason}";
            reviewRequest.ReviewedAt = DateTime.UtcNow;

            await UpdateReviewRequestAsync(reviewRequest, cancellationToken);

            _logger.LogInformation("✅ Review cancelled successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error cancelling review");
            return false;
        }
    }

    /// <summary>
    /// Escalate a review to higher authority
    /// </summary>
    public async Task<bool> EscalateReviewAsync(string reviewId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("⬆️ Escalating review {ReviewId}: {Reason}", reviewId, reason);

            var reviewRequest = await GetReviewRequestAsync(reviewId, cancellationToken);
            if (reviewRequest == null)
            {
                return false;
            }

            reviewRequest.Status = ReviewStatus.Escalated;
            reviewRequest.Priority = ReviewPriority.High;

            await UpdateReviewRequestAsync(reviewRequest, cancellationToken);
            await _notificationService.SendEscalationNotificationAsync(reviewId, reason, cancellationToken);

            _logger.LogInformation("✅ Review escalated successfully");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error escalating review");
            return false;
        }
    }

    // Private helper methods (would be implemented with actual database operations)
    private async Task<string?> FindAvailableReviewerAsync(List<string> roles, CancellationToken cancellationToken)
    {
        // In production, this would query user database for available reviewers with specified roles
        await Task.Delay(1, cancellationToken);
        return roles.Any() ? "admin@company.com" : null;
    }

    private async Task StoreReviewRequestAsync(ReviewRequest request, CancellationToken cancellationToken)
    {
        // In production, this would store in database
        await Task.Delay(1, cancellationToken);
    }

    private async Task<ReviewRequest?> GetStoredReviewRequestAsync(string reviewId, CancellationToken cancellationToken)
    {
        // In production, this would query database
        await Task.Delay(1, cancellationToken);
        return null;
    }

    private async Task<List<ReviewQueueItem>> GetStoredReviewQueueAsync(
        string? userId, ReviewType? type, ReviewPriority? priority, int page, int pageSize, CancellationToken cancellationToken)
    {
        // In production, this would query database with proper filtering and pagination
        await Task.Delay(1, cancellationToken);
        return new List<ReviewQueueItem>();
    }

    private async Task UpdateReviewRequestAsync(ReviewRequest request, CancellationToken cancellationToken)
    {
        // In production, this would update database
        await Task.Delay(1, cancellationToken);
    }

    private async Task StoreFeedbackAsync(HumanFeedback feedback, CancellationToken cancellationToken)
    {
        // In production, this would store feedback in database
        await Task.Delay(1, cancellationToken);
    }

    private async Task StartApprovalWorkflowAsync(ReviewRequest request, ReviewTypeConfig config, CancellationToken cancellationToken)
    {
        // Create workflow steps based on configuration
        var steps = new List<ApprovalStep>();
        
        foreach (var role in config.RequiredRoles)
        {
            steps.Add(new ApprovalStep
            {
                Name = $"{role} Approval",
                Description = $"Approval required from {role}",
                AssignedRole = role,
                Order = steps.Count,
                Timeout = config.Timeout
            });
        }

        if (steps.Any())
        {
            await _workflowService.StartWorkflowAsync(request.Id, $"{request.Type}Workflow", steps, cancellationToken);
        }
    }
}
