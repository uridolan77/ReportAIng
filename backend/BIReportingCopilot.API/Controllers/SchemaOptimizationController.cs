using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Queries;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Schema Optimization controller
/// Provides AI-powered database optimization recommendations and SQL improvements
/// </summary>
[ApiController]
[Route("api/schema-optimization")]
[Authorize]
public class SchemaOptimizationController : ControllerBase
{
    private readonly ILogger<SchemaOptimizationController> _logger;
    private readonly IMediator _mediator;

    public SchemaOptimizationController(
        ILogger<SchemaOptimizationController> logger,
        IMediator mediator)
    {
        _logger = logger;
        _mediator = mediator;
    }

    /// <summary>
    /// Analyze query performance and suggest optimizations
    /// </summary>
    [HttpPost("analyze-query")]
    public async Task<IActionResult> AnalyzeQueryPerformance([FromBody] AnalyzeQueryRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Sql))
            {
                return BadRequest(new { success = false, error = "SQL query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("⚡ Query performance analysis requested by user {UserId}", userId);

            var command = new AnalyzeSchemaOptimizationCommand
            {
                Sql = request.Sql,
                Schema = request.Schema,
                Metrics = request.Metrics
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("⚡ Query performance analysis completed - Improvement Score: {Score:P2}",
                result.ImprovementScore);

            return Ok(new
            {
                success = true,
                data = result,
                metadata = new
                {
                    improvement_score = result.ImprovementScore,
                    suggestions_count = result.Suggestions.Count,
                    index_suggestions_count = result.IndexSuggestions.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in query performance analysis");
            return StatusCode(500, new { success = false, error = "Internal server error during performance analysis" });
        }
    }

    /// <summary>
    /// Optimize SQL query for better performance
    /// </summary>
    [HttpPost("optimize-sql")]
    public async Task<IActionResult> OptimizeSQL([FromBody] OptimizeSQLRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.OriginalSql))
            {
                return BadRequest(new { success = false, error = "Original SQL is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("⚡ SQL optimization requested by user {UserId}", userId);

            var command = new OptimizeSqlCommand
            {
                OriginalSql = request.OriginalSql,
                Schema = request.Schema,
                Goals = new OptimizationGoals
                {
                    Goals = request.Goals ?? new List<OptimizationGoal> { OptimizationGoal.Performance }
                }
            };

            var result = await _mediator.Send(command);

            _logger.LogInformation("⚡ SQL optimization completed - Confidence: {Confidence:P2}",
                result.ConfidenceScore);

            return Ok(new
            {
                success = true,
                data = result,
                metadata = new
                {
                    confidence_score = result.ConfidenceScore,
                    optimization_applied = result.ConfidenceScore > 0.7,
                    improvements_count = result.Improvements.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SQL optimization");
            return StatusCode(500, new { success = false, error = "Internal server error during SQL optimization" });
        }
    }

    /// <summary>
    /// Generate index suggestions for improved query performance
    /// </summary>
    [HttpPost("suggest-indexes")]
    public async Task<IActionResult> SuggestIndexes([FromBody] IndexSuggestionRequest request)
    {
        try
        {
            if (request.Schema == null)
            {
                return BadRequest(new { success = false, error = "Schema metadata is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📊 Index suggestions requested by user {UserId}", userId);

            var command = new GenerateIndexSuggestionsCommand
            {
                Schema = request.Schema,
                QueryPatterns = request.QueryPatterns ?? new List<string>(),
                PerformanceGoals = request.PerformanceGoals ?? new List<PerformanceGoal>()
            };

            var suggestions = await _mediator.Send(command);

            _logger.LogInformation("📊 Index suggestions generated - Count: {Count}", suggestions.Count);

            return Ok(new
            {
                success = true,
                data = suggestions,
                metadata = new
                {
                    suggestions_count = suggestions.Count,
                    high_priority_count = suggestions.Count(s => s.Priority == IndexPriority.High)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating index suggestions");
            return StatusCode(500, new { success = false, error = "Internal server error during index suggestion generation" });
        }
    }

    /// <summary>
    /// Analyze overall schema health and suggest improvements
    /// </summary>
    [HttpPost("analyze-schema-health")]
    public async Task<IActionResult> AnalyzeSchemaHealth([FromBody] SchemaHealthRequest request)
    {
        try
        {
            if (request.Schema == null)
            {
                return BadRequest(new { success = false, error = "Schema metadata is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🏥 Schema health analysis requested by user {UserId}", userId);

            var command = new AnalyzeSchemaHealthCommand
            {
                Schema = request.Schema,
                IncludePerformanceAnalysis = request.IncludePerformanceAnalysis,
                IncludeSecurityAnalysis = request.IncludeSecurityAnalysis
            };

            var analysis = await _mediator.Send(command);

            _logger.LogInformation("🏥 Schema health analysis completed - Health Score: {Score:P2}",
                analysis.OverallHealthScore);

            return Ok(new
            {
                success = true,
                data = analysis,
                metadata = new
                {
                    health_score = analysis.OverallHealthScore,
                    issues_count = analysis.Issues.Count,
                    recommendations_count = analysis.Recommendations.Count,
                    critical_issues = analysis.Issues.Count(i => i.Severity == IssueSeverity.Critical)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in schema health analysis");
            return StatusCode(500, new { success = false, error = "Internal server error during schema health analysis" });
        }
    }

    /// <summary>
    /// Get schema optimization metrics and statistics
    /// </summary>
    [HttpGet("metrics")]
    public async Task<IActionResult> GetOptimizationMetrics([FromQuery] int? days = 30)
    {
        try
        {
            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("📊 Schema optimization metrics requested by user {UserId}", userId);

            var query = new GetSchemaOptimizationMetricsQuery
            {
                TimeWindow = TimeSpan.FromDays(days ?? 30)
            };

            var metrics = await _mediator.Send(query);

            _logger.LogInformation("📊 Schema optimization metrics retrieved - Optimizations: {Count}, Avg Improvement: {Improvement:P2}",
                metrics.TotalOptimizations, metrics.AverageImprovementScore);

            return Ok(new
            {
                success = true,
                data = metrics,
                metadata = new
                {
                    time_window_days = days ?? 30,
                    generated_at = DateTime.UtcNow
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting optimization metrics");
            return StatusCode(500, new { success = false, error = "Internal server error getting optimization metrics" });
        }
    }

    /// <summary>
    /// Get execution plan analysis for a query
    /// </summary>
    [HttpPost("analyze-execution-plan")]
    public async Task<IActionResult> AnalyzeExecutionPlan([FromBody] ExecutionPlanRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Sql))
            {
                return BadRequest(new { success = false, error = "SQL query is required" });
            }

            var userId = User.Identity?.Name ?? "anonymous";
            _logger.LogInformation("🔍 Execution plan analysis requested by user {UserId}", userId);

            var command = new AnalyzeExecutionPlanQuery
            {
                Sql = request.Sql,
                Schema = request.Schema
            };

            var analysis = await _mediator.Send(command);

            _logger.LogInformation("🔍 Execution plan analysis completed - Complexity Score: {Score:P2}",
                analysis.ComplexityScore);

            return Ok(new
            {
                success = true,
                data = analysis,
                metadata = new
                {
                    complexity_score = analysis.ComplexityScore,
                    bottlenecks_count = analysis.Bottlenecks.Count,
                    optimization_opportunities = analysis.OptimizationOpportunities.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in execution plan analysis");
            return StatusCode(500, new { success = false, error = "Internal server error during execution plan analysis" });
        }
    }
}

// Request models
public class AnalyzeQueryRequest
{
    public string Sql { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
    public QueryExecutionMetrics? Metrics { get; set; }
}

public class OptimizeSQLRequest
{
    public string OriginalSql { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
    public List<OptimizationGoal>? Goals { get; set; }
}

public class IndexSuggestionRequest
{
    public SchemaMetadata Schema { get; set; } = new();
    public List<string>? QueryPatterns { get; set; }
    public List<PerformanceGoal>? PerformanceGoals { get; set; }
}

public class SchemaHealthRequest
{
    public SchemaMetadata Schema { get; set; } = new();
    public bool IncludePerformanceAnalysis { get; set; } = true;
    public bool IncludeSecurityAnalysis { get; set; } = true;
}

public class ExecutionPlanRequest
{
    public string Sql { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
}
