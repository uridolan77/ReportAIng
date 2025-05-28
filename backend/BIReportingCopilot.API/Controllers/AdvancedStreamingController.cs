using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Constants;
using CoreModels = BIReportingCopilot.Core.Models;
using System.Text.Json;
using System.Text;
using System.Runtime.CompilerServices;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Advanced streaming controller with sophisticated AI capabilities
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AdvancedStreamingController : ControllerBase
{
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;
    private readonly IAuditService _auditService;
    private readonly ILogger<AdvancedStreamingController> _logger;

    public AdvancedStreamingController(
        IAIService aiService,
        ISchemaService schemaService,
        IAuditService auditService,
        ILogger<AdvancedStreamingController> logger)
    {
        _aiService = aiService;
        _schemaService = schemaService;
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Generate SQL with streaming response and advanced prompt engineering
    /// </summary>
    [HttpPost("sql/stream")]
    public async IAsyncEnumerable<string> StreamSQLGeneration(
        [FromBody] AdvancedSQLRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";
        var sessionId = HttpContext.TraceIdentifier;

        _logger.LogInformation("Starting streaming SQL generation for user {UserId}", userId);

        // Get schema metadata if requested
        CoreModels.SchemaMetadata? schema = null;
        QueryContext? context = null;
        IAsyncEnumerable<StreamingResponse>? streamingResults = null;
        string? errorMessage = null;

        try
        {
            if (request.IncludeSchema)
            {
                schema = await _schemaService.GetSchemaMetadataAsync();
            }

            // Build query context
            context = new QueryContext
            {
                UserId = userId,
                SessionId = sessionId,
                BusinessDomain = request.BusinessDomain,
                PreferredComplexity = request.Complexity,
                PreviousQueries = request.PreviousQueries ?? new List<string>(),
                UserPreferences = request.UserPreferences ?? new Dictionary<string, object>()
            };

            streamingResults = _aiService.GenerateSQLStreamAsync(
                request.Prompt, schema, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming SQL generation for user {UserId}", userId);
            errorMessage = "An error occurred while generating SQL";
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            var errorResponse = new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = errorMessage,
                IsComplete = true
            };

            var jsonError = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            yield return $"data: {jsonError}\n\n";
            yield break;
        }

        if (streamingResults != null)
        {
            await foreach (var response in streamingResults)
            {
                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                yield return $"data: {jsonResponse}\n\n";

                // Log completion
                if (response.IsComplete)
                {
                    try
                    {
                        await _auditService.LogAsync(
                            ApplicationConstants.AuditActions.SQLGenerated,
                            userId,
                            ApplicationConstants.EntityTypes.Query,
                            sessionId,
                            new { Prompt = request.Prompt, GeneratedSQL = response.Content });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to log SQL generation audit for user {UserId}", userId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate insights with streaming response
    /// </summary>
    [HttpPost("insights/stream")]
    public async IAsyncEnumerable<string> StreamInsightGeneration(
        [FromBody] AdvancedInsightRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";

        _logger.LogInformation("Starting streaming insight generation for user {UserId}", userId);

        AnalysisContext? analysisContext = null;
        IAsyncEnumerable<StreamingResponse>? streamingResults = null;
        string? errorMessage = null;

        try
        {
            analysisContext = new AnalysisContext
            {
                BusinessGoal = request.BusinessGoal,
                TimeFrame = request.TimeFrame,
                KeyMetrics = request.KeyMetrics ?? new List<string>(),
                Type = request.AnalysisType
            };

            streamingResults = _aiService.GenerateInsightStreamAsync(
                request.Query, request.Data, analysisContext, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming insight generation for user {UserId}", userId);
            errorMessage = "An error occurred while generating insights";
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            var errorResponse = new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = errorMessage,
                IsComplete = true
            };

            var jsonError = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            yield return $"data: {jsonError}\n\n";
            yield break;
        }

        if (streamingResults != null)
        {
            await foreach (var response in streamingResults)
            {
                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                yield return $"data: {jsonResponse}\n\n";

                if (response.IsComplete)
                {
                    try
                    {
                        await _auditService.LogAsync(
                            ApplicationConstants.AuditActions.InsightGenerated,
                            userId,
                            ApplicationConstants.EntityTypes.Analysis,
                            HttpContext.TraceIdentifier,
                            new { Query = request.Query, DataRows = request.Data.Length });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to log insight generation audit for user {UserId}", userId);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Generate SQL explanation with streaming response
    /// </summary>
    [HttpPost("explain/stream")]
    public async IAsyncEnumerable<string> StreamSQLExplanation(
        [FromBody] SQLExplanationRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var userId = User.FindFirst("sub")?.Value ?? "anonymous";

        _logger.LogInformation("Starting streaming SQL explanation for user {UserId}", userId);

        IAsyncEnumerable<StreamingResponse>? streamingResults = null;
        string? errorMessage = null;

        try
        {
            streamingResults = _aiService.GenerateExplanationStreamAsync(
                request.SQL, request.Complexity, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in streaming SQL explanation for user {UserId}", userId);
            errorMessage = "An error occurred while explaining SQL";
        }

        if (!string.IsNullOrEmpty(errorMessage))
        {
            var errorResponse = new StreamingResponse
            {
                Type = StreamingResponseType.Error,
                Content = errorMessage,
                IsComplete = true
            };

            var jsonError = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            yield return $"data: {jsonError}\n\n";
            yield break;
        }

        if (streamingResults != null)
        {
            await foreach (var response in streamingResults)
            {
                var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                yield return $"data: {jsonResponse}\n\n";

                if (response.IsComplete)
                {
                    try
                    {
                        await _auditService.LogAsync(
                            ApplicationConstants.AuditActions.SQLExplained,
                            userId,
                            ApplicationConstants.EntityTypes.Query,
                            HttpContext.TraceIdentifier,
                            new { SQL = request.SQL });
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to log SQL explanation audit for user {UserId}", userId);
                    }
                }
            }
        }
    }
}

/// <summary>
/// Advanced SQL generation request
/// </summary>
public class AdvancedSQLRequest
{
    public string Prompt { get; set; } = string.Empty;
    public bool IncludeSchema { get; set; } = true;
    public string? BusinessDomain { get; set; }
    public StreamingQueryComplexity Complexity { get; set; } = StreamingQueryComplexity.Medium;
    public List<string>? PreviousQueries { get; set; }
    public Dictionary<string, object>? UserPreferences { get; set; }
}

/// <summary>
/// Advanced insight generation request
/// </summary>
public class AdvancedInsightRequest
{
    public string Query { get; set; } = string.Empty;
    public object[] Data { get; set; } = Array.Empty<object>();
    public string? BusinessGoal { get; set; }
    public string? TimeFrame { get; set; }
    public List<string>? KeyMetrics { get; set; }
    public AnalysisType AnalysisType { get; set; } = AnalysisType.Descriptive;
}

/// <summary>
/// SQL explanation request
/// </summary>
public class SQLExplanationRequest
{
    public string SQL { get; set; } = string.Empty;
    public StreamingQueryComplexity Complexity { get; set; } = StreamingQueryComplexity.Medium;
}
