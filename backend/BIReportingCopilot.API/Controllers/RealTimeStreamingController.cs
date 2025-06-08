using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Queries;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Real-time Streaming Analytics controller
/// Provides live data processing, streaming dashboards, and real-time insights
/// </summary>
[ApiController]
[Route("api/real-time-streaming")]
[Authorize]
public class RealTimeStreamingController : ControllerBase
{
    private readonly ILogger<RealTimeStreamingController> _logger;
    private readonly IMediator _mediator;

    public RealTimeStreamingController(
        ILogger<RealTimeStreamingController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Start a real-time streaming session
    /// </summary>
    [HttpPost("sessions/start")]
    public async Task<IActionResult> StartStreamingSession([FromBody] StartStreamingRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🎬 Starting streaming session for user {UserId}", userId);

            var command = new StartStreamingSessionCommand
            {
                UserId = userId,
                Configuration = request.Configuration ?? new StreamingConfiguration()
            };

            var session = await _mediator.Send(command);

            _logger.LogInformation("🎬 Streaming session {SessionId} started for user {UserId}", 
                session.SessionId, userId);

            return Ok(new
            {
                success = true,
                data = session,
                metadata = new
                {
                    session_id = session.SessionId,
                    started_at = session.StartedAt,
                    configuration = session.Configuration
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error starting streaming session");
            return StatusCode(500, new { success = false, error = "Internal server error starting streaming session" });
        }
    }

    /// <summary>
    /// Stop a streaming session
    /// </summary>
    [HttpPost("sessions/{sessionId}/stop")]
    public async Task<IActionResult> StopStreamingSession(string sessionId)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🛑 Stopping streaming session {SessionId} for user {UserId}", sessionId, userId);

            var command = new StopStreamingSessionCommand
            {
                SessionId = sessionId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("🛑 Streaming session {SessionId} stopped successfully", sessionId);
                return Ok(new { success = true, message = "Streaming session stopped successfully" });
            }
            else
            {
                return NotFound(new { success = false, error = "Streaming session not found or access denied" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error stopping streaming session {SessionId}", sessionId);
            return StatusCode(500, new { success = false, error = "Internal server error stopping streaming session" });
        }
    }

    /// <summary>
    /// Get real-time dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetRealTimeDashboard()
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📈 Getting real-time dashboard for user {UserId}", userId);

            var query = new GetRealTimeDashboardQuery
            {
                UserId = userId
            };

            var dashboard = await _mediator.Send(query);

            _logger.LogInformation("📈 Real-time dashboard retrieved for user {UserId} with {ChartCount} charts", 
                userId, dashboard.LiveCharts.Count);

            return Ok(new
            {
                success = true,
                data = dashboard,
                metadata = new
                {
                    generated_at = dashboard.GeneratedAt,
                    active_sessions = dashboard.ActiveSessions.Count,
                    live_charts = dashboard.LiveCharts.Count,
                    alerts = dashboard.StreamingAlerts.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting real-time dashboard");
            return StatusCode(500, new { success = false, error = "Internal server error getting real-time dashboard" });
        }
    }

    /// <summary>
    /// Get streaming analytics for a time window
    /// </summary>
    [HttpGet("analytics")]
    public async Task<IActionResult> GetStreamingAnalytics([FromQuery] int? hours = 1)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📊 Getting streaming analytics for user {UserId}", userId);

            var query = new GetStreamingAnalyticsQuery
            {
                TimeWindow = TimeSpan.FromHours(hours ?? 1),
                UserId = userId
            };

            var analytics = await _mediator.Send(query);

            _logger.LogInformation("📊 Streaming analytics retrieved - Events: {EventCount}, Users: {UserCount}", 
                analytics.TotalEvents, analytics.UserActivitySummary.ActiveUsers);

            return Ok(new
            {
                success = true,
                data = analytics,
                metadata = new
                {
                    time_window_hours = hours ?? 1,
                    total_events = analytics.TotalEvents,
                    active_users = analytics.UserActivitySummary.ActiveUsers,
                    generated_at = analytics.GeneratedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting streaming analytics");
            return StatusCode(500, new { success = false, error = "Internal server error getting streaming analytics" });
        }
    }

    /// <summary>
    /// Subscribe to a data stream
    /// </summary>
    [HttpPost("subscriptions")]
    public async Task<IActionResult> SubscribeToDataStream([FromBody] SubscribeRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📡 Creating data stream subscription for user {UserId}", userId);

            var command = new SubscribeToDataStreamCommand
            {
                UserId = userId,
                Subscription = request.Subscription
            };

            var subscriptionId = await _mediator.Send(command);

            _logger.LogInformation("📡 Data stream subscription {SubscriptionId} created for user {UserId}", 
                subscriptionId, userId);

            return Ok(new
            {
                success = true,
                data = new { subscription_id = subscriptionId },
                metadata = new
                {
                    event_type = request.Subscription.EventType,
                    created_at = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating data stream subscription");
            return StatusCode(500, new { success = false, error = "Internal server error creating subscription" });
        }
    }

    /// <summary>
    /// Unsubscribe from a data stream
    /// </summary>
    [HttpDelete("subscriptions/{subscriptionId}")]
    public async Task<IActionResult> UnsubscribeFromDataStream(string subscriptionId)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📡 Removing data stream subscription {SubscriptionId} for user {UserId}", 
                subscriptionId, userId);

            var command = new UnsubscribeFromDataStreamCommand
            {
                SubscriptionId = subscriptionId,
                UserId = userId
            };

            var result = await _mediator.Send(command);

            if (result)
            {
                _logger.LogInformation("📡 Data stream subscription {SubscriptionId} removed for user {UserId}", 
                    subscriptionId, userId);
                return Ok(new { success = true, message = "Subscription removed successfully" });
            }
            else
            {
                return NotFound(new { success = false, error = "Subscription not found or access denied" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error removing data stream subscription {SubscriptionId}", subscriptionId);
            return StatusCode(500, new { success = false, error = "Internal server error removing subscription" });
        }
    }

    /// <summary>
    /// Create a real-time alert
    /// </summary>
    [HttpPost("alerts")]
    public async Task<IActionResult> CreateRealTimeAlert([FromBody] CreateAlertRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🚨 Creating real-time alert for user {UserId}: {Title}", userId, request.Alert.Title);

            var command = new CreateRealTimeAlertCommand
            {
                Alert = request.Alert,
                UserId = userId
            };

            var alertId = await _mediator.Send(command);

            _logger.LogInformation("🚨 Real-time alert {AlertId} created for user {UserId}", alertId, userId);

            return Ok(new
            {
                success = true,
                data = new { alert_id = alertId },
                metadata = new
                {
                    title = request.Alert.Title,
                    severity = request.Alert.Severity,
                    created_at = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating real-time alert");
            return StatusCode(500, new { success = false, error = "Internal server error creating alert" });
        }
    }

    /// <summary>
    /// Get active streaming sessions
    /// </summary>
    [HttpGet("sessions")]
    public async Task<IActionResult> GetActiveStreamingSessions()
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📋 Getting active streaming sessions for user {UserId}", userId);

            var query = new GetActiveStreamingSessionsQuery
            {
                UserId = userId
            };

            var sessions = await _mediator.Send(query);

            _logger.LogInformation("📋 Retrieved {Count} active streaming sessions for user {UserId}", 
                sessions.Count, userId);

            return Ok(new
            {
                success = true,
                data = sessions,
                metadata = new
                {
                    session_count = sessions.Count,
                    active_sessions = sessions.Count(s => s.Status == SessionStatus.Active)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting active streaming sessions");
            return StatusCode(500, new { success = false, error = "Internal server error getting streaming sessions" });
        }
    }

    /// <summary>
    /// Get real-time metrics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetRealTimeMetrics()
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📊 Getting real-time metrics for user {UserId}", userId);

            var query = new GetRealTimeMetricsQuery
            {
                UserId = userId
            };

            var metrics = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    active_users = metrics.ActiveUsers,
                    queries_per_minute = metrics.QueriesPerMinute,
                    last_updated = metrics.LastUpdated
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting real-time metrics");
            return StatusCode(500, new { success = false, error = "Internal server error getting real-time metrics" });
        }
    }

    /// <summary>
    /// Get streaming performance metrics
    /// </summary>
    [HttpGet("performance")]
    public async Task<IActionResult> GetStreamingPerformance()
    {
        try
        {
            _logger.LogInformation("📊 Getting streaming performance metrics");

            var query = new GetStreamingPerformanceQuery();
            var performance = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = performance,
                metadata = new
                {
                    total_throughput = performance.TotalThroughput,
                    average_latency = performance.AverageLatency,
                    active_sessions = performance.ActiveSessions,
                    generated_at = performance.GeneratedAt
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting streaming performance metrics");
            return StatusCode(500, new { success = false, error = "Internal server error getting performance metrics" });
        }
    }
}

// Request models
public class StartStreamingRequest
{
    public StreamingConfiguration? Configuration { get; set; }
}

public class SubscribeRequest
{
    public StreamSubscription Subscription { get; set; } = new();
}

public class CreateAlertRequest
{
    public RealTimeAlert Alert { get; set; } = new();
}
