using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.Infrastructure.AI.Streaming;

/// <summary>
/// SignalR hub for real-time streaming functionality
/// </summary>
[Authorize]
public class StreamingHub : Hub
{
    private readonly ILogger<StreamingHub> _logger;
    private readonly IRealTimeStreamingService _streamingService;

    public StreamingHub(
        ILogger<StreamingHub> logger,
        IRealTimeStreamingService streamingService)
    {
        _logger = logger;
        _streamingService = streamingService;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("🌊 StreamingHub - User {UserId} connected with connection {ConnectionId}", userId, connectionId);

        // Add to user group for targeted messaging
        await Groups.AddToGroupAsync(connectionId, $"user_{userId}");

        // Notify connection established
        await Clients.Caller.SendAsync("StreamingConnected", new
        {
            UserId = userId,
            ConnectionId = connectionId,
            Timestamp = DateTime.UtcNow,
            Message = "Connected to streaming hub"
        });

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        var connectionId = Context.ConnectionId;

        _logger.LogInformation("🌊 StreamingHub - User {UserId} disconnected from connection {ConnectionId}", userId, connectionId);

        // Remove from user group
        await Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");

        // Clean up any active streaming sessions
        await CleanupUserSessionsAsync(userId);

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Start a streaming session for real-time updates
    /// </summary>
    public async Task StartStreamingSession(StreamingSessionConfig config)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🚀 Starting streaming session for user {UserId}", userId);

            var sessionInfo = await _streamingService.StartStreamingSessionAsync(userId, config);

            // Add connection to session group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"session_{sessionInfo.SessionId}");

            // Notify session started
            await Clients.Caller.SendAsync("StreamingSessionStarted", new
            {
                SessionId = sessionInfo.SessionId,
                UserId = userId,
                Configuration = config,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("✅ Streaming session {SessionId} started for user {UserId}", sessionInfo.SessionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting streaming session for user {UserId}", GetCurrentUserId());
            await Clients.Caller.SendAsync("StreamingError", new
            {
                Error = "Failed to start streaming session",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Stop a streaming session
    /// </summary>
    public async Task StopStreamingSession(string sessionId)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🛑 Stopping streaming session {SessionId} for user {UserId}", sessionId, userId);

            await _streamingService.StopStreamingSessionAsync(sessionId);

            // Remove connection from session group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"session_{sessionId}");

            // Notify session stopped
            await Clients.Caller.SendAsync("StreamingSessionStopped", new
            {
                SessionId = sessionId,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("✅ Streaming session {SessionId} stopped for user {UserId}", sessionId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping streaming session {SessionId}", sessionId);
            await Clients.Caller.SendAsync("StreamingError", new
            {
                Error = "Failed to stop streaming session",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Subscribe to real-time analytics updates
    /// </summary>
    public async Task SubscribeToAnalytics(TimeSpan updateInterval)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("📊 User {UserId} subscribing to analytics with interval {Interval}", userId, updateInterval);

            // Add to analytics group
            await Groups.AddToGroupAsync(Context.ConnectionId, $"analytics_{userId}");

            // Start streaming analytics (this would typically be handled by a background service)
            _ = Task.Run(async () =>
            {
                try
                {
                    await foreach (var update in _streamingService.StreamAnalyticsAsync(userId, updateInterval))
                    {
                        await Clients.Group($"analytics_{userId}").SendAsync("AnalyticsUpdate", update);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error streaming analytics for user {UserId}", userId);
                }
            });

            await Clients.Caller.SendAsync("AnalyticsSubscribed", new
            {
                UserId = userId,
                UpdateInterval = updateInterval,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error subscribing to analytics for user {UserId}", GetCurrentUserId());
            await Clients.Caller.SendAsync("StreamingError", new
            {
                Error = "Failed to subscribe to analytics",
                Details = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Unsubscribe from analytics updates
    /// </summary>
    public async Task UnsubscribeFromAnalytics()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("📊 User {UserId} unsubscribing from analytics", userId);

            // Remove from analytics group
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"analytics_{userId}");

            await Clients.Caller.SendAsync("AnalyticsUnsubscribed", new
            {
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error unsubscribing from analytics for user {UserId}", GetCurrentUserId());
        }
    }

    /// <summary>
    /// Join a specific streaming group
    /// </summary>
    public async Task JoinGroup(string groupName)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogDebug("👥 User {UserId} joining group {GroupName}", userId, groupName);

            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            await Clients.Caller.SendAsync("GroupJoined", new
            {
                GroupName = groupName,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error joining group {GroupName} for user {UserId}", groupName, GetCurrentUserId());
        }
    }

    /// <summary>
    /// Leave a specific streaming group
    /// </summary>
    public async Task LeaveGroup(string groupName)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogDebug("👥 User {UserId} leaving group {GroupName}", userId, groupName);

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            await Clients.Caller.SendAsync("GroupLeft", new
            {
                GroupName = groupName,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error leaving group {GroupName} for user {UserId}", groupName, GetCurrentUserId());
        }
    }

    #region Private Helper Methods

    private string GetCurrentUserId()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var subClaim = Context.User?.FindFirst("sub")?.Value;
            var nameClaim = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var emailClaim = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

            return userId ?? subClaim ?? nameClaim ?? emailClaim ?? "anonymous";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting current user ID, using anonymous");
            return "anonymous";
        }
    }

    private async Task CleanupUserSessionsAsync(string userId)
    {
        try
        {
            var activeSessions = await _streamingService.GetActiveSessionsAsync(userId);
            
            foreach (var session in activeSessions)
            {
                await _streamingService.StopStreamingSessionAsync(session.SessionId);
            }

            _logger.LogDebug("🧹 Cleaned up {SessionCount} active sessions for user {UserId}", activeSessions.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error cleaning up sessions for user {UserId}", userId);
        }
    }

    #endregion
}
