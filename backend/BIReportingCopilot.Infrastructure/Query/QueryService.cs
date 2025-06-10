using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models.QuerySuggestions;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MediatR;
using CoreModels = BIReportingCopilot.Core.Models;

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

    public Task<QueryPerformanceMetrics> GetQueryPerformanceAsync(string queryHash)
    {
        // This could be implemented as a separate CQRS query if needed
        _logger.LogInformation("🎯 Query performance metrics not yet implemented in CQRS pattern");
        return Task.FromResult(new QueryPerformanceMetrics
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
}
