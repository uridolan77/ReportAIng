namespace BIReportingCopilot.Infrastructure.Security;

/// <summary>
/// Interface for rate limiting services
/// </summary>
public interface IRateLimitingService
{
    /// <summary>
    /// Check rate limit for user and endpoint
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <param name="endpoint">Endpoint being accessed</param>
    /// <returns>Rate limit result</returns>
    Task<RateLimitResult> CheckRateLimitAsync(string userId, string endpoint);

    /// <summary>
    /// Check rate limit with custom policy
    /// </summary>
    /// <param name="identifier">Client identifier</param>
    /// <param name="policy">Rate limit policy to apply</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rate limit result</returns>
    Task<RateLimitResult> CheckRateLimitAsync(string identifier, RateLimitPolicy policy, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check multiple rate limits
    /// </summary>
    /// <param name="identifier">Client identifier</param>
    /// <param name="policies">Rate limit policies to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of policy names to rate limit results</returns>
    Task<Dictionary<string, RateLimitResult>> CheckMultipleRateLimitsAsync(
        string identifier,
        IEnumerable<RateLimitPolicy> policies,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Reset rate limit for identifier and policy
    /// </summary>
    /// <param name="identifier">Client identifier</param>
    /// <param name="policyName">Policy name to reset</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Task</returns>
    Task ResetRateLimitAsync(string identifier, string policyName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get rate limit statistics for identifier
    /// </summary>
    /// <param name="identifier">Client identifier</param>
    /// <param name="policyName">Policy name</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Rate limit statistics</returns>
    Task<RateLimitStatistics?> GetRateLimitStatisticsAsync(string identifier, string policyName, CancellationToken cancellationToken = default);
}


