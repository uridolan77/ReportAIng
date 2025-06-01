using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Infrastructure.AI;
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
    private readonly ContextManager? _contextManager;

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
        ContextManager? contextManager = null) // Optional for backward compatibility
    {
        _logger = logger;
        _aiService = aiService;
        _schemaService = schemaService;
        _sqlQueryService = sqlQueryService;
        _cacheService = cacheService;
        _auditService = auditService;
        _promptService = promptService;
        _settingsService = settingsService;
        _contextManager = contextManager;
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId)
    {
        return await ProcessQueryAsync(request, userId, CancellationToken.None);
    }

    public async Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId, CancellationToken cancellationToken)
    {
        var queryId = Guid.NewGuid().ToString();
        var startTime = DateTime.UtcNow;

        try
        {
            _logger.LogError("🎯🎯🎯 ENHANCED QueryService.ProcessQueryAsync CALLED - Processing query {QueryId} for user {UserId}: {Question}",
                queryId, userId, request.Question);

            // Check cache first if enabled (both in request options AND admin settings)
            var adminCachingEnabled = await _settingsService.GetBooleanSettingAsync("EnableQueryCaching", true);
            var requestCachingEnabled = request.Options.EnableCache;
            var isCachingEnabled = requestCachingEnabled && adminCachingEnabled;

            _logger.LogError("🔍 CACHE DEBUG - Admin setting: {AdminCache}, Request setting: {RequestCache}, Final: {FinalCache}",
                adminCachingEnabled, requestCachingEnabled, isCachingEnabled);

            if (isCachingEnabled)
            {
                var cacheKey = GenerateCacheKey(request.Question);
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
            var fullSchema = await _schemaService.GetSchemaMetadataAsync();

            // Get relevant schema context based on the query
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
            var prompt = await _promptService.BuildQueryPromptAsync(request.Question, relevantSchema);
            var aiStartTime = DateTime.UtcNow;
            var generatedSQL = await _aiService.GenerateSQLAsync(prompt, cancellationToken);
            var aiExecutionTime = (int)(DateTime.UtcNow - aiStartTime).TotalMilliseconds;

            _logger.LogInformation("🤖 AI GENERATED SQL: {GeneratedSQL}", generatedSQL);

            // Validate the generated SQL
            var isValidSQL = await _sqlQueryService.ValidateSqlAsync(generatedSQL);
            if (!isValidSQL)
            {
                var error = "Generated SQL failed validation";
                _logger.LogError("❌ SQL VALIDATION FAILED for query: {Question}", request.Question);
                _logger.LogError("❌ FAILED SQL: {GeneratedSQL}", generatedSQL);
                await LogQueryAsync(userId, request, generatedSQL, false, 0, error);
                await LogPromptDetailsAsync(request.Question, prompt, generatedSQL, false, aiExecutionTime, userId, request.SessionId, error);
                return CreateErrorResponse(queryId, error, generatedSQL);
            }

            // Execute the SQL query
            _logger.LogInformation("Executing SQL: {SQL}", generatedSQL);
            var dbStartTime = DateTime.UtcNow;
            var queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(generatedSQL, request.Options, cancellationToken);
            var dbExecutionTime = (int)(DateTime.UtcNow - dbStartTime).TotalMilliseconds;
            var totalExecutionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            _logger.LogInformation("Database execution completed - Success: {Success}, RowCount: {RowCount}, ExecutionTime: {ExecutionTime}ms",
                queryResult.IsSuccessful, queryResult.Metadata?.RowCount ?? 0, dbExecutionTime);

            // Calculate confidence score
            var confidence = await _aiService.CalculateConfidenceScoreAsync(request.Question, generatedSQL);

            // Generate visualization config if requested
            VisualizationConfig? visualization = null;
            if (request.Options.IncludeVisualization && queryResult.Data?.Length > 0)
            {
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
            var suggestions = await _aiService.GenerateQuerySuggestionsAsync(request.Question, relevantSchema);

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
                ExecutionTimeMs = totalExecutionTime
            };

            // Cache the result if enabled (both in request options AND admin settings)
            if (isCachingEnabled)
            {
                var cacheKey = GenerateCacheKey(request.Question);
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

            return CreateErrorResponse(queryId, ex.Message);
        }
    }

    public async Task<List<QueryHistoryItem>> GetQueryHistoryAsync(string userId, int page = 1, int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Retrieving query history for user {UserId}, page {Page}", userId, page);

            // Get query history from audit logs
            var auditLogs = await _auditService.GetAuditLogsAsync(userId, null, null, "QUERY_EXECUTED");

            var queryHistory = auditLogs
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(log => new QueryHistoryItem
                {
                    Id = log.Id.ToString(),
                    UserId = log.UserId,
                    Question = ExtractNaturalLanguageQuery(log.Details),
                    Sql = ExtractGeneratedSQL(log.Details),
                    Timestamp = log.Timestamp,
                    ExecutionTimeMs = (int)ExtractExecutionTime(log.Details),
                    Successful = ExtractSuccessStatus(log.Details),
                    Error = ExtractErrorMessage(log.Details)
                })
                .ToList();

            return queryHistory;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving query history for user {UserId}", userId);
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

    private QueryResponse CreateErrorResponse(string queryId, string error, string? sql = null)
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
            ExecutionTimeMs = 0
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

    private string ExtractNaturalLanguageQuery(object? details)
    {
        try
        {
            if (details == null) return string.Empty;

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return string.Empty;

            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("NaturalLanguageQuery", out var queryElement))
            {
                return queryElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private string ExtractGeneratedSQL(object? details)
    {
        try
        {
            if (details == null) return string.Empty;

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return string.Empty;

            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("GeneratedSQL", out var sqlElement))
            {
                return sqlElement.GetString() ?? string.Empty;
            }

            return string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }

    private double ExtractExecutionTime(object? details)
    {
        try
        {
            if (details == null) return 0;

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return 0;

            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("ExecutionTimeMs", out var timeElement))
            {
                return timeElement.GetDouble();
            }

            return 0;
        }
        catch
        {
            return 0;
        }
    }

    private bool ExtractSuccessStatus(object? details)
    {
        try
        {
            if (details == null) return false;

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return false;

            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("Error", out var errorElement))
            {
                var error = errorElement.GetString();
                return string.IsNullOrEmpty(error);
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private string? ExtractErrorMessage(object? details)
    {
        try
        {
            if (details == null) return null;

            var detailsStr = details.ToString();
            if (string.IsNullOrEmpty(detailsStr)) return null;

            using var doc = System.Text.Json.JsonDocument.Parse(detailsStr);
            if (doc.RootElement.TryGetProperty("Error", out var errorElement))
            {
                return errorElement.GetString();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

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
