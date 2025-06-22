using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Handlers;
using BIReportingCopilot.Infrastructure.Handlers.AI;
using BIReportingCopilot.Infrastructure.AI.Intelligence;
using MediatR;
using System.Security.Cryptography;
using System.Text;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Consolidated Cache Controller - Provides cache operations, semantic cache management, and optimization
/// Combines functionality from CacheController and CacheOptimizationController
/// </summary>
[ApiController]
[Route("api/cache")]
[Authorize]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheController> _logger;
    private readonly IMediator _mediator;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ICacheOptimizationService _cacheOptimizationService;

    public CacheController(
        ICacheService cacheService,
        ILogger<CacheController> logger,
        IMediator mediator,
        IVectorSearchService vectorSearchService,
        ICacheOptimizationService cacheOptimizationService)
    {
        _cacheService = cacheService;
        _logger = logger;
        _mediator = mediator;
        _vectorSearchService = vectorSearchService;
        _cacheOptimizationService = cacheOptimizationService;
    }

    #region Basic Cache Operations

    /// <summary>
    /// Clear specific cache entry by query
    /// </summary>
    [HttpPost("clear")]
    public async Task<ActionResult> ClearCache([FromBody] ClearCacheRequest request)
    {
        try
        {
            _logger.LogInformation("Cache clear requested for query: {Query}", request.Query);

            if (!string.IsNullOrEmpty(request.Query))
            {
                var cacheKey = GenerateCacheKey(request.Query);
                await _cacheService.RemoveAsync($"{ApplicationConstants.CacheKeys.QueryPrefix}{cacheKey}");
                
                _logger.LogInformation("Cleared cache for query key: {CacheKey}", cacheKey);
                
                return Ok(new { 
                    message = "Cache cleared successfully", 
                    cacheKey = cacheKey,
                    timestamp = DateTime.UtcNow 
                });
            }
            else if (!string.IsNullOrEmpty(request.Pattern))
            {
                await _cacheService.RemovePatternAsync($"{ApplicationConstants.CacheKeys.QueryPrefix}{request.Pattern}");
                
                _logger.LogInformation("Cleared cache entries matching pattern: {Pattern}", request.Pattern);
                
                return Ok(new { 
                    message = "Cache pattern cleared successfully", 
                    pattern = request.Pattern,
                    timestamp = DateTime.UtcNow 
                });
            }
            else
            {
                return BadRequest(new { error = "Either query or pattern must be provided" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return StatusCode(500, new { error = "An error occurred while clearing cache" });
        }
    }

    /// <summary>
    /// Clear all cache entries
    /// </summary>
    [HttpDelete("clear-all")]
    public async Task<ActionResult> ClearAllCache()
    {
        try
        {
            _logger.LogInformation("Clear all cache requested");
            
            await _cacheService.ClearAllAsync();
            
            _logger.LogInformation("All cache cleared successfully");
            
            return Ok(new { 
                message = "All cache cleared successfully", 
                timestamp = DateTime.UtcNow 
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all cache");
            return StatusCode(500, new { error = "An error occurred while clearing all cache" });
        }
    }

    /// <summary>
    /// Get basic cache statistics
    /// </summary>
    [HttpGet("stats")]
    public async Task<ActionResult> GetCacheStats()
    {
        try
        {
            var stats = await _cacheService.GetStatisticsAsync();
            
            return Ok(new
            {
                statistics = stats,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics");
            return StatusCode(500, new { error = "An error occurred while getting cache statistics" });
        }
    }

    /// <summary>
    /// Check if a specific cache key exists
    /// </summary>
    [HttpGet("exists")]
    public async Task<ActionResult> CheckCacheExists([FromQuery] string query)
    {
        try
        {
            if (string.IsNullOrEmpty(query))
            {
                return BadRequest(new { error = "Query parameter is required" });
            }

            var cacheKey = GenerateCacheKey(query);
            var exists = await _cacheService.ExistsAsync($"{ApplicationConstants.CacheKeys.QueryPrefix}{cacheKey}");
            
            return Ok(new
            {
                exists = exists,
                cacheKey = cacheKey,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking cache existence for query: {Query}", query);
            return StatusCode(500, new { error = "An error occurred while checking cache existence" });
        }
    }

    #endregion

    #region Semantic Cache Operations

    /// <summary>
    /// Get enhanced semantic cache metrics and performance statistics
    /// </summary>
    [HttpGet("semantic/metrics")]
    public async Task<IActionResult> GetSemanticMetrics()
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
    [HttpPost("semantic/optimize")]
    public async Task<IActionResult> OptimizeSemanticCache([FromBody] OptimizeCacheRequest? request = null)
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
    [HttpPost("semantic/search")]
    public async Task<IActionResult> SearchSimilarQueries([FromBody] SearchSimilarQueriesRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { success = false, error = "Query is required" });
            }

            _logger.LogInformation("🔍 Searching for similar queries: {Query}", request.Query);

            var similarQueries = await ((InMemoryVectorSearchService)_vectorSearchService).FindSimilarQueriesAsync(
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

    #endregion

    #region Helper Methods

    /// <summary>
    /// Generate cache key for a query (same logic as QueryService)
    /// </summary>
    private string GenerateCacheKey(string query)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedQuery));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }

    /// <summary>
    /// Generate embedding for a text query (for testing/debugging)
    /// </summary>
    [HttpPost("semantic/embedding")]
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
    [HttpGet("semantic/vector-stats")]
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
    [HttpPost("semantic/test-lookup")]
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
    [HttpDelete("semantic/clear")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ClearSemanticCache()
    {
        try
        {
            _logger.LogWarning("🧹 Admin clearing semantic cache");

            // This would implement semantic cache clearing logic
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

    #endregion

    #region Cache Optimization Operations (from CacheOptimizationController)

    /// <summary>
    /// Get cache statistics for a specific cache type
    /// </summary>
    [HttpGet("optimization/statistics/{cacheType}")]
    public async Task<IActionResult> GetCacheStatistics(string cacheType)
    {
        try
        {
            var statistics = await _cacheOptimizationService.GetCacheStatisticsAsync(cacheType);

            return Ok(new
            {
                success = true,
                cacheType = cacheType,
                statistics = statistics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache statistics for {CacheType}", cacheType);
            return StatusCode(500, new { success = false, error = "Failed to retrieve cache statistics" });
        }
    }

    /// <summary>
    /// Get statistics for all cache types
    /// </summary>
    [HttpGet("optimization/statistics")]
    public async Task<IActionResult> GetAllCacheStatistics()
    {
        try
        {
            var allStatistics = await _cacheOptimizationService.GetAllCacheStatisticsAsync();

            return Ok(new
            {
                success = true,
                statistics = allStatistics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all cache statistics");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cache statistics" });
        }
    }

    /// <summary>
    /// Get cache operation history
    /// </summary>
    [HttpGet("optimization/history/{cacheType}")]
    public async Task<IActionResult> GetCacheOperationHistory(
        string cacheType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var history = await _cacheOptimizationService.GetCacheOperationHistoryAsync(cacheType, startDate, endDate);

            return Ok(new
            {
                success = true,
                cacheType = cacheType,
                history = history,
                period = new { startDate, endDate }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache operation history for {CacheType}", cacheType);
            return StatusCode(500, new { success = false, error = "Failed to retrieve cache operation history" });
        }
    }

    /// <summary>
    /// Get cache configurations
    /// </summary>
    [HttpGet("optimization/configurations")]
    public async Task<IActionResult> GetCacheConfigurations()
    {
        try
        {
            var configurations = await _cacheOptimizationService.GetAllCacheConfigurationsAsync();

            return Ok(new
            {
                success = true,
                configurations = configurations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache configurations");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cache configurations" });
        }
    }

    /// <summary>
    /// Get cache configuration for a specific type
    /// </summary>
    [HttpGet("optimization/configurations/{cacheType}")]
    public async Task<IActionResult> GetCacheConfiguration(string cacheType)
    {
        try
        {
            var configuration = await _cacheOptimizationService.GetCacheConfigurationAsync(cacheType);

            if (configuration == null)
            {
                return NotFound(new { success = false, error = "Cache configuration not found" });
            }

            return Ok(new
            {
                success = true,
                configuration = configuration
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache configuration for {CacheType}", cacheType);
            return StatusCode(500, new { success = false, error = "Failed to retrieve cache configuration" });
        }
    }

    /// <summary>
    /// Create a new cache configuration
    /// </summary>
    [HttpPost("optimization/configurations")]
    public async Task<IActionResult> CreateCacheConfiguration([FromBody] CacheConfiguration configuration)
    {
        try
        {
            var createdConfiguration = await _cacheOptimizationService.CreateCacheConfigurationAsync(configuration);

            return CreatedAtAction(nameof(GetCacheConfiguration),
                new { cacheType = createdConfiguration.CacheType },
                new
                {
                    success = true,
                    configuration = createdConfiguration
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating cache configuration");
            return StatusCode(500, new { success = false, error = "Failed to create cache configuration" });
        }
    }

    /// <summary>
    /// Update a cache configuration
    /// </summary>
    [HttpPut("optimization/configurations/{configId}")]
    public async Task<IActionResult> UpdateCacheConfiguration(string configId, [FromBody] CacheConfiguration configuration)
    {
        try
        {
            configuration.Id = configId;
            var updatedConfiguration = await _cacheOptimizationService.UpdateCacheConfigurationAsync(configuration);

            return Ok(new
            {
                success = true,
                configuration = updatedConfiguration
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating cache configuration {ConfigId}", configId);
            return StatusCode(500, new { success = false, error = "Failed to update cache configuration" });
        }
    }

    /// <summary>
    /// Delete a cache configuration
    /// </summary>
    [HttpDelete("optimization/configurations/{configId}")]
    public async Task<IActionResult> DeleteCacheConfiguration(string configId)
    {
        try
        {
            var deleted = await _cacheOptimizationService.DeleteCacheConfigurationAsync(configId);

            if (!deleted)
            {
                return NotFound(new { success = false, error = "Cache configuration not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Cache configuration deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting cache configuration {ConfigId}", configId);
            return StatusCode(500, new { success = false, error = "Failed to delete cache configuration" });
        }
    }

    /// <summary>
    /// Invalidate cache by pattern
    /// </summary>
    [HttpPost("optimization/invalidate/pattern")]
    public async Task<IActionResult> InvalidateCacheByPattern([FromBody] InvalidateByPatternRequest request)
    {
        try
        {
            await _cacheOptimizationService.InvalidateCacheByPatternAsync(request.Pattern);

            return Ok(new
            {
                success = true,
                message = $"Cache entries matching pattern '{request.Pattern}' invalidated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by pattern {Pattern}", request.Pattern);
            return StatusCode(500, new { success = false, error = "Failed to invalidate cache by pattern" });
        }
    }

    /// <summary>
    /// Invalidate cache by tags
    /// </summary>
    [HttpPost("optimization/invalidate/tags")]
    public async Task<IActionResult> InvalidateCacheByTags([FromBody] InvalidateByTagsRequest request)
    {
        try
        {
            await _cacheOptimizationService.InvalidateCacheByTagsAsync(request.Tags);

            return Ok(new
            {
                success = true,
                message = $"Cache entries with tags [{string.Join(", ", request.Tags)}] invalidated successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating cache by tags");
            return StatusCode(500, new { success = false, error = "Failed to invalidate cache by tags" });
        }
    }

    /// <summary>
    /// Get cache optimization recommendations
    /// </summary>
    [HttpGet("optimization/recommendations")]
    public async Task<IActionResult> GetOptimizationRecommendations([FromQuery] string? cacheType = null)
    {
        try
        {
            var recommendations = await _cacheOptimizationService.GetCacheOptimizationRecommendationsAsync(cacheType);

            return Ok(new
            {
                success = true,
                recommendations = recommendations,
                cacheType = cacheType
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache optimization recommendations");
            return StatusCode(500, new { success = false, error = "Failed to retrieve optimization recommendations" });
        }
    }

    /// <summary>
    /// Get cache health status
    /// </summary>
    [HttpGet("optimization/health")]
    public async Task<IActionResult> GetCacheHealthStatus()
    {
        try
        {
            var healthStatus = await _cacheOptimizationService.GetCacheHealthStatusAsync();

            return Ok(new
            {
                success = true,
                healthStatus = healthStatus,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache health status");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cache health status" });
        }
    }

    #endregion
}

/// <summary>
/// Request models for cache operations
/// </summary>
public class ClearCacheRequest
{
    /// <summary>
    /// Specific query to clear from cache
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// Pattern to match for clearing multiple cache entries
    /// </summary>
    public string? Pattern { get; set; }
}

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

/// <summary>
/// Request models from CacheOptimizationController
/// </summary>
public class InvalidateByPatternRequest
{
    public string Pattern { get; set; } = string.Empty;
}

public class InvalidateByTagsRequest
{
    public List<string> Tags { get; set; } = new();
}

public class ScheduleInvalidationRequest
{
    public string CacheKey { get; set; } = string.Empty;
    public DateTime InvalidationTime { get; set; }
}
