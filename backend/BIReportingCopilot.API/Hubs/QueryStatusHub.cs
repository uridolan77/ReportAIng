using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Messaging;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// Enhanced QueryStatusHub with improved async operations and error handling
/// </summary>
public class QueryStatusHub : BaseHub, IProgressHub
{
    public QueryStatusHub(ILogger<QueryStatusHub> logger) : base(logger)
    {
    }

    protected override async Task OnUserConnectedAsync(string userId, string connectionId)
    {
        _logger.LogInformation("💬 [CHAT-HUB] User {UserId} connected with connection {ConnectionId}", userId, connectionId);
        _logger.LogInformation("💬 [CHAT-HUB] User {UserId} added to group 'user_{UserId}' for real-time chat updates", userId, userId);

        // Log all claims for debugging in development
        if (Context.User?.Claims != null)
        {
            _logger.LogDebug("💬 [CHAT-HUB] User {UserId} claims:", userId);
            foreach (var claim in Context.User.Claims)
            {
                _logger.LogDebug("💬 [CHAT-HUB]   Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
        }

        // Send welcome message to confirm connection
        try
        {
            await Clients.Caller.SendAsync("ConnectionConfirmed", new
            {
                UserId = userId,
                ConnectionId = connectionId,
                Timestamp = DateTime.UtcNow,
                Message = "Successfully connected to chat hub"
            });
            _logger.LogInformation("💬 [CHAT-HUB] Welcome message sent to user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "💬 [CHAT-HUB] Failed to send welcome message to user {UserId}", userId);
        }

        await Task.CompletedTask;
    }

    protected override async Task OnUserDisconnectedAsync(string userId, string connectionId, Exception? exception)
    {
        _logger.LogInformation("💬 [CHAT-HUB] User {UserId} disconnected with connection {ConnectionId}", userId, connectionId);

        if (exception != null)
        {
            _logger.LogError(exception, "💬 [CHAT-HUB] User {UserId} disconnected with error from QueryStatusHub", userId);
        }
        else
        {
            _logger.LogInformation("💬 [CHAT-HUB] User {UserId} disconnected gracefully", userId);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// Client can join a specific query group to receive updates for that query
    /// </summary>
    public async Task JoinQueryGroup(string queryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("JoinQueryGroup called with empty queryId by connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            var userId = GetCurrentUserId();
            _logger.LogDebug("User {UserId} joining query group {QueryId}", userId, queryId);

            var groupName = $"query_{queryId}";
            var success = await SafeAddToGroupAsync(groupName);

            if (success)
            {
                await SafeSendAsync(Clients.Caller, "JoinedQueryGroup", queryId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining query group {QueryId} for connection {ConnectionId}", queryId, Context.ConnectionId);
        }
    }

    /// <summary>
    /// Client can leave a query group
    /// </summary>
    public async Task LeaveQueryGroup(string queryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("LeaveQueryGroup called with empty queryId by connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            var userId = GetCurrentUserId();
            _logger.LogDebug("User {UserId} leaving query group {QueryId}", userId, queryId);

            var groupName = $"query_{queryId}";
            var success = await SafeRemoveFromGroupAsync(groupName);

            if (success)
            {
                await SafeSendAsync(Clients.Caller, "LeftQueryGroup", queryId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving query group {QueryId} for connection {ConnectionId}", queryId, Context.ConnectionId);
        }
    }

    /// <summary>
    /// Send query status update to specific query group with enhanced error handling
    /// </summary>
    public async Task SendQueryStatusUpdate(string queryId, object statusUpdate)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("SendQueryStatusUpdate called with empty queryId");
                return;
            }

            await SafeSendAsync(Clients.Group($"query_{queryId}"), "QueryStatusUpdate", new { QueryId = queryId, StatusUpdate = statusUpdate });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending query status update for {QueryId}", queryId);
        }
    }

    /// <summary>
    /// Send notification to specific user with enhanced error handling
    /// </summary>
    public async Task SendUserNotification(string userId, object notification)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("SendUserNotification called with empty userId");
                return;
            }

            await SafeSendAsync(Clients.Group($"user_{userId}"), "UserNotification", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending user notification to {UserId}", userId);
        }
    }

    /// <summary>
    /// Broadcast system-wide notification with enhanced error handling
    /// </summary>
    public async Task SendSystemNotification(object notification)
    {
        try
        {
            await SafeSendAsync(Clients.All, "SystemNotification", notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system notification");
        }
    }

    /// <summary>
    /// Implementation of IProgressHub interface with enhanced error handling
    /// </summary>
    public async Task SendAutoGenerationProgressAsync(string userId, object progressData)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("SendAutoGenerationProgressAsync called with empty userId");
                return;
            }

            await SafeSendAsync(Clients.Group($"user_{userId}"), "AutoGenerationProgress", progressData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending auto generation progress to user {UserId}", userId);
        }
    }

    // IProgressHub interface implementation
    public async Task SendProgressUpdateAsync(string userId, string operationId, int progressPercentage, string? message = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(operationId))
            {
                _logger.LogWarning("SendProgressUpdateAsync called with empty userId or operationId");
                return;
            }

            await SafeSendAsync(Clients.Group($"user_{userId}"), "ProgressUpdate", new
            {
                OperationId = operationId,
                Progress = Math.Max(0, Math.Min(100, progressPercentage)),
                Message = message,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending progress update to user {UserId}", userId);
        }
    }

    public async Task SendQueryStartedAsync(string userId, string queryId, string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("SendQueryStartedAsync called with empty userId or queryId");
                return;
            }

            await SafeSendAsync(Clients.Group($"user_{userId}"), "QueryStarted", new
            {
                QueryId = queryId,
                Query = query,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending query started to user {UserId}", userId);
        }
    }

    public async Task SendQueryCompletedAsync(string userId, string queryId, object result)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("SendQueryCompletedAsync called with empty userId or queryId");
                return;
            }

            await SafeSendAsync(Clients.Group($"user_{userId}"), "QueryCompleted", new
            {
                QueryId = queryId,
                Result = result,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending query completed to user {UserId}", userId);
        }
    }

    public async Task SendQueryFailedAsync(string userId, string queryId, string error)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("SendQueryFailedAsync called with empty userId or queryId");
                return;
            }

            await SafeSendAsync(Clients.Group($"user_{userId}"), "QueryFailed", new
            {
                QueryId = queryId,
                Error = error,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending query failed to user {UserId}", userId);
        }
    }

    public async Task SendNotificationAsync(string userId, string title, string message, string type = "info")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                _logger.LogWarning("SendNotificationAsync called with empty userId");
                return;
            }

