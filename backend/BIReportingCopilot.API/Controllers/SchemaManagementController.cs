using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.DTOs.SchemaManagement;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SchemaManagementController : ControllerBase
{
    private readonly ISchemaManagementService _schemaManagementService;
    private readonly ILogger<SchemaManagementController> _logger;

    public SchemaManagementController(
        ISchemaManagementService schemaManagementService,
        ILogger<SchemaManagementController> logger)
    {
        _schemaManagementService = schemaManagementService;
        _logger = logger;
    }

    #region Business Schema Management

    /// <summary>
    /// Get all business schemas for the current user
    /// </summary>
    [HttpGet("schemas")]
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
            _logger.LogError(ex, "Error getting business schemas");
            return StatusCode(500, new { message = "An error occurred while retrieving business schemas" });
        }
    }

    /// <summary>
    /// Get a specific business schema by ID
    /// </summary>
    [HttpGet("schemas/{schemaId:guid}")]
    public async Task<ActionResult<BusinessSchemaDto>> GetBusinessSchema(Guid schemaId)
    {
        try
        {
            var userId = GetUserId();
            var schema = await _schemaManagementService.GetBusinessSchemaAsync(schemaId, userId);
            return Ok(schema);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business schema {SchemaId}", schemaId);
            return StatusCode(500, new { message = "An error occurred while retrieving the business schema" });
        }
    }

    /// <summary>
    /// Create a new business schema
    /// </summary>
    [HttpPost("schemas")]
    public async Task<ActionResult<BusinessSchemaDto>> CreateBusinessSchema([FromBody] CreateBusinessSchemaRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var schema = await _schemaManagementService.CreateBusinessSchemaAsync(request, userId);
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

    /// <summary>
    /// Update an existing business schema
    /// </summary>
    [HttpPut("schemas/{schemaId:guid}")]
    public async Task<ActionResult<BusinessSchemaDto>> UpdateBusinessSchema(Guid schemaId, [FromBody] UpdateBusinessSchemaRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var schema = await _schemaManagementService.UpdateBusinessSchemaAsync(schemaId, request, userId);
            return Ok(schema);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business schema {SchemaId}", schemaId);
            return StatusCode(500, new { message = "An error occurred while updating the business schema" });
        }
    }

    /// <summary>
    /// Delete a business schema
    /// </summary>
    [HttpDelete("schemas/{schemaId:guid}")]
    public async Task<ActionResult> DeleteBusinessSchema(Guid schemaId)
    {
        try
        {
            var userId = GetUserId();
            await _schemaManagementService.DeleteBusinessSchemaAsync(schemaId, userId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business schema {SchemaId}", schemaId);
            return StatusCode(500, new { message = "An error occurred while deleting the business schema" });
        }
    }

    /// <summary>
    /// Set a business schema as the default
    /// </summary>
    [HttpPost("schemas/{schemaId:guid}/set-default")]
    public async Task<ActionResult<BusinessSchemaDto>> SetDefaultSchema(Guid schemaId)
    {
        try
        {
            var userId = GetUserId();
            var schema = await _schemaManagementService.SetDefaultSchemaAsync(schemaId, userId);
            return Ok(schema);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting default schema {SchemaId}", schemaId);
            return StatusCode(500, new { message = "An error occurred while setting the default schema" });
        }
    }

    #endregion

    #region Schema Version Management

    /// <summary>
    /// Get all versions for a specific schema
    /// </summary>
    [HttpGet("schemas/{schemaId:guid}/versions")]
    public async Task<ActionResult<List<BusinessSchemaVersionDto>>> GetSchemaVersions(Guid schemaId)
    {
        try
        {
            var userId = GetUserId();
            var versions = await _schemaManagementService.GetSchemaVersionsAsync(schemaId, userId);
            return Ok(versions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema versions for schema {SchemaId}", schemaId);
            return StatusCode(500, new { message = "An error occurred while retrieving schema versions" });
        }
    }

    /// <summary>
    /// Get a specific schema version with full details
    /// </summary>
    [HttpGet("versions/{versionId:guid}")]
    public async Task<ActionResult<DetailedSchemaVersionDto>> GetSchemaVersion(Guid versionId)
    {
        try
        {
            var userId = GetUserId();
            var version = await _schemaManagementService.GetSchemaVersionAsync(versionId, userId);
            return Ok(version);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema version {VersionId}", versionId);
            return StatusCode(500, new { message = "An error occurred while retrieving the schema version" });
        }
    }

    /// <summary>
    /// Create a new schema version
    /// </summary>
    [HttpPost("versions")]
    public async Task<ActionResult<BusinessSchemaVersionDto>> CreateSchemaVersion([FromBody] CreateSchemaVersionRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var version = await _schemaManagementService.CreateSchemaVersionAsync(request, userId);
            return CreatedAtAction(nameof(GetSchemaVersion), new { versionId = version.Id }, version);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating schema version");
            return StatusCode(500, new { message = "An error occurred while creating the schema version" });
        }
    }

    /// <summary>
    /// Set a schema version as current
    /// </summary>
    [HttpPost("versions/{versionId:guid}/set-current")]
    public async Task<ActionResult<BusinessSchemaVersionDto>> SetCurrentVersion(Guid versionId)
    {
        try
        {
            var userId = GetUserId();
            var version = await _schemaManagementService.SetCurrentVersionAsync(versionId, userId);
            return Ok(version);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting current version {VersionId}", versionId);
            return StatusCode(500, new { message = "An error occurred while setting the current version" });
        }
    }

    /// <summary>
    /// Delete a schema version
    /// </summary>
    [HttpDelete("versions/{versionId:guid}")]
    public async Task<ActionResult> DeleteSchemaVersion(Guid versionId)
    {
        try
        {
            var userId = GetUserId();
            await _schemaManagementService.DeleteSchemaVersionAsync(versionId, userId);
            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting schema version {VersionId}", versionId);
            return StatusCode(500, new { message = "An error occurred while deleting the schema version" });
        }
    }

    #endregion

    #region Apply Auto-Generated Content

    /// <summary>
    /// Apply auto-generated content to a schema (new or existing)
    /// </summary>
    [HttpPost("apply")]
    public async Task<ActionResult<BusinessSchemaVersionDto>> ApplyToSchema([FromBody] ApplyToSchemaRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var version = await _schemaManagementService.ApplyToSchemaAsync(request, userId);
            return Ok(version);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying auto-generated content to schema");
            return StatusCode(500, new { message = "An error occurred while applying the auto-generated content" });
        }
    }

    #endregion

    #region Schema Content Management

    /// <summary>
    /// Update a table context
    /// </summary>
    [HttpPut("table-contexts/{tableContextId:guid}")]
    public async Task<ActionResult<SchemaTableContextDto>> UpdateTableContext(Guid tableContextId, [FromBody] SchemaTableContextDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var tableContext = await _schemaManagementService.UpdateTableContextAsync(tableContextId, request, userId);
            return Ok(tableContext);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table context {TableContextId}", tableContextId);
            return StatusCode(500, new { message = "An error occurred while updating the table context" });
        }
    }

    /// <summary>
    /// Update a column context
    /// </summary>
    [HttpPut("column-contexts/{columnContextId:guid}")]
    public async Task<ActionResult<SchemaColumnContextDto>> UpdateColumnContext(Guid columnContextId, [FromBody] SchemaColumnContextDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var columnContext = await _schemaManagementService.UpdateColumnContextAsync(columnContextId, request, userId);
            return Ok(columnContext);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating column context {ColumnContextId}", columnContextId);
            return StatusCode(500, new { message = "An error occurred while updating the column context" });
        }
    }

    /// <summary>
    /// Update a glossary term
    /// </summary>
    [HttpPut("glossary-terms/{termId:guid}")]
    public async Task<ActionResult<SchemaGlossaryTermDto>> UpdateGlossaryTerm(Guid termId, [FromBody] SchemaGlossaryTermDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var term = await _schemaManagementService.UpdateGlossaryTermAsync(termId, request, userId);
            return Ok(term);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating glossary term {TermId}", termId);
            return StatusCode(500, new { message = "An error occurred while updating the glossary term" });
        }
    }

    /// <summary>
    /// Update a relationship
    /// </summary>
    [HttpPut("relationships/{relationshipId:guid}")]
    public async Task<ActionResult<SchemaRelationshipDto>> UpdateRelationship(Guid relationshipId, [FromBody] SchemaRelationshipDto request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var relationship = await _schemaManagementService.UpdateRelationshipAsync(relationshipId, request, userId);
            return Ok(relationship);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating relationship {RelationshipId}", relationshipId);
            return StatusCode(500, new { message = "An error occurred while updating the relationship" });
        }
    }

    #endregion

    #region User Preferences

    /// <summary>
    /// Get user schema preferences
    /// </summary>
    [HttpGet("user-preferences")]
    public async Task<ActionResult<List<UserSchemaPreferenceDto>>> GetUserSchemaPreferences()
    {
        try
        {
            var userId = GetUserId();
            var preferences = await _schemaManagementService.GetUserSchemaPreferencesAsync(userId);
            return Ok(preferences);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user schema preferences");
            return StatusCode(500, new { message = "An error occurred while retrieving user schema preferences" });
        }
    }

    /// <summary>
    /// Set user default schema
    /// </summary>
    [HttpPost("user-preferences/default/{schemaVersionId:guid}")]
    public async Task<ActionResult<UserSchemaPreferenceDto>> SetUserDefaultSchema(Guid schemaVersionId)
    {
        try
        {
            var userId = GetUserId();
            var preference = await _schemaManagementService.SetUserDefaultSchemaAsync(schemaVersionId, userId);
            return Ok(preference);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting user default schema {SchemaVersionId}", schemaVersionId);
            return StatusCode(500, new { message = "An error occurred while setting the user default schema" });
        }
    }

    /// <summary>
    /// Get user default schema
    /// </summary>
    [HttpGet("user-preferences/default")]
    public async Task<ActionResult<BusinessSchemaVersionDto>> GetUserDefaultSchema()
    {
        try
        {
            var userId = GetUserId();
            var defaultSchema = await _schemaManagementService.GetUserDefaultSchemaAsync(userId);

            if (defaultSchema == null)
            {
                return NotFound(new { message = "No default schema found for user" });
            }

            return Ok(defaultSchema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user default schema");
            return StatusCode(500, new { message = "An error occurred while retrieving the user default schema" });
        }
    }

    #endregion

    #region Schema Comparison and Export/Import

    /// <summary>
    /// Compare two schema versions
    /// </summary>
    [HttpGet("compare/{sourceVersionId:guid}/{targetVersionId:guid}")]
    public async Task<ActionResult<SchemaComparisonDto>> CompareSchemaVersions(Guid sourceVersionId, Guid targetVersionId)
    {
        try
        {
            var userId = GetUserId();
            var comparison = await _schemaManagementService.CompareSchemaVersionsAsync(sourceVersionId, targetVersionId, userId);
            return Ok(comparison);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error comparing schema versions {SourceVersionId} and {TargetVersionId}", sourceVersionId, targetVersionId);
            return StatusCode(500, new { message = "An error occurred while comparing schema versions" });
        }
    }

    /// <summary>
    /// Export a schema version
    /// </summary>
    [HttpGet("export/{versionId:guid}")]
    public async Task<ActionResult<DetailedSchemaVersionDto>> ExportSchemaVersion(Guid versionId)
    {
        try
        {
            var userId = GetUserId();
            var schemaData = await _schemaManagementService.ExportSchemaVersionAsync(versionId, userId);
            return Ok(schemaData);
        }
        catch (ArgumentException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting schema version {VersionId}", versionId);
            return StatusCode(500, new { message = "An error occurred while exporting the schema version" });
        }
    }

    /// <summary>
    /// Import a schema version
    /// </summary>
    [HttpPost("import/{schemaId:guid}")]
    public async Task<ActionResult<BusinessSchemaVersionDto>> ImportSchemaVersion(Guid schemaId, [FromBody] DetailedSchemaVersionDto schemaData)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetUserId();
            var version = await _schemaManagementService.ImportSchemaVersionAsync(schemaId, schemaData, userId);
            return Ok(version);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing schema version to schema {SchemaId}", schemaId);
            return StatusCode(500, new { message = "An error occurred while importing the schema version" });
        }
    }

    #endregion

    #region Helper Methods

    private string GetUserId()
    {
        return User.Identity?.Name ?? User.FindFirst("sub")?.Value ?? "anonymous";
    }

    #endregion
}
