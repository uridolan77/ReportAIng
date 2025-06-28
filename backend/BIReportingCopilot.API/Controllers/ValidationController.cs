using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Validation;
using BIReportingCopilot.Core.Interfaces.Review;
using BIReportingCopilot.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Validation Controller - Comprehensive SQL validation and human review workflows
/// </summary>
[ApiController]
[Route("api/validation")]
[Authorize]
[Produces("application/json")]
public class ValidationController : ControllerBase
{
    private readonly ILogger<ValidationController> _logger;
    private readonly IEnhancedSemanticSqlValidator _enhancedValidator;
    private readonly IDryRunExecutionService _dryRunService;
    private readonly ISqlSelfCorrectionService _selfCorrectionService;
    private readonly IHumanReviewService _reviewService;
    private readonly IHumanFeedbackService _feedbackService;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly IReviewNotificationService _notificationService;

    public ValidationController(
        ILogger<ValidationController> logger,
        IEnhancedSemanticSqlValidator enhancedValidator,
        IDryRunExecutionService dryRunService,
        ISqlSelfCorrectionService selfCorrectionService,
        IHumanReviewService reviewService,
        IHumanFeedbackService feedbackService,
        IApprovalWorkflowService workflowService,
        IReviewNotificationService notificationService)
    {
        _logger = logger;
        _enhancedValidator = enhancedValidator;
        _dryRunService = dryRunService;
        _selfCorrectionService = selfCorrectionService;
        _reviewService = reviewService;
        _feedbackService = feedbackService;
        _workflowService = workflowService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Comprehensive SQL validation with semantic analysis and self-correction
    /// </summary>
    /// <param name="request">Enhanced validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced validation response</returns>
    [HttpPost("comprehensive")]
    [ProducesResponseType(typeof(EnhancedValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EnhancedValidationResponse>> ValidateComprehensive(
        [FromBody] EnhancedValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Starting comprehensive SQL validation for query: {Query}", request.OriginalQuery);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _enhancedValidator.ValidateWithLevelAsync(request, cancellationToken);

            _logger.LogInformation("✅ Comprehensive validation completed: {Success} (Score: {Score:F2})", 
                response.Success, response.ValidationResult?.OverallScore ?? 0);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for comprehensive validation");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in comprehensive SQL validation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "An error occurred during comprehensive SQL validation",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Semantic validation focusing on business intent alignment
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Semantic validation result</returns>
    [HttpPost("semantic")]
    [ProducesResponseType(typeof(SemanticValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SemanticValidationResult>> ValidateSemantic(
        [FromBody] string sql,
        [FromQuery, Required] string originalQuery,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🧠 Starting semantic validation");

            if (string.IsNullOrWhiteSpace(sql) || string.IsNullOrWhiteSpace(originalQuery))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Input",
                    Detail = "Both SQL and original query are required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await _enhancedValidator.ValidateSemanticAlignmentAsync(sql, originalQuery, cancellationToken);

            _logger.LogInformation("✅ Semantic validation completed: {Valid} (Score: {Score:F2})", 
                result.IsValid, result.AlignmentScore);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in semantic validation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Semantic Validation Error",
                Detail = "An error occurred during semantic validation",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Business logic validation for compliance and rules
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business logic validation result</returns>
    [HttpPost("business-logic")]
    [ProducesResponseType(typeof(BusinessLogicValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BusinessLogicValidationResult>> ValidateBusinessLogic(
        [FromBody] string sql,
        [FromQuery, Required] string originalQuery,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("💼 Starting business logic validation");

            if (string.IsNullOrWhiteSpace(sql) || string.IsNullOrWhiteSpace(originalQuery))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Input",
                    Detail = "Both SQL and original query are required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await _enhancedValidator.ValidateBusinessLogicAsync(sql, originalQuery, cancellationToken);

            _logger.LogInformation("✅ Business logic validation completed: {Valid} (Score: {Score:F2})",
                result.IsValid, result.ComplianceScore);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in business logic validation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Business Logic Validation Error",
                Detail = "An error occurred during business logic validation",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Dry-run execution for SQL validation
    /// </summary>
    /// <param name="request">Dry-run execution request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dry-run execution result</returns>
    [HttpPost("dry-run")]
    [ProducesResponseType(typeof(DryRunExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DryRunExecutionResult>> ExecuteDryRun(
        [FromBody] DryRunExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Starting dry-run execution");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _dryRunService.ExecuteDryRunAsync(
                request.Sql, 
                request.MaxRowsToAnalyze, 
                request.MaxExecutionTime, 
                cancellationToken);

            _logger.LogInformation("✅ Dry-run execution completed: {Success}", result.ExecutedSuccessfully);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for dry-run execution");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in dry-run execution");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Dry-Run Execution Error",
                Detail = "An error occurred during dry-run execution",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// SQL self-correction based on validation issues
    /// </summary>
    /// <param name="sql">Original SQL query</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="validationIssues">List of validation issues to address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Self-correction attempt result</returns>
    [HttpPost("self-correct")]
    [ProducesResponseType(typeof(SelfCorrectionAttempt), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SelfCorrectionAttempt>> SelfCorrectSql(
        [FromBody] string sql,
        [FromQuery, Required] string originalQuery,
        [FromQuery] List<string> validationIssues,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Starting SQL self-correction for {IssueCount} issues", validationIssues.Count);

            if (string.IsNullOrWhiteSpace(sql) || string.IsNullOrWhiteSpace(originalQuery))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Input",
                    Detail = "Both SQL and original query are required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await _selfCorrectionService.CorrectSqlAsync(
                sql, originalQuery, validationIssues, cancellationToken);

            _logger.LogInformation("✅ Self-correction completed: {Success} (Score: {Score:F2})", 
                result.WasSuccessful, result.ImprovementScore);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for self-correction");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SQL self-correction");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Self-Correction Error",
                Detail = "An error occurred during SQL self-correction",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get validation metrics for monitoring and optimization
    /// </summary>
    /// <param name="timeRangeHours">Time range in hours for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation metrics</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ValidationMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ValidationMetrics>> GetValidationMetrics(
        [FromQuery] int timeRangeHours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📊 Getting validation metrics for {Hours} hours", timeRangeHours);

            var timeRange = TimeSpan.FromHours(timeRangeHours);
            var metrics = await _enhancedValidator.GetValidationMetricsAsync(timeRange, cancellationToken);

            _logger.LogInformation("✅ Retrieved validation metrics: {TotalValidations} validations", 
                metrics.TotalValidations);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting validation metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Metrics Error",
                Detail = "An error occurred while retrieving validation metrics",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get correction patterns and statistics
    /// </summary>
    /// <param name="timeRangeHours">Time range in hours for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Correction patterns and statistics</returns>
    [HttpGet("correction-patterns")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Dictionary<string, object>>> GetCorrectionPatterns(
        [FromQuery] int timeRangeHours = 168, // Default 1 week
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📈 Getting correction patterns for {Hours} hours", timeRangeHours);

            var timeRange = TimeSpan.FromHours(timeRangeHours);
            var patterns = await _selfCorrectionService.GetCorrectionPatternsAsync(timeRange, cancellationToken);

            _logger.LogInformation("✅ Retrieved correction patterns");

            return Ok(patterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting correction patterns");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Patterns Error",
                Detail = "An error occurred while retrieving correction patterns",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    #region Human Review Workflows

    /// <summary>
    /// Submit a query for human review
    /// </summary>
    [HttpPost("review/submit")]
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
    [HttpGet("review/{reviewId}")]
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
    [HttpGet("review/queue")]
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
    [HttpPost("review/{reviewId}/assign")]
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
    [HttpPost("review/{reviewId}/complete")]
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
    [HttpGet("review/analytics")]
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
    [HttpPost("review/{reviewId}/cancel")]
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
    [HttpPost("review/{reviewId}/escalate")]
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
    [HttpGet("review/{reviewId}/feedback")]
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
    [HttpGet("review/notifications")]
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
    [HttpPost("review/notifications/{notificationId}/read")]
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

    #endregion
}

// Request/Response DTOs for Human Review
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
