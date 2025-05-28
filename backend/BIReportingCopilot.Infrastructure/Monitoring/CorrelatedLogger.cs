using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Monitoring;

/// <summary>
/// Logger wrapper that adds correlation context to all log entries
/// </summary>
public class CorrelatedLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _innerLogger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CorrelatedLogger(ILogger<T> innerLogger, IHttpContextAccessor httpContextAccessor)
    {
        _innerLogger = innerLogger;
        _httpContextAccessor = httpContextAccessor;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return _innerLogger.BeginScope(state);
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return _innerLogger.IsEnabled(logLevel);
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel))
            return;

        using var scope = _innerLogger.BeginScope(GetCorrelationContext());
        _innerLogger.Log(logLevel, eventId, state, exception, formatter);
    }

    private Dictionary<string, object> GetCorrelationContext()
    {
        var context = new Dictionary<string, object>();

        // Add correlation ID
        var correlationId = GetCorrelationId();
        if (!string.IsNullOrEmpty(correlationId))
        {
            context["CorrelationId"] = correlationId;
        }

        // Add user ID
        var userId = GetUserId();
        if (!string.IsNullOrEmpty(userId))
        {
            context["UserId"] = userId;
        }

        // Add request path
        var requestPath = GetRequestPath();
        if (!string.IsNullOrEmpty(requestPath))
        {
            context["RequestPath"] = requestPath;
        }

        // Add trace ID
        var traceId = Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrEmpty(traceId))
        {
            context["TraceId"] = traceId;
        }

        // Add span ID
        var spanId = Activity.Current?.SpanId.ToString();
        if (!string.IsNullOrEmpty(spanId))
        {
            context["SpanId"] = spanId;
        }

        // Add timestamp
        context["Timestamp"] = DateTimeOffset.UtcNow;

        // Add machine name
        context["MachineName"] = Environment.MachineName;

        return context;
    }

    private string? GetCorrelationId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null) return null;

        // Try to get from headers first
        if (httpContext.Request.Headers.TryGetValue("X-Correlation-ID", out var headerValue))
        {
            return headerValue.FirstOrDefault();
        }

        // Try to get from items
        if (httpContext.Items.TryGetValue("CorrelationId", out var itemValue))
        {
            return itemValue?.ToString();
        }

        return null;
    }

    private string? GetUserId()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext?.User?.Identity?.IsAuthenticated == true)
        {
            return httpContext.User.FindFirst("sub")?.Value ??
                   httpContext.User.FindFirst("userId")?.Value ??
                   httpContext.User.Identity.Name;
        }

        return null;
    }

    private string? GetRequestPath()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return httpContext?.Request?.Path.Value;
    }
}

/// <summary>
/// Extension methods for correlated logging
/// </summary>
public static class CorrelatedLoggerExtensions
{
    public static void LogInformationWithContext<T>(this ILogger<T> logger, string message, params object[] args)
    {
        logger.LogInformation(message, args);
    }

    public static void LogWarningWithContext<T>(this ILogger<T> logger, string message, params object[] args)
    {
        logger.LogWarning(message, args);
    }

    public static void LogErrorWithContext<T>(this ILogger<T> logger, Exception exception, string message, params object[] args)
    {
        logger.LogError(exception, message, args);
    }

    public static void LogDebugWithContext<T>(this ILogger<T> logger, string message, params object[] args)
    {
        logger.LogDebug(message, args);
    }

    public static void LogCriticalWithContext<T>(this ILogger<T> logger, Exception exception, string message, params object[] args)
    {
        logger.LogCritical(exception, message, args);
    }

    public static void LogQueryExecution<T>(this ILogger<T> logger, string query, long durationMs, bool success, int rowCount = 0)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["QueryType"] = "SQL",
            ["Duration"] = durationMs,
            ["Success"] = success,
            ["RowCount"] = rowCount
        });

        if (success)
        {
            logger.LogInformation("Query executed successfully in {Duration}ms, returned {RowCount} rows", durationMs, rowCount);
        }
        else
        {
            logger.LogWarning("Query execution failed after {Duration}ms", durationMs);
        }
    }

    public static void LogAIOperation<T>(this ILogger<T> logger, string operation, string prompt, long durationMs, bool success, double? confidence = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationType"] = "AI",
            ["AIOperation"] = operation,
            ["Duration"] = durationMs,
            ["Success"] = success,
            ["Confidence"] = confidence ?? 0.0
        });

        if (success)
        {
            logger.LogInformation("AI operation '{Operation}' completed successfully in {Duration}ms with confidence {Confidence:P1}", 
                operation, durationMs, confidence ?? 0.0);
        }
        else
        {
            logger.LogWarning("AI operation '{Operation}' failed after {Duration}ms", operation, durationMs);
        }
    }

    public static void LogCacheOperation<T>(this ILogger<T> logger, string operation, string key, bool hit, long? durationMs = null)
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["OperationType"] = "Cache",
            ["CacheOperation"] = operation,
            ["CacheKey"] = key,
            ["CacheHit"] = hit,
            ["Duration"] = durationMs ?? 0
        });

        if (hit)
        {
            logger.LogDebug("Cache {Operation} hit for key '{Key}' in {Duration}ms", operation, key, durationMs ?? 0);
        }
        else
        {
            logger.LogDebug("Cache {Operation} miss for key '{Key}' in {Duration}ms", operation, key, durationMs ?? 0);
        }
    }

    public static void LogPerformanceMetric<T>(this ILogger<T> logger, string metricName, double value, string unit = "ms")
    {
        using var scope = logger.BeginScope(new Dictionary<string, object>
        {
            ["MetricType"] = "Performance",
            ["MetricName"] = metricName,
            ["MetricValue"] = value,
            ["MetricUnit"] = unit
        });

        logger.LogInformation("Performance metric '{MetricName}': {Value} {Unit}", metricName, value, unit);
    }

    public static void LogBusinessEvent<T>(this ILogger<T> logger, string eventName, string userId, Dictionary<string, object>? properties = null)
    {
        var scopeData = new Dictionary<string, object>
        {
            ["EventType"] = "Business",
            ["EventName"] = eventName,
            ["UserId"] = userId
        };

        if (properties != null)
        {
            foreach (var prop in properties)
            {
                scopeData[$"Event_{prop.Key}"] = prop.Value;
            }
        }

        using var scope = logger.BeginScope(scopeData);
        logger.LogInformation("Business event '{EventName}' for user '{UserId}'", eventName, userId);
    }
}
