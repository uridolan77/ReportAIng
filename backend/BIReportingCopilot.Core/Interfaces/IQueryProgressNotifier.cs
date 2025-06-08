namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for notifying query processing progress
/// </summary>
public interface IQueryProgressNotifier
{
    /// <summary>
    /// Notify processing stage via real-time communication
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="queryId">Query ID</param>
    /// <param name="stage">Processing stage</param>
    /// <param name="message">Stage message</param>
    /// <param name="progress">Progress percentage (0-100)</param>
    /// <param name="details">Optional stage details</param>
    Task NotifyProcessingStageAsync(string userId, string queryId, string stage, string message, int progress, object? details = null);

    /// <summary>
    /// Notify progress update
    /// </summary>
    /// <param name="userId">User ID</param>
    /// <param name="progress">Progress percentage (0-100)</param>
    /// <param name="message">Progress message</param>
    /// <param name="stage">Current stage</param>
    Task NotifyProgressAsync(string userId, double progress, string message, string stage);
}
