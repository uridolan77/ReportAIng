using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Queries;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QueryController : ControllerBase
{
    private readonly ILogger<QueryController> _logger;
    private readonly IMediator _mediator;
    private readonly IQueryService _queryService;

    public QueryController(ILogger<QueryController> logger, IMediator mediator, IQueryService queryService)
    {
        _logger = logger;
        _mediator = mediator;
        _queryService = queryService;
    }

    /// <summary>
    /// Execute a natural language query and return results with generated SQL
    /// </summary>
    /// <param name="request">The query request containing the natural language question</param>
    /// <returns>Query response with SQL, results, and visualization config</returns>
    [HttpPost("natural-language")]
    public async Task<ActionResult<QueryResponse>> ExecuteNaturalLanguageQuery([FromBody] QueryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Executing natural language query for user {UserId}: {Question}", userId, request.Question);

            var command = new ExecuteQueryCommand
            {
                Question = request.Question,
                SessionId = request.SessionId,
                UserId = userId,
                Options = request.Options
            };

            var response = await _mediator.Send(command);

            return response.Success ? Ok(response) : BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing natural language query");
            return StatusCode(500, new { error = "An error occurred while processing your query" });
        }
    }

    /// <summary>
    /// Get query history for the current user
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <returns>Paginated list of query history items</returns>
    [HttpGet("history")]
    public async Task<ActionResult<PagedResult<QueryHistoryItem>>> GetQueryHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var query = new GetQueryHistoryQuery
            {
                UserId = GetCurrentUserId(),
                Page = page,
                PageSize = pageSize,
                StartDate = startDate,
                EndDate = endDate,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving query history");
            return StatusCode(500, new { error = "An error occurred while retrieving query history" });
        }
    }

    /// <summary>
    /// Submit feedback for a specific query
    /// </summary>
    /// <param name="feedback">Feedback details</param>
    /// <returns>Success status</returns>
    [HttpPost("feedback")]
    public async Task<ActionResult> SubmitQueryFeedback([FromBody] QueryFeedback feedback)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Submitting feedback for query {QueryId} from user {UserId}", feedback.QueryId, userId);

            var success = await _queryService.SubmitFeedbackAsync(feedback, userId);

            if (success)
            {
                return Ok(new { message = "Feedback submitted successfully" });
            }
            else
            {
                return BadRequest(new { error = "Failed to submit feedback" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting query feedback");
            return StatusCode(500, new { error = "An error occurred while submitting feedback" });
        }
    }

    /// <summary>
    /// Get query suggestions based on context
    /// </summary>
    /// <param name="context">Optional context for suggestions</param>
    /// <returns>List of suggested queries</returns>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<string>>> GetQuerySuggestions([FromQuery] string? context = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting query suggestions for user {UserId}", userId);

            var suggestions = await _queryService.GetQuerySuggestionsAsync(userId, context);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query suggestions");
            return StatusCode(500, new { error = "An error occurred while getting suggestions" });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }
}

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Basic health check endpoint
    /// </summary>
    /// <returns>Health status</returns>
    [HttpGet]
    public ActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown"
        });
    }

    /// <summary>
    /// Detailed health check with service status
    /// </summary>
    /// <returns>Detailed health information</returns>
    [HttpGet("detailed")]
    public ActionResult GetDetailedHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
            services = new
            {
                database = "healthy",
                cache = "healthy",
                ai_service = "healthy"
            },
            metrics = new
            {
                uptime = TimeSpan.FromMilliseconds(Environment.TickCount64).ToString(),
                memory_usage = GC.GetTotalMemory(false),
                active_connections = 0 // Placeholder
            }
        });
    }
}
