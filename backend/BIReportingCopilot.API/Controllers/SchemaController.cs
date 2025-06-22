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

    #region Schema Discovery Endpoints

    /// <summary>
    /// Discover schema from target BI database
    /// </summary>
    [HttpGet("discovery/discover")]
    public async Task<ActionResult<SchemaDiscoveryResult>> DiscoverSchemaAsync(
        [FromQuery] string connectionStringName = "BIDatabase")
    {
        try
        {
            _logger.LogInformation("🔍 Schema discovery requested for: {ConnectionStringName}", connectionStringName);

            var result = await _discoveryService.DiscoverSchemaAsync(connectionStringName);

            _logger.LogInformation("✅ Schema discovery completed: {TableCount} tables discovered",
                result.Tables.Count);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during schema discovery");
            return StatusCode(500, new {
                error = "Schema discovery failed",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get available connection strings for discovery
    /// </summary>
    [HttpGet("discovery/connections")]
    public ActionResult<object> GetAvailableConnections()
    {
        return Ok(new
        {
            connections = new[]
            {
                new { name = "BIDatabase", description = "Primary BI Database (DailyActionsDB)", isDefault = true },
                new { name = "DefaultConnection", description = "Local Development Database", isDefault = false }
            }
        });
    }

    /// <summary>
    /// Save discovered schema to BusinessSchema tables
    /// </summary>
    [HttpPost("discovery/save")]
    public async Task<ActionResult<object>> SaveDiscoveredSchemaAsync(
        [FromBody] SaveSchemaRequest request)
    {
        try
        {
            _logger.LogInformation("💾 Saving discovered schema: {SchemaName}", request.SchemaName);

            // First, discover the schema
            var discoveryResult = await _discoveryService.DiscoverSchemaAsync(request.ConnectionStringName);

            // Convert to BusinessSchema format
            var applyRequest = new ApplyToSchemaRequest
            {
                SchemaId = Guid.Empty, // Create new schema
                NewSchemaName = request.SchemaName,
                NewSchemaDescription = request.Description ?? $"Schema discovered from {discoveryResult.DatabaseName}",
                VersionName = "v1.0",
                VersionDescription = "Initial version from schema discovery",
                CreateNewVersion = true,
                SetAsCurrent = true,
                ChangeLog = new List<SchemaChangeLogEntry>
                {
                    new SchemaChangeLogEntry
                    {
                        Timestamp = DateTime.UtcNow,
                        ChangeType = "Created",
                        Description = $"Schema discovered from {discoveryResult.DatabaseName} with {discoveryResult.Tables.Count} tables and {discoveryResult.Relationships.Count} relationships",
                        ChangedBy = User?.Identity?.Name ?? "System"
                    }
                },
                TableContexts = discoveryResult.Tables.Select(MapToTableContext).ToList(),
                GlossaryTerms = new List<SchemaGlossaryTermDto>(), // Will be auto-generated later
                Relationships = discoveryResult.Relationships.Select(MapToRelationship).ToList()
            };

            // Save to BusinessSchema tables
            var result = await _schemaManagementService.ApplyToSchemaAsync(applyRequest, User?.Identity?.Name ?? "System");

            _logger.LogInformation("✅ Schema saved successfully: {SchemaId}", result.SchemaId);

            return Ok(new
            {
                success = true,
                schemaId = result.SchemaId,
                message = $"Schema '{request.SchemaName}' saved successfully with {discoveryResult.Tables.Count} tables",
                tablesCount = discoveryResult.Tables.Count,
                relationshipsCount = discoveryResult.Relationships.Count
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving discovered schema");
            return StatusCode(500, new {
                error = "Failed to save schema",
                message = ex.Message
            });
        }
    }

    /// <summary>
    /// Get schema discovery wizard steps
    /// </summary>
    [HttpGet("discovery/wizard/steps")]
    public ActionResult<object> GetWizardSteps()
    {
        return Ok(new
        {
            steps = new[]
            {
                new {
                    step = 1,
                    title = "Select Connection",
                    description = "Choose the target database to discover schema from"
                },
                new {
                    step = 2,
                    title = "Discover Schema",
                    description = "Scan the database and discover tables, columns, and relationships"
                },
                new {
                    step = 3,
                    title = "Review & Edit",
                    description = "Review discovered schema and add business context"
                },
                new {
                    step = 4,
                    title = "Save Schema",
                    description = "Save the schema to your BusinessSchema repository"
                }
            }
        });
    }

    #endregion

    #region Private Helper Methods for Schema Discovery

    /// <summary>
    /// Convert discovered table to SchemaTableContextDto
    /// </summary>
    private SchemaTableContextDto MapToTableContext(DiscoveredTable table)
    {
        return new SchemaTableContextDto
        {
            TableName = table.TableName,
            SchemaName = table.SchemaName,
            BusinessPurpose = GenerateTablePurpose(table),
            BusinessContext = GenerateTableContext(table),
            PrimaryUseCase = GeneratePrimaryUseCase(table),
            KeyBusinessMetrics = new List<string>(), // Will be populated later
            ConfidenceScore = 0.7m, // Auto-discovered, medium confidence
            IsAutoGenerated = true,
            ColumnContexts = table.Columns.Select(MapToColumnContext).ToList()
        };
    }

    /// <summary>
    /// Convert discovered column to SchemaColumnContextDto
    /// </summary>
    private SchemaColumnContextDto MapToColumnContext(DiscoveredColumn column)
    {
        return new SchemaColumnContextDto
        {
            ColumnName = column.ColumnName,
            BusinessName = GenerateBusinessName(column),
            BusinessDescription = GenerateColumnDescription(column),
            BusinessDataType = MapToBusinessDataType(column.DataType),
            DataExamples = new List<string>(), // Will be populated later
            IsKeyColumn = column.IsPrimaryKey || column.IsForeignKey,
            IsPrimaryKey = column.IsPrimaryKey,
            IsForeignKey = column.IsForeignKey,
            ConfidenceScore = 0.6m, // Auto-discovered, medium confidence
            IsAutoGenerated = true
        };
    }

    /// <summary>
    /// Convert discovered relationship to SchemaRelationshipDto
    /// </summary>
    private SchemaRelationshipDto MapToRelationship(DiscoveredRelationship relationship)
    {
        return new SchemaRelationshipDto
        {
            FromTable = relationship.ParentTable,
            ToTable = relationship.ReferencedTable,
            RelationshipType = "One-to-Many", // Default assumption
            FromColumns = new List<string> { relationship.ParentColumn },
            ToColumns = new List<string> { relationship.ReferencedColumn },
            BusinessDescription = $"Foreign key relationship from {relationship.ParentTable} to {relationship.ReferencedTable}",
            IsAutoGenerated = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    // Helper methods for generating business context
    private string GenerateTablePurpose(DiscoveredTable table) =>
        $"Data table containing {table.Columns.Count} columns for {table.TableName.Replace("_", " ").ToLower()} information";

    private string GenerateTableContext(DiscoveredTable table) =>
        $"Database table from {table.SchemaName} schema";

    private string GeneratePrimaryUseCase(DiscoveredTable table) =>
        "Data storage and retrieval operations";

    private string GenerateKeyMetrics(DiscoveredTable table) =>
        string.Join(", ", table.Columns.Where(c => c.IsPrimaryKey || c.ColumnName.ToLower().Contains("count") || c.ColumnName.ToLower().Contains("total")).Select(c => c.ColumnName));

    private string GenerateBusinessName(DiscoveredColumn column) =>
        column.ColumnName.Replace("_", " ").Replace("ID", "Identifier").Replace("Dt", "Date");

    private string GenerateColumnDescription(DiscoveredColumn column)
    {
        if (column.IsPrimaryKey) return "Primary key identifier";
        if (column.IsForeignKey) return $"Foreign key reference to {column.ReferencedTable}";
        if (column.ColumnName.ToLower().Contains("date")) return "Date/time value";
        if (column.ColumnName.ToLower().Contains("name")) return "Name or title field";
        if (column.ColumnName.ToLower().Contains("count")) return "Numeric count value";
        return $"{column.DataType} data field";
    }

    private string MapToBusinessDataType(string sqlDataType) => sqlDataType.ToLower() switch
    {
        "varchar" or "nvarchar" or "char" or "nchar" or "text" or "ntext" => "Text",
        "int" or "bigint" or "smallint" or "tinyint" => "Integer",
        "decimal" or "numeric" or "float" or "real" or "money" or "smallmoney" => "Decimal",
        "bit" => "Boolean",
        "datetime" or "datetime2" or "smalldatetime" or "date" or "time" => "DateTime",
        "uniqueidentifier" => "Guid",
        _ => "Unknown"
    };

    private string GenerateDataExamples(DiscoveredColumn column)
    {
        if (column.IsPrimaryKey) return "1, 2, 3...";
        if (column.ColumnName.ToLower().Contains("name")) return "Sample Name, Example Title";
        if (column.ColumnName.ToLower().Contains("date")) return "2024-01-01, 2024-12-31";
        if (column.DataType.ToLower().Contains("int")) return "100, 250, 500";
        return "Sample data values";
    }

    #endregion
}

/// <summary>
/// Request model for saving discovered schema
/// </summary>
public class SaveSchemaRequest
{
    public string SchemaName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string ConnectionStringName { get; set; } = "BIDatabase";
}
