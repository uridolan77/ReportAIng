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
}
