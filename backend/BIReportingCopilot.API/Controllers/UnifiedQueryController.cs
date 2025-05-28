using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using MediatR;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Queries;
using BIReportingCopilot.API.Hubs;
using System.Security.Claims;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Unified query controller combining standard, enhanced, and streaming query capabilities
/// </summary>
[ApiController]
[Route("api/query")]
[Authorize]
public class UnifiedQueryController : ControllerBase
{
    private readonly ILogger<UnifiedQueryController> _logger;
    private readonly IMediator _mediator;
    private readonly IQueryService _queryService;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;
    private readonly BIReportingCopilot.Infrastructure.Performance.IStreamingSqlQueryService _streamingQueryService;
    private readonly BIReportingCopilot.Infrastructure.AI.IQueryProcessor _queryProcessor;
    private readonly ISemanticAnalyzer _semanticAnalyzer;
    private readonly ISqlQueryService _sqlQueryService;
    private readonly IAuditService _auditService;
    private readonly IHubContext<QueryStatusHub> _hubContext;

    public UnifiedQueryController(
        ILogger<UnifiedQueryController> logger,
        IMediator mediator,
        IQueryService queryService,
        IAIService aiService,
        ISchemaService schemaService,
        BIReportingCopilot.Infrastructure.Performance.IStreamingSqlQueryService streamingQueryService,
        BIReportingCopilot.Infrastructure.AI.IQueryProcessor queryProcessor,
        ISemanticAnalyzer semanticAnalyzer,
        ISqlQueryService sqlQueryService,
        IAuditService auditService,
        IHubContext<QueryStatusHub> hubContext)
    {
        _logger = logger;
        _mediator = mediator;
        _queryService = queryService;
        _aiService = aiService;
        _schemaService = schemaService;
        _streamingQueryService = streamingQueryService;
        _queryProcessor = queryProcessor;
        _semanticAnalyzer = semanticAnalyzer;
        _sqlQueryService = sqlQueryService;
        _auditService = auditService;
        _hubContext = hubContext;
    }

    #region Standard Query Operations

    /// <summary>
    /// Execute a natural language query and return results with generated SQL
    /// </summary>
    /// <param name="request">The query request containing the natural language question</param>
    /// <returns>Query response with SQL, results, and visualization config</returns>
    [HttpPost("execute")]
    public async Task<ActionResult<QueryResponse>> ExecuteNaturalLanguageQuery([FromBody] QueryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Executing natural language query for user {UserId}: {Question}", userId, request.Question);

            var command = new ExecuteQueryCommand
            {
                Question = request.Question,
                SessionId = request.SessionId,
                UserId = userId,
                Options = request.Options
            };

            var response = await _mediator.Send(command);
            return response.Success ? Ok(response) : BadRequest(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing natural language query");
            return StatusCode(500, new { error = "An error occurred while processing your query" });
        }
    }

