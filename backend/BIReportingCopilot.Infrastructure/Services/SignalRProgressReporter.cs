using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Services;

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
        await SendProgressUpdateAsync(userId, progress, message, stage, currentTable, currentColumn, null, null, null, null);
    }

    public async Task SendProgressUpdateAsync(string userId, double progress, string message, string stage, string? currentTable = null, string? currentColumn = null, int? tablesProcessed = null, int? totalTables = null, int? columnsProcessed = null, int? totalColumns = null, object? aiPrompt = null)
    {
        try
        {
            _logger.LogInformation("📡 Sending progress update to user {UserId}: {Progress}% - {Message} ({Stage})", userId, progress, message, stage);
            _logger.LogInformation("📡 Target group: user_{UserId}, Table: {CurrentTable}, Column: {CurrentColumn}", userId, currentTable, currentColumn);

            var progressData = new
            {
                Progress = progress,
                Message = message,
                Stage = stage,
                CurrentTable = currentTable,
                CurrentColumn = currentColumn,
                TablesProcessed = tablesProcessed,
                TotalTables = totalTables,
                ColumnsProcessed = columnsProcessed,
                TotalColumns = totalColumns,
                AIPrompt = aiPrompt,
                Timestamp = DateTime.UtcNow
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
}
