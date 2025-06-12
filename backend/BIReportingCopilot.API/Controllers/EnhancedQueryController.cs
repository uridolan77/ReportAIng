using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Enhanced Query Controller - Provides advanced query features and user context
/// </summary>
[ApiController]
[Route("api/enhanced-query")]
[Authorize]
public class EnhancedQueryController : ControllerBase
{
    private readonly ILogger<EnhancedQueryController> _logger;
    private readonly IContextManager _contextManager;
    private readonly IQueryService _queryService;

    public EnhancedQueryController(
        ILogger<EnhancedQueryController> logger,
        IContextManager contextManager,
        IQueryService queryService)
    {
        _logger = logger;
        _contextManager = contextManager;
        _queryService = queryService;
    }

    /// <summary>
    /// Get user context for AI personalization
    /// </summary>
    /// <returns>User context with preferred tables, filters, and patterns</returns>
    [HttpGet("context")]
    public async Task<ActionResult<UserContextResponse>> GetUserContext()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting user context for user {UserId}", userId);

            // Get user context from context manager
            var userContext = await _contextManager.GetUserContextAsync(userId);
            
            // Get query patterns
            var queryPatterns = await _contextManager.GetQueryPatternsAsync(userId);

            // Map to response format
            var response = new UserContextResponse
            {
                Domain = userContext.Domain ?? "General",
                PreferredTables = userContext.PreferredTables ?? new List<string>(),
                CommonFilters = userContext.CommonFilters ?? new List<string>(),
                RecentPatterns = queryPatterns.Select(p => new QueryPatternResponse
                {
                    Pattern = p.Pattern,
                    Frequency = p.Frequency,
                    LastUsed = p.LastUsed.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    Intent = p.Intent.ToString(),
                    AssociatedTables = p.AssociatedTables ?? new List<string>()
                }).ToList(),
                LastUpdated = userContext.LastUpdated.ToString("yyyy-MM-ddTHH:mm:ssZ")
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user context");
            return StatusCode(500, new { error = "Error retrieving user context" });
        }
    }

    /// <summary>
    /// Find similar queries based on semantic analysis
    /// </summary>
    /// <param name="query">Query to find similar queries for</param>
    /// <param name="limit">Maximum number of similar queries to return</param>
    /// <returns>List of similar queries</returns>
    [HttpPost("similar")]
    public async Task<ActionResult<List<SimilarQueryResponse>>> FindSimilarQueries(
        [FromBody] string query, 
        [FromQuery] int limit = 5)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Finding similar queries for user {UserId}: {Query}", userId, query);

            // Get similar queries from query service
            var similarQueries = await _queryService.FindSimilarQueriesAsync(query, userId, limit);

            var response = similarQueries.Select(q => new SimilarQueryResponse
            {
                Sql = q.Sql,
                Explanation = q.Explanation,
                Confidence = q.Confidence,
                Classification = q.Classification.ToString()
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries");
            return StatusCode(500, new { error = "Error finding similar queries" });
        }
    }

    /// <summary>
    /// Calculate similarity between two queries
    /// </summary>
    /// <param name="request">Similarity calculation request</param>
    /// <returns>Similarity analysis</returns>
    [HttpPost("similarity")]
    public async Task<ActionResult<SimilarityResponse>> CalculateSimilarity([FromBody] SimilarityRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Calculating similarity for user {UserId}", userId);

            // Calculate similarity using query service
            var similarityScore = await _queryService.CalculateSemanticSimilarityAsync(request.Query1, request.Query2);

            var response = new SimilarityResponse
            {
                SimilarityScore = similarityScore,
                CommonEntities = new List<string>(), // TODO: Implement entity extraction
                CommonKeywords = new List<string>(), // TODO: Implement keyword extraction
                Analysis = $"Semantic similarity score: {similarityScore:F2}"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating query similarity");
            return StatusCode(500, new { error = "Error calculating query similarity" });
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value 
               ?? User.FindFirst("sub")?.Value 
               ?? User.Identity?.Name 
               ?? "anonymous";
    }
}

#region Response Models

/// <summary>
/// User context response for frontend
/// </summary>
public class UserContextResponse
{
    public string Domain { get; set; } = string.Empty;
    public List<string> PreferredTables { get; set; } = new();
    public List<string> CommonFilters { get; set; } = new();
    public List<QueryPatternResponse> RecentPatterns { get; set; } = new();
    public string LastUpdated { get; set; } = string.Empty;
}

/// <summary>
/// Query pattern response
/// </summary>
public class QueryPatternResponse
{
    public string Pattern { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public string LastUsed { get; set; } = string.Empty;
    public string Intent { get; set; } = string.Empty;
    public List<string> AssociatedTables { get; set; } = new();
}

/// <summary>
/// Similar query response
/// </summary>
public class SimilarQueryResponse
{
    public string Sql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Classification { get; set; } = string.Empty;
}

/// <summary>
/// Similarity calculation request
/// </summary>
public class SimilarityRequest
{
    public string Query1 { get; set; } = string.Empty;
    public string Query2 { get; set; } = string.Empty;
}

/// <summary>
/// Similarity calculation response
/// </summary>
public class SimilarityResponse
{
    public double SimilarityScore { get; set; }
    public List<string> CommonEntities { get; set; } = new();
    public List<string> CommonKeywords { get; set; } = new();
    public string Analysis { get; set; } = string.Empty;
}

#endregion
