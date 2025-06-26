using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Commands;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Query history and feedback management controller
/// </summary>
[ApiController]
[Route("api/query")]
[Authorize]
public class QueryHistoryController : ControllerBase
{
    private readonly ILogger<QueryHistoryController> _logger;
    private readonly IQueryService _queryService;

    public QueryHistoryController(
        ILogger<QueryHistoryController> logger,
        IQueryService queryService)
    {
        _logger = logger;
        _queryService = queryService;
    }

    /// <summary>
    /// Get query history for the current user
    /// </summary>
    [HttpGet("history")]
    public async Task<ActionResult<BIReportingCopilot.Core.Commands.PagedResult<QueryHistoryItem>>> GetQueryHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? searchTerm = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Getting query history for user {UserId}, page {Page}, pageSize {PageSize}", 
                userId, page, pageSize);

            var result = await _queryService.GetQueryHistoryAsync(userId, pageSize, page);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query history");
            return StatusCode(500, new { error = "Failed to retrieve query history" });
        }
    }

    /// <summary>
    /// Submit feedback for a query
    /// </summary>
    [HttpPost("feedback")]
    public async Task<ActionResult> SubmitQueryFeedback([FromBody] QueryFeedback feedback)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            feedback.SubmittedAt = DateTime.UtcNow;

            _logger.LogInformation("Submitting feedback for query {QueryId} from user {UserId}",
                feedback.QueryId, userId);

            await _queryService.SubmitFeedbackAsync(feedback, userId);
            
            return Ok(new { message = "Feedback submitted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting query feedback for query {QueryId}", feedback.QueryId);
            return StatusCode(500, new { error = "Failed to submit feedback" });
        }
    }

    /// <summary>
    /// Get user context information
    /// </summary>
    [HttpGet("context")]
    public async Task<ActionResult<object>> GetUserContext()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            _logger.LogInformation("Getting user context for user {UserId}", userId);

            // Get recent queries for context
            var recentQueries = await _queryService.GetQueryHistoryAsync(userId, 10, 1);

            var context = new
            {
                UserId = userId,
                RecentQueries = recentQueries.Take(10).Select(q => q.Query ?? "").ToList(),
                PreferredDataSources = new List<string>(),
                CommonFilters = new Dictionary<string, object>(),
                LastActivity = recentQueries.FirstOrDefault()?.ExecutedAt ?? DateTime.UtcNow
            };

            return Ok(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user context");
            return StatusCode(500, new { error = "Failed to get user context" });
        }
    }

    /// <summary>
    /// Find similar queries to the provided query
    /// </summary>
    [HttpPost("similar")]
    public async Task<ActionResult<List<object>>> FindSimilarQueries(
        [FromBody] string query,
        [FromQuery] int limit = 5)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

            _logger.LogInformation("Finding similar queries for user {UserId}", userId);

            // For now, return a simple mock response since the method doesn't exist
            var response = new List<object>
            {
                new
                {
                    Query = "Sample similar query",
                    SimilarityScore = 0.85,
                    ExecutedAt = DateTime.UtcNow.AddDays(-1),
                    ExecutionTime = TimeSpan.FromSeconds(2),
                    Success = true
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries");
            return StatusCode(500, new { error = "Failed to find similar queries" });
        }
    }

    /// <summary>
    /// Calculate similarity between two queries
    /// </summary>
    [HttpPost("similarity")]
    public async Task<ActionResult<object>> CalculateSimilarity([FromBody] object request)
    {
        try
        {
            _logger.LogInformation("Calculating similarity between queries");

            // Mock similarity calculation
            var similarity = 0.75; // Mock value

            var response = new
            {
                SimilarityScore = similarity,
                IsHighSimilarity = similarity > 0.8,
                ComparedAt = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating query similarity");
            return StatusCode(500, new { error = "Failed to calculate similarity" });
        }
    }
}

// These response models are already defined elsewhere
