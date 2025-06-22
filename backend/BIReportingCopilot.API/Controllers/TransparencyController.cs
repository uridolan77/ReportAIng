using Microsoft.AspNetCore.Mvc;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Infrastructure.Transparency;
using BIReportingCopilot.Core.Interfaces.Data;
using BIReportingCopilot.Core.DTOs;

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
    private readonly ILogger<TransparencyController> _logger;

    public TransparencyController(
        IPromptConstructionTracer promptTracer,
        IBusinessContextAnalyzer contextAnalyzer,
        IBusinessMetadataRetrievalService metadataService,
        IContextualPromptBuilder promptBuilder,
        IAuditService auditService,
        ILogger<TransparencyController> logger)
    {
        _promptTracer = promptTracer;
        _contextAnalyzer = contextAnalyzer;
        _metadataService = metadataService;
        _promptBuilder = promptBuilder;
        _auditService = auditService;
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

            // For now, create a mock confidence breakdown
            // In a full implementation, this would retrieve stored analysis data
            var breakdown = new ConfidenceBreakdown
            {
                AnalysisId = analysisId,
                OverallConfidence = 0.85,
                FactorBreakdown = new Dictionary<string, double>
                {
                    ["Intent Classification"] = 0.92,
                    ["Entity Recognition"] = 0.78,
                    ["Schema Relevance"] = 0.85,
                    ["Template Selection"] = 0.88
                },
                ConfidenceFactors = new List<ConfidenceFactor>
                {
                    new ConfidenceFactor
                    {
                        FactorName = "Intent Classification",
                        Score = 0.92,
                        Weight = 0.3,
                        Description = "High confidence in understanding user intent"
                    },
                    new ConfidenceFactor
                    {
                        FactorName = "Entity Recognition", 
                        Score = 0.78,
                        Weight = 0.25,
                        Description = "Good entity extraction with some ambiguity"
                    }
                },
                Timestamp = DateTime.UtcNow
            };

            return Ok(breakdown);
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

            // For now, create mock alternative options
            // In a full implementation, this would retrieve stored alternatives
            var alternatives = new List<AlternativeOptionDto>
            {
                new AlternativeOptionDto
                {
                    OptionId = Guid.NewGuid().ToString(),
                    Type = "Template",
                    Description = "Use analytical template instead of reporting template",
                    Score = 0.82,
                    Rationale = "Better suited for complex analytical queries",
                    EstimatedImprovement = 15.0
                },
                new AlternativeOptionDto
                {
                    OptionId = Guid.NewGuid().ToString(),
                    Type = "Context",
                    Description = "Include additional business rules context",
                    Score = 0.75,
                    Rationale = "May improve accuracy for domain-specific queries",
                    EstimatedImprovement = 8.0
                }
            };

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

            // For now, create mock optimization suggestions
            // In a full implementation, this would use the PromptOptimizationEngine
            var suggestions = new List<OptimizationSuggestionDto>
            {
                new OptimizationSuggestionDto
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    Type = "Token Optimization",
                    Title = "Reduce context verbosity",
                    Description = "Remove redundant schema information to save tokens",
                    Priority = "Medium",
                    EstimatedTokenSaving = 150,
                    EstimatedPerformanceGain = 12.0,
                    Implementation = "Filter out less relevant table columns"
                },
                new OptimizationSuggestionDto
                {
                    SuggestionId = Guid.NewGuid().ToString(),
                    Type = "Accuracy Improvement",
                    Title = "Add domain-specific examples",
                    Description = "Include more relevant query examples for better context",
                    Priority = "High",
                    EstimatedTokenSaving = -50, // Uses more tokens
                    EstimatedPerformanceGain = 25.0,
                    Implementation = "Add 2-3 similar query examples to the prompt"
                }
            };

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

            // For now, create mock metrics
            // In a full implementation, this would aggregate real data
            var metrics = new TransparencyMetricsDto
            {
                TotalAnalyses = 156,
                AverageConfidence = 0.84,
                ConfidenceDistribution = new Dictionary<string, int>
                {
                    ["High (>0.8)"] = 98,
                    ["Medium (0.6-0.8)"] = 45,
                    ["Low (<0.6)"] = 13
                },
                TopIntentTypes = new Dictionary<string, int>
                {
                    ["Analytical"] = 67,
                    ["Reporting"] = 45,
                    ["Exploratory"] = 32,
                    ["Comparative"] = 12
                },
                OptimizationImpact = new Dictionary<string, double>
                {
                    ["Token Savings"] = 23.5,
                    ["Performance Improvement"] = 18.2,
                    ["Accuracy Gain"] = 15.8
                },
                TimeRange = new { from = DateTime.UtcNow.AddDays(-days), to = DateTime.UtcNow }
            };

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving transparency metrics");
            return StatusCode(500, new { error = "Failed to retrieve transparency metrics", details = ex.Message });
        }
    }
}
