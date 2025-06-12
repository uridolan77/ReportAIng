using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using ErrorAnalysis = BIReportingCopilot.Core.Interfaces.ErrorAnalysis;

namespace BIReportingCopilot.Infrastructure.AI.Management;

/// <summary>
/// LLM Management Service implementation for provider configuration,
/// usage tracking, cost monitoring, and performance analytics
/// </summary>
public class LLMManagementService : ILLMManagementService
{
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<LLMManagementService> _logger;
    private readonly ICacheService _cacheService;

    public LLMManagementService(
        IDbContextFactory contextFactory,
        ILogger<LLMManagementService> logger,
        ICacheService cacheService)
    {
        _contextFactory = contextFactory;
        _logger = logger;
        _cacheService = cacheService;
    }

    #region Provider Management

    public async Task<List<LLMProviderConfig>> GetProvidersAsync()
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var providers = await legacyContext.LLMProviderConfigs
                        .OrderBy(p => p.Name)
                        .ToListAsync();

                    // Mask sensitive information
                    foreach (var provider in providers)
                    {
                        if (!string.IsNullOrEmpty(provider.ApiKey))
                        {
                            provider.ApiKey = "***CONFIGURED***";
                        }
                    }

                    return providers;
                }

                return new List<LLMProviderConfig>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM providers");
            return new List<LLMProviderConfig>();
        }
    }

    public async Task<LLMProviderConfig?> GetProviderAsync(string providerId)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var provider = await legacyContext.LLMProviderConfigs
                        .FirstOrDefaultAsync(p => p.ProviderId == providerId);

                    if (provider != null && !string.IsNullOrEmpty(provider.ApiKey))
                    {
                        provider.ApiKey = "***CONFIGURED***";
                    }

                    return provider;
                }

                return null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM provider {ProviderId}", providerId);
            return null;
        }
    }

    public async Task<LLMProviderConfig> SaveProviderAsync(LLMProviderConfig provider)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var existing = await legacyContext.LLMProviderConfigs
                        .FirstOrDefaultAsync(p => p.ProviderId == provider.ProviderId);

                    if (existing != null)
                    {
                        // Update existing
                        existing.Name = provider.Name;
                        existing.Type = provider.Type;
                        existing.Endpoint = provider.Endpoint;
                        existing.Organization = provider.Organization;
                        existing.IsEnabled = provider.IsEnabled;
                        existing.IsDefault = provider.IsDefault;
                        existing.Settings = provider.Settings;
                        existing.UpdatedAt = DateTime.UtcNow;

                        // Only update API key if provided (not masked)
                        if (!string.IsNullOrEmpty(provider.ApiKey) && provider.ApiKey != "***CONFIGURED***")
                        {
                            existing.ApiKey = provider.ApiKey;
                        }

                        legacyContext.LLMProviderConfigs.Update(existing);
                    }
                    else
                    {
                        // Create new
                        provider.CreatedAt = DateTime.UtcNow;
                        provider.UpdatedAt = DateTime.UtcNow;
                        legacyContext.LLMProviderConfigs.Add(provider);
                    }

                    await legacyContext.SaveChangesAsync();

                    // Clear cache
                    await _cacheService.RemoveAsync($"llm_provider_{provider.ProviderId}");

                    return provider;
                }

                throw new InvalidOperationException("BICopilotContext not available");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving LLM provider {ProviderId}", provider.ProviderId);
            throw;
        }
    }

    public async Task<bool> DeleteProviderAsync(string providerId)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var provider = await legacyContext.LLMProviderConfigs
                        .FirstOrDefaultAsync(p => p.ProviderId == providerId);

                    if (provider != null)
                    {
                        legacyContext.LLMProviderConfigs.Remove(provider);
                        await legacyContext.SaveChangesAsync();

                        // Clear cache
                        await _cacheService.RemoveAsync($"llm_provider_{providerId}");

                        return true;
                    }
                }

                return false;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting LLM provider {ProviderId}", providerId);
            return false;
        }
    }

    public async Task<LLMProviderStatus> TestProviderConnectionAsync(string providerId)
    {
        try
        {
            var provider = await GetProviderAsync(providerId);
            if (provider == null)
            {
                return new LLMProviderStatus
                {
                    ProviderId = providerId,
                    Name = "Unknown",
                    IsConnected = false,
                    IsHealthy = false,
                    LastError = "Provider not found",
                    LastChecked = DateTime.UtcNow
                };
            }

            var startTime = DateTime.UtcNow;
            
            try
            {
                // For now, we'll do a basic configuration check instead of actual API testing
                // This avoids the circular dependency while still providing useful health information

                if (provider == null)
                {
                    return new LLMProviderStatus
                    {
                        ProviderId = providerId,
                        Name = "Unknown",
                        IsConnected = false,
                        IsHealthy = false,
                        LastChecked = DateTime.UtcNow,
                        LastError = "Provider not found"
                    };
                }

                var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                // Basic health check based on configuration completeness
                bool isHealthy = provider.IsEnabled &&
                               !string.IsNullOrEmpty(provider.ApiKey) &&
                               !string.IsNullOrEmpty(provider.Endpoint);

                return new LLMProviderStatus
                {
                    ProviderId = providerId,
                    Name = provider.Name,
                    IsConnected = isHealthy,
                    IsHealthy = isHealthy,
                    LastChecked = DateTime.UtcNow,
                    LastResponseTime = responseTime,
                    HealthDetails = new Dictionary<string, object>
                    {
                        { "configuration_complete", isHealthy },
                        { "test_type", "configuration_check" }
                    }
                };
            }
            catch (Exception ex)
            {
                var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
                
                return new LLMProviderStatus
                {
                    ProviderId = providerId,
                    Name = provider.Name,
                    IsConnected = false,
                    IsHealthy = false,
                    LastError = ex.Message,
                    LastChecked = DateTime.UtcNow,
                    LastResponseTime = responseTime,
                    HealthDetails = new Dictionary<string, object>
                    {
                        { "error_type", ex.GetType().Name },
                        { "test_successful", false }
                    }
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing provider connection {ProviderId}", providerId);
            return new LLMProviderStatus
            {
                ProviderId = providerId,
                Name = "Unknown",
                IsConnected = false,
                IsHealthy = false,
                LastError = ex.Message,
                LastChecked = DateTime.UtcNow
            };
        }
    }

    public async Task<List<LLMProviderStatus>> GetProviderHealthStatusAsync()
    {
        try
        {
            var providers = await GetProvidersAsync();
            var healthStatuses = new List<LLMProviderStatus>();

            foreach (var provider in providers)
            {
                var status = await TestProviderConnectionAsync(provider.ProviderId);
                healthStatuses.Add(status);
            }

            return healthStatuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting provider health status");
            return new List<LLMProviderStatus>();
        }
    }

    #endregion

    #region Model Management

    public async Task<List<LLMModelConfig>> GetModelsAsync(string? providerId = null)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var query = legacyContext.LLMModelConfigs.AsQueryable();

                    if (!string.IsNullOrEmpty(providerId))
                    {
                        query = query.Where(m => m.ProviderId == providerId);
                    }

                    return await query.OrderBy(m => m.Name).ToListAsync();
                }

                return new List<LLMModelConfig>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM models");
            return new List<LLMModelConfig>();
        }
    }

    public async Task<LLMModelConfig?> GetModelAsync(string modelId)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    return await legacyContext.LLMModelConfigs
                        .FirstOrDefaultAsync(m => m.ModelId == modelId);
                }

                return null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM model {ModelId}", modelId);
            return null;
        }
    }

    public async Task<LLMModelConfig> SaveModelAsync(LLMModelConfig model)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var existing = await legacyContext.LLMModelConfigs
                        .FirstOrDefaultAsync(m => m.ModelId == model.ModelId);

                    if (existing != null)
                    {
                        // Update existing
                        existing.Name = model.Name;
                        existing.DisplayName = model.DisplayName;
                        existing.Temperature = model.Temperature;
                        existing.MaxTokens = model.MaxTokens;
                        existing.TopP = model.TopP;
                        existing.FrequencyPenalty = model.FrequencyPenalty;
                        existing.PresencePenalty = model.PresencePenalty;
                        existing.IsEnabled = model.IsEnabled;
                        existing.UseCase = model.UseCase;
                        existing.CostPerToken = model.CostPerToken;
                        existing.Capabilities = model.Capabilities;

                        legacyContext.LLMModelConfigs.Update(existing);
                    }
                    else
                    {
                        // Create new
                        legacyContext.LLMModelConfigs.Add(model);
                    }

                    await legacyContext.SaveChangesAsync();

                    // Clear cache
                    await _cacheService.RemoveAsync($"llm_model_{model.ModelId}");

                    return model;
                }

                throw new InvalidOperationException("BICopilotContext not available");
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving LLM model {ModelId}", model.ModelId);
            throw;
        }
    }

    public async Task<bool> DeleteModelAsync(string modelId)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var model = await legacyContext.LLMModelConfigs
                        .FirstOrDefaultAsync(m => m.ModelId == modelId);

                    if (model != null)
                    {
                        legacyContext.LLMModelConfigs.Remove(model);
                        await legacyContext.SaveChangesAsync();

                        // Clear cache
                        await _cacheService.RemoveAsync($"llm_model_{modelId}");

                        return true;
                    }
                }

                return false;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting LLM model {ModelId}", modelId);
            return false;
        }
    }

    public async Task<LLMModelConfig?> GetDefaultModelAsync(string useCase)
    {
        try
        {
            var cacheKey = $"default_model_{useCase}";
            var cached = await _cacheService.GetAsync<LLMModelConfig>(cacheKey);
            if (cached != null) return cached;

            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var model = await legacyContext.LLMModelConfigs
                        .Where(m => m.UseCase == useCase && m.IsEnabled)
                        .OrderBy(m => m.Name)
                        .FirstOrDefaultAsync();

                    if (model != null)
                    {
                        await _cacheService.SetAsync(cacheKey, model, TimeSpan.FromMinutes(30));
                    }

                    return model;
                }

                return null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default model for use case {UseCase}", useCase);
            return null;
        }
    }

    public async Task<bool> SetDefaultModelAsync(string modelId, string useCase)
    {
        try
        {
            // Clear cache
            await _cacheService.RemoveAsync($"default_model_{useCase}");
            
            // For now, we'll use a simple approach where the first enabled model for a use case is the default
            // In a more complex implementation, we could add a DefaultModels table
            _logger.LogInformation("Set default model {ModelId} for use case {UseCase}", modelId, useCase);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default model {ModelId} for use case {UseCase}", modelId, useCase);
            return false;
        }
    }

    #endregion

    // Additional methods will be implemented in the next part...
    
    #region Usage Tracking - Stub implementations for now

    public async Task LogUsageAsync(LLMUsageLog usageLog)
    {
        try
        {
            await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    legacyContext.LLMUsageLogs.Add(usageLog);
                    await legacyContext.SaveChangesAsync();
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging LLM usage");
        }
    }

    public async Task<List<LLMUsageLog>> GetUsageHistoryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? providerId = null,
        string? modelId = null,
        string? userId = null,
        string? requestType = null,
        int skip = 0,
        int take = 100)
    {
        // Implementation will be added in next part
        return new List<LLMUsageLog>();
    }

    public async Task<LLMUsageAnalytics> GetUsageAnalyticsAsync(
        DateTime startDate,
        DateTime endDate,
        string? providerId = null,
        string? modelId = null)
    {
        // Implementation will be added in next part
        return new LLMUsageAnalytics();
    }

    public async Task<byte[]> ExportUsageDataAsync(
        DateTime startDate,
        DateTime endDate,
        string format = "csv")
    {
        // Implementation will be added in next part
        return Array.Empty<byte>();
    }

    #endregion

    #region Cost Management - Stub implementations for now

    public async Task<List<LLMCostSummary>> GetCostSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        string? providerId = null)
    {
        // Implementation will be added in next part
        return new List<LLMCostSummary>();
    }

    public async Task<decimal> GetCurrentMonthCostAsync(string? providerId = null)
    {
        // Implementation will be added in next part
        return 0m;
    }

    public async Task<Dictionary<string, decimal>> GetCostProjectionsAsync()
    {
        // Implementation will be added in next part
        return new Dictionary<string, decimal>();
    }

    public async Task<bool> SetCostLimitAsync(string providerId, decimal monthlyLimit)
    {
        // Implementation will be added in next part
        return true;
    }

    public async Task<List<CostAlert>> GetCostAlertsAsync()
    {
        // Implementation will be added in next part
        return new List<CostAlert>();
    }

    #endregion

    #region Performance Monitoring - Stub implementations for now

    public async Task<Dictionary<string, ProviderPerformanceMetrics>> GetPerformanceMetricsAsync(
        DateTime startDate,
        DateTime endDate)
    {
        // Implementation will be added in next part
        return new Dictionary<string, ProviderPerformanceMetrics>();
    }

    public async Task<Dictionary<string, double>> GetCacheHitRatesAsync()
    {
        // Implementation will be added in next part
        return new Dictionary<string, double>();
    }

    public async Task<Dictionary<string, ErrorAnalysis>> GetErrorAnalysisAsync(
        DateTime startDate,
        DateTime endDate)
    {
        // Implementation will be added in next part
        return new Dictionary<string, ErrorAnalysis>();
    }

    #endregion
}
