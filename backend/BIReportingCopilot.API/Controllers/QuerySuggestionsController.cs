using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for managing query suggestions
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuerySuggestionsController : ControllerBase
{
    private readonly IQuerySuggestionService _suggestionService;
    private readonly ILogger<QuerySuggestionsController> _logger;

    public QuerySuggestionsController(
        IQuerySuggestionService suggestionService,
        ILogger<QuerySuggestionsController> logger)
    {
        _suggestionService = suggestionService;
        _logger = logger;
    }

    /// <summary>
    /// Get all suggestion categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<List<SuggestionCategoryDto>>> GetCategories(
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var categories = await _suggestionService.GetCategoriesAsync(includeInactive);
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suggestion categories");
            return StatusCode(500, "An error occurred while retrieving categories");
        }
    }

    /// <summary>
    /// Get suggestions grouped by category
    /// </summary>
    [HttpGet("grouped")]
    public async Task<ActionResult<List<GroupedSuggestionsDto>>> GetGroupedSuggestions(
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var groupedSuggestions = await _suggestionService.GetGroupedSuggestionsAsync(includeInactive);
            return Ok(groupedSuggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving grouped suggestions");
            return StatusCode(500, "An error occurred while retrieving suggestions");
        }
    }

    /// <summary>
    /// Get suggestions by category
    /// </summary>
    [HttpGet("category/{categoryKey}")]
    public async Task<ActionResult<List<QuerySuggestionDto>>> GetSuggestionsByCategory(
        string categoryKey,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var suggestions = await _suggestionService.GetSuggestionsByCategoryAsync(categoryKey, includeInactive);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving suggestions for category {CategoryKey}", categoryKey);
            return StatusCode(500, "An error occurred while retrieving suggestions");
        }
    }

    /// <summary>
    /// Search suggestions with filters
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<SuggestionSearchResultDto>> SearchSuggestions(
        [FromBody] SuggestionSearchDto searchDto)
    {
        try
        {
            var results = await _suggestionService.SearchSuggestionsAsync(searchDto);
            return Ok(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching suggestions");
            return StatusCode(500, "An error occurred while searching suggestions");
        }
    }

    /// <summary>
    /// Get popular suggestions
    /// </summary>
    [HttpGet("popular")]
    public async Task<ActionResult<List<QuerySuggestionDto>>> GetPopularSuggestions(
        [FromQuery] int count = 10)
    {
        try
        {
            var suggestions = await _suggestionService.GetPopularSuggestionsAsync(count);
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving popular suggestions");
            return StatusCode(500, "An error occurred while retrieving popular suggestions");
        }
    }

    /// <summary>
    /// Get available time frames
    /// </summary>
    [HttpGet("timeframes")]
    public async Task<ActionResult<List<TimeFrameDefinitionDto>>> GetTimeFrames(
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var timeFrames = await _suggestionService.GetTimeFramesAsync(includeInactive);
            return Ok(timeFrames);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving time frames");
            return StatusCode(500, "An error occurred while retrieving time frames");
        }
    }

    /// <summary>
    /// Record suggestion usage
    /// </summary>
    [HttpPost("usage")]
    public async Task<ActionResult> RecordUsage([FromBody] RecordSuggestionUsageDto dto)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            await _suggestionService.RecordUsageAsync(dto, userId);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording suggestion usage");
            return StatusCode(500, "An error occurred while recording usage");
        }
    }

    // Admin-only endpoints
    /// <summary>
    /// Create a new suggestion category (Admin only)
    /// </summary>
    [HttpPost("categories")]
    [Authorize(Policy = ApplicationConstants.Permissions.ManageSchema)]
    public async Task<ActionResult<SuggestionCategoryDto>> CreateCategory(
        [FromBody] CreateUpdateSuggestionCategoryDto dto)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            var category = await _suggestionService.CreateCategoryAsync(dto, userId);
            return CreatedAtAction(nameof(GetCategories), new { id = category.Id }, category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating suggestion category");
            return StatusCode(500, "An error occurred while creating the category");
        }
    }

    /// <summary>
    /// Update a suggestion category (Admin only)
    /// </summary>
    [HttpPut("categories/{id}")]
    [Authorize(Policy = ApplicationConstants.Permissions.ManageSchema)]
    public async Task<ActionResult<SuggestionCategoryDto>> UpdateCategory(
        long id,
        [FromBody] CreateUpdateSuggestionCategoryDto dto)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            var category = await _suggestionService.UpdateCategoryAsync(id, dto, userId);
            
            if (category == null)
                return NotFound();

            return Ok(category);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating suggestion category {Id}", id);
            return StatusCode(500, "An error occurred while updating the category");
        }
    }

    /// <summary>
    /// Create a new suggestion (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Policy = ApplicationConstants.Permissions.ManageSchema)]
    public async Task<ActionResult<QuerySuggestionDto>> CreateSuggestion(
        [FromBody] CreateUpdateQuerySuggestionDto dto)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            var suggestion = await _suggestionService.CreateSuggestionAsync(dto, userId);
            return CreatedAtAction(nameof(GetSuggestionsByCategory), 
                new { categoryKey = suggestion.CategoryKey }, suggestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating suggestion");
            return StatusCode(500, "An error occurred while creating the suggestion");
        }
    }

    /// <summary>
    /// Update a suggestion (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Policy = ApplicationConstants.Permissions.ManageSchema)]
    public async Task<ActionResult<QuerySuggestionDto>> UpdateSuggestion(
        long id,
        [FromBody] CreateUpdateQuerySuggestionDto dto)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            var suggestion = await _suggestionService.UpdateSuggestionAsync(id, dto, userId);
            
            if (suggestion == null)
                return NotFound();

            return Ok(suggestion);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating suggestion {Id}", id);
            return StatusCode(500, "An error occurred while updating the suggestion");
        }
    }

    /// <summary>
    /// Delete a suggestion (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = ApplicationConstants.Permissions.ManageSchema)]
    public async Task<ActionResult> DeleteSuggestion(long id)
    {
        try
        {
            var userId = User.Identity?.Name ?? "Unknown";
            var deleted = await _suggestionService.DeleteSuggestionAsync(id, userId);
            
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting suggestion {Id}", id);
            return StatusCode(500, "An error occurred while deleting the suggestion");
        }
    }

    /// <summary>
    /// Get usage analytics (Admin only)
    /// </summary>
    [HttpGet("analytics")]
    [Authorize(Policy = ApplicationConstants.Permissions.ViewSystemStats)]
    public async Task<ActionResult<List<SuggestionUsageAnalyticsDto>>> GetUsageAnalytics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null)
    {
        try
        {
            var analytics = await _suggestionService.GetUsageAnalyticsAsync(fromDate, toDate);
            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving usage analytics");
            return StatusCode(500, "An error occurred while retrieving analytics");
        }
    }
}
