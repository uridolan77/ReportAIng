using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Tuning;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Infrastructure.Data;
using System.Security.Claims;
using TuningDashboardData = BIReportingCopilot.Core.DTOs.TuningDashboardData;
using CreateQueryPatternRequest = BIReportingCopilot.Core.DTOs.CreateQueryPatternRequest;
using BusinessContextAutoGenerator = BIReportingCopilot.Infrastructure.AI.Management.BusinessContextAutoGenerator;

namespace BIReportingCopilot.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous] // Temporarily allow anonymous access for testing
public class TuningController : ControllerBase
{
    private readonly ITuningService _tuningService;
    private readonly IBusinessTableManagementService _businessTableService;
    private readonly ILogger<TuningController> _logger;
    private readonly BICopilotContext _context;

    public TuningController(ITuningService tuningService, IBusinessTableManagementService businessTableService, ILogger<TuningController> logger, BICopilotContext context)
    {
        _tuningService = tuningService;
        _businessTableService = businessTableService;
        _logger = logger;
        _context = context;
    }

    private string GetCurrentUserId()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var subClaim = User.FindFirst("sub")?.Value;
        var nameClaim = User.FindFirst(ClaimTypes.Name)?.Value;
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

        var result = userId ?? subClaim ?? nameClaim ?? emailClaim ?? "unknown";

        _logger.LogInformation("🔍 TuningController GetCurrentUserId - NameIdentifier: {UserId}, Sub: {SubClaim}, Name: {NameClaim}, Email: {Email}, Result: {Result}",
            userId, subClaim, nameClaim, emailClaim, result);

