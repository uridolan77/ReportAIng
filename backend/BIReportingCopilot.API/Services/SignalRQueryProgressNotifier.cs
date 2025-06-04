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
        _hubContext = hubContext;
        _logger = logger;
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

            _logger.LogDebug("📡 Sending SignalR progress notification - User: {UserId}, QueryId: {QueryId}, Stage: {Stage}, Progress: {Progress}%",
                userId, queryId, stage, progress);

            // Send to user-specific group
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("QueryProcessingProgress", progressData);
            
            // Also send to query-specific group for detailed tracking
            await _hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryProcessingProgress", progressData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR progress notification for user {UserId}, query {QueryId}", userId, queryId);
        }
    }
}
