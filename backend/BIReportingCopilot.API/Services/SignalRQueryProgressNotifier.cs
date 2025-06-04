using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.API.Services;

/// <summary>
/// SignalR implementation of query progress notifier
/// </summary>
public class SignalRQueryProgressNotifier : IQueryProgressNotifier
{
    private readonly IHubContext<QueryStatusHub> _hubContext;
    private readonly ILogger<SignalRQueryProgressNotifier> _logger;

    public SignalRQueryProgressNotifier(
        IHubContext<QueryStatusHub> hubContext,
        ILogger<SignalRQueryProgressNotifier> logger)
    {
        _hubContext = hubContext ?? throw new ArgumentNullException(nameof(hubContext));
        _logger = logger;

        _logger.LogInformation("🔧 SignalRQueryProgressNotifier initialized with hub context: {HubContextType}",
            _hubContext?.GetType().Name ?? "NULL");
    }

    public async Task NotifyProcessingStageAsync(string userId, string queryId, string stage, string message, int progress, object? details = null)
    {
        try
        {
            var progressData = new
            {
                QueryId = queryId,
                Stage = stage,
                Message = message,
                Progress = progress,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("📡 Sending SignalR progress notification - User: {UserId}, QueryId: {QueryId}, Stage: {Stage}, Progress: {Progress}%",
                userId, queryId, stage, progress);
            _logger.LogInformation("📡 Progress data: {@ProgressData}", progressData);

            // Send to user-specific group
            _logger.LogInformation("📡 Sending to user group: user_{UserId}", userId);
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("QueryProcessingProgress", progressData);

            // Also send to query-specific group for detailed tracking
            _logger.LogInformation("📡 Sending to query group: query_{QueryId}", queryId);
            await _hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryProcessingProgress", progressData);

            // Also send to all connected clients for debugging
            _logger.LogInformation("📡 Broadcasting to all clients for debugging");
            await _hubContext.Clients.All.SendAsync("QueryProcessingProgress", progressData);

            _logger.LogInformation("📡 SignalR progress notification sent successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR progress notification for user {UserId}, query {QueryId}", userId, queryId);
        }
    }
}
