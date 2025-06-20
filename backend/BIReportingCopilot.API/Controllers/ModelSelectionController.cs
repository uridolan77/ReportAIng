using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Dynamic model selection and optimization controller
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ModelSelectionController : ControllerBase
{
    private readonly IDynamicModelSelectionService _modelSelectionService;
    private readonly ILogger<ModelSelectionController> _logger;

    public ModelSelectionController(
        IDynamicModelSelectionService modelSelectionService,
        ILogger<ModelSelectionController> logger)
    {
        _modelSelectionService = modelSelectionService;
        _logger = logger;
    }

    /// <summary>
    /// Select optimal model based on criteria
    /// </summary>
    [HttpPost("select")]
    public async Task<IActionResult> SelectOptimalModel([FromBody] ModelSelectionCriteria criteria)
    {
        try
        {
            var result = await _modelSelectionService.SelectOptimalModelAsync(criteria);

            return Ok(new
            {
                success = true,
                result = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting optimal model");
            return StatusCode(500, new { success = false, error = "Failed to select optimal model" });
        }
    }

    /// <summary>
    /// Get available models
    /// </summary>
    [HttpGet("available")]
    public async Task<IActionResult> GetAvailableModels()
    {
        try
        {
            var models = await _modelSelectionService.GetAvailableModelsAsync();

            return Ok(new
            {
                success = true,
                models = models,
                count = models.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available models");
            return StatusCode(500, new { success = false, error = "Failed to retrieve available models" });
        }
    }

    /// <summary>
    /// Get model information
    /// </summary>
    [HttpGet("info/{providerId}/{modelId}")]
    public async Task<IActionResult> GetModelInfo(string providerId, string modelId)
    {
        try
        {
            var modelInfo = await _modelSelectionService.GetModelInfoAsync(providerId, modelId);

            if (modelInfo == null)
            {
                return NotFound(new { success = false, error = "Model not found" });
            }

            return Ok(new
            {
                success = true,
                modelInfo = modelInfo
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model info for {ProviderId}/{ModelId}", providerId, modelId);
            return StatusCode(500, new { success = false, error = "Failed to retrieve model information" });
        }
    }

    /// <summary>
    /// Track model performance
    /// </summary>
    [HttpPost("performance")]
    public async Task<IActionResult> TrackModelPerformance([FromBody] TrackPerformanceRequest request)
    {
        try
        {
            await _modelSelectionService.TrackModelPerformanceAsync(
                request.ProviderId, 
                request.ModelId, 
                request.DurationMs, 
                request.Cost, 
                request.Accuracy);

            return Ok(new
            {
                success = true,
                message = "Model performance tracked successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking model performance");
            return StatusCode(500, new { success = false, error = "Failed to track model performance" });
        }
    }

    /// <summary>
    /// Get model performance metrics
    /// </summary>
    [HttpGet("performance/{providerId}/{modelId}")]
    public async Task<IActionResult> GetModelPerformanceMetrics(string providerId, string modelId)
    {
        try
        {
            var metrics = await _modelSelectionService.GetModelPerformanceMetricsAsync(providerId, modelId);

            return Ok(new
            {
                success = true,
                providerId = providerId,
                modelId = modelId,
                metrics = metrics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model performance metrics for {ProviderId}/{ModelId}", providerId, modelId);
            return StatusCode(500, new { success = false, error = "Failed to retrieve model performance metrics" });
        }
    }

    /// <summary>
    /// Select model with failover
    /// </summary>
    [HttpPost("select/failover")]
    public async Task<IActionResult> SelectWithFailover([FromBody] FailoverSelectionRequest request)
    {
        try
        {
            var result = await _modelSelectionService.SelectWithFailoverAsync(request.Criteria, request.ExcludeProviders);

            return Ok(new
            {
                success = true,
                result = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting model with failover");
            return StatusCode(500, new { success = false, error = "Failed to select model with failover" });
        }
    }

    /// <summary>
    /// Check provider availability
    /// </summary>
    [HttpGet("availability/{providerId}")]
    public async Task<IActionResult> IsProviderAvailable(string providerId)
    {
        try
        {
            var available = await _modelSelectionService.IsProviderAvailableAsync(providerId);

            return Ok(new
            {
                success = true,
                providerId = providerId,
                available = available
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking provider availability for {ProviderId}", providerId);
            return StatusCode(500, new { success = false, error = "Failed to check provider availability" });
        }
    }

    /// <summary>
    /// Mark provider as unavailable
    /// </summary>
    [HttpPost("availability/{providerId}/unavailable")]
    public async Task<IActionResult> MarkProviderUnavailable(string providerId, [FromBody] MarkUnavailableRequest request)
    {
        try
        {
            await _modelSelectionService.MarkProviderUnavailableAsync(providerId, request.Duration);

            return Ok(new
            {
                success = true,
                message = $"Provider {providerId} marked as unavailable for {request.Duration}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking provider unavailable for {ProviderId}", providerId);
            return StatusCode(500, new { success = false, error = "Failed to mark provider unavailable" });
        }
    }

    /// <summary>
    /// Get model capabilities
    /// </summary>
    [HttpGet("capabilities/{providerId}/{modelId}")]
    public async Task<IActionResult> GetModelCapabilities(string providerId, string modelId)
    {
        try
        {
            var capabilities = await _modelSelectionService.GetModelCapabilitiesAsync(providerId, modelId);

            return Ok(new
            {
                success = true,
                providerId = providerId,
                modelId = modelId,
                capabilities = capabilities
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model capabilities for {ProviderId}/{ModelId}", providerId, modelId);
            return StatusCode(500, new { success = false, error = "Failed to retrieve model capabilities" });
        }
    }

    /// <summary>
    /// Update model capabilities
    /// </summary>
    [HttpPut("capabilities/{providerId}/{modelId}")]
    public async Task<IActionResult> UpdateModelCapabilities(string providerId, string modelId, [FromBody] Dictionary<string, object> capabilities)
    {
        try
        {
            await _modelSelectionService.UpdateModelCapabilitiesAsync(providerId, modelId, capabilities);

            return Ok(new
            {
                success = true,
                message = $"Capabilities updated for {providerId}/{modelId}"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating model capabilities for {ProviderId}/{ModelId}", providerId, modelId);
            return StatusCode(500, new { success = false, error = "Failed to update model capabilities" });
        }
    }

    /// <summary>
    /// Get model selection statistics
    /// </summary>
    [HttpGet("statistics")]
    public async Task<IActionResult> GetModelSelectionStats(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        try
        {
            var stats = await _modelSelectionService.GetModelSelectionStatsAsync(startDate, endDate);

            return Ok(new
            {
                success = true,
                stats = stats,
                period = new { startDate, endDate }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model selection statistics");
            return StatusCode(500, new { success = false, error = "Failed to retrieve model selection statistics" });
        }
    }

    /// <summary>
    /// Get model selection accuracy
    /// </summary>
    [HttpGet("accuracy/{providerId}/{modelId}")]
    public async Task<IActionResult> GetModelSelectionAccuracy(string providerId, string modelId)
    {
        try
        {
            var accuracy = await _modelSelectionService.GetModelSelectionAccuracyAsync(providerId, modelId);

            return Ok(new
            {
                success = true,
                providerId = providerId,
                modelId = modelId,
                accuracy = accuracy
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model selection accuracy for {ProviderId}/{ModelId}", providerId, modelId);
            return StatusCode(500, new { success = false, error = "Failed to retrieve model selection accuracy" });
        }
    }

    /// <summary>
    /// Get model recommendations based on usage patterns
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<IActionResult> GetModelRecommendations([FromQuery] string? queryComplexity = null)
    {
        try
        {
            var criteria = new ModelSelectionCriteria
            {
                QueryComplexity = queryComplexity ?? "medium",
                Priority = "Balanced"
            };

            var result = await _modelSelectionService.SelectOptimalModelAsync(criteria);

            return Ok(new
            {
                success = true,
                recommendation = result,
                criteria = criteria
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model recommendations");
            return StatusCode(500, new { success = false, error = "Failed to retrieve model recommendations" });
        }
    }

    /// <summary>
    /// Compare models performance
    /// </summary>
    [HttpPost("compare")]
    public async Task<IActionResult> CompareModels([FromBody] CompareModelsRequest request)
    {
        try
        {
            var comparisons = new List<object>();

            foreach (var model in request.Models)
            {
                var metrics = await _modelSelectionService.GetModelPerformanceMetricsAsync(model.ProviderId, model.ModelId);
                var info = await _modelSelectionService.GetModelInfoAsync(model.ProviderId, model.ModelId);

                comparisons.Add(new
                {
                    providerId = model.ProviderId,
                    modelId = model.ModelId,
                    metrics = metrics,
                    info = info
                });
            }

            return Ok(new
            {
                success = true,
                comparisons = comparisons
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing models");
            return StatusCode(500, new { success = false, error = "Failed to compare models" });
        }
    }
}

/// <summary>
/// Request model for tracking performance
/// </summary>
public class TrackPerformanceRequest
{
    public string ProviderId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public decimal Cost { get; set; }
    public double Accuracy { get; set; }
}

/// <summary>
/// Request model for failover selection
/// </summary>
public class FailoverSelectionRequest
{
    public ModelSelectionCriteria Criteria { get; set; } = new();
    public List<string> ExcludeProviders { get; set; } = new();
}

/// <summary>
/// Request model for marking provider unavailable
/// </summary>
public class MarkUnavailableRequest
{
    public TimeSpan Duration { get; set; }
}

/// <summary>
/// Request model for comparing models
/// </summary>
public class CompareModelsRequest
{
    public List<ModelIdentifier> Models { get; set; } = new();
}

/// <summary>
/// Model identifier for comparisons
/// </summary>
public class ModelIdentifier
{
    public string ProviderId { get; set; } = string.Empty;
    public string ModelId { get; set; } = string.Empty;
}
