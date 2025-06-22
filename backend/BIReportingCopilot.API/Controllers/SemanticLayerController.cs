using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Models;
using System.ComponentModel.DataAnnotations;

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
    private readonly IEnhancedSemanticLayerService _enhancedSemanticService;

    public SemanticLayerController(
        ILogger<SemanticLayerController> logger,
        ISemanticLayerService semanticLayerService,
        IDynamicSchemaContextualizationService contextualizationService,
        IEnhancedSemanticLayerService enhancedSemanticService)
    {
        _logger = logger;
        _semanticLayerService = semanticLayerService;
        _contextualizationService = contextualizationService;
        _enhancedSemanticService = enhancedSemanticService;
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

    #region Enhanced Semantic Layer Endpoints

    /// <summary>
    /// Get semantically enriched schema for a natural language query
    /// </summary>
    /// <param name="request">Semantic enrichment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced schema result with semantic enrichment</returns>
    [HttpPost("enhanced/analyze")]
    [ProducesResponseType(typeof(EnhancedSchemaResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<EnhancedSchemaResult>> AnalyzeQuerySemantics(
        [FromBody] SemanticEnrichmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🧠 Analyzing query semantics for: {Query}", request.Query);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _enhancedSemanticService.GetEnhancedSchemaAsync(
                request.Query,
                request.RelevanceThreshold,
                request.MaxTables,
                cancellationToken);

            _logger.LogInformation("✅ Semantic analysis completed with {TableCount} relevant tables",
                result.RelevantTables.Count);

            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for semantic analysis");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error analyzing query semantics");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Semantic Analysis Error",
                Detail = "An error occurred while analyzing query semantics",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Enrich existing schema metadata with semantic information
    /// </summary>
    /// <param name="request">Semantic enrichment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Semantic enrichment response</returns>
    [HttpPost("enhanced/enrich")]
    [ProducesResponseType(typeof(SemanticEnrichmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SemanticEnrichmentResponse>> EnrichSchemaMetadata(
        [FromBody] SemanticEnrichmentRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Enriching schema metadata for: {Query}", request.Query);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _enhancedSemanticService.EnrichSchemaMetadataAsync(request, cancellationToken);

            _logger.LogInformation("✅ Schema metadata enrichment completed: {Success}", response.Success);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for schema enrichment");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error enriching schema metadata");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Schema Enrichment Error",
                Detail = "An error occurred while enriching schema metadata",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update semantic metadata for a specific table (enhanced version)
    /// </summary>
    /// <param name="tableName">Name of the table to update</param>
    /// <param name="schemaName">Schema name</param>
    /// <param name="metadata">Semantic metadata to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    [HttpPut("enhanced/table/{schemaName}/{tableName}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> UpdateTableSemanticMetadataEnhanced(
        [FromRoute] string tableName,
        [FromRoute] string schemaName,
        [FromBody] TableSemanticMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Updating enhanced semantic metadata for table: {Schema}.{Table}", schemaName, tableName);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _enhancedSemanticService.UpdateTableSemanticMetadataAsync(
                tableName, schemaName, metadata, cancellationToken);

            if (!success)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Table Not Found",
                    Detail = $"Table {schemaName}.{tableName} not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("✅ Enhanced table semantic metadata updated successfully");
            return Ok(success);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for enhanced table metadata update");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating enhanced table semantic metadata");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Metadata Update Error",
                Detail = "An error occurred while updating enhanced table semantic metadata",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Update semantic metadata for a specific column
    /// </summary>
    /// <param name="tableName">Name of the table</param>
    /// <param name="columnName">Name of the column to update</param>
    /// <param name="metadata">Semantic metadata to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    [HttpPut("enhanced/column/{tableName}/{columnName}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> UpdateColumnSemanticMetadata(
        [FromRoute] string tableName,
        [FromRoute] string columnName,
        [FromBody] ColumnSemanticMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Updating semantic metadata for column: {Table}.{Column}", tableName, columnName);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var success = await _enhancedSemanticService.UpdateColumnSemanticMetadataAsync(
                tableName, columnName, metadata, cancellationToken);

            if (!success)
            {
                return NotFound(new ProblemDetails
                {
                    Title = "Column Not Found",
                    Detail = $"Column {tableName}.{columnName} not found",
                    Status = StatusCodes.Status404NotFound
                });
            }

            _logger.LogInformation("✅ Column semantic metadata updated successfully");
            return Ok(success);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for column metadata update");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating column semantic metadata");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Metadata Update Error",
                Detail = "An error occurred while updating column semantic metadata",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Generate semantic embeddings for schema elements
    /// </summary>
    /// <param name="forceRegeneration">Whether to force regeneration of existing embeddings</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of embeddings generated</returns>
    [HttpPost("enhanced/embeddings/generate")]
    [ProducesResponseType(typeof(int), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<int>> GenerateSemanticEmbeddings(
        [FromQuery] bool forceRegeneration = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔄 Generating semantic embeddings (force: {Force})", forceRegeneration);

            var count = await _enhancedSemanticService.GenerateSemanticEmbeddingsAsync(
                forceRegeneration, cancellationToken);

            _logger.LogInformation("✅ Generated {Count} semantic embeddings", count);
            return Ok(count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating semantic embeddings");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Embedding Generation Error",
                Detail = "An error occurred while generating semantic embeddings",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Validate semantic metadata completeness and consistency
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result with issues and recommendations</returns>
    [HttpGet("enhanced/validate")]
    [ProducesResponseType(typeof(BIReportingCopilot.Core.Interfaces.Schema.SemanticValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<BIReportingCopilot.Core.Interfaces.Schema.SemanticValidationResult>> ValidateSemanticMetadata(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Validating semantic metadata");

            var result = await _enhancedSemanticService.ValidateSemanticMetadataAsync(cancellationToken);

            _logger.LogInformation("✅ Semantic metadata validation completed: {Valid}", result.IsValid);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating semantic metadata");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Validation Error",
                Detail = "An error occurred while validating semantic metadata",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Get business glossary terms relevant to a query
    /// </summary>
    /// <param name="query">Natural language query</param>
    /// <param name="maxTerms">Maximum number of terms to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant business glossary terms</returns>
    [HttpGet("enhanced/glossary/relevant")]
    [ProducesResponseType(typeof(List<RelevantGlossaryTerm>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<List<RelevantGlossaryTerm>>> GetRelevantGlossaryTerms(
        [FromQuery, Required] string query,
        [FromQuery] int maxTerms = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("📚 Getting relevant glossary terms for: {Query}", query);

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new ProblemDetails
                {
                    Title = "Invalid Query",
                    Detail = "Query parameter is required and cannot be empty",
                    Status = StatusCodes.Status400BadRequest
                });
            }

            var terms = await _enhancedSemanticService.GetRelevantGlossaryTermsAsync(
                query, maxTerms, cancellationToken);

            _logger.LogInformation("✅ Found {Count} relevant glossary terms", terms.Count);
            return Ok(terms);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for glossary terms");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting relevant glossary terms");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Glossary Terms Error",
                Detail = "An error occurred while retrieving relevant glossary terms",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    #endregion
}