    /// <summary>
    /// Get query history for the current user
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Items per page (default: 20)</param>
    /// <returns>Paginated list of query history items</returns>
    [HttpGet("history")]
    public async Task<ActionResult<PagedResult<QueryHistoryItem>>> GetQueryHistory(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] string? searchTerm = null)
    {
        try
        {
            var query = new GetQueryHistoryQuery
            {
                UserId = GetCurrentUserId(),
                Page = page,
                PageSize = pageSize,
                StartDate = startDate,
                EndDate = endDate,
                SearchTerm = searchTerm
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving query history");
            return StatusCode(500, new { error = "An error occurred while retrieving query history" });
        }
    }

    /// <summary>
    /// Submit feedback for a specific query
    /// </summary>
    /// <param name="feedback">Feedback details</param>
    /// <returns>Success status</returns>
    [HttpPost("feedback")]
    public async Task<ActionResult> SubmitQueryFeedback([FromBody] QueryFeedback feedback)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Submitting feedback for query {QueryId} from user {UserId}", feedback.QueryId, userId);

            var success = await _queryService.SubmitFeedbackAsync(feedback, userId);

            if (success)
            {
                return Ok(new { message = "Feedback submitted successfully" });
            }
            else
            {
                return BadRequest(new { error = "Failed to submit feedback" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting query feedback");
            return StatusCode(500, new { error = "An error occurred while submitting feedback" });
        }
    }

    /// <summary>
    /// Get query suggestions based on context
    /// </summary>
    /// <param name="context">Optional context for suggestions</param>
    /// <returns>List of suggested queries</returns>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<string>>> GetQuerySuggestions([FromQuery] string? context = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting query suggestions for user {UserId}", userId);

            var suggestions = await _queryService.GetQuerySuggestionsAsync(userId, context);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query suggestions");
            return StatusCode(500, new { error = "An error occurred while getting suggestions" });
        }
    }

    #endregion

    #region Enhanced Query Operations

    /// <summary>
    /// Process a natural language query with enhanced AI capabilities
    /// </summary>
    /// <param name="request">Enhanced query request</param>
    /// <returns>Processed query with semantic analysis and optimization</returns>
    [HttpPost("enhanced")]
    public async Task<ActionResult<EnhancedQueryResponse>> ProcessEnhancedQuery([FromBody] EnhancedQueryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Processing enhanced query for user {UserId}: {Query}", userId, request.Query);

            // Process the query with enhanced AI pipeline
            var processedQuery = await _queryProcessor.ProcessQueryAsync(request.Query, userId);

            // Execute the optimized SQL if requested
            QueryResult? queryResult = null;
            if (request.ExecuteQuery)
            {
                try
                {
                    queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(processedQuery.Sql);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to execute optimized SQL, trying alternatives");

                    // Try alternative queries if main query fails
                    foreach (var alternative in processedQuery.AlternativeQueries.Take(2))
                    {
                        try
                        {
                            queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(alternative.Sql);
                            processedQuery.Sql = alternative.Sql; // Update to working SQL
                            break;
                        }
                        catch
                        {
                            continue; // Try next alternative
                        }
                    }
                }
            }

            // Perform semantic analysis if requested
            SemanticAnalysisResponse? semanticAnalysis = null;
            if (request.IncludeSemanticAnalysis)
            {
                var analysis = await _semanticAnalyzer.AnalyzeAsync(request.Query);
                semanticAnalysis = new SemanticAnalysisResponse
                {
                    Entities = analysis.Entities.Select(e => e.Text).ToList(),
                    Keywords = analysis.Keywords.ToList(),
                    Intent = analysis.Intent.ToString(),
                    Confidence = analysis.ConfidenceScore,
                    Complexity = analysis.Metadata.ContainsKey("complexity_level") ? analysis.Metadata["complexity_level"].ToString() ?? "medium" : "medium"
                };
            }

            var response = new EnhancedQueryResponse
            {
                ProcessedQuery = processedQuery,
                QueryResult = queryResult,
                SemanticAnalysis = semanticAnalysis,
                Classification = new ClassificationResponse
                {
                    Category = processedQuery.Classification.Category.ToString(),
                    Complexity = processedQuery.Classification.Complexity.ToString(),
                    EstimatedExecutionTime = processedQuery.Classification.EstimatedExecutionTime,
                    RecommendedVisualization = processedQuery.Classification.RecommendedVisualization.ToString(),
                    OptimizationSuggestions = processedQuery.Classification.OptimizationSuggestions
                },
                Alternatives = processedQuery.AlternativeQueries.Select(alt => new AlternativeQueryResponse
                {
                    Sql = alt.Sql,
                    Score = alt.Score,
                    Reasoning = alt.Reasoning,
                    Strengths = alt.Strengths,
                    Weaknesses = alt.Weaknesses
                }).ToList(),
                Success = true,
                Timestamp = DateTime.UtcNow
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing enhanced query");
            return StatusCode(500, new EnhancedQueryResponse
            {
                Success = false,
                ErrorMessage = "Error processing enhanced query",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    #endregion

    #region Streaming Query Operations

    /// <summary>
    /// Get metadata for a streaming query before execution
    /// </summary>
    [HttpPost("streaming/metadata")]
    public async Task<ActionResult<StreamingQueryMetadata>> GetStreamingQueryMetadata(
        [FromBody] StreamingQueryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
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
    [HttpPost("streaming/execute")]
    public async Task<ActionResult> ExecuteStreamingQuery([FromBody] StreamingQueryRequest request)
    {
        var queryId = Guid.NewGuid().ToString();
        var userId = GetCurrentUserId();

        try
        {
            _logger.LogInformation("Starting streaming query {QueryId} for user {UserId}: {Question}",
                queryId, userId, request.Question);

            // Send initial status
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
    /// Stream query results with advanced backpressure control
    /// </summary>
    [HttpPost("streaming/backpressure")]
    public IAsyncEnumerable<StreamingQueryChunk> StreamQueryWithBackpressure(
        [FromBody] AdvancedStreamingRequest request,
        CancellationToken cancellationToken = default)
    {
        return StreamQueryWithBackpressureInternal(request, cancellationToken);
    }

    /// <summary>
    /// Stream SQL generation with real-time AI processing
    /// </summary>
    [HttpPost("streaming/ai")]
    public IAsyncEnumerable<StreamingResponse> StreamSQLGeneration(
        [FromBody] StreamingSQLRequest request,
        CancellationToken cancellationToken = default)
    {
        return StreamSQLGenerationInternal(request, cancellationToken);
    }

    #endregion

    #region Private Helper Methods

    private async Task ExecuteStreamingQueryBackground(string queryId, string userId, string sql, QueryOptions options)
    {
        var totalRows = 0;
        var chunkCount = 0;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

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

            stopwatch.Stop();

            // Send completion notification
            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryCompleted", new
            {
                QueryId = queryId,
                TotalRows = totalRows,
                TotalChunks = chunkCount,
                TotalTimeMs = stopwatch.ElapsedMilliseconds,
                Timestamp = DateTime.UtcNow
            });

            _logger.LogInformation("Completed streaming query {QueryId} for user {UserId}: {TotalRows} rows in {TotalChunks} chunks, {TotalTime}ms",
                queryId, userId, totalRows, chunkCount, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in background streaming query {QueryId}", queryId);

            await _hubContext.Clients.User(userId).SendAsync("StreamingQueryError", new
            {
                QueryId = queryId,
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }

    private async IAsyncEnumerable<StreamingQueryChunk> StreamQueryWithBackpressureInternal(
        AdvancedStreamingRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var hasError = false;
        string sql;
        QueryOptions options;

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

            // Convert infrastructure chunk to API chunk
            yield return new StreamingQueryChunk
            {
                ChunkIndex = chunk.ChunkIndex,
                Data = chunk.Data,
                TotalRowsInChunk = chunk.TotalRowsInChunk,
                IsLastChunk = chunk.IsLastChunk,
                ProcessingTimeMs = chunk.ProcessingTimeMs,
                Timestamp = chunk.Timestamp
            };
            chunkIndex++;
        }

        _logger.LogInformation("Completed backpressure streaming query for user {UserId}, total chunks: {ChunkCount}", userId, chunkIndex);
    }

    private async IAsyncEnumerable<StreamingResponse> StreamSQLGenerationInternal(
        StreamingSQLRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        var sessionId = request.SessionId ?? Guid.NewGuid().ToString();

        // Prepare schema and context
        var (schema, context, errorResponse) = await PrepareStreamingContextAsync(request, userId, sessionId);

        if (errorResponse != null)
        {
            yield return errorResponse;
            yield break;
        }

        // Stream the SQL generation
        var streamingResults = _aiService.GenerateSQLStreamAsync(request.Prompt, schema, context, cancellationToken);

        await foreach (var response in streamingResults)
        {
            yield return response;
        }
    }

    private async Task<(SchemaMetadata? schema, BIReportingCopilot.Core.Models.QueryContext? context, StreamingResponse? errorResponse)>
        PrepareStreamingContextAsync(StreamingSQLRequest request, string userId, string sessionId)
    {
        SchemaMetadata? schema = null;

        // Get schema if requested
        if (request.IncludeSchema)
        {
            try
            {
                schema = await _schemaService.GetSchemaMetadataAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting schema for user {UserId}", userId);
                return (null, null, new StreamingResponse
                {
                    Type = StreamingResponseType.Error,
                    Content = "Failed to load database schema",
                    IsComplete = true,
                    Timestamp = DateTime.UtcNow
                });
            }
        }

        // Build query context
        var context = new BIReportingCopilot.Core.Models.QueryContext
        {
            UserId = userId,
            SessionId = sessionId,
            BusinessDomain = request.BusinessDomain,
            PreferredComplexity = (BIReportingCopilot.Core.Models.StreamingQueryComplexity)(int)request.Complexity,
            PreviousQueries = request.PreviousQueries ?? new List<string>(),
            UserPreferences = request.UserPreferences ?? new Dictionary<string, object>()
        };

        return (schema, context, null);
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

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }

    #endregion
}

#region Request/Response Models

/// <summary>
/// Request for enhanced query processing
/// </summary>
public class EnhancedQueryRequest
{
    public string Query { get; set; } = string.Empty;
    public bool ExecuteQuery { get; set; } = true;
    public bool IncludeAlternatives { get; set; } = true;
    public bool IncludeSemanticAnalysis { get; set; } = true;
}

/// <summary>
/// Response for enhanced query processing
/// </summary>
public class EnhancedQueryResponse
{
    public ProcessedQuery? ProcessedQuery { get; set; }
    public QueryResult? QueryResult { get; set; }
    public SemanticAnalysisResponse? SemanticAnalysis { get; set; }
    public ClassificationResponse? Classification { get; set; }
    public List<AlternativeQueryResponse> Alternatives { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Semantic analysis response
/// </summary>
public class SemanticAnalysisResponse
{
    public List<string> Entities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public string Intent { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Complexity { get; set; } = string.Empty;
}

/// <summary>
/// Classification response
/// </summary>
public class ClassificationResponse
{
    public string Category { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public TimeSpan EstimatedExecutionTime { get; set; }
    public string RecommendedVisualization { get; set; } = string.Empty;
    public List<string> OptimizationSuggestions { get; set; } = new();
}

/// <summary>
/// Alternative query response
/// </summary>
public class AlternativeQueryResponse
{
    public string Sql { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}

/// <summary>
/// Streaming query request
/// </summary>
public class StreamingQueryRequest
{
    public string Question { get; set; } = string.Empty;
    public QueryOptions Options { get; set; } = new();
}

/// <summary>
/// Advanced streaming request
/// </summary>
public class AdvancedStreamingRequest
{
    public string Question { get; set; } = string.Empty;
    public int MaxRows { get; set; } = 10000;
    public int TimeoutSeconds { get; set; } = 30;
    public int ChunkSize { get; set; } = 1000;
}

/// <summary>
/// Streaming SQL request
/// </summary>
public class StreamingSQLRequest
{
    public string Prompt { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public bool IncludeSchema { get; set; } = true;
    public string? BusinessDomain { get; set; }
    public QueryComplexity Complexity { get; set; } = QueryComplexity.Medium;
    public List<string>? PreviousQueries { get; set; }
    public Dictionary<string, object>? UserPreferences { get; set; }
}

/// <summary>
/// Streaming query metadata
/// </summary>
public class StreamingQueryMetadata
{
    public int EstimatedRowCount { get; set; }
    public int EstimatedChunkCount { get; set; }
    public TimeSpan EstimatedDuration { get; set; }
    public string[] ColumnNames { get; set; } = Array.Empty<string>();
    public string[] ColumnTypes { get; set; } = Array.Empty<string>();
    public bool SupportsStreaming { get; set; }
    public int RecommendedChunkSize { get; set; }
}

/// <summary>
/// Streaming query chunk
/// </summary>
public class StreamingQueryChunk
{
    public int ChunkIndex { get; set; }
    public List<Dictionary<string, object>> Data { get; set; } = new();
    public int TotalRowsInChunk { get; set; }
    public bool IsLastChunk { get; set; }
    public long ProcessingTimeMs { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

#endregion
