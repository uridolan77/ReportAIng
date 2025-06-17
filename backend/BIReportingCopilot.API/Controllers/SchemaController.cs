using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Infrastructure.Schema;
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
{    private readonly ILogger<SchemaController> _logger;
    private readonly BIReportingCopilot.Core.Interfaces.Schema.ISchemaService _schemaService;
    private readonly BIReportingCopilot.Infrastructure.Interfaces.ISchemaManagementService _schemaManagementService;
    private readonly DatabaseSchemaDiscoveryService _discoveryService;    public SchemaController(
        ILogger<SchemaController> logger,
        BIReportingCopilot.Core.Interfaces.Schema.ISchemaService schemaService,
        BIReportingCopilot.Infrastructure.Interfaces.ISchemaManagementService schemaManagementService,
        DatabaseSchemaDiscoveryService discoveryService)
    {
        _logger = logger;        _schemaService = schemaService;
        _schemaManagementService = schemaManagementService;
        _discoveryService = discoveryService;
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var subClaim = User.FindFirst("sub")?.Value;
        var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

        var result = userId ?? subClaim ?? nameClaim ?? emailClaim ?? "unknown";
        return result;
    }/// <summary>
    /// Get database schema metadata - now uses live discovery from BI database
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<SchemaMetadata>> GetSchema()
    {
        try
        {
            _logger.LogInformation("🔍 Getting schema metadata from BI database via discovery service");
            
            // Use the discovery service to get live schema from BI database
            var discoveryResult = await _discoveryService.DiscoverSchemaAsync("BIDatabase");
            
            // Convert discovery result to SchemaMetadata format expected by frontend
            var schema = new SchemaMetadata
            {
                DatabaseName = discoveryResult.DatabaseName,
                LastUpdated = DateTime.UtcNow,
                Tables = discoveryResult.Tables.Select(t => new TableMetadata
                {
                    Name = t.TableName,
                    Schema = t.SchemaName,
                    Description = $"Table with {t.Columns.Count} columns",
                    RowCount = 0, // Discovery service doesn't count rows for performance
                    LastUpdated = DateTime.UtcNow,
                    Columns = t.Columns.Select(c => new ColumnMetadata
                    {
                        Name = c.ColumnName,
                        DataType = c.DataType,
                        Description = $"{c.DataType} column" + (c.IsPrimaryKey ? " (Primary Key)" : "") + (c.IsForeignKey ? " (Foreign Key)" : ""),
                        IsNullable = c.IsNullable,
                        IsPrimaryKey = c.IsPrimaryKey,
                        IsForeignKey = c.IsForeignKey,
                        SemanticTags = new[] { c.DataType.ToLower() },
                        SampleValues = new string[0] // Could be populated later if needed
                    }).ToList()
                }).ToList()
            };
            
            _logger.LogInformation("✅ Schema discovery completed: {TableCount} tables found", schema.Tables.Count);
            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting schema metadata from BI database");
            return StatusCode(500, new { error = "Failed to retrieve schema metadata from BI database", details = ex.Message });
        }
    }    /// <summary>
    /// Get all tables in the database - now uses live discovery from BI database
    /// </summary>
    [HttpGet("tables")]
    public async Task<ActionResult<List<TableMetadata>>> GetTables()
    {
        try
        {
            _logger.LogInformation("🔍 Getting database tables from BI database via discovery service");
            
            var discoveryResult = await _discoveryService.DiscoverSchemaAsync("BIDatabase");
            
            var tables = discoveryResult.Tables.Select(t => new TableMetadata
            {
                Name = t.TableName,
                Schema = t.SchemaName,
                Description = $"Table with {t.Columns.Count} columns",
                RowCount = 0, // Discovery service doesn't count rows for performance
                LastUpdated = DateTime.UtcNow,
                Columns = t.Columns.Select(c => new ColumnMetadata
                {
                    Name = c.ColumnName,
                    DataType = c.DataType,
                    Description = $"{c.DataType} column" + (c.IsPrimaryKey ? " (Primary Key)" : "") + (c.IsForeignKey ? " (Foreign Key)" : ""),
                    IsNullable = c.IsNullable,
                    IsPrimaryKey = c.IsPrimaryKey,
                    IsForeignKey = c.IsForeignKey,
                    SemanticTags = new[] { c.DataType.ToLower() },
                    SampleValues = new string[0]
                }).ToList()
            }).ToList();
            
            _logger.LogInformation("✅ Tables discovery completed: {TableCount} tables found", tables.Count);
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting database tables from BI database");
            return StatusCode(500, new { error = "Failed to retrieve tables from BI database", details = ex.Message });
        }
    }    /// <summary>
    /// Get metadata for a specific table - now uses live discovery from BI database
    /// </summary>
    [HttpGet("tables/{tableName}")]
    public async Task<ActionResult<TableMetadata>> GetTableMetadata(string tableName)
    {
        try
        {
            _logger.LogInformation("🔍 Getting metadata for table: {TableName} from BI database", tableName);
            
            var discoveryResult = await _discoveryService.DiscoverSchemaAsync("BIDatabase");
            
            var discoveredTable = discoveryResult.Tables.FirstOrDefault(t => 
                t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase) ||
                $"{t.SchemaName}.{t.TableName}".Equals(tableName, StringComparison.OrdinalIgnoreCase));
            
            if (discoveredTable == null)
            {
                _logger.LogWarning("Table '{TableName}' not found in BI database", tableName);
                return NotFound(new { error = $"Table '{tableName}' not found in BI database" });
            }
            
            var tableMetadata = new TableMetadata
            {
                Name = discoveredTable.TableName,
                Schema = discoveredTable.SchemaName,
                Description = $"Table with {discoveredTable.Columns.Count} columns",
                RowCount = 0, // Discovery service doesn't count rows for performance
                LastUpdated = DateTime.UtcNow,
                Columns = discoveredTable.Columns.Select(c => new ColumnMetadata
                {
                    Name = c.ColumnName,
                    DataType = c.DataType,
                    Description = $"{c.DataType} column" + (c.IsPrimaryKey ? " (Primary Key)" : "") + (c.IsForeignKey ? " (Foreign Key)" : ""),
                    IsNullable = c.IsNullable,
                    IsPrimaryKey = c.IsPrimaryKey,
                    IsForeignKey = c.IsForeignKey,
                    SemanticTags = new[] { c.DataType.ToLower() },
                    SampleValues = new string[0]
                }).ToList()
            };
            
            _logger.LogInformation("✅ Table metadata discovery completed for: {TableName}", tableName);
            return Ok(tableMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting table metadata for: {TableName} from BI database", tableName);
            return StatusCode(500, new { error = $"Failed to retrieve metadata for table '{tableName}' from BI database", details = ex.Message });
        }
    }    /// <summary>
    /// Refresh schema metadata - now triggers fresh discovery from BI database
    /// </summary>
    [HttpPost("refresh")]
    public async Task<ActionResult> RefreshSchema()
    {
        try
        {
            _logger.LogInformation("🔄 Refreshing schema metadata by triggering fresh discovery from BI database");
            
            // Trigger a fresh discovery from the BI database
            var discoveryResult = await _discoveryService.DiscoverSchemaAsync("BIDatabase");
            
            _logger.LogInformation("✅ Schema refresh completed: {TableCount} tables discovered from BI database", 
                discoveryResult.Tables.Count);
            
            return Ok(new { 
                message = "Schema refreshed successfully from BI database", 
                tablesDiscovered = discoveryResult.Tables.Count,
                databaseName = discoveryResult.DatabaseName
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error refreshing schema from BI database");
            return StatusCode(500, new { error = "Failed to refresh schema from BI database", details = ex.Message });
        }
    }

    /// <summary>
    /// Get available databases
    /// </summary>
    [HttpGet("databases")]
    public async Task<ActionResult<List<string>>> GetDatabases()
    {        try
        {
            _logger.LogInformation("Getting available databases");
            var userId = GetCurrentUserId();
            var databases = await _schemaManagementService.GetDatabasesAsync(userId);
            return Ok(databases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting databases");
            return StatusCode(500, new { error = "Failed to retrieve databases", details = ex.Message });
        }
    }    /// <summary>
    /// Get data sources for schema exploration - now uses live discovery from BI database
    /// </summary>
    [HttpGet("datasources")]
    public async Task<ActionResult> GetDataSources()
    {
        try
        {
            _logger.LogInformation("🔍 Getting data sources from BI database via discovery service");
            
            var discoveryResult = await _discoveryService.DiscoverSchemaAsync("BIDatabase");
            
            // Transform to simple format expected by frontend
            var dataSources = discoveryResult.Tables.Select(t => new
            {
                name = t.TableName,
                schema = t.SchemaName ?? "dbo",
                type = "table",
                rowCount = 0, // Discovery service doesn't count rows for performance
                columns = t.Columns?.Count ?? 0,
                fullName = $"{t.SchemaName}.{t.TableName}"
            }).ToList();
            
            _logger.LogInformation("✅ Data sources discovery completed: {TableCount} tables found", dataSources.Count);
            return Ok(dataSources);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting data sources from BI database");
            return StatusCode(500, new { error = "Failed to retrieve data sources from BI database", details = ex.Message });
        }
    }
}
