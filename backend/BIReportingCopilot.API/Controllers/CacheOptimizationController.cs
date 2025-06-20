using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Cache optimization and management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CacheOptimizationController : ControllerBase
{
    private readonly ICacheOptimizationService _cacheService;
    private readonly ILogger<CacheOptimizationController> _logger;

    public CacheOptimizationController(
        ICacheOptimizationService cacheService,
        ILogger<CacheOptimizationController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get cache statistics for a specific cache type
    /// </summary>
    [HttpGet("statistics/{cacheType}")]
    public async Task<IActionResult> GetCacheStatistics(string cacheType)
    {
        try
        {
            var statistics = await _cacheService.GetCacheStatisticsAsync(cacheType);

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
    [HttpGet("statistics")]
    public async Task<IActionResult> GetAllCacheStatistics()
    {
        try
        {
            var allStatistics = await _cacheService.GetAllCacheStatisticsAsync();

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
    [HttpGet("history/{cacheType}")]
    public async Task<IActionResult> GetCacheOperationHistory(
        string cacheType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var history = await _cacheService.GetCacheOperationHistoryAsync(cacheType, startDate, endDate);

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
    [HttpGet("configurations")]
    public async Task<IActionResult> GetCacheConfigurations()
    {
        try
        {
            var configurations = await _cacheService.GetAllCacheConfigurationsAsync();

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
    [HttpGet("configurations/{cacheType}")]
    public async Task<IActionResult> GetCacheConfiguration(string cacheType)
    {
        try
        {
            var configuration = await _cacheService.GetCacheConfigurationAsync(cacheType);

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
    [HttpPost("configurations")]
    public async Task<IActionResult> CreateCacheConfiguration([FromBody] CacheConfiguration configuration)
    {
        try
        {
            var createdConfiguration = await _cacheService.CreateCacheConfigurationAsync(configuration);

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
    [HttpPut("configurations/{configId}")]
    public async Task<IActionResult> UpdateCacheConfiguration(string configId, [FromBody] CacheConfiguration configuration)
    {
        try
        {
            configuration.Id = configId;
            var updatedConfiguration = await _cacheService.UpdateCacheConfigurationAsync(configuration);

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
    [HttpDelete("configurations/{configId}")]
    public async Task<IActionResult> DeleteCacheConfiguration(string configId)
    {
        try
        {
            var deleted = await _cacheService.DeleteCacheConfigurationAsync(configId);

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
    [HttpPost("invalidate/pattern")]
    public async Task<IActionResult> InvalidateCacheByPattern([FromBody] InvalidateByPatternRequest request)
    {
        try
        {
            await _cacheService.InvalidateCacheByPatternAsync(request.Pattern);

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
    [HttpPost("invalidate/tags")]
    public async Task<IActionResult> InvalidateCacheByTags([FromBody] InvalidateByTagsRequest request)
    {
        try
        {
            await _cacheService.InvalidateCacheByTagsAsync(request.Tags);

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
    /// Get cache keys for invalidation based on entity
    /// </summary>
    [HttpGet("invalidation-keys/{entityType}/{entityId}")]
    public async Task<IActionResult> GetCacheKeysForInvalidation(string entityType, string entityId)
    {
        try
        {
            var keys = await _cacheService.GetCacheKeysForInvalidationAsync(entityType, entityId);

            return Ok(new
            {
                success = true,
                entityType = entityType,
                entityId = entityId,
                keys = keys
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache keys for invalidation");
            return StatusCode(500, new { success = false, error = "Failed to get cache keys for invalidation" });
        }
    }

    /// <summary>
    /// Schedule cache invalidation
    /// </summary>
    [HttpPost("invalidate/schedule")]
    public async Task<IActionResult> ScheduleCacheInvalidation([FromBody] ScheduleInvalidationRequest request)
    {
        try
        {
            await _cacheService.ScheduleCacheInvalidationAsync(request.CacheKey, request.InvalidationTime);

            return Ok(new
            {
                success = true,
                message = $"Cache invalidation scheduled for key '{request.CacheKey}' at {request.InvalidationTime}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling cache invalidation");
            return StatusCode(500, new { success = false, error = "Failed to schedule cache invalidation" });
        }
    }

    /// <summary>
    /// Get cache optimization recommendations
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetOptimizationRecommendations([FromQuery] string? cacheType = null)
    {
        try
        {
            var recommendations = await _cacheService.GetCacheOptimizationRecommendationsAsync(cacheType);

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
    /// Analyze cache usage patterns
    /// </summary>
    [HttpGet("analysis/{cacheType}")]
    public async Task<IActionResult> AnalyzeCacheUsagePatterns(
        string cacheType,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var analysis = await _cacheService.AnalyzeCacheUsagePatternsAsync(cacheType, startDate, endDate);

            return Ok(new
            {
                success = true,
                cacheType = cacheType,
                analysis = analysis,
                period = new { startDate, endDate }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing cache usage patterns for {CacheType}", cacheType);
            return StatusCode(500, new { success = false, error = "Failed to analyze cache usage patterns" });
        }
    }

    /// <summary>
    /// Get unused cache keys
    /// </summary>
    [HttpGet("unused/{cacheType}")]
    public async Task<IActionResult> GetUnusedCacheKeys(string cacheType, [FromQuery] int unusedHours = 24)
    {
        try
        {
            var threshold = TimeSpan.FromHours(unusedHours);
            var unusedKeys = await _cacheService.GetUnusedCacheKeysAsync(cacheType, threshold);

            return Ok(new
            {
                success = true,
                cacheType = cacheType,
                unusedKeys = unusedKeys,
                threshold = unusedHours
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting unused cache keys for {CacheType}", cacheType);
            return StatusCode(500, new { success = false, error = "Failed to retrieve unused cache keys" });
        }
    }

    /// <summary>
    /// Get cache health status
    /// </summary>
    [HttpGet("health")]
    public async Task<IActionResult> GetCacheHealthStatus()
    {
        try
        {
            var healthStatus = await _cacheService.GetCacheHealthStatusAsync();

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

    /// <summary>
    /// Get cache alerts
    /// </summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetCacheAlerts([FromQuery] bool activeOnly = true)
    {
        try
        {
            var alerts = await _cacheService.GetCacheAlertsAsync(activeOnly);

            return Ok(new
            {
                success = true,
                alerts = alerts,
                activeOnly = activeOnly
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cache alerts");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cache alerts" });
        }
    }
}

/// <summary>
/// Request model for invalidating cache by pattern
/// </summary>
public class InvalidateByPatternRequest
{
    public string Pattern { get; set; } = string.Empty;
}

/// <summary>
/// Request model for invalidating cache by tags
/// </summary>
public class InvalidateByTagsRequest
{
    public List<string> Tags { get; set; } = new();
}

/// <summary>
/// Request model for scheduling cache invalidation
/// </summary>
public class ScheduleInvalidationRequest
{
    public string CacheKey { get; set; } = string.Empty;
    public DateTime InvalidationTime { get; set; }
}