            await SafeSendAsync(Clients.Group($"user_{userId}"), "Notification", new
            {
                Title = title,
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending notification to user {UserId}", userId);
        }
    }

    public async Task SendToGroupAsync(string groupName, string method, object data)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(groupName) || string.IsNullOrWhiteSpace(method))
            {
                _logger.LogWarning("SendToGroupAsync called with empty groupName or method");
                return;
            }

            await SafeSendAsync(Clients.Group(groupName), method, data);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending to group {GroupName} with method {Method}", groupName, method);
        }
    }

    public async Task AddToGroupAsync(string connectionId, string groupName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connectionId) || string.IsNullOrWhiteSpace(groupName))
            {
                _logger.LogWarning("AddToGroupAsync called with empty connectionId or groupName");
                return;
            }

            await Groups.AddToGroupAsync(connectionId, groupName);
            _logger.LogDebug("Added connection {ConnectionId} to group {GroupName}", connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding connection {ConnectionId} to group {GroupName}", connectionId, groupName);
        }
    }

    public async Task RemoveFromGroupAsync(string connectionId, string groupName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(connectionId) || string.IsNullOrWhiteSpace(groupName))
            {
                _logger.LogWarning("RemoveFromGroupAsync called with empty connectionId or groupName");
                return;
            }

            await Groups.RemoveFromGroupAsync(connectionId, groupName);
            _logger.LogDebug("Removed connection {ConnectionId} from group {GroupName}", connectionId, groupName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing connection {ConnectionId} from group {GroupName}", connectionId, groupName);
        }
    }

    public async Task SendSystemNotificationAsync(string message, string type = "info")
    {
        try
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("SendSystemNotificationAsync called with empty message");
                return;
            }

            await SafeSendAsync(Clients.All, "SystemNotification", new
            {
                Message = message,
                Type = type,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending system notification");
        }
    }
}

/// <summary>
/// Enhanced extension methods for easier hub usage from services with improved error handling
/// </summary>
public static class QueryStatusHubExtensions
{
    public static async Task<bool> NotifyQueryStarted(this IHubContext<QueryStatusHub> hubContext, string queryId, string userId, ILogger? logger = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId) || string.IsNullOrWhiteSpace(userId))
                return false;

            await hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryStarted", new
            {
                QueryId = queryId,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Status = "started"
            });
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error notifying query started for {QueryId}", queryId);
            return false;
        }
    }

    public static async Task<bool> NotifyQueryProgress(this IHubContext<QueryStatusHub> hubContext, string queryId, double progress, string? message = null, ILogger? logger = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
                return false;

            await hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryProgress", new
            {
                QueryId = queryId,
                Progress = Math.Max(0, Math.Min(100, progress)), // Clamp between 0-100
                Message = message,
                Timestamp = DateTime.UtcNow,
                Status = "in_progress"
            });
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error notifying query progress for {QueryId}", queryId);
            return false;
        }
    }

    public static async Task<bool> NotifyQueryCompleted(this IHubContext<QueryStatusHub> hubContext, string queryId, bool success, int executionTimeMs, string? error = null, ILogger? logger = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
                return false;

            await hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryCompleted", new
            {
                QueryId = queryId,
                Success = success,
                ExecutionTimeMs = Math.Max(0, executionTimeMs), // Ensure non-negative
                Error = error,
                Timestamp = DateTime.UtcNow,
                Status = success ? "completed" : "failed"
            });
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error notifying query completed for {QueryId}", queryId);
            return false;
        }
    }

    public static async Task<bool> NotifyUserActivity(this IHubContext<QueryStatusHub> hubContext, string userId, string activity, object? details = null, ILogger? logger = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(activity))
                return false;

            await hubContext.Clients.Group($"user_{userId}").SendAsync("UserActivity", new
            {
                UserId = userId,
                Activity = activity,
                Details = details,
                Timestamp = DateTime.UtcNow
            });
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error notifying user activity for {UserId}", userId);
            return false;
        }
    }

    public static async Task<bool> NotifySystemAlert(this IHubContext<QueryStatusHub> hubContext, string alertType, string message, string severity = "info", ILogger? logger = null)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(alertType) || string.IsNullOrWhiteSpace(message))
                return false;

            await hubContext.Clients.All.SendAsync("SystemAlert", new
            {
                AlertType = alertType,
                Message = message,
                Severity = severity,
                Timestamp = DateTime.UtcNow
            });
            return true;
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "Error sending system alert: {AlertType}", alertType);
            return false;
        }
    }
}
