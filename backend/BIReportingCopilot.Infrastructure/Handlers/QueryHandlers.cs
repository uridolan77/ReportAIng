using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using IContextManager = BIReportingCopilot.Core.Interfaces.IContextManager;

namespace BIReportingCopilot.Infrastructure.Handlers;

/// <summary>
/// Query handler for retrieving user's query history with enhanced pagination and filtering
/// </summary>
public class GetQueryHistoryQueryHandler : IRequestHandler<GetQueryHistoryQuery, PagedResult<QueryHistoryItem>>
{
    private readonly ILogger<GetQueryHistoryQueryHandler> _logger;
    private readonly IDbContextFactory _contextFactory;

    public GetQueryHistoryQueryHandler(
        ILogger<GetQueryHistoryQueryHandler> logger,
        IDbContextFactory contextFactory)
    {
        _logger = logger;
        _contextFactory = contextFactory;
    }

    public async Task<PagedResult<QueryHistoryItem>> Handle(GetQueryHistoryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📋 Retrieving query history - User: {UserId}, Page: {Page}, PageSize: {PageSize}",
                request.UserId, request.Page, request.PageSize);

            return await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                var query = queryContext.QueryHistory.Where(q => q.UserId == request.UserId);

                // Apply filters
                if (request.StartDate.HasValue)
                    query = query.Where(q => q.ExecutedAt >= request.StartDate.Value);

                if (request.EndDate.HasValue)
                    query = query.Where(q => q.ExecutedAt <= request.EndDate.Value);

                if (!string.IsNullOrWhiteSpace(request.SearchTerm))
                    query = query.Where(q => q.Query.Contains(request.SearchTerm));

                var totalCount = await query.CountAsync(cancellationToken);

                var items = await query
                    .OrderByDescending(q => q.ExecutedAt)
                    .Skip((request.Page - 1) * request.PageSize)
                    .Take(request.PageSize)
                    .Select(entity => new QueryHistoryItem
                    {
                        Id = entity.Id.ToString(),
                        UserId = entity.UserId,
                        Question = entity.Query,
                        Sql = entity.GeneratedSql ?? "",
                        Timestamp = entity.ExecutedAt,
                        ExecutionTimeMs = entity.ExecutionTimeMs,
                        Successful = entity.IsSuccessful,
                        Error = entity.ErrorMessage
                    })
                    .ToListAsync(cancellationToken);

                _logger.LogInformation("📋 Retrieved {Count} query history items for user {UserId}",
                    items.Count, request.UserId);

                return new PagedResult<QueryHistoryItem>
                {
                    Items = items,
                    TotalCount = totalCount,
                    Page = request.Page,
                    PageSize = request.PageSize
                };
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving query history for user {UserId}", request.UserId);
            return new PagedResult<QueryHistoryItem>
            {
                Items = new List<QueryHistoryItem>(),
                TotalCount = 0,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}

/// <summary>
/// Query handler for getting cached query results
/// </summary>
public class GetCachedQueryQueryHandler : IRequestHandler<GetCachedQueryQuery, QueryResponse?>
{
    private readonly ILogger<GetCachedQueryQueryHandler> _logger;
    private readonly IQueryCacheService _queryCacheService;

    public GetCachedQueryQueryHandler(
        ILogger<GetCachedQueryQueryHandler> logger,
        IQueryCacheService queryCacheService)
    {
        _logger = logger;
        _queryCacheService = queryCacheService;
    }

    public async Task<QueryResponse?> Handle(GetCachedQueryQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _queryCacheService.GetCachedQueryAsync(request.QueryHash);
            
            if (result != null)
            {
                _logger.LogInformation("🎯 Cache hit for hash: {Hash}", request.QueryHash);
            }
            else
            {
                _logger.LogInformation("🎯 Cache miss for hash: {Hash}", request.QueryHash);
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error retrieving cached query for hash: {Hash}", request.QueryHash);
            return null;
        }
    }
}

/// <summary>
/// Query handler for getting query suggestions
/// </summary>
public class GetQuerySuggestionsQueryHandler : IRequestHandler<GetQuerySuggestionsQuery, List<string>>
{
    private readonly ILogger<GetQuerySuggestionsQueryHandler> _logger;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;

