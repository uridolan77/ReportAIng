using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.DTOs;
using System.Collections.Concurrent;

namespace BIReportingCopilot.Infrastructure.Hubs
{
    /// <summary>
    /// SignalR hub for real-time transparency updates during query processing
    /// </summary>
    [Authorize]
    public class TransparencyHub : Hub
    {
        private readonly ILogger<TransparencyHub> _logger;
        private static readonly ConcurrentDictionary<string, string> _userConnections = new();
        private static readonly ConcurrentDictionary<string, List<string>> _traceSubscriptions = new();

        public TransparencyHub(ILogger<TransparencyHub> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Called when a client connects to the hub
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.Identity?.Name ?? "anonymous";
            var connectionId = Context.ConnectionId;

            _userConnections[userId] = connectionId;
            _logger.LogInformation("User {UserId} connected to transparency hub with connection {ConnectionId}", userId, connectionId);

            await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
            await base.OnConnectedAsync();
        }

        /// <summary>
        /// Called when a client disconnects from the hub
        /// </summary>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.Identity?.Name ?? "anonymous";
            var connectionId = Context.ConnectionId;

            _userConnections.TryRemove(userId, out _);
            
            // Remove from all trace subscriptions
            var tracesToRemove = new List<string>();
            foreach (var kvp in _traceSubscriptions)
            {
                kvp.Value.Remove(connectionId);
                if (kvp.Value.Count == 0)
                {
                    tracesToRemove.Add(kvp.Key);
                }
            }

            foreach (var traceId in tracesToRemove)
            {
                _traceSubscriptions.TryRemove(traceId, out _);
            }

            _logger.LogInformation("User {UserId} disconnected from transparency hub", userId);
            await base.OnDisconnectedAsync(exception);
        }

        /// <summary>
        /// Subscribe to real-time updates for a specific trace
        /// </summary>
        public async Task SubscribeToTrace(string traceId)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.User?.Identity?.Name ?? "anonymous";

            _traceSubscriptions.AddOrUpdate(
                traceId,
                new List<string> { connectionId },
                (key, existing) =>
                {
                    if (!existing.Contains(connectionId))
                        existing.Add(connectionId);
                    return existing;
                });

            await Groups.AddToGroupAsync(connectionId, $"trace_{traceId}");
            
            _logger.LogInformation("User {UserId} subscribed to trace {TraceId}", userId, traceId);
            
            await Clients.Caller.SendAsync("TraceSubscribed", new { traceId, message = "Successfully subscribed to trace updates" });
        }

        /// <summary>
        /// Unsubscribe from real-time updates for a specific trace
        /// </summary>
        public async Task UnsubscribeFromTrace(string traceId)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.User?.Identity?.Name ?? "anonymous";

            if (_traceSubscriptions.TryGetValue(traceId, out var connections))
            {
                connections.Remove(connectionId);
                if (connections.Count == 0)
                {
                    _traceSubscriptions.TryRemove(traceId, out _);
                }
            }

            await Groups.RemoveFromGroupAsync(connectionId, $"trace_{traceId}");
            
            _logger.LogInformation("User {UserId} unsubscribed from trace {TraceId}", userId, traceId);
            
            await Clients.Caller.SendAsync("TraceUnsubscribed", new { traceId, message = "Successfully unsubscribed from trace updates" });
        }

        /// <summary>
        /// Get current transparency status
        /// </summary>
        public async Task GetTransparencyStatus()
        {
            var userId = Context.User?.Identity?.Name ?? "anonymous";
            
            var status = new
            {
                userId,
                connectionId = Context.ConnectionId,
                subscribedTraces = _traceSubscriptions.Where(kvp => kvp.Value.Contains(Context.ConnectionId)).Select(kvp => kvp.Key).ToList(),
                timestamp = DateTime.UtcNow
            };

            await Clients.Caller.SendAsync("TransparencyStatus", status);
        }

        /// <summary>
        /// Send real-time transparency update to subscribers
        /// </summary>
        public static async Task SendTransparencyUpdate(IHubContext<TransparencyHub> hubContext, string traceId, TransparencyUpdateDto update)
        {
            await hubContext.Clients.Group($"trace_{traceId}").SendAsync("TransparencyUpdate", update);
        }

        /// <summary>
        /// Send step completion update to subscribers
        /// </summary>
        public static async Task SendStepCompleted(IHubContext<TransparencyHub> hubContext, string traceId, PromptConstructionStepDto step)
        {
            await hubContext.Clients.Group($"trace_{traceId}").SendAsync("StepCompleted", new { traceId, step });
        }

        /// <summary>
        /// Send confidence update to subscribers
        /// </summary>
        public static async Task SendConfidenceUpdate(IHubContext<TransparencyHub> hubContext, string traceId, double confidence, string reason)
        {
            await hubContext.Clients.Group($"trace_{traceId}").SendAsync("ConfidenceUpdate", new { traceId, confidence, reason, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Send trace completion notification to subscribers
        /// </summary>
        public static async Task SendTraceCompleted(IHubContext<TransparencyHub> hubContext, string traceId, TransparencyTraceDto trace)
        {
            await hubContext.Clients.Group($"trace_{traceId}").SendAsync("TraceCompleted", new { traceId, trace, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Send error notification to subscribers
        /// </summary>
        public static async Task SendTransparencyError(IHubContext<TransparencyHub> hubContext, string traceId, string error, string? details = null)
        {
            await hubContext.Clients.Group($"trace_{traceId}").SendAsync("TransparencyError", new { traceId, error, details, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Send optimization suggestion to subscribers
        /// </summary>
        public static async Task SendOptimizationSuggestion(IHubContext<TransparencyHub> hubContext, string traceId, OptimizationSuggestionDto suggestion)
        {
            await hubContext.Clients.Group($"trace_{traceId}").SendAsync("OptimizationSuggestion", new { traceId, suggestion, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Send alternative option to subscribers
        /// </summary>
        public static async Task SendAlternativeOption(IHubContext<TransparencyHub> hubContext, string traceId, AlternativeOptionDto alternative)
        {
            await hubContext.Clients.Group($"trace_{traceId}").SendAsync("AlternativeOption", new { traceId, alternative, timestamp = DateTime.UtcNow });
        }

        /// <summary>
        /// Broadcast transparency metrics update to all connected users
        /// </summary>
        public static async Task BroadcastMetricsUpdate(IHubContext<TransparencyHub> hubContext, TransparencyMetricsDto metrics)
        {
            await hubContext.Clients.All.SendAsync("MetricsUpdate", new { metrics, timestamp = DateTime.UtcNow });
        }
    }

    /// <summary>
    /// Real-time transparency update DTO
    /// </summary>
    public class TransparencyUpdateDto
    {
        public string TraceId { get; set; } = string.Empty;
        public string UpdateType { get; set; } = string.Empty; // "StepStarted", "StepCompleted", "ConfidenceChanged", "Error"
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public double? Progress { get; set; } // 0.0 to 1.0
        public string? CurrentStep { get; set; }
        public double? CurrentConfidence { get; set; }
    }
}
