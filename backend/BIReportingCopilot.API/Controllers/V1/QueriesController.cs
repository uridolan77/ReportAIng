using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.API.Versioning;
using BIReportingCopilot.Infrastructure.Monitoring;
using System.Diagnostics;
using QueryRequest = BIReportingCopilot.Core.Validation.QueryRequest;
using FeedbackRequest = BIReportingCopilot.Core.Validation.FeedbackRequest;

namespace BIReportingCopilot.API.Controllers.V1;

/// <summary>
/// Version 1.0 of the Queries API - Core query generation and execution
/// </summary>
[ApiVersion("1.0")]
[Authorize]
[Tags("Queries")]
public class QueriesController : VersionedApiController
{
    private readonly IQueryService _queryService;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;
    private readonly IValidator<QueryRequest> _queryValidator;
    private readonly IValidator<FeedbackRequest> _feedbackValidator;
    private readonly IMetricsCollector _metricsCollector;
    private readonly ILogger<QueriesController> _logger;

    public QueriesController(
        IQueryService queryService,
        IAIService aiService,
        ISchemaService schemaService,
        IValidator<QueryRequest> queryValidator,
        IValidator<FeedbackRequest> feedbackValidator,
        IMetricsCollector metricsCollector,
        ILogger<QueriesController> logger)
    {
        _queryService = queryService;
        _aiService = aiService;
        _schemaService = schemaService;
        _queryValidator = queryValidator;
        _feedbackValidator = feedbackValidator;
        _metricsCollector = metricsCollector;
        _logger = logger;
    }

