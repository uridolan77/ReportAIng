using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Review;
using BIReportingCopilot.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Phase 4: Human-in-Loop Review API Controller
/// Provides endpoints for managing human review workflows
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class HumanReviewController : ControllerBase
{
    private readonly ILogger<HumanReviewController> _logger;
    private readonly IHumanReviewService _reviewService;
    private readonly IHumanFeedbackService _feedbackService;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly IReviewNotificationService _notificationService;

    public HumanReviewController(
        ILogger<HumanReviewController> logger,
        IHumanReviewService reviewService,
        IHumanFeedbackService feedbackService,
        IApprovalWorkflowService workflowService,
        IReviewNotificationService notificationService)
    {
        _logger = logger;
        _reviewService = reviewService;
        _feedbackService = feedbackService;
        _workflowService = workflowService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Submit a query for human review
    /// </summary>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(ReviewRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ReviewRequest>> SubmitForReview(
        [FromBody] SubmitReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📝 Submitting query for review: {Type}", request.Type);

            var userId = User.Identity?.Name ?? "unknown";
            
            var reviewRequest = await _reviewService.SubmitForReviewAsync(
                request.OriginalQuery,
                request.GeneratedSql,
                request.Type,
                userId,
                request.Priority,
                cancellationToken);

            return Ok(reviewRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error submitting review request");
            return Problem("Failed to submit review request", statusCode: 500);
        }
    }

    /// <summary>
    /// Get review request by ID
    /// </summary>
    [HttpGet("{reviewId}")]
    [ProducesResponseType(typeof(ReviewRequest), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReviewRequest>> GetReviewRequest(
        string reviewId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var reviewRequest = await _reviewService.GetReviewRequestAsync(reviewId, cancellationToken);
            
            if (reviewRequest == null)
            {
                return NotFound($"Review request {reviewId} not found");
            }

            return Ok(reviewRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting review request: {ReviewId}", reviewId);
            return Problem("Failed to get review request", statusCode: 500);
        }
    }

    /// <summary>
    /// Get pending reviews queue
    /// </summary>
    [HttpGet("queue")]
    [ProducesResponseType(typeof(ReviewQueueResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewQueueResponse>> GetReviewQueue(
        [FromQuery] string? userId = null,
        [FromQuery] ReviewType? type = null,
        [FromQuery] ReviewPriority? priority = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var currentUserId = User.Identity?.Name ?? "unknown";
            var targetUserId = userId ?? currentUserId;

            var reviews = await _reviewService.GetPendingReviewsAsync(
                targetUserId, type, priority, page, pageSize, cancellationToken);

            var response = new ReviewQueueResponse
            {
                Reviews = reviews,
                Page = page,
                PageSize = pageSize,
                TotalCount = reviews.Count // In production, get actual total count
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting review queue");
            return Problem("Failed to get review queue", statusCode: 500);
        }
    }

    /// <summary>
    /// Assign review to a user
    /// </summary>
    [HttpPost("{reviewId}/assign")]
    [Authorize(Roles = "Admin,ReviewManager")]
    [ProducesResponseType(typeof(AssignmentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AssignmentResult>> AssignReview(
        string reviewId,
        [FromBody] AssignReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _reviewService.AssignReviewAsync(reviewId, request.AssignedTo, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Review request {reviewId} not found");
            }

            return Ok(new AssignmentResult { Success = true, ReviewId = reviewId, AssignedTo = request.AssignedTo });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error assigning review: {ReviewId}", reviewId);
            return Problem("Failed to assign review", statusCode: 500);
        }
    }

    /// <summary>
    /// Complete a review with feedback
    /// </summary>
    [HttpPost("{reviewId}/complete")]
    [ProducesResponseType(typeof(CompletionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CompletionResult>> CompleteReview(
        string reviewId,
        [FromBody] CompleteReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.Identity?.Name ?? "unknown";
            
            var feedback = new HumanFeedback
            {
                ReviewRequestId = reviewId,
                ReviewerId = userId,
                Type = request.FeedbackType,
                Action = request.Action,
                CorrectedSql = request.CorrectedSql,
                Comments = request.Comments,
                QualityRating = request.QualityRating,
                IsApproved = request.Action == FeedbackAction.Approve
            };

            var success = await _reviewService.CompleteReviewAsync(reviewId, feedback, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Review request {reviewId} not found");
            }

            return Ok(new CompletionResult { Success = true, ReviewId = reviewId, Action = request.Action });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error completing review: {ReviewId}", reviewId);
            return Problem("Failed to complete review", statusCode: 500);
        }
    }

    /// <summary>
    /// Get review analytics
    /// </summary>
    [HttpGet("analytics")]
    [Authorize(Roles = "Admin,ReviewManager")]
    [ProducesResponseType(typeof(ReviewAnalytics), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReviewAnalytics>> GetReviewAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var analytics = await _reviewService.GetReviewAnalyticsAsync(startDate, endDate, userId, cancellationToken);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting review analytics");
            return Problem("Failed to get review analytics", statusCode: 500);
        }
    }

    /// <summary>
    /// Cancel a review request
    /// </summary>
    [HttpPost("{reviewId}/cancel")]
    [ProducesResponseType(typeof(CancellationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CancellationResult>> CancelReview(
        string reviewId,
        [FromBody] CancelReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _reviewService.CancelReviewAsync(reviewId, request.Reason, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Review request {reviewId} not found");
            }

            return Ok(new CancellationResult { Success = true, ReviewId = reviewId, Reason = request.Reason });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error cancelling review: {ReviewId}", reviewId);
            return Problem("Failed to cancel review", statusCode: 500);
        }
    }

    /// <summary>
    /// Escalate a review to higher authority
    /// </summary>
    [HttpPost("{reviewId}/escalate")]
    [ProducesResponseType(typeof(EscalationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<EscalationResult>> EscalateReview(
        string reviewId,
        [FromBody] EscalateReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _reviewService.EscalateReviewAsync(reviewId, request.Reason, cancellationToken);
            
            if (!success)
            {
                return NotFound($"Review request {reviewId} not found");
            }

            return Ok(new EscalationResult { Success = true, ReviewId = reviewId, Reason = request.Reason });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error escalating review: {ReviewId}", reviewId);
            return Problem("Failed to escalate review", statusCode: 500);
        }
    }

    /// <summary>
    /// Get feedback for a review
    /// </summary>
    [HttpGet("{reviewId}/feedback")]
    [ProducesResponseType(typeof(List<HumanFeedback>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<HumanFeedback>>> GetReviewFeedback(
        string reviewId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var feedback = await _feedbackService.GetFeedbackAsync(reviewId, cancellationToken);
            return Ok(feedback);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting review feedback: {ReviewId}", reviewId);
            return Problem("Failed to get review feedback", statusCode: 500);
        }
    }

    /// <summary>
    /// Get user notifications
    /// </summary>
    [HttpGet("notifications")]
    [ProducesResponseType(typeof(List<ReviewNotification>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ReviewNotification>>> GetNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.Identity?.Name ?? "unknown";
            var notifications = await _notificationService.GetNotificationsAsync(
                userId, unreadOnly, page, pageSize, cancellationToken);
            
            return Ok(notifications);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting notifications");
            return Problem("Failed to get notifications", statusCode: 500);
        }
    }

    /// <summary>
    /// Mark notification as read
    /// </summary>
    [HttpPost("notifications/{notificationId}/read")]
    [ProducesResponseType(typeof(NotificationResult), StatusCodes.Status200OK)]
    public async Task<ActionResult<NotificationResult>> MarkNotificationAsRead(
        string notificationId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var success = await _notificationService.MarkNotificationAsReadAsync(notificationId, cancellationToken);
            return Ok(new NotificationResult { Success = success, NotificationId = notificationId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error marking notification as read: {NotificationId}", notificationId);
            return Problem("Failed to mark notification as read", statusCode: 500);
        }
    }
}

// Request/Response DTOs
public class SubmitReviewRequest
{
    [Required]
    public string OriginalQuery { get; set; } = string.Empty;
    
    [Required]
    public string GeneratedSql { get; set; } = string.Empty;
    
    [Required]
    public ReviewType Type { get; set; }
    
    public ReviewPriority Priority { get; set; } = ReviewPriority.Normal;
}

public class AssignReviewRequest
{
    [Required]
    public string AssignedTo { get; set; } = string.Empty;
}

public class CompleteReviewRequest
{
    [Required]
    public FeedbackType FeedbackType { get; set; }
    
    [Required]
    public FeedbackAction Action { get; set; }
    
    public string? CorrectedSql { get; set; }
    public string? Comments { get; set; }
    
    [Range(1, 5)]
    public int QualityRating { get; set; } = 3;
}

public class CancelReviewRequest
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}

public class EscalateReviewRequest
{
    [Required]
    public string Reason { get; set; } = string.Empty;
}

// Response DTOs
public class ReviewQueueResponse
{
    public List<ReviewQueueItem> Reviews { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public class AssignmentResult
{
    public bool Success { get; set; }
    public string ReviewId { get; set; } = string.Empty;
    public string AssignedTo { get; set; } = string.Empty;
}

public class CompletionResult
{
    public bool Success { get; set; }
    public string ReviewId { get; set; } = string.Empty;
    public FeedbackAction Action { get; set; }
}

public class CancellationResult
{
    public bool Success { get; set; }
    public string ReviewId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class EscalationResult
{
    public bool Success { get; set; }
    public string ReviewId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
}

public class NotificationResult
{
    public bool Success { get; set; }
    public string NotificationId { get; set; } = string.Empty;
}
