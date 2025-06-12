using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using static BIReportingCopilot.Core.ApplicationConstants;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// LLM Management Controller for comprehensive AI provider and model management
/// Provides endpoints for provider configuration, usage tracking, cost monitoring, and performance analytics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Analyst")]
public class LLMManagementController : ControllerBase
{
    private readonly ILLMManagementService _llmManagementService;
    private readonly ILogger<LLMManagementController> _logger;
    private readonly IWebHostEnvironment _environment;

    public LLMManagementController(
        ILLMManagementService llmManagementService,
        ILogger<LLMManagementController> logger,
        IWebHostEnvironment environment)
    {
        _llmManagementService = llmManagementService;
        _logger = logger;
        _environment = environment;
    }

    #region Provider Management

    /// <summary>
    /// Get all configured LLM providers
    /// </summary>
    [HttpGet("providers")]
    public async Task<ActionResult<List<LLMProviderConfig>>> GetProviders()
    {
        try
        {
            var providers = await _llmManagementService.GetProvidersAsync();
            return Ok(providers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM providers");
            return StatusCode(500, "Error retrieving LLM providers");
        }
    }

    /// <summary>
    /// Get a specific provider configuration
    /// </summary>
    [HttpGet("providers/{providerId}")]
    public async Task<ActionResult<LLMProviderConfig>> GetProvider(string providerId)
    {
        try
        {
            var provider = await _llmManagementService.GetProviderAsync(providerId);
            if (provider == null)
                return NotFound($"Provider {providerId} not found");

            return Ok(provider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider {ProviderId}", providerId);
            return StatusCode(500, "Error retrieving provider");
        }
    }

    /// <summary>
    /// Create or update a provider configuration (Admin only)
    /// </summary>
    [HttpPost("providers")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<LLMProviderConfig>> SaveProvider([FromBody] LLMProviderConfig provider)
    {
        try
        {
            var savedProvider = await _llmManagementService.SaveProviderAsync(provider);
            return Ok(savedProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving provider {ProviderId}", provider.ProviderId);
            return StatusCode(500, "Error saving provider");
        }
    }

    /// <summary>
    /// Delete a provider configuration (Admin only)
    /// </summary>
    [HttpDelete("providers/{providerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> DeleteProvider(string providerId)
    {
        try
        {
            var deleted = await _llmManagementService.DeleteProviderAsync(providerId);
            if (!deleted)
                return NotFound($"Provider {providerId} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting provider {ProviderId}", providerId);
            return StatusCode(500, "Error deleting provider");
        }
    }

    /// <summary>
    /// Test connection to a provider
    /// </summary>
    [HttpPost("providers/{providerId}/test")]
    public async Task<ActionResult<LLMProviderStatus>> TestProvider(string providerId)
    {
        try
        {
            var status = await _llmManagementService.TestProviderConnectionAsync(providerId);
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing provider {ProviderId}", providerId);
            return StatusCode(500, "Error testing provider connection");
        }
    }

    /// <summary>
    /// Get health status for all providers
    /// </summary>
    [HttpGet("providers/health")]
    public async Task<ActionResult<List<LLMProviderStatus>>> GetProviderHealth()
    {
        try
        {
            var healthStatuses = await _llmManagementService.GetProviderHealthStatusAsync();
            return Ok(healthStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving provider health status");
            return StatusCode(500, "Error retrieving provider health status");
        }
    }

    #endregion

    #region Model Management

    /// <summary>
    /// Get all configured models, optionally filtered by provider
    /// </summary>
    [HttpGet("models")]
    public async Task<ActionResult<List<LLMModelConfig>>> GetModels([FromQuery] string? providerId = null)
    {
        try
        {
            var models = await _llmManagementService.GetModelsAsync(providerId);
            return Ok(models);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM models");
            return StatusCode(500, "Error retrieving LLM models");
        }
    }

    /// <summary>
    /// Get a specific model configuration
    /// </summary>
    [HttpGet("models/{modelId}")]
    public async Task<ActionResult<LLMModelConfig>> GetModel(string modelId)
    {
        try
        {
            var model = await _llmManagementService.GetModelAsync(modelId);
            if (model == null)
                return NotFound($"Model {modelId} not found");

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving model {ModelId}", modelId);
            return StatusCode(500, "Error retrieving model");
        }
    }

    /// <summary>
    /// Create or update a model configuration
    /// </summary>
    [HttpPost("models")]
    public async Task<ActionResult<LLMModelConfig>> SaveModel([FromBody] LLMModelConfig model)
    {
        try
        {
            var savedModel = await _llmManagementService.SaveModelAsync(model);
            return Ok(savedModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving model {ModelId}", model.ModelId);
            return StatusCode(500, "Error saving model");
        }
    }

    /// <summary>
    /// Delete a model configuration
    /// </summary>
    [HttpDelete("models/{modelId}")]
    public async Task<ActionResult> DeleteModel(string modelId)
    {
        try
        {
            var deleted = await _llmManagementService.DeleteModelAsync(modelId);
            if (!deleted)
                return NotFound($"Model {modelId} not found");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting model {ModelId}", modelId);
            return StatusCode(500, "Error deleting model");
        }
    }

    /// <summary>
    /// Get default model for a specific use case
    /// </summary>
    [HttpGet("models/default/{useCase}")]
    public async Task<ActionResult<LLMModelConfig>> GetDefaultModel(string useCase)
    {
        try
        {
            var model = await _llmManagementService.GetDefaultModelAsync(useCase);
            if (model == null)
                return NotFound($"No default model found for use case {useCase}");

            return Ok(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving default model for use case {UseCase}", useCase);
            return StatusCode(500, "Error retrieving default model");
        }
    }

    /// <summary>
    /// Set default model for a use case
    /// </summary>
    [HttpPost("models/{modelId}/set-default/{useCase}")]
    public async Task<ActionResult> SetDefaultModel(string modelId, string useCase)
    {
        try
        {
            var success = await _llmManagementService.SetDefaultModelAsync(modelId, useCase);
            if (!success)
                return BadRequest("Failed to set default model");

            return Ok(new { message = $"Model {modelId} set as default for {useCase}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default model {ModelId} for use case {UseCase}", modelId, useCase);
            return StatusCode(500, "Error setting default model");
        }
    }

    #endregion

    #region Usage Tracking

    /// <summary>
    /// Get usage history with filtering options
    /// </summary>
    [HttpGet("usage/history")]
    public async Task<ActionResult<List<LLMUsageLog>>> GetUsageHistory(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? providerId = null,
        [FromQuery] string? modelId = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? requestType = null,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 100)
    {
        try
        {
            var history = await _llmManagementService.GetUsageHistoryAsync(
                startDate, endDate, providerId, modelId, userId, requestType, skip, take);
            return Ok(history);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage history");
            return StatusCode(500, "Error retrieving usage history");
        }
    }

    /// <summary>
    /// Get usage analytics for a date range
    /// </summary>
    [HttpGet("usage/analytics")]
    public async Task<ActionResult<LLMUsageAnalytics>> GetUsageAnalytics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? providerId = null,
        [FromQuery] string? modelId = null)
    {
        try
        {
            var analytics = await _llmManagementService.GetUsageAnalyticsAsync(
                startDate, endDate, providerId, modelId);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage analytics");
            return StatusCode(500, "Error retrieving usage analytics");
        }
    }

    /// <summary>
    /// Export usage data
    /// </summary>
    [HttpGet("usage/export")]
    public async Task<ActionResult> ExportUsageData(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string format = "csv")
    {
        try
        {
            var data = await _llmManagementService.ExportUsageDataAsync(startDate, endDate, format);
            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/json";
            var fileName = $"llm_usage_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}.{format}";
            
            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting usage data");
            return StatusCode(500, "Error exporting usage data");
        }
    }

    #endregion

    #region Cost Management

    /// <summary>
    /// Get cost summary for a date range
    /// </summary>
    [HttpGet("costs/summary")]
    public async Task<ActionResult<List<LLMCostSummary>>> GetCostSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string? providerId = null)
    {
        try
        {
            var summary = await _llmManagementService.GetCostSummaryAsync(startDate, endDate, providerId);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost summary");
            return StatusCode(500, "Error retrieving cost summary");
        }
    }

    /// <summary>
    /// Get current month cost
    /// </summary>
    [HttpGet("costs/current-month")]
    public async Task<ActionResult<decimal>> GetCurrentMonthCost([FromQuery] string? providerId = null)
    {
        try
        {
            var cost = await _llmManagementService.GetCurrentMonthCostAsync(providerId);
            return Ok(cost);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current month cost");
            return StatusCode(500, "Error retrieving current month cost");
        }
    }

    /// <summary>
    /// Get cost projections
    /// </summary>
    [HttpGet("costs/projections")]
    public async Task<ActionResult<Dictionary<string, decimal>>> GetCostProjections()
    {
        try
        {
            var projections = await _llmManagementService.GetCostProjectionsAsync();
            return Ok(projections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost projections");
            return StatusCode(500, "Error retrieving cost projections");
        }
    }

    /// <summary>
    /// Set cost limit for a provider (Admin only)
    /// </summary>
    [HttpPost("costs/limits/{providerId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult> SetCostLimit(string providerId, [FromBody] decimal monthlyLimit)
    {
        try
        {
            var success = await _llmManagementService.SetCostLimitAsync(providerId, monthlyLimit);
            if (!success)
                return BadRequest("Failed to set cost limit");

            return Ok(new { message = $"Cost limit set for provider {providerId}" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cost limit for provider {ProviderId}", providerId);
            return StatusCode(500, "Error setting cost limit");
        }
    }

    /// <summary>
    /// Get cost alerts
    /// </summary>
    [HttpGet("costs/alerts")]
    public async Task<ActionResult<List<CostAlert>>> GetCostAlerts()
    {
        try
        {
            var alerts = await _llmManagementService.GetCostAlertsAsync();
            return Ok(alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost alerts");
            return StatusCode(500, "Error retrieving cost alerts");
        }
    }

    #endregion

    #region Performance Monitoring

    /// <summary>
    /// Get performance metrics for providers
    /// </summary>
    [HttpGet("performance/metrics")]
    public async Task<ActionResult<Dictionary<string, ProviderPerformanceMetrics>>> GetPerformanceMetrics(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var metrics = await _llmManagementService.GetPerformanceMetricsAsync(startDate, endDate);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics");
            return StatusCode(500, "Error retrieving performance metrics");
        }
    }

    /// <summary>
    /// Get cache hit rates for LLM requests
    /// </summary>
    [HttpGet("performance/cache-hit-rates")]
    public async Task<ActionResult<Dictionary<string, double>>> GetCacheHitRates()
    {
        try
        {
            var hitRates = await _llmManagementService.GetCacheHitRatesAsync();
            return Ok(hitRates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache hit rates");
            return StatusCode(500, "Error retrieving cache hit rates");
        }
    }

    /// <summary>
    /// Get error analysis for providers
    /// </summary>
    [HttpGet("performance/error-analysis")]
    public async Task<ActionResult<Dictionary<string, ErrorAnalysis>>> GetErrorAnalysis(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        try
        {
            var analysis = await _llmManagementService.GetErrorAnalysisAsync(startDate, endDate);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving error analysis");
            return StatusCode(500, "Error retrieving error analysis");
        }
    }

    #endregion

    #region Dashboard Summary

    /// <summary>
    /// Get comprehensive dashboard summary
    /// </summary>
    [HttpGet("dashboard/summary")]
    public async Task<ActionResult<object>> GetDashboardSummary()
    {
        try
        {
            var endDate = DateTime.UtcNow;
            var startDate = endDate.AddDays(-30);

            var providers = await _llmManagementService.GetProvidersAsync();
            var healthStatuses = await _llmManagementService.GetCachedProviderHealthStatusAsync();
            var currentMonthCost = await _llmManagementService.GetCurrentMonthCostAsync();
            var usageAnalytics = await _llmManagementService.GetUsageAnalyticsAsync(startDate, endDate);
            var performanceMetrics = await _llmManagementService.GetPerformanceMetricsAsync(startDate, endDate);
            var costAlerts = await _llmManagementService.GetCostAlertsAsync();

            // Debug logging
            _logger.LogInformation("Dashboard Summary Debug - Start: {StartDate}, End: {EndDate}", startDate, endDate);
            _logger.LogInformation("Dashboard Summary Debug - Total Requests: {TotalRequests}, Total Cost: {TotalCost}",
                usageAnalytics.TotalRequests, usageAnalytics.TotalCost);

            var summary = new
            {
                providers = new
                {
                    total = providers.Count,
                    enabled = providers.Count(p => p.IsEnabled),
                    healthy = healthStatuses.Count(h => h.IsHealthy)
                },
                usage = new
                {
                    totalRequests = usageAnalytics.TotalRequests,
                    totalTokens = usageAnalytics.TotalTokens,
                    averageResponseTime = usageAnalytics.AverageResponseTime,
                    successRate = usageAnalytics.SuccessRate
                },
                costs = new
                {
                    currentMonth = currentMonthCost,
                    total30Days = usageAnalytics.TotalCost,
                    activeAlerts = costAlerts.Count(a => a.IsEnabled)
                },
                performance = new
                {
                    averageResponseTime = performanceMetrics.Values.Any()
                        ? performanceMetrics.Values.Average(m => m.AverageResponseTime)
                        : 0,
                    overallSuccessRate = performanceMetrics.Values.Any()
                        ? performanceMetrics.Values.Average(m => m.SuccessRate)
                        : 0,
                    totalErrors = performanceMetrics.Values.Sum(m => m.ErrorsByType.Values.Sum())
                },
                lastUpdated = DateTime.UtcNow
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard summary");
            return StatusCode(500, "Error retrieving dashboard summary");
        }
    }



    #endregion
}
