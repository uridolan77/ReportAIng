using MediatR;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Commands;

public class GetQueryHistoryQuery : IRequest<PagedResult<QueryHistoryItem>>
{
    public string UserId { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
}

public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}

/// <summary>
/// Query for getting cached query result
/// </summary>
public class GetCachedQueryQuery : IRequest<QueryResponse?>
{
    public string QueryHash { get; set; } = string.Empty;
}

/// <summary>
/// Query for getting query suggestions for a user
/// </summary>
public class GetQuerySuggestionsQuery : IRequest<List<string>>
{
    public string UserId { get; set; } = string.Empty;
    public string? Context { get; set; }
}

/// <summary>
/// Query for getting semantic cache statistics
/// </summary>
public class GetCacheStatisticsQuery : IRequest<CacheStatistics>
{
    public string? UserId { get; set; }
}

/// <summary>
/// Query for checking if a query exists in cache
/// </summary>
public class CheckQueryCacheQuery : IRequest<bool>
{
    public string QueryHash { get; set; } = string.Empty;
}

/// <summary>
/// Query for getting user's recent query patterns
/// </summary>
public class GetUserQueryPatternsQuery : IRequest<List<QueryPattern>>
{
    public string UserId { get; set; } = string.Empty;
    public int Limit { get; set; } = 10;
}

/// <summary>
/// Query for getting relevant schema for a question
/// </summary>
public class GetRelevantSchemaQuery : IRequest<SchemaMetadata>
{
    public string Question { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

/// <summary>
/// Query for validating SQL syntax
/// </summary>
public class ValidateSqlQuery : IRequest<bool>
{
    public string Sql { get; set; } = string.Empty;
}

/// <summary>
/// Query for getting query execution metrics
/// </summary>
public class GetQueryMetricsQuery : IRequest<QueryMetrics>
{
    public string? UserId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

// QueryMetrics moved to MLModels.cs to consolidate duplications
