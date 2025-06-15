using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Commands;
using MediatR;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Schema Controller - Provides schema operations, management, and optimization
/// </summary>
[ApiController]
[Route("api/schema")]
[Authorize]
public class SchemaController : ControllerBase
{
    private readonly ILogger<SchemaController> _logger;
    private readonly ISchemaService _schemaService;
    private readonly ISchemaManagementService _schemaManagementService;
    private readonly ISchemaOptimizationService _schemaOptimizationService;
    private readonly IMediator _mediator;

    public SchemaController(
        ILogger<SchemaController> logger,
        ISchemaService schemaService,
        ISchemaManagementService schemaManagementService,
        ISchemaOptimizationService schemaOptimizationService,
        IMediator mediator)
    {
        _logger = logger;
        _schemaService = schemaService;
        _schemaManagementService = schemaManagementService;
        _schemaOptimizationService = schemaOptimizationService;
        _mediator = mediator;
    }

    #region Basic Schema Operations

    /// <summary>
    /// Get database schema information
    /// </summary>
    /// <param name="connectionName">Optional connection name (default uses primary)</param>
    /// <returns>Database schema with tables, columns, and relationships</returns>
    [HttpGet]
    public async Task<ActionResult<SchemaMetadata>> GetSchema([FromQuery] string? connectionName = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting schema for user {UserId}, connection: {ConnectionName}", userId, connectionName ?? "default");

            var schema = await _schemaService.GetSchemaMetadataAsync(connectionName);
            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving database schema");
            return StatusCode(500, new { error = "An error occurred while retrieving schema information" });
        }
    }

    /// <summary>
    /// Get available database connections
    /// </summary>
    /// <returns>List of available database connections</returns>
    [HttpGet("connections")]
    public async Task<ActionResult<List<string>>> GetConnections()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting available connections for user {UserId}", userId);

            var connections = await _schemaService.GetAccessibleTablesAsync(userId);
            return Ok(connections);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving database connections");
            return StatusCode(500, new { error = "An error occurred while retrieving connections" });
        }
    }

    /// <summary>
    /// Get available data sources
    /// </summary>
    /// <returns>List of available data sources</returns>
    [HttpGet("datasources")]
    [AllowAnonymous] // Allow anonymous access for data source discovery
    public async Task<ActionResult<object>> GetDataSources()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting available data sources for user {UserId}", userId);

            var schema = await _schemaService.GetSchemaMetadataAsync();

            var dataSources = new[]
            {
                new
                {
                    name = "DailyActionsDB",
                    displayName = "Daily Actions Database",
                    type = "SQL Server",
                    status = "Connected",
                    description = "Main database containing player statistics and gaming data",
                    tables = schema.Tables?.Count ?? 0,
                    lastUpdated = DateTime.UtcNow
                }
            };

            return Ok(dataSources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving data sources");
            return StatusCode(500, new { error = "An error occurred while retrieving data sources" });
        }
    }

    /// <summary>
    /// Get table details including sample data
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="connectionName">Optional connection name</param>
    /// <returns>Table details with sample data</returns>
    [HttpGet("tables/{tableName}")]
    public async Task<ActionResult<TableMetadata>> GetTableDetails(
        string tableName,
        [FromQuery] string? connectionName = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting table details for {TableName}, user {UserId}", tableName, userId);

            var tableDetails = await _schemaService.GetTableMetadataAsync(tableName, connectionName);
            return Ok(tableDetails);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving table details for {TableName}", tableName);
            return StatusCode(500, new { error = "An error occurred while retrieving table details" });
        }
    }

    /// <summary>
    /// Get schema suggestions for query building
    /// </summary>
    /// <param name="context">Context for suggestions (e.g., partial query)</param>
    /// <returns>Schema-based suggestions</returns>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<string>>> GetSchemaSuggestions([FromQuery] string? context = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting schema suggestions for user {UserId}", userId);

            var suggestions = await _schemaService.GetSchemaSuggestionsAsync(userId);
            var suggestionStrings = suggestions.Select(s => s.Name).ToList();
            return Ok(suggestionStrings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema suggestions");
            return StatusCode(500, new { error = "An error occurred while getting schema suggestions" });
        }
    }

    /// <summary>
    /// Refresh schema cache
    /// </summary>
    /// <param name="connectionName">Optional connection name</param>
    /// <returns>Success status</returns>
    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshSchema([FromQuery] string? connectionName = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Refreshing schema cache for user {UserId}, connection: {ConnectionName}", userId, connectionName ?? "default");

            await _schemaService.RefreshSchemaMetadataAsync(connectionName);
            return Ok(new { message = "Schema cache refreshed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schema cache");
            return StatusCode(500, new { error = "An error occurred while refreshing schema cache" });
        }
    }

    /// <summary>
    /// Get data quality assessment for a table
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="connectionName">Optional connection name</param>
    /// <returns>Data quality metrics</returns>
    [HttpGet("tables/{tableName}/quality")]
    public async Task<ActionResult<DataQualityScore>> GetDataQuality(
        string tableName,
        [FromQuery] string? connectionName = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting data quality for table {TableName}, user {UserId}", tableName, userId);

            var assessment = await _schemaService.AssessDataQualityAsync(tableName);
            return Ok(assessment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing data quality for {TableName}", tableName);
            return StatusCode(500, new { error = "An error occurred while assessing data quality" });
        }
    }

    #endregion

    #region Schema Management Operations

    /// <summary>
    /// Get all business schemas for the current user
    /// </summary>
    [HttpGet("business-schemas")]
    public async Task<ActionResult<List<BusinessSchemaDto>>> GetBusinessSchemas()
    {
        try
        {
            var userId = GetUserId();
            var schemas = await _schemaManagementService.GetBusinessSchemasAsync(userId);
            return Ok(schemas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business schemas");
            return StatusCode(500, new { message = "An error occurred while retrieving business schemas" });
        }
    }

    /// <summary>
    /// Get a specific business schema by ID
    /// </summary>
    [HttpGet("business-schemas/{schemaId:guid}")]
    public async Task<ActionResult<BusinessSchemaDto>> GetBusinessSchema(Guid schemaId)
    {
        try
        {
            var userId = GetUserId();
            var schema = await _schemaManagementService.GetBusinessSchemaAsync(schemaId.ToString(), userId);
            
            if (schema == null)
            {
                return NotFound(new { message = "Business schema not found" });
            }

            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business schema {SchemaId}", schemaId);
            return StatusCode(500, new { message = "An error occurred while retrieving the business schema" });
        }
    }

    /// <summary>
    /// Create a new business schema
    /// </summary>
    [HttpPost("business-schemas")]
    public async Task<ActionResult<BusinessSchemaDto>> CreateBusinessSchema([FromBody] BIReportingCopilot.Core.DTOs.CreateBusinessSchemaRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            
            // Map DTO to interface model
            var interfaceRequest = new BIReportingCopilot.Core.Interfaces.Schema.CreateBusinessSchemaRequest
            {
                Name = request.Name,
                Description = request.Description ?? string.Empty,
                IsPublic = !request.IsDefault // Map IsDefault to IsPublic inversely, adjust as needed
            };
            
            var schema = await _schemaManagementService.CreateBusinessSchemaAsync(interfaceRequest, userId);
            return CreatedAtAction(nameof(GetBusinessSchema), new { schemaId = schema.Id }, schema);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business schema");
            return StatusCode(500, new { message = "An error occurred while creating the business schema" });
        }
    }

    #endregion

    #region Schema Optimization Operations

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

            _logger.LogInformation("⚡ SQL optimization completed - Improvement: {Improvement:P2}",
                result.ImprovementScore);

            return Ok(new
            {
                success = true,
                originalSql = request.OriginalSql,
                optimizedSql = result.OptimizedSql,
                improvementScore = result.ImprovementScore,
                optimizations = result.Optimizations,
                estimatedPerformanceGain = result.EstimatedPerformanceGain,
                recommendations = result.Recommendations
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing SQL");
            return StatusCode(500, new { success = false, error = "An error occurred while optimizing the SQL query" });
        }
    }

    /// <summary>
    /// Analyze overall schema health and suggest improvements
    /// </summary>
    [HttpPost("analyze-health")]
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

            var result = await _mediator.Send(command);

            _logger.LogInformation("🏥 Schema health analysis completed - Overall Score: {Score:F2}",
                result.OverallHealthScore);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing schema health");
            return StatusCode(500, new { success = false, error = "An error occurred while analyzing schema health" });
        }
    }

    /// <summary>
    /// Get schema optimization metrics and statistics
    /// </summary>
    [HttpGet("optimization-metrics")]
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

            return Ok(metrics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving schema optimization metrics");
            return StatusCode(500, new { success = false, error = "An error occurred while retrieving optimization metrics" });
        }
    }

    #endregion

    #region Helper Methods

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }

    private string GetUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
               User.FindFirst("sub")?.Value ??
               User.Identity?.Name ??
               "anonymous";
    }

    #endregion
}

#region Request Models

public class OptimizeSQLRequest
{
    public string OriginalSql { get; set; } = string.Empty;
    public SchemaMetadata Schema { get; set; } = new();
    public List<OptimizationGoal>? Goals { get; set; }
}

public class SchemaHealthRequest
{
    public SchemaMetadata Schema { get; set; } = new();
    public bool IncludePerformanceAnalysis { get; set; } = true;
    public bool IncludeSecurityAnalysis { get; set; } = true;
}

#endregion
