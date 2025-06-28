using BIReportingCopilot.Infrastructure.AI.Management;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// API controller for metadata enhancement operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MetadataEnhancementController : ControllerBase
{
    private readonly MetadataEnhancementService _enhancementService;
    private readonly ILogger<MetadataEnhancementController> _logger;

    public MetadataEnhancementController(
        MetadataEnhancementService enhancementService,
        ILogger<MetadataEnhancementController> logger)
    {
        _enhancementService = enhancementService;
        _logger = logger;
    }

    /// <summary>
    /// Start metadata enhancement process
    /// </summary>
    /// <param name="request">Enhancement configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhancement result</returns>
    [HttpPost("enhance")]
    public async Task<ActionResult<MetadataEnhancementService.MetadataEnhancementResult>> EnhanceMetadataAsync(
        [FromBody] MetadataEnhancementService.MetadataEnhancementRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            
            _logger.LogInformation("🚀 Starting metadata enhancement for user {UserId}", userId);

            var result = await _enhancementService.EnhanceMetadataAsync(request, userId, cancellationToken);

            if (result.Success)
            {
                _logger.LogInformation("✅ Metadata enhancement completed successfully - Enhanced {Fields} fields", 
                    result.FieldsEnhanced);
                return Ok(result);
            }
            else
            {
                _logger.LogWarning("⚠️ Metadata enhancement completed with issues: {Message}", result.Message);
                return BadRequest(result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during metadata enhancement");
            return StatusCode(500, new { 
                Success = false, 
                Message = "Internal server error during metadata enhancement",
                Error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Preview metadata enhancement without making changes
    /// </summary>
    /// <param name="request">Enhancement configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Preview result</returns>
    [HttpPost("preview")]
    public async Task<ActionResult<MetadataEnhancementService.MetadataEnhancementResult>> PreviewEnhancementAsync(
        [FromBody] MetadataEnhancementService.MetadataEnhancementRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            
            // Force preview mode
            request.PreviewOnly = true;
            
            _logger.LogInformation("👁️ Previewing metadata enhancement for user {UserId}", userId);

            var result = await _enhancementService.EnhanceMetadataAsync(request, userId, cancellationToken);

            _logger.LogInformation("📋 Preview completed - Would enhance {Fields} fields across {Records} records", 
                result.FieldsEnhanced, 
                result.ColumnsProcessed + result.TablesProcessed + result.GlossaryTermsProcessed);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during metadata enhancement preview");
            return StatusCode(500, new { 
                Success = false, 
                Message = "Internal server error during preview",
                Error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Get enhancement status and statistics
    /// </summary>
    /// <returns>Enhancement statistics</returns>
    [HttpGet("status")]
    [AllowAnonymous] // Allow anonymous access for testing
    public ActionResult<object> GetEnhancementStatus()
    {
        try
        {
            // This is a simplified status endpoint
            // In a full implementation, this would query actual statistics from the database
            
            var status = new
            {
                ServiceStatus = "Available",
                LastEnhancement = DateTime.UtcNow.AddHours(-2), // Mock data
                TotalEnhancements = 156, // Mock data
                AvailableModes = new[]
                {
                    "EmptyFieldsOnly",
                    "Enhancement", 
                    "Selective"
                },
                SupportedEntities = new[]
                {
                    "BusinessColumnInfo",
                    "BusinessTableInfo", 
                    "BusinessGlossary"
                }
            };

            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting enhancement status");
            return StatusCode(500, new { 
                Message = "Internal server error getting status",
                Error = ex.Message 
            });
        }
    }

    /// <summary>
    /// Get available enhancement modes and their descriptions
    /// </summary>
    /// <returns>Enhancement modes information</returns>
    [HttpGet("modes")]
    public ActionResult<object> GetEnhancementModes()
    {
        try
        {
            var modes = new[]
            {
                new
                {
                    Mode = "EmptyFieldsOnly",
                    Description = "Only populate empty semantic fields (recommended for initial setup)",
                    IsDefault = true,
                    RiskLevel = "Low"
                },
                new
                {
                    Mode = "Enhancement",
                    Description = "Improve existing content in addition to filling empty fields",
                    IsDefault = false,
                    RiskLevel = "Medium"
                },
                new
                {
                    Mode = "Selective",
                    Description = "Target specific fields or tables for enhancement",
                    IsDefault = false,
                    RiskLevel = "Low"
                }
            };

            return Ok(new { 
                AvailableModes = modes,
                RecommendedMode = "EmptyFieldsOnly",
                DefaultBatchSize = 50
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting enhancement modes");
            return StatusCode(500, new { 
                Message = "Internal server error getting modes",
                Error = ex.Message 
            });
        }
    }
}
