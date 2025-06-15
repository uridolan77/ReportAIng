namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for notifying query processing progress
/// </summary>
public interface IQueryProgressNotifier
{
    /// <summary>
    /// Notify processing stage with detailed information
    /// </summary>
    Task NotifyProcessingStageAsync(string userId, string queryId, string stage, string message, int progress, object? details = null);

    /// <summary>
    /// Notify progress with stage information
    /// </summary>
    Task NotifyProgressAsync(string userId, double progress, string message, string stage);
}
