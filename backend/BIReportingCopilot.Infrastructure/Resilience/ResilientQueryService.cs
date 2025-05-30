using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using BIReportingCopilot.Core.Exceptions;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.DTOs;
using Microsoft.Data.SqlClient;

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
            .AdvancedCircuitBreakerAsync(
                failureThreshold: 0.5,
                samplingDuration: TimeSpan.FromSeconds(30),
                minimumThroughput: 3,
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
            var yesterday = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd");
            return new List<string> {
                $"Show me total deposits for yesterday ({yesterday})",
                "Top 10 players by deposits in the last 7 days",
                "Show me daily revenue for the last week"
            };
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
            // Extract SQL from QueryRequest and validate it
            return await _innerService.ValidateQueryAsync(request.Question);
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
            // Extract SQL from QueryRequest and optimize it
            return await _innerService.OptimizeQueryAsync(request.Question);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Query optimization failed");
            return new QueryOptimizationResult
            {
                OriginalQuery = request.Question,
                OptimizedQuery = request.Question,
                AppliedOptimizations = new List<string>(), // Empty list means no optimization applied
                Suggestions = new List<OptimizationSuggestion>
                {
                    new OptimizationSuggestion
                    {
                        Description = "Optimization service temporarily unavailable",
                        Type = "Service",
                        Recommendation = "Please try again later"
                    }
                }
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
            Error = message,
            Result = new QueryResult
            {
                Data = new object[0],
                Metadata = new QueryMetadata
                {
                    Columns = new ColumnMetadata[0],
                    RowCount = 0,
                    ColumnCount = 0
                },
                IsSuccessful = false
            },
            ExecutionTimeMs = 0,
            Confidence = 0,
            Suggestions = Array.Empty<string>(),
            Cached = false,
            Timestamp = DateTime.UtcNow
        };
    }

    // Missing IQueryService methods
    public async Task InvalidateQueryCacheAsync(string pattern)
    {
        await _databaseRetryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogInformation("Invalidating query cache with pattern: {Pattern}", pattern);
            await _innerService.InvalidateQueryCacheAsync(pattern);
        });
    }

    public async Task<ProcessedQuery> ProcessAdvancedQueryAsync(string query, string userId, Core.Models.QueryContext? context = null)
    {
        return await _databaseRetryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogInformation("Processing advanced query for user {UserId}: {Query}", userId, query);
            return await _innerService.ProcessAdvancedQueryAsync(query, userId, context);
        });
    }

    public async Task<double> CalculateSemanticSimilarityAsync(string query1, string query2)
    {
        return await _databaseRetryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Calculating semantic similarity between queries");
            return await _innerService.CalculateSemanticSimilarityAsync(query1, query2);
        });
    }

    public async Task<List<ProcessedQuery>> FindSimilarQueriesAsync(string query, string userId, int limit = 5)
    {
        return await _databaseRetryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Finding similar queries for user {UserId} with limit {Limit}", userId, limit);
            return await _innerService.FindSimilarQueriesAsync(query, userId, limit);
        });
    }

    public async Task<bool> ValidateQueryAsync(string sql)
    {
        return await _databaseRetryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Validating SQL query");
            return await _innerService.ValidateQueryAsync(sql);
        });
    }

    public async Task<QueryOptimizationResult> OptimizeQueryAsync(string sql)
    {
        return await _databaseRetryPolicy.ExecuteAsync(async () =>
        {
            _logger.LogDebug("Optimizing SQL query");
            return await _innerService.OptimizeQueryAsync(sql);
        });
    }
}
