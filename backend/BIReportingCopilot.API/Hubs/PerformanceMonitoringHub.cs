using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// SignalR hub for real-time performance monitoring and optimization
/// </summary>
[Authorize]
public class PerformanceMonitoringHub : BaseHub
{
    private readonly IPerformanceOptimizationService _performanceService;
    private readonly IResourceMonitoringService _monitoringService;
    private readonly ICacheOptimizationService _cacheService;

    public PerformanceMonitoringHub(
        ILogger<PerformanceMonitoringHub> logger,
        IPerformanceOptimizationService performanceService,
        IResourceMonitoringService monitoringService,
        ICacheOptimizationService cacheService) : base(logger)
    {
        _performanceService = performanceService;
        _monitoringService = monitoringService;
        _cacheService = cacheService;
    }

    protected override async Task OnUserConnectedAsync(string userId, string connectionId)
    {
        _logger.LogInformation("⚡ PerformanceMonitoringHub - User {UserId} connected with connection {ConnectionId}", userId, connectionId);

        // Add to performance monitoring group
        await Groups.AddToGroupAsync(connectionId, "PerformanceMonitoring");
        
        // Add to user-specific performance group
        await Groups.AddToGroupAsync(connectionId, $"PerformanceUser_{userId}");

        // Send initial performance data
        await SendInitialPerformanceDataAsync(userId);
    }

    protected override async Task OnUserDisconnectedAsync(string userId, string connectionId, Exception? exception)
    {
        _logger.LogInformation("⚡ PerformanceMonitoringHub - User {UserId} disconnected", userId);
        
        // Remove from groups
        await Groups.RemoveFromGroupAsync(connectionId, "PerformanceMonitoring");
        await Groups.RemoveFromGroupAsync(connectionId, $"PerformanceUser_{userId}");
    }

    /// <summary>
    /// Subscribe to real-time performance metrics
    /// </summary>
    public async Task SubscribeToPerformanceMetrics(string entityType = "system", string entityId = "main")
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("⚡ User {UserId} subscribed to performance metrics for {EntityType}:{EntityId}", 
            userId, entityType, entityId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "PerformanceMetrics");
        await Groups.AddToGroupAsync(Context.ConnectionId, $"Entity_{entityType}_{entityId}");
        
