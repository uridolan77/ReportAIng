using Microsoft.Extensions.Logging;
using System.Diagnostics;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

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
            activity?.SetTag("query.type", request.Type?.ToString() ?? "Unknown");
            activity?.SetTag("query.question", request.Question);
            activity?.SetTag("query.max_rows", request.Options?.MaxRows ?? 0);

            _logger.LogInformation("Processing query for user {UserId}: {Question}", userId, request.Question);

            var response = await _innerService.ProcessQueryAsync(request, userId, cancellationToken);

            stopwatch.Stop();

            // Add response tags
            activity?.SetTag("query.success", response.Success);
            activity?.SetTag("query.duration_ms", response.ExecutionTimeMs);
            activity?.SetTag("query.row_count", response.RowCount);

            if (!response.Success)
            {
                activity?.SetTag("error", true);
                activity?.SetTag("error.message", response.ErrorMessage);
            }

            // Record metrics
            _metricsCollector.RecordQueryExecution(
                request.Type?.ToString() ?? "Unknown",
                stopwatch.ElapsedMilliseconds,
                response.Success,
                response.RowCount);

            _logger.LogInformation("Query processed successfully for user {UserId} in {Duration}ms, returned {RowCount} rows",
                userId, stopwatch.ElapsedMilliseconds, response.RowCount);

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
            activity?.SetTag("feedback.rating", feedback.Rating);
            activity?.SetTag("feedback.type", feedback.Type?.ToString() ?? "Unknown");

            var result = await _innerService.SubmitFeedbackAsync(feedback, userId);

            stopwatch.Stop();
            activity?.SetTag("success", result);

            _metricsCollector.IncrementCounter("feedback_submissions", new()
            {
                ["user_id"] = userId,
                ["success"] = result.ToString().ToLower(),
                ["rating"] = feedback.Rating.ToString()
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
            activity?.SetTag("query.type", request.Type?.ToString() ?? "Unknown");

            var result = await _innerService.ValidateQueryAsync(request);

            stopwatch.Stop();
            activity?.SetTag("validation.result", result);

            _metricsCollector.IncrementCounter("query_validations", new()
            {
                ["result"] = result.ToString().ToLower(),
                ["type"] = request.Type?.ToString() ?? "Unknown"
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
}
