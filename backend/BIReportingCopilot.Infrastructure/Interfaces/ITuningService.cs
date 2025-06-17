namespace BIReportingCopilot.Infrastructure.Interfaces;

/// <summary>
/// Infrastructure-specific tuning service interface
/// </summary>
public interface ITuningService
{
    Task<string> GetRecommendationsAsync(CancellationToken cancellationToken = default);
    Task<bool> ApplyRecommendationsAsync(List<string> recommendationIds, CancellationToken cancellationToken = default);
    Task<string> AnalyzePerformanceAsync(string request, CancellationToken cancellationToken = default);
    Task<string> OptimizeQueryAsync(string query, CancellationToken cancellationToken = default);
}
