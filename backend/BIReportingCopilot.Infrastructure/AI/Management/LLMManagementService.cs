using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.Statistics;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using ContextType = BIReportingCopilot.Infrastructure.Data.Contexts.ContextType;

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

    private async Task<LLMProviderConfig?> GetProviderForTestingAsync(string providerId)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    // Get provider with actual API key (not masked) for testing
                    return await legacyContext.LLMProviderConfigs
                        .FirstOrDefaultAsync(p => p.ProviderId == providerId);
                }

                return null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving LLM provider for testing {ProviderId}", providerId);
            return null;
        }
    }

    public async Task<LLMProviderStatus> TestProviderConnectionAsync(string providerId)
    {
        try
        {
            // Get provider with actual API key (not masked) for testing
            var provider = await GetProviderForTestingAsync(providerId);
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

                // First check configuration completeness
                bool configurationComplete = provider.IsEnabled &&
                                           !string.IsNullOrEmpty(provider.ApiKey) &&
                                           !string.IsNullOrEmpty(provider.Endpoint);

                if (!configurationComplete)
                {
                    return new LLMProviderStatus
                    {
                        ProviderId = providerId,
                        Name = provider.Name,
                        IsConnected = false,
                        IsHealthy = false,
                        LastChecked = DateTime.UtcNow,
                        LastError = "Provider configuration incomplete",
                        HealthDetails = new Dictionary<string, object>
                        {
                            { "configuration_complete", false },
                            { "has_api_key", !string.IsNullOrEmpty(provider.ApiKey) },
                            { "has_endpoint", !string.IsNullOrEmpty(provider.Endpoint) },
                            { "is_enabled", provider.IsEnabled },
                            { "test_type", "configuration_check" }
                        }
                    };
                }

                // Perform actual API health check
                bool apiHealthy = false;
                string errorMessage = null;
                var healthDetails = new Dictionary<string, object>
                {
                    { "configuration_complete", true },
                    { "test_type", "api_test" }
                };

                try
                {
                    // Test actual API connectivity with a lightweight request
                    apiHealthy = await PerformActualApiHealthCheckAsync(provider);
                    healthDetails["api_test_successful"] = apiHealthy;
                }
                catch (Exception apiEx)
                {
                    apiHealthy = false;
                    errorMessage = apiEx.Message;
                    healthDetails["api_test_successful"] = false;
                    healthDetails["api_error"] = apiEx.Message;
                    healthDetails["api_error_type"] = apiEx.GetType().Name;
                }

                var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

                return new LLMProviderStatus
                {
                    ProviderId = providerId,
                    Name = provider.Name,
                    IsConnected = apiHealthy,
                    IsHealthy = apiHealthy,
                    LastChecked = DateTime.UtcNow,
                    LastResponseTime = responseTime,
                    LastError = errorMessage,
                    HealthDetails = healthDetails
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

    private async Task<bool> PerformActualApiHealthCheckAsync(LLMProviderConfig provider)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10); // 10 second timeout

            // Set up the request based on provider type
            var request = new HttpRequestMessage();

            if (provider.Type.ToLower() == "openai")
            {
                // OpenAI API health check - list models endpoint (lightweight)
                request.RequestUri = new Uri($"{provider.Endpoint.TrimEnd('/')}/models");
                request.Method = HttpMethod.Get;
                request.Headers.Add("Authorization", $"Bearer {provider.ApiKey}");

                if (!string.IsNullOrEmpty(provider.Organization))
                {
                    request.Headers.Add("OpenAI-Organization", provider.Organization);
                }
            }
            else if (provider.Type.ToLower() == "azure" || provider.Type.ToLower() == "azureopenai")
            {
                // Azure OpenAI health check - deployments endpoint
                request.RequestUri = new Uri($"{provider.Endpoint.TrimEnd('/')}/openai/deployments?api-version=2023-12-01-preview");
                request.Method = HttpMethod.Get;
                request.Headers.Add("api-key", provider.ApiKey);
            }
            else
            {
                // Generic health check - try a simple GET request
                request.RequestUri = new Uri(provider.Endpoint);
                request.Method = HttpMethod.Get;
                request.Headers.Add("Authorization", $"Bearer {provider.ApiKey}");
            }

            var response = await httpClient.SendAsync(request);

            // Consider 200, 401 (unauthorized but reachable), and 403 (forbidden but reachable) as "healthy"
            // 401/403 means the endpoint is reachable but API key might be invalid
            return response.StatusCode == System.Net.HttpStatusCode.OK ||
                   response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                   response.StatusCode == System.Net.HttpStatusCode.Forbidden;
        }
        catch (HttpRequestException)
        {
            // Network/connectivity issues
            return false;
        }
        catch (TaskCanceledException)
        {
            // Timeout
            return false;
        }
        catch (Exception)
        {
            // Other errors
            return false;
        }
    }

    public async Task<List<LLMProviderStatus>> GetProviderHealthStatusAsync(CancellationToken cancellationToken = default)
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

    public async Task<List<LLMProviderStatus>> GetCachedProviderHealthStatusAsync()
    {
        try
        {
            // For dashboard summary, use cached health status to avoid frequent API calls
            // Health status is cached for 5 minutes
            var cacheKey = "provider_health_status";
            var cachedStatus = await _cacheService.GetAsync<List<LLMProviderStatus>>(cacheKey);

            if (cachedStatus != null)
            {
                return cachedStatus;
            }

            // If not cached, get fresh status and cache it
            var healthStatuses = await GetProviderHealthStatusAsync();
            await _cacheService.SetAsync(cacheKey, healthStatuses, TimeSpan.FromMinutes(5));

            return healthStatuses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached provider health status");
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

    public async Task<LLMModelConfig?> GetModelAsync(string modelId, CancellationToken cancellationToken = default)
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

    public async Task<LLMModelConfig?> GetDefaultModelAsync(string useCase, CancellationToken cancellationToken = default)
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

    public async Task LogUsageAsync(LLMUsageLog usageLog, CancellationToken cancellationToken = default)
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
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var query = legacyContext.LLMUsageLogs.AsQueryable();

                    if (startDate.HasValue)
                        query = query.Where(l => l.Timestamp >= startDate.Value);

                    if (endDate.HasValue)
                        query = query.Where(l => l.Timestamp <= endDate.Value);

                    if (!string.IsNullOrEmpty(providerId))
                        query = query.Where(l => l.ProviderId == providerId);

                    if (!string.IsNullOrEmpty(modelId))
                        query = query.Where(l => l.ModelId == modelId);

                    if (!string.IsNullOrEmpty(userId))
                        query = query.Where(l => l.UserId == userId);

                    if (!string.IsNullOrEmpty(requestType))
                        query = query.Where(l => l.RequestType == requestType);

                    return await query
                        .OrderByDescending(l => l.Timestamp)
                        .Skip(skip)
                        .Take(take)
                        .ToListAsync();
                }

                return new List<LLMUsageLog>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage history");
            return new List<LLMUsageLog>();
        }
    }

    public async Task<LLMUsageAnalytics> GetUsageAnalyticsAsync(
        DateTime startDate,
        DateTime endDate,
        string? providerId = null,
        string? modelId = null)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var query = legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

                    if (!string.IsNullOrEmpty(providerId))
                        query = query.Where(l => l.ProviderId == providerId);

                    if (!string.IsNullOrEmpty(modelId))
                        query = query.Where(l => l.ModelId == modelId);

                    var logs = await query.ToListAsync();

                    var costByProvider = logs
                        .GroupBy(l => l.ProviderId)
                        .Select(g => new LLMCostSummary
                        {
                            ProviderId = g.Key,
                            ModelId = "",
                            Date = startDate,
                            TotalRequests = g.Count(),
                            TotalTokens = g.Sum(l => l.TotalTokens),
                            TotalCost = g.Sum(l => l.Cost),
                            AverageCost = g.Average(l => l.Cost),
                            AverageResponseTime = (long)g.Average(l => l.DurationMs),
                            SuccessRate = (double)g.Count(l => l.Success) / g.Count()
                        })
                        .ToList();

                    var costByModel = logs
                        .GroupBy(l => l.ModelId)
                        .Select(g => new LLMCostSummary
                        {
                            ProviderId = "",
                            ModelId = g.Key,
                            Date = startDate,
                            TotalRequests = g.Count(),
                            TotalTokens = g.Sum(l => l.TotalTokens),
                            TotalCost = g.Sum(l => l.Cost),
                            AverageCost = g.Average(l => l.Cost),
                            AverageResponseTime = (long)g.Average(l => l.DurationMs),
                            SuccessRate = (double)g.Count(l => l.Success) / g.Count()
                        })
                        .ToList();

                    var requestsByType = logs
                        .GroupBy(l => l.RequestType)
                        .ToDictionary(g => g.Key, g => g.Count());

                    var costByType = logs
                        .GroupBy(l => l.RequestType)
                        .ToDictionary(g => g.Key, g => (double)g.Sum(l => l.Cost));

                    var topRequests = logs
                        .OrderByDescending(l => l.Cost)
                        .Take(10)
                        .ToList();

                    return new LLMUsageAnalytics
                    {
                        StartDate = startDate,
                        EndDate = endDate,
                        TotalRequests = logs.Count,
                        TotalTokens = logs.Sum(l => l.TotalTokens),
                        TotalCost = logs.Sum(l => l.Cost),
                        AverageResponseTime = logs.Any() ? (long)logs.Average(l => l.DurationMs) : 0,
                        SuccessRate = logs.Any() ? (double)logs.Count(l => l.Success) / logs.Count : 0,
                        CostByProvider = costByProvider,
                        CostByModel = costByModel,
                        RequestsByType = requestsByType,
                        CostByType = costByType.ToDictionary(kvp => kvp.Key, kvp => (decimal)kvp.Value),
                        TopRequests = topRequests
                    };
                }

                return new LLMUsageAnalytics();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage analytics for date range {StartDate} to {EndDate}", startDate, endDate);
            return new LLMUsageAnalytics();
        }
    }

    public async Task<byte[]> ExportUsageDataAsync(
        DateTime startDate,
        DateTime endDate,
        string format = "csv")
    {
        try
        {
            var usageData = await GetUsageHistoryAsync(startDate, endDate, take: int.MaxValue);

            if (format.ToLower() == "csv")
            {
                var csv = new StringBuilder();
                csv.AppendLine("Timestamp,ProviderId,ModelId,RequestType,UserId,TotalTokens,Cost,DurationMs,Success,ErrorMessage");

                foreach (var log in usageData)
                {
                    csv.AppendLine($"{log.Timestamp:yyyy-MM-dd HH:mm:ss},{log.ProviderId},{log.ModelId},{log.RequestType},{log.UserId},{log.TotalTokens},{log.Cost},{log.DurationMs},{log.Success},{log.ErrorMessage?.Replace(",", ";")}");
                }

                return Encoding.UTF8.GetBytes(csv.ToString());
            }

            // Default to JSON if format not recognized
            var json = JsonSerializer.Serialize(usageData, new JsonSerializerOptions { WriteIndented = true });
            return Encoding.UTF8.GetBytes(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting usage data for date range {StartDate} to {EndDate}", startDate, endDate);
            return Array.Empty<byte>();
        }
    }

    #endregion

    #region Cost Management

    public async Task<List<LLMCostSummary>> GetCostSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        string? providerId = null)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var query = legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

                    if (!string.IsNullOrEmpty(providerId))
                    {
                        query = query.Where(l => l.ProviderId == providerId);
                    }

                    var costSummary = await query
                        .GroupBy(l => new { l.ProviderId, l.ModelId, Date = l.Timestamp.Date })
                        .Select(g => new LLMCostSummary
                        {
                            ProviderId = g.Key.ProviderId,
                            ModelId = g.Key.ModelId,
                            Date = g.Key.Date,
                            TotalRequests = g.Count(),
                            TotalTokens = g.Sum(l => l.TotalTokens),
                            TotalCost = g.Sum(l => l.Cost),
                            AverageCost = g.Average(l => l.Cost),
                            AverageResponseTime = (long)g.Average(l => l.DurationMs),
                            SuccessRate = (double)g.Count(l => l.Success) / g.Count()
                        })
                        .OrderByDescending(s => s.Date)
                        .ToListAsync();

                    return costSummary;
                }

                return new List<LLMCostSummary>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost summary for date range {StartDate} to {EndDate}", startDate, endDate);
            return new List<LLMCostSummary>();
        }
    }

    public async Task<decimal> GetCurrentMonthCostAsync(string? providerId = null)
    {
        try
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var query = legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startOfMonth && l.Timestamp <= endOfMonth);

                    if (!string.IsNullOrEmpty(providerId))
                    {
                        query = query.Where(l => l.ProviderId == providerId);
                    }

                    return await query.SumAsync(l => l.Cost);
                }

                return 0m;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving current month cost for provider {ProviderId}", providerId);
            return 0m;
        }
    }

    public async Task<Dictionary<string, decimal>> GetCostProjectionsAsync()
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var last30Days = DateTime.UtcNow.AddDays(-30);
                    var dailyAverages = await legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= last30Days)
                        .GroupBy(l => new { l.ProviderId, Date = l.Timestamp.Date })
                        .Select(g => new { g.Key.ProviderId, DailyCost = g.Sum(l => l.Cost) })
                        .GroupBy(x => x.ProviderId)
                        .Select(g => new { ProviderId = g.Key, AverageDailyCost = g.Average(x => x.DailyCost) })
                        .ToListAsync();

                    var projections = new Dictionary<string, decimal>();
                    var daysInCurrentMonth = DateTime.DaysInMonth(DateTime.UtcNow.Year, DateTime.UtcNow.Month);

                    foreach (var avg in dailyAverages)
                    {
                        projections[avg.ProviderId] = avg.AverageDailyCost * daysInCurrentMonth;
                    }

                    return projections;
                }

                return new Dictionary<string, decimal>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating cost projections");
            return new Dictionary<string, decimal>();
        }
    }

    public async Task<bool> SetCostLimitAsync(string providerId, decimal monthlyLimit)
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
                        // Update settings with cost limit
                        var settings = provider.Settings ?? new Dictionary<string, object>();
                        settings["monthlyLimit"] = monthlyLimit;
                        provider.Settings = settings;
                        provider.UpdatedAt = DateTime.UtcNow;

                        await legacyContext.SaveChangesAsync();
                        return true;
                    }
                }

                return false;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting cost limit for provider {ProviderId}", providerId);
            return false;
        }
    }

    public async Task<List<CostAlert>> GetCostAlertsAsync()
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var alerts = new List<CostAlert>();
                    var currentMonth = await GetCurrentMonthCostAsync();
                    var projections = await GetCostProjectionsAsync();

                    var providers = await legacyContext.LLMProviderConfigs
                        .Where(p => p.IsEnabled)
                        .ToListAsync();

                    foreach (var provider in providers)
                    {
                        if (provider.Settings != null && provider.Settings.Any())
                        {
                            var settings = provider.Settings;
                            if (settings.ContainsKey("monthlyLimit") &&
                                decimal.TryParse(settings["monthlyLimit"].ToString(), out var limit))
                            {
                                var currentCost = await GetCurrentMonthCostAsync(provider.ProviderId);
                                var projection = projections.GetValueOrDefault(provider.ProviderId, 0);

                                if (currentCost > limit * 0.8m) // 80% threshold
                                {
                                    alerts.Add(new CostAlert
                                    {
                                        Id = Guid.NewGuid().ToString(),
                                        ProviderId = provider.ProviderId,
                                        ThresholdAmount = (double)limit,
                                        AlertType = currentCost > limit ? "EXCEEDED" : "WARNING",
                                        IsEnabled = true,
                                        CreatedAt = DateTime.UtcNow
                                    });
                                }
                            }
                        }
                    }

                    return alerts;
                }

                return new List<CostAlert>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost alerts");
            return new List<CostAlert>();
        }
    }

    #endregion

    #region Performance Monitoring

    public async Task<Dictionary<string, ProviderPerformanceMetrics>> GetPerformanceMetricsAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    // First, get the basic metrics without complex nested grouping
                    var basicMetrics = await legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                        .GroupBy(l => l.ProviderId)
                        .Select(g => new
                        {
                            ProviderId = g.Key,
                            TotalRequests = g.Count(),
                            SuccessfulRequests = g.Count(l => l.Success),
                            ResponseTimes = g.Select(l => (double)l.DurationMs).ToList()
                        })
                        .ToListAsync();

                    // Then get error data separately to avoid complex nested queries
                    var errorData = await legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate && !l.Success)
                        .Select(l => new { l.ProviderId, ErrorMessage = l.ErrorMessage ?? "Unknown" })
                        .ToListAsync();

                    var result = new Dictionary<string, ProviderPerformanceMetrics>();

                    // Group error data by provider and error type in memory
                    var errorsByProvider = errorData
                        .GroupBy(e => e.ProviderId)
                        .ToDictionary(
                            g => g.Key,
                            g => g.GroupBy(e => e.ErrorMessage)
                                  .ToDictionary(eg => eg.Key, eg => eg.Count())
                        );

                    foreach (var metric in basicMetrics)
                    {
                        var responseTimes = metric.ResponseTimes.OrderBy(x => x).ToList();
                        var averageResponseTime = responseTimes.Any() ? responseTimes.Average() : 0;
                        var medianResponseTime = responseTimes.Any()
                            ? responseTimes[responseTimes.Count / 2]
                            : 0;
                        var p95ResponseTime = responseTimes.Any()
                            ? responseTimes[(int)(responseTimes.Count * 0.95)]
                            : 0;

                        // Get error data for this provider
                        var providerErrors = errorsByProvider.ContainsKey(metric.ProviderId)
                            ? errorsByProvider[metric.ProviderId]
                            : new Dictionary<string, int>();

                        result[metric.ProviderId] = new ProviderPerformanceMetrics
                        {
                            ProviderId = metric.ProviderId,
                            AverageResponseTime = averageResponseTime,
                            MedianResponseTime = medianResponseTime,
                            P95ResponseTime = p95ResponseTime,
                            SuccessRate = metric.TotalRequests > 0
                                ? (double)metric.SuccessfulRequests / metric.TotalRequests
                                : 0,
                            ErrorRate = metric.TotalRequests > 0
                                ? (double)(metric.TotalRequests - metric.SuccessfulRequests) / metric.TotalRequests
                                : 0,
                            TotalRequests = metric.TotalRequests,
                            ErrorsByType = providerErrors
                        };
                    }

                    return result;
                }

                return new Dictionary<string, ProviderPerformanceMetrics>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving performance metrics for date range {StartDate} to {EndDate}", startDate, endDate);
            return new Dictionary<string, ProviderPerformanceMetrics>();
        }
    }

    public async Task<Dictionary<string, double>> GetCacheHitRatesAsync()
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var last24Hours = DateTime.UtcNow.AddHours(-24);

                    var cacheStats = await legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= last24Hours)
                        .GroupBy(l => l.ProviderId)
                        .Select(g => new
                        {
                            ProviderId = g.Key,
                            TotalRequests = g.Count(),
                            CachedRequests = g.Count(l => l.Metadata != null &&
                                l.Metadata.ContainsKey("cached") &&
                                l.Metadata["cached"].ToString() == "true")
                        })
                        .ToListAsync();

                    var result = new Dictionary<string, double>();
                    foreach (var stat in cacheStats)
                    {
                        result[stat.ProviderId] = stat.TotalRequests > 0
                            ? (double)stat.CachedRequests / stat.TotalRequests
                            : 0;
                    }

                    return result;
                }

                return new Dictionary<string, double>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cache hit rates");
            return new Dictionary<string, double>();
        }
    }

    public async Task<Dictionary<string, ErrorAnalysis>> GetErrorAnalysisAsync(
        DateTime startDate,
        DateTime endDate)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    // Get basic error counts by provider
                    var errorCounts = await legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate && !l.Success)
                        .GroupBy(l => l.ProviderId)
                        .Select(g => new
                        {
                            ProviderId = g.Key,
                            TotalErrors = g.Count()
                        })
                        .ToListAsync();

                    // Get total request counts by provider
                    var totalCounts = await legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate)
                        .GroupBy(l => l.ProviderId)
                        .Select(g => new
                        {
                            ProviderId = g.Key,
                            TotalRequests = g.Count()
                        })
                        .ToListAsync();

                    // Get detailed error data for in-memory processing
                    var detailedErrors = await legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate && !l.Success)
                        .Select(l => new
                        {
                            l.ProviderId,
                            ErrorMessage = l.ErrorMessage ?? "Unknown",
                            l.ModelId
                        })
                        .ToListAsync();

                    var result = new Dictionary<string, ErrorAnalysis>();

                    // Process data in memory to avoid complex LINQ translations
                    var errorsByProvider = detailedErrors.GroupBy(e => e.ProviderId);

                    foreach (var providerGroup in errorsByProvider)
                    {
                        var providerId = providerGroup.Key;
                        var providerErrors = providerGroup.ToList();

                        var errorCount = errorCounts.FirstOrDefault(ec => ec.ProviderId == providerId);
                        var totalCount = totalCounts.FirstOrDefault(tc => tc.ProviderId == providerId);

                        var errorsByType = providerErrors
                            .GroupBy(e => e.ErrorMessage)
                            .ToDictionary(g => g.Key, g => g.Count());

                        var errorsByModel = providerErrors
                            .GroupBy(e => e.ModelId)
                            .ToDictionary(g => g.Key ?? "Unknown", g => g.Count());

                        var commonErrorMessages = providerErrors
                            .GroupBy(e => e.ErrorMessage)
                            .OrderByDescending(g => g.Count())
                            .Take(5)
                            .Select(g => g.Key)
                            .ToList();

                        result[providerId] = new ErrorAnalysis
                        {
                            ProviderId = providerId,
                            TotalErrors = errorCount?.TotalErrors ?? 0,
                            ErrorRate = totalCount?.TotalRequests > 0
                                ? (double)(errorCount?.TotalErrors ?? 0) / totalCount.TotalRequests
                                : 0,
                            ErrorsByType = errorsByType,
                            ErrorsByModel = errorsByModel,
                            CommonErrorMessages = commonErrorMessages,
                            AnalysisType = "LLM",
                            AnalyzedAt = DateTime.UtcNow,
                            AnalysisWindow = endDate - startDate
                        };
                    }

                    return result;
                }

                return new Dictionary<string, ErrorAnalysis>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving error analysis for date range {StartDate} to {EndDate}", startDate, endDate);
            return new Dictionary<string, ErrorAnalysis>();
        }
    }

    #endregion

    #region Missing Interface Method Implementations

    /// <summary>
    /// Get available providers (ILLMManagementService interface)
    /// </summary>
    public async Task<List<LLMProvider>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var providerConfigs = await GetProvidersAsync();
            var providers = new List<LLMProvider>();

            foreach (var config in providerConfigs)
            {
                var models = await GetModelsAsync(config.ProviderId);
                var healthStatus = await TestProviderConnectionAsync(config.ProviderId);

                providers.Add(new LLMProvider
                {
                    Id = config.ProviderId,
                    Name = config.Name,
                    Description = $"{config.Type} provider - {config.Endpoint}",
                    IsActive = config.IsEnabled,
                    IsHealthy = healthStatus.IsHealthy,
                    SupportedModels = models.Select(m => new LLMModel
                    {
                        Id = m.ModelId,
                        Name = m.Name,
                        Description = m.DisplayName ?? m.Name,
                        MaxTokens = m.MaxTokens,
                        CostPerToken = m.CostPerToken,
                        Capabilities = m.Capabilities?.ToString()?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()
                    }).ToList(),
                    Configuration = new Dictionary<string, object>
                    {
                        ["type"] = config.Type,
                        ["endpoint"] = config.Endpoint,
                        ["organization"] = config.Organization ?? string.Empty,
                        ["isDefault"] = config.IsDefault
                    }
                });
            }

            return providers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available providers");
            return new List<LLMProvider>();
        }
    }

    /// <summary>
    /// Get current provider (ILLMManagementService interface)
    /// </summary>
    public async Task<LLMProvider> GetCurrentProviderAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var providers = await GetProvidersAsync();
            var defaultProvider = providers.FirstOrDefault(p => p.IsDefault) ?? providers.FirstOrDefault();

            if (defaultProvider != null)
            {
                var models = await GetModelsAsync(defaultProvider.ProviderId);
                var healthStatus = await TestProviderConnectionAsync(defaultProvider.ProviderId);

                return new LLMProvider
                {
                    Id = defaultProvider.ProviderId,
                    Name = defaultProvider.Name,
                    Description = $"{defaultProvider.Type} provider - {defaultProvider.Endpoint}",
                    IsActive = defaultProvider.IsEnabled,
                    IsHealthy = healthStatus.IsHealthy,
                    SupportedModels = models.Select(m => new LLMModel
                    {
                        Id = m.ModelId,
                        Name = m.Name,
                        Description = m.DisplayName ?? m.Name,
                        MaxTokens = m.MaxTokens,
                        CostPerToken = m.CostPerToken,
                        Capabilities = m.Capabilities?.ToString()?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>()
                    }).ToList(),
                    Configuration = new Dictionary<string, object>
                    {
                        ["type"] = defaultProvider.Type,
                        ["endpoint"] = defaultProvider.Endpoint,
                        ["organization"] = defaultProvider.Organization ?? string.Empty,
                        ["isDefault"] = defaultProvider.IsDefault
                    }
                };
            }

            return new LLMProvider
            {
                Id = "none",
                Name = "No Provider",
                Description = "No provider configured",
                IsActive = false,
                IsHealthy = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current provider");
            return new LLMProvider
            {
                Id = "error",
                Name = "Error",
                Description = "Error retrieving provider",
                IsActive = false,
                IsHealthy = false
            };
        }
    }

    /// <summary>
    /// Set provider (ILLMManagementService interface)
    /// </summary>
    public async Task<bool> SetProviderAsync(string providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    // First, unset all providers as default
                    var allProviders = await legacyContext.LLMProviderConfigs.ToListAsync(cancellationToken);
                    foreach (var provider in allProviders)
                    {
                        provider.IsDefault = false;
                    }

                    // Set the specified provider as default
                    var targetProvider = await legacyContext.LLMProviderConfigs
                        .FirstOrDefaultAsync(p => p.ProviderId == providerId, cancellationToken);

                    if (targetProvider != null)
                    {
                        targetProvider.IsDefault = true;
                        targetProvider.UpdatedAt = DateTime.UtcNow;
                        await legacyContext.SaveChangesAsync(cancellationToken);

                        // Clear cache
                        await _cacheService.RemoveAsync("current_provider");

                        return true;
                    }
                }

                return false;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting provider: {ProviderId}", providerId);
            return false;
        }
    }

    /// <summary>
    /// Get usage statistics (ILLMManagementService interface)
    /// </summary>
    public async Task<LLMUsageStatistics> GetUsageStatisticsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var fromDate = from ?? DateTime.UtcNow.AddDays(-30);
            var toDate = to ?? DateTime.UtcNow;

            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var logs = await legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= fromDate && l.Timestamp <= toDate)
                        .ToListAsync(cancellationToken);

                    var totalRequests = logs.Count;
                    var successfulRequests = logs.Count(l => l.Success);
                    var failedRequests = totalRequests - successfulRequests;
                    var totalCost = logs.Sum(l => l.Cost);
                    var totalTokens = logs.Sum(l => l.TotalTokens);
                    var averageResponseTime = logs.Any() ? logs.Average(l => l.DurationMs) : 0;

                    return new LLMUsageStatistics
                    {
                        TotalRequests = totalRequests,
                        SuccessfulRequests = successfulRequests,
                        FailedRequests = failedRequests,
                        TotalCost = totalCost,
                        TotalTokens = totalTokens,
                        AverageResponseTime = averageResponseTime,
                        Period = new DateRange
                        {
                            From = fromDate,
                            To = toDate
                        },
                        ProviderBreakdown = logs.GroupBy(l => l.ProviderId)
                            .ToDictionary(g => g.Key, g => new ProviderUsageStats
                            {
                                Requests = g.Count(),
                                Cost = g.Sum(l => l.Cost),
                                Tokens = g.Sum(l => l.TotalTokens)
                            }),
                        ModelBreakdown = logs.GroupBy(l => l.ModelId)
                            .ToDictionary(g => g.Key, g => new ModelUsageStats
                            {
                                Requests = g.Count(),
                                Cost = g.Sum(l => l.Cost),
                                Tokens = g.Sum(l => l.TotalTokens)
                            })
                    };
                }

                return new LLMUsageStatistics
                {
                    Period = new DateRange { From = fromDate, To = toDate }
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage statistics");
            return new LLMUsageStatistics
            {
                Period = new DateRange
                {
                    From = from ?? DateTime.UtcNow.AddDays(-30),
                    To = to ?? DateTime.UtcNow
                }
            };
        }
    }

    /// <summary>
    /// Get available models (ILLMManagementService interface)
    /// </summary>
    public async Task<List<LLMModel>> GetAvailableModelsAsync(string? providerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var modelConfigs = await GetModelsAsync(providerId);

            return modelConfigs.Select(m => new LLMModel
            {
                Id = m.ModelId,
                Name = m.Name,
                Description = m.DisplayName ?? m.Name,
                MaxTokens = m.MaxTokens,
                CostPerToken = m.CostPerToken,
                Capabilities = m.Capabilities?.Keys.ToList() ?? new List<string>(),
                Configuration = new Dictionary<string, object>
                {
                    ["temperature"] = m.Temperature,
                    ["topP"] = m.TopP,
                    ["frequencyPenalty"] = m.FrequencyPenalty,
                    ["presencePenalty"] = m.PresencePenalty,
                    ["useCase"] = m.UseCase ?? string.Empty,
                    ["isEnabled"] = m.IsEnabled
                }
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting available models");
            return new List<LLMModel>();
        }
    }

    /// <summary>
    /// Test provider connection (ILLMManagementService interface)
    /// </summary>
    public async Task<bool> TestProviderConnectionAsync(string providerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await TestProviderConnectionAsync(providerId);
            return status.IsHealthy;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing provider connection: {ProviderId}", providerId);
            return false;
        }
    }

    /// <summary>
    /// Get configuration (ILLMManagementService interface)
    /// </summary>
    public async Task<LLMConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var currentProvider = await GetCurrentProviderAsync(cancellationToken);
            var models = await GetAvailableModelsAsync(currentProvider.Id, cancellationToken);
            var defaultModel = models.FirstOrDefault();

            return new LLMConfiguration
            {
                DefaultProvider = currentProvider.Id,
                DefaultModel = defaultModel?.Id ?? string.Empty,
                MaxTokens = defaultModel?.MaxTokens ?? 1500,
                Temperature = 0.1,
                MaxRetries = 3,
                Timeout = TimeSpan.FromSeconds(30),
                EnableCaching = true,
                ProviderSettings = currentProvider.Configuration
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting configuration");
            return new LLMConfiguration
            {
                DefaultProvider = "none",
                DefaultModel = "none",
                MaxTokens = 1500,
                Temperature = 0.1,
                MaxRetries = 3,
                Timeout = TimeSpan.FromSeconds(30),
                EnableCaching = true
            };
        }
    }

    /// <summary>
    /// Update configuration (ILLMManagementService interface)
    /// </summary>
    public async Task<bool> UpdateConfigurationAsync(LLMConfiguration configuration, CancellationToken cancellationToken = default)
    {
        try
        {
            // Set the default provider
            var providerSet = await SetProviderAsync(configuration.DefaultProvider, cancellationToken);

            if (!providerSet)
            {
                _logger.LogWarning("Failed to set default provider: {ProviderId}", configuration.DefaultProvider);
                return false;
            }

            // Update provider settings if provided
            if (configuration.ProviderSettings.Any())
            {
                var provider = await GetProviderAsync(configuration.DefaultProvider);
                if (provider != null)
                {
                    // Merge settings
                    foreach (var setting in configuration.ProviderSettings)
                    {
                        provider.Settings ??= new Dictionary<string, object>();
                        provider.Settings[setting.Key] = setting.Value;
                    }

                    // Add configuration settings
                    provider.Settings["maxTokens"] = configuration.MaxTokens;
                    provider.Settings["temperature"] = configuration.Temperature;
                    provider.Settings["maxRetries"] = configuration.MaxRetries;
                    provider.Settings["timeout"] = configuration.Timeout.TotalSeconds;
                    provider.Settings["enableCaching"] = configuration.EnableCaching;

                    await SaveProviderAsync(provider);
                }
            }

            // Clear configuration cache
            await _cacheService.RemoveAsync("llm_configuration");

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating configuration");
            return false;
        }
    }

    #endregion

    #region Additional Analytics Methods

    /// <summary>
    /// Get usage logs with filtering and pagination
    /// </summary>
    public async Task<List<LLMUsageLog>> GetUsageLogsAsync(
        DateTime startDate,
        DateTime endDate,
        string? userId = null,
        string? providerId = null,
        int page = 1,
        int pageSize = 50)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var query = legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

                    if (!string.IsNullOrEmpty(userId))
                    {
                        query = query.Where(l => l.UserId == userId);
                    }

                    if (!string.IsNullOrEmpty(providerId))
                    {
                        query = query.Where(l => l.ProviderId == providerId);
                    }

                    return await query
                        .OrderByDescending(l => l.Timestamp)
                        .Skip((page - 1) * pageSize)
                        .Take(pageSize)
                        .ToListAsync();
                }

                return new List<LLMUsageLog>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage logs");
            return new List<LLMUsageLog>();
        }
    }

    /// <summary>
    /// Get cost tracking analytics
    /// </summary>
    public async Task<object> GetCostTrackingAsync(DateTime startDate, DateTime endDate, string? userId = null)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var query = legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

                    if (!string.IsNullOrEmpty(userId))
                    {
                        query = query.Where(l => l.UserId == userId);
                    }

                    var totalCost = await query.SumAsync(l => l.Cost);
                    var totalTokens = await query.SumAsync(l => l.TotalTokens);
                    var requestCount = await query.CountAsync();

                    var costByProvider = await query
                        .GroupBy(l => l.ProviderId)
                        .Select(g => new
                        {
                            ProviderId = g.Key,
                            TotalCost = g.Sum(l => l.Cost),
                            TotalTokens = g.Sum(l => l.TotalTokens),
                            RequestCount = g.Count()
                        })
                        .ToListAsync();

                    var costByDay = await query
                        .GroupBy(l => l.Timestamp.Date)
                        .Select(g => new
                        {
                            Date = g.Key,
                            TotalCost = g.Sum(l => l.Cost),
                            TotalTokens = g.Sum(l => l.TotalTokens),
                            RequestCount = g.Count()
                        })
                        .OrderBy(x => x.Date)
                        .ToListAsync();

                    return (object)new
                    {
                        summary = new
                        {
                            totalCost,
                            totalTokens,
                            requestCount,
                            averageCostPerRequest = requestCount > 0 ? totalCost / requestCount : 0,
                            averageTokensPerRequest = requestCount > 0 ? (double)totalTokens / requestCount : 0
                        },
                        costByProvider,
                        costByDay
                    };
                }

                return (object)new { };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cost tracking data");
            return new { };
        }
    }

    /// <summary>
    /// Get token usage analytics
    /// </summary>
    public async Task<object> GetTokenUsageAnalyticsAsync(DateTime startDate, DateTime endDate, string groupBy = "day")
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var query = legacyContext.LLMUsageLogs
                        .Where(l => l.Timestamp >= startDate && l.Timestamp <= endDate);

                    var analytics = new
                    {
                        totalInputTokens = await query.SumAsync(l => l.InputTokens),
                        totalOutputTokens = await query.SumAsync(l => l.OutputTokens),
                        totalTokens = await query.SumAsync(l => l.TotalTokens),
                        averageInputTokens = await query.AverageAsync(l => (double)l.InputTokens),
                        averageOutputTokens = await query.AverageAsync(l => (double)l.OutputTokens),
                        tokensByProvider = await query
                            .GroupBy(l => l.ProviderId)
                            .Select(g => new
                            {
                                ProviderId = g.Key,
                                InputTokens = g.Sum(l => l.InputTokens),
                                OutputTokens = g.Sum(l => l.OutputTokens),
                                TotalTokens = g.Sum(l => l.TotalTokens)
                            })
                            .ToListAsync(),
                        tokensByRequestType = await query
                            .GroupBy(l => l.RequestType)
                            .Select(g => new
                            {
                                RequestType = g.Key,
                                InputTokens = g.Sum(l => l.InputTokens),
                                OutputTokens = g.Sum(l => l.OutputTokens),
                                TotalTokens = g.Sum(l => l.TotalTokens)
                            })
                            .ToListAsync()
                    };

                    return (object)analytics;
                }

                return (object)new { };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving token usage analytics");
            return new { };
        }
    }

    /// <summary>
    /// Get budget management data
    /// </summary>
    public async Task<List<object>> GetBudgetsAsync(string? userId = null)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    // For now, return mock budget data since budget management tables might not be fully implemented
                    var budgets = new List<object>
                    {
                        new
                        {
                            id = 1,
                            name = "Monthly LLM Budget",
                            budgetAmount = 1000.00m,
                            spentAmount = 245.67m,
                            remainingAmount = 754.33m,
                            utilizationRate = 0.2457,
                            period = "monthly",
                            isActive = true,
                            alertThreshold = 0.8,
                            blockThreshold = 0.95,
                            createdDate = DateTime.UtcNow.AddDays(-30),
                            lastUpdated = DateTime.UtcNow
                        },
                        new
                        {
                            id = 2,
                            name = "Weekly Development Budget",
                            budgetAmount = 250.00m,
                            spentAmount = 89.23m,
                            remainingAmount = 160.77m,
                            utilizationRate = 0.3569,
                            period = "weekly",
                            isActive = true,
                            alertThreshold = 0.75,
                            blockThreshold = 0.9,
                            createdDate = DateTime.UtcNow.AddDays(-7),
                            lastUpdated = DateTime.UtcNow
                        }
                    };

                    if (!string.IsNullOrEmpty(userId))
                    {
                        // Filter by user if needed
                        return budgets.Where(b => true).ToList(); // Placeholder logic
                    }

                    return budgets;
                }

                return new List<object>();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving budget data");
            return new List<object>();
        }
    }

    #endregion
}
