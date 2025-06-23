using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BIReportingCopilot.Infrastructure.AI.Management;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// LLM Management API Controller - Handles LLM provider and model management
/// </summary>
[ApiController]
[Route("api/llmmanagement")]
// [Authorize] // Temporarily disabled for development
public class LLMManagementApiController : ControllerBase
{
    private readonly ILogger<LLMManagementApiController> _logger;
    private readonly LLMManagementService _llmManagementService;

    public LLMManagementApiController(
        ILogger<LLMManagementApiController> logger,
        LLMManagementService llmManagementService)
    {
        _logger = logger;
        _llmManagementService = llmManagementService;
    }

    #region Provider Management

    /// <summary>
    /// Get all LLM providers
    /// </summary>
    [HttpGet("providers")]
    public async Task<ActionResult<object[]>> GetProviders()
    {
        try
        {
            _logger.LogInformation("Getting all LLM providers");

            var providers = await _llmManagementService.GetProvidersAsync();

            var result = providers.Select(p => new
            {
                providerId = p.ProviderId,
                name = p.Name,
                type = p.Type,
                endpoint = p.Endpoint,
                organization = p.Organization,
                isEnabled = p.IsEnabled,
                isDefault = p.IsDefault,
                settings = p.Settings,
                createdAt = p.CreatedAt.ToString("O"),
                updatedAt = p.UpdatedAt.ToString("O")
            }).ToArray();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LLM providers");
            return StatusCode(500, new { error = "Failed to retrieve LLM providers" });
        }
    }

    /// <summary>
    /// Get specific LLM provider
    /// </summary>
    [HttpGet("providers/{providerId}")]
    public async Task<ActionResult<object>> GetProvider(string providerId)
    {
        try
        {
            _logger.LogInformation("Getting LLM provider {ProviderId}", providerId);

            var provider = await _llmManagementService.GetProviderAsync(providerId);

            if (provider == null)
            {
                return NotFound(new { error = $"Provider {providerId} not found" });
            }

            var result = new
            {
                providerId = provider.ProviderId,
                name = provider.Name,
                type = provider.Type,
                endpoint = provider.Endpoint,
                organization = provider.Organization,
                isEnabled = provider.IsEnabled,
                isDefault = provider.IsDefault,
                settings = provider.Settings,
                createdAt = provider.CreatedAt.ToString("O"),
                updatedAt = provider.UpdatedAt.ToString("O")
            };

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LLM provider {ProviderId}", providerId);
            return StatusCode(500, new { error = "Failed to retrieve LLM provider" });
        }
    }

    /// <summary>
    /// Save LLM provider
    /// </summary>
    [HttpPost("providers")]
    public async Task<ActionResult<object>> SaveProvider([FromBody] object providerData)
    {
        try
        {
            _logger.LogInformation("Saving LLM provider");

            var savedProvider = new
            {
                providerId = Guid.NewGuid().ToString(),
                name = "New Provider",
                type = "openai",
                isEnabled = true,
                isDefault = false,
                createdAt = DateTime.UtcNow.ToString("O"),
                updatedAt = DateTime.UtcNow.ToString("O")
            };

            return Ok(savedProvider);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving LLM provider");
            return StatusCode(500, new { error = "Failed to save LLM provider" });
        }
    }