    public GetQuerySuggestionsQueryHandler(
        ILogger<GetQuerySuggestionsQueryHandler> logger,
        IAIService aiService,
        ISchemaService schemaService)
    {
        _logger = logger;
        _aiService = aiService;
        _schemaService = schemaService;
    }

    public async Task<List<string>> Handle(GetQuerySuggestionsQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("💡 Generating query suggestions for user: {UserId}", request.UserId);

            var schema = await _schemaService.GetSchemaMetadataAsync();
            var suggestions = await _aiService.GenerateQuerySuggestionsAsync(
                request.Context ?? "general business queries", schema);

            _logger.LogInformation("💡 Generated {Count} suggestions for user {UserId}",
                suggestions?.Length ?? 0, request.UserId);

            return suggestions?.ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating query suggestions for user {UserId}", request.UserId);
            return new List<string>();
        }
    }
}

/// <summary>
/// Query handler for getting relevant schema for a question
/// </summary>
public class GetRelevantSchemaQueryHandler : IRequestHandler<GetRelevantSchemaQuery, SchemaMetadata>
{
    private readonly ILogger<GetRelevantSchemaQueryHandler> _logger;
    private readonly ISchemaService _schemaService;
    private readonly IContextManager? _contextManager;

    public GetRelevantSchemaQueryHandler(
        ILogger<GetRelevantSchemaQueryHandler> logger,
        ISchemaService schemaService,
        IContextManager? contextManager = null)
    {
        _logger = logger;
        _schemaService = schemaService;
        _contextManager = contextManager;
    }

    public async Task<SchemaMetadata> Handle(GetRelevantSchemaQuery request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("🔍 Getting relevant schema for question: {Question}", request.Question);

            // Get full schema first
            var fullSchema = await _schemaService.GetSchemaMetadataAsync();

            // If context manager is available, get relevant subset
            if (_contextManager != null)
            {
                var schemaContext = await _contextManager.GetRelevantSchemaAsync(request.Question, fullSchema);
                
                var relevantSchema = new SchemaMetadata
                {
                    Tables = schemaContext.RelevantTables,
                    DatabaseName = fullSchema.DatabaseName,
                    Version = fullSchema.Version,
                    LastUpdated = fullSchema.LastUpdated
                };

                _logger.LogInformation("🔍 Using relevant schema with {TableCount} tables for question",
                    relevantSchema.Tables.Count);

                return relevantSchema;
            }

            _logger.LogInformation("🔍 Using full schema with {TableCount} tables (no context manager)",
                fullSchema.Tables.Count);

            return fullSchema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting relevant schema for question: {Question}", request.Question);
            
            // Return minimal schema as fallback
            return new SchemaMetadata
            {
                Tables = new List<TableMetadata>(),
                DatabaseName = "Unknown",
                LastUpdated = DateTime.UtcNow
            };
        }
    }
}

/// <summary>
/// Query handler for validating SQL syntax
/// </summary>
public class ValidateSqlQueryHandler : IRequestHandler<ValidateSqlQuery, bool>
{
    private readonly ILogger<ValidateSqlQueryHandler> _logger;
    private readonly ISqlQueryService _sqlQueryService;

    public ValidateSqlQueryHandler(
        ILogger<ValidateSqlQueryHandler> logger,
        ISqlQueryService sqlQueryService)
    {
        _logger = logger;
        _sqlQueryService = sqlQueryService;
    }

    public async Task<bool> Handle(ValidateSqlQuery request, CancellationToken cancellationToken)
    {
        try
        {
            var isValid = await _sqlQueryService.ValidateSqlAsync(request.Sql);
            
            _logger.LogInformation("✅ SQL validation result: {IsValid} for SQL length: {Length}",
                isValid, request.Sql.Length);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating SQL");
            return false;
        }
    }
}
