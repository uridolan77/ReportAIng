using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Models.Analytics;
using System.Collections.Concurrent;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// SignalR hub for real-time template analytics updates
/// </summary>
[Authorize]
public class TemplateAnalyticsHub : Hub
{
    private readonly ITemplatePerformanceService _performanceService;
    private readonly IABTestingService _abTestingService;
    private readonly ILogger<TemplateAnalyticsHub> _logger;
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();

    public TemplateAnalyticsHub(
        ITemplatePerformanceService performanceService,
        IABTestingService abTestingService,
        ILogger<TemplateAnalyticsHub> logger)
    {
        _performanceService = performanceService;
        _abTestingService = abTestingService;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier ?? Context.ConnectionId;
        _userConnections.TryAdd(Context.ConnectionId, userId);
        
        _logger.LogInformation("User {UserId} connected to analytics hub", userId);
        
        // Send initial dashboard data
        await SendInitialDashboardData();
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _userConnections.TryRemove(Context.ConnectionId, out var userId);
        _logger.LogInformation("User {UserId} disconnected from analytics hub", userId);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Subscribe to real-time performance updates
    /// </summary>
    public async Task SubscribeToPerformanceUpdates(string? intentType = null)
    {
        var groupName = string.IsNullOrEmpty(intentType) ? "AllPerformance" : $"Performance_{intentType}";
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        _logger.LogDebug("User {UserId} subscribed to performance updates for {IntentType}", 
            Context.UserIdentifier, intentType ?? "all");
    }

    /// <summary>
    /// Subscribe to A/B test updates
    /// </summary>
    public async Task SubscribeToABTestUpdates()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "ABTestUpdates");
        
        _logger.LogDebug("User {UserId} subscribed to A/B test updates", Context.UserIdentifier);
    }

    /// <summary>
    /// Subscribe to template alerts
    /// </summary>
    public async Task SubscribeToAlerts()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, "Alerts");
        
        _logger.LogDebug("User {UserId} subscribed to alerts", Context.UserIdentifier);
    }

    /// <summary>
    /// Get real-time dashboard data
    /// </summary>
    public async Task GetRealTimeDashboard()
    {
        try
        {
            var dashboardData = await BuildRealTimeDashboardData();
            await Clients.Caller.SendAsync("DashboardUpdate", dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time dashboard data");
            await Clients.Caller.SendAsync("Error", "Failed to retrieve dashboard data");
        }
    }

    /// <summary>
    /// Get template performance in real-time
    /// </summary>
    public async Task GetTemplatePerformance(string templateKey)
    {
        try
        {
            var performance = await _performanceService.GetTemplatePerformanceAsync(templateKey);
            await Clients.Caller.SendAsync("TemplatePerformanceUpdate", templateKey, performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending template performance for {TemplateKey}", templateKey);
            await Clients.Caller.SendAsync("Error", $"Failed to retrieve performance for {templateKey}");
        }
    }

    private async Task SendInitialDashboardData()
    {
        try
        {
            var dashboardData = await BuildRealTimeDashboardData();
            await Clients.Caller.SendAsync("InitialDashboard", dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending initial dashboard data");
        }
    }

    private async Task<object> BuildRealTimeDashboardData()
    {
        var performanceDashboard = await _performanceService.GetDashboardDataAsync();
        var abTestDashboard = await _abTestingService.GetTestDashboardAsync();
        var alerts = await _performanceService.GetPerformanceAlertsAsync();
        var recentTemplates = await _performanceService.GetRecentlyUsedTemplatesAsync(days: 1);

        return new
        {
            Performance = performanceDashboard,
            ABTesting = abTestDashboard,
            Alerts = alerts.Take(5),
            RecentActivity = recentTemplates.Take(10),
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Broadcast performance update to subscribers
    /// </summary>
    public static async Task BroadcastPerformanceUpdate(IHubContext<TemplateAnalyticsHub> hubContext, 
        string templateKey, object performanceData, string? intentType = null)
    {
        var groupName = string.IsNullOrEmpty(intentType) ? "AllPerformance" : $"Performance_{intentType}";
        await hubContext.Clients.Group(groupName).SendAsync("PerformanceUpdate", templateKey, performanceData);
    }

    /// <summary>
    /// Broadcast A/B test update to subscribers
    /// </summary>
    public static async Task BroadcastABTestUpdate(IHubContext<TemplateAnalyticsHub> hubContext, 
        long testId, object testData)
    {
        await hubContext.Clients.Group("ABTestUpdates").SendAsync("ABTestUpdate", testId, testData);
    }

    /// <summary>
    /// Broadcast alert to subscribers
    /// </summary>
    public static async Task BroadcastAlert(IHubContext<TemplateAnalyticsHub> hubContext, 
        PerformanceAlert alert)
    {
        await hubContext.Clients.Group("Alerts").SendAsync("NewAlert", alert);
    }

    /// <summary>
    /// Broadcast dashboard update to all connected clients
    /// </summary>
    public static async Task BroadcastDashboardUpdate(IHubContext<TemplateAnalyticsHub> hubContext, 
        object dashboardData)
    {
        await hubContext.Clients.All.SendAsync("DashboardUpdate", dashboardData);
    }
}

/// <summary>
/// Background service for periodic analytics updates
/// </summary>
public class AnalyticsUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<TemplateAnalyticsHub> _hubContext;
    private readonly ILogger<AnalyticsUpdateService> _logger;
    private readonly TimeSpan _updateInterval = TimeSpan.FromMinutes(5);

    public AnalyticsUpdateService(
        IServiceProvider serviceProvider,
        IHubContext<TemplateAnalyticsHub> hubContext,
        ILogger<AnalyticsUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Analytics Update Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendPeriodicUpdates();
                await Task.Delay(_updateInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in analytics update service");
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }

    private async Task SendPeriodicUpdates()
    {
        using var scope = _serviceProvider.CreateScope();
        var performanceService = scope.ServiceProvider.GetRequiredService<ITemplatePerformanceService>();
        var abTestingService = scope.ServiceProvider.GetRequiredService<IABTestingService>();

        try
        {
            // Get updated dashboard data
            var performanceDashboard = await performanceService.GetDashboardDataAsync();
            var abTestDashboard = await abTestingService.GetTestDashboardAsync();
            var alerts = await performanceService.GetPerformanceAlertsAsync();

            var dashboardUpdate = new
            {
                Performance = performanceDashboard,
                ABTesting = abTestDashboard,
                Alerts = alerts.Take(5),
                LastUpdated = DateTime.UtcNow
            };

            await TemplateAnalyticsHub.BroadcastDashboardUpdate(_hubContext, dashboardUpdate);
            
            _logger.LogDebug("Sent periodic analytics update");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending periodic analytics updates");
        }
    }
}
