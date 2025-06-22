using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.Interfaces.Transparency
{
    /// <summary>
    /// Service interface for AI transparency business logic and data aggregation
    /// </summary>
    public interface ITransparencyService
    {
        /// <summary>
        /// Get confidence breakdown for a specific analysis/trace
        /// </summary>
        Task<ConfidenceBreakdown> GetConfidenceBreakdownAsync(string analysisId);

        /// <summary>
        /// Get alternative options for a specific trace
        /// </summary>
        Task<List<AlternativeOptionDto>> GetAlternativeOptionsAsync(string traceId);

        /// <summary>
        /// Generate optimization suggestions for a prompt/trace
        /// </summary>
        Task<List<OptimizationSuggestionDto>> GetOptimizationSuggestionsAsync(string userQuery, string? traceId = null);

        /// <summary>
        /// Get aggregated transparency metrics
        /// </summary>
        Task<TransparencyMetricsDto> GetTransparencyMetricsAsync(string? userId = null, int days = 7);

        /// <summary>
        /// Get dashboard-specific transparency metrics
        /// </summary>
        Task<TransparencyDashboardMetricsDto> GetDashboardMetricsAsync(int days = 30);

        /// <summary>
        /// Get transparency settings for a user
        /// </summary>
        Task<TransparencySettingsDto> GetTransparencySettingsAsync(string userId);

        /// <summary>
        /// Update transparency settings for a user
        /// </summary>
        Task UpdateTransparencySettingsAsync(string userId, TransparencySettingsDto settings);

        /// <summary>
        /// Export transparency data
        /// </summary>
        Task<byte[]> ExportTransparencyDataAsync(ExportTransparencyRequest request);

        /// <summary>
        /// Get recent transparency traces for a user
        /// </summary>
        Task<List<TransparencyTraceDto>> GetRecentTracesAsync(string? userId = null, int limit = 10);

        /// <summary>
        /// Get detailed trace information with steps
        /// </summary>
        Task<TransparencyTraceDetailDto> GetTraceDetailAsync(string traceId);

        /// <summary>
        /// Get confidence trends over time
        /// </summary>
        Task<List<ConfidenceTrendDto>> GetConfidenceTrendsAsync(string? userId = null, int days = 30);

        /// <summary>
        /// Get token usage analytics
        /// </summary>
        Task<TokenUsageAnalyticsDto> GetTokenUsageAnalyticsAsync(string? userId = null, int days = 30);

        /// <summary>
        /// Get performance metrics for transparency operations
        /// </summary>
        Task<TransparencyPerformanceDto> GetPerformanceMetricsAsync(int days = 7);
    }
}
