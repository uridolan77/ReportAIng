using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.QuerySuggestions;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.Infrastructure.Query;

public class QuerySuggestionService : IQuerySuggestionService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<QuerySuggestionService> _logger;

    public QuerySuggestionService(
        BICopilotContext context,
        ILogger<QuerySuggestionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    // Category Management
    public async Task<List<SuggestionCategoryDto>> GetCategoriesAsync(bool includeInactive = false)
    {
        var query = _context.SuggestionCategories.AsQueryable();
        
        if (!includeInactive)
            query = query.Where(c => c.IsActive);

        var categories = await query
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Title)
            .Select(c => new SuggestionCategoryDto
            {
                Id = c.Id,
                CategoryKey = c.CategoryKey,
                Title = c.Title,
                Icon = c.Icon,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                SuggestionCount = c.Suggestions.Count(s => s.IsActive)
            })
            .ToListAsync();

        return categories;
    }

    public async Task<SuggestionCategoryDto?> GetCategoryAsync(long id)
    {
        var category = await _context.SuggestionCategories
            .Where(c => c.Id == id)
            .Select(c => new SuggestionCategoryDto
            {
                Id = c.Id,
                CategoryKey = c.CategoryKey,
                Title = c.Title,
                Icon = c.Icon,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                SuggestionCount = c.Suggestions.Count(s => s.IsActive)
            })
            .FirstOrDefaultAsync();

        return category;
    }

    public async Task<SuggestionCategoryDto?> GetCategoryByKeyAsync(string categoryKey)
    {
        var category = await _context.SuggestionCategories
            .Where(c => c.CategoryKey == categoryKey)
            .Select(c => new SuggestionCategoryDto
            {
                Id = c.Id,
                CategoryKey = c.CategoryKey,
                Title = c.Title,
                Icon = c.Icon,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                SuggestionCount = c.Suggestions.Count(s => s.IsActive)
            })
            .FirstOrDefaultAsync();

        return category;
    }

    public async Task<SuggestionCategoryDto> CreateCategoryAsync(CreateUpdateSuggestionCategoryDto dto, string userId)
    {
        var category = new SuggestionCategory
        {
            CategoryKey = dto.CategoryKey,
            Title = dto.Title,
            Icon = dto.Icon,
            Description = dto.Description,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        _context.SuggestionCategories.Add(category);
        await _context.SaveChangesAsync();

        return new SuggestionCategoryDto
        {
            Id = category.Id,
            CategoryKey = category.CategoryKey,
            Title = category.Title,
            Icon = category.Icon,
            Description = category.Description,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            SuggestionCount = 0
        };
    }

    public async Task<SuggestionCategoryDto?> UpdateCategoryAsync(long id, CreateUpdateSuggestionCategoryDto dto, string userId)
    {
        var category = await _context.SuggestionCategories.FindAsync(id);
        if (category == null) return null;

        category.CategoryKey = dto.CategoryKey;
        category.Title = dto.Title;
        category.Icon = dto.Icon;
        category.Description = dto.Description;
        category.SortOrder = dto.SortOrder;
        category.IsActive = dto.IsActive;
        category.UpdatedBy = userId;
        category.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new SuggestionCategoryDto
        {
            Id = category.Id,
            CategoryKey = category.CategoryKey,
            Title = category.Title,
            Icon = category.Icon,
            Description = category.Description,
            SortOrder = category.SortOrder,
            IsActive = category.IsActive,
            SuggestionCount = category.Suggestions.Count(s => s.IsActive)
        };
    }

    public async Task<bool> DeleteCategoryAsync(long id, string userId)
    {
        var category = await _context.SuggestionCategories.FindAsync(id);
        if (category == null) return false;

        _context.SuggestionCategories.Remove(category);
        await _context.SaveChangesAsync();
        return true;
    }

    // Suggestion Management
    public async Task<List<QuerySuggestionDto>> GetSuggestionsAsync(bool includeInactive = false)
    {
        var query = _context.QuerySuggestions
            .Include(s => s.Category)
            .AsQueryable();

        if (!includeInactive)
            query = query.Where(s => s.IsActive);

        var suggestions = await query
            .OrderBy(s => s.Category.SortOrder)
            .ThenBy(s => s.SortOrder)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return suggestions;
    }

    public async Task<List<QuerySuggestionDto>> GetSuggestionsByCategoryAsync(string categoryKey, bool includeInactive = false)
    {
        var query = _context.QuerySuggestions
            .Include(s => s.Category)
            .Where(s => s.Category.CategoryKey == categoryKey);

        if (!includeInactive)
            query = query.Where(s => s.IsActive);

        var suggestions = await query
            .OrderBy(s => s.SortOrder)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return suggestions;
    }

    public async Task<List<GroupedSuggestionsDto>> GetGroupedSuggestionsAsync(bool includeInactive = false)
    {
        var categoriesQuery = _context.SuggestionCategories.AsQueryable();
        if (!includeInactive)
            categoriesQuery = categoriesQuery.Where(c => c.IsActive);

        var categories = await categoriesQuery
            .Include(c => c.Suggestions.Where(s => includeInactive || s.IsActive))
            .OrderBy(c => c.SortOrder)
            .ToListAsync();

        var grouped = categories.Select(c => new GroupedSuggestionsDto
        {
            Category = new SuggestionCategoryDto
            {
                Id = c.Id,
                CategoryKey = c.CategoryKey,
                Title = c.Title,
                Icon = c.Icon,
                Description = c.Description,
                SortOrder = c.SortOrder,
                IsActive = c.IsActive,
                SuggestionCount = c.Suggestions.Count
            },
            Suggestions = c.Suggestions
                .OrderBy(s => s.SortOrder)
                .Select(s => MapToDto(s))
                .ToList()
        }).ToList();

        return grouped;
    }

    private static QuerySuggestionDto MapToDto(QuerySuggestion suggestion)
    {
        return new QuerySuggestionDto
        {
            Id = suggestion.Id,
            CategoryId = suggestion.CategoryId,
            CategoryKey = suggestion.Category?.CategoryKey ?? "",
            CategoryTitle = suggestion.Category?.Title ?? "",
            QueryText = suggestion.QueryText,
            Description = suggestion.Description,
            DefaultTimeFrame = suggestion.DefaultTimeFrame,
            SortOrder = suggestion.SortOrder,
            IsActive = suggestion.IsActive,
            TargetTables = ParseJsonArray(suggestion.TargetTables),
            Complexity = suggestion.Complexity,
            RequiredPermissions = ParseJsonArray(suggestion.RequiredPermissions),
            Tags = ParseJsonArray(suggestion.Tags),
            UsageCount = suggestion.UsageCount,
            LastUsed = suggestion.LastUsed,
            CreatedDate = suggestion.CreatedDate,
            CreatedBy = suggestion.CreatedBy
        };
    }

    private static List<string> ParseJsonArray(string? json)
    {
        if (string.IsNullOrEmpty(json)) return new List<string>();
        
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
        }
        catch
        {
            return new List<string>();
        }
    }

    // Time Frame Management
    public async Task<List<TimeFrameDefinitionDto>> GetTimeFramesAsync(bool includeInactive = false)
    {
        var query = _context.TimeFrameDefinitions.AsQueryable();
        
        if (!includeInactive)
            query = query.Where(t => t.IsActive);

        var timeFrames = await query
            .OrderBy(t => t.SortOrder)
            .Select(t => new TimeFrameDefinitionDto
            {
                Id = t.Id,
                TimeFrameKey = t.TimeFrameKey,
                DisplayName = t.DisplayName,
                Description = t.Description,
                SqlExpression = t.SqlExpression,
                SortOrder = t.SortOrder,
                IsActive = t.IsActive
            })
            .ToListAsync();

        return timeFrames;
    }

    public async Task<TimeFrameDefinitionDto?> GetTimeFrameAsync(string timeFrameKey)
    {
        var timeFrame = await _context.TimeFrameDefinitions
            .Where(t => t.TimeFrameKey == timeFrameKey)
            .Select(t => new TimeFrameDefinitionDto
            {
                Id = t.Id,
                TimeFrameKey = t.TimeFrameKey,
                DisplayName = t.DisplayName,
                Description = t.Description,
                SqlExpression = t.SqlExpression,
                SortOrder = t.SortOrder,
                IsActive = t.IsActive
            })
            .FirstOrDefaultAsync();

        return timeFrame;
    }

    // Usage Analytics
    public async Task RecordUsageAsync(RecordSuggestionUsageDto dto, string userId)
    {
        var usage = new SuggestionUsageAnalytics
        {
            SuggestionId = dto.SuggestionId,
            UserId = userId,
            SessionId = dto.SessionId,
            UsedAt = DateTime.UtcNow,
            TimeFrameUsed = dto.TimeFrameUsed,
            ResultCount = dto.ResultCount,
            ExecutionTimeMs = dto.ExecutionTimeMs,
            WasSuccessful = dto.WasSuccessful,
            UserFeedback = dto.UserFeedback
        };

        _context.SuggestionUsageAnalytics.Add(usage);

        // Update suggestion usage count and last used
        var suggestion = await _context.QuerySuggestions.FindAsync(dto.SuggestionId);
        if (suggestion != null)
        {
            suggestion.UsageCount++;
            suggestion.LastUsed = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
    }

    // Individual Suggestion Management
    public async Task<QuerySuggestionDto?> GetSuggestionAsync(long id)
    {
        var suggestion = await _context.QuerySuggestions
            .Include(s => s.Category)
            .Where(s => s.Id == id)
            .FirstOrDefaultAsync();

        return suggestion != null ? MapToDto(suggestion) : null;
    }

    public async Task<QuerySuggestionDto> CreateSuggestionAsync(CreateUpdateQuerySuggestionDto dto, string userId)
    {
        var suggestion = new QuerySuggestion
        {
            CategoryId = dto.CategoryId,
            QueryText = dto.QueryText,
            Description = dto.Description,
            DefaultTimeFrame = dto.DefaultTimeFrame,
            SortOrder = dto.SortOrder,
            IsActive = dto.IsActive,
            TargetTables = JsonSerializer.Serialize(dto.TargetTables),
            Complexity = dto.Complexity,
            RequiredPermissions = JsonSerializer.Serialize(dto.RequiredPermissions),
            Tags = JsonSerializer.Serialize(dto.Tags),
            UsageCount = 0,
            CreatedBy = userId,
            CreatedDate = DateTime.UtcNow
        };

        _context.QuerySuggestions.Add(suggestion);
        await _context.SaveChangesAsync();

        // Load the category for the response
        await _context.Entry(suggestion)
            .Reference(s => s.Category)
            .LoadAsync();

        return MapToDto(suggestion);
    }

    public async Task<QuerySuggestionDto?> UpdateSuggestionAsync(long id, CreateUpdateQuerySuggestionDto dto, string userId)
    {
        var suggestion = await _context.QuerySuggestions
            .Include(s => s.Category)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (suggestion == null) return null;

        suggestion.CategoryId = dto.CategoryId;
        suggestion.QueryText = dto.QueryText;
        suggestion.Description = dto.Description;
        suggestion.DefaultTimeFrame = dto.DefaultTimeFrame;
        suggestion.SortOrder = dto.SortOrder;
        suggestion.IsActive = dto.IsActive;
        suggestion.TargetTables = JsonSerializer.Serialize(dto.TargetTables);
        suggestion.Complexity = dto.Complexity;
        suggestion.RequiredPermissions = JsonSerializer.Serialize(dto.RequiredPermissions);
        suggestion.Tags = JsonSerializer.Serialize(dto.Tags);
        suggestion.UpdatedBy = userId;
        suggestion.UpdatedDate = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToDto(suggestion);
    }

    public async Task<bool> DeleteSuggestionAsync(long id, string userId)
    {
        var suggestion = await _context.QuerySuggestions.FindAsync(id);
        if (suggestion == null) return false;

        _context.QuerySuggestions.Remove(suggestion);
        await _context.SaveChangesAsync();
        return true;
    }

    // Search and Filtering
    public async Task<SuggestionSearchResultDto> SearchSuggestionsAsync(SuggestionSearchDto searchDto)
    {
        var query = _context.QuerySuggestions
            .Include(s => s.Category)
            .AsQueryable();

        // Apply filters
        if (searchDto.IsActive.HasValue)
            query = query.Where(s => s.IsActive == searchDto.IsActive.Value);

        if (!string.IsNullOrEmpty(searchDto.SearchTerm))
        {
            var searchTerm = searchDto.SearchTerm.ToLower();
            query = query.Where(s =>
                s.QueryText.ToLower().Contains(searchTerm) ||
                s.Description.ToLower().Contains(searchTerm) ||
                s.Category.Title.ToLower().Contains(searchTerm));
        }

        if (!string.IsNullOrEmpty(searchDto.CategoryKey))
            query = query.Where(s => s.Category.CategoryKey == searchDto.CategoryKey);

        if (searchDto.MinComplexity.HasValue)
            query = query.Where(s => s.Complexity >= searchDto.MinComplexity.Value);

        if (searchDto.MaxComplexity.HasValue)
            query = query.Where(s => s.Complexity <= searchDto.MaxComplexity.Value);

        if (searchDto.Tags.Any())
        {
            foreach (var tag in searchDto.Tags)
            {
                query = query.Where(s => s.Tags.Contains(tag));
            }
        }

        // Get total count before pagination
        var totalCount = await query.CountAsync();

        // Apply sorting
        query = searchDto.SortBy.ToLower() switch
        {
            "querytext" => searchDto.SortDescending ? query.OrderByDescending(s => s.QueryText) : query.OrderBy(s => s.QueryText),
            "description" => searchDto.SortDescending ? query.OrderByDescending(s => s.Description) : query.OrderBy(s => s.Description),
            "complexity" => searchDto.SortDescending ? query.OrderByDescending(s => s.Complexity) : query.OrderBy(s => s.Complexity),
            "usagecount" => searchDto.SortDescending ? query.OrderByDescending(s => s.UsageCount) : query.OrderBy(s => s.UsageCount),
            "lastused" => searchDto.SortDescending ? query.OrderByDescending(s => s.LastUsed) : query.OrderBy(s => s.LastUsed),
            "createddate" => searchDto.SortDescending ? query.OrderByDescending(s => s.CreatedDate) : query.OrderBy(s => s.CreatedDate),
            _ => searchDto.SortDescending ? query.OrderByDescending(s => s.SortOrder) : query.OrderBy(s => s.SortOrder)
        };

        // Apply pagination
        var suggestions = await query
            .Skip(searchDto.Skip)
            .Take(searchDto.Take)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return new SuggestionSearchResultDto
        {
            Suggestions = suggestions,
            TotalCount = totalCount,
            Skip = searchDto.Skip,
            Take = searchDto.Take
        };
    }

    // Popular and Recent Suggestions
    public async Task<List<QuerySuggestionDto>> GetPopularSuggestionsAsync(int count = 10)
    {
        var suggestions = await _context.QuerySuggestions
            .Include(s => s.Category)
            .Where(s => s.IsActive)
            .OrderByDescending(s => s.UsageCount)
            .ThenByDescending(s => s.LastUsed)
            .Take(count)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return suggestions;
    }

    public async Task<List<QuerySuggestionDto>> GetRecentSuggestionsAsync(int count = 10)
    {
        var suggestions = await _context.QuerySuggestions
            .Include(s => s.Category)
            .Where(s => s.IsActive && s.LastUsed.HasValue)
            .OrderByDescending(s => s.LastUsed)
            .Take(count)
            .Select(s => MapToDto(s))
            .ToListAsync();

        return suggestions;
    }

    // Analytics Methods - Basic implementations
    public Task<List<SuggestionUsageAnalyticsDto>> GetUsageAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null)
    {
        _logger.LogInformation("Getting usage analytics from {FromDate} to {ToDate}", fromDate, toDate);
        // Return empty list for now - can be implemented when analytics requirements are defined
        return Task.FromResult(new List<SuggestionUsageAnalyticsDto>());
    }

    public Task<SuggestionUsageAnalyticsDto?> GetSuggestionAnalyticsAsync(long suggestionId, DateTime? fromDate = null, DateTime? toDate = null)
    {
        _logger.LogInformation("Getting analytics for suggestion {SuggestionId} from {FromDate} to {ToDate}", suggestionId, fromDate, toDate);
        // Return null for now - can be implemented when analytics requirements are defined
        return Task.FromResult<SuggestionUsageAnalyticsDto?>(null);
    }

    // Bulk Operations - Basic implementations
    public Task<int> BulkUpdateSortOrderAsync(Dictionary<long, int> suggestionSortOrders, string userId)
    {
        _logger.LogInformation("Bulk updating sort order for {Count} suggestions by user {UserId}", suggestionSortOrders.Count, userId);
        // Return 0 for now - can be implemented when bulk operations are needed
        return Task.FromResult(0);
    }

    public Task<int> BulkToggleActiveStatusAsync(List<long> suggestionIds, bool isActive, string userId)
    {
        _logger.LogInformation("Bulk toggling active status for {Count} suggestions to {IsActive} by user {UserId}", suggestionIds.Count, isActive, userId);
        // Return 0 for now - can be implemented when bulk operations are needed
        return Task.FromResult(0);
    }

    // Import/Export - Basic implementations
    public Task<List<QuerySuggestionDto>> ImportSuggestionsAsync(List<CreateUpdateQuerySuggestionDto> suggestions, string userId)
    {
        _logger.LogInformation("Importing {Count} suggestions by user {UserId}", suggestions.Count, userId);
        // Return empty list for now - can be implemented when import/export is needed
        return Task.FromResult(new List<QuerySuggestionDto>());
    }

    public Task<byte[]> ExportSuggestionsAsync(string? categoryKey = null)
    {
        _logger.LogInformation("Exporting suggestions for category {CategoryKey}", categoryKey);
        // Return empty byte array for now - can be implemented when export is needed
        return Task.FromResult(Array.Empty<byte>());
    }
}
