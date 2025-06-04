using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// SignalR-based implementation of progress reporting
/// </summary>
public class SignalRProgressReporter : IProgressReporter
{
    private readonly IHubContext<Hub> _hubContext;
    private readonly ILogger<SignalRProgressReporter> _logger;

    public SignalRProgressReporter(IHubContext<Hub> hubContext, ILogger<SignalRProgressReporter> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendProgressUpdateAsync(string userId, double progress, string message, string stage, string? currentTable = null, string? currentColumn = null)
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
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("📡 Progress data: {@ProgressData}", progressData);

            // Use group-based messaging to match the SignalR hub setup
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("AutoGenerationProgress", progressData);

            _logger.LogInformation("📡 Progress update sent successfully to user group user_{UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send progress update to user {UserId}", userId);
        }
    }
}