    /// <summary>
    /// Generate and execute a SQL query from natural language
    /// </summary>
    /// <param name="request">The query request containing natural language query and options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Query results with generated SQL and data</returns>
    [HttpPost("execute")]
    [ProducesResponseType(typeof(ApiResponse<QueryResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    [ProducesResponseType(typeof(ApiResponse<object>), 401)]
    [ProducesResponseType(typeof(ApiResponse<object>), 500)]
    public async Task<IActionResult> ExecuteQuery(
        [FromBody] QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = User.Identity?.Name ?? "Unknown";

        try
        {
            // Validate request
            var validationResult = await _queryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return ValidationError<QueryResponse>(errors);
            }

            _logger.LogInformation("Executing query for user {UserId}: {Query}", userId, request.NaturalLanguageQuery);

            // Generate SQL
            var sql = await _aiService.GenerateSQLAsync(request.NaturalLanguageQuery, cancellationToken);

            // Calculate confidence score
            var confidence = await _aiService.CalculateConfidenceScoreAsync(request.NaturalLanguageQuery, sql);

            // Execute query
            var queryRequest = new BIReportingCopilot.Core.Models.QueryRequest
            {
                Question = request.NaturalLanguageQuery,
                SessionId = Guid.NewGuid().ToString(),
                Options = new QueryOptions
                {
                    MaxRows = request.MaxRows ?? 1000,
                    TimeoutSeconds = request.TimeoutSeconds ?? 30,
                    EnableCache = request.CacheResults
                }
            };
            var queryResponse = await _queryService.ProcessQueryAsync(queryRequest, userId, cancellationToken);
            queryResponse.Confidence = confidence;

            // Generate explanation if requested
            if (request.IncludeExplanation)
            {
                // Store explanation in the Result metadata or create a custom property
                var explanation = await _aiService.GenerateInsightAsync(sql, queryResponse.Result?.Data ?? new object[0]);
                // Note: QueryResponse doesn't have Explanation property, so we could add it to suggestions or metadata
                var currentSuggestions = queryResponse.Suggestions?.ToList() ?? new List<string>();
                currentSuggestions.Insert(0, $"Explanation: {explanation}");
                queryResponse.Suggestions = currentSuggestions.ToArray();
            }

            // Record metrics
            var rowCount = queryResponse.Result?.Metadata?.RowCount ?? 0;
            _metricsCollector.RecordQueryExecution("v1", stopwatch.ElapsedMilliseconds, queryResponse.Success, rowCount);

            var metadata = new ApiMetadata
            {
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds,
                Cache = new CacheInfo { FromCache = false }
            };

            return Ok(queryResponse, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query for user {UserId}: {Query}", userId, request.NaturalLanguageQuery);
            _metricsCollector.IncrementCounter("query_errors_total", new() { { "version", "v1" }, { "user_id", userId } });
            throw; // Let global exception handler deal with it
        }
    }

    /// <summary>
    /// Generate SQL without executing the query
    /// </summary>
    /// <param name="request">The query request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Generated SQL with confidence score</returns>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(ApiResponse<SqlGenerationResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> GenerateSQL(
        [FromBody] QueryRequest request,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = User.Identity?.Name ?? "Unknown";

        try
        {
            // Validate request
            var validationResult = await _queryValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return ValidationError<SqlGenerationResponse>(errors);
            }

            _logger.LogInformation("Generating SQL for user {UserId}: {Query}", userId, request.NaturalLanguageQuery);

            // Generate SQL
            var sql = await _aiService.GenerateSQLAsync(request.NaturalLanguageQuery, cancellationToken);

            // Calculate confidence score
            var confidence = await _aiService.CalculateConfidenceScoreAsync(request.NaturalLanguageQuery, sql);

            var response = new SqlGenerationResponse
            {
                GeneratedSQL = sql,
                ConfidenceScore = confidence,
                NaturalLanguageQuery = request.NaturalLanguageQuery
            };

            var metadata = new ApiMetadata
            {
                ProcessingTimeMs = stopwatch.ElapsedMilliseconds
            };

            return Ok(response, metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SQL for user {UserId}: {Query}", userId, request.NaturalLanguageQuery);
            throw;
        }
    }

    /// <summary>
    /// Get query suggestions based on context
    /// </summary>
    /// <param name="context">Context for generating suggestions</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of suggested queries</returns>
    [HttpGet("suggestions")]
    [ProducesResponseType(typeof(ApiResponse<QuerySuggestionsResponse>), 200)]
    public async Task<IActionResult> GetQuerySuggestions(
        [FromQuery] string? context = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var schema = await _schemaService.GetSchemaAsync();
            var suggestions = await _aiService.GenerateQuerySuggestionsAsync(context ?? "general", schema);

            var response = new QuerySuggestionsResponse
            {
                Suggestions = suggestions,
                Context = context ?? "general"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query suggestions");
            throw;
        }
    }

    /// <summary>
    /// Submit feedback for a query
    /// </summary>
    /// <param name="request">Feedback request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success response</returns>
    [HttpPost("feedback")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> SubmitFeedback(
        [FromBody] FeedbackRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = User.Identity?.Name ?? "Unknown";

        try
        {
            // Validate request
            var validationResult = await _feedbackValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

                return ValidationError<object>(errors);
            }

            _logger.LogInformation("Received feedback from user {UserId} for query {QueryId}: Rating {Rating}",
                userId, request.QueryId, request.Rating);

            // Process feedback (this would typically involve storing it and updating AI models)
            var feedback = new QueryFeedback
            {
                QueryId = request.QueryId,
                Feedback = request.IsHelpful ? "positive" : "negative",
                Comments = request.Comments
            };

            // Record metrics
            _metricsCollector.IncrementCounter("feedback_received_total", new()
            {
                { "version", "v1" },
                { "user_id", userId },
                { "rating", request.Rating.ToString() }
            });

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing feedback from user {UserId}", userId);
            throw;
        }
    }
}

/// <summary>
/// Response model for SQL generation
/// </summary>
public class SqlGenerationResponse
{
    public string GeneratedSQL { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public string NaturalLanguageQuery { get; set; } = string.Empty;
}

/// <summary>
/// Response model for query suggestions
/// </summary>
public class QuerySuggestionsResponse
{
    public string[] Suggestions { get; set; } = Array.Empty<string>();
    public string Context { get; set; } = string.Empty;
}
