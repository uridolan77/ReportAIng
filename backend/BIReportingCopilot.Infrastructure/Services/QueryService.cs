using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Core.Services;
using BIReportingCopilot.Infrastructure.AI;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using CoreModels = BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Services;

public class QueryService : IQueryService
{
    private readonly ILogger<QueryService> _logger;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;
    private readonly ISqlQueryService _sqlQueryService;
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;
    private readonly IPromptService _promptService;
    private readonly IAITuningSettingsService _settingsService;
    private readonly IContextManager? _contextManager;
    private readonly BICopilotContext _context;
    private readonly IHubContext<QueryStatusHub> _hubContext;

    // Legacy support removed - using IAIService instead

    public QueryService(
        ILogger<QueryService> logger,
        IAIService aiService,
        ISchemaService schemaService,
        ISqlQueryService sqlQueryService,
        ICacheService cacheService,
        IAuditService auditService,
        IPromptService promptService,
        IAITuningSettingsService settingsService,
        BICopilotContext context,
        IHubContext<QueryStatusHub> hubContext,
        IContextManager? contextManager = null) // Optional for backward compatibility
    {
        _logger = logger;
        _aiService = aiService;
        _schemaService = schemaService;
        _sqlQueryService = sqlQueryService;
        _cacheService = cacheService;
        _auditService = auditService;
        _promptService = promptService;
        _settingsService = settingsService;
        _context = context;
        _contextManager = contextManager;
        _hubContext = hubContext;
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId)
    {
        return await ProcessQueryAsync(request, userId, CancellationToken.None);
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId, CancellationToken cancellationToken)
    {
        var queryId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;
        PromptDetails? promptDetails = null; // Declare at method level

        try
        {
            _logger.LogError("🎯🎯🎯 ENHANCED QueryService.ProcessQueryAsync CALLED - Processing query {QueryId} for user {UserId}: {Question}",
                queryId, userId, request.Question);

            // Notify query processing started
            await NotifyProcessingStage(userId, queryId, "started", "Query processing initiated", 0);

            // Check cache first if enabled (both in request options AND admin settings)
            await NotifyProcessingStage(userId, queryId, "cache_check", "Checking query cache", 5);
            var adminCachingEnabled = await _settingsService.GetBooleanSettingAsync("EnableQueryCaching", true);
            var requestCachingEnabled = request.Options.EnableCache;
            var isCachingEnabled = requestCachingEnabled && adminCachingEnabled;

            _logger.LogError("🔍 CACHE DEBUG - Admin setting: {AdminCache}, Request setting: {RequestCache}, Final: {FinalCache}",
                adminCachingEnabled, requestCachingEnabled, isCachingEnabled);

            // FORCE CACHE BYPASS - Clear any stale cache entries for this query when caching is disabled
            var cacheKey = GenerateCacheKey(request.Question);
            if (!isCachingEnabled)
            {
                _logger.LogError("🧹 CACHE DISABLED - Clearing any stale cache for query: {CacheKey}", cacheKey);
                await _cacheService.RemoveAsync($"query:{cacheKey}");
            }

            if (isCachingEnabled)
            {
                _logger.LogError("🔑 CACHE KEY DEBUG - Question: '{Question}' -> Key: {CacheKey}", request.Question, cacheKey);
                var cachedResult = await GetCachedQueryAsync(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogError("🎯 CACHE HIT - Returning cached result for query {QueryId} with key {CacheKey}", queryId, cacheKey);
                    _logger.LogError("🎯 CACHED QUERY WAS: '{CachedQuestion}' -> SQL: {CachedSQL}", cachedResult.Sql?.Substring(0, Math.Min(100, cachedResult.Sql?.Length ?? 0)) + "...", cachedResult.Sql?.Substring(0, Math.Min(200, cachedResult.Sql?.Length ?? 0)) + "...");
                    cachedResult.QueryId = queryId;
                    cachedResult.Cached = true;
                    return cachedResult;
                }
                else
                {
                    _logger.LogError("🎯 CACHE MISS - No cached result found for query {QueryId} with key {CacheKey}", queryId, cacheKey);
                }
            }
            else
            {
                _logger.LogError("🚫 CACHE DISABLED - Query caching disabled for query {QueryId} (Admin: {AdminCache}, Request: {RequestCache})",
                    queryId, adminCachingEnabled, requestCachingEnabled);
            }

            // Get full schema metadata first
            await NotifyProcessingStage(userId, queryId, "schema_loading", "Loading database schema", 10);
            var fullSchema = await _schemaService.GetSchemaMetadataAsync();

            // Get relevant schema context based on the query
            await NotifyProcessingStage(userId, queryId, "schema_analysis", "Analyzing relevant schema for query", 15);
            SchemaMetadata relevantSchema;


            try
            {
                if (_contextManager != null)
                {
                    _logger.LogInformation("QueryService: Calling ContextManager.GetRelevantSchemaAsync");
                    var schemaContext = await _contextManager.GetRelevantSchemaAsync(request.Question, fullSchema);

                    relevantSchema = new SchemaMetadata
                    {
                        Tables = schemaContext.RelevantTables,
                        DatabaseName = fullSchema.DatabaseName,
                        Version = fullSchema.Version,
                        LastUpdated = fullSchema.LastUpdated
                    };

                    _logger.LogInformation("QueryService: Using relevant schema with {TableCount} tables for query: {Query}. Tables: {TableNames}",
                        relevantSchema.Tables.Count, request.Question, string.Join(", ", relevantSchema.Tables.Select(t => t.Name)));
                }
                else
                {
                    _logger.LogWarning("QueryService: ContextManager not available, using full schema with {TableCount} tables", fullSchema.Tables.Count);
                    relevantSchema = fullSchema;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "QueryService: Error getting relevant schema, falling back to full schema");
                relevantSchema = fullSchema;
            }

            // Generate SQL using enhanced AI prompt with relevant business context
            await NotifyProcessingStage(userId, queryId, "prompt_building", "Building AI prompt with business context", 25);
            var prompt = await _promptService.BuildQueryPromptAsync(request.Question, relevantSchema);

            // Debug: Generate prompt details with detailed logging
            await NotifyProcessingStage(userId, queryId, "prompt_details", "Generating detailed prompt information", 30);
            _logger.LogInformation("🔍 Generating prompt details for query: {Question}", request.Question);
            promptDetails = await _promptService.BuildDetailedQueryPromptAsync(request.Question, relevantSchema);
            _logger.LogInformation("📝 Prompt details generated: {HasPromptDetails}, Template: {TemplateName}, Sections: {SectionCount}",
                promptDetails != null, promptDetails?.TemplateName, promptDetails?.Sections?.Length ?? 0);

            await NotifyProcessingStage(userId, queryId, "ai_processing", "Sending request to AI service for SQL generation", 40, new {
                prompt = prompt?.Substring(0, Math.Min(200, prompt?.Length ?? 0)) + "...",
                promptLength = prompt?.Length ?? 0,
                templateName = promptDetails?.TemplateName
            });
            var aiStartTime = DateTime.UtcNow;
            var generatedSQL = await _aiService.GenerateSQLAsync(prompt, cancellationToken);
            var aiExecutionTime = (int)(DateTime.UtcNow - aiStartTime).TotalMilliseconds;

            _logger.LogInformation("🤖 AI GENERATED SQL: {GeneratedSQL}", generatedSQL);

            await NotifyProcessingStage(userId, queryId, "ai_completed", "AI generated SQL successfully", 50, new {
                generatedSQL = generatedSQL?.Substring(0, Math.Min(300, generatedSQL?.Length ?? 0)) + "...",
                aiExecutionTime = aiExecutionTime
            });

            // Validate the generated SQL
            await NotifyProcessingStage(userId, queryId, "sql_validation", "Validating generated SQL", 55);
            var isValidSQL = await _sqlQueryService.ValidateSqlAsync(generatedSQL);
            if (!isValidSQL)
            {
                var error = "Generated SQL failed validation";
                _logger.LogError("❌ SQL VALIDATION FAILED for query: {Question}", request.Question);
                _logger.LogError("❌ FAILED SQL: {GeneratedSQL}", generatedSQL);
                await NotifyProcessingStage(userId, queryId, "validation_failed", $"SQL validation failed: {error}", 100, new {
                    error = error,
                    sql = generatedSQL
                });
                await LogQueryAsync(userId, request, generatedSQL, false, 0, error);
                await LogPromptDetailsAsync(request.Question, prompt, generatedSQL, false, aiExecutionTime, userId, request.SessionId, error);
                return CreateErrorResponse(queryId, error, generatedSQL, promptDetails);
            }

            // Execute the SQL query
            await NotifyProcessingStage(userId, queryId, "sql_execution", "Executing SQL query against database", 60, new {
                sql = generatedSQL?.Substring(0, Math.Min(200, generatedSQL?.Length ?? 0)) + "..."
            });
            _logger.LogInformation("Executing SQL: {SQL}", generatedSQL);
            var dbStartTime = DateTime.UtcNow;
            var queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(generatedSQL, request.Options, cancellationToken);
            var dbExecutionTime = (int)(DateTime.UtcNow - dbStartTime).TotalMilliseconds;
            var totalExecutionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("Database execution completed - Success: {Success}, RowCount: {RowCount}, ExecutionTime: {ExecutionTime}ms",
                queryResult.IsSuccessful, queryResult.Metadata?.RowCount ?? 0, dbExecutionTime);

            // Check if SQL execution failed
            if (!queryResult.IsSuccessful || !string.IsNullOrEmpty(queryResult.Metadata?.Error))
            {
                var sqlError = queryResult.Metadata?.Error ?? "SQL execution failed";
                _logger.LogError("❌ SQL EXECUTION FAILED: {Error}", sqlError);
                await LogQueryAsync(userId, request, generatedSQL, false, totalExecutionTime, sqlError);
                await LogPromptDetailsAsync(request.Question, prompt, generatedSQL, false, totalExecutionTime, userId, request.SessionId, sqlError);

                return new QueryResponse
                {
                    QueryId = queryId,
                    Sql = generatedSQL,
                    Success = false,
                    Error = $"SQL Error: {sqlError}",
                    Timestamp = DateTime.UtcNow,
                    ExecutionTimeMs = totalExecutionTime,
                    PromptDetails = promptDetails,
                    Confidence = 0,
                    Suggestions = Array.Empty<string>(),
                    Cached = false
                };
            }

            await NotifyProcessingStage(userId, queryId, "sql_completed", "SQL execution completed successfully", 70, new {
                rowCount = queryResult.Metadata?.RowCount ?? 0,
                dbExecutionTime = dbExecutionTime
            });

            // Calculate confidence score
            await NotifyProcessingStage(userId, queryId, "confidence_calculation", "Calculating confidence score", 75);
            var confidence = await _aiService.CalculateConfidenceScoreAsync(request.Question, generatedSQL);

            // Generate visualization config if requested
            VisualizationConfig? visualization = null;
            if (request.Options.IncludeVisualization && queryResult.Data?.Length > 0)
            {
                await NotifyProcessingStage(userId, queryId, "visualization_generation", "Generating visualization recommendations", 80);
                var vizConfigJson = await _aiService.GenerateVisualizationConfigAsync(
                    request.Question, queryResult.Metadata.Columns, queryResult.Data);

                // Parse visualization config (simplified for now)
                visualization = new VisualizationConfig
                {
                    Type = "bar", // Default to bar chart
                    Config = new Dictionary<string, object>()
                };
            }

            // Generate suggestions
            await NotifyProcessingStage(userId, queryId, "suggestions_generation", "Generating follow-up suggestions", 85);
            var suggestions = await _aiService.GenerateQuerySuggestionsAsync(request.Question, relevantSchema);
            _logger.LogInformation("💡 Generated {Count} suggestions for query: {Suggestions}",
                suggestions?.Length ?? 0, string.Join(", ", suggestions ?? Array.Empty<string>()));

            var response = new QueryResponse
            {
                QueryId = queryId,
                Sql = generatedSQL,
                Result = queryResult,
                Visualization = visualization,
                Confidence = confidence,
                Suggestions = suggestions,
                Cached = false,
                Success = true,
                Timestamp = DateTime.UtcNow,
                ExecutionTimeMs = totalExecutionTime,
                PromptDetails = promptDetails
            };

            // Debug: Log response details
            _logger.LogInformation("📤 Query response created: QueryId={QueryId}, HasPromptDetails={HasPromptDetails}, PromptTemplate={TemplateName}",
                response.QueryId, response.PromptDetails != null, response.PromptDetails?.TemplateName);

            // Cache the result if enabled (both in request options AND admin settings)
            if (isCachingEnabled)
            {
                await CacheQueryAsync(cacheKey, response, TimeSpan.FromHours(24));
                _logger.LogError("💾 CACHE STORED - Question: '{Question}' -> Key: {CacheKey} -> SQL: {SQL}",
                    request.Question, cacheKey, generatedSQL?.Substring(0, Math.Min(100, generatedSQL?.Length ?? 0)) + "...");
            }
            else
            {
                _logger.LogError("💾 CACHE NOT STORED - Caching disabled for query {QueryId}", queryId);
            }

            // Log the successful query
            await LogQueryAsync(userId, request, generatedSQL, true, totalExecutionTime);

            // Log prompt details for admin debugging
            await LogPromptDetailsAsync(request.Question, prompt, generatedSQL, true, totalExecutionTime, userId, request.SessionId);

            _logger.LogInformation("Query completed successfully - QueryId: {QueryId}, ExecutionTime: {ExecutionTime}ms, RowCount: {RowCount}",
                queryId, totalExecutionTime, response.Result?.Metadata?.RowCount ?? 0);

            return response;
        }
        catch (Exception ex)
        {
            var errorExecutionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
            _logger.LogError(ex, "Error processing query {QueryId} for user {UserId}", queryId, userId);

            await LogQueryAsync(userId, request, "", false, errorExecutionTime, ex.Message);

            return CreateErrorResponse(queryId, ex.Message, "", promptDetails);
        }
    }

