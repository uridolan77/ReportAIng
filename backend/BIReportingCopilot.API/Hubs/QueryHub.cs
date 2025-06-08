using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// Enhanced QueryHub with improved async operations and error handling
/// </summary>
public class QueryHub : BaseHub
{
    public QueryHub(ILogger<QueryHub> logger) : base(logger)
    {
    }

    protected override async Task OnUserConnectedAsync(string userId, string connectionId)
    {
        _logger.LogInformation("📡 QueryHub - User {UserId} connected with connection {ConnectionId}", userId, connectionId);
        // Additional QueryHub-specific connection logic can go here
        await Task.CompletedTask;
    }

    protected override async Task OnUserDisconnectedAsync(string userId, string connectionId, Exception? exception)
    {
        _logger.LogInformation("📡 QueryHub - User {UserId} disconnected with connection {ConnectionId}", userId, connectionId);
        // Additional QueryHub-specific disconnection logic can go here
        await Task.CompletedTask;
    }

    /// <summary>
    /// Join a specific query session for real-time updates with enhanced error handling
    /// </summary>
    public async Task JoinQuerySession(string queryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("JoinQuerySession called with empty queryId by connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            var userId = GetCurrentUserId();
            _logger.LogDebug("User {UserId} joining query session {QueryId}", userId, queryId);

            var groupName = $"query_{queryId}";
            var success = await SafeAddToGroupAsync(groupName);

            if (success)
            {
                // Notify the group that a user joined
                var joinNotification = new
                {
                    UserId = userId,
                    QueryId = queryId,
                    Timestamp = DateTime.UtcNow
                };

                await SafeSendAsync(Clients.Group(groupName), "UserJoinedSession", joinNotification);
                await SafeSendAsync(Clients.Caller, "JoinedQuerySession", queryId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error joining query session {QueryId} for connection {ConnectionId}", queryId, Context.ConnectionId);
        }
    }

    /// <summary>
    /// Leave a specific query session with enhanced error handling
    /// </summary>
    public async Task LeaveQuerySession(string queryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("LeaveQuerySession called with empty queryId by connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            var userId = GetCurrentUserId();
            _logger.LogDebug("User {UserId} leaving query session {QueryId}", userId, queryId);

            var groupName = $"query_{queryId}";
            var success = await SafeRemoveFromGroupAsync(groupName);

            if (success)
            {
                // Notify the group that a user left
                var leaveNotification = new
                {
                    UserId = userId,
                    QueryId = queryId,
                    Timestamp = DateTime.UtcNow
                };

                await SafeSendAsync(Clients.Group(groupName), "UserLeftSession", leaveNotification);
                await SafeSendAsync(Clients.Caller, "LeftQuerySession", queryId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error leaving query session {QueryId} for connection {ConnectionId}", queryId, Context.ConnectionId);
        }
    }

    /// <summary>
    /// Request cancellation of a streaming query with enhanced error handling
    /// </summary>
    public async Task RequestQueryCancellation(string queryId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("RequestQueryCancellation called with empty queryId by connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            var userId = GetCurrentUserId();
            _logger.LogInformation("User {UserId} requesting cancellation of query {QueryId}", userId, queryId);

            var cancellationRequest = new
            {
                QueryId = queryId,
                RequestedBy = userId,
                Timestamp = DateTime.UtcNow
            };

            // Broadcast cancellation request to the query session
            await SafeSendAsync(Clients.Group($"query_{queryId}"), "QueryCancellationRequested", cancellationRequest);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting query cancellation for {QueryId} by connection {ConnectionId}", queryId, Context.ConnectionId);
        }
    }

    /// <summary>
    /// Send a message to other users in the same query session with enhanced error handling
    /// </summary>
    public async Task SendMessageToQuerySession(string queryId, string message)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(queryId))
            {
                _logger.LogWarning("SendMessageToQuerySession called with empty queryId by connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                _logger.LogWarning("SendMessageToQuerySession called with empty message by connection {ConnectionId}", Context.ConnectionId);
                return;
            }

            var userId = GetCurrentUserId();
            _logger.LogDebug("User {UserId} sending message to query session {QueryId}", userId, queryId);

            var sessionMessage = new
            {
                QueryId = queryId,
                UserId = userId,
                Message = message.Trim(),
                Timestamp = DateTime.UtcNow
            };

            await SafeSendAsync(Clients.Group($"query_{queryId}"), "SessionMessage", sessionMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to query session {QueryId} by connection {ConnectionId}", queryId, Context.ConnectionId);
        }
    }

}
