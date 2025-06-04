using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BIReportingCopilot.API.Hubs;

[Authorize]
public class QueryHub : Hub
{
    private readonly ILogger<QueryHub> _logger;

    public QueryHub(ILogger<QueryHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} connected to QueryHub with connection {ConnectionId}", userId, Context.ConnectionId);
        _logger.LogInformation("📡 Adding user to group: user_{UserId}", userId);

        // Add user to their personal group for targeted messaging
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} disconnected from QueryHub with connection {ConnectionId}", userId, Context.ConnectionId);

        // Remove user from their personal group
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific query session for real-time updates
    /// </summary>
    public async Task JoinQuerySession(string queryId)
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("User {UserId} joining query session {QueryId}", userId, queryId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"query_{queryId}");

        // Notify the group that a user joined
        await Clients.Group($"query_{queryId}").SendAsync("UserJoinedSession", new
        {
            UserId = userId,
            QueryId = queryId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Leave a specific query session
    /// </summary>
    public async Task LeaveQuerySession(string queryId)
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("User {UserId} leaving query session {QueryId}", userId, queryId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"query_{queryId}");

        // Notify the group that a user left
        await Clients.Group($"query_{queryId}").SendAsync("UserLeftSession", new
        {
            UserId = userId,
            QueryId = queryId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Request cancellation of a streaming query
    /// </summary>
    public async Task RequestQueryCancellation(string queryId)
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("User {UserId} requesting cancellation of query {QueryId}", userId, queryId);

        // Broadcast cancellation request to the query session
        await Clients.Group($"query_{queryId}").SendAsync("QueryCancellationRequested", new
        {
            QueryId = queryId,
            RequestedBy = userId,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Send a message to other users in the same query session
    /// </summary>
    public async Task SendMessageToQuerySession(string queryId, string message)
    {
        var userId = GetCurrentUserId();
        _logger.LogDebug("User {UserId} sending message to query session {QueryId}", userId, queryId);

        await Clients.Group($"query_{queryId}").SendAsync("SessionMessage", new
        {
            QueryId = queryId,
            UserId = userId,
            Message = message,
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get connection info for debugging
    /// </summary>
    public async Task GetConnectionInfo()
    {
        var userId = GetCurrentUserId();

        await Clients.Caller.SendAsync("ConnectionInfo", new
        {
            ConnectionId = Context.ConnectionId,
            UserId = userId,
            UserAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString(),
            RemoteIpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString(),
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get current user ID using the same logic as controllers
    /// </summary>
    private string GetCurrentUserId()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var subClaim = Context.User?.FindFirst("sub")?.Value;
        var nameClaim = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var emailClaim = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

        var result = userId ?? subClaim ?? nameClaim ?? emailClaim ?? "anonymous";

        _logger.LogInformation("🔍 QueryHub GetCurrentUserId - NameIdentifier: {UserId}, Sub: {SubClaim}, Name: {NameClaim}, Email: {Email}, Result: {Result}",
            userId, subClaim, nameClaim, emailClaim, result);

        return result;
    }
}
