using MediatR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Commands;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Extensions;
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
    private readonly IServiceProvider _serviceProvider;

    public GenerateSqlCommandHandler(
        ILogger<GenerateSqlCommandHandler> logger,
        IAIService aiService,
        IPromptService promptService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _aiService = aiService;
        _promptService = promptService;
        _serviceProvider = serviceProvider;
    }

    public async Task<GenerateSqlResponse> Handle(GenerateSqlCommand request, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogInformation("🔍 [SQL-HANDLER] GenerateSqlCommandHandler.Handle called for question: {Question}", request.Question);
            _logger.LogInformation("🤖 Generating SQL for question: {Question}", request.Question);

            // Check if enhanced prompt is available and valid
            string prompt;
            PromptDetails? promptDetails;

            if (IsEnhancedPromptValid(request))
            {
                _logger.LogInformation("🚀 [ENHANCED-PROMPT] Using enhanced business-aware prompt - Length: {Length}", request.EnhancedPrompt!.Length);
                prompt = request.EnhancedPrompt!;

                // Create prompt details for enhanced prompt
                promptDetails = new PromptDetails
                {
                    FullPrompt = prompt,
                    TemplateName = "Enhanced Business Context Template",
                    TemplateVersion = "1.0",
                    TokenCount = EstimateTokenCount(prompt),
                    GeneratedAt = DateTime.UtcNow,
                    // Enhanced properties
                    PromptLength = prompt.Length,
                    SchemaTablesCount = request.SchemaMetadata?.RelevantTables.Count ?? 0,
                    BusinessDomain = request.BusinessProfile?.Domain.Name ?? "Unknown",
                    ConfidenceScore = request.BusinessProfile?.ConfidenceScore ?? 0,
                    IsEnhancedPrompt = true,
                    EnhancementSource = "Enhanced"
                };

                _logger.LogInformation("✅ [ENHANCED-PROMPT] Enhanced prompt ready - Domain: {Domain}, Confidence: {Confidence:P2}, Tables: {TableCount}",
                    promptDetails.BusinessDomain, promptDetails.ConfidenceScore, promptDetails.SchemaTablesCount);
            }
            else
            {
                var fallbackReason = GetPromptFallbackReason(request);
                _logger.LogInformation("🔄 [BASIC-PROMPT] Using basic prompt building - Reason: {Reason}", fallbackReason);

                try
                {
                    // Build AI prompt with business context (fallback to basic)
                    prompt = await _promptService.BuildQueryPromptAsync(request.Question, request.Schema);

                    // Generate detailed prompt information for debugging
                    promptDetails = await _promptService.BuildDetailedQueryPromptAsync(request.Question, request.Schema);

                    // Enhance basic prompt details with new properties
                    if (promptDetails != null)
                    {
                        promptDetails.PromptLength = prompt?.Length ?? 0;
                        promptDetails.SchemaTablesCount = request.Schema.Tables.Count;
                        promptDetails.BusinessDomain = "Unknown";
                        promptDetails.ConfidenceScore = 0.5; // Default confidence for basic prompts
                        promptDetails.IsEnhancedPrompt = false;
                        promptDetails.EnhancementSource = "Basic";
                    }

                    _logger.LogInformation("📝 [BASIC-PROMPT] Basic prompt built - Template: {TemplateName}, Length: {Length}",
                        promptDetails?.TemplateName, prompt?.Length ?? 0);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ [BASIC-PROMPT] Failed to build basic prompt, using minimal fallback");

                    // Ultimate fallback - create minimal prompt
                    prompt = $"Generate SQL for: {request.Question}\n\nAvailable tables: {string.Join(", ", request.Schema.Tables.Select(t => t.Name))}";
                    promptDetails = new PromptDetails
                    {
                        FullPrompt = prompt,
                        TemplateName = "Minimal Fallback Template",
                        TemplateVersion = "1.0",
                        TokenCount = EstimateTokenCount(prompt),
                        GeneratedAt = DateTime.UtcNow,
                        // Enhanced properties
                        PromptLength = prompt.Length,
                        SchemaTablesCount = request.Schema.Tables.Count,
                        BusinessDomain = "Unknown",
                        ConfidenceScore = 0.3, // Low confidence for fallback
                        IsEnhancedPrompt = false,
                        EnhancementSource = "Fallback"
                    };
                }
            }

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

                var failedResponse = new GenerateSqlResponse
                {
                    Success = false,
                    Error = "AI service returned empty SQL",
                    AiExecutionTimeMs = aiExecutionTime,
                    PromptDetails = promptDetails
                };

                // Track failed template usage
                try
                {
                    await _serviceProvider.TrackTemplateUsageFromSqlResultAsync(failedResponse, request.UserId);
                }
                catch (Exception trackingEx)
                {
                    _logger.LogWarning(trackingEx, "⚠️ Failed to track failed template usage");
                }

                return failedResponse;
            }

            // Calculate confidence score
            var confidence = await _aiService.CalculateConfidenceScoreAsync(request.Question, generatedSQL, cancellationToken);

            _logger.LogInformation("✅ SQL generated successfully - Length: {Length}, Confidence: {Confidence:P2}, Time: {Time}ms",
                generatedSQL.Length, confidence, aiExecutionTime);

            var response = new GenerateSqlResponse
            {
                Sql = generatedSQL,
                Success = true,
                Confidence = confidence,
                PromptDetails = promptDetails,
                AiExecutionTimeMs = aiExecutionTime
            };

            // Track template usage for performance analytics
            try
            {
                await _serviceProvider.TrackTemplateUsageFromSqlResultAsync(response, request.UserId);
            }
            catch (Exception trackingEx)
            {
                _logger.LogWarning(trackingEx, "⚠️ Failed to track template usage, continuing with response");
            }

            return response;
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

    /// <summary>
    /// Validate if enhanced prompt is available and usable
    /// </summary>
    private bool IsEnhancedPromptValid(GenerateSqlCommand request)
    {
        if (string.IsNullOrEmpty(request.EnhancedPrompt))
        {
            return false;
        }

        if (request.EnhancedPrompt.Length < 50)
        {
            _logger.LogWarning("⚠️ [PROMPT-VALIDATION] Enhanced prompt too short: {Length} characters", request.EnhancedPrompt.Length);
            return false;
        }

        if (request.BusinessProfile == null)
        {
            _logger.LogDebug("🔍 [PROMPT-VALIDATION] BusinessProfile is null, enhanced prompt may be incomplete");
            // Still allow enhanced prompt without business profile for partial enhancement
        }

        if (request.SchemaMetadata == null)
        {
            _logger.LogDebug("🔍 [PROMPT-VALIDATION] SchemaMetadata is null, enhanced prompt may be incomplete");
            // Still allow enhanced prompt without schema metadata for partial enhancement
        }

        return true;
    }

    /// <summary>
    /// Get reason for falling back to basic prompt
    /// </summary>
    private string GetPromptFallbackReason(GenerateSqlCommand request)
    {
        if (string.IsNullOrEmpty(request.EnhancedPrompt))
            return "No enhanced prompt provided";

        if (request.EnhancedPrompt.Length < 50)
            return $"Enhanced prompt too short ({request.EnhancedPrompt.Length} chars)";

        return "Enhanced prompt validation failed";
    }

    /// <summary>
    /// Estimate token count for a given text (rough approximation)
    /// </summary>
    private int EstimateTokenCount(string text)
    {
        if (string.IsNullOrEmpty(text))
            return 0;

        // Rough estimation: 1 token ≈ 4 characters for English text
        // This is a simplified estimation; actual tokenization may vary
        return (int)Math.Ceiling(text.Length / 4.0);
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