    public async Task<List<QueryHistoryItem>> GetQueryHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("📋 RETRIEVING QUERY HISTORY - User: {UserId}, Page: {Page}, PageSize: {PageSize}", userId, page, pageSize);

            // Get query history directly from QueryHistory table (not audit logs)
            var queryHistoryEntities = await _context.QueryHistory
                .Where(q => q.UserId == userId)
                .OrderByDescending(q => q.QueryTimestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            _logger.LogInformation("📋 FOUND {Count} QUERY HISTORY RECORDS for user {UserId}", queryHistoryEntities.Count, userId);

            var queryHistory = queryHistoryEntities
                .Select(entity => new QueryHistoryItem
                {
                    Id = entity.Id.ToString(),
                    UserId = entity.UserId,
                    Question = entity.NaturalLanguageQuery,
                    Sql = entity.GeneratedSQL,
                    Timestamp = entity.QueryTimestamp,
                    ExecutionTimeMs = entity.ExecutionTimeMs ?? 0,
                    Successful = entity.IsSuccessful,
                    Error = entity.ErrorMessage
                })
                .ToList();

            _logger.LogInformation("📋 RETURNING {Count} QUERY HISTORY ITEMS for user {UserId}", queryHistory.Count, userId);
            return queryHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ ERROR retrieving query history for user {UserId}", userId);
            return new List<QueryHistoryItem>();
        }
    }

    public async Task<bool> SubmitFeedbackAsync(QueryFeedback feedback, string userId)
    {
        try
        {
            _logger.LogInformation("Submitting feedback for query {QueryId} from user {UserId}: {Feedback}",
                feedback.QueryId, userId, feedback.Feedback);

            // TODO: Store feedback in database
            await _auditService.LogAsync("QUERY_FEEDBACK", userId, "QueryFeedback", feedback.QueryId, feedback);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting feedback for query {QueryId}", feedback.QueryId);
            return false;
        }
    }

    public async Task<List<string>> GetQuerySuggestionsAsync(string userId, string? context = null)
    {
        try
        {
            var schema = await _schemaService.GetSchemaMetadataAsync();
            var suggestions = await _aiService.GenerateQuerySuggestionsAsync(context ?? "general business queries", schema);
            return suggestions.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query suggestions for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<QueryResponse?> GetCachedQueryAsync(string queryHash)
    {
        try
        {
            return await _cacheService.GetAsync<QueryResponse>($"query:{queryHash}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving cached query {QueryHash}", queryHash);
            return null;
        }
    }

    public async Task CacheQueryAsync(string queryHash, QueryResponse response, TimeSpan? expiry = null)
    {
        try
        {
            await _cacheService.SetAsync($"query:{queryHash}", response, expiry ?? TimeSpan.FromHours(24));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching query {QueryHash}", queryHash);
        }
    }

    public async Task InvalidateQueryCacheAsync(string pattern)
    {
        try
        {
            await _cacheService.RemovePatternAsync($"query:{pattern}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating query cache pattern {Pattern}", pattern);
        }
    }

    private string GenerateCacheKey(string question)
    {
        // ROBUST cache key generation with semantic fingerprinting to prevent collisions
        var normalizedQuestion = question.Trim().ToLowerInvariant()
            .Replace("  ", " ") // Remove double spaces
            .Replace("\r\n", " ")
            .Replace("\n", " ")
            .Replace("\t", " ");

        // Extract key semantic elements for better differentiation
        var queryType = DetermineQueryType(normalizedQuestion);
        var keyWords = ExtractKeyWords(normalizedQuestion);
        var questionStructure = AnalyzeQuestionStructure(normalizedQuestion);

        // Add current date for time-sensitive queries
        var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

        // Create comprehensive cache key with multiple differentiators
        var cacheKeyData = $"q:{normalizedQuestion}|type:{queryType}|keywords:{keyWords}|structure:{questionStructure}|date:{currentDate}|len:{question.Length}";

        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(cacheKeyData));
        var hexHash = Convert.ToHexString(hash);

        _logger.LogError("🔑 ENHANCED CACHE KEY - Question: '{Question}' -> Type: {QueryType} -> Keywords: {Keywords} -> Hash: {Hash}",
            question, queryType, keyWords, hexHash);

        return hexHash;
    }

    private string DetermineQueryType(string question)
    {
        var lowerQuestion = question.ToLowerInvariant();

        if (lowerQuestion.Contains("count") && (lowerQuestion.Contains("player") || lowerQuestion.Contains("user")))
            return "player_count";
        if (lowerQuestion.Contains("top") && lowerQuestion.Contains("player"))
            return "top_players";
        if (lowerQuestion.Contains("deposit"))
            return "deposits";
        if (lowerQuestion.Contains("revenue") || lowerQuestion.Contains("income"))
            return "revenue";
        if (lowerQuestion.Contains("bonus"))
            return "bonus";
        if (lowerQuestion.Contains("yesterday"))
            return "yesterday";
        if (lowerQuestion.Contains("last") && (lowerQuestion.Contains("day") || lowerQuestion.Contains("week")))
            return "recent_period";

        return "general";
    }

    private string ExtractKeyWords(string question)
    {
        var words = question.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2) // Filter out short words
            .Where(w => !new[] { "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "man", "new", "now", "old", "see", "two", "way", "who", "boy", "did", "its", "let", "put", "say", "she", "too", "use" }.Contains(w))
            .OrderBy(w => w)
            .Take(5); // Take top 5 meaningful words

        return string.Join("|", words);
    }

    private string AnalyzeQuestionStructure(string question)
    {
        var structure = "";
        var lowerQuestion = question.ToLowerInvariant();

        if (lowerQuestion.StartsWith("count")) structure += "count_";
        if (lowerQuestion.StartsWith("top")) structure += "top_";
        if (lowerQuestion.StartsWith("show")) structure += "show_";
        if (lowerQuestion.Contains("yesterday")) structure += "yesterday_";
        if (lowerQuestion.Contains("last")) structure += "last_";
        if (lowerQuestion.Contains("player")) structure += "player_";
        if (lowerQuestion.Contains("deposit")) structure += "deposit_";

        return structure.TrimEnd('_');
    }

    private QueryResponse CreateErrorResponse(string queryId, string error, string? sql = null, PromptDetails? promptDetails = null)
    {
        return new QueryResponse
        {
            QueryId = queryId,
            Sql = sql ?? "",
            Success = false,
            Error = error,
            Timestamp = DateTime.UtcNow,
            Confidence = 0,
            Suggestions = Array.Empty<string>(),
            Cached = false,
            ExecutionTimeMs = 0,
            PromptDetails = promptDetails
        };
    }

    private async Task LogQueryAsync(string userId, QueryRequest request, string sql, bool successful, int executionTime, string? error = null)
    {
        try
        {
            await _auditService.LogQueryAsync(userId, request.SessionId, request.Question, sql, successful, executionTime, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging query for user {UserId}", userId);
        }
    }

    // Removed old audit log parsing methods - now reading directly from QueryHistory table

    private async Task LogPromptDetailsAsync(string userQuery, string fullPrompt, string generatedSQL, bool success, int executionTime, string? userId = null, string? sessionId = null, string? errorMessage = null)
    {
        try
        {
            // This will be handled by the PromptService, but we can add additional context here
            _logger.LogDebug("Query processed - User: {UserId}, Success: {Success}, ExecutionTime: {ExecutionTime}ms",
                userId, success, executionTime);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging prompt details for user {UserId}", userId);
        }
    }

    #region Advanced Query Processing Methods

    public async Task<ProcessedQuery> ProcessAdvancedQueryAsync(string query, string userId, CoreModels.QueryContext? context = null)
    {
        try
        {
            _logger.LogInformation("Processing advanced query for user {UserId}: {Query}", userId, query);

            var processedQuery = new ProcessedQuery
            {
                OriginalQuery = query,
                UserId = userId,
                Context = context,
                ProcessedAt = DateTime.UtcNow
            };

            // Generate SQL using AI service
            var schema = await _schemaService.GetSchemaMetadataAsync();
            processedQuery.GeneratedSQL = await _aiService.GenerateSQLAsync(query);

            // Calculate semantic similarity with previous queries
            var similarQueries = await FindSimilarQueriesAsync(query, userId, 3);
            processedQuery.SimilarQueries = similarQueries;

            return processedQuery;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing advanced query for user {UserId}: {Query}", userId, query);
            throw;
        }
    }

    public async Task<double> CalculateSemanticSimilarityAsync(string query1, string query2)
    {
        try
        {
            // Fallback to simple text similarity
            return CalculateSimpleTextSimilarity(query1, query2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating semantic similarity");
            return 0.0;
        }
    }

    public async Task<List<ProcessedQuery>> FindSimilarQueriesAsync(string query, string userId, int limit = 5)
    {
        try
        {
            // In a real implementation, this would search a database of processed queries
            // For now, return empty list
            return new List<ProcessedQuery>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries for user {UserId}", userId);
            return new List<ProcessedQuery>();
        }
    }

    private double CalculateSimpleTextSimilarity(string text1, string text2)
    {
        // Simple Jaccard similarity
        var words1 = text1.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var words2 = text2.ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return union > 0 ? (double)intersection / union : 0.0;
    }

    // Additional methods for interface compliance
    public async Task<QueryPerformanceMetrics> GetQueryPerformanceAsync(string queryHash)
    {
        try
        {
            // Basic implementation - in production, this would query performance data
            return new QueryPerformanceMetrics
            {
                ExecutionTime = TimeSpan.FromMilliseconds(100),
                RowCount = 0,
                FromCache = false
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query performance for hash: {QueryHash}", queryHash);
            throw;
        }
    }

    public async Task<bool> ValidateQueryAsync(string sql)
    {
        try
        {
            // Basic validation - check if it's a SELECT query
            var trimmedSql = sql.Trim();
            return trimmedSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating SQL: {Sql}", sql);
            return false;
        }
    }

    public async Task<List<QuerySuggestion>> GetSmartSuggestionsAsync(string userId, string? context = null)
    {
        try
        {
            // Basic implementation - return some default suggestions
            return new List<QuerySuggestion>
            {
                new QuerySuggestion { Text = "Show me all data", Confidence = 0.8 },
                new QuerySuggestion { Text = "Count total records", Confidence = 0.7 },
                new QuerySuggestion { Text = "Show recent data", Confidence = 0.6 }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting smart suggestions for user: {UserId}", userId);
            return new List<QuerySuggestion>();
        }
    }

    public async Task<QueryOptimizationResult> OptimizeQueryAsync(string sql)
    {
        try
        {
            // Basic implementation - return the original SQL with minimal optimization
            return new QueryOptimizationResult
            {
                OriginalQuery = sql,
                OptimizedQuery = sql,
                ImprovementScore = 0.0,
                Suggestions = new List<OptimizationSuggestion>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing SQL: {Sql}", sql);
            throw;
        }
    }

    #endregion
}
