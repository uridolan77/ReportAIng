using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.AI;

/// <summary>
/// LLM management service interface for managing language models
/// </summary>
public interface ILLMManagementService
{
    Task<List<LLMProvider>> GetAvailableProvidersAsync(CancellationToken cancellationToken = default);
    Task<LLMProvider> GetCurrentProviderAsync(CancellationToken cancellationToken = default);
    Task<bool> SetProviderAsync(string providerId, CancellationToken cancellationToken = default);
    Task<LLMUsageStatistics> GetUsageStatisticsAsync(DateTime? from = null, DateTime? to = null, CancellationToken cancellationToken = default);
    Task<List<LLMModel>> GetAvailableModelsAsync(string? providerId = null, CancellationToken cancellationToken = default);
    Task<bool> TestProviderConnectionAsync(string providerId, CancellationToken cancellationToken = default);
    Task<LLMConfiguration> GetConfigurationAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateConfigurationAsync(LLMConfiguration configuration, CancellationToken cancellationToken = default);
    Task<List<LLMProviderConfig>> GetProvidersAsync();

    // Additional methods expected by Infrastructure services
    Task<LLMModelConfig?> GetModelAsync(string modelId, CancellationToken cancellationToken = default);
    Task<LLMModelConfig?> GetDefaultModelAsync(string useCase, CancellationToken cancellationToken = default);
    Task LogUsageAsync(LLMUsageLog usageLog, CancellationToken cancellationToken = default);
    Task<List<LLMProviderStatus>> GetProviderHealthStatusAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// LLM provider information
/// </summary>
public class LLMProvider
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsHealthy { get; set; }
    public List<LLMModel> SupportedModels { get; set; } = new();
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// LLM model information
/// </summary>
public class LLMModel
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public int MaxTokens { get; set; }
    public decimal CostPerToken { get; set; }
    public List<string> Capabilities { get; set; } = new();

    // Properties expected by Infrastructure services
    public string Description { get; set; } = string.Empty;
    public Dictionary<string, object> Configuration { get; set; } = new();
}

/// <summary>
/// LLM usage statistics
/// </summary>
public class LLMUsageStatistics
{
    public int TotalRequests { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public double AverageResponseTime { get; set; }
    public Dictionary<string, int> RequestsByModel { get; set; } = new();
    public Dictionary<string, int> RequestsByProvider { get; set; } = new();
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }

    // Properties expected by Infrastructure services
    public int SuccessfulRequests { get; set; }
    public int FailedRequests { get; set; }
    public DateRange Period { get; set; } = new();
    public Dictionary<string, ProviderUsageStats> ProviderBreakdown { get; set; } = new();
    public Dictionary<string, ModelUsageStats> ModelBreakdown { get; set; } = new();
}

/// <summary>
/// Date range for statistics
/// </summary>
public class DateRange
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
}

/// <summary>
/// Provider usage statistics
/// </summary>
public class ProviderUsageStats
{
    public int Requests { get; set; }
    public decimal Cost { get; set; }
    public int Tokens { get; set; }
}

/// <summary>
/// Model usage statistics
/// </summary>
public class ModelUsageStats
{
    public int Requests { get; set; }
    public decimal Cost { get; set; }
    public int Tokens { get; set; }
}

/// <summary>
/// LLM configuration
/// </summary>
public class LLMConfiguration
{
    public string DefaultProvider { get; set; } = string.Empty;
    public string DefaultModel { get; set; } = string.Empty;
    public int MaxTokens { get; set; } = 1500;
    public double Temperature { get; set; } = 0.1;
    public int MaxRetries { get; set; } = 3;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
    public bool EnableCaching { get; set; } = true;
    public Dictionary<string, object> ProviderSettings { get; set; } = new();
}
