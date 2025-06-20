using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Core.Interfaces.CostOptimization;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.RealTime;

/// <summary>
/// Service for coordinating real-time notifications across all monitoring hubs
/// Note: This service uses generic hub contexts to avoid circular dependencies with API project
/// </summary>
public class RealTimeNotificationService : IRealTimeNotificationService
{
    private readonly IHubContext<Hub> _hubContext;
    private readonly ILogger<RealTimeNotificationService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public RealTimeNotificationService(
        IHubContext<Hub> hubContext,
        ILogger<RealTimeNotificationService> logger,
        IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    #region Cost Notifications

    public async Task NotifyCostUpdateAsync(string userId, decimal newCost, string category, CancellationToken cancellationToken = default)
    {
        try
        {
            var costUpdate = new
            {
                UserId = userId,
                NewCost = newCost,
                Category = category,
                Timestamp = DateTime.UtcNow
            };

            // Send to all cost monitoring clients
            await _hubContext.Clients.Group("CostMonitoring").SendAsync("CostUpdate", costUpdate, cancellationToken);

            // Send to specific user
            await _hubContext.Clients.Group($"CostUser_{userId}").SendAsync("UserCostUpdate", costUpdate, cancellationToken);

            _logger.LogInformation("💰 Cost update notification sent: ${NewCost} for user {UserId}", newCost, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending cost update notification");
        }
    }

    public async Task NotifyBudgetAlertAsync(string userId, BudgetManagement budget, string alertType, CancellationToken cancellationToken = default)
    {
        try
        {
            var budgetAlert = new
            {
                UserId = userId,
                Budget = budget,
                AlertType = alertType,
                UtilizationRate = budget.BudgetAmount > 0 ? budget.SpentAmount / budget.BudgetAmount : 0,
                Timestamp = DateTime.UtcNow
            };

            // Send to budget alerts subscribers
            await _hubContext.Clients.Group("BudgetAlerts").SendAsync("BudgetAlert", budgetAlert, cancellationToken);

            // Send to specific user
            await _hubContext.Clients.Group($"CostUser_{userId}").SendAsync("UserBudgetAlert", budgetAlert, cancellationToken);

            _logger.LogInformation("💰 Budget alert sent: {AlertType} for budget {BudgetName} (user {UserId})", 
                alertType, budget.Name, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending budget alert notification");
        }
    }

    public async Task NotifyOptimizationRecommendationAsync(string userId, CostOptimizationRecommendation recommendation, CancellationToken cancellationToken = default)
    {
        try
        {
            var recommendationNotification = new
            {
                UserId = userId,
                Recommendation = recommendation,
                Timestamp = DateTime.UtcNow
            };

            // Send to optimization subscribers
            await _hubContext.Clients.Group("OptimizationRecommendations").SendAsync("NewOptimizationRecommendation", recommendationNotification, cancellationToken);

            // Send to specific user
            await _hubContext.Clients.Group($"CostUser_{userId}").SendAsync("UserOptimizationRecommendation", recommendationNotification, cancellationToken);

            _logger.LogInformation("💰 Optimization recommendation sent: {Title} for user {UserId}", 
                recommendation.Title, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending optimization recommendation notification");
        }
    }

    #endregion

    #region Performance Notifications

    public async Task NotifyPerformanceAlertAsync(string entityType, string entityId, PerformanceAlert alert, CancellationToken cancellationToken = default)
    {
        try
        {
            var performanceAlert = new
            {
                EntityType = entityType,
                EntityId = entityId,
                Alert = alert,
                Timestamp = DateTime.UtcNow
            };

            // Send to performance alerts subscribers
            await _hubContext.Clients.Group("PerformanceAlerts").SendAsync("PerformanceAlert", performanceAlert, cancellationToken);

            // Send to entity-specific group
            await _hubContext.Clients.Group($"Entity_{entityType}_{entityId}").SendAsync("EntityPerformanceAlert", performanceAlert, cancellationToken);

            _logger.LogInformation("⚡ Performance alert sent: {AlertType} for {EntityType}:{EntityId}",
                alert.AlertType, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending performance alert notification");
        }
    }

    public async Task NotifyPerformanceMetricUpdateAsync(string entityType, string entityId, PerformanceMetricsEntry metric, CancellationToken cancellationToken = default)
    {
        try
        {
            var metricUpdate = new
            {
                EntityType = entityType,
                EntityId = entityId,
                Metric = metric,
                Timestamp = DateTime.UtcNow
            };

            // Send to performance metrics subscribers
            await _hubContext.Clients.Group("PerformanceMetrics").SendAsync("PerformanceMetricUpdate", metricUpdate, cancellationToken);

            // Send to entity-specific group
            await _hubContext.Clients.Group($"Entity_{entityType}_{entityId}").SendAsync("EntityMetricUpdate", metricUpdate, cancellationToken);

            _logger.LogDebug("⚡ Performance metric update sent: {MetricName} = {Value} for {EntityType}:{EntityId}", 
                metric.MetricName, metric.Value, entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending performance metric update notification");
        }
    }

    public async Task NotifyOptimizationCompletedAsync(string entityType, string entityId, PerformanceOptimizationResult result, CancellationToken cancellationToken = default)
    {
        try
        {
            var optimizationResult = new
            {
                EntityType = entityType,
                EntityId = entityId,
                Result = result,
                Timestamp = DateTime.UtcNow
            };

            // Send to performance monitoring subscribers
            await _hubContext.Clients.Group("PerformanceMonitoring").SendAsync("OptimizationCompleted", optimizationResult, cancellationToken);

            _logger.LogInformation("⚡ Optimization completed notification sent for {EntityType}:{EntityId}", entityType, entityId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending optimization completed notification");
        }
    }

    #endregion

    #region Resource Notifications

    public async Task NotifyQuotaExceededAsync(string userId, string resourceType, ResourceQuota quota, CancellationToken cancellationToken = default)
    {
        try
        {
            var quotaAlert = new
            {
                UserId = userId,
                ResourceType = resourceType,
                Quota = quota,
                UtilizationRate = quota.MaxQuantity > 0 ? (double)quota.CurrentUsage / quota.MaxQuantity : 0,
                Timestamp = DateTime.UtcNow
            };

            // Send to quota updates subscribers
            await _hubContext.Clients.Group("QuotaUpdates").SendAsync("QuotaExceeded", quotaAlert, cancellationToken);

            // Send to specific user
            await _hubContext.Clients.Group($"ResourceUser_{userId}").SendAsync("UserQuotaExceeded", quotaAlert, cancellationToken);

            _logger.LogWarning("🔧 Quota exceeded notification sent: {ResourceType} for user {UserId}", resourceType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending quota exceeded notification");
        }
    }

    public async Task NotifyCircuitBreakerStateChangeAsync(string serviceName, CircuitBreakerState newState, CancellationToken cancellationToken = default)
    {
        try
        {
            var stateChange = new
            {
                ServiceName = serviceName,
                NewState = newState,
                Timestamp = DateTime.UtcNow
            };

            // Send to circuit breaker subscribers
            await _hubContext.Clients.Group("CircuitBreakerUpdates").SendAsync("CircuitBreakerStateChanged", stateChange, cancellationToken);

            _logger.LogInformation("🔧 Circuit breaker state change notification sent: {ServiceName} -> {State}", 
                serviceName, newState.State);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending circuit breaker state change notification");
        }
    }

    public async Task NotifyResourceLoadUpdateAsync(string resourceId, double loadPercentage, CancellationToken cancellationToken = default)
    {
        try
        {
            var loadUpdate = new
            {
                ResourceId = resourceId,
                LoadPercentage = loadPercentage,
                Timestamp = DateTime.UtcNow
            };

            // Send to load balancing subscribers
            await _hubContext.Clients.Group("LoadBalancing").SendAsync("ResourceLoadUpdate", loadUpdate, cancellationToken);

            _logger.LogDebug("🔧 Resource load update notification sent: {ResourceId} = {LoadPercentage:P}", 
                resourceId, loadPercentage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending resource load update notification");
        }
    }

    #endregion

    #region Batch Notifications

    public async Task SendBatchNotificationsAsync(List<RealTimeNotification> notifications, CancellationToken cancellationToken = default)
    {
        try
        {
            var tasks = notifications.Select(notification => SendNotificationAsync(notification, cancellationToken));
            await Task.WhenAll(tasks);

            _logger.LogInformation("📡 Sent batch of {Count} real-time notifications", notifications.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending batch notifications");
        }
    }

    private async Task SendNotificationAsync(RealTimeNotification notification, CancellationToken cancellationToken)
    {
        switch (notification.Type)
        {
            case "CostUpdate":
                var cost = notification.Data.TryGetValue("cost", out var costValue) ? Convert.ToDecimal(costValue) : 0m;
                var category = notification.Data.TryGetValue("category", out var categoryValue) ? categoryValue?.ToString() ?? "" : "";
                await NotifyCostUpdateAsync(notification.UserId, cost, category, cancellationToken);
                break;
            case "PerformanceAlert":
                // Handle performance alert
                break;
            case "QuotaExceeded":
                // Handle quota exceeded
                break;
            default:
                _logger.LogWarning("Unknown notification type: {Type}", notification.Type);
                break;
        }
    }

    #endregion
}

/// <summary>
/// Interface for real-time notification service
/// </summary>
public interface IRealTimeNotificationService
{
    // Cost notifications
    Task NotifyCostUpdateAsync(string userId, decimal newCost, string category, CancellationToken cancellationToken = default);
    Task NotifyBudgetAlertAsync(string userId, BudgetManagement budget, string alertType, CancellationToken cancellationToken = default);
    Task NotifyOptimizationRecommendationAsync(string userId, CostOptimizationRecommendation recommendation, CancellationToken cancellationToken = default);

    // Performance notifications
    Task NotifyPerformanceAlertAsync(string entityType, string entityId, PerformanceAlert alert, CancellationToken cancellationToken = default);
    Task NotifyPerformanceMetricUpdateAsync(string entityType, string entityId, PerformanceMetricsEntry metric, CancellationToken cancellationToken = default);
    Task NotifyOptimizationCompletedAsync(string entityType, string entityId, PerformanceOptimizationResult result, CancellationToken cancellationToken = default);

    // Resource notifications
    Task NotifyQuotaExceededAsync(string userId, string resourceType, ResourceQuota quota, CancellationToken cancellationToken = default);
    Task NotifyCircuitBreakerStateChangeAsync(string serviceName, CircuitBreakerState newState, CancellationToken cancellationToken = default);
    Task NotifyResourceLoadUpdateAsync(string resourceId, double loadPercentage, CancellationToken cancellationToken = default);

    // Batch notifications
    Task SendBatchNotificationsAsync(List<RealTimeNotification> notifications, CancellationToken cancellationToken = default);
}

/// <summary>
/// Real-time notification model
/// </summary>
public class RealTimeNotification
{
    public string Type { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
