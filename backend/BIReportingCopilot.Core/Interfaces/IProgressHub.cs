namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for SignalR hub that handles progress reporting
/// </summary>
public interface IProgressHub
{
    /// <summary>
    /// Send auto-generation progress update to a specific user
    /// </summary>
    Task SendAutoGenerationProgressAsync(string userId, object progressData);
}
