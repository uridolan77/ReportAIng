using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Infrastructure.Schema;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Infrastructure.Data.Entities;
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
    private readonly DatabaseSchemaDiscoveryService _discoveryService;
    private readonly IBusinessTableManagementService _businessTableService;

    public SchemaController(
        ILogger<SchemaController> logger,
        BIReportingCopilot.Core.Interfaces.Schema.ISchemaService schemaService,
        BIReportingCopilot.Infrastructure.Interfaces.ISchemaManagementService schemaManagementService,
        DatabaseSchemaDiscoveryService discoveryService,
        IBusinessTableManagementService businessTableService)
    {
        _logger = logger;
        _schemaService = schemaService;
        _schemaManagementService = schemaManagementService;
        _discoveryService = discoveryService;
        _businessTableService = businessTableService;
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
            _logger.LogInformation("🔍 Getting schema metadata from cached schema service");

            // Use cached schema service for performance instead of full discovery
            var schemaMetadata = await _schemaService.GetSchemaMetadataAsync("BIDatabase");
            
            // Schema metadata is already in the correct format from cached service
            var schema = schemaMetadata;
            
            _logger.LogInformation("✅ Schema discovery completed: {TableCount} tables found", schema.Tables.Count);
            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting schema metadata from BI database");
            return StatusCode(500, new { error = "Failed to retrieve schema metadata from BI database", details = ex.Message });
        }
    }

    /// <summary>
    /// Get test data for frontend debugging
    /// </summary>
    [HttpGet("tables/test")]
    [ProducesResponseType(typeof(IEnumerable<object>), 200)]
    public IActionResult GetTestTables()
    {
        var testData = new[]
        {
            new
            {
                tableInformation = "common.tbl_Daily_actions",
                domainUseCase = "Gaming Analytics",
                businessContext = "Daily player actions tracking",
                qualityUsage = "High",
                ruleGovernance = "GDPR compliant",
                lastUpdated = "2024-01-15",
                actions = "Edit"
            },
            new
            {
                tableInformation = "common.tbl_Daily_actions_players",
                domainUseCase = "Player Analytics",
                businessContext = "Player behavior analysis",
                qualityUsage = "Medium",
                ruleGovernance = "Data retention 7 years",
                lastUpdated = "2024-01-14",
                actions = "Edit"
            }
        };

        return Ok(testData);
    }

    /// <summary>
    /// Get all tables in the database - now returns business metadata when available
    /// </summary>
    [HttpGet("tables")]
    [AllowAnonymous] // Temporary for testing
    public async Task<ActionResult<List<object>>> GetTables()
    {
        try
        {
            _logger.LogInformation("🔍 Getting business table records from BusinessTableInfo table");

            // Get business table records and transform them to match frontend expectations
            var businessTables = await _businessTableService.GetBusinessTablesAsync();

            // Transform to the format expected by the frontend (matching businessApi.ts interface)
            var result = businessTables.Select(bt => new
            {
                schemaName = bt.SchemaName ?? "dbo",
                tableName = bt.TableName,
                businessPurpose = bt.BusinessPurpose ?? bt.BusinessContext,
                domainClassification = bt.PrimaryUseCase ?? "General",
                estimatedRowCount = (int?)null, // We don't have this in BusinessTableInfo
                lastUpdated = bt.UpdatedDate?.ToString("yyyy-MM-dd") ?? bt.CreatedDate.ToString("yyyy-MM-dd")
            }).ToList();

            _logger.LogInformation("✅ Retrieved {Count} business table records from BusinessTableInfo table", result.Count);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting tables");
            return StatusCode(500, new { error = "Failed to retrieve tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get metadata for a specific table - uses cached schema for performance
    /// </summary>
    [HttpGet("tables/{tableName}")]
    public async Task<ActionResult<TableMetadata>> GetTableMetadata(string tableName)
    {
        try
        {
            _logger.LogInformation("🔍 Getting metadata for table: {TableName} from cached schema", tableName);

            // Use cached schema instead of full discovery for performance
            var schemaMetadata = await _schemaService.GetSchemaMetadataAsync("BIDatabase");

            var discoveredTable = schemaMetadata.Tables.FirstOrDefault(t =>
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) ||
                $"{t.Schema}.{t.Name}".Equals(tableName, StringComparison.OrdinalIgnoreCase));

            if (discoveredTable == null)
            {
                _logger.LogWarning("Table '{TableName}' not found in cached schema, trying fresh discovery", tableName);

                // Fallback to fresh discovery if not found in cache
                var discoveryResult = await _discoveryService.DiscoverSchemaAsync("BIDatabase");
                var discoveredTableFromDiscovery = discoveryResult.Tables.FirstOrDefault(t =>
                    t.TableName.Equals(tableName, StringComparison.OrdinalIgnoreCase) ||
                    $"{t.SchemaName}.{t.TableName}".Equals(tableName, StringComparison.OrdinalIgnoreCase));

                if (discoveredTableFromDiscovery != null)
                {
                    // Convert DiscoveredTable to TableMetadata for the fallback case
                    discoveredTable = new TableMetadata
                    {
                        Name = discoveredTableFromDiscovery.TableName,
                        Schema = discoveredTableFromDiscovery.SchemaName,
                        Description = $"Table with {discoveredTableFromDiscovery.Columns?.Count ?? 0} columns",
                        RowCount = discoveredTableFromDiscovery.RowCount,
                        LastUpdated = DateTime.UtcNow,
                        Columns = discoveredTableFromDiscovery.Columns?.Select(c => new ColumnMetadata
                        {
                            Name = c.ColumnName,
                            DataType = c.DataType,
                            Description = $"{c.DataType} column" + (c.IsPrimaryKey ? " (Primary Key)" : "") + (c.IsForeignKey ? " (Foreign Key)" : ""),
                            IsNullable = c.IsNullable,
                            IsPrimaryKey = c.IsPrimaryKey,
                            IsForeignKey = c.IsForeignKey,
                            DefaultValue = c.DefaultValue,
                            MaxLength = c.MaxLength,
                            Precision = c.Precision,
                            Scale = c.Scale,
                            SemanticTags = new[] { c.DataType?.ToLower() ?? "unknown" },
                            SampleValues = new string[0]
                        }).ToList() ?? new List<ColumnMetadata>()
                    };
                }

                if (discoveredTable == null)
                {
                    _logger.LogWarning("Table '{TableName}' not found in BI database", tableName);
                    return NotFound(new { error = $"Table '{tableName}' not found in BI database" });
                }
            }

            // discoveredTable is already a TableMetadata object (either from cache or converted from DiscoveredTable)
            var tableMetadata = discoveredTable;

            _logger.LogInformation("✅ Table metadata discovery completed for: {TableName}", tableName);
            return Ok(tableMetadata);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting table metadata for: {TableName} from BI database", tableName);
            return StatusCode(500, new { error = $"Failed to retrieve metadata for table '{tableName}' from BI database", details = ex.Message });
        }
    }

    /// <summary>
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
    {
        try
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
    }

    /// <summary>
    /// Get data sources for schema exploration - now uses live discovery from BI database
    /// </summary>
    [HttpGet("datasources")]
    public async Task<ActionResult> GetDataSources()
    {
        try
        {
            _logger.LogInformation("🔍 Getting data sources from cached schema service");

            var schemaMetadata = await _schemaService.GetSchemaMetadataAsync("BIDatabase");

            // Transform to simple format expected by frontend
            var dataSources = schemaMetadata.Tables.Select(t => new
            {
                name = t.Name,
                schema = t.Schema ?? "dbo",
                type = "table",
                rowCount = t.RowCount,
                columns = t.Columns?.Count ?? 0,
                fullName = $"{t.Schema}.{t.Name}"
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
