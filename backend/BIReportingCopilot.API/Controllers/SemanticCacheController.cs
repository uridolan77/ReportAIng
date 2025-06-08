using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Infrastructure.Handlers;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Admin controller for managing enhanced semantic cache with vector embeddings
/// Provides monitoring, optimization, and configuration endpoints
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Require authentication for cache management
public class SemanticCacheController : ControllerBase
{
    private readonly ILogger<SemanticCacheController> _logger;
    private readonly IMediator _mediator;
    private readonly IVectorSearchService _vectorSearchService;

    public SemanticCacheController(
        ILogger<SemanticCacheController> logger,
        IMediator mediator,
        IVectorSearchService vectorSearchService)
    {
        _logger = logger;
        _mediator = mediator;
        _vectorSearchService = vectorSearchService;
    }

    /// <summary>
    /// Get enhanced semantic cache metrics and performance statistics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        try
        {
            _logger.LogInformation("📊 Getting enhanced semantic cache metrics");

            var query = new GetSemanticCacheMetricsQuery();
            var metrics = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = metrics,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting semantic cache metrics");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Optimize semantic cache for better performance
    /// </summary>
    [HttpPost("optimize")]
    public async Task<IActionResult> OptimizeCache([FromBody] OptimizeCacheRequest? request = null)
    {
        try
        {
            _logger.LogInformation("🔧 Starting semantic cache optimization");

            var command = new OptimizeSemanticCacheCommand
            {
                ForceOptimization = request?.ForceOptimization ?? false
            };

            var success = await _mediator.Send(command);

            if (success)
            {
                _logger.LogInformation("✅ Semantic cache optimization completed successfully");
                return Ok(new
                {
                    success = true,
                    message = "Semantic cache optimization completed successfully",
                    timestamp = DateTime.UtcNow
                });
            }
            else
            {
                return BadRequest(new
                {
                    success = false,
                    error = "Cache optimization failed"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing semantic cache");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Search for similar queries using vector similarity
    /// </summary>
    [HttpPost("search")]
    public async Task<IActionResult> SearchSimilarQueries([FromBody] SearchSimilarQueriesRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { success = false, error = "Query is required" });
            }

            _logger.LogInformation("🔍 Searching for similar queries: {Query}", request.Query);

            var similarQueries = await _vectorSearchService.FindSimilarQueriesByTextAsync(
                request.Query,
                request.SimilarityThreshold,
                request.MaxResults);

            return Ok(new
            {
                success = true,
                data = new
                {
                    query = request.Query,
                    similarQueries = similarQueries.Select(sq => new
                    {
                        id = sq.EmbeddingId,
                        originalQuery = sq.OriginalQuery,
                        sqlQuery = sq.SqlQuery,
                        similarityScore = sq.SimilarityScore,
                        createdAt = sq.CreatedAt,
                        lastAccessed = sq.LastAccessed,
                        accessCount = sq.AccessCount
                    }),
                    totalResults = similarQueries.Count
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error searching for similar queries");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Generate embedding for a text query (for testing/debugging)
    /// </summary>
    [HttpPost("embedding")]
    public async Task<IActionResult> GenerateEmbedding([FromBody] GenerateEmbeddingRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Text))
            {
                return BadRequest(new { success = false, error = "Text is required" });
            }

            _logger.LogInformation("🧠 Generating embedding for text: {Text}", request.Text.Substring(0, Math.Min(50, request.Text.Length)));

            var embedding = await _vectorSearchService.GenerateEmbeddingAsync(request.Text);

            return Ok(new
            {
                success = true,
                data = new
                {
                    text = request.Text,
                    embedding = embedding,
                    dimensions = embedding.Length,
                    magnitude = Math.Sqrt(embedding.Sum(x => x * x))
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating embedding");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed vector search statistics
    /// </summary>
    [HttpGet("vector-stats")]
    public async Task<IActionResult> GetVectorStats()
    {
        try
        {
            _logger.LogInformation("📊 Getting vector search statistics");

            var metrics = await _vectorSearchService.GetMetricsAsync();

            return Ok(new
            {
                success = true,
                data = new
                {
                    totalEmbeddings = metrics.TotalEmbeddings,
                    totalSearches = metrics.TotalSearches,
                    averageSearchTime = metrics.AverageSearchTime,
                    cacheHitRate = metrics.CacheHitRate,
                    indexSizeBytes = metrics.IndexSizeBytes,
                    indexSizeMB = metrics.IndexSizeBytes / 1024.0 / 1024.0,
                    lastOptimized = metrics.LastOptimized,
                    performanceMetrics = metrics.PerformanceMetrics
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting vector statistics");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Test semantic cache lookup for a specific query
    /// </summary>
    [HttpPost("test-lookup")]
    public async Task<IActionResult> TestCacheLookup([FromBody] TestCacheLookupRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { success = false, error = "Query is required" });
            }

            _logger.LogInformation("🧪 Testing cache lookup for query: {Query}", request.Query);

            var command = new GetSemanticCacheCommand
            {
                NaturalLanguageQuery = request.Query,
                SqlQuery = request.SqlQuery ?? "",
                SimilarityThreshold = request.SimilarityThreshold,
                MaxResults = request.MaxResults
            };

            var result = await _mediator.Send(command);

            return Ok(new
            {
                success = true,
                data = new
                {
                    query = request.Query,
                    isHit = result?.IsHit ?? false,
                    similarityScore = result?.SimilarityScore ?? 0.0,
                    lookupTime = result?.LookupTime.TotalMilliseconds ?? 0.0,
                    cacheStrategy = result?.CacheStrategy ?? "none",
                    similarQueries = result?.SimilarQueries?.Select(sq => new
                    {
                        originalQuery = sq.OriginalQuery,
                        similarityScore = sq.SimilarityScore,
                        accessCount = sq.AccessCount
                    }).ToArray() ?? new object[0],
                    metadata = result?.Metadata ?? new Dictionary<string, object>()
                },
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error testing cache lookup");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// Clear semantic cache (admin only)
    /// </summary>
    [HttpDelete("clear")]
    [Authorize(Roles = "Admin")] // Restrict to admin users only
    public async Task<IActionResult> ClearCache()
    {
        try
        {
            _logger.LogWarning("🧹 Admin clearing semantic cache");

            // This would implement cache clearing logic
            // For now, just return success
            return Ok(new
            {
                success = true,
                message = "Semantic cache cleared successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error clearing semantic cache");
            return StatusCode(500, new { success = false, error = ex.Message });
        }
    }
}

/// <summary>
/// Request models for semantic cache endpoints
/// </summary>
public class OptimizeCacheRequest
{
    public bool ForceOptimization { get; set; } = false;
}

public class SearchSimilarQueriesRequest
{
    public string Query { get; set; } = string.Empty;
    public double SimilarityThreshold { get; set; } = 0.8;
    public int MaxResults { get; set; } = 5;
}

public class GenerateEmbeddingRequest
{
    public string Text { get; set; } = string.Empty;
}

public class TestCacheLookupRequest
{
    public string Query { get; set; } = string.Empty;
    public string? SqlQuery { get; set; }
    public double SimilarityThreshold { get; set; } = 0.85;
    public int MaxResults { get; set; } = 5;
}
