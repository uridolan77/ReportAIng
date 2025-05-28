using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.Services;

public class QueryService : IQueryService
{
    private readonly ILogger<QueryService> _logger;
    private readonly IOpenAIService _openAIService;
    private readonly ISchemaService _schemaService;
    private readonly ISqlQueryService _sqlQueryService;
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;
    private readonly IPromptService _promptService;
    private readonly IAITuningSettingsService _settingsService;

    public QueryService(
        ILogger<QueryService> logger,
        IOpenAIService openAIService,
        ISchemaService schemaService,
        ISqlQueryService sqlQueryService,
        ICacheService cacheService,
        IAuditService auditService,
        IPromptService promptService,
        IAITuningSettingsService settingsService)
    {
        _logger = logger;
        _openAIService = openAIService;
        _schemaService = schemaService;
        _sqlQueryService = sqlQueryService;
        _cacheService = cacheService;
        _auditService = auditService;
        _promptService = promptService;
        _settingsService = settingsService;
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
            _logger.LogInformation("Processing query {QueryId} for user {UserId}: {Question}",
                queryId, userId, request.Question);

            // Check cache first if enabled (both in request options AND admin settings)
            var isCachingEnabled = request.Options.EnableCache && await _settingsService.GetBooleanSettingAsync("EnableQueryCaching", true);
            if (isCachingEnabled)
            {
                var cacheKey = GenerateCacheKey(request.Question);
                var cachedResult = await GetCachedQueryAsync(cacheKey);
                if (cachedResult != null)
                {
                    _logger.LogInformation("Returning cached result for query {QueryId}", queryId);
                    cachedResult.QueryId = queryId;
                    cachedResult.Cached = true;
                    return cachedResult;
                }
            }
            else if (!await _settingsService.GetBooleanSettingAsync("EnableQueryCaching", true))
            {
                _logger.LogInformation("Query caching disabled by admin settings for query {QueryId}", queryId);
            }

            // Get schema metadata
            var schema = await _schemaService.GetSchemaMetadataAsync();

            // Generate SQL using enhanced AI prompt with business context
            var prompt = await _promptService.BuildQueryPromptAsync(request.Question, schema);
            var aiStartTime = DateTime.UtcNow;
            var generatedSQL = await _openAIService.GenerateSQLAsync(prompt, cancellationToken);
            var aiExecutionTime = (int)(DateTime.UtcNow - aiStartTime).TotalMilliseconds;

            // Validate the generated SQL
            var isValidSQL = await _sqlQueryService.ValidateSqlAsync(generatedSQL);
            if (!isValidSQL)
            {
                var error = "Generated SQL failed validation";
                await LogQueryAsync(userId, request, generatedSQL, false, 0, error);
                await LogPromptDetailsAsync(request.Question, prompt, generatedSQL, false, aiExecutionTime, userId, request.SessionId, error);
                return CreateErrorResponse(queryId, error, generatedSQL);
            }

            // Execute the SQL query
            var queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(generatedSQL, request.Options, cancellationToken);
            var totalExecutionTime = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            // Calculate confidence score
            var confidence = await _openAIService.CalculateConfidenceScoreAsync(request.Question, generatedSQL);

            // Generate visualization config if requested
            VisualizationConfig? visualization = null;
            if (request.Options.IncludeVisualization && queryResult.Data?.Length > 0)
            {
                var vizConfigJson = await _openAIService.GenerateVisualizationConfigAsync(
                    request.Question, queryResult.Metadata.Columns, queryResult.Data);

                // Parse visualization config (simplified for now)
                visualization = new VisualizationConfig
                {
                    Type = "bar", // Default to bar chart
                    Config = new Dictionary<string, object>()
                };
            }

            // Generate suggestions
            var suggestions = await _openAIService.GenerateQuerySuggestionsAsync(request.Question, schema);

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
            }

            // Log the successful query
            await LogQueryAsync(userId, request, generatedSQL, true, totalExecutionTime);

            // Log prompt details for admin debugging
            await LogPromptDetailsAsync(request.Question, prompt, generatedSQL, true, totalExecutionTime, userId, request.SessionId);

            _logger.LogInformation("Successfully processed query {QueryId} in {ExecutionTime}ms",
                queryId, totalExecutionTime);

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
            var suggestions = await _openAIService.GenerateQuerySuggestionsAsync(context ?? "general business queries", schema);
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
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(question.ToLowerInvariant()));
        return Convert.ToHexString(hash);
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
}
