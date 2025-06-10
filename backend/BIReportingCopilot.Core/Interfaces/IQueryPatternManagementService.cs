using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for managing query patterns and templates
/// </summary>
public interface IQueryPatternManagementService
{
    // Query Pattern Operations
    Task<List<QueryPatternDto>> GetQueryPatternsAsync();
    Task<QueryPatternDto?> GetQueryPatternAsync(long id);
    Task<QueryPatternDto> CreateQueryPatternAsync(CreateQueryPatternRequest request, string userId);
    Task<QueryPatternDto?> UpdateQueryPatternAsync(long id, CreateQueryPatternRequest request, string userId);
    Task<bool> DeleteQueryPatternAsync(long id);
    Task<string> TestQueryPatternAsync(long id, string naturalLanguageQuery);

    // Statistics and Usage
    Task<QueryPatternStatistics> GetPatternStatisticsAsync();
    Task IncrementPatternUsageAsync(long patternId);
}

/// <summary>
/// Statistics for query patterns
/// </summary>
public class QueryPatternStatistics
{
    public int TotalPatterns { get; set; }
    public List<string> MostUsedPatterns { get; set; } = new();
    public Dictionary<string, int> PatternUsageStats { get; set; } = new();
    public double AverageUsageCount { get; set; }
}
