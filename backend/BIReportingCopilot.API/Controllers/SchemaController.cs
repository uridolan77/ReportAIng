using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Simplified Schema Controller - Provides basic schema operations
/// </summary>
[ApiController]
[Route("api/schema")]
[Authorize]
public class SchemaController : ControllerBase
{
    private readonly ILogger<SchemaController> _logger;
    private readonly BIReportingCopilot.Core.Interfaces.Schema.ISchemaService _schemaService;
    private readonly BIReportingCopilot.Infrastructure.Interfaces.ISchemaManagementService _schemaManagementService;

    public SchemaController(
        ILogger<SchemaController> logger,
        BIReportingCopilot.Core.Interfaces.Schema.ISchemaService schemaService,
        BIReportingCopilot.Infrastructure.Interfaces.ISchemaManagementService schemaManagementService)
    {
        _logger = logger;
        _schemaService = schemaService;
        _schemaManagementService = schemaManagementService;
    }

    /// <summary>
    /// Get database schema metadata
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SchemaMetadata>> GetSchema()
    {
        try
        {
            _logger.LogInformation("Getting schema metadata");
            var schema = await _schemaManagementService.GetSchemaMetadataAsync();
            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema metadata");
            return StatusCode(500, new { error = "Failed to retrieve schema metadata", details = ex.Message });
        }
    }

    /// <summary>
    /// Get all tables in the database
    /// </summary>
    [HttpGet("tables")]
    public async Task<ActionResult<List<TableMetadata>>> GetTables()
    {
        try
        {
            _logger.LogInformation("Getting database tables");
            var tables = await _schemaManagementService.GetTablesAsync();
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting database tables");
            return StatusCode(500, new { error = "Failed to retrieve tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get metadata for a specific table
    /// </summary>
    [HttpGet("tables/{tableName}")]
    public async Task<ActionResult<TableMetadata>> GetTableMetadata(string tableName)
    {
        try
        {
            _logger.LogInformation("Getting metadata for table: {TableName}", tableName);
            var tableMetadata = await _schemaManagementService.GetTableMetadataAsync(tableName);
            
            if (tableMetadata == null)
            {
                return NotFound(new { error = $"Table '{tableName}' not found" });
            }
            
            return Ok(tableMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table metadata for: {TableName}", tableName);
            return StatusCode(500, new { error = $"Failed to retrieve metadata for table '{tableName}'", details = ex.Message });
        }
    }

    /// <summary>
    /// Refresh schema metadata
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshSchema()
    {
        try
        {
            _logger.LogInformation("Refreshing schema metadata");
            await _schemaManagementService.RefreshSchemaAsync();
            return Ok(new { message = "Schema refreshed successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schema");
            return StatusCode(500, new { error = "Failed to refresh schema", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available databases
    /// </summary>
    [HttpGet("databases")]
    public async Task<ActionResult<List<string>>> GetDatabases()
    {
        try
        {
            _logger.LogInformation("Getting available databases");
            var databases = await _schemaManagementService.GetDatabasesAsync();
            return Ok(databases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting databases");
            return StatusCode(500, new { error = "Failed to retrieve databases", details = ex.Message });
        }
    }

    /// <summary>
    /// Get data sources for schema exploration
    /// </summary>
    [HttpGet("datasources")]
    public async Task<ActionResult> GetDataSources()
    {
        try
        {
            _logger.LogInformation("Getting data sources");
            var tables = await _schemaManagementService.GetTablesAsync();            // Transform to simple format expected by frontend
            var dataSources = tables.Select(t => new
            {
                name = t.Name,
                schema = t.Schema ?? "dbo",
                type = "table",
                rowCount = (int)t.RowCount,
                columns = t.Columns?.Count ?? 0
            }).ToList();
            
            return Ok(dataSources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting data sources");
            return StatusCode(500, new { error = "Failed to retrieve data sources", details = ex.Message });
        }
    }
}