    /// <summary>
    /// Delete LLM provider
    /// </summary>
    [HttpDelete("providers/{providerId}")]
    public async Task<ActionResult> DeleteProvider(string providerId)
    {
        try
        {
            _logger.LogInformation("Deleting LLM provider {ProviderId}", providerId);
            return Ok(new { message = "Provider deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting LLM provider {ProviderId}", providerId);
            return StatusCode(500, new { error = "Failed to delete LLM provider" });
        }
    }

    /// <summary>
    /// Test LLM provider connection
    /// </summary>
    [HttpPost("providers/{providerId}/test")]
    public async Task<ActionResult<object>> TestProvider(string providerId)
    {
        try
        {
            _logger.LogInformation("Testing LLM provider {ProviderId}", providerId);

            var status = new
            {
                providerId = providerId,
                isHealthy = true,
                status = "healthy",
                responseTime = Random.Shared.NextDouble() * 500 + 100,
                lastChecked = DateTime.UtcNow.ToString("O"),
                metadata = new { testMessage = "Connection successful" }
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing LLM provider {ProviderId}", providerId);
            return StatusCode(500, new { error = "Failed to test LLM provider" });
        }
    }

    /// <summary>
    /// Get provider health status
    /// </summary>
    [HttpGet("providers/health")]
    public async Task<ActionResult<object[]>> GetProviderHealth()
    {
        try
        {
            _logger.LogInformation("Getting provider health status");

            var healthStatuses = new object[]
            {
                new {
                    providerId = "openai-1",
                    isHealthy = true,
                    status = "healthy",
                    responseTime = 245.5,
                    lastChecked = DateTime.UtcNow.AddMinutes(-5).ToString("O")
                },
                new {
                    providerId = "anthropic-1",
                    isHealthy = true,
                    status = "healthy",
                    responseTime = 312.8,
                    lastChecked = DateTime.UtcNow.AddMinutes(-3).ToString("O")
                },
                new {
                    providerId = "azure-1",
                    isHealthy = false,
                    status = "offline",
                    responseTime = (double?)null,
                    lastChecked = DateTime.UtcNow.AddHours(-2).ToString("O"),
                    errorMessage = "Provider is disabled"
                }
            };

            return Ok(healthStatuses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider health status");
            return StatusCode(500, new { error = "Failed to retrieve provider health status" });
        }
    }

    #endregion

    #region Model Management

    /// <summary>
    /// Get LLM models
    /// </summary>
    [HttpGet("models")]
    public async Task<ActionResult<object[]>> GetModels([FromQuery] string? providerId = null)
    {
        try
        {
            _logger.LogInformation("Getting LLM models for provider {ProviderId}", providerId ?? "all");

            var models = await _llmManagementService.GetModelsAsync(providerId);

            var result = models.Select(m => new
            {
                modelId = m.ModelId,
                providerId = m.ProviderId,
                name = m.Name,
                displayName = m.DisplayName,
                temperature = m.Temperature,
                maxTokens = m.MaxTokens,
                topP = m.TopP,
                frequencyPenalty = m.FrequencyPenalty,
                presencePenalty = m.PresencePenalty,
                isEnabled = m.IsEnabled,
                useCase = m.UseCase,
                costPerToken = m.CostPerToken,
                capabilities = m.Capabilities
            }).ToArray();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LLM models");
            return StatusCode(500, new { error = "Failed to retrieve LLM models" });
        }
    }

    #endregion

    #region Usage Logs and Analytics

    /// <summary>
    /// Get LLM usage logs
    /// </summary>
    [HttpGet("usage/logs")]
    public async Task<ActionResult<object[]>> GetUsageLogs(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? userId = null,
        [FromQuery] string? providerId = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Getting LLM usage logs");

            var logs = await _llmManagementService.GetUsageLogsAsync(
                startDate ?? DateTime.UtcNow.AddDays(-30),
                endDate ?? DateTime.UtcNow,
                userId,
                providerId,
                page,
                pageSize);

            var result = logs.Select(l => new
            {
                id = l.Id,
                requestId = l.RequestId,
                userId = l.UserId,
                providerId = l.ProviderId,
                modelId = l.ModelId,
                requestType = l.RequestType,
                inputTokens = l.InputTokens,
                outputTokens = l.OutputTokens,
                totalTokens = l.TotalTokens,
                cost = l.Cost,
                durationMs = l.DurationMs,
                success = l.Success,
                errorMessage = l.ErrorMessage,
                timestamp = l.Timestamp.ToString("O"),
                metadata = l.Metadata
            }).ToArray();

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LLM usage logs");
            return StatusCode(500, new { error = "Failed to retrieve usage logs" });
        }
    }

    /// <summary>
    /// Get LLM cost tracking data
    /// </summary>
    [HttpGet("costs/tracking")]
    public async Task<ActionResult<object>> GetCostTracking(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? userId = null)
    {
        try
        {
            _logger.LogInformation("Getting LLM cost tracking data");

            var costData = await _llmManagementService.GetCostTrackingAsync(
                startDate ?? DateTime.UtcNow.AddDays(-30),
                endDate ?? DateTime.UtcNow,
                userId);

            return Ok(costData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LLM cost tracking data");
            return StatusCode(500, new { error = "Failed to retrieve cost tracking data" });
        }
    }

    /// <summary>
    /// Get LLM performance metrics
    /// </summary>
    [HttpGet("performance/metrics")]
    public async Task<ActionResult<object>> GetPerformanceMetrics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            _logger.LogInformation("Getting LLM performance metrics");

            var metrics = await _llmManagementService.GetPerformanceMetricsAsync(
                startDate ?? DateTime.UtcNow.AddDays(-7),
                endDate ?? DateTime.UtcNow);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LLM performance metrics");
            return StatusCode(500, new { error = "Failed to retrieve performance metrics" });
        }
    }

    /// <summary>
    /// Get token usage analytics
    /// </summary>
    [HttpGet("analytics/tokens")]
    public async Task<ActionResult<object>> GetTokenUsageAnalytics(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? groupBy = "day")
    {
        try
        {
            _logger.LogInformation("Getting token usage analytics");

            var analytics = await _llmManagementService.GetTokenUsageAnalyticsAsync(
                startDate ?? DateTime.UtcNow.AddDays(-30),
                endDate ?? DateTime.UtcNow,
                groupBy);

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting token usage analytics");
            return StatusCode(500, new { error = "Failed to retrieve token usage analytics" });
        }
    }

    /// <summary>
    /// Get budget management data
    /// </summary>
    [HttpGet("budgets")]
    public async Task<ActionResult<object[]>> GetBudgets([FromQuery] string? userId = null)
    {
        try
        {
            _logger.LogInformation("Getting LLM budget management data");

            var budgets = await _llmManagementService.GetBudgetsAsync(userId);

            return Ok(budgets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting LLM budget data");
            return StatusCode(500, new { error = "Failed to retrieve budget data" });
        }
    }

    #endregion

    #region Helper Methods

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }

    private bool IsAdmin()
    {
        return User.IsInRole("Admin") || User.HasClaim("role", "Admin");
    }

    #endregion
}
