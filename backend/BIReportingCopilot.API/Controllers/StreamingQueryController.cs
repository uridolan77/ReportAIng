using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Streaming;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.API.Hubs;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Streaming query execution controller for real-time query processing
/// </summary>
[ApiController]
[Route("api/query/streaming")]
[Authorize]
public class StreamingQueryController : ControllerBase
{
    private readonly ILogger<StreamingQueryController> _logger;
    private readonly BIReportingCopilot.Infrastructure.Performance.IStreamingSqlQueryService _streamingQueryService;
    private readonly IHubContext<QueryStatusHub> _hubContext;

    public StreamingQueryController(
        ILogger<StreamingQueryController> logger,
        BIReportingCopilot.Infrastructure.Performance.IStreamingSqlQueryService streamingQueryService,
        IHubContext<QueryStatusHub> hubContext)
    {
        _logger = logger;
        _streamingQueryService = streamingQueryService;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Get metadata for streaming query execution
    /// </summary>
    [HttpPost("metadata")]
    public async Task<ActionResult<StreamingQueryMetadata>> GetStreamingQueryMetadata(
        [FromBody] StreamingQueryRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Getting streaming query metadata for user {UserId}", userId);

            var metadata = new
            {
                EstimatedRowCount = 1000, // This would be calculated based on the query
                EstimatedExecutionTime = TimeSpan.FromSeconds(30),
                ChunkSize = 100,
                SupportsStreaming = true,
                QueryId = Guid.NewGuid().ToString()
            };

            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streaming query metadata");
            return StatusCode(500, new { error = "Failed to get streaming query metadata" });
        }
    }

    /// <summary>
    /// Execute a streaming query with real-time results
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult> ExecuteStreamingQuery([FromBody] StreamingQueryRequest request)
    {
        var queryId = Guid.NewGuid().ToString();
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";

        try
        {
            _logger.LogInformation("Starting streaming query execution for user {UserId}, queryId {QueryId}", 
                userId, queryId);

            // Send initial status
            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryStarted", new
            {
                QueryId = queryId,
                Status = "started",
                Timestamp = DateTime.UtcNow
            });

            // Mock streaming query execution
            for (int i = 0; i < 5; i++)
            {
                // Send each chunk to the client
                await _hubContext.Clients.User(userId).SendAsync("StreamingQueryChunk", new
                {
                    QueryId = queryId,
                    Data = new[] { new { id = i, value = $"Sample data {i}" } },
                    ChunkIndex = i,
                    TotalChunks = 5,
                    IsLastChunk = i == 4,
                    Timestamp = DateTime.UtcNow
                });

                // Add small delay to prevent overwhelming the client
                await Task.Delay(500);
            }

            // Send completion status
            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryCompleted", new
            {
                QueryId = queryId,
                Status = "completed",
                Timestamp = DateTime.UtcNow
            });

            return Ok(new { queryId, message = "Streaming query started successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing streaming query for queryId {QueryId}", queryId);

            // Send error status
            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryError", new
            {
                QueryId = queryId,
                Status = "error",
                Error = "An error occurred during query execution",
                Timestamp = DateTime.UtcNow
            });

            return StatusCode(500, new { error = "Failed to execute streaming query" });
        }
    }

    /// <summary>
    /// Cancel a running streaming query
    /// </summary>
    [HttpPost("cancel/{queryId}")]
    public async Task<ActionResult> CancelStreamingQuery(string queryId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Cancelling streaming query {QueryId} for user {UserId}", queryId, userId);

            // Mock query cancellation
            _logger.LogInformation("Query {QueryId} cancelled", queryId);

            // Send cancellation status
            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryCancelled", new
            {
                QueryId = queryId,
                Status = "cancelled",
                Timestamp = DateTime.UtcNow
            });

            return Ok(new { message = "Query cancelled successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling streaming query {QueryId}", queryId);
            return StatusCode(500, new { error = "Failed to cancel query" });
        }
    }

    /// <summary>
    /// Get status of a streaming query
    /// </summary>
    [HttpGet("status/{queryId}")]
    public async Task<ActionResult<StreamingQueryStatus>> GetStreamingQueryStatus(string queryId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Getting status for streaming query {QueryId}", queryId);

            // Mock query status
            var status = new
            {
                QueryId = queryId,
                Status = "completed",
                ProcessedRows = 1000,
                TotalRows = 1000,
                ProgressPercentage = 100.0,
                StartedAt = DateTime.UtcNow.AddMinutes(-5),
                CompletedAt = DateTime.UtcNow,
                Error = (string?)null
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streaming query status for {QueryId}", queryId);
            return StatusCode(500, new { error = "Failed to get query status" });
        }
    }

    /// <summary>
    /// Get list of active streaming queries for the user
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<List<StreamingQueryInfo>>> GetActiveStreamingQueries()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            
            _logger.LogInformation("Getting active streaming queries for user {UserId}", userId);

            // Mock active queries
            var activeQueries = new List<object>
            {
                new
                {
                    QueryId = Guid.NewGuid().ToString(),
                    Query = "SELECT * FROM sample_table",
                    Status = "running",
                    StartedAt = DateTime.UtcNow.AddMinutes(-2),
                    ProgressPercentage = 75.0
                }
            };

            return Ok(activeQueries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active streaming queries");
            return StatusCode(500, new { error = "Failed to get active queries" });
        }
    }
}

// These streaming models are already defined elsewhere
