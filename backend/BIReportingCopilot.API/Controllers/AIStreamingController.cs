using Microsoft.AspNetCore.Mvc;
using BIReportingCopilot.Core.Interfaces.Streaming;
using BIReportingCopilot.Core.Interfaces.Data;
using BIReportingCopilot.Core.DTOs;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for AI streaming services and real-time processing
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AIStreamingController : ControllerBase
{
    private readonly IRealTimeStreamingService _streamingService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AIStreamingController> _logger;
    private readonly Dictionary<string, StreamingSession> _activeSessions = new();

    public AIStreamingController(
        IRealTimeStreamingService streamingService,
        IAuditService auditService,
        ILogger<AIStreamingController> logger)
    {
        _streamingService = streamingService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Stream query processing with real-time updates
    /// </summary>
    [HttpGet("query/{sessionId}")]
    public async Task<ActionResult<string>> StreamQueryProcessing(
        string sessionId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting query streaming for session: {SessionId}", sessionId);

        try
        {
            // Check if session exists
            if (!_activeSessions.TryGetValue(sessionId, out var session))
            {
                return NotFound(new { error = "Session not found", sessionId = sessionId });
            }

            // Simulate processing
            await Task.Delay(2000, cancellationToken);

            // Update session status
            session.Status = "completed";
            session.EndTime = DateTime.UtcNow;

            return Ok(new { message = "Query processing completed", sessionId = sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in query streaming for session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Query processing failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Stream insight generation with real-time updates
    /// </summary>
    [HttpGet("insights/{queryId}")]
    public async Task<ActionResult<string>> StreamInsightGeneration(
        string queryId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting insight streaming for query: {QueryId}", queryId);

        try
        {
            // Simulate insight generation
            await Task.Delay(2000, cancellationToken);

            return Ok(new {
                message = "Insight generation completed",
                queryId = queryId,
                insight = "Based on the analysis, revenue shows a 15% increase in Q3 with strong performance in the technology sector."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in insight streaming for query {QueryId}", queryId);
            return StatusCode(500, new { error = "Insight generation failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Stream analytics updates for a user
    /// </summary>
    [HttpGet("analytics/{userId}")]
    public async Task<ActionResult<string>> StreamAnalytics(
        string userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting analytics streaming for user: {UserId}", userId);

        try
        {
            // Simulate analytics processing
            await Task.Delay(1000, cancellationToken);

            var random = new Random();
            var analytics = new
            {
                QueryCount = random.Next(10, 100),
                AverageResponseTime = random.NextDouble() * 2000 + 500,
                UserActivity = new { trend = "increasing", percentage = random.Next(5, 25) }
            };

            return Ok(new {
                message = "Analytics generated",
                userId = userId,
                analytics = analytics
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in analytics streaming for user {UserId}", userId);
            return StatusCode(500, new { error = "Analytics generation failed", details = ex.Message });
        }
    }

    /// <summary>
    /// Start a new streaming session
    /// </summary>
    [HttpPost("start")]
    public async Task<ActionResult<string>> StartStreaming([FromBody] BIReportingCopilot.Core.DTOs.StartStreamingRequest request)
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString();
            
            var session = new StreamingSession
            {
                SessionId = sessionId,
                Query = request.Query,
                Status = "starting",
                Progress = 0.0,
                StartTime = DateTime.UtcNow
            };

            _activeSessions[sessionId] = session;

            _logger.LogInformation("Started streaming session {SessionId} for query: {Query}", sessionId, request.Query);

            // Log the session start for audit
            await _auditService.LogAsync(
                "StreamingSessionStart",
                request.UserId ?? "anonymous",
                "StreamingSession",
                sessionId,
                new { query = request.Query, options = request.Options });

            return Ok(new { sessionId = sessionId, status = "started" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting streaming session");
            return StatusCode(500, new { error = "Failed to start streaming session", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a streaming session
    /// </summary>
    [HttpPost("cancel/{sessionId}")]
    public async Task<ActionResult> CancelStreaming(string sessionId)
    {
        try
        {
            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                session.Status = "cancelled";
                session.EndTime = DateTime.UtcNow;
                
                _logger.LogInformation("Cancelled streaming session: {SessionId}", sessionId);
                
                return Ok(new { message = "Streaming session cancelled", sessionId = sessionId });
            }

            return NotFound(new { error = "Session not found", sessionId = sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling streaming session {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to cancel streaming session", details = ex.Message });
        }
    }

    /// <summary>
    /// Get streaming session status
    /// </summary>
    [HttpGet("status/{sessionId}")]
    public ActionResult<StreamingSession> GetSessionStatus(string sessionId)
    {
        try
        {
            if (_activeSessions.TryGetValue(sessionId, out var session))
            {
                return Ok(session);
            }

            return NotFound(new { error = "Session not found", sessionId = sessionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving session status for {SessionId}", sessionId);
            return StatusCode(500, new { error = "Failed to retrieve session status", details = ex.Message });
        }
    }
}
