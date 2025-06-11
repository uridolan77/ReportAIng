using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for LLM management operations including provider configuration,
/// usage tracking, cost monitoring, and performance analytics
/// </summary>
public interface ILLMManagementService
{
    #region Provider Management
    
    /// <summary>
    /// Get all configured LLM providers
    /// </summary>
    Task<List<LLMProviderConfig>> GetProvidersAsync();
    
    /// <summary>
    /// Get a specific provider configuration
    /// </summary>
    Task<LLMProviderConfig?> GetProviderAsync(string providerId);
    
    /// <summary>
    /// Create or update a provider configuration
    /// </summary>
    Task<LLMProviderConfig> SaveProviderAsync(LLMProviderConfig provider);
    
    /// <summary>
    /// Delete a provider configuration
    /// </summary>
    Task<bool> DeleteProviderAsync(string providerId);
    
    /// <summary>
    /// Test connection to a provider
    /// </summary>
    Task<LLMProviderStatus> TestProviderConnectionAsync(string providerId);
    
    /// <summary>
    /// Get health status for all providers
    /// </summary>
    Task<List<LLMProviderStatus>> GetProviderHealthStatusAsync();
    
    #endregion
    
    #region Model Management
    
    /// <summary>
    /// Get all configured models for a provider
    /// </summary>
    Task<List<LLMModelConfig>> GetModelsAsync(string? providerId = null);
    
    /// <summary>
    /// Get a specific model configuration
    /// </summary>
    Task<LLMModelConfig?> GetModelAsync(string modelId);
    
    /// <summary>
    /// Save model configuration
    /// </summary>
    Task<LLMModelConfig> SaveModelAsync(LLMModelConfig model);
    
    /// <summary>
    /// Delete a model configuration
    /// </summary>
    Task<bool> DeleteModelAsync(string modelId);
    
    /// <summary>
    /// Get default model for a specific use case
    /// </summary>
    Task<LLMModelConfig?> GetDefaultModelAsync(string useCase);
    
    /// <summary>
    /// Set default model for a use case
    /// </summary>
    Task<bool> SetDefaultModelAsync(string modelId, string useCase);
    
    #endregion
    
    #region Usage Tracking
    
    /// <summary>
    /// Log an LLM usage event
    /// </summary>
    Task LogUsageAsync(LLMUsageLog usageLog);
    
    /// <summary>
    /// Get usage history with filtering
    /// </summary>
    Task<List<LLMUsageLog>> GetUsageHistoryAsync(
        DateTime? startDate = null,
        DateTime? endDate = null,
        string? providerId = null,
        string? modelId = null,
        string? userId = null,
        string? requestType = null,
        int skip = 0,
        int take = 100);
    
    /// <summary>
    /// Get usage analytics for a date range
    /// </summary>
    Task<LLMUsageAnalytics> GetUsageAnalyticsAsync(
        DateTime startDate,
        DateTime endDate,
        string? providerId = null,
        string? modelId = null);
    
    /// <summary>
    /// Export usage data
    /// </summary>
    Task<byte[]> ExportUsageDataAsync(
        DateTime startDate,
        DateTime endDate,
        string format = "csv");
    
    #endregion
    
    #region Cost Management
    
    /// <summary>
    /// Get cost summary for a date range
    /// </summary>
    Task<List<LLMCostSummary>> GetCostSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        string? providerId = null);
    
    /// <summary>
    /// Get real-time cost tracking
    /// </summary>
    Task<decimal> GetCurrentMonthCostAsync(string? providerId = null);
    
    /// <summary>
    /// Get cost projections based on current usage
    /// </summary>
    Task<Dictionary<string, decimal>> GetCostProjectionsAsync();
    
    /// <summary>
    /// Set cost alerts and limits
    /// </summary>
    Task<bool> SetCostLimitAsync(string providerId, decimal monthlyLimit);
    
    /// <summary>
    /// Get cost alerts
    /// </summary>
    Task<List<CostAlert>> GetCostAlertsAsync();
    
    #endregion
    
    #region Performance Monitoring
    
    /// <summary>
    /// Get performance metrics for providers
    /// </summary>
    Task<Dictionary<string, ProviderPerformanceMetrics>> GetPerformanceMetricsAsync(
        DateTime startDate,
        DateTime endDate);
    
    /// <summary>
    /// Get cache hit rates for LLM requests
    /// </summary>
    Task<Dictionary<string, double>> GetCacheHitRatesAsync();
    
    /// <summary>
    /// Get error rates and failure analysis
    /// </summary>
    Task<Dictionary<string, ErrorAnalysis>> GetErrorAnalysisAsync(
        DateTime startDate,
        DateTime endDate);
    
    #endregion
}

/// <summary>
/// Cost alert configuration
/// </summary>
public class CostAlert
{
    public string Id { get; set; } = string.Empty;
    public string ProviderId { get; set; } = string.Empty;
    public decimal ThresholdAmount { get; set; }
    public string AlertType { get; set; } = string.Empty; // Warning, Critical
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Provider performance metrics
/// </summary>
public class ProviderPerformanceMetrics
{
    public string ProviderId { get; set; } = string.Empty;
    public long AverageResponseTime { get; set; }
    public long MedianResponseTime { get; set; }
    public long P95ResponseTime { get; set; }
    public double SuccessRate { get; set; }
    public double ErrorRate { get; set; }
    public int TotalRequests { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
}

/// <summary>
/// Error analysis data
/// </summary>
public class ErrorAnalysis
{
    public string ProviderId { get; set; } = string.Empty;
    public int TotalErrors { get; set; }
    public double ErrorRate { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public Dictionary<string, int> ErrorsByModel { get; set; } = new();
    public List<string> CommonErrorMessages { get; set; } = new();
}
