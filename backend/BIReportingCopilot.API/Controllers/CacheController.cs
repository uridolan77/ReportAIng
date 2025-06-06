using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Constants;
using System.Security.Cryptography;
using System.Text;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for cache management operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly IQueryService _queryService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(
        ICacheService cacheService,
        IQueryService queryService,
        ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _queryService = queryService;
        _logger = logger;
    }

    /// <summary>
    /// Clear specific cache entry by query
    /// </summary>
    /// <param name="request">Cache clear request</param>
    /// <returns>Success status</returns>
    [HttpPost("clear")]
    public async Task<ActionResult> ClearCache([FromBody] ClearCacheRequest request)
    {
        try
        {
            _logger.LogInformation("Cache clear requested for query: {Query}", request.Query);

            if (!string.IsNullOrEmpty(request.Query))
            {
                // Generate the same cache key that would be used for this query
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
                // Clear cache entries matching pattern
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
    /// <returns>Success status</returns>
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
    /// Get cache statistics
    /// </summary>
    /// <returns>Cache statistics</returns>
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
    /// <param name="query">Query to check</param>
    /// <returns>Cache existence status</returns>
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

    /// <summary>
    /// Generate cache key for a query (same logic as QueryService)
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <returns>Cache key</returns>
    private string GenerateCacheKey(string query)
    {
        // Use the same logic as in QueryService to generate consistent cache keys
        var normalizedQuery = query.Trim().ToLowerInvariant();
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(normalizedQuery));
        return Convert.ToHexString(hashBytes).ToLowerInvariant();
    }
}

/// <summary>
/// Request model for cache clearing operations
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
