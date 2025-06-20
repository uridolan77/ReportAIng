using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Performance optimization and monitoring controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceOptimizationService _performanceService;
    private readonly ICacheOptimizationService _cacheService;
    private readonly IResourceMonitoringService _monitoringService;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        IPerformanceOptimizationService performanceService,
        ICacheOptimizationService cacheService,
        IResourceMonitoringService monitoringService,
        ILogger<PerformanceController> logger)
    {
        _performanceService = performanceService;
        _cacheService = cacheService;
        _monitoringService = monitoringService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze performance for an entity
    /// </summary>
    [HttpGet("analyze/{entityType}/{entityId}")]
    public async Task<IActionResult> AnalyzePerformance(string entityType, string entityId)
    {
        try
        {
            var metrics = await _performanceService.AnalyzePerformanceAsync(entityId, entityType);

            return Ok(new
            {
                success = true,
                entityType = entityType,
                entityId = entityId,
                metrics = metrics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing performance for {EntityType}:{EntityId}", entityType, entityId);
            return StatusCode(500, new { success = false, error = "Failed to analyze performance" });
        }
    }

    /// <summary>
    /// Identify performance bottlenecks
    /// </summary>
    [HttpGet("bottlenecks/{entityType}/{entityId}")]
    public async Task<IActionResult> IdentifyBottlenecks(string entityType, string entityId)
    {
        try
        {
            var bottlenecks = await _performanceService.IdentifyBottlenecksAsync(entityId, entityType);

            return Ok(new
            {
                success = true,
                entityType = entityType,
                entityId = entityId,
                bottlenecks = bottlenecks
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error identifying bottlenecks for {EntityType}:{EntityId}", entityType, entityId);
            return StatusCode(500, new { success = false, error = "Failed to identify bottlenecks" });
        }
    }

    /// <summary>
    /// Get optimization suggestions
    /// </summary>
    [HttpGet("suggestions/{entityType}/{entityId}")]
    public async Task<IActionResult> GetOptimizationSuggestions(string entityType, string entityId)
    {
        try
        {
            var suggestions = await _performanceService.GetOptimizationSuggestionsAsync(entityId, entityType);

            return Ok(new
            {
                success = true,
                entityType = entityType,
                entityId = entityId,
                suggestions = suggestions
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimization suggestions for {EntityType}:{EntityId}", entityType, entityId);
            return StatusCode(500, new { success = false, error = "Failed to get optimization suggestions" });
        }
    }

    /// <summary>
    /// Auto-tune performance for an entity
    /// </summary>
    [HttpPost("auto-tune/{entityType}/{entityId}")]
    public async Task<IActionResult> AutoTunePerformance(string entityType, string entityId)
    {
        try
        {
            var result = await _performanceService.AutoTunePerformanceAsync(entityId, entityType);

            return Ok(new
            {
                success = result.Success,
                result = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-tuning performance for {EntityType}:{EntityId}", entityType, entityId);
            return StatusCode(500, new { success = false, error = "Failed to auto-tune performance" });
        }
    }

    /// <summary>
    /// Get performance benchmarks
    /// </summary>
    [HttpGet("benchmarks")]
    public async Task<IActionResult> GetBenchmarks([FromQuery] string? category = null)
    {
        try
        {
            var benchmarks = await _performanceService.GetBenchmarksAsync(category);

            return Ok(new
            {
                success = true,
                benchmarks = benchmarks,
                category = category
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance benchmarks");
            return StatusCode(500, new { success = false, error = "Failed to retrieve benchmarks" });
        }
    }

    /// <summary>
    /// Create a performance benchmark
    /// </summary>
    [HttpPost("benchmarks")]
    public async Task<IActionResult> CreateBenchmark([FromBody] CreateBenchmarkRequest request)
    {
        try
        {
            var benchmark = await _performanceService.CreateBenchmarkAsync(
                request.Name, 
                request.Category, 
                request.Value, 
                request.Unit);

            return CreatedAtAction(nameof(GetBenchmarks), new { category = benchmark.Category }, new
            {
                success = true,
                benchmark = benchmark
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating benchmark");
            return StatusCode(500, new { success = false, error = "Failed to create benchmark" });
        }
    }

    /// <summary>
    /// Update a performance benchmark
    /// </summary>
    [HttpPut("benchmarks/{benchmarkId}")]
    public async Task<IActionResult> UpdateBenchmark(string benchmarkId, [FromBody] UpdateBenchmarkRequest request)
    {
        try
        {
            var benchmark = await _performanceService.UpdateBenchmarkAsync(benchmarkId, request.NewValue);

            return Ok(new
            {
                success = true,
                benchmark = benchmark
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating benchmark {BenchmarkId}", benchmarkId);
            return StatusCode(500, new { success = false, error = "Failed to update benchmark" });
        }
    }

    /// <summary>
    /// Get performance improvements over time
    /// </summary>
    [HttpGet("improvements")]
    public async Task<IActionResult> GetPerformanceImprovements(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var improvements = await _performanceService.GetPerformanceImprovementsAsync(startDate, endDate);

            return Ok(new
            {
                success = true,
                improvements = improvements,
                period = new { startDate, endDate }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance improvements");
            return StatusCode(500, new { success = false, error = "Failed to retrieve performance improvements" });
        }
    }

    /// <summary>
    /// Record a performance metric
    /// </summary>
    [HttpPost("metrics")]
    public async Task<IActionResult> RecordMetric([FromBody] PerformanceMetricsEntry metric)
    {
        try
        {
            await _performanceService.RecordPerformanceMetricAsync(metric);

            return Ok(new
            {
                success = true,
                message = "Performance metric recorded successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording performance metric");
            return StatusCode(500, new { success = false, error = "Failed to record performance metric" });
        }
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    [HttpGet("metrics/{metricName}")]
    public async Task<IActionResult> GetMetrics(
        string metricName,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var metrics = await _performanceService.GetPerformanceMetricsAsync(metricName, startDate, endDate);

            return Ok(new
            {
                success = true,
                metricName = metricName,
                metrics = metrics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics for {MetricName}", metricName);
            return StatusCode(500, new { success = false, error = "Failed to retrieve performance metrics" });
        }
    }

    /// <summary>
    /// Get aggregated performance metrics
    /// </summary>
    [HttpGet("metrics/{metricName}/aggregated")]
    public async Task<IActionResult> GetAggregatedMetrics(
        string metricName,
        [FromQuery] string aggregationType = "average",
        [FromQuery] int periodHours = 24)
    {
        try
        {
            var period = TimeSpan.FromHours(periodHours);
            var metrics = await _performanceService.GetAggregatedMetricsAsync(metricName, aggregationType, period);

            return Ok(new
            {
                success = true,
                metricName = metricName,
                aggregationType = aggregationType,
                period = periodHours,
                metrics = metrics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting aggregated metrics for {MetricName}", metricName);
            return StatusCode(500, new { success = false, error = "Failed to retrieve aggregated metrics" });
        }
    }

    /// <summary>
    /// Get active performance alerts
    /// </summary>
    [HttpGet("alerts")]
    public async Task<IActionResult> GetActiveAlerts()
    {
        try
        {
            var alerts = await _performanceService.GetActivePerformanceAlertsAsync();

            return Ok(new
            {
                success = true,
                alerts = alerts,
                count = alerts.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active performance alerts");
            return StatusCode(500, new { success = false, error = "Failed to retrieve performance alerts" });
        }
    }

    /// <summary>
    /// Resolve a performance alert
    /// </summary>
    [HttpPost("alerts/{alertId}/resolve")]
    public async Task<IActionResult> ResolveAlert(string alertId, [FromBody] ResolveAlertRequest request)
    {
        try
        {
            var resolved = await _performanceService.ResolvePerformanceAlertAsync(alertId, request.ResolutionNotes);

            if (!resolved)
            {
                return NotFound(new { success = false, error = "Alert not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Alert resolved successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resolving alert {AlertId}", alertId);
            return StatusCode(500, new { success = false, error = "Failed to resolve alert" });
        }
    }

    /// <summary>
    /// Apply a performance optimization
    /// </summary>
    [HttpPost("optimizations/{optimizationId}/apply")]
    public async Task<IActionResult> ApplyOptimization(string optimizationId)
    {
        try
        {
            var applied = await _performanceService.ApplyPerformanceOptimizationAsync(optimizationId);

            if (!applied)
            {
                return NotFound(new { success = false, error = "Optimization not found or could not be applied" });
            }

            return Ok(new
            {
                success = true,
                message = "Optimization applied successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying optimization {OptimizationId}", optimizationId);
            return StatusCode(500, new { success = false, error = "Failed to apply optimization" });
        }
    }
}

/// <summary>
/// Request model for creating benchmarks
/// </summary>
public class CreateBenchmarkRequest
{
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public double Value { get; set; }
    public string Unit { get; set; } = string.Empty;
}

/// <summary>
/// Request model for updating benchmarks
/// </summary>
public class UpdateBenchmarkRequest
{
    public double NewValue { get; set; }
}

/// <summary>
/// Request model for resolving alerts
/// </summary>
public class ResolveAlertRequest
{
    public string ResolutionNotes { get; set; } = string.Empty;
}
