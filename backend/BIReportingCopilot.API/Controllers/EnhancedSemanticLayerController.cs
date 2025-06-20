using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Enhanced Semantic Layer Controller - Phase 2 semantic enrichment functionality
/// </summary>
[ApiController]
[Route("api/semantic")]
[Authorize]
public class EnhancedSemanticLayerController : ControllerBase
{
    private readonly ILogger<EnhancedSemanticLayerController> _logger;
    private readonly IEnhancedSemanticLayerService _enhancedSemanticService;

    public EnhancedSemanticLayerController(
        ILogger<EnhancedSemanticLayerController> logger,
        IEnhancedSemanticLayerService enhancedSemanticService)
    {
        _logger = logger;
        _enhancedSemanticService = enhancedSemanticService;
    }

    /// <summary>
    /// Get semantically enriched schema for a natural language query
    /// </summary>
    /// <param name="request">Semantic enrichment request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced schema result with semantic enrichment</returns>
    [HttpPost("analyze")]
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
    [HttpPost("enrich")]
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
    /// Update semantic metadata for a specific table
    /// </summary>
    /// <param name="tableName">Name of the table to update</param>
    /// <param name="schemaName">Schema name</param>
    /// <param name="metadata">Semantic metadata to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    [HttpPut("table/{schemaName}/{tableName}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<bool>> UpdateTableSemanticMetadata(
        [FromRoute] string tableName,
        [FromRoute] string schemaName,
        [FromBody] TableSemanticMetadata metadata,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Updating semantic metadata for table: {Schema}.{Table}", schemaName, tableName);

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

            _logger.LogInformation("✅ Table semantic metadata updated successfully");
            return Ok(success);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "⚠️ Invalid request for table metadata update");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating table semantic metadata");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Metadata Update Error",
                Detail = "An error occurred while updating table semantic metadata",
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
    [HttpPut("column/{tableName}/{columnName}")]
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
    [HttpPost("embeddings/generate")]
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
    [HttpGet("validate")]
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
    [HttpGet("glossary/relevant")]
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
}
