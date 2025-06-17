using MediatR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using SqlQueryResult = BIReportingCopilot.Core.Models.SqlQueryResult;

namespace BIReportingCopilot.Infrastructure.Handlers.Query;

/// <summary>
/// Command handler for generating SQL from natural language using AI
/// Focused on the AI interaction and prompt building aspects
/// </summary>
public class GenerateSqlCommandHandler : IRequestHandler<GenerateSqlCommand, GenerateSqlResponse>
{
    private readonly ILogger<GenerateSqlCommandHandler> _logger;
    private readonly IAIService _aiService;
    private readonly IPromptService _promptService;

    public GenerateSqlCommandHandler(
        ILogger<GenerateSqlCommandHandler> logger,
        IAIService aiService,
        IPromptService promptService)
    {
        _logger = logger;
        _aiService = aiService;
        _promptService = promptService;
    }

    public async Task<GenerateSqlResponse> Handle(GenerateSqlCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("🤖 Generating SQL for question: {Question}", request.Question);

            // Build AI prompt with business context
            var prompt = await _promptService.BuildQueryPromptAsync(request.Question, request.Schema);
            
            // Generate detailed prompt information for debugging
            var promptDetails = await _promptService.BuildDetailedQueryPromptAsync(request.Question, request.Schema);
            
            _logger.LogInformation("📝 Prompt built - Template: {TemplateName}, Length: {Length}", 
                promptDetails?.TemplateName, prompt?.Length ?? 0);

            // Generate SQL using AI with managed provider/model if specified
            string generatedSQL;
            if (_aiService is ILLMAwareAIService llmAwareService &&
                (!string.IsNullOrEmpty(request.ProviderId) || !string.IsNullOrEmpty(request.ModelId)))
            {
                generatedSQL = await llmAwareService.GenerateSQLWithManagedModelAsync(
                    prompt, request.ProviderId, request.ModelId, cancellationToken);
            }
            else
            {
                generatedSQL = await _aiService.GenerateSQLAsync(prompt, request.Schema, cancellationToken);
            }
            var aiExecutionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            if (string.IsNullOrWhiteSpace(generatedSQL))
            {
                _logger.LogError("❌ AI returned empty SQL for question: {Question}", request.Question);
                return new GenerateSqlResponse
                {
                    Success = false,
                    Error = "AI service returned empty SQL",
                    AiExecutionTimeMs = aiExecutionTime,
                    PromptDetails = promptDetails
                };
            }

            // Calculate confidence score
            var confidence = await _aiService.CalculateConfidenceScoreAsync(request.Question, generatedSQL);

            _logger.LogInformation("✅ SQL generated successfully - Length: {Length}, Confidence: {Confidence:P2}, Time: {Time}ms",
                generatedSQL.Length, confidence, aiExecutionTime);

            return new GenerateSqlResponse
            {
                Sql = generatedSQL,
                Success = true,
                Confidence = confidence,
                PromptDetails = promptDetails,
                AiExecutionTimeMs = aiExecutionTime
            };
        }
        catch (Exception ex)
        {
            var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "❌ Error generating SQL for question: {Question}", request.Question);

            return new GenerateSqlResponse
            {
                Success = false,
                Error = $"SQL generation failed: {ex.Message}",
                AiExecutionTimeMs = executionTime
            };
        }
    }
}

/// <summary>
/// Command handler for executing SQL queries
/// Focused on database interaction and result processing
/// </summary>
public class ExecuteSqlCommandHandler : IRequestHandler<ExecuteSqlCommand, SqlQueryResult>
{
    private readonly ILogger<ExecuteSqlCommandHandler> _logger;
    private readonly ISqlQueryService _sqlQueryService;

    public ExecuteSqlCommandHandler(
        ILogger<ExecuteSqlCommandHandler> logger,
        ISqlQueryService sqlQueryService)
    {
        _logger = logger;
        _sqlQueryService = sqlQueryService;
    }

    public async Task<SqlQueryResult> Handle(ExecuteSqlCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("🗄️ Executing SQL query - Length: {Length}", request.Sql.Length);

            var queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(request.Sql, cancellationToken);
            var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Convert QueryResult to SqlQueryResult
            var result = ConvertToSqlQueryResult(queryResult, request.Sql, executionTime);

            if (result.IsSuccessful)
            {
                _logger.LogInformation("✅ SQL executed successfully - Rows: {RowCount}, Time: {Time}ms",
                    result.RowCount, executionTime);
            }
            else
            {
                _logger.LogError("❌ SQL execution failed - Error: {Error}, Time: {Time}ms",
                    result.Error, executionTime);
            }

            return result;
        }
        catch (Exception ex)
        {
            var executionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "❌ Error executing SQL query");

            var metadata = new SqlQueryMetadata
            {
                QueryId = Guid.NewGuid().ToString(),
                QueryType = "SELECT",
                ExecutedAt = DateTime.UtcNow,
                UserId = "system",
                Error = $"SQL execution error: {ex.Message}",
                ExecutionTimeMs = executionTime
            };

