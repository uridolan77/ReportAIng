using BIReportingCopilot.Core.DTOs.QuerySuggestions;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Service interface for managing query suggestions
/// </summary>
public interface IQuerySuggestionService
{
    // Category Management
    Task<List<SuggestionCategoryDto>> GetCategoriesAsync(bool includeInactive = false);
    Task<SuggestionCategoryDto?> GetCategoryAsync(long id);
    Task<SuggestionCategoryDto?> GetCategoryByKeyAsync(string categoryKey);
    Task<SuggestionCategoryDto> CreateCategoryAsync(CreateUpdateSuggestionCategoryDto dto, string userId);
    Task<SuggestionCategoryDto?> UpdateCategoryAsync(long id, CreateUpdateSuggestionCategoryDto dto, string userId);
    Task<bool> DeleteCategoryAsync(long id, string userId);

    // Suggestion Management
    Task<List<QuerySuggestionDto>> GetSuggestionsAsync(bool includeInactive = false);
    Task<List<QuerySuggestionDto>> GetSuggestionsByCategoryAsync(string categoryKey, bool includeInactive = false);
    Task<List<GroupedSuggestionsDto>> GetGroupedSuggestionsAsync(bool includeInactive = false);
    Task<QuerySuggestionDto?> GetSuggestionAsync(long id);
    Task<QuerySuggestionDto> CreateSuggestionAsync(CreateUpdateQuerySuggestionDto dto, string userId);
    Task<QuerySuggestionDto?> UpdateSuggestionAsync(long id, CreateUpdateQuerySuggestionDto dto, string userId);
    Task<bool> DeleteSuggestionAsync(long id, string userId);

    // Search and Filtering
    Task<SuggestionSearchResultDto> SearchSuggestionsAsync(SuggestionSearchDto searchDto);
    Task<List<QuerySuggestionDto>> GetPopularSuggestionsAsync(int count = 10);
    Task<List<QuerySuggestionDto>> GetRecentSuggestionsAsync(int count = 10);

    // Time Frame Management
    Task<List<TimeFrameDefinitionDto>> GetTimeFramesAsync(bool includeInactive = false);
    Task<TimeFrameDefinitionDto?> GetTimeFrameAsync(string timeFrameKey);

    // Usage Analytics
    Task RecordUsageAsync(RecordSuggestionUsageDto dto, string userId);
    Task<List<SuggestionUsageAnalyticsDto>> GetUsageAnalyticsAsync(DateTime? fromDate = null, DateTime? toDate = null);
    Task<SuggestionUsageAnalyticsDto?> GetSuggestionAnalyticsAsync(long suggestionId, DateTime? fromDate = null, DateTime? toDate = null);

    // Bulk Operations
    Task<int> BulkUpdateSortOrderAsync(Dictionary<long, int> suggestionSortOrders, string userId);
    Task<int> BulkToggleActiveStatusAsync(List<long> suggestionIds, bool isActive, string userId);

    // Import/Export
    Task<List<QuerySuggestionDto>> ImportSuggestionsAsync(List<CreateUpdateQuerySuggestionDto> suggestions, string userId);
    Task<byte[]> ExportSuggestionsAsync(string? categoryKey = null);
}
