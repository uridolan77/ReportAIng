using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Query;

namespace BIReportingCopilot.Infrastructure.Business;

/// <summary>
/// Business interface wrapper for QueryPatternManagementService
/// </summary>
public class BusinessQueryPatternManagementService : IQueryPatternManagementService
{
    private readonly QueryPatternManagementService _queryPatternService;

    public BusinessQueryPatternManagementService(QueryPatternManagementService queryPatternService)
    {
        _queryPatternService = queryPatternService;
    }

    public async Task<List<QueryPatternDto>> GetQueryPatternsAsync(CancellationToken cancellationToken = default)
    {
        return await _queryPatternService.GetQueryPatternsAsync();
    }

    public async Task<QueryPatternDto?> GetQueryPatternAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _queryPatternService.GetQueryPatternAsync(id);
    }

    public async Task<QueryPatternDto> CreateQueryPatternAsync(CreateQueryPatternRequest request, string userId, CancellationToken cancellationToken = default)
    {
        return await _queryPatternService.CreateQueryPatternAsync(request, userId);
    }

    public async Task<QueryPatternDto?> UpdateQueryPatternAsync(long id, CreateQueryPatternRequest request, string userId, CancellationToken cancellationToken = default)
    {
        return await _queryPatternService.UpdateQueryPatternAsync(id, request, userId);
    }

    public async Task<bool> DeleteQueryPatternAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _queryPatternService.DeleteQueryPatternAsync(id);
    }

    public async Task<string> TestQueryPatternAsync(long id, string naturalLanguageQuery, CancellationToken cancellationToken = default)
    {
        return await _queryPatternService.TestQueryPatternAsync(id, naturalLanguageQuery);
    }    public async Task<QueryPatternStatistics> GetPatternStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var statsResult = await _queryPatternService.GetPatternStatisticsAsync();
        
        // Convert from Statistics.QueryPatternStatistics to Models.QueryPatternStatistics
        return new QueryPatternStatistics
        {
            TotalPatterns = statsResult.TotalPatterns,
            ActivePatterns = statsResult.ActivePatterns,
            AverageConfidence = statsResult.AverageMatchAccuracy,
            LastUpdated = statsResult.LastUpdated,
            PatternsByType = statsResult.PatternsByComplexity
        };
    }
}