            return new SqlQueryResult
            {
                Success = false,
                Error = $"SQL execution error: {ex.Message}",
                ExecutionTimeMs = executionTime,
                Metadata = ConvertSqlQueryMetadataToDict(metadata)
            };
        }
    }

    /// <summary>
    /// Convert QueryResult to SqlQueryResult for compatibility
    /// </summary>
    private static SqlQueryResult ConvertToSqlQueryResult(QueryResult queryResult, string sql, int executionTime)
    {
        var result = new SqlQueryResult
        {
            Success = queryResult.IsSuccessful,
            Error = queryResult.Metadata?.Error,
            RowCount = queryResult.Metadata?.RowCount ?? 0,
            ExecutionTimeMs = executionTime,
            ExecutedSql = sql
        };

        // Convert data format from object[] to List<Dictionary<string, object>>
        if (queryResult.Data != null)
        {
            result.Data = ConvertDataFormat(queryResult.Data, queryResult.Metadata?.Columns);
        }

        return result;
    }

    /// <summary>
    /// Convert data from object[] format to List<Dictionary<string, object>> format
    /// </summary>
    private static List<Dictionary<string, object>> ConvertDataFormat(object[] data, ColumnMetadata[]? columns)
    {
        var convertedData = new List<Dictionary<string, object>>();

        if (data == null || columns == null)
            return convertedData;

        foreach (var item in data)
        {
            if (item is Dictionary<string, object> dict)
            {
                convertedData.Add(dict);
            }
            else if (item is IDictionary<string, object> idict)
            {
                convertedData.Add(new Dictionary<string, object>(idict));
            }
            else
            {
                // Handle other object types by converting to dictionary using reflection
                var properties = item.GetType().GetProperties();
                var row = new Dictionary<string, object>();
                foreach (var prop in properties)
                {
                    row[prop.Name] = prop.GetValue(item) ?? DBNull.Value;
                }
                convertedData.Add(row);
            }
        }

        return convertedData;
    }

    /// <summary>
    /// Convert SqlQueryMetadata to Dictionary<string, object> for compatibility
    /// </summary>
    private static Dictionary<string, object> ConvertSqlQueryMetadataToDict(SqlQueryMetadata metadata)
    {
        return new Dictionary<string, object>
        {
            ["QueryId"] = metadata.QueryId,
            ["QueryType"] = metadata.QueryType,
            ["TablesAccessed"] = metadata.TablesAccessed,
            ["ParameterCount"] = metadata.ParameterCount,
            ["ComplexityScore"] = metadata.ComplexityScore,
            ["ExecutedAt"] = metadata.ExecutedAt,
            ["UserId"] = metadata.UserId,
            ["Error"] = metadata.Error ?? string.Empty,
            ["ExecutionTimeMs"] = metadata.ExecutionTimeMs,
            ["AdditionalMetadata"] = metadata.AdditionalMetadata
        };
    }
}

/// <summary>
/// Command handler for caching query results
/// </summary>
public class CacheQueryCommandHandler : IRequestHandler<CacheQueryCommand, bool>
{
    private readonly ILogger<CacheQueryCommandHandler> _logger;
    private readonly IQueryCacheService _queryCacheService;

    public CacheQueryCommandHandler(
        ILogger<CacheQueryCommandHandler> logger,
        IQueryCacheService queryCacheService)
    {
        _logger = logger;
        _queryCacheService = queryCacheService;
    }

    public async Task<bool> Handle(CacheQueryCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _queryCacheService.CacheQueryAsync(request.QueryHash, request.Response, request.Expiry);
            _logger.LogInformation("💾 Query cached successfully - Hash: {Hash}", request.QueryHash);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error caching query - Hash: {Hash}", request.QueryHash);
            return false;
        }
    }
}

/// <summary>
/// Command handler for invalidating query cache
/// </summary>
public class InvalidateQueryCacheCommandHandler : IRequestHandler<InvalidateQueryCacheCommand, bool>
{
    private readonly ILogger<InvalidateQueryCacheCommandHandler> _logger;
    private readonly IQueryCacheService _queryCacheService;

    public InvalidateQueryCacheCommandHandler(
        ILogger<InvalidateQueryCacheCommandHandler> logger,
        IQueryCacheService queryCacheService)
    {
        _logger = logger;
        _queryCacheService = queryCacheService;
    }

    public async Task<bool> Handle(InvalidateQueryCacheCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await _queryCacheService.InvalidateByPatternAsync(request.Pattern);
            _logger.LogInformation("🧹 Cache invalidated - Pattern: {Pattern}", request.Pattern);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error invalidating cache - Pattern: {Pattern}", request.Pattern);
            return false;
        }
    }
}

/// <summary>
/// Command handler for submitting query feedback
/// </summary>
public class SubmitQueryFeedbackCommandHandler : IRequestHandler<SubmitQueryFeedbackCommand, bool>
{
    private readonly ILogger<SubmitQueryFeedbackCommandHandler> _logger;
    private readonly IAuditService _auditService;

    public SubmitQueryFeedbackCommandHandler(
        ILogger<SubmitQueryFeedbackCommandHandler> logger,
        IAuditService auditService)
    {
        _logger = logger;
        _auditService = auditService;
    }

    public async Task<bool> Handle(SubmitQueryFeedbackCommand request, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("📝 Submitting feedback for query {QueryId} from user {UserId}",
                request.Feedback.QueryId, request.UserId);

            await _auditService.LogAsync("QUERY_FEEDBACK", request.UserId, "QueryFeedback", 
                request.Feedback.QueryId, request.Feedback);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error submitting feedback for query {QueryId}", request.Feedback.QueryId);
            return false;
        }
    }
}
