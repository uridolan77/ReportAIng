namespace BIReportingCopilot.Core.Exceptions;

/// <summary>
/// Exception thrown when circuit breaker is open
/// </summary>
public class CircuitBreakerOpenException : Exception
{
    public string ServiceName { get; }
    public TimeSpan RetryAfter { get; }
    public int FailureCount { get; }

    public CircuitBreakerOpenException(
        string serviceName,
        TimeSpan retryAfter,
        int failureCount = 0)
        : base($"Circuit breaker is open for service '{serviceName}'. Retry after: {retryAfter}")
    {
        ServiceName = serviceName;
        RetryAfter = retryAfter;
        FailureCount = failureCount;
    }

    public CircuitBreakerOpenException(
        string serviceName,
        string message,
        TimeSpan retryAfter,
        int failureCount = 0)
        : base(message)
    {
        ServiceName = serviceName;
        RetryAfter = retryAfter;
        FailureCount = failureCount;
    }

    public CircuitBreakerOpenException(
        string serviceName,
        string message,
        Exception innerException,
        TimeSpan retryAfter,
        int failureCount = 0)
        : base(message, innerException)
    {
        ServiceName = serviceName;
        RetryAfter = retryAfter;
        FailureCount = failureCount;
    }
}
