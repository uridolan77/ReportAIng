using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Data.SqlClient;

namespace BIReportingCopilot.Infrastructure.Resilience;

/// <summary>
/// Resilient query service with circuit breaker, retry policies, and timeout handling
/// </summary>
public class ResilientQueryService : IQueryService
{
    private readonly IQueryService _innerService;
    private readonly ILogger<ResilientQueryService> _logger;
    private readonly IAsyncPolicy _databaseRetryPolicy;
    private readonly IAsyncPolicy _circuitBreakerPolicy;
    private readonly IAsyncPolicy _combinedPolicy;

    public ResilientQueryService(IQueryService innerService, ILogger<ResilientQueryService> logger)
    {
        _innerService = innerService;
        _logger = logger;

        // Configure database retry policy
        _databaseRetryPolicy = Policy
            .Handle<SqlException>(ex => IsTransientError(ex))
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (exception, timespan, retryCount, context) =>
                {
                    _logger.LogWarning("Database operation retry {RetryCount} after {Delay}ms. Error: {Error}",
                        retryCount, timespan.TotalMilliseconds, exception.Message);
                });

        // Configure circuit breaker for database operations
        _circuitBreakerPolicy = Policy
            .Handle<SqlException>()
            .Or<TimeoutException>()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 3,
                durationOfBreak: TimeSpan.FromMinutes(1),
                onBreak: (exception, duration) =>
                {
                    _logger.LogError("Database circuit breaker opened for {Duration}ms. Error: {Error}",
                        duration.TotalMilliseconds, exception.Message);
                },
                onReset: () =>
                {
                    _logger.LogInformation("Database circuit breaker reset - database connection recovered");
                },
                onHalfOpen: () =>
                {
                    _logger.LogInformation("Database circuit breaker half-open - testing database connection");
                });

        // Combine policies
        _combinedPolicy = Policy.WrapAsync(_databaseRetryPolicy, _circuitBreakerPolicy);
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId)
    {
        return await ProcessQueryAsync(request, userId, CancellationToken.None);
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId, CancellationToken cancellationToken)
    {
        try
        {
            return await _combinedPolicy.ExecuteAsync(async () =>
            {
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
                using var combinedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

                return await _innerService.ProcessQueryAsync(request, userId, combinedCts.Token);
            });
        }
        catch (CircuitBreakerOpenException)
        {
            _logger.LogWarning("Query processing failed - circuit breaker is open");
            return CreateErrorResponse("Database service is temporarily unavailable. Please try again later.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Query processing failed after all retry attempts for user {UserId}", userId);
            return CreateErrorResponse("Query processing failed. Please check your query and try again.");
        }
    }

    public async Task<List<QueryHistoryItem>> GetQueryHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            var simpleRetryPolicy = Policy
                .Handle<SqlException>(ex => IsTransientError(ex))
                .WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1));

            return await simpleRetryPolicy.ExecuteAsync(async () =>
            {
                return await _innerService.GetQueryHistoryAsync(userId, page, pageSize);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get query history for user {UserId}", userId);
            return new List<QueryHistoryItem>();
        }
    }

    public async Task<bool> SubmitFeedbackAsync(QueryFeedback feedback, string userId)
    {
        try
        {
            var simpleRetryPolicy = Policy
                .Handle<SqlException>(ex => IsTransientError(ex))
                .WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1));

            return await simpleRetryPolicy.ExecuteAsync(async () =>
            {
                return await _innerService.SubmitFeedbackAsync(feedback, userId);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to submit feedback for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<string>> GetQuerySuggestionsAsync(string userId, string? context = null)
    {
        try
        {
            var simpleRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1));

            return await simpleRetryPolicy.ExecuteAsync(async () =>
            {
                return await _innerService.GetQuerySuggestionsAsync(userId, context);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get query suggestions for user {UserId}, using defaults", userId);
            return new List<string> { "Show me all data", "Count total records", "Show recent data" };
        }
    }

    public async Task<QueryResponse?> GetCachedQueryAsync(string queryHash)
    {
        try
        {
            // Cache operations are less critical, use simple retry
            var cacheRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(500));

            return await cacheRetryPolicy.ExecuteAsync(async () =>
            {
                return await _innerService.GetCachedQueryAsync(queryHash);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get cached query for hash {QueryHash}", queryHash);
            return null;
        }
    }

    public async Task CacheQueryAsync(string queryHash, QueryResponse response, TimeSpan? expiry = null)
    {
        try
        {
            // Fire and forget for caching
            _ = Task.Run(async () =>
            {
                try
                {
                    await _innerService.CacheQueryAsync(queryHash, response, expiry);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to cache query for hash {QueryHash}", queryHash);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to initiate cache operation for hash {QueryHash}", queryHash);
        }
    }

    public async Task<QueryPerformanceMetrics> GetQueryPerformanceAsync(string queryHash)
    {
        try
        {
            return await _innerService.GetQueryPerformanceAsync(queryHash);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get query performance for hash {QueryHash}", queryHash);
            return new QueryPerformanceMetrics
            {
                QueryHash = queryHash,
                ExecutionTime = TimeSpan.Zero,
                FromCache = false
            };
        }
    }

    public async Task<bool> ValidateQueryAsync(QueryRequest request)
    {
        try
        {
            return await _innerService.ValidateQueryAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Query validation failed, allowing by default");
            return true; // Fail open for validation
        }
    }

    public async Task<List<QuerySuggestion>> GetSmartSuggestionsAsync(string userId, string? context = null)
    {
        try
        {
            var simpleRetryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(2, _ => TimeSpan.FromSeconds(1));

            return await simpleRetryPolicy.ExecuteAsync(async () =>
            {
                return await _innerService.GetSmartSuggestionsAsync(userId, context);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get smart suggestions for user {UserId}", userId);
            return new List<QuerySuggestion>();
        }
    }

    public async Task<QueryOptimizationResult> OptimizeQueryAsync(QueryRequest request)
    {
        try
        {
            return await _innerService.OptimizeQueryAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Query optimization failed");
            return new QueryOptimizationResult
            {
                OriginalQuery = request.Question,
                OptimizedQuery = request.Question,
                OptimizationApplied = false,
                Suggestions = new List<string> { "Optimization service temporarily unavailable" }
            };
        }
    }

    private static bool IsTransientError(SqlException ex)
    {
        // Common transient SQL error codes
        var transientErrorCodes = new[]
        {
            2,    // Timeout
            53,   // Network path not found
            121,  // Semaphore timeout
            1205, // Deadlock
            1222, // Lock request timeout
            8645, // Timeout waiting for memory resource
            8651  // Low memory condition
        };

        return transientErrorCodes.Contains(ex.Number);
    }

    private static QueryResponse CreateErrorResponse(string message)
    {
        return new QueryResponse
        {
            Success = false,
            ErrorMessage = message,
            Data = new object[0],
            Columns = new ColumnInfo[0],
            ExecutionTimeMs = 0,
            RowCount = 0
        };
    }
}
