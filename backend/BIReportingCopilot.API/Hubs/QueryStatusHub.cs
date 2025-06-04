using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.API.Hubs;

// [Authorize] // Temporarily disabled for debugging
public class QueryStatusHub : Hub, IProgressHub
{
    private readonly ILogger<QueryStatusHub> _logger;

    public QueryStatusHub(ILogger<QueryStatusHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var subClaim = Context.User?.FindFirst("sub")?.Value;
        var nameClaim = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
        var emailClaim = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

        _logger.LogInformation("🔗 User connected to QueryStatusHub - ConnectionId: {ConnectionId}, NameIdentifier: {UserId}, Sub: {SubClaim}, Name: {NameClaim}, Email: {Email}",
            Context.ConnectionId, userId, subClaim, nameClaim, emailClaim);

        // Log all claims for debugging
        if (Context.User?.Claims != null)
        {
            foreach (var claim in Context.User.Claims)
            {
                _logger.LogInformation("🔗 Claim: {Type} = {Value}", claim.Type, claim.Value);
            }
        }

        // Add user to their personal group for targeted notifications
        // Try different user ID formats
        var userIdToUse = userId ?? subClaim ?? nameClaim ?? emailClaim;
        if (!string.IsNullOrEmpty(userIdToUse))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userIdToUse}");
            _logger.LogInformation("🔗 Added user {UserId} to group user_{UserId}", userIdToUse, userIdToUse);
        }
        else
        {
            _logger.LogWarning("🔗 No user ID found in claims for connection {ConnectionId}", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogInformation("User {UserId} disconnected from QueryStatusHub with connection {ConnectionId}",
            userId, Context.ConnectionId);

        if (exception != null)
        {
            _logger.LogError(exception, "User {UserId} disconnected with error", userId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    // Client can join a specific query group to receive updates for that query
    public async Task JoinQueryGroup(string queryId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogDebug("User {UserId} joining query group {QueryId}", userId, queryId);

        await Groups.AddToGroupAsync(Context.ConnectionId, $"query_{queryId}");

        // Notify the client they've joined the group
        await Clients.Caller.SendAsync("JoinedQueryGroup", queryId);
    }

    // Client can leave a query group
    public async Task LeaveQueryGroup(string queryId)
    {
        var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _logger.LogDebug("User {UserId} leaving query group {QueryId}", userId, queryId);

        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"query_{queryId}");

        // Notify the client they've left the group
        await Clients.Caller.SendAsync("LeftQueryGroup", queryId);
    }

    // Send query status update to specific query group
    public async Task SendQueryStatusUpdate(string queryId, object statusUpdate)
    {
        await Clients.Group($"query_{queryId}").SendAsync("QueryStatusUpdate", queryId, statusUpdate);
    }

    // Send notification to specific user
    public async Task SendUserNotification(string userId, object notification)
    {
        await Clients.Group($"user_{userId}").SendAsync("UserNotification", notification);
    }

    // Broadcast system-wide notification
    public async Task SendSystemNotification(object notification)
    {
        await Clients.All.SendAsync("SystemNotification", notification);
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

        _logger.LogInformation("🔍 QueryStatusHub GetCurrentUserId - NameIdentifier: {UserId}, Sub: {SubClaim}, Name: {NameClaim}, Email: {Email}, Result: {Result}",
            userId, subClaim, nameClaim, emailClaim, result);

        return result;
    }

    /// <summary>
    /// Implementation of IProgressHub interface
    /// </summary>
    public async Task SendAutoGenerationProgressAsync(string userId, object progressData)
    {
        await Clients.Group($"user_{userId}").SendAsync("AutoGenerationProgress", progressData);
    }
}

// Extension methods for easier hub usage from services
public static class QueryStatusHubExtensions
{
    public static async Task NotifyQueryStarted(this IHubContext<QueryStatusHub> hubContext, string queryId, string userId)
    {
        await hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryStarted", new
        {
            QueryId = queryId,
            UserId = userId,
            Timestamp = DateTime.UtcNow,
            Status = "started"
        });
    }

    public static async Task NotifyQueryProgress(this IHubContext<QueryStatusHub> hubContext, string queryId, double progress, string? message = null)
    {
        await hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryProgress", new
        {
            QueryId = queryId,
            Progress = progress,
            Message = message,
            Timestamp = DateTime.UtcNow,
            Status = "in_progress"
        });
    }

    public static async Task NotifyQueryCompleted(this IHubContext<QueryStatusHub> hubContext, string queryId, bool success, int executionTimeMs, string? error = null)
    {
        await hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryCompleted", new
        {
            QueryId = queryId,
            Success = success,
            ExecutionTimeMs = executionTimeMs,
            Error = error,
            Timestamp = DateTime.UtcNow,
            Status = success ? "completed" : "failed"
        });
    }

    public static async Task NotifyUserActivity(this IHubContext<QueryStatusHub> hubContext, string userId, string activity, object? details = null)
    {
        await hubContext.Clients.Group($"user_{userId}").SendAsync("UserActivity", new
        {
            UserId = userId,
            Activity = activity,
            Details = details,
            Timestamp = DateTime.UtcNow
        });
    }

    public static async Task NotifySystemAlert(this IHubContext<QueryStatusHub> hubContext, string alertType, string message, string severity = "info")
    {
        await hubContext.Clients.All.SendAsync("SystemAlert", new
        {
            AlertType = alertType,
            Message = message,
            Severity = severity,
            Timestamp = DateTime.UtcNow
        });
    }
}
