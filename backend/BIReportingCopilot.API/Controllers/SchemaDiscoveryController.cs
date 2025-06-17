using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BIReportingCopilot.Infrastructure.Schema;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for discovering and managing database schemas
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchemaDiscoveryController : ControllerBase
{
    private readonly ILogger<SchemaDiscoveryController> _logger;
    private readonly DatabaseSchemaDiscoveryService _discoveryService;
    private readonly ISchemaManagementService _schemaManagementService;

    public SchemaDiscoveryController(
        ILogger<SchemaDiscoveryController> logger,
        DatabaseSchemaDiscoveryService discoveryService,
        ISchemaManagementService schemaManagementService)
    {
        _logger = logger;
        _discoveryService = discoveryService;
        _schemaManagementService = schemaManagementService;
    }

    /// <summary>
    /// Discover schema from target BI database
    /// </summary>
    [HttpGet("discover")]
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
    [HttpGet("connections")]
    public ActionResult<object> GetAvailableConnections()
    {        return Ok(new
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
    [HttpPost("save")]
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
            };            // Save to BusinessSchema tables
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
    [HttpGet("wizard/steps")]
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
            BusinessDataType = MapToBusinessDataType(column.DataType),            DataExamples = new List<string>(), // Will be populated later
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
    {        return new SchemaRelationshipDto
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
