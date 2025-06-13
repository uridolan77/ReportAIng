using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Messaging;

namespace BIReportingCopilot.Infrastructure.Messaging;

/// <summary>
/// SignalR-based implementation of progress reporting
/// Uses QueryStatusHub since that's where the frontend connects
/// </summary>
public class SignalRProgressReporter : IProgressReporter
{
    private readonly IHubContext _hubContext;
    private readonly ILogger<SignalRProgressReporter> _logger;

    // Constructor that accepts any hub context (for DI flexibility)
    public SignalRProgressReporter(object hubContext, ILogger<SignalRProgressReporter> logger)
    {
        _hubContext = hubContext as IHubContext ?? throw new ArgumentException("Hub context must implement IHubContext", nameof(hubContext));
        _logger = logger;
    }

    public async Task SendProgressUpdateAsync(string userId, double progress, string message, string stage, string? currentTable = null, string? currentColumn = null)
    {
        await SendProgressUpdateAsync(userId, progress, message, stage, currentTable, currentColumn, null, null, null, null, null, null);
    }

    public async Task SendProgressUpdateAsync(string userId, double progress, string message, string stage, string? currentTable = null, string? currentColumn = null, int? tablesProcessed = null, int? totalTables = null, int? columnsProcessed = null, int? totalColumns = null, int? glossaryTermsGenerated = null, int? relationshipsFound = null, object? aiPrompt = null)
    {
        try
        {
            _logger.LogInformation("📡 Sending progress update to user {UserId}: {Progress}% - {Message} ({Stage})", userId, progress, message, stage);
            _logger.LogInformation("📡 Target group: user_{UserId}, Table: {CurrentTable}, Column: {CurrentColumn}", userId, currentTable, currentColumn);
            _logger.LogInformation("📡 Statistics: Tables {TablesProcessed}/{TotalTables}, Columns {ColumnsProcessed}/{TotalColumns}, Glossary {GlossaryTerms}, Relationships {Relationships}",
                tablesProcessed, totalTables, columnsProcessed, totalColumns, glossaryTermsGenerated, relationshipsFound);

            // Use consistent field naming (PascalCase for C# convention, but also include camelCase for frontend compatibility)
            var progressData = new
            {
                // Primary fields (PascalCase)
                Progress = progress,
                Message = message,
                Stage = stage,
                CurrentTable = currentTable,
                CurrentColumn = currentColumn,
                TablesProcessed = tablesProcessed ?? 0,
                TotalTables = totalTables ?? 0,
                ColumnsProcessed = columnsProcessed ?? 0,
                TotalColumns = totalColumns ?? 0,
                GlossaryTermsGenerated = glossaryTermsGenerated ?? 0,
                RelationshipsFound = relationshipsFound ?? 0,
                AIPrompt = aiPrompt,
                Timestamp = DateTime.UtcNow,

                // Duplicate fields in camelCase for frontend compatibility
                progress = progress,
                message = message,
                stage = stage,
                currentTable = currentTable,
                currentColumn = currentColumn,
                tablesProcessed = tablesProcessed ?? 0,
                totalTables = totalTables ?? 0,
                columnsProcessed = columnsProcessed ?? 0,
                totalColumns = totalColumns ?? 0,
                glossaryTermsGenerated = glossaryTermsGenerated ?? 0,
                relationshipsFound = relationshipsFound ?? 0
            };

            _logger.LogInformation("📡 Progress data: {@ProgressData}", progressData);

            // Send progress update to user group
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("AutoGenerationProgress", progressData);

            _logger.LogInformation("📡 Progress update sent successfully to user group user_{UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send progress update to user {UserId}", userId);
        }
    }

    #region Missing Interface Method Implementations

