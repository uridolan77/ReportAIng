using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// SignalR hub for real-time cost monitoring and alerts
/// </summary>
[Authorize]
public class CostMonitoringHub : BaseHub
{
    private readonly ICostManagementService _costService;
    private readonly ICostAnalyticsService _analyticsService;
    private readonly IResourceManagementService _resourceService;

    public CostMonitoringHub(
        ILogger<CostMonitoringHub> logger,
        ICostManagementService costService,
        ICostAnalyticsService analyticsService,
        IResourceManagementService resourceService) : base(logger)
    {
        _costService = costService;
        _analyticsService = analyticsService;
        _resourceService = resourceService;
    }

    protected override async Task OnUserConnectedAsync(string userId, string connectionId)
    {
        _logger.LogInformation("💰 CostMonitoringHub - User {UserId} connected with connection {ConnectionId}", userId, connectionId);

        // Add to cost monitoring group
        await Groups.AddToGroupAsync(connectionId, "CostMonitoring");
        
        // Add to user-specific cost group
        await Groups.AddToGroupAsync(connectionId, $"CostUser_{userId}");

        // Send initial cost data
        await SendInitialCostDataAsync(userId);
    }

    protected override async Task OnUserDisconnectedAsync(string userId, string connectionId, Exception? exception)
    {
        _logger.LogInformation("💰 CostMonitoringHub - User {UserId} disconnected", userId);
        
        // Remove from groups
        await Groups.RemoveFromGroupAsync(connectionId, "CostMonitoring");
        await Groups.RemoveFromGroupAsync(connectionId, $"CostUser_{userId}");
    }

    /// <summary>
    /// Subscribe to real-time cost updates
    /// </summary>
    public async Task SubscribeToCostUpdates()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("💰 User {UserId} subscribed to cost updates", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "CostUpdates");
        
        // Send current cost metrics
        await SendRealTimeCostMetrics(userId);
    }

    /// <summary>
    /// Subscribe to budget alerts
    /// </summary>
    public async Task SubscribeToBudgetAlerts()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("💰 User {UserId} subscribed to budget alerts", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "BudgetAlerts");
        
        // Send current budget status
        await SendBudgetStatus(userId);
    }

    /// <summary>
    /// Subscribe to cost optimization recommendations
    /// </summary>
    public async Task SubscribeToOptimizationRecommendations()
    {
        var userId = GetCurrentUserId();
        _logger.LogInformation("💰 User {UserId} subscribed to optimization recommendations", userId);

        await Groups.AddToGroupAsync(Context.ConnectionId, "OptimizationRecommendations");
        
        // Send current recommendations
        await SendOptimizationRecommendations(userId);
    }

    /// <summary>
    /// Get real-time cost dashboard data
    /// </summary>
    public async Task GetRealTimeDashboard()
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var dashboardData = await BuildRealTimeDashboardAsync(userId);
            await Clients.Caller.SendAsync("RealTimeDashboard", dashboardData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting real-time dashboard for user {UserId}", userId);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to load dashboard data" });
        }
    }

    /// <summary>
    /// Request cost forecast update
    /// </summary>
    public async Task RequestCostForecast(int days = 30)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var forecast = await _analyticsService.ForecastCostsAsync(userId, days);
            await Clients.Caller.SendAsync("CostForecast", new
            {
                forecast = forecast,
                forecastDays = days,
                generatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating cost forecast for user {UserId}", userId);
            await Clients.Caller.SendAsync("Error", new { message = "Failed to generate cost forecast" });
        }
    }

    /// <summary>
    /// Track cost event in real-time
    /// </summary>
    public async Task TrackCostEvent(string eventType, decimal amount, string? description = null)
    {
        var userId = GetCurrentUserId();
        
        try
        {
            var costEvent = new
            {
                EventType = eventType,
                Amount = amount,
                Description = description,
                UserId = userId,
                Timestamp = DateTime.UtcNow
            };

            // Broadcast to cost monitoring group
            await Clients.Group("CostMonitoring").SendAsync("CostEvent", costEvent);
            
            // Send to user's group
            await Clients.Group($"CostUser_{userId}").SendAsync("UserCostEvent", costEvent);
            
            _logger.LogInformation("💰 Cost event tracked: {EventType} - ${Amount} for user {UserId}", 
                eventType, amount, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error tracking cost event for user {UserId}", userId);
        }
    }

    #region Private Helper Methods

    private async Task SendInitialCostDataAsync(string userId)
    {
        try
        {
            var analytics = await _analyticsService.GenerateCostAnalyticsAsync(userId);
            var realTimeMetrics = await _analyticsService.GetRealTimeCostMetricsAsync();
            
            await Clients.Caller.SendAsync("InitialCostData", new
            {
                analytics = analytics,
                realTimeMetrics = realTimeMetrics,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending initial cost data to user {UserId}", userId);
        }
    }

    private async Task SendRealTimeCostMetrics(string userId)
    {
        try
        {
            var metrics = await _analyticsService.GetRealTimeCostMetricsAsync();
            await Clients.Caller.SendAsync("RealTimeCostMetrics", metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending real-time cost metrics to user {UserId}", userId);
        }
    }

    private async Task SendBudgetStatus(string userId)
    {
        try
        {
            var budgets = await _costService.GetBudgetsAsync(userId, "User");
            var budgetStatus = await _analyticsService.GetBudgetUtilizationAsync(userId);
            
            await Clients.Caller.SendAsync("BudgetStatus", new
            {
                budgets = budgets,
                utilization = budgetStatus,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending budget status to user {UserId}", userId);
        }
    }

    private async Task SendOptimizationRecommendations(string userId)
    {
        try
        {
            var recommendations = await _analyticsService.GenerateOptimizationRecommendationsAsync(userId);
            await Clients.Caller.SendAsync("OptimizationRecommendations", recommendations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending optimization recommendations to user {UserId}", userId);
        }
    }

    private async Task<object> BuildRealTimeDashboardAsync(string userId)
    {
        var analytics = await _analyticsService.GenerateCostAnalyticsAsync(userId);
        var realTimeMetrics = await _analyticsService.GetRealTimeCostMetricsAsync();
        var budgetUtilization = await _analyticsService.GetBudgetUtilizationAsync(userId);
        var recommendations = await _analyticsService.GenerateOptimizationRecommendationsAsync(userId);
        var alerts = await _analyticsService.GetCostAlertsAsync(userId, true);

        return new
        {
            analytics = analytics,
            realTimeMetrics = realTimeMetrics,
            budgetUtilization = budgetUtilization,
            recommendations = recommendations.Take(5), // Top 5 recommendations
            alerts = alerts.Take(10), // Recent alerts
            timestamp = DateTime.UtcNow
        };
    }

    #endregion
}
