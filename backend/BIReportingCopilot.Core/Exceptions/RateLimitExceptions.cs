namespace BIReportingCopilot.Core.Exceptions;

/// <summary>
/// Exception thrown when rate limit is exceeded
/// </summary>
public class RateLimitExceededException : Exception
{
    public string LimitType { get; }
    public int CurrentCount { get; }
    public int MaxAllowed { get; }
    public TimeSpan RetryAfter { get; }
    public string? UserId { get; }

    public RateLimitExceededException(
        string limitType,
        int currentCount,
        int maxAllowed,
        TimeSpan retryAfter,
        string? userId = null)
        : base($"Rate limit exceeded for {limitType}. Current: {currentCount}, Max: {maxAllowed}. Retry after: {retryAfter}")
    {
        LimitType = limitType;
        CurrentCount = currentCount;
        MaxAllowed = maxAllowed;
        RetryAfter = retryAfter;
        UserId = userId;
    }

    public RateLimitExceededException(
        string limitType,
        int currentCount,
        int maxAllowed,
        TimeSpan retryAfter,
        string? userId,
        Exception innerException)
        : base($"Rate limit exceeded for {limitType}. Current: {currentCount}, Max: {maxAllowed}. Retry after: {retryAfter}", innerException)
    {
        LimitType = limitType;
        CurrentCount = currentCount;
        MaxAllowed = maxAllowed;
        RetryAfter = retryAfter;
        UserId = userId;
    }
}

/// <summary>
/// Exception thrown when rate limiting service is unavailable
/// </summary>
public class RateLimitServiceUnavailableException : Exception
{
    public string ServiceName { get; }

    public RateLimitServiceUnavailableException(string serviceName)
        : base($"Rate limiting service '{serviceName}' is unavailable")
    {
        ServiceName = serviceName;
    }

    public RateLimitServiceUnavailableException(string serviceName, Exception innerException)
        : base($"Rate limiting service '{serviceName}' is unavailable", innerException)
    {
        ServiceName = serviceName;
    }
}

/// <summary>
/// Exception thrown when rate limit configuration is invalid
/// </summary>
public class InvalidRateLimitConfigurationException : Exception
{
    public string ConfigurationKey { get; }

    public InvalidRateLimitConfigurationException(string configurationKey, string message)
        : base($"Invalid rate limit configuration for '{configurationKey}': {message}")
    {
        ConfigurationKey = configurationKey;
    }

    public InvalidRateLimitConfigurationException(string configurationKey, string message, Exception innerException)
        : base($"Invalid rate limit configuration for '{configurationKey}': {message}", innerException)
    {
        ConfigurationKey = configurationKey;
    }
}
