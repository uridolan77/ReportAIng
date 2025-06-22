using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Resource and cost management controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ResourceManagementController : ControllerBase
{
    private readonly IResourceManagementService _resourceService;
    private readonly IResourceMonitoringService _monitoringService;
    private readonly ICostManagementService _costManagementService;
    private readonly ICostAnalyticsService _costAnalyticsService;
    private readonly ILogger<ResourceManagementController> _logger;

    public ResourceManagementController(
        IResourceManagementService resourceService,
        IResourceMonitoringService monitoringService,
        ICostManagementService costManagementService,
        ICostAnalyticsService costAnalyticsService,
        ILogger<ResourceManagementController> logger)
    {
        _resourceService = resourceService;
        _monitoringService = monitoringService;
        _costManagementService = costManagementService;
        _costAnalyticsService = costAnalyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get user resource quotas
    /// </summary>
    [HttpGet("quotas")]
    public async Task<IActionResult> GetUserResourceQuotas()
    {
        try
        {
            var userId = GetCurrentUserId();
            var quotas = await _resourceService.GetUserResourceQuotasAsync(userId);

            return Ok(new
            {
                success = true,
                userId = userId,
                quotas = quotas
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user resource quotas");
            return StatusCode(500, new { success = false, error = "Failed to retrieve resource quotas" });
        }
    }

    /// <summary>
    /// Get specific resource quota
    /// </summary>
    [HttpGet("quotas/{resourceType}")]
    public async Task<IActionResult> GetResourceQuota(string resourceType)
    {
        try
        {
            var userId = GetCurrentUserId();
            var quota = await _resourceService.GetResourceQuotaAsync(userId, resourceType);

            if (quota == null)
            {
                return NotFound(new { success = false, error = "Resource quota not found" });
            }

            return Ok(new
            {
                success = true,
                quota = quota
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource quota for {ResourceType}", resourceType);
            return StatusCode(500, new { success = false, error = "Failed to retrieve resource quota" });
        }
    }

    /// <summary>
    /// Create a new resource quota
    /// </summary>
    [HttpPost("quotas")]
    public async Task<IActionResult> CreateResourceQuota([FromBody] ResourceQuota quota)
    {
        try
        {
            var userId = GetCurrentUserId();
            quota.UserId = userId;

            var createdQuota = await _resourceService.CreateResourceQuotaAsync(quota);

            return CreatedAtAction(nameof(GetResourceQuota), 
                new { resourceType = createdQuota.ResourceType }, 
                new
                {
                    success = true,
                    quota = createdQuota
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating resource quota");
            return StatusCode(500, new { success = false, error = "Failed to create resource quota" });
        }
    }

    /// <summary>
    /// Update a resource quota
    /// </summary>
    [HttpPut("quotas/{quotaId}")]
    public async Task<IActionResult> UpdateResourceQuota(string quotaId, [FromBody] ResourceQuota quota)
    {
        try
        {
            quota.Id = quotaId;
            var updatedQuota = await _resourceService.UpdateResourceQuotaAsync(quota);

            return Ok(new
            {
                success = true,
                quota = updatedQuota
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource quota {QuotaId}", quotaId);
            return StatusCode(500, new { success = false, error = "Failed to update resource quota" });
        }
    }

    /// <summary>
    /// Delete a resource quota
    /// </summary>
    [HttpDelete("quotas/{quotaId}")]
    public async Task<IActionResult> DeleteResourceQuota(string quotaId)
    {
        try
        {
            var deleted = await _resourceService.DeleteResourceQuotaAsync(quotaId);

            if (!deleted)
            {
                return NotFound(new { success = false, error = "Resource quota not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Resource quota deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting resource quota {QuotaId}", quotaId);
            return StatusCode(500, new { success = false, error = "Failed to delete resource quota" });
        }
    }

    /// <summary>
    /// Check resource quota availability
    /// </summary>
    [HttpPost("quotas/check")]
    public async Task<IActionResult> CheckResourceQuota([FromBody] CheckQuotaRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var allowed = await _resourceService.CheckResourceQuotaAsync(userId, request.ResourceType, request.RequestedQuantity);

            return Ok(new
            {
                success = true,
                allowed = allowed,
                resourceType = request.ResourceType,
                requestedQuantity = request.RequestedQuantity
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking resource quota");
            return StatusCode(500, new { success = false, error = "Failed to check resource quota" });
        }
    }

    /// <summary>
    /// Get current resource usage
    /// </summary>
    [HttpGet("usage")]
    public async Task<IActionResult> GetCurrentResourceUsage()
    {
        try
        {
            var userId = GetCurrentUserId();
            var usage = await _resourceService.GetCurrentResourceUsageAsync(userId);

            return Ok(new
            {
                success = true,
                userId = userId,
                usage = usage
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current resource usage");
            return StatusCode(500, new { success = false, error = "Failed to retrieve resource usage" });
        }
    }

    /// <summary>
    /// Reset resource usage for a specific type
    /// </summary>
    [HttpPost("usage/reset/{resourceType}")]
    public async Task<IActionResult> ResetResourceUsage(string resourceType)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _resourceService.ResetResourceUsageAsync(userId, resourceType);

            return Ok(new
            {
                success = true,
                message = $"Resource usage reset for {resourceType}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting resource usage for {ResourceType}", resourceType);
            return StatusCode(500, new { success = false, error = "Failed to reset resource usage" });
        }
    }

    /// <summary>
    /// Get user priority
    /// </summary>
    [HttpGet("priority")]
    public async Task<IActionResult> GetUserPriority()
    {
        try
        {
            var userId = GetCurrentUserId();
            var priority = await _resourceService.GetUserPriorityAsync(userId);

            return Ok(new
            {
                success = true,
                userId = userId,
                priority = priority
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user priority");
            return StatusCode(500, new { success = false, error = "Failed to retrieve user priority" });
        }
    }

    /// <summary>
    /// Set user priority (admin only)
    /// </summary>
    [HttpPost("priority")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> SetUserPriority([FromBody] SetPriorityRequest request)
    {
        try
        {
            await _resourceService.SetUserPriorityAsync(request.UserId, request.Priority);

            return Ok(new
            {
                success = true,
                message = $"Priority set to {request.Priority} for user {request.UserId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user priority");
            return StatusCode(500, new { success = false, error = "Failed to set user priority" });
        }
    }

    /// <summary>
    /// Get queued requests for a resource type
    /// </summary>
    [HttpGet("queue/{resourceType}")]
    public async Task<IActionResult> GetQueuedRequests(string resourceType)
    {
        try
        {
            var queuedRequests = await _resourceService.GetQueuedRequestsAsync(resourceType);

            return Ok(new
            {
                success = true,
                resourceType = resourceType,
                queuedRequests = queuedRequests,
                count = queuedRequests.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting queued requests for {ResourceType}", resourceType);
            return StatusCode(500, new { success = false, error = "Failed to retrieve queued requests" });
        }
    }

    /// <summary>
    /// Queue a request
    /// </summary>
    [HttpPost("queue")]
    public async Task<IActionResult> QueueRequest([FromBody] QueueRequestRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var queued = await _resourceService.QueueRequestAsync(request.RequestId, userId, request.ResourceType, request.Priority);

            if (!queued)
            {
                return BadRequest(new { success = false, error = "Failed to queue request" });
            }

            return Ok(new
            {
                success = true,
                message = "Request queued successfully",
                requestId = request.RequestId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error queueing request");
            return StatusCode(500, new { success = false, error = "Failed to queue request" });
        }
    }

    /// <summary>
    /// Get circuit breaker state
    /// </summary>
    [HttpGet("circuit-breaker/{serviceName}")]
    public async Task<IActionResult> GetCircuitBreakerState(string serviceName)
    {
        try
        {
            var state = await _resourceService.GetCircuitBreakerStateAsync(serviceName);

            return Ok(new
            {
                success = true,
                serviceName = serviceName,
                state = state
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting circuit breaker state for {ServiceName}", serviceName);
            return StatusCode(500, new { success = false, error = "Failed to retrieve circuit breaker state" });
        }
    }

    /// <summary>
    /// Check if service is available
    /// </summary>
    [HttpGet("availability/{serviceName}")]
    public async Task<IActionResult> IsServiceAvailable(string serviceName)
    {
        try
        {
            var available = await _resourceService.IsServiceAvailableAsync(serviceName);

            return Ok(new
            {
                success = true,
                serviceName = serviceName,
                available = available
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking service availability for {ServiceName}", serviceName);
            return StatusCode(500, new { success = false, error = "Failed to check service availability" });
        }
    }

    /// <summary>
    /// Get resource load statistics
    /// </summary>
    [HttpGet("load/{resourceType}")]
    public async Task<IActionResult> GetResourceLoadStats(string resourceType)
    {
        try
        {
            var loadStats = await _resourceService.GetResourceLoadStatsAsync(resourceType);

            return Ok(new
            {
                success = true,
                resourceType = resourceType,
                loadStats = loadStats
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource load stats for {ResourceType}", resourceType);
            return StatusCode(500, new { success = false, error = "Failed to retrieve resource load statistics" });
        }
    }

    /// <summary>
    /// Select optimal resource
    /// </summary>
    [HttpGet("optimal/{resourceType}")]
    public async Task<IActionResult> SelectOptimalResource(string resourceType)
    {
        try
        {
            var optimalResource = await _resourceService.SelectOptimalResourceAsync(resourceType);

            return Ok(new
            {
                success = true,
                resourceType = resourceType,
                optimalResource = optimalResource
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting optimal resource for {ResourceType}", resourceType);
            return StatusCode(500, new { success = false, error = "Failed to select optimal resource" });
        }
    }

    #region Cost Management

    /// <summary>
    /// Get cost analytics summary
    /// </summary>
    [HttpGet("cost/analytics")]
    public async Task<IActionResult> GetCostAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var analytics = await _costAnalyticsService.GenerateCostAnalyticsAsync(userId, startDate, endDate);

            return Ok(new
            {
                success = true,
                data = analytics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost analytics");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cost analytics" });
        }
    }

    /// <summary>
    /// Get cost history for a user
    /// </summary>
    [HttpGet("cost/history")]
    public async Task<IActionResult> GetCostHistory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int limit = 100)
    {
        try
        {
            var userId = GetCurrentUserId();
            var history = await _costManagementService.GetCostHistoryAsync(userId, startDate, endDate);

            return Ok(new
            {
                success = true,
                data = history.Take(limit),
                total = history.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost history");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cost history" });
        }
    }

    /// <summary>
    /// Get total cost for a user
    /// </summary>
    [HttpGet("cost/total")]
    public async Task<IActionResult> GetTotalCost(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var totalCost = await _costManagementService.GetTotalCostAsync(userId, startDate, endDate);

            return Ok(new
            {
                success = true,
                totalCost = totalCost,
                period = new { startDate, endDate }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total cost");
            return StatusCode(500, new { success = false, error = "Failed to retrieve total cost" });
        }
    }

    /// <summary>
    /// Get cost breakdown by dimension
    /// </summary>
    [HttpGet("cost/breakdown/{dimension}")]
    public async Task<IActionResult> GetCostBreakdown(
        string dimension,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var breakdown = await _costAnalyticsService.GetCostBreakdownByDimensionAsync(dimension, userId, startDate, endDate);

            return Ok(new
            {
                success = true,
                dimension = dimension,
                breakdown = breakdown
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost breakdown for dimension {Dimension}", dimension);
            return StatusCode(500, new { success = false, error = $"Failed to retrieve cost breakdown for {dimension}" });
        }
    }

    /// <summary>
    /// Get cost trends
    /// </summary>
    [HttpGet("cost/trends")]
    public async Task<IActionResult> GetCostTrends(
        [FromQuery] string? category = null,
        [FromQuery] string granularity = "daily",
        [FromQuery] int periods = 30)
    {
        try
        {
            var userId = GetCurrentUserId();
            var trends = await _costAnalyticsService.GetCostTrendsAsync(userId, category, granularity, periods);

            return Ok(new
            {
                success = true,
                trends = trends,
                granularity = granularity,
                periods = periods
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost trends");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cost trends" });
        }
    }

    /// <summary>
    /// Predict cost for a query
    /// </summary>
    [HttpPost("cost/predict")]
    public async Task<IActionResult> PredictCost([FromBody] CostPredictionRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var prediction = await _costManagementService.PredictCostAsync(
                request.QueryId,
                userId,
                request.Criteria);

            return Ok(new
            {
                success = true,
                prediction = prediction
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error predicting cost");
            return StatusCode(500, new { success = false, error = "Failed to predict cost" });
        }
    }

    /// <summary>
    /// Get cost forecasts
    /// </summary>
    [HttpGet("cost/forecast")]
    public async Task<IActionResult> GetCostForecast([FromQuery] int days = 30)
    {
        try
        {
            var userId = GetCurrentUserId();
            var forecast = await _costAnalyticsService.ForecastCostsAsync(userId, days);

            return Ok(new
            {
                success = true,
                forecast = forecast,
                forecastDays = days
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cost forecast");
            return StatusCode(500, new { success = false, error = "Failed to retrieve cost forecast" });
        }
    }

    /// <summary>
    /// Get budget information
    /// </summary>
    [HttpGet("cost/budgets")]
    public async Task<IActionResult> GetBudgets()
    {
        try
        {
            var userId = GetCurrentUserId();
            var budgets = await _costManagementService.GetBudgetsAsync(userId, "User");

            return Ok(new
            {
                success = true,
                budgets = budgets
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting budgets");
            return StatusCode(500, new { success = false, error = "Failed to retrieve budgets" });
        }
    }

    /// <summary>
    /// Create a new budget
    /// </summary>
    [HttpPost("cost/budgets")]
    public async Task<IActionResult> CreateBudget([FromBody] BudgetManagement budget)
    {
        try
        {
            var userId = GetCurrentUserId();
            budget.EntityId = userId;
            budget.Type = "User";

            var createdBudget = await _costManagementService.CreateBudgetAsync(budget);

            return CreatedAtAction(nameof(GetBudgets), new { id = createdBudget.Id }, new
            {
                success = true,
                budget = createdBudget
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating budget");
            return StatusCode(500, new { success = false, error = "Failed to create budget" });
        }
    }

    /// <summary>
    /// Update an existing budget
    /// </summary>
    [HttpPut("cost/budgets/{budgetId}")]
    public async Task<IActionResult> UpdateBudget(string budgetId, [FromBody] BudgetManagement budget)
    {
        try
        {
            budget.Id = budgetId;
            var updatedBudget = await _costManagementService.UpdateBudgetAsync(budget);

            return Ok(new
            {
                success = true,
                budget = updatedBudget
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating budget {BudgetId}", budgetId);
            return StatusCode(500, new { success = false, error = "Failed to update budget" });
        }
    }

    /// <summary>
    /// Delete a budget
    /// </summary>
    [HttpDelete("cost/budgets/{budgetId}")]
    public async Task<IActionResult> DeleteBudget(string budgetId)
    {
        try
        {
            var deleted = await _costManagementService.DeleteBudgetAsync(budgetId);

            if (!deleted)
            {
                return NotFound(new { success = false, error = "Budget not found" });
            }

            return Ok(new
            {
                success = true,
                message = "Budget deleted successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting budget {BudgetId}", budgetId);
            return StatusCode(500, new { success = false, error = "Failed to delete budget" });
        }
    }

    /// <summary>
    /// Get cost optimization recommendations
    /// </summary>
    [HttpGet("cost/recommendations")]
    public async Task<IActionResult> GetOptimizationRecommendations()
    {
        try
        {
            var userId = GetCurrentUserId();
            var recommendations = await _costAnalyticsService.GenerateOptimizationRecommendationsAsync(userId);

            return Ok(new
            {
                success = true,
                recommendations = recommendations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimization recommendations");
            return StatusCode(500, new { success = false, error = "Failed to retrieve optimization recommendations" });
        }
    }

    /// <summary>
    /// Get ROI analysis
    /// </summary>
    [HttpGet("cost/roi")]
    public async Task<IActionResult> GetROIAnalysis(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var roi = await _costAnalyticsService.CalculateROIAsync(userId, startDate, endDate);

            return Ok(new
            {
                success = true,
                roi = roi
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ROI analysis");
            return StatusCode(500, new { success = false, error = "Failed to retrieve ROI analysis" });
        }
    }

    /// <summary>
    /// Get real-time cost metrics
    /// </summary>
    [HttpGet("cost/realtime")]
    public async Task<IActionResult> GetRealTimeCostMetrics()
    {
        try
        {
            var metrics = await _costAnalyticsService.GetRealTimeCostMetricsAsync();

            return Ok(new
            {
                success = true,
                metrics = metrics,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time cost metrics");
            return StatusCode(500, new { success = false, error = "Failed to retrieve real-time cost metrics" });
        }
    }

    #endregion

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }
}

/// <summary>
/// Request model for cost prediction
/// </summary>
public class CostPredictionRequest
{
    public string QueryId { get; set; } = string.Empty;
    public ModelSelectionCriteria Criteria { get; set; } = new();
}

/// <summary>
/// Request model for checking quota
/// </summary>
public class CheckQuotaRequest
{
    public string ResourceType { get; set; } = string.Empty;
    public int RequestedQuantity { get; set; }
}

/// <summary>
/// Request model for setting priority
/// </summary>
public class SetPriorityRequest
{
    public string UserId { get; set; } = string.Empty;
    public int Priority { get; set; }
}

/// <summary>
/// Request model for queueing requests
/// </summary>
public class QueueRequestRequest
{
    public string RequestId { get; set; } = string.Empty;
    public string ResourceType { get; set; } = string.Empty;
    public int Priority { get; set; }
}
