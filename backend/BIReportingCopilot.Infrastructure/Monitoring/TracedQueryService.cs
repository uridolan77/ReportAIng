using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI;

namespace BIReportingCopilot.Infrastructure.Monitoring;

/// <summary>
/// Query service wrapper that adds distributed tracing and detailed monitoring
/// </summary>
public class TracedQueryService : IQueryService
{
    private readonly IQueryService _innerService;
    private readonly ILogger<TracedQueryService> _logger;
    private readonly IMetricsCollector _metricsCollector;
    private static readonly ActivitySource ActivitySource = new("BIReportingCopilot.QueryService");

    public TracedQueryService(
        IQueryService innerService,
        ILogger<TracedQueryService> logger,
        IMetricsCollector metricsCollector)
    {
        _innerService = innerService;
        _logger = logger;
        _metricsCollector = metricsCollector;
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId)
    {
        return await ProcessQueryAsync(request, userId, CancellationToken.None);
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId, CancellationToken cancellationToken)
    {
        using var activity = ActivitySource.StartActivity("ProcessQuery");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Add trace tags
            activity?.SetTag("user.id", userId);
            activity?.SetTag("query.session", request.SessionId ?? "Unknown");
            activity?.SetTag("query.question", request.Question);
            activity?.SetTag("query.max_rows", request.Options?.MaxRows ?? 0);

            _logger.LogInformation("Processing query for user {UserId}: {Question}", userId, request.Question);

            var response = await _innerService.ProcessQueryAsync(request, userId, cancellationToken);

            stopwatch.Stop();

            // Add response tags
            activity?.SetTag("query.success", response.Success);
            activity?.SetTag("query.duration_ms", response.ExecutionTimeMs);
            activity?.SetTag("query.row_count", response.Result?.Metadata?.RowCount ?? 0);

            if (!response.Success)
            {
                activity?.SetTag("error", true);
                activity?.SetTag("error.message", response.Error);
            }

            // Record metrics
            _metricsCollector.RecordQueryExecution(
                "query",
                stopwatch.ElapsedMilliseconds,
                response.Success,
                response.Result?.Metadata?.RowCount ?? 0);

            _logger.LogInformation("Query processed successfully for user {UserId} in {Duration}ms, returned {RowCount} rows",
                userId, stopwatch.ElapsedMilliseconds, response.Result?.Metadata?.RowCount ?? 0);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            activity?.SetTag("error", true);
            activity?.SetTag("error.type", ex.GetType().Name);
            activity?.SetTag("error.message", ex.Message);

            // Record error metrics
            _metricsCollector.RecordError("query_processing", "TracedQueryService", ex);

            _logger.LogError(ex, "Query processing failed for user {UserId} after {Duration}ms",
                userId, stopwatch.ElapsedMilliseconds);

            throw;
        }
    }

    public async Task<List<QueryHistoryItem>> GetQueryHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        using var activity = ActivitySource.StartActivity("GetQueryHistory");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("user.id", userId);
            activity?.SetTag("page", page);
            activity?.SetTag("page_size", pageSize);

            var result = await _innerService.GetQueryHistoryAsync(userId, page, pageSize);

            stopwatch.Stop();
            activity?.SetTag("result.count", result.Count);

            _metricsCollector.RecordQueryExecution("history", stopwatch.ElapsedMilliseconds, true, result.Count);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("query_history", "TracedQueryService", ex);
            throw;
        }
    }

    public async Task<bool> SubmitFeedbackAsync(QueryFeedback feedback, string userId)
    {
        using var activity = ActivitySource.StartActivity("SubmitFeedback");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("user.id", userId);
            activity?.SetTag("feedback.rating", FeedbackLearningEngineExtensions.GetRatingFromFeedback(feedback));
            activity?.SetTag("feedback.type", feedback.Feedback ?? "Unknown");

            var result = await _innerService.SubmitFeedbackAsync(feedback, userId);

            stopwatch.Stop();
            activity?.SetTag("success", result);

            _metricsCollector.IncrementCounter("feedback_submissions", new TagList
            {
                ["user_id"] = userId,
                ["success"] = result.ToString().ToLower(),
                ["rating"] = FeedbackLearningEngineExtensions.GetRatingFromFeedback(feedback).ToString()
            });

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("feedback_submission", "TracedQueryService", ex);
            throw;
        }
    }

    public async Task<List<string>> GetQuerySuggestionsAsync(string userId, string? context = null)
    {
        using var activity = ActivitySource.StartActivity("GetQuerySuggestions");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("user.id", userId);
            activity?.SetTag("context", context ?? "none");

            var result = await _innerService.GetQuerySuggestionsAsync(userId, context);

            stopwatch.Stop();
            activity?.SetTag("suggestions.count", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("query_suggestions", "TracedQueryService", ex);
            throw;
        }
    }

    public async Task<QueryResponse?> GetCachedQueryAsync(string queryHash)
    {
        using var activity = ActivitySource.StartActivity("GetCachedQuery");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("query.hash", queryHash);

            var result = await _innerService.GetCachedQueryAsync(queryHash);

            stopwatch.Stop();
            var hit = result != null;
            activity?.SetTag("cache.hit", hit);

            _metricsCollector.RecordCacheOperation("get", hit, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("cache_get", "TracedQueryService", ex);
            throw;
        }
    }

    public async Task CacheQueryAsync(string queryHash, QueryResponse response, TimeSpan? expiry = null)
    {
        using var activity = ActivitySource.StartActivity("CacheQuery");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("query.hash", queryHash);
            activity?.SetTag("cache.expiry_minutes", expiry?.TotalMinutes ?? 0);

            await _innerService.CacheQueryAsync(queryHash, response, expiry);

            stopwatch.Stop();
            _metricsCollector.RecordCacheOperation("set", true, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("cache_set", "TracedQueryService", ex);
            throw;
        }
    }

    public async Task<QueryPerformanceMetrics> GetQueryPerformanceAsync(string queryHash)
    {
        using var activity = ActivitySource.StartActivity("GetQueryPerformance");

        try
        {
            activity?.SetTag("query.hash", queryHash);

            var result = await _innerService.GetQueryPerformanceAsync(queryHash);

            activity?.SetTag("performance.execution_time_ms", result.ExecutionTime.TotalMilliseconds);
            activity?.SetTag("performance.from_cache", result.FromCache);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("query_performance", "TracedQueryService", ex);
            throw;
        }
    }

    public async Task<bool> ValidateQueryAsync(QueryRequest request)
    {
        using var activity = ActivitySource.StartActivity("ValidateQuery");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("query.question", request.Question);
            activity?.SetTag("query.session", request.SessionId ?? "Unknown");

            var result = await _innerService.ValidateQueryAsync(request);

            stopwatch.Stop();
            activity?.SetTag("validation.result", result);

            _metricsCollector.IncrementCounter("query_validations", new TagList
            {
                ["result"] = result.ToString().ToLower(),
                ["session"] = request.SessionId ?? "Unknown"
            });

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("query_validation", "TracedQueryService", ex);
            throw;
        }
    }

    public async Task<List<QuerySuggestion>> GetSmartSuggestionsAsync(string userId, string? context = null)
    {
        using var activity = ActivitySource.StartActivity("GetSmartSuggestions");

        try
        {
            activity?.SetTag("user.id", userId);
            activity?.SetTag("context", context ?? "none");

            var result = await _innerService.GetSmartSuggestionsAsync(userId, context);

            activity?.SetTag("suggestions.count", result.Count);

            return result;
        }
        catch (Exception ex)
        {
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("smart_suggestions", "TracedQueryService", ex);
            throw;
        }
    }

    public async Task<QueryOptimizationResult> OptimizeQueryAsync(QueryRequest request)
    {
        using var activity = ActivitySource.StartActivity("OptimizeQuery");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("query.question", request.Question);

            var result = await _innerService.OptimizeQueryAsync(request);

            stopwatch.Stop();
            activity?.SetTag("optimization.applied", result.OptimizationApplied);
            activity?.SetTag("optimization.suggestions_count", result.Suggestions.Count);

            _metricsCollector.RecordQueryExecution("optimization", stopwatch.ElapsedMilliseconds, result.OptimizationApplied);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("query_optimization", "TracedQueryService", ex);
            throw;
        }
    }

    // Missing IQueryService methods
    public async Task InvalidateQueryCacheAsync(string pattern)
    {
        using var activity = ActivitySource.StartActivity("InvalidateQueryCache");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("cache.pattern", pattern);

            await _innerService.InvalidateQueryCacheAsync(pattern);

            stopwatch.Stop();
            _metricsCollector.RecordCacheOperation("invalidate", true, stopwatch.ElapsedMilliseconds);

            _logger.LogInformation("Invalidated query cache with pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("cache_invalidate", "TracedQueryService", ex);
            _logger.LogError(ex, "Error invalidating query cache with pattern: {Pattern}", pattern);
            throw;
        }
    }

    public async Task<ProcessedQuery> ProcessAdvancedQueryAsync(string query, string userId, Core.Models.QueryContext? context = null)
    {
        using var activity = ActivitySource.StartActivity("ProcessAdvancedQuery");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("user.id", userId);
            activity?.SetTag("query.text", query);
            activity?.SetTag("context.provided", context != null);

            _logger.LogInformation("Processing advanced query for user {UserId}: {Query}", userId, query);

            var result = await _innerService.ProcessAdvancedQueryAsync(query, userId, context);

            stopwatch.Stop();
            activity?.SetTag("query.success", result != null);
            activity?.SetTag("query.confidence", result?.Confidence ?? 0);

            _metricsCollector.RecordQueryExecution("advanced", stopwatch.ElapsedMilliseconds, result != null);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("advanced_query", "TracedQueryService", ex);
            _logger.LogError(ex, "Error processing advanced query for user {UserId}: {Query}", userId, query);
            throw;
        }
    }

    public async Task<double> CalculateSemanticSimilarityAsync(string query1, string query2)
    {
        using var activity = ActivitySource.StartActivity("CalculateSemanticSimilarity");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("query1.length", query1.Length);
            activity?.SetTag("query2.length", query2.Length);

            var similarity = await _innerService.CalculateSemanticSimilarityAsync(query1, query2);

            stopwatch.Stop();
            activity?.SetTag("similarity.score", similarity);

            _metricsCollector.RecordQueryExecution("similarity", stopwatch.ElapsedMilliseconds, true);

            return similarity;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("semantic_similarity", "TracedQueryService", ex);
            _logger.LogError(ex, "Error calculating semantic similarity");
            throw;
        }
    }

    public async Task<List<ProcessedQuery>> FindSimilarQueriesAsync(string query, string userId, int limit = 5)
    {
        using var activity = ActivitySource.StartActivity("FindSimilarQueries");
        var stopwatch = Stopwatch.StartNew();

        try
        {
            activity?.SetTag("user.id", userId);
            activity?.SetTag("query.text", query);
            activity?.SetTag("limit", limit);

            var result = await _innerService.FindSimilarQueriesAsync(query, userId, limit);

            stopwatch.Stop();
            activity?.SetTag("result.count", result.Count);

            _metricsCollector.RecordQueryExecution("similar_queries", stopwatch.ElapsedMilliseconds, true, result.Count);

            _logger.LogDebug("Found {Count} similar queries for user {UserId}", result.Count, userId);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            activity?.SetTag("error", true);
            activity?.SetTag("error.message", ex.Message);

            _metricsCollector.RecordError("similar_queries", "TracedQueryService", ex);
            _logger.LogError(ex, "Error finding similar queries for user {UserId}", userId);
            throw;
        }
    }
}
