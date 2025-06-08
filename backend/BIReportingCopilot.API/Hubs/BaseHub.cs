using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// Base hub class with common functionality and improved async handling
/// </summary>
[Authorize]
public abstract class BaseHub : Hub
{
    protected readonly ILogger _logger;

    protected BaseHub(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Enhanced OnConnectedAsync with proper error handling
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        try
        {
            var userId = GetCurrentUserId();
            var connectionId = Context.ConnectionId;
            
            _logger.LogInformation("User {UserId} connected to {HubName} with connection {ConnectionId}", 
                userId, GetType().Name, connectionId);

            // Add user to their personal group for targeted messaging
            await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
            
            _logger.LogDebug("Added user {UserId} to personal group user_{UserId}", userId, userId);

            // Call derived class implementation
            await OnUserConnectedAsync(userId, connectionId);

            await base.OnConnectedAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnConnectedAsync for connection {ConnectionId}", Context.ConnectionId);
            // Don't rethrow - let the connection continue
        }
    }

    /// <summary>
    /// Enhanced OnDisconnectedAsync with proper error handling
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var userId = GetCurrentUserId();
            var connectionId = Context.ConnectionId;
            
            _logger.LogInformation("User {UserId} disconnected from {HubName} with connection {ConnectionId}", 
                userId, GetType().Name, connectionId);

            if (exception != null)
            {
                _logger.LogError(exception, "User {UserId} disconnected with error from {HubName}", 
                    userId, GetType().Name);
            }

            // Remove user from their personal group
            await Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");

            // Call derived class implementation
            await OnUserDisconnectedAsync(userId, connectionId, exception);

            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync for connection {ConnectionId}", Context.ConnectionId);
            // Don't rethrow - let the disconnection continue
        }
    }

    /// <summary>
    /// Centralized user ID retrieval logic with comprehensive claim checking
    /// </summary>
    protected string GetCurrentUserId()
    {
        try
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var subClaim = Context.User?.FindFirst("sub")?.Value;
            var nameClaim = Context.User?.FindFirst(ClaimTypes.Name)?.Value;
            var emailClaim = Context.User?.FindFirst(ClaimTypes.Email)?.Value;

            var result = userId ?? subClaim ?? nameClaim ?? emailClaim ?? "anonymous";

            _logger.LogDebug("GetCurrentUserId - NameIdentifier: {UserId}, Sub: {SubClaim}, Name: {NameClaim}, Email: {Email}, Result: {Result}",
                userId, subClaim, nameClaim, emailClaim, result);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current user ID for connection {ConnectionId}", Context.ConnectionId);
            return "anonymous";
        }
    }

    /// <summary>
    /// Safe group operation with error handling
    /// </summary>
    protected async Task<bool> SafeAddToGroupAsync(string groupName, string? connectionId = null)
    {
        try
        {
            connectionId ??= Context.ConnectionId;
            await Groups.AddToGroupAsync(connectionId, groupName);
            _logger.LogDebug("Added connection {ConnectionId} to group {GroupName}", connectionId, groupName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding connection {ConnectionId} to group {GroupName}", 
                connectionId ?? Context.ConnectionId, groupName);
            return false;
        }
    }

    /// <summary>
    /// Safe group removal with error handling
    /// </summary>
    protected async Task<bool> SafeRemoveFromGroupAsync(string groupName, string? connectionId = null)
    {
        try
        {
            connectionId ??= Context.ConnectionId;
            await Groups.RemoveFromGroupAsync(connectionId, groupName);
            _logger.LogDebug("Removed connection {ConnectionId} from group {GroupName}", connectionId, groupName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing connection {ConnectionId} from group {GroupName}", 
                connectionId ?? Context.ConnectionId, groupName);
            return false;
        }
    }

    /// <summary>
    /// Safe client notification with error handling
    /// </summary>
    protected async Task<bool> SafeSendAsync(IClientProxy client, string method, object? arg = null)
    {
        try
        {
            if (arg != null)
            {
                await client.SendAsync(method, arg);
            }
            else
            {
                await client.SendAsync(method);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending {Method} to client", method);
            return false;
        }
    }

    /// <summary>
    /// Get connection information for debugging
    /// </summary>
    public async Task GetConnectionInfo()
    {
        try
        {
            var userId = GetCurrentUserId();
            var connectionInfo = new
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                HubName = GetType().Name,
                UserAgent = Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString(),
                RemoteIpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString(),
                IsAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false,
                Timestamp = DateTime.UtcNow,
                UserGroups = new[] { $"user_{userId}" }
            };

            await SafeSendAsync(Clients.Caller, "ConnectionInfo", connectionInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting connection info for {ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// Test connection method for debugging
    /// </summary>
    public async Task TestConnection()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("TestConnection called by user {UserId} with connection {ConnectionId}", 
                userId, Context.ConnectionId);

            var testResponse = new
            {
                ConnectionId = Context.ConnectionId,
                UserId = userId,
                HubName = GetType().Name,
                Message = "Connection test successful",
                Timestamp = DateTime.UtcNow,
                IsAuthenticated = Context.User?.Identity?.IsAuthenticated ?? false,
                UserGroups = new[] { $"user_{userId}" }
            };

            await SafeSendAsync(Clients.Caller, "TestConnectionResponse", testResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TestConnection for {ConnectionId}", Context.ConnectionId);
        }
    }

    /// <summary>
    /// Virtual method for derived classes to handle user connection
    /// </summary>
    protected virtual Task OnUserConnectedAsync(string userId, string connectionId)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Virtual method for derived classes to handle user disconnection
    /// </summary>
    protected virtual Task OnUserDisconnectedAsync(string userId, string connectionId, Exception? exception)
    {
        return Task.CompletedTask;
    }
}
