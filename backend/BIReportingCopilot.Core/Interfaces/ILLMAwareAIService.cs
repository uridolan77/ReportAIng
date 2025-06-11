using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Enhanced AI service interface that uses LLM Management system for provider and model selection
/// </summary>
public interface ILLMAwareAIService : IAIService
{
    /// <summary>
    /// Generate SQL using the configured model for SQL generation use case
    /// </summary>
    Task<string> GenerateSQLWithManagedModelAsync(string prompt, string? providerId = null, string? modelId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate insights using the configured model for insights use case
    /// </summary>
    Task<string> GenerateInsightsWithManagedModelAsync(string prompt, string? providerId = null, string? modelId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate completion using a specific managed model
    /// </summary>
    Task<string> GenerateCompletionWithManagedModelAsync(string prompt, string useCase, string? providerId = null, string? modelId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Stream SQL generation using managed models
    /// </summary>
    IAsyncEnumerable<StreamingResponse> GenerateSQLStreamWithManagedModelAsync(
        string prompt, 
        SchemaMetadata? schema = null, 
        QueryContext? context = null,
        string? providerId = null,
        string? modelId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Log usage to LLM Management system
    /// </summary>
    Task LogUsageAsync(LLMUsageLog usageLog);

    /// <summary>
    /// Get the best model for a specific use case
    /// </summary>
    Task<LLMModelConfig?> GetBestModelForUseCaseAsync(string useCase, string? preferredProviderId = null);

    /// <summary>
    /// Get available providers and their status
    /// </summary>
    Task<List<LLMProviderStatus>> GetAvailableProvidersAsync();
}
