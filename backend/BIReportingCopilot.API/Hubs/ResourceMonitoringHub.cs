using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// SignalR hub for real-time resource monitoring and management
/// </summary>
[Authorize]
public class ResourceMonitoringHub : BaseHub
{
    private readonly IResourceManagementService _resourceService;
    private readonly IResourceMonitoringService _monitoringService;

    public ResourceMonitoringHub(
        ILogger<ResourceMonitoringHub> logger,
        IResourceManagementService resourceService,
        IResourceMonitoringService monitoringService) : base(logger)
    {
        _resourceService = resourceService;
        _monitoringService = monitoringService;
    }

    protected override async Task OnUserConnectedAsync(string userId, string connectionId)
    {
        _logger.LogInformation("🔧 ResourceMonitoringHub - User {UserId} connected with connection {ConnectionId}", userId, connectionId);

        // Add to resource monitoring group
        await Groups.AddToGroupAsync(connectionId, "ResourceMonitoring");
        
        // Add to user-specific resource group
        await Groups.AddToGroupAsync(connectionId, $"ResourceUser_{userId}");

        // Send initial resource data
        await SendInitialResourceDataAsync(userId);
    }

    protected override async Task OnUserDisconnectedAsync(string userId, string connectionId, Exception? exception)
    {
        _logger.LogInformation("🔧 ResourceMonitoringHub - User {UserId} disconnected", userId);
        
        // Remove from groups
        await Groups.RemoveFromGroupAsync(connectionId, "ResourceMonitoring");
        await Groups.RemoveFromGroupAsync(connectionId, $"ResourceUser_{userId}");
    }

    /// <summary>
    /// Subscribe to resource quota updates
    /// </summary>
    public async Task SubscribeToQuotaUpdates()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("🔧 User {UserId} subscribed to quota updates", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "QuotaUpdates");
        
