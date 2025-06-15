using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models.QuerySuggestions;

namespace BIReportingCopilot.Core.Interfaces.Query;

/// <summary>
/// Interface for query suggestion services
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

    // Time Frames
    Task<List<TimeFrameDefinitionDto>> GetTimeFramesAsync(bool includeInactive = false);
    Task<TimeFrameDefinitionDto?> GetTimeFrameAsync(string timeFrameKey);    // Usage and Analytics
    Task RecordUsageAsync(RecordSuggestionUsageDto dto, string userId);
    Task<SuggestionSearchResultDto> SearchSuggestionsAsync(SuggestionSearchDto searchDto);
    Task<List<QuerySuggestionDto>> GetPopularSuggestionsAsync(int count = 10);
    Task<List<QuerySuggestionDto>> GetRecentSuggestionsAsync(int count = 10);
    Task<object> GetUsageAnalyticsAsync(CancellationToken cancellationToken = default);
    
    // Legacy interface support
    Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string query, string? userId = null, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GetSuggestionsAsync(string partialQuery, string? context = null, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GetPopularQueriesAsync(int count = 10, CancellationToken cancellationToken = default);
    Task<List<QuerySuggestion>> GetRecentQueriesAsync(string userId, int count = 10, CancellationToken cancellationToken = default);
    Task SaveQuerySuggestionAsync(QuerySuggestion suggestion, CancellationToken cancellationToken = default);
    Task<bool> UpdateSuggestionRatingAsync(string suggestionId, int rating, CancellationToken cancellationToken = default);
}
