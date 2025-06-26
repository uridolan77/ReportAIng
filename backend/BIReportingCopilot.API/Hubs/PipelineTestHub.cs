using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;
using System.Collections.Concurrent;

namespace BIReportingCopilot.API.Hubs;

/// <summary>
/// SignalR Hub for real-time AI Pipeline Testing updates
/// </summary>
[Authorize]
public class PipelineTestHub : Hub
{
    private readonly ILogger<PipelineTestHub> _logger;
    private static readonly ConcurrentDictionary<string, string> _userConnections = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> _testSessions = new();

    public PipelineTestHub(ILogger<PipelineTestHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = GetUserId();
        var connectionId = Context.ConnectionId;

        _userConnections[connectionId] = userId;
        
        _logger.LogInformation("🔗 User {UserId} connected to pipeline test hub with connection {ConnectionId}", 
            userId, connectionId);

        await Groups.AddToGroupAsync(connectionId, $"user_{userId}");
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        
        if (_userConnections.TryRemove(connectionId, out var userId))
        {
            _logger.LogInformation("🔌 User {UserId} disconnected from pipeline test hub (Connection: {ConnectionId})", 
                userId, connectionId);

            // Remove from any test sessions
            foreach (var session in _testSessions.Values)
            {
                session.Remove(connectionId);
            }

            await Groups.RemoveFromGroupAsync(connectionId, $"user_{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// Join a specific test session for real-time updates
    /// </summary>
    public async Task JoinTestSession(string testId)
    {
        var connectionId = Context.ConnectionId;
        var userId = GetUserId();

        if (!_testSessions.ContainsKey(testId))
        {
            _testSessions[testId] = new HashSet<string>();
        }

        _testSessions[testId].Add(connectionId);
        await Groups.AddToGroupAsync(connectionId, $"test_{testId}");

        _logger.LogInformation("👥 User {UserId} joined test session {TestId}", userId, testId);

        // Send current session status if available
        await Clients.Caller.SendAsync("TestSessionJoined", new { testId, userId, joinedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Leave a specific test session
    /// </summary>
    public async Task LeaveTestSession(string testId)
    {
        var connectionId = Context.ConnectionId;
        var userId = GetUserId();

        if (_testSessions.TryGetValue(testId, out var session))
        {
            session.Remove(connectionId);
            if (session.Count == 0)
            {
                _testSessions.TryRemove(testId, out _);
            }
        }

        await Groups.RemoveFromGroupAsync(connectionId, $"test_{testId}");

        _logger.LogInformation("👋 User {UserId} left test session {TestId}", userId, testId);

        await Clients.Caller.SendAsync("TestSessionLeft", new { testId, userId, leftAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Get active test sessions for the current user
    /// </summary>
    public async Task GetActiveTestSessions()
    {
        var userId = GetUserId();
        var activeSessions = _testSessions.Keys.ToList();

        await Clients.Caller.SendAsync("ActiveTestSessions", new { userId, sessions = activeSessions, timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Send a heartbeat to keep connection alive
    /// </summary>
    public async Task Heartbeat()
    {
        var userId = GetUserId();
        await Clients.Caller.SendAsync("HeartbeatResponse", new { userId, timestamp = DateTime.UtcNow });
    }

    private string GetUserId()
    {
        return Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value 
            ?? Context.User?.FindFirst("sub")?.Value 
            ?? Context.User?.FindFirst(ClaimTypes.Name)?.Value 
            ?? "anonymous";
    }

    /// <summary>
    /// Get connection statistics
    /// </summary>
    public static PipelineTestHubStats GetStats()
    {
        return new PipelineTestHubStats
        {
            TotalConnections = _userConnections.Count,
            ActiveTestSessions = _testSessions.Count,
            TotalSessionParticipants = _testSessions.Values.Sum(s => s.Count),
            Timestamp = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Hub statistics
/// </summary>
public class PipelineTestHubStats
{
    public int TotalConnections { get; set; }
    public int ActiveTestSessions { get; set; }
    public int TotalSessionParticipants { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Service for sending real-time updates to pipeline test clients
/// </summary>
public interface IPipelineTestNotificationService
{
    Task SendTestStartedAsync(string testId, string userId, object testRequest);
    Task SendStepStartedAsync(string testId, string stepName, object stepDetails);
    Task SendStepProgressAsync(string testId, string stepName, int progressPercent, string? message = null);
    Task SendStepCompletedAsync(string testId, string stepName, object stepResult);
    Task SendStepErrorAsync(string testId, string stepName, string error, object? details = null);
    Task SendTestCompletedAsync(string testId, object testResult);
    Task SendTestErrorAsync(string testId, string error, object? details = null);
    Task SendParameterValidationAsync(string testId, object validationResult);
    Task SendConfigurationSavedAsync(string userId, object configuration);
    Task SendConfigurationLoadedAsync(string userId, object configuration);
}

/// <summary>
/// Implementation of pipeline test notification service
/// </summary>
public class PipelineTestNotificationService : IPipelineTestNotificationService
{
    private readonly IHubContext<PipelineTestHub> _hubContext;
    private readonly ILogger<PipelineTestNotificationService> _logger;

    public PipelineTestNotificationService(
        IHubContext<PipelineTestHub> hubContext,
        ILogger<PipelineTestNotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendTestStartedAsync(string testId, string userId, object testRequest)
    {
        _logger.LogInformation("📡 Sending test started notification for test {TestId}", testId);
        
        await _hubContext.Clients.Group($"test_{testId}")
            .SendAsync("TestStarted", new { testId, userId, testRequest, timestamp = DateTime.UtcNow });
    }

    public async Task SendStepStartedAsync(string testId, string stepName, object stepDetails)
    {
        _logger.LogInformation("📡 Sending step started notification for {StepName} in test {TestId}", stepName, testId);

        await _hubContext.Clients.Group($"test_{testId}")
            .SendAsync("StepStarted", new { testId, stepName, stepDetails, timestamp = DateTime.UtcNow });
    }

    public async Task SendStepProgressAsync(string testId, string stepName, int progressPercent, string? message = null)
    {
        await _hubContext.Clients.Group($"test_{testId}")
            .SendAsync("StepProgress", new { testId, stepName, progressPercent, message, timestamp = DateTime.UtcNow });
    }

    public async Task SendStepCompletedAsync(string testId, string stepName, object stepResult)
    {
        _logger.LogInformation("📡 Sending step completed notification for {StepName} in test {TestId}", stepName, testId);

        await _hubContext.Clients.Group($"test_{testId}")
            .SendAsync("StepCompleted", new { testId, stepName, stepResult, timestamp = DateTime.UtcNow });
    }

    public async Task SendStepErrorAsync(string testId, string stepName, string error, object? details = null)
    {
        _logger.LogWarning("📡 Sending step error notification for {StepName} in test {TestId}: {Error}", stepName, testId, error);
        
        await _hubContext.Clients.Group($"test_{testId}")
            .SendAsync("StepError", new { testId, stepName, error, details, timestamp = DateTime.UtcNow });
    }

    public async Task SendTestCompletedAsync(string testId, object testResult)
    {
        _logger.LogInformation("📡 Sending test completed notification for test {TestId}", testId);
        
        await _hubContext.Clients.Group($"test_{testId}")
            .SendAsync("TestCompleted", new { testId, testResult, timestamp = DateTime.UtcNow });
    }

    public async Task SendTestErrorAsync(string testId, string error, object? details = null)
    {
        _logger.LogError("📡 Sending test error notification for test {TestId}: {Error}", testId, error);
        
        await _hubContext.Clients.Group($"test_{testId}")
            .SendAsync("TestError", new { testId, error, details, timestamp = DateTime.UtcNow });
    }

    public async Task SendParameterValidationAsync(string testId, object validationResult)
    {
        await _hubContext.Clients.Group($"test_{testId}")
            .SendAsync("ParameterValidation", new { testId, validationResult, timestamp = DateTime.UtcNow });
    }

    public async Task SendConfigurationSavedAsync(string userId, object configuration)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ConfigurationSaved", new { userId, configuration, timestamp = DateTime.UtcNow });
    }

    public async Task SendConfigurationLoadedAsync(string userId, object configuration)
    {
        await _hubContext.Clients.Group($"user_{userId}")
            .SendAsync("ConfigurationLoaded", new { userId, configuration, timestamp = DateTime.UtcNow });
    }
}
