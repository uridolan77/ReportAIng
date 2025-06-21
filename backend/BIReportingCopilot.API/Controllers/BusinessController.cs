using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using System.Security.Claims;
using BusinessTableStatistics = BIReportingCopilot.Core.Models.BusinessTableStatistics;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Business information management controller for tables, columns, and business context
/// </summary>
[ApiController]
[Route("api/business")]
[Authorize]
public class BusinessController : ControllerBase
{
    private readonly ILogger<BusinessController> _logger;
    private readonly IBusinessTableManagementService _businessTableService;
    private readonly IGlossaryManagementService _glossaryService;
    private readonly IQueryPatternManagementService _queryPatternService;

    public BusinessController(
        ILogger<BusinessController> logger,
        IBusinessTableManagementService businessTableService,
        IGlossaryManagementService glossaryService,
        IQueryPatternManagementService queryPatternService)
    {
        _logger = logger;
        _businessTableService = businessTableService;
        _glossaryService = glossaryService;
        _queryPatternService = queryPatternService;
    }

    #region Business Tables

    /// <summary>
    /// Get all business tables with their metadata (optimized for performance)
    /// </summary>
    [HttpGet("tables")]
    public async Task<ActionResult<List<BusinessTableInfoOptimizedDto>>> GetBusinessTables()
    {
        try
        {
            _logger.LogInformation("Getting all business tables (optimized)");
            var tables = await _businessTableService.GetBusinessTablesOptimizedAsync();
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business tables");
            return StatusCode(500, new { error = "Failed to retrieve business tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get optimized business tables list (for performance)
    /// </summary>
    [HttpGet("tables/optimized")]
    public async Task<ActionResult<List<BusinessTableInfoOptimizedDto>>> GetBusinessTablesOptimized()
    {
        try
        {
            _logger.LogInformation("Getting optimized business tables list");
            var tables = await _businessTableService.GetBusinessTablesOptimizedAsync();
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimized business tables");
            return StatusCode(500, new { error = "Failed to retrieve business tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific business table by ID
    /// </summary>
    [HttpGet("tables/{id:long}")]
    public async Task<ActionResult<BusinessTableInfoDto>> GetBusinessTable(long id)
    {
        try
        {
            _logger.LogInformation("Getting business table {TableId}", id);
            var table = await _businessTableService.GetBusinessTableAsync(id);
            
            if (table == null)
            {
                return NotFound(new { error = $"Business table with ID {id} not found" });
            }

            return Ok(table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business table {TableId}", id);
            return StatusCode(500, new { error = "Failed to retrieve business table", details = ex.Message });
        }
    }

    /// <summary>
    /// Create new business table
    /// </summary>
    [HttpPost("tables")]
    public async Task<ActionResult<BusinessTableInfoDto>> CreateBusinessTable([FromBody] CreateTableInfoRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating business table {TableName} by user {UserId}", request.TableName, userId);
            
            var table = await _businessTableService.CreateBusinessTableAsync(request, userId);
            return CreatedAtAction(nameof(GetBusinessTable), new { id = table.Id }, table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business table {TableName}", request.TableName);
            return StatusCode(500, new { error = "Failed to create business table", details = ex.Message });
        }
    }

    /// <summary>
    /// Update existing business table
    /// </summary>
    [HttpPut("tables/{id:long}")]
    public async Task<ActionResult<BusinessTableInfoDto>> UpdateBusinessTable(long id, [FromBody] CreateTableInfoRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Updating business table {TableId} by user {UserId}", id, userId);
            
            var table = await _businessTableService.UpdateBusinessTableAsync(id, request, userId);
            
            if (table == null)
            {
                return NotFound(new { error = $"Business table with ID {id} not found" });
            }

            return Ok(table);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business table {TableId}", id);
            return StatusCode(500, new { error = "Failed to update business table", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete business table
    /// </summary>
    [HttpDelete("tables/{id:long}")]
    public async Task<ActionResult> DeleteBusinessTable(long id)
    {
        try
        {
            _logger.LogInformation("Deleting business table {TableId}", id);
            var result = await _businessTableService.DeleteBusinessTableAsync(id);
            
            if (!result)
            {
                return NotFound(new { error = $"Business table with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business table {TableId}", id);
            return StatusCode(500, new { error = "Failed to delete business table", details = ex.Message });
        }
    }

    /// <summary>
    /// Search business tables
    /// </summary>
    [HttpGet("tables/search")]
    public async Task<ActionResult<List<BusinessTableInfoDto>>> SearchBusinessTables([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { error = "Search term is required" });
            }

            _logger.LogInformation("Searching business tables with term: {SearchTerm}", searchTerm);
            var tables = await _businessTableService.SearchBusinessTablesAsync(searchTerm);
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching business tables with term: {SearchTerm}", searchTerm);
            return StatusCode(500, new { error = "Failed to search business tables", details = ex.Message });
        }
    }

    /// <summary>
    /// Get business table statistics
    /// </summary>
    [HttpGet("tables/statistics")]
    public async Task<ActionResult<BusinessTableStatistics>> GetTableStatistics()
    {
        try
        {
            _logger.LogInformation("Getting business table statistics");
            var statistics = await _businessTableService.GetTableStatisticsAsync();
            return Ok(statistics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business table statistics");
            return StatusCode(500, new { error = "Failed to retrieve table statistics", details = ex.Message });
        }
    }

    #endregion

    #region Business Glossary

    /// <summary>
    /// Get all glossary terms with pagination
    /// </summary>
    [HttpGet("glossary")]
    public async Task<ActionResult<object>> GetGlossaryTerms([FromQuery] int page = 1, [FromQuery] int limit = 50, [FromQuery] string? category = null)
    {
        try
        {
            _logger.LogInformation("Getting glossary terms - Page: {Page}, Limit: {Limit}, Category: {Category}", page, limit, category);
            var allTerms = await _glossaryService.GetGlossaryTermsAsync();

            // Filter by category if specified
            if (!string.IsNullOrEmpty(category))
            {
                allTerms = allTerms.Where(t => t.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) == true).ToList();
            }

            // Apply pagination
            var totalCount = allTerms.Count;
            var skip = (page - 1) * limit;
            var paginatedTerms = allTerms.Skip(skip).Take(limit).ToList();

            var response = new
            {
                terms = paginatedTerms,
                total = totalCount
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary terms");
            return StatusCode(500, new { error = "Failed to retrieve glossary terms", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific glossary term by ID
    /// </summary>
    [HttpGet("glossary/{termId}")]
    public async Task<ActionResult<BusinessGlossaryDto>> GetGlossaryTerm(string termId)
    {
        try
        {
            _logger.LogInformation("Getting glossary term {TermId}", termId);
            var term = await _glossaryService.GetGlossaryTermAsync(termId);
            
            if (term == null)
            {
                return NotFound(new { error = $"Glossary term with ID {termId} not found" });
            }

            return Ok(term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary term {TermId}", termId);
            return StatusCode(500, new { error = "Failed to retrieve glossary term", details = ex.Message });
        }
    }

    /// <summary>
    /// Create new glossary term
    /// </summary>
    [HttpPost("glossary")]
    public async Task<ActionResult<BusinessGlossaryDto>> CreateGlossaryTerm([FromBody] BusinessGlossaryDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating glossary term {Term} by user {UserId}", request.Term, userId);
            
            var term = await _glossaryService.CreateGlossaryTermAsync(request, userId);
            return CreatedAtAction(nameof(GetGlossaryTerm), new { termId = term.Id }, term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating glossary term {Term}", request.Term);
            return StatusCode(500, new { error = "Failed to create glossary term", details = ex.Message });
        }
    }

    /// <summary>
    /// Update existing glossary term
    /// </summary>
    [HttpPut("glossary/{id:long}")]
    public async Task<ActionResult<BusinessGlossaryDto>> UpdateGlossaryTerm(long id, [FromBody] BusinessGlossaryDto request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Updating glossary term {TermId} by user {UserId}", id, userId);
            
            var term = await _glossaryService.UpdateGlossaryTermAsync(id, request, userId);
            
            if (term == null)
            {
                return NotFound(new { error = $"Glossary term with ID {id} not found" });
            }

            return Ok(term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating glossary term {TermId}", id);
            return StatusCode(500, new { error = "Failed to update glossary term", details = ex.Message });
        }
    }

    /// <summary>
    /// Delete glossary term
    /// </summary>
    [HttpDelete("glossary/{id:long}")]
    public async Task<ActionResult> DeleteGlossaryTerm(long id)
    {
        try
        {
            _logger.LogInformation("Deleting glossary term {TermId}", id);
            var result = await _glossaryService.DeleteGlossaryTermAsync(id);
            
            if (!result)
            {
                return NotFound(new { error = $"Glossary term with ID {id} not found" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting glossary term {TermId}", id);
            return StatusCode(500, new { error = "Failed to delete glossary term", details = ex.Message });
        }
    }

    /// <summary>
    /// Search glossary terms
    /// </summary>
    [HttpGet("glossary/search")]
    public async Task<ActionResult<List<BusinessGlossaryDto>>> SearchGlossaryTerms([FromQuery] string searchTerm)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return BadRequest(new { error = "Search term is required" });
            }

            _logger.LogInformation("Searching glossary terms with term: {SearchTerm}", searchTerm);
            var terms = await _glossaryService.SearchGlossaryTermsAsync(searchTerm);
            return Ok(terms);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching glossary terms with term: {SearchTerm}", searchTerm);
            return StatusCode(500, new { error = "Failed to search glossary terms", details = ex.Message });
        }
    }

    #endregion

    #region Query Patterns

    /// <summary>
    /// Get all query patterns
    /// </summary>
    [HttpGet("patterns")]
    public async Task<ActionResult<List<QueryPatternDto>>> GetQueryPatterns()
    {
        try
        {
            _logger.LogInformation("Getting all query patterns");
            var patterns = await _queryPatternService.GetQueryPatternsAsync();
            return Ok(patterns);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query patterns");
            return StatusCode(500, new { error = "Failed to retrieve query patterns", details = ex.Message });
        }
    }

    /// <summary>
    /// Get specific query pattern by ID
    /// </summary>
    [HttpGet("patterns/{id:long}")]
    public async Task<ActionResult<QueryPatternDto>> GetQueryPattern(long id)
    {
        try
        {
            _logger.LogInformation("Getting query pattern {PatternId}", id);
            var pattern = await _queryPatternService.GetQueryPatternAsync(id);
            
            if (pattern == null)
            {
                return NotFound(new { error = $"Query pattern with ID {id} not found" });
            }

            return Ok(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query pattern {PatternId}", id);
            return StatusCode(500, new { error = "Failed to retrieve query pattern", details = ex.Message });
        }
    }

    /// <summary>
    /// Create new query pattern
    /// </summary>
    [HttpPost("patterns")]
    public async Task<ActionResult<QueryPatternDto>> CreateQueryPattern([FromBody] CreateQueryPatternRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Creating query pattern {PatternName} by user {UserId}", request.PatternName, userId);
            
            var pattern = await _queryPatternService.CreateQueryPatternAsync(request, userId);
            return CreatedAtAction(nameof(GetQueryPattern), new { id = pattern.Id }, pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating query pattern {PatternName}", request.PatternName);
            return StatusCode(500, new { error = "Failed to create query pattern", details = ex.Message });
        }
    }

    /// <summary>
    /// Update existing query pattern
    /// </summary>
    [HttpPut("patterns/{id:long}")]
    public async Task<ActionResult<QueryPatternDto>> UpdateQueryPattern(long id, [FromBody] CreateQueryPatternRequest request)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "system";
            _logger.LogInformation("Updating query pattern {PatternId} by user {UserId}", id, userId);
            
            var pattern = await _queryPatternService.UpdateQueryPatternAsync(id, request, userId);
            
            if (pattern == null)
            {
                return NotFound(new { error = $"Query pattern with ID {id} not found" });
            }

            return Ok(pattern);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating query pattern {PatternId}", id);
            return StatusCode(500, new { error = "Failed to update query pattern", details = ex.Message });
        }
    }

    #endregion
}
