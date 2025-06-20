using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.Infrastructure.CostOptimization;

/// <summary>
/// Dynamic model selection service for intelligent AI provider and model selection
/// </summary>
public class DynamicModelSelectionService : IDynamicModelSelectionService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<DynamicModelSelectionService> _logger;
    private readonly IAIProviderFactory _providerFactory;
    private readonly ICostManagementService _costManagementService;
    
    private readonly Dictionary<string, ModelOption> _modelCapabilities = new();
    private readonly Dictionary<string, DateTime> _providerAvailability = new();
    private readonly Dictionary<string, List<PerformanceMetric>> _performanceHistory = new();
    private DateTime _lastCapabilitiesUpdate = DateTime.MinValue;
    private readonly TimeSpan _capabilitiesUpdateInterval = TimeSpan.FromHours(1);

    public DynamicModelSelectionService(
        BICopilotContext context,
        ILogger<DynamicModelSelectionService> logger,
        IAIProviderFactory providerFactory,
        ICostManagementService costManagementService)
    {
        _context = context;
        _logger = logger;
        _providerFactory = providerFactory;
        _costManagementService = costManagementService;
        InitializeModelCapabilities();
    }

    #region Model Selection

    public async Task<ModelSelectionResult> SelectOptimalModelAsync(ModelSelectionCriteria criteria, CancellationToken cancellationToken = default)
    {
        try
        {
            await RefreshModelCapabilitiesIfNeededAsync(cancellationToken);
            
            var availableModels = await GetAvailableModelsAsync(cancellationToken);
            var scoredModels = new List<(ModelOption model, double score)>();

            foreach (var model in availableModels.Where(m => m.IsAvailable))
            {
                var score = await CalculateModelScoreAsync(model, criteria, cancellationToken);
                scoredModels.Add((model, score));
            }

            var bestModel = scoredModels
                .OrderByDescending(x => x.score)
                .FirstOrDefault();

            if (bestModel.model == null)
            {
                throw new InvalidOperationException("No suitable model found for the given criteria");
            }

            var result = new ModelSelectionResult
            {
                SelectedModel = bestModel.model.ModelId,
                ProviderId = bestModel.model.ProviderId,
                EstimatedCost = bestModel.model.EstimatedCost,
                EstimatedDurationMs = bestModel.model.EstimatedDurationMs,
                ConfidenceScore = bestModel.score,
                Reasoning = GenerateSelectionReasoning(bestModel.model, criteria, bestModel.score),
                AlternativeOptions = scoredModels
                    .Where(x => x.model != bestModel.model)
                    .OrderByDescending(x => x.score)
                    .Take(3)
                    .Select(x => x.model)
                    .ToList()
            };

            _logger.LogInformation("Selected model {ModelId} from {ProviderId} with score {Score:F2} for criteria: {Criteria}", 
                result.SelectedModel, result.ProviderId, bestModel.score, JsonSerializer.Serialize(criteria));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting optimal model");
            throw;
        }
    }

    public async Task<List<ModelOption>> GetAvailableModelsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await RefreshModelCapabilitiesIfNeededAsync(cancellationToken);
            
            var models = new List<ModelOption>();
            var providers = _providerFactory.GetAvailableProviders();

            foreach (var providerId in providers)
            {
                if (await IsProviderAvailableAsync(providerId, cancellationToken))
                {
                    var providerModels = GetModelsForProvider(providerId);
                    models.AddRange(providerModels);
                }
            }

            return models;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available models");
            return new List<ModelOption>();
        }
    }

    public async Task<ModelOption?> GetModelInfoAsync(string providerId, string modelId, CancellationToken cancellationToken = default)
    {
        try
        {
            await RefreshModelCapabilitiesIfNeededAsync(cancellationToken);
            
            var key = $"{providerId}:{modelId}";
            if (_modelCapabilities.TryGetValue(key, out var model))
            {
                // Update with latest performance metrics
                var metrics = await GetModelPerformanceMetricsAsync(providerId, modelId, cancellationToken);
                if (metrics.ContainsKey("accuracy"))
                {
                    model.AccuracyScore = metrics["accuracy"];
                }
                
                return model;
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model info for {ProviderId}/{ModelId}", providerId, modelId);
            return null;
        }
    }

    #endregion

    #region Model Performance Tracking

    public async Task TrackModelPerformanceAsync(string providerId, string modelId, long durationMs, decimal cost, double accuracy, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{providerId}:{modelId}";
            var metric = new PerformanceMetric
            {
                MetricName = "model_performance",
                Value = accuracy,
                Unit = "accuracy_score",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object>
                {
                    ["duration_ms"] = durationMs,
                    ["cost"] = cost,
                    ["provider_id"] = providerId,
                    ["model_id"] = modelId
                }
            };

            if (!_performanceHistory.ContainsKey(key))
            {
                _performanceHistory[key] = new List<PerformanceMetric>();
            }

            _performanceHistory[key].Add(metric);

            // Keep only last 100 metrics per model
            if (_performanceHistory[key].Count > 100)
            {
                _performanceHistory[key] = _performanceHistory[key]
                    .OrderByDescending(m => m.Timestamp)
                    .Take(100)
                    .ToList();
            }

            // Update model capabilities with latest performance
            if (_modelCapabilities.TryGetValue(key, out var model))
            {
                model.AccuracyScore = accuracy;
                model.EstimatedDurationMs = durationMs;
                model.EstimatedCost = cost;
            }

            _logger.LogDebug("Tracked performance for {ProviderId}/{ModelId}: Duration={DurationMs}ms, Cost={Cost}, Accuracy={Accuracy}", 
                providerId, modelId, durationMs, cost, accuracy);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking model performance");
        }
    }

    public async Task<Dictionary<string, double>> GetModelPerformanceMetricsAsync(string providerId, string modelId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{providerId}:{modelId}";
            var metrics = new Dictionary<string, double>();

            if (_performanceHistory.TryGetValue(key, out var history) && history.Any())
            {
                var recentMetrics = history
                    .Where(m => m.Timestamp >= DateTime.UtcNow.AddDays(-7))
                    .ToList();

                if (recentMetrics.Any())
                {
                    metrics["accuracy"] = recentMetrics.Average(m => m.Value);
                    metrics["avg_duration_ms"] = recentMetrics.Average(m => 
                        m.Metadata.ContainsKey("duration_ms") ? Convert.ToDouble(m.Metadata["duration_ms"]) : 0);
                    metrics["avg_cost"] = recentMetrics.Average(m => 
                        m.Metadata.ContainsKey("cost") ? Convert.ToDouble(m.Metadata["cost"]) : 0);
                    metrics["request_count"] = recentMetrics.Count;
                }
            }

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model performance metrics");
            return new Dictionary<string, double>();
        }
    }

    #endregion

    #region Provider Failover

    public async Task<ModelSelectionResult> SelectWithFailoverAsync(ModelSelectionCriteria criteria, List<string> excludeProviders, CancellationToken cancellationToken = default)
    {
        try
        {
            var availableModels = await GetAvailableModelsAsync(cancellationToken);
            var filteredModels = availableModels
                .Where(m => !excludeProviders.Contains(m.ProviderId, StringComparer.OrdinalIgnoreCase))
                .ToList();

            if (!filteredModels.Any())
            {
                throw new InvalidOperationException("No available models after excluding specified providers");
            }

            // Temporarily update criteria to prefer available providers
            var failoverCriteria = new ModelSelectionCriteria
            {
                QueryComplexity = criteria.QueryComplexity,
                MaxCost = criteria.MaxCost,
                MaxDurationMs = criteria.MaxDurationMs,
                MinAccuracy = criteria.MinAccuracy,
                Priority = "Availability", // Override priority for failover
                CustomCriteria = criteria.CustomCriteria
            };

            return await SelectOptimalModelAsync(failoverCriteria, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting model with failover");
            throw;
        }
    }

    public async Task<bool> IsProviderAvailableAsync(string providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if provider was recently marked as unavailable
            if (_providerAvailability.TryGetValue(providerId, out var unavailableUntil))
            {
                if (DateTime.UtcNow < unavailableUntil)
                {
                    return false;
                }
                else
                {
                    // Remove expired unavailability
                    _providerAvailability.Remove(providerId);
                }
            }

            // Try to get the provider and check if it's configured
            try
            {
                var provider = await _providerFactory.CreateProviderAsync(providerId, cancellationToken);
                return provider.IsConfigured && await provider.IsAvailableAsync(cancellationToken);
            }
            catch
            {
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking provider availability: {ProviderId}", providerId);
            return false;
        }
    }

    public async Task MarkProviderUnavailableAsync(string providerId, TimeSpan unavailableDuration, CancellationToken cancellationToken = default)
    {
        try
        {
            var unavailableUntil = DateTime.UtcNow.Add(unavailableDuration);
            _providerAvailability[providerId] = unavailableUntil;

            _logger.LogWarning("Marked provider {ProviderId} as unavailable until {UnavailableUntil}", 
                providerId, unavailableUntil);

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking provider unavailable: {ProviderId}", providerId);
        }
    }

    #endregion

    #region Model Capabilities

    public async Task<Dictionary<string, object>> GetModelCapabilitiesAsync(string providerId, string modelId, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{providerId}:{modelId}";
            if (_modelCapabilities.TryGetValue(key, out var model))
            {
                return model.Capabilities;
            }

            return new Dictionary<string, object>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model capabilities");
            return new Dictionary<string, object>();
        }
    }

    public async Task UpdateModelCapabilitiesAsync(string providerId, string modelId, Dictionary<string, object> capabilities, CancellationToken cancellationToken = default)
    {
        try
        {
            var key = $"{providerId}:{modelId}";
            if (_modelCapabilities.TryGetValue(key, out var model))
            {
                model.Capabilities = capabilities;
                _logger.LogInformation("Updated capabilities for {ProviderId}/{ModelId}", providerId, modelId);
            }

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating model capabilities");
        }
    }

    #endregion

    #region Selection Analytics

    public async Task<Dictionary<string, int>> GetModelSelectionStatsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            // This would typically query a database of selection history
            // For now, return mock data based on performance history
            var stats = new Dictionary<string, int>();

            foreach (var kvp in _performanceHistory)
            {
                var metrics = kvp.Value;
                if (startDate.HasValue)
                    metrics = metrics.Where(m => m.Timestamp >= startDate.Value).ToList();
                if (endDate.HasValue)
                    metrics = metrics.Where(m => m.Timestamp <= endDate.Value).ToList();

                stats[kvp.Key] = metrics.Count;
            }

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model selection stats");
            return new Dictionary<string, int>();
        }
    }

    public async Task<double> GetModelSelectionAccuracyAsync(string providerId, string modelId, CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await GetModelPerformanceMetricsAsync(providerId, modelId, cancellationToken);
            return metrics.TryGetValue("accuracy", out var accuracy) ? accuracy : 0.0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting model selection accuracy");
            return 0.0;
        }
    }

    #endregion

    #region Helper Methods

    private void InitializeModelCapabilities()
    {
        // Initialize with default model capabilities
        var defaultModels = new[]
        {
            new ModelOption
            {
                ProviderId = "openai",
                ModelId = "gpt-4",
                EstimatedCost = 0.03m,
                EstimatedDurationMs = 5000,
                AccuracyScore = 0.95,
                IsAvailable = true,
                Capabilities = new Dictionary<string, object>
                {
                    ["max_tokens"] = 8192,
                    ["supports_streaming"] = true,
                    ["supports_functions"] = true,
                    ["context_window"] = 8192,
                    ["quality"] = "high"
                }
            },
            new ModelOption
            {
                ProviderId = "openai",
                ModelId = "gpt-3.5-turbo",
                EstimatedCost = 0.002m,
                EstimatedDurationMs = 2000,
                AccuracyScore = 0.85,
                IsAvailable = true,
                Capabilities = new Dictionary<string, object>
                {
                    ["max_tokens"] = 4096,
                    ["supports_streaming"] = true,
                    ["supports_functions"] = true,
                    ["context_window"] = 4096,
                    ["quality"] = "medium"
                }
            },
            new ModelOption
            {
                ProviderId = "azure",
                ModelId = "gpt-4",
                EstimatedCost = 0.03m,
                EstimatedDurationMs = 5000,
                AccuracyScore = 0.95,
                IsAvailable = true,
                Capabilities = new Dictionary<string, object>
                {
                    ["max_tokens"] = 8192,
                    ["supports_streaming"] = true,
                    ["supports_functions"] = true,
                    ["context_window"] = 8192,
                    ["quality"] = "high",
                    ["enterprise_features"] = true
                }
            },
            new ModelOption
            {
                ProviderId = "azure",
                ModelId = "gpt-35-turbo",
                EstimatedCost = 0.002m,
                EstimatedDurationMs = 2000,
                AccuracyScore = 0.85,
                IsAvailable = true,
                Capabilities = new Dictionary<string, object>
                {
                    ["max_tokens"] = 4096,
                    ["supports_streaming"] = true,
                    ["supports_functions"] = true,
                    ["context_window"] = 4096,
                    ["quality"] = "medium",
                    ["enterprise_features"] = true
                }
            }
        };

        foreach (var model in defaultModels)
        {
            var key = $"{model.ProviderId}:{model.ModelId}";
            _modelCapabilities[key] = model;
        }

        _lastCapabilitiesUpdate = DateTime.UtcNow;
    }

    private async Task RefreshModelCapabilitiesIfNeededAsync(CancellationToken cancellationToken)
    {
        if (DateTime.UtcNow - _lastCapabilitiesUpdate > _capabilitiesUpdateInterval)
        {
            // Could refresh from external sources or configuration
            _lastCapabilitiesUpdate = DateTime.UtcNow;
        }
        await Task.CompletedTask;
    }

    private List<ModelOption> GetModelsForProvider(string providerId)
    {
        return _modelCapabilities.Values
            .Where(m => m.ProviderId.Equals(providerId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private async Task<double> CalculateModelScoreAsync(ModelOption model, ModelSelectionCriteria criteria, CancellationToken cancellationToken)
    {
        try
        {
            var score = 0.0;
            var weights = GetScoringWeights(criteria.Priority);

            // Cost score (lower cost = higher score)
            if (criteria.MaxCost > 0)
            {
                var costScore = Math.Max(0, 1.0 - (double)(model.EstimatedCost / criteria.MaxCost));
                score += costScore * weights["cost"];
            }

            // Speed score (lower duration = higher score)
            if (criteria.MaxDurationMs > 0)
            {
                var speedScore = Math.Max(0, 1.0 - (double)(model.EstimatedDurationMs / criteria.MaxDurationMs));
                score += speedScore * weights["speed"];
            }

            // Accuracy score
            if (criteria.MinAccuracy > 0)
            {
                var accuracyScore = model.AccuracyScore >= criteria.MinAccuracy ? model.AccuracyScore : 0;
                score += accuracyScore * weights["accuracy"];
            }

            // Availability score
            var availabilityScore = model.IsAvailable ? 1.0 : 0.0;
            score += availabilityScore * weights["availability"];

            // Query complexity matching
            var complexityScore = CalculateComplexityScore(model, criteria.QueryComplexity);
            score += complexityScore * weights["complexity"];

            return Math.Max(0, Math.Min(1, score));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating model score");
            return 0.0;
        }
    }

    private static Dictionary<string, double> GetScoringWeights(string priority)
    {
        return priority?.ToLower() switch
        {
            "cost" => new Dictionary<string, double>
            {
                ["cost"] = 0.5,
                ["speed"] = 0.2,
                ["accuracy"] = 0.2,
                ["availability"] = 0.05,
                ["complexity"] = 0.05
            },
            "speed" => new Dictionary<string, double>
            {
                ["cost"] = 0.2,
                ["speed"] = 0.5,
                ["accuracy"] = 0.2,
                ["availability"] = 0.05,
                ["complexity"] = 0.05
            },
            "accuracy" => new Dictionary<string, double>
            {
                ["cost"] = 0.1,
                ["speed"] = 0.2,
                ["accuracy"] = 0.5,
                ["availability"] = 0.1,
                ["complexity"] = 0.1
            },
            "availability" => new Dictionary<string, double>
            {
                ["cost"] = 0.1,
                ["speed"] = 0.2,
                ["accuracy"] = 0.2,
                ["availability"] = 0.4,
                ["complexity"] = 0.1
            },
            _ => new Dictionary<string, double> // Balanced
            {
                ["cost"] = 0.25,
                ["speed"] = 0.25,
                ["accuracy"] = 0.25,
                ["availability"] = 0.15,
                ["complexity"] = 0.1
            }
        };
    }

    private static double CalculateComplexityScore(ModelOption model, string queryComplexity)
    {
        var modelQuality = model.Capabilities.TryGetValue("quality", out var quality) ? quality.ToString() : "medium";

        return (queryComplexity?.ToLower(), modelQuality?.ToLower()) switch
        {
            ("simple", "high") => 0.8, // Overkill but works
            ("simple", "medium") => 1.0, // Perfect match
            ("simple", "low") => 0.9, // Good match
            ("medium", "high") => 0.9, // Good match
            ("medium", "medium") => 1.0, // Perfect match
            ("medium", "low") => 0.7, // Might struggle
            ("complex", "high") => 1.0, // Perfect match
            ("complex", "medium") => 0.8, // Might struggle
            ("complex", "low") => 0.5, // Poor match
            ("very_complex", "high") => 1.0, // Perfect match
            ("very_complex", "medium") => 0.6, // Poor match
            ("very_complex", "low") => 0.3, // Very poor match
            _ => 0.7 // Default
        };
    }

    private static string GenerateSelectionReasoning(ModelOption model, ModelSelectionCriteria criteria, double score)
    {
        var reasons = new List<string>();

        if (criteria.Priority?.ToLower() == "cost")
            reasons.Add($"Selected for cost optimization (${model.EstimatedCost:F4} per request)");
        else if (criteria.Priority?.ToLower() == "speed")
            reasons.Add($"Selected for speed optimization ({model.EstimatedDurationMs}ms estimated duration)");
        else if (criteria.Priority?.ToLower() == "accuracy")
            reasons.Add($"Selected for accuracy optimization ({model.AccuracyScore:P} accuracy score)");
        else
            reasons.Add("Selected based on balanced criteria");

        reasons.Add($"Overall score: {score:F2}");
        reasons.Add($"Model capabilities: {string.Join(", ", model.Capabilities.Keys)}");

        return string.Join(". ", reasons);
    }

    #endregion

    private class PerformanceMetric
    {
        public string MetricName { get; set; } = string.Empty;
        public double Value { get; set; }
        public string Unit { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }
}
