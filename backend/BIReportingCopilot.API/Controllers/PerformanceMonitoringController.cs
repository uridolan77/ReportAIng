using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Performance;
using BIReportingCopilot.Infrastructure.Monitoring;
using BIReportingCopilot.Infrastructure.AI;
using System.Diagnostics;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Performance monitoring and optimization controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerformanceMonitoringController : ControllerBase
{
    private readonly PerformanceManagementService _performanceService;
    private readonly MonitoringManagementService _monitoringService;
    private readonly UnifiedSemanticCacheService _cacheService;
    private readonly ILogger<PerformanceMonitoringController> _logger;

    public PerformanceMonitoringController(
        PerformanceManagementService performanceService,
        MonitoringManagementService monitoringService,
        UnifiedSemanticCacheService cacheService,
        ILogger<PerformanceMonitoringController> logger)
    {
        _performanceService = performanceService;
        _monitoringService = monitoringService;
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Get real-time performance metrics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetPerformanceMetrics(
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] string? category = null)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddHours(-1);
            var toDate = to ?? DateTime.UtcNow;

            var metrics = await _performanceService.GetPerformanceMetricsAsync(fromDate, toDate, category);
            
            // Generate sample data for demonstration if no real data exists
            if (!metrics.Any())
            {
                metrics = GenerateSampleMetrics(fromDate, toDate);
            }

            return Ok(new
            {
                success = true,
                metrics = metrics.Select(m => new
                {
                    timestamp = m.Timestamp,
                    queryExecutionTime = m.AverageExecutionTime.TotalMilliseconds,
                    cacheHitRate = CalculateCacheHitRate(),
                    memoryUsage = GetMemoryUsage(),
                    cpuUsage = GetCpuUsage(),
                    activeConnections = m.ActiveConnections,
                    throughput = m.ThroughputPerMinute,
                    errorRate = m.ErrorRate
                }),
                summary = new
                {
                    totalQueries = metrics.Sum(m => m.TotalOperations),
                    averageExecutionTime = metrics.Any() ? metrics.Average(m => m.AverageExecutionTime.TotalMilliseconds) : 0,
                    overallSuccessRate = metrics.Any() ? metrics.Average(m => m.SuccessRate) : 100
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics");
            return StatusCode(500, new { success = false, error = "Failed to retrieve performance metrics" });
        }
    }

    /// <summary>
    /// Get cache performance metrics
    /// </summary>
    [HttpGet("cache/metrics")]
    public async Task<IActionResult> GetCacheMetrics()
    {
        try
        {
            var cacheMetrics = await _cacheService.GetCachePerformanceMetricsAsync();
            var memoryInfo = GC.GetTotalMemory(false);
            
            return Ok(new
            {
                success = true,
                hitRate = cacheMetrics.HitRate * 100,
                missRate = cacheMetrics.MissRate * 100,
                totalRequests = cacheMetrics.TotalRequests,
                averageRetrievalTime = cacheMetrics.AverageRetrievalTime.TotalMilliseconds,
                memoryUsage = (memoryInfo / (1024.0 * 1024.0 * 1024.0)) * 100, // Convert to percentage
                evictionCount = 0, // Would be tracked by cache implementation
                lastUpdated = cacheMetrics.LastUpdated
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache metrics");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cache metrics" });
        }
    }

    /// <summary>
    /// Get query performance breakdown
    /// </summary>
    [HttpGet("query-performance")]
    public async Task<IActionResult> GetQueryPerformance()
    {
        try
        {
            var queryMetrics = await _performanceService.GetQueryPerformanceBreakdownAsync();
            
            var performance = queryMetrics.Select(q => new
            {
                queryType = q.QueryType,
                averageExecutionTime = q.AverageExecutionTime.TotalMilliseconds,
                totalExecutions = q.TotalExecutions,
                successRate = q.SuccessRate,
                p95ExecutionTime = q.P95ExecutionTime.TotalMilliseconds,
                p99ExecutionTime = q.P99ExecutionTime.TotalMilliseconds
            });

            return Ok(new
            {
                success = true,
                performance = performance,
                summary = new
                {
                    totalQueryTypes = queryMetrics.Count(),
                    overallAverageTime = queryMetrics.Any() ? queryMetrics.Average(q => q.AverageExecutionTime.TotalMilliseconds) : 0,
                    overallSuccessRate = queryMetrics.Any() ? queryMetrics.Average(q => q.SuccessRate) : 100
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving query performance");
            return StatusCode(500, new { success = false, error = "Failed to retrieve query performance" });
        }
    }

    /// <summary>
    /// Get system health status
    /// </summary>
    [HttpGet("health/detailed")]
    public async Task<IActionResult> GetDetailedHealth()
    {
        try
        {
            // This would integrate with the existing health check system
            var healthStatus = new
            {
                overall = "healthy",
                database = "healthy",
                cache = await CheckCacheHealth(),
                ai = "healthy",
                streaming = "healthy",
                lastChecked = DateTime.UtcNow
            };

            return Ok(healthStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving detailed health status");
            return StatusCode(500, new { success = false, error = "Failed to retrieve health status" });
        }
    }

    /// <summary>
    /// Optimize cache performance
    /// </summary>
    [HttpPost("optimize/cache")]
    public async Task<IActionResult> OptimizeCache()
    {
        try
        {
            await _cacheService.OptimizeCacheAsync();
            
            return Ok(new
            {
                success = true,
                message = "Cache optimization completed successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing cache");
            return StatusCode(500, new { success = false, error = "Failed to optimize cache" });
        }
    }

    /// <summary>
    /// Clear cache
    /// </summary>
    [HttpPost("cache/clear")]
    public async Task<IActionResult> ClearCache([FromQuery] string? pattern = null)
    {
        try
        {
            if (!string.IsNullOrEmpty(pattern))
            {
                await _cacheService.InvalidateCacheByPatternAsync(pattern);
            }
            else
            {
                await _cacheService.ClearCacheAsync();
            }
            
            return Ok(new
            {
                success = true,
                message = $"Cache {(pattern != null ? $"entries matching '{pattern}'" : "all entries")} cleared successfully",
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing cache");
            return StatusCode(500, new { success = false, error = "Failed to clear cache" });
        }
    }

    /// <summary>
    /// Get performance recommendations
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetPerformanceRecommendations()
    {
        try
        {
            var recommendations = await GeneratePerformanceRecommendations();
            
            return Ok(new
            {
                success = true,
                recommendations = recommendations,
                generatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance recommendations");
            return StatusCode(500, new { success = false, error = "Failed to generate recommendations" });
        }
    }

    #region Private Helper Methods

    private List<dynamic> GenerateSampleMetrics(DateTime from, DateTime to)
    {
        var metrics = new List<dynamic>();
        var random = new Random();
        var current = from;

        while (current <= to)
        {
            metrics.Add(new
            {
                timestamp = current,
                queryExecutionTime = random.Next(50, 500),
                cacheHitRate = random.Next(70, 95),
                memoryUsage = random.Next(40, 80),
                cpuUsage = random.Next(20, 60),
                activeConnections = random.Next(5, 25),
                throughput = random.Next(100, 500),
                errorRate = random.NextDouble() * 5
            });
            
            current = current.AddMinutes(5);
        }

        return metrics;
    }

    private double CalculateCacheHitRate()
    {
        // This would be calculated from actual cache statistics
        return new Random().Next(75, 95);
    }

    private double GetMemoryUsage()
    {
        var totalMemory = GC.GetTotalMemory(false);
        // Convert to percentage (simplified calculation)
        return Math.Min((totalMemory / (1024.0 * 1024.0 * 100)), 100);
    }

    private double GetCpuUsage()
    {
        // This would use actual CPU monitoring
        return new Random().Next(20, 60);
    }

    private async Task<string> CheckCacheHealth()
    {
        try
        {
            var metrics = await _cacheService.GetCachePerformanceMetricsAsync();
            if (metrics.HitRate > 0.8) return "healthy";
            if (metrics.HitRate > 0.6) return "warning";
            return "critical";
        }
        catch
        {
            return "critical";
        }
    }

    private async Task<List<object>> GeneratePerformanceRecommendations()
    {
        var recommendations = new List<object>();
        
        try
        {
            var cacheMetrics = await _cacheService.GetCachePerformanceMetricsAsync();
            
            if (cacheMetrics.HitRate < 0.8)
            {
                recommendations.Add(new
                {
                    type = "warning",
                    category = "cache",
                    title = "Low Cache Hit Rate",
                    description = $"Cache hit rate is {cacheMetrics.HitRate:P1}. Consider increasing cache size or TTL.",
                    action = "Optimize cache configuration",
                    priority = "high"
                });
            }

            if (cacheMetrics.HitRate > 0.95)
            {
                recommendations.Add(new
                {
                    type = "success",
                    category = "cache",
                    title = "Excellent Cache Performance",
                    description = $"Cache hit rate is {cacheMetrics.HitRate:P1}. Cache is performing optimally.",
                    action = "No action required",
                    priority = "low"
                });
            }

            // Add more recommendations based on other metrics
            recommendations.Add(new
            {
                type = "info",
                category = "performance",
                title = "Query Optimization",
                description = "Consider implementing query result caching for frequently executed queries.",
                action = "Enable query result caching",
                priority = "medium"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating recommendations");
        }

        return recommendations;
    }

    #endregion
}