        return result;
    }

    #region Dashboard

    [HttpGet("dashboard")]
    public async Task<ActionResult<TuningDashboardData>> GetDashboard()
    {
        try
        {
            var dashboard = await _tuningService.GetDashboardDataAsync();
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tuning dashboard data");
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Business Table Info

    [HttpGet("tables")]
    public async Task<ActionResult<List<BusinessTableInfoDto>>> GetBusinessTables()
    {
        try
        {
            var tables = await _businessTableService.GetBusinessTablesAsync();
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business tables");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("tables/{id}")]
    public async Task<ActionResult<BusinessTableInfoDto>> GetBusinessTable(long id)
    {
        try
        {
            var table = await _businessTableService.GetBusinessTableAsync(id);
            if (table == null)
                return NotFound();

            return Ok(table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business table {TableId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("tables")]
    public async Task<ActionResult<BusinessTableInfoDto>> CreateBusinessTable([FromBody] CreateTableInfoRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var table = await _businessTableService.CreateBusinessTableAsync(request, userId);
            return CreatedAtAction(nameof(GetBusinessTable), new { id = table.Id }, table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business table");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("tables/{id}")]
    public async Task<ActionResult<BusinessTableInfoDto>> UpdateBusinessTable(long id, [FromBody] CreateTableInfoRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var table = await _businessTableService.UpdateBusinessTableAsync(id, request, userId);
            if (table == null)
                return NotFound();

            return Ok(table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business table {TableId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("tables/{id}")]
    public async Task<ActionResult> DeleteBusinessTable(long id)
    {
        try
        {
            var success = await _businessTableService.DeleteBusinessTableAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business table {TableId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Query Patterns

    [HttpGet("patterns")]
    public async Task<ActionResult<List<QueryPatternDto>>> GetQueryPatterns()
    {
        try
        {
            var patterns = await _tuningService.GetQueryPatternsAsync();
            return Ok(patterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query patterns");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("patterns/{id}")]
    public async Task<ActionResult<QueryPatternDto>> GetQueryPattern(long id)
    {
        try
        {
            var pattern = await _tuningService.GetQueryPatternAsync(id.ToString());
            if (pattern == null)
                return NotFound();

            return Ok(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query pattern {PatternId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("patterns")]
    public async Task<ActionResult<QueryPatternDto>> CreateQueryPattern([FromBody] CreateQueryPatternRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var createRequest = new BIReportingCopilot.Core.Interfaces.Tuning.CreateQueryPatternRequest
            {
                Name = request.PatternName,
                Pattern = request.NaturalLanguagePattern,
                SqlTemplate = request.SqlTemplate,
                Description = request.Description
            };
            var pattern = await _tuningService.CreateQueryPatternAsync(createRequest, userId);
            return CreatedAtAction(nameof(GetQueryPattern), new { id = pattern.Id }, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating query pattern");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("patterns/{id}")]
    public async Task<ActionResult<QueryPatternDto>> UpdateQueryPattern(long id, [FromBody] CreateQueryPatternRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updateRequest = new BIReportingCopilot.Core.Interfaces.Tuning.UpdateQueryPatternRequest
            {
                Name = request.PatternName,
                Pattern = request.NaturalLanguagePattern,
                SqlTemplate = request.SqlTemplate,
                Description = request.Description
            };
            var pattern = await _tuningService.UpdateQueryPatternAsync(id.ToString(), updateRequest, userId);
            if (pattern == null)
                return NotFound();

            return Ok(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating query pattern {PatternId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("patterns/{id}")]
    public async Task<ActionResult> DeleteQueryPattern(long id)
    {
        try
        {
            var success = await _tuningService.DeleteQueryPatternAsync(id.ToString());
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting query pattern {PatternId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("patterns/{id}/test")]
    public async Task<ActionResult<string>> TestQueryPattern(long id, [FromBody] string naturalLanguageQuery)
    {
        try
        {
            var result = await _tuningService.TestQueryPatternAsync(id.ToString(), naturalLanguageQuery);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing query pattern {PatternId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Business Glossary

    [HttpGet("glossary")]
    public async Task<ActionResult<List<BusinessGlossaryDto>>> GetGlossaryTerms()
    {
        try
        {
            var terms = await _tuningService.GetGlossaryTermsAsync();
            return Ok(terms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary terms");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("glossary")]
    public async Task<ActionResult<BusinessGlossaryDto>> CreateGlossaryTerm([FromBody] BusinessGlossaryDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var createRequest = new BIReportingCopilot.Core.Interfaces.Tuning.CreateGlossaryTermRequest
            {
                Term = request.Term,
                Definition = request.Definition,
                Synonyms = request.Synonyms,
                Category = request.Category
            };
            var term = await _tuningService.CreateGlossaryTermAsync(createRequest, userId);
            return Ok(term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating glossary term");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("glossary/{id}")]
    public async Task<ActionResult<BusinessGlossaryDto>> UpdateGlossaryTerm(long id, [FromBody] BusinessGlossaryDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var updateRequest = new BIReportingCopilot.Core.Interfaces.Tuning.UpdateGlossaryTermRequest
            {
                Term = request.Term,
                Definition = request.Definition,
                Synonyms = request.Synonyms,
                Category = request.Category
            };
            var term = await _tuningService.UpdateGlossaryTermAsync(id.ToString(), updateRequest, userId);
            if (term == null)
                return NotFound();

            return Ok(term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating glossary term {TermId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("glossary/{id}")]
    public async Task<ActionResult> DeleteGlossaryTerm(long id)
    {
        try
        {
            var success = await _tuningService.DeleteGlossaryTermAsync(id.ToString());
            if (!success)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting glossary term {TermId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region AI Settings

    [HttpGet("settings")]
    public async Task<ActionResult<List<AITuningSettingsDto>>> GetAISettings()
    {
        try
        {
            var settings = await _tuningService.GetAISettingsAsync();
            return Ok(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI settings");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("settings/{id}")]
    public async Task<ActionResult<AITuningSettingsDto>> UpdateAISetting(long id, [FromBody] AITuningSettingsDto request)
    {
        try
        {
            var userId = GetCurrentUserId();
            
            // Convert AITuningSettingsDto to UpdateAISettingRequest
            var updateRequest = new BIReportingCopilot.Core.Interfaces.Tuning.UpdateAISettingRequest
            {
                Name = request.SettingKey,
                Value = request.SettingValue,
                Description = request.Description
            };
            
            var setting = await _tuningService.UpdateAISettingAsync(id.ToString(), updateRequest, userId);
            if (setting == null)
                return NotFound();

            return Ok(setting);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI setting {SettingId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Auto-Generation

    [HttpPost("auto-generate")]
    public async Task<ActionResult<AutoGenerationResponse>> AutoGenerateBusinessContext([FromBody] AutoGenerationRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("🚀 NEW CONTROLLER: Starting auto-generation for user {UserId}", userId);
            _logger.LogInformation("🚀 NEW CONTROLLER: Request details - Tables: {Tables}, Fields: {Fields}",
                string.Join(", ", request.SpecificTables ?? new List<string>()),
                request.SpecificFields != null ? string.Join(", ", request.SpecificFields.Select(kv => $"{kv.Key}:[{string.Join(",", kv.Value)}]")) : "None");
            _logger.LogInformation("🚀 NEW CONTROLLER: Full request object: {@Request}", request);

            // Use the new BusinessContextAutoGenerator directly via interface
            var autoGenerator = HttpContext.RequestServices.GetRequiredService<IBusinessContextAutoGenerator>();

            _logger.LogInformation("🤖 Using IBusinessContextAutoGenerator for auto-generation");

            // Cast to concrete implementation to access the new method
            if (autoGenerator is BusinessContextAutoGenerator concreteGenerator)
            {
                var response = await concreteGenerator.GenerateBusinessContextAsync(request, userId);

                _logger.LogInformation("🤖 Auto-generation completed. Success: {Success}, Tables: {Tables}, Terms: {Terms}",
                    response.Success, response.GeneratedTableContexts.Count, response.GeneratedGlossaryTerms.Count);

                return Ok(response);
            }
            else
            {
                _logger.LogError("❌ Failed to cast IBusinessContextAutoGenerator to concrete implementation");
                return StatusCode(500, "Auto-generation service not properly configured");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in auto-generation");
            return StatusCode(500, "Error during auto-generation process");
        }
    }

    [HttpPost("auto-generate/tables")]
    public async Task<ActionResult<List<AutoGeneratedTableContext>>> AutoGenerateTableContexts()
    {
        try
        {
            var userId = GetCurrentUserId();
            var contexts = await _tuningService.AutoGenerateTableContextsAsync(userId);
            return Ok(contexts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-generating table contexts");
            return StatusCode(500, "Error generating table contexts");
        }
    }

    [HttpPost("auto-generate/glossary")]
    public async Task<ActionResult<List<AutoGeneratedGlossaryTerm>>> AutoGenerateGlossaryTerms()
    {
        try
        {
            var userId = GetCurrentUserId();
            var terms = await _tuningService.AutoGenerateGlossaryTermsAsync(userId);
            return Ok(terms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-generating glossary terms");
            return StatusCode(500, "Error generating glossary terms");
        }
    }

    [HttpPost("auto-generate/relationships")]
    public async Task<ActionResult<BusinessRelationshipAnalysis>> AutoGenerateRelationshipAnalysis()
    {
        try
        {
            var userId = GetCurrentUserId();
            var analysis = await _tuningService.AutoGenerateRelationshipAnalysisAsync(userId);
            return Ok(analysis);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-generating relationship analysis");
            return StatusCode(500, "Error generating relationship analysis");
        }
    }

    [HttpPost("auto-generate/table/{tableName}")]
    public async Task<ActionResult<AutoGeneratedTableContext>> AutoGenerateTableContext(string tableName, [FromQuery] string? schemaName = "common")
    {
        try
        {
            var userId = GetCurrentUserId();
            // The interface method only takes userId, so we call it without table/schema parameters
            var context = await _tuningService.AutoGenerateTableContextAsync(userId);
            return Ok(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-generating context for table {Schema}.{Table}", schemaName, tableName);
            return StatusCode(500, "Error generating table context");
        }
    }

    [HttpPost("auto-generate/apply")]
    public async Task<ActionResult> ApplyAutoGeneratedContext([FromBody] AutoGenerationResponse autoGenerated)
    {
        try
        {
            var userId = GetCurrentUserId();
            // The interface expects (string userId, object context, CancellationToken)
            await _tuningService.ApplyAutoGeneratedContextAsync(userId, autoGenerated);
            return Ok(new { message = "Auto-generated context applied successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying auto-generated context");
            return StatusCode(500, "Error applying auto-generated context");
        }
    }

    #endregion

    #region Prompt Templates

    [HttpGet("prompt-templates")]
    public async Task<ActionResult<List<PromptTemplateDto>>> GetPromptTemplates()
    {
        try
        {
            var templates = await _tuningService.GetPromptTemplatesAsync();
            return Ok(templates);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt templates");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("prompt-templates/{id}")]
    public async Task<ActionResult<PromptTemplateDto>> GetPromptTemplate(long id)
    {
        try
        {
            var template = await _tuningService.GetPromptTemplateAsync(id.ToString());
            if (template == null)
                return NotFound();

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt template {TemplateId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("prompt-templates")]
    public async Task<ActionResult<PromptTemplateDto>> CreatePromptTemplate([FromBody] CreatePromptTemplateRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            // The interface expects (object request, CancellationToken)
            var template = await _tuningService.CreatePromptTemplateAsync(request);
            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prompt template");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPut("prompt-templates/{id}")]
    public async Task<ActionResult<PromptTemplateDto>> UpdatePromptTemplate(long id, [FromBody] CreatePromptTemplateRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            var template = await _tuningService.UpdatePromptTemplateAsync(id.ToString(), request);
            if (template == null)
                return NotFound();

            return Ok(template);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prompt template {TemplateId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpDelete("prompt-templates/{id}")]
    public async Task<ActionResult> DeletePromptTemplate(long id)
    {
        try
        {
            await _tuningService.DeletePromptTemplateAsync(id.ToString());
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting prompt template {TemplateId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("prompt-templates/{id}/activate")]
    public async Task<ActionResult> ActivatePromptTemplate(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _tuningService.ActivatePromptTemplateAsync(id.ToString());
            return Ok(new { message = "Template activated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating prompt template {TemplateId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("prompt-templates/{id}/deactivate")]
    public async Task<ActionResult> DeactivatePromptTemplate(long id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _tuningService.DeactivatePromptTemplateAsync(id.ToString());
            return Ok(new { message = "Template deactivated successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating prompt template {TemplateId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpPost("prompt-templates/{id}/test")]
    public async Task<ActionResult<PromptTemplateTestResult>> TestPromptTemplate(long id, [FromBody] PromptTemplateTestRequest request)
    {
        try
        {
            var result = await _tuningService.TestPromptTemplateAsync(id.ToString(), request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing prompt template {TemplateId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion

    #region Prompt Logs (Admin Debugging)

    [HttpPost("update-sql-template")]
    public ActionResult UpdateSqlGenerationTemplate()
    {
        try
        {
            // TODO: Implement SQL template update logic
            _logger.LogInformation("SQL generation template update requested");
            return Ok(new { message = "SQL generation template update functionality not yet implemented" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating SQL generation template");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("prompt-logs")]
    public async Task<ActionResult<IEnumerable<PromptLogDto>>> GetPromptLogs(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? promptType = null,
        [FromQuery] bool? success = null,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var query = _context.PromptLogs.AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(promptType))
                query = query.Where(p => p.PromptType == promptType);

            if (success.HasValue)
                query = query.Where(p => p.Success == success.Value);

            if (fromDate.HasValue)
                query = query.Where(p => p.CreatedDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.CreatedDate <= toDate.Value);

            var logs = await query
                .OrderByDescending(p => p.CreatedDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PromptLogDto
                {
                    Id = p.Id,
                    PromptType = p.PromptType,
                    UserQuery = p.UserQuery,
                    FullPrompt = p.FullPrompt,
                    GeneratedSQL = p.GeneratedSQL,
                    Success = p.Success,
                    ErrorMessage = p.ErrorMessage,
                    PromptLength = p.PromptLength,
                    ResponseLength = p.ResponseLength,
                    ExecutionTimeMs = p.ExecutionTimeMs,
                    CreatedDate = p.CreatedDate,
                    UserId = p.UserId,
                    SessionId = p.SessionId,
                    Metadata = p.Metadata
                })
                .ToListAsync();

            return Ok(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prompt logs");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("prompt-logs/{id}")]
    public async Task<ActionResult<PromptLogDto>> GetPromptLog(long id)
    {
        try
        {
            var log = await _context.PromptLogs
                .Where(p => p.Id == id)
                .Select(p => new PromptLogDto
                {
                    Id = p.Id,
                    PromptType = p.PromptType,
                    UserQuery = p.UserQuery,
                    FullPrompt = p.FullPrompt,
                    GeneratedSQL = p.GeneratedSQL,
                    Success = p.Success,
                    ErrorMessage = p.ErrorMessage,
                    PromptLength = p.PromptLength,
                    ResponseLength = p.ResponseLength,
                    ExecutionTimeMs = p.ExecutionTimeMs,
                    CreatedDate = p.CreatedDate,
                    UserId = p.UserId,
                    SessionId = p.SessionId,
                    Metadata = p.Metadata
                })
                .FirstOrDefaultAsync();

            if (log == null)
                return NotFound();

            return Ok(log);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving prompt log {Id}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    #endregion
}
