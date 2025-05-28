using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchemaController : ControllerBase
{
    private readonly ILogger<SchemaController> _logger;
    private readonly ISchemaService _schemaService;

    public SchemaController(ILogger<SchemaController> logger, ISchemaService schemaService)
    {
        _logger = logger;
        _schemaService = schemaService;
    }

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
    /// Get available data sources (alias for connections)
    /// </summary>
    /// <returns>List of available data sources</returns>
    [HttpGet("datasources")]
    public async Task<ActionResult<object>> GetDataSources()
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Getting available data sources for user {UserId}", userId);

            // Get schema metadata to provide more detailed information
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

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
    }
}
