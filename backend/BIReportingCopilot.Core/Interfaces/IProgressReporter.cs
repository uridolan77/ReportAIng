namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for reporting progress updates during long-running operations
/// </summary>
public interface IProgressReporter
{
    /// <summary>
    /// Send a progress update to a specific user
    /// </summary>
    /// <param name="userId">User ID to send the update to</param>
    /// <param name="progress">Progress percentage (0-100)</param>
    /// <param name="message">Progress message</param>
    /// <param name="stage">Current stage of the operation</param>
    /// <param name="currentTable">Current table being processed (optional)</param>
    /// <param name="currentColumn">Current column being processed (optional)</param>
    Task SendProgressUpdateAsync(string userId, double progress, string message, string stage, string? currentTable = null, string? currentColumn = null);

    /// <summary>
    /// Send a detailed progress update with additional information
    /// </summary>
    /// <param name="userId">User ID to send the update to</param>
    /// <param name="progress">Progress percentage (0-100)</param>
    /// <param name="message">Progress message</param>
    /// <param name="stage">Current stage of the operation</param>
    /// <param name="currentTable">Current table being processed (optional)</param>
    /// <param name="currentColumn">Current column being processed (optional)</param>
    /// <param name="tablesProcessed">Number of tables processed (optional)</param>
    /// <param name="totalTables">Total number of tables (optional)</param>
    /// <param name="columnsProcessed">Number of columns processed (optional)</param>
    /// <param name="totalColumns">Total number of columns (optional)</param>
    /// <param name="aiPrompt">AI prompt information (optional)</param>
    Task SendProgressUpdateAsync(string userId, double progress, string message, string stage, string? currentTable = null, string? currentColumn = null, int? tablesProcessed = null, int? totalTables = null, int? columnsProcessed = null, int? totalColumns = null, object? aiPrompt = null);
}
