using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using System.Security.Claims;
using System.Runtime.CompilerServices;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Performance;
using BIReportingCopilot.API.Hubs;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StreamingQueryController : ControllerBase
{
    private readonly IStreamingSqlQueryService _streamingQueryService;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;
    private readonly IHubContext<QueryHub> _hubContext;
    private readonly ILogger<StreamingQueryController> _logger;

    public StreamingQueryController(
        IStreamingSqlQueryService streamingQueryService,
        IAIService aiService,
        ISchemaService schemaService,
        IHubContext<QueryHub> hubContext,
        ILogger<StreamingQueryController> logger)
    {
        _streamingQueryService = streamingQueryService;
        _aiService = aiService;
        _schemaService = schemaService;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Get metadata for a streaming query before execution
    /// </summary>
    [HttpPost("metadata")]
    public async Task<ActionResult<StreamingQueryMetadata>> GetStreamingQueryMetadata(
        [FromBody] StreamingQueryRequest request)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("Getting streaming metadata for user {UserId}: {Question}", userId, request.Question);

            // Generate SQL from natural language
            var schema = await _schemaService.GetSchemaMetadataAsync();
            var schemaSummary = await _schemaService.GetSchemaSummaryAsync();
            var prompt = $"Generate SQL for: {request.Question}\n\nSchema: {schemaSummary}";
            var sql = await _aiService.GenerateSQLAsync(prompt);

            // Get streaming metadata
            var metadata = await _streamingQueryService.GetStreamingQueryMetadataAsync(sql, request.Options);

            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting streaming query metadata");
            return StatusCode(500, new { error = "Failed to get query metadata", details = ex.Message });
        }
    }

    /// <summary>
    /// Execute a streaming query with real-time progress updates via SignalR
    /// </summary>
    [HttpPost("execute")]
    public async Task<ActionResult> ExecuteStreamingQuery([FromBody] StreamingQueryRequest request)
    {
        var queryId = Guid.NewGuid().ToString();
        var userId = User.Identity?.Name ?? "anonymous";

        try
        {
            _logger.LogInformation("Starting streaming query {QueryId} for user {UserId}: {Question}",
                queryId, userId, request.Question);

            // Notify client that query started
            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryStarted", new
            {
                QueryId = queryId,
                Question = request.Question,
                Timestamp = DateTime.UtcNow
            });

            // Generate SQL from natural language
            var schema = await _schemaService.GetSchemaMetadataAsync();
            var schemaSummary = await _schemaService.GetSchemaSummaryAsync();
            var prompt = $"Generate SQL for: {request.Question}\n\nSchema: {schemaSummary}";
            var sql = await _aiService.GenerateSQLAsync(prompt);

            // Validate SQL
            var isValid = await _streamingQueryService.ValidateSqlAsync(sql);
            if (!isValid)
            {
                await _hubContext.Clients.User(userId).SendAsync("StreamingQueryError", new
                {
                    QueryId = queryId,
                    Error = "Generated SQL failed validation",
                    Sql = sql
                });
                return BadRequest(new { error = "Generated SQL failed validation", sql });
            }

            // Send SQL to client
            await _hubContext.Clients.User(userId).SendAsync("StreamingQuerySql", new
            {
                QueryId = queryId,
                Sql = sql
            });

            // Start background task for streaming execution
            _ = Task.Run(async () => await ExecuteStreamingQueryBackground(queryId, userId, sql, request.Options));

            return Ok(new { queryId, message = "Streaming query started", sql });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting streaming query {QueryId}", queryId);

            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryError", new
            {
                QueryId = queryId,
                Error = ex.Message
            });

            return StatusCode(500, new { error = "Failed to start streaming query", details = ex.Message });
        }
    }

    /// <summary>
    /// Cancel a running streaming query
    /// </summary>
    [HttpPost("{queryId}/cancel")]
    public async Task<ActionResult> CancelStreamingQuery(string queryId)
    {
        var userId = User.Identity?.Name ?? "anonymous";

        try
        {
            _logger.LogInformation("Cancelling streaming query {QueryId} for user {UserId}", queryId, userId);

            // In a real implementation, you'd maintain a registry of running queries with cancellation tokens
            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryCancelled", new
            {
                QueryId = queryId,
                Timestamp = DateTime.UtcNow
            });

            return Ok(new { message = "Query cancellation requested" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling streaming query {QueryId}", queryId);
            return StatusCode(500, new { error = "Failed to cancel query", details = ex.Message });
        }
    }

    private async Task ExecuteStreamingQueryBackground(string queryId, string userId, string sql, QueryOptions options)
    {
        var startTime = DateTime.UtcNow;
        var totalRows = 0;
        var chunkCount = 0;

        try
        {
            await foreach (var chunk in _streamingQueryService.ExecuteSelectQueryChunkedAsync(sql, options))
            {
                totalRows += chunk.TotalRowsInChunk;
                chunkCount++;

                // Send chunk to client via SignalR
                await _hubContext.Clients.User(userId).SendAsync("StreamingQueryChunk", new
                {
                    QueryId = queryId,
                    ChunkIndex = chunk.ChunkIndex,
                    Data = chunk.Data,
                    TotalRowsInChunk = chunk.TotalRowsInChunk,
                    IsLastChunk = chunk.IsLastChunk,
                    ProcessingTimeMs = chunk.ProcessingTimeMs,
                    TotalRowsSoFar = totalRows,
                    Timestamp = chunk.Timestamp
                });

                _logger.LogDebug("Sent chunk {ChunkIndex} with {RowCount} rows for query {QueryId}",
                    chunk.ChunkIndex, chunk.TotalRowsInChunk, queryId);

                if (chunk.IsLastChunk)
                    break;
            }

            var totalTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Send completion notification
            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryCompleted", new
            {
                QueryId = queryId,
                TotalRows = totalRows,
                TotalChunks = chunkCount,
                ExecutionTimeMs = totalTime,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Completed streaming query {QueryId}: {TotalRows} rows in {TotalTime}ms",
                queryId, totalRows, totalTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming query background execution {QueryId}", queryId);

            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryError", new
            {
                QueryId = queryId,
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Stream query results with advanced backpressure control
    /// </summary>
    [HttpPost("stream-backpressure")]
    public IAsyncEnumerable<StreamingQueryChunk> StreamQueryWithBackpressure(
        [FromBody] AdvancedStreamingRequest request,
        CancellationToken cancellationToken = default)
    {
        return StreamQueryWithBackpressureInternal(request, cancellationToken);
    }

    private async IAsyncEnumerable<StreamingQueryChunk> StreamQueryWithBackpressureInternal(
        AdvancedStreamingRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        _logger.LogInformation("Starting backpressure streaming query for user {UserId}: {Question}", userId, request.Question);

        // Generate SQL from natural language
        string sql;
        QueryOptions options;
        bool hasError = false;

        try
        {
            sql = await _aiService.GenerateSQLAsync(request.Question, cancellationToken);
            _logger.LogDebug("Generated SQL for streaming: {Sql}", sql);

            options = new QueryOptions
            {
                MaxRows = request.MaxRows,
                TimeoutSeconds = request.TimeoutSeconds,
                ChunkSize = request.ChunkSize,
                EnableStreaming = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL for backpressure streaming query for user {UserId}", userId);
            hasError = true;
            sql = string.Empty;
            options = new QueryOptions();
        }

        if (hasError)
        {
            yield return CreateErrorChunk();
            yield break;
        }

        var chunkIndex = 0;
        await foreach (var chunk in _streamingQueryService.ExecuteSelectQueryWithBackpressureAsync(sql, options, cancellationToken))
        {
            // Send progress update via SignalR (non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubContext.Clients.User(userId).SendAsync("QueryProgress", new
                    {
                        ChunkIndex = chunk.ChunkIndex,
                        RowsInChunk = chunk.TotalRowsInChunk,
                        IsLastChunk = chunk.IsLastChunk,
                        ProcessingTime = chunk.ProcessingTimeMs
                    }, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending progress update for user {UserId}", userId);
                }
            });

            yield return chunk;
            chunkIndex++;
        }

        _logger.LogInformation("Completed backpressure streaming query for user {UserId}, total chunks: {ChunkCount}", userId, chunkIndex);
    }

    private static StreamingQueryChunk CreateErrorChunk()
    {
        return new StreamingQueryChunk
        {
            ChunkIndex = -1,
            Data = new List<Dictionary<string, object>>(),
            TotalRowsInChunk = 0,
            IsLastChunk = true,
            ProcessingTimeMs = 0
        };
    }

    /// <summary>
    /// Stream query results with real-time progress reporting
    /// </summary>
    [HttpPost("stream-progress")]
    public IAsyncEnumerable<StreamingProgressUpdate> StreamQueryWithProgress(
        [FromBody] AdvancedStreamingRequest request,
        CancellationToken cancellationToken = default)
    {
        return StreamQueryWithProgressInternal(request, cancellationToken);
    }

    private async IAsyncEnumerable<StreamingProgressUpdate> StreamQueryWithProgressInternal(
        AdvancedStreamingRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        _logger.LogInformation("Starting progress streaming query for user {UserId}: {Question}", userId, request.Question);

        // Generate SQL from natural language
        string sql;
        QueryOptions options;
        bool hasError = false;
        string errorMessage = string.Empty;

        try
        {
            sql = await _aiService.GenerateSQLAsync(request.Question, cancellationToken);
            _logger.LogDebug("Generated SQL for progress streaming: {Sql}", sql);

            options = new QueryOptions
            {
                MaxRows = request.MaxRows,
                TimeoutSeconds = request.TimeoutSeconds,
                ChunkSize = request.ChunkSize,
                EnableStreaming = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL for progress streaming query for user {UserId}", userId);
            hasError = true;
            errorMessage = ex.Message;
            sql = string.Empty;
            options = new QueryOptions();
        }

        if (hasError)
        {
            yield return CreateErrorProgress(errorMessage);
            yield break;
        }

        await foreach (var progress in _streamingQueryService.ExecuteSelectQueryWithProgressAsync(sql, options, cancellationToken))
        {
            // Send detailed progress via SignalR (non-blocking)
            _ = Task.Run(async () =>
            {
                try
                {
                    await _hubContext.Clients.User(userId).SendAsync("DetailedProgress", progress, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error sending detailed progress for user {UserId}", userId);
                }
            });

            yield return progress;

            if (progress.IsCompleted)
            {
                _logger.LogInformation("Completed progress streaming query for user {UserId}, total rows: {RowCount}",
                    userId, progress.RowsProcessed);
                break;
            }
        }
    }

    private static StreamingProgressUpdate CreateErrorProgress(string errorMessage)
    {
        return new StreamingProgressUpdate
        {
            RowsProcessed = 0,
            EstimatedTotalRows = 0,
            ProgressPercentage = 0,
            ElapsedTime = TimeSpan.Zero,
            EstimatedTimeRemaining = TimeSpan.Zero,
            RowsPerSecond = 0,
            Status = "Error",
            IsCompleted = true,
            ErrorMessage = errorMessage
        };
    }
}

public class StreamingQueryRequest
{
    public string Question { get; set; } = string.Empty;
    public string SessionId { get; set; } = string.Empty;
    public QueryOptions Options { get; set; } = new();
}

public class AdvancedStreamingRequest
{
    public string Question { get; set; } = string.Empty;
    public int MaxRows { get; set; } = 10000;
    public int TimeoutSeconds { get; set; } = 300;
    public int ChunkSize { get; set; } = 1000;
    public bool EnableProgressReporting { get; set; } = true;
}
