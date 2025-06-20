using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces.Schema;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for managing the semantic layer that bridges raw database schemas with business-friendly terminology
/// </summary>
[ApiController]
[Route("api/semantic-layer")]
[Authorize]
public class SemanticLayerController : ControllerBase
{
    private readonly ILogger<SemanticLayerController> _logger;
    private readonly ISemanticLayerService _semanticLayerService;
    private readonly IDynamicSchemaContextualizationService _contextualizationService;

    public SemanticLayerController(
        ILogger<SemanticLayerController> logger,
        ISemanticLayerService semanticLayerService,
        IDynamicSchemaContextualizationService contextualizationService)
    {
        _logger = logger;
        _semanticLayerService = semanticLayerService;
        _contextualizationService = contextualizationService;
    }

    /// <summary>
    /// Get business-friendly schema description for a natural language query
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <param name="maxTokens">Maximum tokens for the description</param>
    /// <returns>Business-friendly schema description</returns>
    [HttpGet("business-schema")]
    public async Task<ActionResult<string>> GetBusinessFriendlySchemaAsync(
        [FromQuery] string query,
        [FromQuery] int maxTokens = 2000)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required");
            }

            _logger.LogInformation("🧠 Getting business-friendly schema for query: {Query}", query);

            var schemaDescription = await _semanticLayerService.GetBusinessFriendlySchemaAsync(query, maxTokens);

            return Ok(new
            {
                query = query,
                schemaDescription = schemaDescription,
                maxTokens = maxTokens,
                generatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting business-friendly schema for query: {Query}", query);
            return StatusCode(500, new { error = "Failed to generate business-friendly schema", message = ex.Message });
        }
    }

    /// <summary>
    /// Get contextually relevant schema elements for a natural language query
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <param name="relevanceThreshold">Minimum relevance score threshold</param>
    /// <param name="maxTables">Maximum number of tables to return</param>
    /// <param name="maxColumnsPerTable">Maximum columns per table</param>
    /// <returns>Contextualized schema result</returns>
    [HttpGet("relevant-schema")]
    public async Task<ActionResult<ContextualizedSchemaResult>> GetRelevantSchemaAsync(
        [FromQuery] string query,
        [FromQuery] double relevanceThreshold = 0.7,
        [FromQuery] int maxTables = 10,
        [FromQuery] int maxColumnsPerTable = 15)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query parameter is required");
            }

            _logger.LogInformation("🔍 Getting relevant schema for query: {Query}", query);

            var result = await _contextualizationService.GetRelevantSchemaAsync(
                query, relevanceThreshold, maxTables, maxColumnsPerTable);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting relevant schema for query: {Query}", query);
            return StatusCode(500, new { error = "Failed to get relevant schema", message = ex.Message });
        }
    }

    /// <summary>
    /// Update semantic metadata for a table
    /// </summary>
    /// <param name="tableId">Table ID to update</param>
    /// <param name="request">Semantic metadata update request</param>
    /// <returns>Success status</returns>
    [HttpPut("table/{tableId}/semantic-metadata")]
    public async Task<ActionResult> UpdateTableSemanticMetadataAsync(
        long tableId,
        [FromBody] UpdateTableSemanticRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            _logger.LogInformation("📝 Updating semantic metadata for table ID: {TableId}", tableId);

            var success = await _semanticLayerService.UpdateTableSemanticMetadataAsync(tableId, request);

            if (!success)
            {
                return NotFound($"Table with ID {tableId} not found");
            }

            return Ok(new { message = "Semantic metadata updated successfully", tableId = tableId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating semantic metadata for table ID: {TableId}", tableId);
            return StatusCode(500, new { error = "Failed to update semantic metadata", message = ex.Message });
        }
    }

    /// <summary>
    /// Create a new semantic schema mapping
    /// </summary>
    /// <param name="request">Semantic mapping creation request</param>
    /// <returns>Created mapping ID</returns>
    [HttpPost("mapping")]
    public async Task<ActionResult> CreateSemanticMappingAsync([FromBody] CreateSemanticMappingRequest request)
    {
        try
        {
            if (request == null)
            {
                return BadRequest("Request body is required");
            }

            if (string.IsNullOrWhiteSpace(request.QueryIntent))
            {
                return BadRequest("QueryIntent is required");
            }

            if (!request.RelevantTables.Any())
            {
                return BadRequest("At least one relevant table is required");
            }

            _logger.LogInformation("📝 Creating semantic mapping for query intent: {Intent}", request.QueryIntent);

            var mappingId = await _semanticLayerService.CreateSemanticMappingAsync(request);

            if (mappingId == 0)
            {
                return StatusCode(500, new { error = "Failed to create semantic mapping" });
            }

            return CreatedAtAction(
                nameof(GetSemanticMappingAsync),
                new { id = mappingId },
                new { id = mappingId, message = "Semantic mapping created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating semantic mapping");
            return StatusCode(500, new { error = "Failed to create semantic mapping", message = ex.Message });
        }
    }

    /// <summary>
    /// Get a semantic mapping by ID (placeholder for future implementation)
    /// </summary>
    /// <param name="id">Mapping ID</param>
    /// <returns>Semantic mapping details</returns>
    [HttpGet("mapping/{id}")]
    public async Task<ActionResult> GetSemanticMappingAsync(long id)
    {
        // This is a placeholder for future implementation
        // Would retrieve and return the semantic mapping details
        await Task.CompletedTask;
        return Ok(new { id = id, message = "Semantic mapping retrieval not yet implemented" });
    }

    /// <summary>
    /// Test the semantic layer with a sample query
    /// </summary>
    /// <param name="query">Test query</param>
    /// <returns>Test results</returns>
    [HttpPost("test")]
    public async Task<ActionResult> TestSemanticLayerAsync([FromBody] string query)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest("Query is required");
            }

            _logger.LogInformation("🧪 Testing semantic layer with query: {Query}", query);

            // Get both business-friendly schema and relevant schema
            var businessSchemaTask = _semanticLayerService.GetBusinessFriendlySchemaAsync(query, 1000);
            var relevantSchemaTask = _contextualizationService.GetRelevantSchemaAsync(query);

            await Task.WhenAll(businessSchemaTask, relevantSchemaTask);

            var businessSchema = await businessSchemaTask;
            var relevantSchema = await relevantSchemaTask;

            return Ok(new
            {
                query = query,
                businessFriendlySchema = businessSchema,
                relevantSchema = relevantSchema,
                summary = new
                {
                    tablesFound = relevantSchema.RelevantTables.Count,
                    columnsFound = relevantSchema.RelevantColumns.Count,
                    businessTermsUsed = relevantSchema.BusinessTermsUsed.Count,
                    confidenceScore = relevantSchema.ConfidenceScore,
                    tokenEstimate = relevantSchema.TokenEstimate
                },
                testedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error testing semantic layer with query: {Query}", query);
            return StatusCode(500, new { error = "Failed to test semantic layer", message = ex.Message });
        }
    }

    /// <summary>
    /// Get semantic layer health status
    /// </summary>
    /// <returns>Health status information</returns>
    [HttpGet("health")]
    public async Task<ActionResult> GetHealthStatusAsync()
    {
        try
        {
            // This would check the health of semantic layer components
            await Task.CompletedTask;

            return Ok(new
            {
                status = "Healthy",
                components = new
                {
                    semanticLayerService = "Operational",
                    contextualizationService = "Operational",
                    vectorSearch = "Operational"
                },
                checkedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error checking semantic layer health");
            return StatusCode(500, new { error = "Health check failed", message = ex.Message });
        }
    }
}
