using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Queries;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Advanced Natural Language Understanding controller
/// Provides deep semantic analysis, intent recognition, and context management
/// </summary>
[ApiController]
[Route("api/advanced-nlu")]
[Authorize]
public class AdvancedNLUController : ControllerBase
{
    private readonly ILogger<AdvancedNLUController> _logger;
    private readonly IMediator _mediator;

    public AdvancedNLUController(
        ILogger<AdvancedNLUController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Perform comprehensive NLU analysis on a natural language query
    /// </summary>
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeQuery([FromBody] AnalyzeNLURequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { success = false, error = "Query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🧠 Advanced NLU analysis requested by user {UserId}: {Query}", userId, request.Query);

            var command = new AnalyzeNLUCommand
            {
                NaturalLanguageQuery = request.Query,
                UserId = userId,
                Context = request.Context
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("🧠 Advanced NLU analysis completed - Confidence: {Confidence:P2}, Intent: {Intent}",
                result.ConfidenceScore, result.IntentAnalysis.PrimaryIntent);

            return Ok(new
            {
                success = true,
                data = result,
                metadata = new
                {
                    confidence = result.ConfidenceScore,
                    intent = result.IntentAnalysis.PrimaryIntent,
                    entities_count = result.EntityAnalysis.Entities.Count,
                    processing_time_ms = result.ProcessingTimeMs
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in advanced NLU analysis");
            return StatusCode(500, new { success = false, error = "Internal server error during NLU analysis" });
        }
    }

    /// <summary>
    /// Classify query intent with confidence scoring
    /// </summary>
    [HttpPost("classify-intent")]
    public async Task<IActionResult> ClassifyIntent([FromBody] ClassifyIntentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { success = false, error = "Query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🎯 Intent classification requested by user {UserId}: {Query}", userId, request.Query);

            var query = new ClassifyIntentQuery
            {
                Query = request.Query,
                UserId = userId
            };

            var result = await _mediator.Send(query);

            _logger.LogInformation("🎯 Intent classified - Primary: {Intent}, Confidence: {Confidence:P2}",
                result.PrimaryIntent, result.Confidence);

            return Ok(new
            {
                success = true,
                data = result,
                metadata = new
                {
                    primary_intent = result.PrimaryIntent,
                    confidence = result.Confidence,
                    secondary_intents = result.SecondaryIntents?.Count ?? 0
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in intent classification");
            return StatusCode(500, new { success = false, error = "Internal server error during intent classification" });
        }
    }

    /// <summary>
    /// Generate smart query suggestions based on partial input
    /// </summary>
    [HttpPost("suggestions")]
    public async Task<IActionResult> GetSmartSuggestions([FromBody] SmartSuggestionsRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PartialQuery))
            {
                return BadRequest(new { success = false, error = "Partial query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("💡 Smart suggestions requested by user {UserId}: {PartialQuery}", userId, request.PartialQuery);

            var query = new GetIntelligentSuggestionsQuery
            {
                UserId = userId,
                Schema = request.Schema,
                Context = request.Context
            };

            var suggestions = await _mediator.Send(query);

            _logger.LogInformation("💡 Smart suggestions generated - Count: {Count}", suggestions.Count);

            return Ok(new
            {
                success = true,
                data = suggestions,
                metadata = new
                {
                    suggestion_count = suggestions.Count,
                    partial_query = request.PartialQuery
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating smart suggestions");
            return StatusCode(500, new { success = false, error = "Internal server error during suggestion generation" });
        }
    }

    /// <summary>
    /// Get real-time query assistance for partial queries
    /// </summary>
    [HttpPost("assistance")]
    public async Task<IActionResult> GetQueryAssistance([FromBody] QueryAssistanceRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.PartialQuery))
            {
                return BadRequest(new { success = false, error = "Partial query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🤝 Query assistance requested by user {UserId}: {PartialQuery}", userId, request.PartialQuery);

            var query = new GetQueryAssistanceQuery
            {
                PartialQuery = request.PartialQuery,
                UserId = userId,
                Schema = request.Schema
            };

            var assistance = await _mediator.Send(query);

            _logger.LogInformation("🤝 Query assistance provided - Autocomplete: {AutocompleteCount}, Hints: {HintCount}",
                assistance.AutocompleteSuggestions.Count, assistance.PerformanceHints.Count);

            return Ok(new
            {
                success = true,
                data = assistance,
                metadata = new
                {
                    autocomplete_count = assistance.AutocompleteSuggestions.Count,
                    hint_count = assistance.PerformanceHints.Count,
                    validation_errors = assistance.ValidationErrors.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error providing query assistance");
            return StatusCode(500, new { success = false, error = "Internal server error during query assistance" });
        }
    }

    /// <summary>
    /// Get NLU processing metrics and statistics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetNLUMetrics()
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📊 NLU metrics requested by user {UserId}", userId);

            var query = new GetNLUMetricsQuery
            {
                UserId = userId,
                TimeWindow = TimeSpan.FromDays(7)
            };

            var metrics = await _mediator.Send(query);

            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    time_window_days = 7,
                    generated_at = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting NLU metrics");
            return StatusCode(500, new { success = false, error = "Internal server error getting NLU metrics" });
        }
    }
}

// Request/Response models
public class AnalyzeNLURequest
{
    public string Query { get; set; } = string.Empty;
    public NLUAnalysisContext? Context { get; set; }
}

public class ClassifyIntentRequest
{
    public string Query { get; set; } = string.Empty;
}

public class SmartSuggestionsRequest
{
    public string PartialQuery { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
    public NLUAnalysisContext? Context { get; set; }
}

public class QueryAssistanceRequest
{
    public string PartialQuery { get; set; } = string.Empty;
    public SchemaMetadata? Schema { get; set; }
}
