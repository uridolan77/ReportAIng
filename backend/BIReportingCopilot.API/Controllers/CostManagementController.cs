using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Cost management and optimization controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CostManagementController : ControllerBase
{
    private readonly ICostManagementService _costManagementService;
    private readonly ICostAnalyticsService _costAnalyticsService;
    private readonly ILogger<CostManagementController> _logger;

    public CostManagementController(
        ICostManagementService costManagementService,
        ICostAnalyticsService costAnalyticsService,
        ILogger<CostManagementController> logger)
    {
        _costManagementService = costManagementService;
        _costAnalyticsService = costAnalyticsService;
        _logger = logger;
    }

    /// <summary>
    /// Get cost analytics summary
    /// </summary>
    [HttpGet("analytics")]
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
    [HttpGet("history")]
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
    [HttpGet("total")]
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
    [HttpGet("breakdown/{dimension}")]
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
    [HttpGet("trends")]
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
    [HttpPost("predict")]
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
    [HttpGet("forecast")]
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
    [HttpGet("budgets")]
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
    [HttpPost("budgets")]
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
    [HttpPut("budgets/{budgetId}")]
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
    [HttpDelete("budgets/{budgetId}")]
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
    [HttpGet("recommendations")]
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
    [HttpGet("roi")]
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
    [HttpGet("realtime")]
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
