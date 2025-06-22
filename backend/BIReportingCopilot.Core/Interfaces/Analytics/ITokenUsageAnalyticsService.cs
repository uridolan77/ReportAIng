namespace BIReportingCopilot.Core.Interfaces.Analytics;

/// <summary>
/// Service for tracking and analyzing token usage patterns
/// </summary>
public interface ITokenUsageAnalyticsService
{
    /// <summary>
    /// Record token usage for a specific request
    /// </summary>
    Task RecordTokenUsageAsync(string userId, string requestType, string intentType,
        int tokensUsed, decimal cost, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get daily token usage analytics for a user
    /// </summary>
    Task<IEnumerable<TokenUsageRecord>> GetDailyUsageAsync(string userId,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get token usage analytics by request type
    /// </summary>
    Task<IEnumerable<TokenUsageRecord>> GetUsageByRequestTypeAsync(string requestType,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get token usage analytics by intent type
    /// </summary>
    Task<IEnumerable<TokenUsageRecord>> GetUsageByIntentTypeAsync(string intentType,
        DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get aggregated usage statistics for a date range
    /// </summary>
    Task<TokenUsageStatistics> GetUsageStatisticsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top users by token usage
    /// </summary>
    Task<IEnumerable<UserTokenUsageSummary>> GetTopUsersByUsageAsync(DateTime startDate, DateTime endDate, 
        int topCount = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get usage trends over time
    /// </summary>
    Task<IEnumerable<TokenUsageTrend>> GetUsageTrendsAsync(DateTime startDate, DateTime endDate, 
        string? userId = null, string? requestType = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Aggregate daily usage data (typically run as a background job)
    /// </summary>
    Task AggregateUsageDataAsync(DateTime date, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update business context for token usage analytics
    /// </summary>
    Task UpdateBusinessContextAsync(string id, string? naturalLanguageDescription = null, 
        string? businessRules = null, string? relationshipContext = null, 
        string? dataGovernanceLevel = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Token usage statistics summary
/// </summary>
public class TokenUsageStatistics
{
    public int TotalRequests { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageTokensPerRequest { get; set; }
    public decimal AverageCostPerRequest { get; set; }
    public Dictionary<string, int> RequestTypeBreakdown { get; set; } = new();
    public Dictionary<string, int> IntentTypeBreakdown { get; set; } = new();
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

/// <summary>
/// User token usage summary
/// </summary>
public class UserTokenUsageSummary
{
    public string UserId { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageTokensPerRequest { get; set; }
    public string MostUsedRequestType { get; set; } = string.Empty;
    public string MostUsedIntentType { get; set; } = string.Empty;
}

/// <summary>
/// Token usage record
/// </summary>
public class TokenUsageRecord
{
    public string Id { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string RequestType { get; set; } = string.Empty;
    public string IntentType { get; set; } = string.Empty;
    public int TotalRequests { get; set; }
    public int TotalTokensUsed { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageTokensPerRequest { get; set; }
    public decimal AverageCostPerRequest { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

/// <summary>
/// Token usage trend data point
/// </summary>
public class TokenUsageTrend
{
    public DateTime Date { get; set; }
    public int TotalRequests { get; set; }
    public int TotalTokens { get; set; }
    public decimal TotalCost { get; set; }
    public decimal AverageTokensPerRequest { get; set; }
    public string? RequestType { get; set; }
    public string? IntentType { get; set; }
}
