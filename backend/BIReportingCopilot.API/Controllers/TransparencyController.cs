using Microsoft.AspNetCore.Mvc;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Infrastructure.Transparency;
using BIReportingCopilot.Core.Interfaces.Data;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces.Transparency;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for AI transparency and explainability features
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TransparencyController : ControllerBase
{
    private readonly IPromptConstructionTracer _promptTracer;
    private readonly IBusinessContextAnalyzer _contextAnalyzer;
    private readonly IBusinessMetadataRetrievalService _metadataService;
    private readonly IContextualPromptBuilder _promptBuilder;
    private readonly IAuditService _auditService;
    private readonly ITransparencyService _transparencyService;
    private readonly ILogger<TransparencyController> _logger;

    public TransparencyController(
        IPromptConstructionTracer promptTracer,
        IBusinessContextAnalyzer contextAnalyzer,
        IBusinessMetadataRetrievalService metadataService,
        IContextualPromptBuilder promptBuilder,
        IAuditService auditService,
        ITransparencyService transparencyService,
        ILogger<TransparencyController> logger)
    {
        _promptTracer = promptTracer;
        _contextAnalyzer = contextAnalyzer;
        _metadataService = metadataService;
        _promptBuilder = promptBuilder;
        _auditService = auditService;
        _transparencyService = transparencyService;
        _logger = logger;
    }

    /// <summary>
    /// Get transparency trace for a specific trace ID
    /// </summary>
    [HttpGet("trace/{traceId}")]
    public async Task<ActionResult<TransparencyReport>> GetTransparencyTrace(string traceId)
    {
        try
        {
            _logger.LogInformation("Retrieving transparency trace for ID: {TraceId}", traceId);

            var report = await _promptTracer.GenerateTransparencyReportAsync(traceId, includeDetailedMetrics: true);
            
            if (report.ErrorMessage != null)
            {
                return NotFound(new { error = "Trace not found", traceId = traceId });
            }

            return Ok(report);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transparency trace {TraceId}", traceId);
            return StatusCode(500, new { error = "Failed to retrieve transparency trace", details = ex.Message });
        }
    }

    /// <summary>
    /// Analyze prompt construction for a user query
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<PromptConstructionTraceDto>> AnalyzePromptConstruction(
        [FromBody] AnalyzePromptRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing prompt construction for query: {Query}", request.UserQuery);

            // Step 1: Analyze business context
            var profile = await _contextAnalyzer.AnalyzeUserQuestionAsync(
                request.UserQuery, 
                request.UserId);

            // Step 2: Get relevant metadata
            var schema = await _metadataService.GetRelevantBusinessMetadataAsync(
                profile, 
                10); // Max tables for analysis

            // Step 3: Build prompt to get build result
            var prompt = await _promptBuilder.BuildBusinessAwarePromptAsync(
                request.UserQuery, 
                profile, 
                schema);

            // Create a mock ProgressiveBuildResult for tracing
            var buildResult = new BIReportingCopilot.Infrastructure.BusinessContext.Enhanced.ProgressiveBuildResult
            {
                FinalPrompt = prompt,
                BuildSteps = new List<BIReportingCopilot.Infrastructure.BusinessContext.Enhanced.ProgressiveBuildStep>
                {
                    new BIReportingCopilot.Infrastructure.BusinessContext.Enhanced.ProgressiveBuildStep
                    {
                        StepName = "Context Analysis",
                        Description = $"Analyzed intent: {profile.Intent.Type}",
                        Timestamp = DateTime.UtcNow
                    },
                    new BIReportingCopilot.Infrastructure.BusinessContext.Enhanced.ProgressiveBuildStep
                    {
                        StepName = "Schema Integration",
                        Description = $"Added {schema.RelevantTables.Count} relevant tables",
                        Timestamp = DateTime.UtcNow
                    }
                }
            };

            // Step 4: Trace prompt construction
            var trace = await _promptTracer.TracePromptConstructionAsync(
                request.UserQuery, 
                profile, 
                buildResult);

            // Log the analysis for audit
            await _auditService.LogAsync(
                "PromptAnalysis", 
                request.UserId ?? "anonymous", 
                "PromptTrace", 
                trace.TraceId,
                new { userQuery = request.UserQuery, confidence = trace.OverallConfidence });

            return Ok(trace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing prompt construction");
            return StatusCode(500, new { error = "Failed to analyze prompt construction", details = ex.Message });
        }
    }

    /// <summary>
    /// Get confidence breakdown for a specific analysis
    /// </summary>
    [HttpGet("confidence/{analysisId}")]
    public async Task<ActionResult<ConfidenceBreakdown>> GetConfidenceBreakdown(string analysisId)
    {
        try
        {
            _logger.LogInformation("Retrieving confidence breakdown for analysis: {AnalysisId}", analysisId);

            var breakdown = await _transparencyService.GetConfidenceBreakdownAsync(analysisId);
            return Ok(breakdown);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Analysis {AnalysisId} not found", analysisId);
            return NotFound(new { error = "Analysis not found", analysisId = analysisId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving confidence breakdown for {AnalysisId}", analysisId);
            return StatusCode(500, new { error = "Failed to retrieve confidence breakdown", details = ex.Message });
        }
    }

    /// <summary>
    /// Get alternative options for a specific trace
    /// </summary>
    [HttpGet("alternatives/{traceId}")]
    public async Task<ActionResult<List<AlternativeOptionDto>>> GetAlternativeOptions(string traceId)
    {
        try
        {
            _logger.LogInformation("Retrieving alternative options for trace: {TraceId}", traceId);

            var alternatives = await _transparencyService.GetAlternativeOptionsAsync(traceId);
            return Ok(alternatives);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving alternative options for {TraceId}", traceId);
            return StatusCode(500, new { error = "Failed to retrieve alternative options", details = ex.Message });
        }
    }

    /// <summary>
    /// Get optimization suggestions for a prompt
    /// </summary>
    [HttpPost("optimize")]
    public async Task<ActionResult<OptimizationSuggestionDto[]>> GetOptimizationSuggestions(
        [FromBody] OptimizePromptRequest request)
    {
        try
        {
            _logger.LogInformation("Generating optimization suggestions for query: {Query}", request.UserQuery);

            var suggestions = await _transparencyService.GetOptimizationSuggestionsAsync(request.UserQuery, request.TraceId);
            return Ok(suggestions.ToArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating optimization suggestions");
            return StatusCode(500, new { error = "Failed to generate optimization suggestions", details = ex.Message });
        }
    }

    /// <summary>
    /// Get transparency metrics and analytics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<TransparencyMetricsDto>> GetTransparencyMetrics(
        [FromQuery] string? userId = null,
        [FromQuery] int days = 7)
    {
        try
        {
            _logger.LogInformation("Retrieving transparency metrics for user: {UserId}, days: {Days}", userId, days);

            var metrics = await _transparencyService.GetTransparencyMetricsAsync(userId, days);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transparency metrics");
            return StatusCode(500, new { error = "Failed to retrieve transparency metrics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get dashboard-specific transparency metrics
    /// </summary>
    [HttpGet("metrics/dashboard")]
    public async Task<ActionResult<TransparencyDashboardMetricsDto>> GetDashboardMetrics(
        [FromQuery] int days = 30)
    {
        try
        {
            _logger.LogInformation("Retrieving dashboard transparency metrics for {Days} days", days);

            var metrics = await _transparencyService.GetDashboardMetricsAsync(days);
            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving dashboard transparency metrics");
            return StatusCode(500, new { error = "Failed to retrieve dashboard metrics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get transparency settings for the current user
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<TransparencySettingsDto>> GetTransparencySettings()
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("Retrieving transparency settings for user: {UserId}", userId);

            var settings = await _transparencyService.GetTransparencySettingsAsync(userId);
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transparency settings");
            return StatusCode(500, new { error = "Failed to retrieve transparency settings", details = ex.Message });
        }
    }

    /// <summary>
    /// Update transparency settings for the current user
    /// </summary>
    [HttpPut("settings")]
    public async Task<ActionResult> UpdateTransparencySettings([FromBody] TransparencySettingsDto settings)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("Updating transparency settings for user: {UserId}", userId);

            await _transparencyService.UpdateTransparencySettingsAsync(userId, settings);
            return Ok(new { message = "Settings updated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating transparency settings");
            return StatusCode(500, new { error = "Failed to update transparency settings", details = ex.Message });
        }
    }

    /// <summary>
    /// Export transparency data
    /// </summary>
    [HttpPost("export")]
    public async Task<ActionResult> ExportTransparencyData([FromBody] ExportTransparencyRequest request)
    {
        try
        {
            _logger.LogInformation("Exporting transparency data in format: {Format}", request.Format);

            var data = await _transparencyService.ExportTransparencyDataAsync(request);

            var contentType = request.Format.ToLower() switch
            {
                "json" => "application/json",
                "csv" => "text/csv",
                "excel" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                _ => "application/octet-stream"
            };

            var fileName = $"transparency-data-{DateTime.UtcNow:yyyy-MM-dd}.{request.Format.ToLower()}";

            return File(data, contentType, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting transparency data");
            return StatusCode(500, new { error = "Failed to export transparency data", details = ex.Message });
        }
    }

    /// <summary>
    /// Get recent transparency traces
    /// </summary>
    [HttpGet("traces/recent")]
    public async Task<ActionResult<List<TransparencyTraceDto>>> GetRecentTraces(
        [FromQuery] string? userId = null,
        [FromQuery] int limit = 10)
    {
        try
        {
            _logger.LogInformation("Retrieving recent transparency traces for user: {UserId}, limit: {Limit}", userId, limit);

            var traces = await _transparencyService.GetRecentTracesAsync(userId, limit);
            return Ok(traces);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving recent transparency traces");
            return StatusCode(500, new { error = "Failed to retrieve recent traces", details = ex.Message });
        }
    }

    /// <summary>
    /// Get detailed trace information
    /// </summary>
    [HttpGet("traces/{traceId}/detail")]
    public async Task<ActionResult<TransparencyTraceDetailDto>> GetTraceDetail(string traceId)
    {
        try
        {
            _logger.LogInformation("Retrieving detailed trace information for: {TraceId}", traceId);

            var traceDetail = await _transparencyService.GetTraceDetailAsync(traceId);
            return Ok(traceDetail);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Trace {TraceId} not found", traceId);
            return NotFound(new { error = "Trace not found", traceId = traceId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving trace detail for {TraceId}", traceId);
            return StatusCode(500, new { error = "Failed to retrieve trace detail", details = ex.Message });
        }
    }

    /// <summary>
    /// Get confidence trends over time
    /// </summary>
    [HttpGet("analytics/confidence-trends")]
    public async Task<ActionResult<List<ConfidenceTrendDto>>> GetConfidenceTrends(
        [FromQuery] string? userId = null,
        [FromQuery] int days = 30)
    {
        try
        {
            _logger.LogInformation("Retrieving confidence trends for user: {UserId}, days: {Days}", userId, days);

            var trends = await _transparencyService.GetConfidenceTrendsAsync(userId, days);
            return Ok(trends);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving confidence trends");
            return StatusCode(500, new { error = "Failed to retrieve confidence trends", details = ex.Message });
        }
    }

    /// <summary>
    /// Get token usage analytics
    /// </summary>
    [HttpGet("analytics/token-usage")]
    public async Task<ActionResult<TokenUsageAnalyticsDto>> GetTokenUsageAnalytics(
        [FromQuery] string? userId = null,
        [FromQuery] int days = 30)
    {
        try
        {
            _logger.LogInformation("Retrieving token usage analytics for user: {UserId}, days: {Days}", userId, days);

            var analytics = await _transparencyService.GetTokenUsageAnalyticsAsync(userId, days);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving token usage analytics");
            return StatusCode(500, new { error = "Failed to retrieve token usage analytics", details = ex.Message });
        }
    }

    /// <summary>
    /// Get performance metrics for transparency operations
    /// </summary>
    [HttpGet("analytics/performance")]
    public async Task<ActionResult<TransparencyPerformanceDto>> GetPerformanceMetrics(
        [FromQuery] int days = 7)
    {
        try
        {
            _logger.LogInformation("Retrieving transparency performance metrics for {Days} days", days);

            var performance = await _transparencyService.GetPerformanceMetricsAsync(days);
            return Ok(performance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transparency performance metrics");
            return StatusCode(500, new { error = "Failed to retrieve performance metrics", details = ex.Message });
        }
    }
}