    /// <summary>
    /// Report progress async (IProgressReporter interface)
    /// </summary>
    public async Task ReportProgressAsync(string operationId, int progressPercentage, string? message = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📊 Reporting progress for operation {OperationId}: {Progress}%", operationId, progressPercentage);

            var progressData = new
            {
                OperationId = operationId,
                Progress = progressPercentage,
                Message = message ?? $"Progress: {progressPercentage}%",
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("ProgressUpdate", progressData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reporting progress for operation: {OperationId}", operationId);
        }
    }

    /// <summary>
    /// Report started async (IProgressReporter interface)
    /// </summary>
    public async Task ReportStartedAsync(string operationId, string operationName, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🚀 Operation started: {OperationName} ({OperationId})", operationName, operationId);

            var startData = new
            {
                OperationId = operationId,
                OperationName = operationName,
                Status = "Started",
                StartTime = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("OperationStarted", startData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reporting operation start: {OperationId}", operationId);
        }
    }

    /// <summary>
    /// Report completed async (IProgressReporter interface)
    /// </summary>
    public async Task ReportCompletedAsync(string operationId, object? result = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("✅ Operation completed: {OperationId}", operationId);

            var completedData = new
            {
                OperationId = operationId,
                Status = "Completed",
                Result = result,
                CompletedTime = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("OperationCompleted", completedData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reporting operation completion: {OperationId}", operationId);
        }
    }

    /// <summary>
    /// Report failed async (IProgressReporter interface)
    /// </summary>
    public async Task ReportFailedAsync(string operationId, string errorMessage, Exception? exception = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogError(exception, "❌ Operation failed: {OperationId} - {ErrorMessage}", operationId, errorMessage);

            var failedData = new
            {
                OperationId = operationId,
                Status = "Failed",
                ErrorMessage = errorMessage,
                Exception = exception?.Message,
                FailedTime = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("OperationFailed", failedData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reporting operation failure: {OperationId}", operationId);
        }
    }

    /// <summary>
    /// Report step async (IProgressReporter interface)
    /// </summary>
    public async Task ReportStepAsync(string operationId, string stepName, string? stepDetails = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📋 Operation step: {OperationId} - {StepName}", operationId, stepName);

            var stepData = new
            {
                OperationId = operationId,
                StepName = stepName,
                StepDetails = stepDetails,
                StepTime = DateTime.UtcNow
            };

            await _hubContext.Clients.All.SendAsync("OperationStep", stepData, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error reporting operation step: {OperationId}", operationId);
        }
    }

    /// <summary>
    /// Create scope (IProgressReporter interface)
    /// </summary>
    public IDisposable CreateScope(string operationId, string operationName)
    {
        return new ProgressScope(this, operationId, operationName);
    }

    #endregion

    #region Progress Scope Implementation

    private class ProgressScope : IDisposable
    {
        private readonly SignalRProgressReporter _reporter;
        private readonly string _operationId;
        private readonly string _operationName;
        private bool _disposed;

        public ProgressScope(SignalRProgressReporter reporter, string operationId, string operationName)
        {
            _reporter = reporter;
            _operationId = operationId;
            _operationName = operationName;

            // Report operation started
            _ = Task.Run(async () => await _reporter.ReportStartedAsync(_operationId, _operationName));
        }

        public string OperationId => _operationId;

        public async Task ReportProgressAsync(int progressPercentage, string? message = null, CancellationToken cancellationToken = default)
        {
            if (!_disposed)
            {
                await _reporter.ReportProgressAsync(_operationId, progressPercentage, message, cancellationToken);
            }
        }

        public async Task ReportStepAsync(string stepName, string? stepDetails = null, CancellationToken cancellationToken = default)
        {
            if (!_disposed)
            {
                await _reporter.ReportStepAsync(_operationId, stepName, stepDetails, cancellationToken);
            }
        }

        public async Task ReportCompletedAsync(object? result = null, CancellationToken cancellationToken = default)
        {
            if (!_disposed)
            {
                await _reporter.ReportCompletedAsync(_operationId, result, cancellationToken);
                _disposed = true;
            }
        }

        public async Task ReportFailedAsync(string errorMessage, Exception? exception = null, CancellationToken cancellationToken = default)
        {
            if (!_disposed)
            {
                await _reporter.ReportFailedAsync(_operationId, errorMessage, exception, cancellationToken);
                _disposed = true;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                // Complete the operation if not already completed or failed
                _ = Task.Run(async () => await _reporter.ReportCompletedAsync(_operationId));
                _disposed = true;
            }
        }
    }

    #endregion
}
