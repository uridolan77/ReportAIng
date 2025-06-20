using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Infrastructure.Schema;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for managing business metadata population and management
/// </summary>
[ApiController]
[Route("api/business-metadata")]
[Authorize]
public class BusinessMetadataController : ControllerBase
{
    private readonly ILogger<BusinessMetadataController> _logger;
    private readonly BusinessMetadataPopulationService _populationService;

    public BusinessMetadataController(
        ILogger<BusinessMetadataController> logger,
        BusinessMetadataPopulationService populationService)
    {
        _logger = logger;
        _populationService = populationService;
    }

    /// <summary>
    /// Populate business metadata for the specific relevant gaming tables only
    /// </summary>
    /// <param name="useAI">Whether to use AI for metadata generation</param>
    /// <param name="overwriteExisting">Whether to overwrite existing metadata</param>
    /// <returns>Population results</returns>
    [HttpPost("populate-relevant")]
    public async Task<ActionResult<BusinessMetadataPopulationResult>> PopulateRelevantTablesAsync(
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
    [HttpPost("populate-all")]
    public async Task<ActionResult<BusinessMetadataPopulationResult>> PopulateAllTablesAsync(
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
    [HttpPost("populate-table/{schemaName}/{tableName}")]
    public async Task<ActionResult<TableMetadataResult>> PopulateTableMetadataAsync(
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

    /// <summary>
    /// Get population status and statistics
    /// </summary>
    /// <returns>Population status information</returns>
    [HttpGet("status")]
    public async Task<ActionResult> GetPopulationStatusAsync()
    {
        try
        {
            // This would query the database to get current population status
            // For now, returning a placeholder response
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

    /// <summary>
    /// Preview what metadata would be generated for a table without saving
    /// </summary>
    /// <param name="schemaName">Schema name</param>
    /// <param name="tableName">Table name</param>
    /// <param name="useAI">Whether to use AI for metadata generation</param>
    /// <returns>Preview of generated metadata</returns>
    [HttpGet("preview/{schemaName}/{tableName}")]
    public async Task<ActionResult> PreviewTableMetadataAsync(
        string schemaName,
        string tableName,
        [FromQuery] bool useAI = true)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(schemaName) || string.IsNullOrWhiteSpace(tableName))
            {
                return BadRequest("Schema name and table name are required");
            }

            _logger.LogInformation("👁️ Previewing metadata for table: {Schema}.{Table}", schemaName, tableName);

            // This would generate metadata without saving it
            // For now, returning a placeholder response
            await Task.CompletedTask;

            return Ok(new
            {
                success = true,
                message = $"Preview generated for {schemaName}.{tableName}",
                preview = new
                {
                    schema = schemaName,
                    table = tableName,
                    generationMethod = useAI ? "AI-Generated" : "Rule-Based",
                    estimatedBusinessPurpose = "To be generated based on table analysis",
                    estimatedDomain = "To be classified based on table content",
                    estimatedImportance = 0.5,
                    note = "This is a preview - actual generation will provide detailed metadata"
                },
                generatedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error previewing metadata for table: {Schema}.{Table}", schemaName, tableName);
            return StatusCode(500, new
            {
                success = false,
                message = "Failed to preview table metadata",
                error = ex.Message
            });
        }
    }
}
