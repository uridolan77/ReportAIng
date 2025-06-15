using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models.QuerySuggestions;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;
using CoreModels = BIReportingCopilot.Core.Models;
using ContextType = BIReportingCopilot.Infrastructure.Data.Contexts.ContextType;

namespace BIReportingCopilot.Infrastructure.Query;

/// <summary>
/// Enhanced QueryService using CQRS pattern with MediatR for better separation of concerns
/// Delegates complex business logic to focused command and query handlers
/// </summary>
public class QueryService : IQueryService
{
    private readonly ILogger<QueryService> _logger;
    private readonly IMediator _mediator;
    private readonly IDbContextFactory _contextFactory;

    public QueryService(
        ILogger<QueryService> logger,
        IMediator mediator,
        IDbContextFactory contextFactory)
    {
        _logger = logger;
        _mediator = mediator;
        _contextFactory = contextFactory;

        _logger.LogInformation("🔧 Enhanced QueryService initialized with CQRS pattern using MediatR");
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId)
    {
        return await ProcessQueryAsync(request, userId, CancellationToken.None);
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🎯 CQRS QueryService - Delegating to ProcessQueryCommandHandler for user {UserId}: {Question}",
                userId, request.Question);

            // Delegate to CQRS command handler for better separation of concerns
            var command = new ProcessQueryCommand
            {
                Question = request.Question,
                SessionId = request.SessionId,
                UserId = userId,
                Options = request.Options,
                CancellationToken = cancellationToken
            };