        // Send current performance metrics
        await SendPerformanceMetrics(entityType, entityId);
    }

    /// <summary>
    /// Subscribe to performance alerts
    /// </summary>
    public async Task SubscribeToPerformanceAlerts()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("⚡ User {UserId} subscribed to performance alerts", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "PerformanceAlerts");
        
        // Send current active alerts
        await SendActiveAlerts();
    }

    /// <summary>
    /// Subscribe to cache performance updates
    /// </summary>
    public async Task SubscribeToCachePerformance()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("⚡ User {UserId} subscribed to cache performance", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "CachePerformance");
        
        // Send current cache statistics
        await SendCacheStatistics();
    }

    /// <summary>
    /// Start monitoring a specific entity
    /// </summary>
    public async Task StartMonitoring(string entityType, string entityId, int intervalSeconds = 30)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var monitoringId = await _monitoringService.StartMonitoringAsync();
            
            await Clients.Caller.SendAsync("MonitoringStarted", new
            {
                monitoringId = monitoringId,
                entityType = entityType,
                entityId = entityId,
                intervalSeconds = intervalSeconds,
                startedAt = DateTime.UtcNow
            });

            _logger.LogInformation("⚡ Started monitoring {EntityType}:{EntityId} for user {UserId}", 
                entityType, entityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting monitoring for {EntityType}:{EntityId}", entityType, entityId);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to start monitoring" });
        }
    }

    /// <summary>
    /// Stop monitoring a specific entity
    /// </summary>
    public async Task StopMonitoring(string monitoringId)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            await _monitoringService.StopMonitoringAsync(monitoringId);
            
            await Clients.Caller.SendAsync("MonitoringStopped", new
            {
                monitoringId = monitoringId,
                stopped = true,
                stoppedAt = DateTime.UtcNow
            });

            _logger.LogInformation("⚡ Stopped monitoring {MonitoringId} for user {UserId}", monitoringId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping monitoring {MonitoringId}", monitoringId);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to stop monitoring" });
        }
    }

    /// <summary>
    /// Get real-time performance dashboard
    /// </summary>
    public async Task GetRealTimePerformanceDashboard(string entityType = "system", string entityId = "main")
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var dashboardData = await BuildPerformanceDashboardAsync(entityType, entityId);
            await Clients.Caller.SendAsync("RealTimePerformanceDashboard", dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance dashboard for user {UserId}", userId);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to load performance dashboard" });
        }
    }

    /// <summary>
    /// Trigger performance optimization
    /// </summary>
    public async Task TriggerOptimization(string entityType, string entityId)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            // Start optimization in background
            _ = Task.Run(async () =>
            {
                try
                {
                    var result = await _performanceService.AutoTunePerformanceAsync(entityId, entityType);
                    
                    // Send result to user
                    await Clients.Group($"PerformanceUser_{userId}").SendAsync("OptimizationCompleted", new
                    {
                        entityType = entityType,
                        entityId = entityId,
                        result = result,
                        completedAt = DateTime.UtcNow
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during optimization for {EntityType}:{EntityId}", entityType, entityId);
                    await Clients.Group($"PerformanceUser_{userId}").SendAsync("OptimizationFailed", new
                    {
                        entityType = entityType,
                        entityId = entityId,
                        error = ex.Message,
                        failedAt = DateTime.UtcNow
                    });
                }
            });

            await Clients.Caller.SendAsync("OptimizationStarted", new
            {
                entityType = entityType,
                entityId = entityId,
                startedAt = DateTime.UtcNow
            });

            _logger.LogInformation("⚡ Triggered optimization for {EntityType}:{EntityId} by user {UserId}", 
                entityType, entityId, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error triggering optimization for {EntityType}:{EntityId}", entityType, entityId);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to trigger optimization" });
        }
    }

    /// <summary>
    /// Record a performance metric in real-time
    /// </summary>
    public async Task RecordMetric(string metricName, double value, string? entityType = null, string? entityId = null)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var metric = new PerformanceMetricsEntry
            {
                MetricName = metricName,
                Value = value,
                EntityType = entityType ?? "user",
                EntityId = entityId ?? userId,
                Timestamp = DateTime.UtcNow,
                UserId = userId
            };

            await _performanceService.RecordPerformanceMetricAsync(metric);

            // Broadcast to performance monitoring group
            await Clients.Group("PerformanceMonitoring").SendAsync("MetricRecorded", metric);
            
            _logger.LogInformation("⚡ Metric recorded: {MetricName} = {Value} for user {UserId}", 
                metricName, value, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording metric {MetricName} for user {UserId}", metricName, userId);
        }
    }

    #region Private Helper Methods

    private async Task SendInitialPerformanceDataAsync(string userId)
    {
        try
        {
            var metrics = await _performanceService.AnalyzePerformanceAsync("system", "main");
            var alerts = await _performanceService.GetActivePerformanceAlertsAsync();
            var cacheStats = await _cacheService.GetAllCacheStatisticsAsync();
            
            await Clients.Caller.SendAsync("InitialPerformanceData", new
            {
                metrics = metrics,
                alerts = alerts.Take(5),
                cacheStats = cacheStats,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending initial performance data to user {UserId}", userId);
        }
    }

    private async Task SendPerformanceMetrics(string entityType, string entityId)
    {
        try
        {
            var metrics = await _performanceService.AnalyzePerformanceAsync(entityId, entityType);
            await Clients.Caller.SendAsync("PerformanceMetrics", new
            {
                entityType = entityType,
                entityId = entityId,
                metrics = metrics,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending performance metrics for {EntityType}:{EntityId}", entityType, entityId);
        }
    }

    private async Task SendActiveAlerts()
    {
        try
        {
            var alerts = await _performanceService.GetActivePerformanceAlertsAsync();
            await Clients.Caller.SendAsync("ActivePerformanceAlerts", alerts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending active performance alerts");
        }
    }

    private async Task SendCacheStatistics()
    {
        try
        {
            var cacheStats = await _cacheService.GetAllCacheStatisticsAsync();
            await Clients.Caller.SendAsync("CacheStatistics", cacheStats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending cache statistics");
        }
    }

    private async Task<object> BuildPerformanceDashboardAsync(string entityType, string entityId)
    {
        var metrics = await _performanceService.AnalyzePerformanceAsync(entityId, entityType);
        var bottlenecks = await _performanceService.IdentifyBottlenecksAsync(entityId, entityType);
        var suggestions = await _performanceService.GetOptimizationSuggestionsAsync(entityId, entityType);
        var alerts = await _performanceService.GetActivePerformanceAlertsAsync();
        var cacheStats = await _cacheService.GetAllCacheStatisticsAsync();

        return new
        {
            entityType = entityType,
            entityId = entityId,
            metrics = metrics,
            bottlenecks = bottlenecks.Take(5),
            suggestions = suggestions.Take(5),
            alerts = alerts.Take(10),
            cacheStats = cacheStats,
            timestamp = DateTime.UtcNow
        };
    }

    #endregion
}