        // Send current quota status
        await SendQuotaStatus(userId);
    }

    /// <summary>
    /// Subscribe to resource usage monitoring
    /// </summary>
    public async Task SubscribeToUsageMonitoring()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("🔧 User {UserId} subscribed to usage monitoring", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "UsageMonitoring");
        
        // Send current usage data
        await SendUsageData(userId);
    }

    /// <summary>
    /// Subscribe to circuit breaker status updates
    /// </summary>
    public async Task SubscribeToCircuitBreakerUpdates()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("🔧 User {UserId} subscribed to circuit breaker updates", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "CircuitBreakerUpdates");
        
        // Send current circuit breaker states
        await SendCircuitBreakerStates();
    }

    /// <summary>
    /// Subscribe to load balancing updates
    /// </summary>
    public async Task SubscribeToLoadBalancing()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("🔧 User {UserId} subscribed to load balancing updates", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "LoadBalancing");
        
        // Send current load statistics
        await SendLoadStatistics();
    }

    /// <summary>
    /// Get real-time resource dashboard
    /// </summary>
    public async Task GetRealTimeResourceDashboard()
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var dashboardData = await BuildResourceDashboardAsync(userId);
            await Clients.Caller.SendAsync("RealTimeResourceDashboard", dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting resource dashboard for user {UserId}", userId);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to load resource dashboard" });
        }
    }

    /// <summary>
    /// Monitor resource usage for a specific type
    /// </summary>
    public async Task MonitorResourceType(string resourceType, int intervalSeconds = 30)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var monitoringId = await _monitoringService.StartResourceMonitoringAsync(resourceType, TimeSpan.FromSeconds(intervalSeconds));
            
            await Clients.Caller.SendAsync("ResourceMonitoringStarted", new
            {
                monitoringId = monitoringId,
                resourceType = resourceType,
                intervalSeconds = intervalSeconds,
                startedAt = DateTime.UtcNow
            });

            _logger.LogInformation("🔧 Started resource monitoring for {ResourceType} by user {UserId}", 
                resourceType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting resource monitoring for {ResourceType}", resourceType);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to start resource monitoring" });
        }
    }

    /// <summary>
    /// Stop resource monitoring
    /// </summary>
    public async Task StopResourceMonitoring(string monitoringId)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var stopped = await _monitoringService.StopResourceMonitoringAsync(monitoringId);
            
            await Clients.Caller.SendAsync("ResourceMonitoringStopped", new
            {
                monitoringId = monitoringId,
                stopped = stopped,
                stoppedAt = DateTime.UtcNow
            });

            _logger.LogInformation("🔧 Stopped resource monitoring {MonitoringId} for user {UserId}", monitoringId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping resource monitoring {MonitoringId}", monitoringId);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to stop resource monitoring" });
        }
    }

    /// <summary>
    /// Request quota increase
    /// </summary>
    public async Task RequestQuotaIncrease(string resourceType, int requestedIncrease, string justification)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var request = new
            {
                UserId = userId,
                ResourceType = resourceType,
                RequestedIncrease = requestedIncrease,
                Justification = justification,
                RequestedAt = DateTime.UtcNow,
                Status = "Pending"
            };

            // Broadcast to admin group for approval
            await Clients.Group("AdminUsers").SendAsync("QuotaIncreaseRequest", request);
            
            // Confirm to user
            await Clients.Caller.SendAsync("QuotaIncreaseRequested", request);

            _logger.LogInformation("🔧 Quota increase requested for {ResourceType} by user {UserId}: +{RequestedIncrease}", 
                resourceType, userId, requestedIncrease);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error requesting quota increase for {ResourceType}", resourceType);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to request quota increase" });
        }
    }

    /// <summary>
    /// Check resource availability
    /// </summary>
    public async Task CheckResourceAvailability(string resourceType)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var optimalResource = await _resourceService.SelectOptimalResourceAsync(resourceType);
            var loadStats = await _resourceService.GetResourceLoadStatsAsync(resourceType);
            
            await Clients.Caller.SendAsync("ResourceAvailability", new
            {
                resourceType = resourceType,
                optimalResource = optimalResource,
                loadStats = loadStats,
                checkedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking resource availability for {ResourceType}", resourceType);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to check resource availability" });
        }
    }

    /// <summary>
    /// Update resource load in real-time
    /// </summary>
    public async Task UpdateResourceLoad(string resourceId, double loadPercentage)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            await _resourceService.UpdateResourceLoadAsync(resourceId, loadPercentage);

            var loadUpdate = new
            {
                ResourceId = resourceId,
                LoadPercentage = loadPercentage,
                UpdatedBy = userId,
                UpdatedAt = DateTime.UtcNow
            };

            // Broadcast to load balancing group
            await Clients.Group("LoadBalancing").SendAsync("ResourceLoadUpdated", loadUpdate);
            
            _logger.LogInformation("🔧 Resource load updated: {ResourceId} = {LoadPercentage:P} by user {UserId}", 
                resourceId, loadPercentage, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating resource load for {ResourceId}", resourceId);
        }
    }

    #region Private Helper Methods

    private async Task SendInitialResourceDataAsync(string userId)
    {
        try
        {
            var quotas = await _resourceService.GetUserResourceQuotasAsync(userId);
            var usage = await _resourceService.GetCurrentResourceUsageAsync(userId);
            var priority = await _resourceService.GetUserPriorityAsync(userId);
            
            await Clients.Caller.SendAsync("InitialResourceData", new
            {
                quotas = quotas,
                usage = usage,
                priority = priority,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending initial resource data to user {UserId}", userId);
        }
    }

    private async Task SendQuotaStatus(string userId)
    {
        try
        {
            var quotas = await _resourceService.GetUserResourceQuotasAsync(userId);
            await Clients.Caller.SendAsync("QuotaStatus", quotas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending quota status to user {UserId}", userId);
        }
    }

    private async Task SendUsageData(string userId)
    {
        try
        {
            var usage = await _resourceService.GetCurrentResourceUsageAsync(userId);
            await Clients.Caller.SendAsync("UsageData", usage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending usage data to user {UserId}", userId);
        }
    }

    private async Task SendCircuitBreakerStates()
    {
        try
        {
            var services = new[] { "ai-service", "database", "cache", "external-api" };
            var states = new Dictionary<string, object>();
            
            foreach (var service in services)
            {
                var state = await _resourceService.GetCircuitBreakerStateAsync(service);
                states[service] = state;
            }
            
            await Clients.Caller.SendAsync("CircuitBreakerStates", states);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending circuit breaker states");
        }
    }

    private async Task SendLoadStatistics()
    {
        try
        {
            var resourceTypes = new[] { "ai-service", "database", "cache" };
            var loadStats = new Dictionary<string, object>();
            
            foreach (var resourceType in resourceTypes)
            {
                var stats = await _resourceService.GetResourceLoadStatsAsync(resourceType);
                loadStats[resourceType] = stats;
            }
            
            await Clients.Caller.SendAsync("LoadStatistics", loadStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending load statistics");
        }
    }

    private async Task<object> BuildResourceDashboardAsync(string userId)
    {
        var quotas = await _resourceService.GetUserResourceQuotasAsync(userId);
        var usage = await _resourceService.GetCurrentResourceUsageAsync(userId);
        var priority = await _resourceService.GetUserPriorityAsync(userId);
        
        // Get circuit breaker states
        var services = new[] { "ai-service", "database", "cache", "external-api" };
        var circuitBreakerStates = new Dictionary<string, object>();
        foreach (var service in services)
        {
            circuitBreakerStates[service] = await _resourceService.GetCircuitBreakerStateAsync(service);
        }

        // Get load statistics
        var resourceTypes = new[] { "ai-service", "database", "cache" };
        var loadStats = new Dictionary<string, object>();
        foreach (var resourceType in resourceTypes)
        {
            loadStats[resourceType] = await _resourceService.GetResourceLoadStatsAsync(resourceType);
        }

        return new
        {
            quotas = quotas,
            usage = usage,
            priority = priority,
            circuitBreakerStates = circuitBreakerStates,
            loadStats = loadStats,
            timestamp = DateTime.UtcNow
        };
    }

    #endregion
}