            return await _mediator.Send(command, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.ProcessQueryAsync for user {UserId}", userId);
            throw;
        }
    }

    public async Task<List<QueryHistoryItem>> GetQueryHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("📋 CQRS QueryService - Delegating to GetQueryHistoryQueryHandler for user {UserId}", userId);

            var query = new GetQueryHistoryQuery
            {
                UserId = userId,
                Page = page,
                PageSize = pageSize
            };

            var result = await _mediator.Send(query);
            return result.Items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.GetQueryHistoryAsync for user {UserId}", userId);
            return new List<QueryHistoryItem>();
        }
    }

    public async Task<bool> SubmitFeedbackAsync(QueryFeedback feedback, string userId)
    {
        try
        {
            _logger.LogInformation("🎯 CQRS QueryService - Delegating to SubmitQueryFeedbackCommandHandler");

            var command = new SubmitQueryFeedbackCommand
            {
                Feedback = feedback,
                UserId = userId
            };

            return await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.SubmitFeedbackAsync for query {QueryId}", feedback.QueryId);
            return false;
        }
    }

    public async Task<List<string>> GetQuerySuggestionsAsync(string userId, string? context = null)
    {
        try
        {
            _logger.LogInformation("🎯 CQRS QueryService - Delegating to GetQuerySuggestionsQueryHandler");

            var query = new GetQuerySuggestionsQuery
            {
                UserId = userId,
                Context = context
            };

            return await _mediator.Send(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.GetQuerySuggestionsAsync for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<QueryResponse?> GetCachedQueryAsync(string queryHash)
    {
        try
        {
            var query = new GetCachedQueryQuery { QueryHash = queryHash };
            return await _mediator.Send(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.GetCachedQueryAsync");
            return null;
        }
    }

    public async Task CacheQueryAsync(string queryHash, QueryResponse response, TimeSpan? expiry = null)
    {
        try
        {
            var command = new CacheQueryCommand
            {
                QueryHash = queryHash,
                Response = response,
                Expiry = expiry
            };
            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.CacheQueryAsync");
        }
    }

    public async Task InvalidateQueryCacheAsync(string pattern)
    {
        try
        {
            var command = new InvalidateQueryCacheCommand { Pattern = pattern };
            await _mediator.Send(command);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.InvalidateQueryCacheAsync");
        }
    }

    // All business logic has been moved to CQRS handlers for better separation of concerns

    #region Interface Compliance Methods - Delegated to CQRS Handlers

    public async Task<ProcessedQuery> ProcessAdvancedQueryAsync(string query, string userId, CoreModels.QueryContext? context = null)
    {
        _logger.LogInformation("Processing advanced query for user {UserId}", userId);

        // For now, return a basic processed query result
        // This can be enhanced later with actual advanced processing logic
        return new ProcessedQuery
        {
            OriginalQuery = query,
            Sql = query, // For now, just return the original query
            Explanation = "Basic processing - advanced features to be implemented",
            Confidence = 0.8,
            UserId = userId,
            ProcessedAt = DateTime.UtcNow,
            QueryType = "Advanced"
        };
    }

    public Task<double> CalculateSemanticSimilarityAsync(string query1, string query2)
    {
        // This could be implemented as a separate CQRS query if needed
        _logger.LogInformation("🎯 Semantic similarity calculation not yet implemented in CQRS pattern");
        return Task.FromResult(0.0);
    }

    public Task<List<ProcessedQuery>> FindSimilarQueriesAsync(string query, string userId, int limit = 5)
    {
        // This could be implemented as a separate CQRS query if needed
        _logger.LogInformation("🎯 Similar queries search not yet implemented in CQRS pattern");
        return Task.FromResult(new List<ProcessedQuery>());
    }

    public Task<BIReportingCopilot.Core.DTOs.QueryPerformanceMetrics> GetQueryPerformanceAsync(string queryHash)
    {
        // This could be implemented as a separate CQRS query if needed
        _logger.LogInformation("🎯 Query performance metrics not yet implemented in CQRS pattern");
        return Task.FromResult(new BIReportingCopilot.Core.DTOs.QueryPerformanceMetrics
        {
            QueryHash = queryHash,
            ExecutionTime = TimeSpan.FromMilliseconds(100),
            RowsAffected = 0,
            FromCache = false,
            ExecutedAt = DateTime.UtcNow
        });
    }

    public async Task<bool> ValidateQueryAsync(string sql)
    {
        try
        {
            var query = new ValidateSqlQuery { Sql = sql };
            return await _mediator.Send(query);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.ValidateQueryAsync");
            return false;
        }
    }

    public async Task<List<QuerySuggestion>> GetSmartSuggestionsAsync(string userId, string? context = null)
    {
        try
        {
            var suggestions = await GetQuerySuggestionsAsync(userId, context);
            return suggestions.Select(s => new QuerySuggestion { QueryText = s }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in QueryService.GetSmartSuggestionsAsync");
            return new List<QuerySuggestion>();
        }
    }

    public Task<QueryOptimizationResult> OptimizeQueryAsync(string sql)
    {
        // This could be implemented as a separate CQRS command if needed
        _logger.LogInformation("🎯 Query optimization not yet implemented in CQRS pattern");
        return Task.FromResult(new QueryOptimizationResult
        {
            OriginalQuery = sql,
            OptimizedQuery = sql,
            ImprovementScore = 0.0,
            Suggestions = new List<OptimizationSuggestion>()
        });
    }

    #endregion

    #region Missing Interface Method Implementations

    /// <summary>
    /// Execute query (IQueryService interface)
    /// </summary>
    public async Task<QueryResult> ExecuteQueryAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Executing query request for user: {UserId}", request.UserId);

            var response = await ProcessQueryAsync(request, request.UserId ?? "system", cancellationToken);

            return new QueryResult
            {
                Data = response.Data,
                Metadata = new QueryMetadata
                {
                    ColumnCount = response.Columns?.Length ?? 0,
                    RowCount = response.Data?.Length ?? 0,
                    ExecutionTimeMs = (int)(response.ExecutionTime?.TotalMilliseconds ?? 0),
                    Columns = response.Columns ?? Array.Empty<ColumnMetadata>(),
                    DataSource = "DailyActionsDB",
                    QueryTimestamp = DateTime.UtcNow
                },
                IsSuccessful = response.IsSuccessful
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing query request");
            return new QueryResult
            {
                IsSuccessful = false,
                Metadata = new QueryMetadata
                {
                    Error = ex.Message,
                    QueryTimestamp = DateTime.UtcNow
                }
            };
        }
    }

    /// <summary>
    /// Execute select query (IQueryService interface)
    /// </summary>
    public async Task<QueryResult> ExecuteSelectQueryAsync(string sqlQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new QueryRequest
            {
                Question = sqlQuery,
                UserId = "system"
            };

            return await ExecuteQueryAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing select query");
            return new QueryResult
            {
                IsSuccessful = false,
                Metadata = new QueryMetadata
                {
                    Error = ex.Message,
                    QueryTimestamp = DateTime.UtcNow
                }
            };
        }
    }

    /// <summary>
    /// Validate query (IQueryService interface)
    /// </summary>
    public async Task<bool> ValidateQueryAsync(string sqlQuery, CancellationToken cancellationToken = default)
    {
        return await ValidateQueryAsync(sqlQuery);
    }

    /// <summary>
    /// Get query history (IQueryService interface)
    /// </summary>
    public async Task<List<UnifiedQueryHistoryEntity>> GetQueryHistoryAsync(string userId, int pageSize = 50, int pageNumber = 1, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                var appContext = (BICopilotContext)context;
                return await appContext.QueryHistory
                    .Where(q => q.UserId == userId)
                    .OrderByDescending(q => q.ExecutedAt)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync(cancellationToken);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query history for user: {UserId}", userId);
            return new List<UnifiedQueryHistoryEntity>();
        }
    }

    /// <summary>
    /// Get cached query result (IQueryService interface)
    /// </summary>
    public async Task<QueryResult> GetCachedQueryResultAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var cachedResponse = await GetCachedQueryAsync(cacheKey);
            if (cachedResponse != null)
            {
                return new QueryResult
                {
                    Data = cachedResponse.Data,
                    Metadata = new QueryMetadata
                    {
                        ColumnCount = cachedResponse.Columns?.Length ?? 0,
                        RowCount = cachedResponse.Data?.Length ?? 0,
                        ExecutionTimeMs = (int)(cachedResponse.ExecutionTime?.TotalMilliseconds ?? 0),
                        Columns = cachedResponse.Columns ?? Array.Empty<ColumnMetadata>(),
                        DataSource = "Cache",
                        QueryTimestamp = DateTime.UtcNow
                    },
                    IsSuccessful = cachedResponse.IsSuccessful
                };
            }

            return new QueryResult
            {
                IsSuccessful = false,
                Metadata = new QueryMetadata
                {
                    Error = "No cached result found",
                    QueryTimestamp = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting cached query result for key: {CacheKey}", cacheKey);
            return new QueryResult
            {
                IsSuccessful = false,
                Metadata = new QueryMetadata
                {
                    Error = ex.Message,
                    QueryTimestamp = DateTime.UtcNow
                }
            };
        }
    }

    /// <summary>
    /// Save query result (IQueryService interface)
    /// </summary>
    public async Task SaveQueryResultAsync(string cacheKey, QueryResult result, TimeSpan? expiry = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = new QueryResponse
            {
                Data = result.Data,
                Columns = result.Metadata.Columns,
                ExecutionTime = TimeSpan.FromMilliseconds(result.Metadata.ExecutionTimeMs),
                IsSuccessful = result.IsSuccessful,
                Sql = cacheKey // Using cache key as SQL for now
            };

            await CacheQueryAsync(cacheKey, response, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving query result for key: {CacheKey}", cacheKey);
        }
    }

    /// <summary>
    /// Get performance metrics (IQueryService interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Interfaces.Query.QueryPerformanceMetrics> GetPerformanceMetricsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                var appContext = (BICopilotContext)context;
                var recentQueries = await appContext.QueryHistory
                    .Where(q => q.ExecutedAt >= DateTime.UtcNow.AddDays(-7))
                    .ToListAsync(cancellationToken);

                return new BIReportingCopilot.Core.Interfaces.Query.QueryPerformanceMetrics
                {
                    TotalQueries = recentQueries.Count,
                    AverageExecutionTime = recentQueries.Count > 0 ?
                        recentQueries.Average(q => q.ExecutionTimeMs) :
                        0.0,
                    SuccessRate = recentQueries.Count > 0 ?
                        (double)recentQueries.Count(q => q.IsSuccessful) / recentQueries.Count :
                        0.0,
                    CacheHitRate = 0.0, // Not available in current implementation
                    LastUpdated = DateTime.UtcNow
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting performance metrics");
            return new BIReportingCopilot.Core.Interfaces.Query.QueryPerformanceMetrics
            {
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    // Additional IQueryService interface methods expected by Infrastructure services
    /// <summary>
    /// Process query async (IQueryService interface)
    /// </summary>
    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        return await ProcessQueryAsync(request, request.UserId ?? "system", cancellationToken);
    }

    /// <summary>
    /// Submit feedback async (IQueryService interface)
    /// </summary>
    public async Task<bool> SubmitFeedbackAsync(string queryId, string feedback, CancellationToken cancellationToken = default)
    {
        try
        {
            var feedbackObj = new QueryFeedback
            {
                QueryId = queryId,
                FeedbackText = feedback,
                Rating = 3, // Default rating
                SubmittedAt = DateTime.UtcNow
            };
            return await SubmitFeedbackAsync(feedbackObj, "system");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for query {QueryId}", queryId);
            return false;
        }
    }

    /// <summary>
    /// Get query suggestions async (IQueryService interface)
    /// </summary>
    public async Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string partialQuery, CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestions = await GetQuerySuggestionsAsync("system", partialQuery);
            return suggestions.Select(s => new QuerySuggestion { QueryText = s }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query suggestions");
            return new List<QuerySuggestion>();
        }
    }

    /// <summary>
    /// Get cached query async (IQueryService interface)
    /// </summary>
    public async Task<QueryResult?> GetCachedQueryAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        var result = await GetCachedQueryResultAsync(cacheKey, cancellationToken);
        return result.IsSuccessful ? result : null;
    }

    /// <summary>
    /// Cache query async (IQueryService interface)
    /// </summary>
    public async Task CacheQueryAsync(string cacheKey, QueryResult result, CancellationToken cancellationToken = default)
    {
        await SaveQueryResultAsync(cacheKey, result, null, cancellationToken);
    }

    /// <summary>
    /// Get query performance async (IQueryService interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Interfaces.Query.QueryPerformanceMetrics> GetQueryPerformanceAsync(CancellationToken cancellationToken = default)
    {
        return await GetPerformanceMetricsAsync(cancellationToken);
    }

    /// <summary>
    /// Get smart suggestions async (IQueryService interface)
    /// </summary>
    public async Task<List<QuerySuggestion>> GetSmartSuggestionsAsync(string query, CancellationToken cancellationToken = default)
    {
        return await GetSmartSuggestionsAsync("system", query);
    }

    /// <summary>
    /// Optimize query async (IQueryService interface)
    /// </summary>
    public async Task<QueryResult> OptimizeQueryAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            // For now, just execute the query as-is
            return await ExecuteSelectQueryAsync(query, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing query");
            return new QueryResult
            {
                IsSuccessful = false,
                Metadata = new QueryMetadata
                {
                    Error = ex.Message,
                    QueryTimestamp = DateTime.UtcNow
                }
            };
        }
    }

    /// <summary>
    /// Invalidate query cache async (IQueryService interface)
    /// </summary>
    public async Task InvalidateQueryCacheAsync(string pattern, CancellationToken cancellationToken = default)
    {
        await InvalidateQueryCacheAsync(pattern);
    }

    /// <summary>
    /// Process advanced query async (IQueryService interface)
    /// </summary>
    public async Task<QueryResult> ProcessAdvancedQueryAsync(QueryRequest request, CancellationToken cancellationToken = default)
    {
        return await ExecuteQueryAsync(request, cancellationToken);
    }

    /// <summary>
    /// Calculate semantic similarity async (IQueryService interface)
    /// </summary>
    public async Task<double> CalculateSemanticSimilarityAsync(string query1, string query2, CancellationToken cancellationToken = default)
    {
        return await CalculateSemanticSimilarityAsync(query1, query2);
    }

    /// <summary>
    /// Find similar queries async (IQueryService interface)
    /// </summary>
    public async Task<List<UnifiedQueryHistoryEntity>> FindSimilarQueriesAsync(string query, CancellationToken cancellationToken = default)
    {
        try
        {
            // Stub implementation - return empty list for now
            return new List<UnifiedQueryHistoryEntity>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries");
            return new List<UnifiedQueryHistoryEntity>();
        }
    }

    // Missing IQueryService interface methods
    public async Task<bool> SubmitFeedbackAsync(QueryFeedback feedback, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting query feedback for user {UserId}", userId);
            
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                
                // Create feedback entity
                var feedbackEntity = new QueryFeedbackEntity
                {
                    Id = Guid.NewGuid().ToString(),
                    UserId = userId,
                    QueryId = feedback.QueryId,
                    Rating = feedback.Rating,
                    Comments = feedback.Comments,
                    CreatedAt = DateTime.UtcNow
                };
                
                queryContext.QueryFeedback.Add(feedbackEntity);
                await queryContext.SaveChangesAsync(cancellationToken);
                
                _logger.LogInformation("Query feedback submitted successfully for user {UserId}", userId);
                return true;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting query feedback for user {UserId}", userId);
            return false;
        }
    }

    public async Task<List<QuerySuggestion>> GetQuerySuggestionsAsync(string partialQuery, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Getting query suggestions for user {UserId}, partial query: {PartialQuery}", userId, partialQuery);
            
            var suggestions = new List<QuerySuggestion>();
            
            // Add some basic suggestions based on partial query
            if (!string.IsNullOrWhiteSpace(partialQuery))
            {
                suggestions.Add(new QuerySuggestion
                {
                    Text = $"{partialQuery} by date",
                    Confidence = 0.8,
                    Category = new SuggestionCategory { CategoryKey = "Temporal", Title = "Temporal" }
                });
                
                suggestions.Add(new QuerySuggestion
                {
                    Text = $"{partialQuery} top 10",
                    Confidence = 0.7,
                    Category = new SuggestionCategory { CategoryKey = "Aggregation", Title = "Aggregation" }
                });
            }
            
            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query suggestions for user {UserId}", userId);
            return new List<QuerySuggestion>();
        }
    }

    public async Task<List<UnifiedQueryHistoryEntity>> FindSimilarQueriesAsync(string query, string userId, int maxResults = 10, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Finding similar queries for user {UserId}, query: {Query}", userId, query);
            
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                
                // Find similar queries based on text similarity
                var similarQueries = await queryContext.QueryHistory
                    .Where(q => q.UserId == userId && q.Query.Contains(query))
                    .Take(maxResults)
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("Found {Count} similar queries for user {UserId}", similarQueries.Count, userId);
                return similarQueries;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries for user {UserId}", userId);
            return new List<UnifiedQueryHistoryEntity>();
        }
    }

    #endregion
}
