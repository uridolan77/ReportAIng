using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Validation;
using BIReportingCopilot.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Enhanced Validation Controller - Phase 3 enhanced SQL validation pipeline
/// </summary>
[ApiController]
[Route("api/validation")]
[Authorize]
public class EnhancedValidationController : ControllerBase
{
    private readonly ILogger<EnhancedValidationController> _logger;
    private readonly IEnhancedSemanticSqlValidator _enhancedValidator;
    private readonly IDryRunExecutionService _dryRunService;
    private readonly ISqlSelfCorrectionService _selfCorrectionService;

    public EnhancedValidationController(
        ILogger<EnhancedValidationController> logger,
        IEnhancedSemanticSqlValidator enhancedValidator,
        IDryRunExecutionService dryRunService,
        ISqlSelfCorrectionService selfCorrectionService)
    {
        _logger = logger;
        _enhancedValidator = enhancedValidator;
        _dryRunService = dryRunService;
        _selfCorrectionService = selfCorrectionService;
    }

    /// <summary>
    /// Comprehensive SQL validation with semantic analysis and self-correction
    /// </summary>
    /// <param name="request">Enhanced validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced validation response</returns>
    [HttpPost("comprehensive")]
    [ProducesResponseType(typeof(EnhancedValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EnhancedValidationResponse>> ValidateComprehensive(
        [FromBody] EnhancedValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Starting comprehensive SQL validation for query: {Query}", request.OriginalQuery);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _enhancedValidator.ValidateWithLevelAsync(request, cancellationToken);

            _logger.LogInformation("✅ Comprehensive validation completed: {Success} (Score: {Score:F2})", 
                response.Success, response.ValidationResult?.OverallScore ?? 0);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for comprehensive validation");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in comprehensive SQL validation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "An error occurred during comprehensive SQL validation",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Semantic validation focusing on business intent alignment
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Semantic validation result</returns>
    [HttpPost("semantic")]
    [ProducesResponseType(typeof(SemanticValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SemanticValidationResult>> ValidateSemantic(
        [FromBody] string sql,
        [FromQuery, Required] string originalQuery,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🧠 Starting semantic validation");

            if (string.IsNullOrWhiteSpace(sql) || string.IsNullOrWhiteSpace(originalQuery))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Input",
                    Detail = "Both SQL and original query are required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await _enhancedValidator.ValidateSemanticAlignmentAsync(sql, originalQuery, cancellationToken);

            _logger.LogInformation("✅ Semantic validation completed: {Valid} (Score: {Score:F2})", 
                result.IsValid, result.AlignmentScore);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in semantic validation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Semantic Validation Error",
                Detail = "An error occurred during semantic validation",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Business logic validation for compliance and rules
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business logic validation result</returns>
    [HttpPost("business-logic")]
    [ProducesResponseType(typeof(BusinessLogicValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BusinessLogicValidationResult>> ValidateBusinessLogic(
        [FromBody] string sql,
        [FromQuery, Required] string originalQuery,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("💼 Starting business logic validation");

            if (string.IsNullOrWhiteSpace(sql) || string.IsNullOrWhiteSpace(originalQuery))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Input",
                    Detail = "Both SQL and original query are required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await _enhancedValidator.ValidateBusinessLogicAsync(sql, originalQuery, cancellationToken);

            _logger.LogInformation("✅ Business logic validation completed: {Valid} (Score: {Score:F2})", 
                result.IsValid, result.BusinessLogicScore);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in business logic validation");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Business Logic Validation Error",
                Detail = "An error occurred during business logic validation",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Dry-run execution for SQL validation
    /// </summary>
    /// <param name="request">Dry-run execution request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dry-run execution result</returns>
    [HttpPost("dry-run")]
    [ProducesResponseType(typeof(DryRunExecutionResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DryRunExecutionResult>> ExecuteDryRun(
        [FromBody] DryRunExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Starting dry-run execution");

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _dryRunService.ExecuteDryRunAsync(
                request.Sql, 
                request.MaxRowsToAnalyze, 
                request.MaxExecutionTime, 
                cancellationToken);

            _logger.LogInformation("✅ Dry-run execution completed: {Success}", result.ExecutedSuccessfully);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for dry-run execution");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in dry-run execution");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Dry-Run Execution Error",
                Detail = "An error occurred during dry-run execution",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// SQL self-correction based on validation issues
    /// </summary>
    /// <param name="sql">Original SQL query</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="validationIssues">List of validation issues to address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Self-correction attempt result</returns>
    [HttpPost("self-correct")]
    [ProducesResponseType(typeof(SelfCorrectionAttempt), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SelfCorrectionAttempt>> SelfCorrectSql(
        [FromBody] string sql,
        [FromQuery, Required] string originalQuery,
        [FromQuery] List<string> validationIssues,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Starting SQL self-correction for {IssueCount} issues", validationIssues.Count);

            if (string.IsNullOrWhiteSpace(sql) || string.IsNullOrWhiteSpace(originalQuery))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Input",
                    Detail = "Both SQL and original query are required",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var result = await _selfCorrectionService.CorrectSqlAsync(
                sql, originalQuery, validationIssues, cancellationToken);

            _logger.LogInformation("✅ Self-correction completed: {Success} (Score: {Score:F2})", 
                result.WasSuccessful, result.ImprovementScore);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for self-correction");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SQL self-correction");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Self-Correction Error",
                Detail = "An error occurred during SQL self-correction",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get validation metrics for monitoring and optimization
    /// </summary>
    /// <param name="timeRangeHours">Time range in hours for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation metrics</returns>
    [HttpGet("metrics")]
    [ProducesResponseType(typeof(ValidationMetrics), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ValidationMetrics>> GetValidationMetrics(
        [FromQuery] int timeRangeHours = 24,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📊 Getting validation metrics for {Hours} hours", timeRangeHours);

            var timeRange = TimeSpan.FromHours(timeRangeHours);
            var metrics = await _enhancedValidator.GetValidationMetricsAsync(timeRange, cancellationToken);

            _logger.LogInformation("✅ Retrieved validation metrics: {TotalValidations} validations", 
                metrics.TotalValidations);

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting validation metrics");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Metrics Error",
                Detail = "An error occurred while retrieving validation metrics",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get correction patterns and statistics
    /// </summary>
    /// <param name="timeRangeHours">Time range in hours for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Correction patterns and statistics</returns>
    [HttpGet("correction-patterns")]
    [ProducesResponseType(typeof(Dictionary<string, object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<Dictionary<string, object>>> GetCorrectionPatterns(
        [FromQuery] int timeRangeHours = 168, // Default 1 week
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📈 Getting correction patterns for {Hours} hours", timeRangeHours);

            var timeRange = TimeSpan.FromHours(timeRangeHours);
            var patterns = await _selfCorrectionService.GetCorrectionPatternsAsync(timeRange, cancellationToken);

            _logger.LogInformation("✅ Retrieved correction patterns");

            return Ok(patterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting correction patterns");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Patterns Error",
                Detail = "An error occurred while retrieving correction patterns",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }
}
