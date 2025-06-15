namespace BIReportingCopilot.Core.Interfaces.Messaging;

/// <summary>
/// Progress hub interface for real-time progress notifications
/// </summary>
public interface IProgressHub
{
    Task SendProgressUpdateAsync(string userId, string operationId, int progressPercentage, string? message = null);
    Task SendQueryStartedAsync(string userId, string queryId, string query);
    Task SendQueryCompletedAsync(string userId, string queryId, object result);
    Task SendQueryFailedAsync(string userId, string queryId, string error);
    Task SendNotificationAsync(string userId, string title, string message, string type = "info");
    Task SendToGroupAsync(string groupName, string method, object data);
    Task AddToGroupAsync(string connectionId, string groupName);
    Task RemoveFromGroupAsync(string connectionId, string groupName);
    Task SendSystemNotificationAsync(string message, string type = "info");
}

/// <summary>
/// Progress reporter interface for reporting operation progress
/// </summary>
public interface IProgressReporter
{
    Task ReportProgressAsync(string operationId, int progressPercentage, string? message = null, CancellationToken cancellationToken = default);
    Task ReportStartedAsync(string operationId, string description, CancellationToken cancellationToken = default);
    Task ReportCompletedAsync(string operationId, object? result = null, CancellationToken cancellationToken = default);
    Task ReportFailedAsync(string operationId, string error, Exception? exception = null, CancellationToken cancellationToken = default);
    Task ReportStepAsync(string operationId, string stepName, string? description = null, CancellationToken cancellationToken = default);
    IDisposable CreateScope(string operationId, string description);
    
    // Method used by TuningService
    Task SendProgressUpdateAsync(string userId, double progress, string message, string stage, 
        string? currentTable = null, string? currentColumn = null, int? tablesProcessed = null, 
        int? totalTables = null, int? columnsProcessed = null, int? totalColumns = null, 
        int? glossaryTermsGenerated = null, int? relationshipsFound = null, object? aiPrompt = null);
}

/// <summary>
/// Progress scope for automatic progress reporting
/// </summary>
public interface IProgressScope : IDisposable
{
    string OperationId { get; }
    Task ReportProgressAsync(int progressPercentage, string? message = null);
    Task ReportStepAsync(string stepName, string? description = null);
    Task ReportCompletedAsync(object? result = null);
    Task ReportFailedAsync(string error, Exception? exception = null);
}

/// <summary>
/// Progress update data
/// </summary>
public class ProgressUpdate
{
    public string OperationId { get; set; } = string.Empty;
    public int ProgressPercentage { get; set; }
    public string? Message { get; set; }
    public string? CurrentStep { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Notification data
/// </summary>
public class NotificationData
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = "info"; // info, success, warning, error
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public Dictionary<string, object> Data { get; set; } = new();
}
