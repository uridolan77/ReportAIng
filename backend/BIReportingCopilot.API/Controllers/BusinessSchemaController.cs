using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Business schema management controller for versioned schema definitions
/// </summary>
[ApiController]
[Route("api/business-schemas")]
[Authorize]
public class BusinessSchemaController : ControllerBase
{
    private readonly ILogger<BusinessSchemaController> _logger;
    private readonly IBusinessSchemaService _businessSchemaService;

    public BusinessSchemaController(
        ILogger<BusinessSchemaController> logger,
        IBusinessSchemaService businessSchemaService)
    {
        _logger = logger;
        _businessSchemaService = businessSchemaService;
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
}
