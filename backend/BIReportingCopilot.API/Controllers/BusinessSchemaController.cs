using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.Business;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Infrastructure.Schema;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Business schema management controller for versioned schema definitions and metadata management
/// </summary>
[ApiController]
[Route("api/business-schemas")]
[Authorize]
public class BusinessSchemaController : ControllerBase
{
    private readonly ILogger<BusinessSchemaController> _logger;
    private readonly IBusinessSchemaService _businessSchemaService;
    private readonly BusinessMetadataPopulationService _populationService;
    private readonly IBusinessTableManagementService _businessTableService;
    private readonly IGlossaryManagementService _glossaryService;

    public BusinessSchemaController(
        ILogger<BusinessSchemaController> logger,
        IBusinessSchemaService businessSchemaService,
        BusinessMetadataPopulationService populationService,
        IBusinessTableManagementService businessTableService,
        IGlossaryManagementService glossaryService)
    {
        _logger = logger;
        _businessSchemaService = businessSchemaService;
        _populationService = populationService;
        _businessTableService = businessTableService;
        _glossaryService = glossaryService;
    }

    #region Business Schemas

    /// <summary>
    /// Get all business schemas
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<BusinessSchemaDto>>> GetBusinessSchemas()
    {
        try
        {
            _logger.LogInformation("Getting all business schemas");
            var schemas = await _businessSchemaService.GetBusinessSchemasAsync();
            return Ok(schemas);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business schemas");
            return StatusCode(500, new { error = "Failed to retrieve business schemas", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific business schema by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<BusinessSchemaDto>> GetBusinessSchema(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting business schema {SchemaId}", id);
            var schema = await _businessSchemaService.GetBusinessSchemaAsync(id);
            
            if (schema == null)
            {
                return NotFound(new { error = $"Business schema with ID {id} not found" });
            }

            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business schema {SchemaId}", id);
            return StatusCode(500, new { error = "Failed to retrieve business schema", details = ex.Message });
        }
    }

    /// <summary>
    /// Create new business schema
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<BusinessSchemaDto>> CreateBusinessSchema([FromBody] CreateBusinessSchemaRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating business schema {SchemaName} by user {UserId}", request.Name, userId);
            
            var schema = await _businessSchemaService.CreateBusinessSchemaAsync(request, userId);
            return CreatedAtAction(nameof(GetBusinessSchema), new { id = schema.Id }, schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business schema {SchemaName}", request.Name);
            return StatusCode(500, new { error = "Failed to create business schema", details = ex.Message });
        }
    }

    /// <summary>
    /// Update existing business schema
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<ActionResult<BusinessSchemaDto>> UpdateBusinessSchema(Guid id, [FromBody] UpdateBusinessSchemaRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Updating business schema {SchemaId} by user {UserId}", id, userId);
            
            var schema = await _businessSchemaService.UpdateBusinessSchemaAsync(id, request, userId);
            
            if (schema == null)
            {
                return NotFound(new { error = $"Business schema with ID {id} not found" });
            }

            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business schema {SchemaId}", id);
            return StatusCode(500, new { error = "Failed to update business schema", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete business schema
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteBusinessSchema(Guid id)
    {
        try
        {
            _logger.LogInformation("Deleting business schema {SchemaId}", id);
            var result = await _businessSchemaService.DeleteBusinessSchemaAsync(id);
            
            if (!result)
            {
                return NotFound(new { error = $"Business schema with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business schema {SchemaId}", id);
            return StatusCode(500, new { error = "Failed to delete business schema", details = ex.Message });
        }
    }

    /// <summary>
    /// Set schema as default
    /// </summary>
    [HttpPost("{id:guid}/set-default")]
    public async Task<ActionResult> SetDefaultSchema(Guid id)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Setting schema {SchemaId} as default by user {UserId}", id, userId);
            
            var result = await _businessSchemaService.SetDefaultSchemaAsync(id, userId);
            
            if (!result)
            {
                return NotFound(new { error = $"Business schema with ID {id} not found" });
            }

            return Ok(new { message = "Schema set as default successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default schema {SchemaId}", id);
            return StatusCode(500, new { error = "Failed to set default schema", details = ex.Message });
        }
    }

    #endregion

    #region Schema Versions

    /// <summary>
    /// Get all versions for a schema
    /// </summary>
    [HttpGet("{schemaId:guid}/versions")]
    public async Task<ActionResult<List<BusinessSchemaVersionDto>>> GetSchemaVersions(Guid schemaId)
    {
        try
        {
            _logger.LogInformation("Getting versions for schema {SchemaId}", schemaId);
            var versions = await _businessSchemaService.GetSchemaVersionsAsync(schemaId);
            return Ok(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting versions for schema {SchemaId}", schemaId);
            return StatusCode(500, new { error = "Failed to retrieve schema versions", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific schema version
    /// </summary>
    [HttpGet("{schemaId:guid}/versions/{versionId:guid}")]
    public async Task<ActionResult<BusinessSchemaVersionDto>> GetSchemaVersion(Guid schemaId, Guid versionId)
    {
        try
        {
            _logger.LogInformation("Getting version {VersionId} for schema {SchemaId}", versionId, schemaId);
            var version = await _businessSchemaService.GetSchemaVersionAsync(versionId);
            
            if (version == null || version.SchemaId != schemaId)
            {
                return NotFound(new { error = $"Schema version with ID {versionId} not found" });
            }

            return Ok(version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting version {VersionId} for schema {SchemaId}", versionId, schemaId);
            return StatusCode(500, new { error = "Failed to retrieve schema version", details = ex.Message });
        }
    }

    /// <summary>
    /// Create new schema version
    /// </summary>
    [HttpPost("{schemaId:guid}/versions")]
    public async Task<ActionResult<BusinessSchemaVersionDto>> CreateSchemaVersion(Guid schemaId, [FromBody] CreateSchemaVersionRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating new version for schema {SchemaId} by user {UserId}", schemaId, userId);
            
            var version = await _businessSchemaService.CreateSchemaVersionAsync(schemaId, request, userId);
            return CreatedAtAction(nameof(GetSchemaVersion), new { schemaId, versionId = version.Id }, version);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating version for schema {SchemaId}", schemaId);
            return StatusCode(500, new { error = "Failed to create schema version", details = ex.Message });
        }
    }

    /// <summary>
    /// Set version as current
    /// </summary>
    [HttpPost("{schemaId:guid}/versions/{versionId:guid}/set-current")]
    public async Task<ActionResult> SetCurrentVersion(Guid schemaId, Guid versionId)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Setting version {VersionId} as current for schema {SchemaId} by user {UserId}", versionId, schemaId, userId);
            
            var result = await _businessSchemaService.SetCurrentVersionAsync(versionId, userId);
            
            if (!result)
            {
                return NotFound(new { error = $"Schema version with ID {versionId} not found" });
            }

            return Ok(new { message = "Version set as current successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting current version {VersionId} for schema {SchemaId}", versionId, schemaId);
            return StatusCode(500, new { error = "Failed to set current version", details = ex.Message });
        }
    }

    #endregion

    #region Table Contexts

    /// <summary>
    /// Get table contexts for a schema version
    /// </summary>
    [HttpGet("versions/{versionId:guid}/tables")]
    public async Task<ActionResult<List<SchemaTableContextDto>>> GetTableContexts(Guid versionId)
    {
        try
        {
            _logger.LogInformation("Getting table contexts for version {VersionId}", versionId);
            var tables = await _businessSchemaService.GetTableContextsAsync(versionId);
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table contexts for version {VersionId}", versionId);
            return StatusCode(500, new { error = "Failed to retrieve table contexts", details = ex.Message });
        }
    }

    /// <summary>
    /// Create or update table context
    /// </summary>
    [HttpPost("versions/{versionId:guid}/tables")]
    public async Task<ActionResult<SchemaTableContextDto>> CreateTableContext(Guid versionId, [FromBody] CreateTableContextRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating table context for {TableName} in version {VersionId} by user {UserId}", request.TableName, versionId, userId);
            
            var tableContext = await _businessSchemaService.CreateTableContextAsync(versionId, request, userId);
            return CreatedAtAction(nameof(GetTableContexts), new { versionId }, tableContext);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating table context for {TableName} in version {VersionId}", request.TableName, versionId);
            return StatusCode(500, new { error = "Failed to create table context", details = ex.Message });
        }
    }

    #endregion

    #region User Preferences

    /// <summary>
    /// Get user's schema preferences
    /// </summary>
    [HttpGet("preferences")]
    public async Task<ActionResult<List<UserSchemaPreferenceDto>>> GetUserSchemaPreferences()
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Getting schema preferences for user {UserId}", userId);
            
            var preferences = await _businessSchemaService.GetUserSchemaPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogError(ex, "Error getting schema preferences for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to retrieve user schema preferences", details = ex.Message });
        }
    }

    /// <summary>
    /// Set user's default schema
    /// </summary>
    [HttpPost("preferences/default")]
    public async Task<ActionResult> SetUserDefaultSchema([FromBody] SetUserDefaultSchemaRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Setting default schema {SchemaId} for user {UserId}", request.SchemaId, userId);
            
            var result = await _businessSchemaService.SetUserDefaultSchemaAsync(userId, request.SchemaId, request.VersionId);
            
            if (!result)
            {
                return NotFound(new { error = "Schema or version not found" });
            }

            return Ok(new { message = "User default schema set successfully" });
        }
        catch (Exception ex)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogError(ex, "Error setting default schema for user {UserId}", userId);
            return StatusCode(500, new { error = "Failed to set user default schema", details = ex.Message });
        }
    }

    #endregion

    #region AI Generation

    /// <summary>
    /// Generate business context automatically for a schema version
    /// </summary>
    [HttpPost("versions/{versionId:guid}/generate-context")]
    public async Task<ActionResult<SchemaGenerationResultDto>> GenerateBusinessContext(Guid versionId, [FromBody] GenerateContextRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Generating business context for version {VersionId} by user {UserId}", versionId, userId);
            
            var result = await _businessSchemaService.GenerateBusinessContextAsync(versionId, request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating business context for version {VersionId}", versionId);
            return StatusCode(500, new { error = "Failed to generate business context", details = ex.Message });
        }
    }

    /// <summary>
    /// Import business context from existing schema
    /// </summary>
    [HttpPost("versions/{versionId:guid}/import-context")]
    public async Task<ActionResult<SchemaImportResultDto>> ImportBusinessContext(Guid versionId, [FromBody] ImportContextRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Importing business context to version {VersionId} from {SourceType} by user {UserId}", versionId, request.SourceType, userId);
            
            var result = await _businessSchemaService.ImportBusinessContextAsync(versionId, request, userId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing business context to version {VersionId}", versionId);
            return StatusCode(500, new { error = "Failed to import business context", details = ex.Message });
        }
    }

    #endregion

    #region Business Metadata Population

    /// <summary>
    /// Populate business metadata for the specific relevant gaming tables only
    /// </summary>
    /// <param name="useAI">Whether to use AI for metadata generation</param>
    /// <param name="overwriteExisting">Whether to overwrite existing metadata</param>
    /// <returns>Population results</returns>
    [HttpPost("metadata/populate-relevant")]
    public async Task<ActionResult<object>> PopulateRelevantTablesAsync(
        [FromQuery] bool useAI = true,
        [FromQuery] bool overwriteExisting = false)
    {
        try
        {
            _logger.LogInformation("🎯 Starting targeted metadata population for relevant gaming tables - UseAI: {UseAI}, Overwrite: {Overwrite}",
                useAI, overwriteExisting);

            var result = await _populationService.PopulateRelevantTablesAsync(useAI, overwriteExisting);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = $"Successfully processed {result.SuccessCount} relevant gaming tables with {result.ErrorCount} errors",
                    summary = new
                    {
                        totalProcessed = result.ProcessedTables.Count,
                        successful = result.SuccessCount,
                        errors = result.ErrorCount,
                        skipped = result.ProcessedTables.Count(t => t.Skipped),
                        relevantTablesOnly = true
                    },
                    details = result.ProcessedTables.Select(t => new
                    {
                        schema = t.SchemaName,
                        table = t.TableName,
                        success = t.Success,
                        skipped = t.Skipped,
                        error = t.ErrorMessage
                    }),
                    processedAt = DateTime.UtcNow
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Relevant tables metadata population failed",
                    error = result.ErrorMessage
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during relevant tables metadata population");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to populate relevant tables metadata",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Automatically populate business metadata for all tables
    /// </summary>
    /// <param name="useAI">Whether to use AI for metadata generation</param>
    /// <param name="overwriteExisting">Whether to overwrite existing metadata</param>
    /// <returns>Population results</returns>
    [HttpPost("metadata/populate-all")]
    public async Task<ActionResult<object>> PopulateAllTablesAsync(
        [FromQuery] bool useAI = true,
        [FromQuery] bool overwriteExisting = false)
    {
        try
        {
            _logger.LogInformation("🚀 Starting automatic metadata population - UseAI: {UseAI}, Overwrite: {Overwrite}",
                useAI, overwriteExisting);

            var result = await _populationService.PopulateAllTablesAsync(useAI, overwriteExisting);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = $"Successfully processed {result.SuccessCount} tables with {result.ErrorCount} errors",
                    summary = new
                    {
                        totalProcessed = result.ProcessedTables.Count,
                        successful = result.SuccessCount,
                        errors = result.ErrorCount,
                        skipped = result.ProcessedTables.Count(t => t.Skipped)
                    },
                    details = result.ProcessedTables.Select(t => new
                    {
                        schema = t.SchemaName,
                        table = t.TableName,
                        success = t.Success,
                        skipped = t.Skipped,
                        error = t.ErrorMessage
                    }),
                    processedAt = DateTime.UtcNow
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Metadata population failed",
                    error = result.ErrorMessage
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during metadata population");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to populate metadata",
                error = ex.Message
            });
        }
    }

    /// <summary>
    /// Populate metadata for a specific table
    /// </summary>
    /// <param name="schemaName">Schema name</param>
    /// <param name="tableName">Table name</param>
    /// <param name="useAI">Whether to use AI for metadata generation</param>
    /// <param name="overwriteExisting">Whether to overwrite existing metadata</param>
    /// <returns>Table population result</returns>
    [HttpPost("metadata/populate-table/{schemaName}/{tableName}")]
    public async Task<ActionResult<object>> PopulateTableMetadataAsync(
        string schemaName,
        string tableName,
        [FromQuery] bool useAI = true,
        [FromQuery] bool overwriteExisting = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(schemaName) || string.IsNullOrWhiteSpace(tableName))
            {
                return BadRequest("Schema name and table name are required");
            }

            _logger.LogInformation("📊 Populating metadata for table: {Schema}.{Table}", schemaName, tableName);

            var tableInfo = new DatabaseTableInfo
            {
                SchemaName = schemaName,
                TableName = tableName
            };

            var result = await _populationService.PopulateTableMetadataAsync(tableInfo, useAI, overwriteExisting);

            if (result.Success)
            {
                return Ok(new
                {
                    success = true,
                    message = result.Skipped
                        ? $"Table {schemaName}.{tableName} already has metadata (skipped)"
                        : $"Successfully populated metadata for {schemaName}.{tableName}",
                    table = new
                    {
                        schema = result.SchemaName,
                        name = result.TableName,
                        skipped = result.Skipped,
                        businessMetadata = result.BusinessMetadata
                    },
                    processedAt = DateTime.UtcNow
                });
            }
            else
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = $"Failed to populate metadata for {schemaName}.{tableName}",
                    error = result.ErrorMessage
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error populating metadata for table: {Schema}.{Table}", schemaName, tableName);
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to populate table metadata",
                error = ex.Message
            });
        }
    }

    #endregion

    #region Business Metadata Management

    /// <summary>
    /// Get all business tables with optional filtering and pagination
    /// </summary>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="search">Search term for table name or business purpose</param>
    /// <param name="schema">Filter by schema name</param>
    /// <param name="domain">Filter by business domain</param>
    /// <returns>Paginated list of business tables</returns>
    [HttpGet("metadata/tables")]
    public async Task<ActionResult<object>> GetBusinessTables(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? schema = null,
        [FromQuery] string? domain = null)
    {
        try
        {
            _logger.LogInformation("📊 Getting business tables - Page: {Page}, Size: {PageSize}, Search: {Search}",
                page, pageSize, search);

            // Mock implementation for now
            var mockTables = new[]
            {
                new { id = 1, tableName = "SalesData", schemaName = "dbo", businessPurpose = "Track sales transactions", domain = "Sales" },
                new { id = 2, tableName = "CustomerInfo", schemaName = "dbo", businessPurpose = "Store customer information", domain = "Customer" },
                new { id = 3, tableName = "ProductCatalog", schemaName = "dbo", businessPurpose = "Product information and pricing", domain = "Product" }
            };

            return Ok(new
            {
                success = true,
                data = mockTables,
                pagination = new
                {
                    currentPage = page,
                    pageSize = pageSize,
                    totalItems = mockTables.Length,
                    totalPages = 1,
                    hasNextPage = false,
                    hasPreviousPage = false
                },
                filters = new
                {
                    search = search,
                    schema = schema,
                    domain = domain
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting business tables");
            return StatusCode(500, new { error = "Failed to retrieve business tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get population status and statistics
    /// </summary>
    /// <returns>Population status information</returns>
    [HttpGet("metadata/status")]
    public async Task<ActionResult> GetPopulationStatusAsync()
    {
        try
        {
            await Task.CompletedTask;

            return Ok(new
            {
                status = "Ready",
                message = "Business metadata population service is operational",
                capabilities = new
                {
                    aiGeneration = true,
                    ruleBasedGeneration = true,
                    batchProcessing = true,
                    individualTableProcessing = true
                },
                checkedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error checking population status");
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to get population status",
                error = ex.Message
            });
        }
    }

    #endregion

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
    }
}
