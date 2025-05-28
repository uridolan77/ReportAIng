using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

public interface IQueryService
{
    Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId);
    Task<QueryResponse> ProcessQueryAsync(QueryRequest request, string userId, CancellationToken cancellationToken);
    Task<List<QueryHistoryItem>> GetQueryHistoryAsync(string userId, int page = 1, int pageSize = 20);
    Task<bool> SubmitFeedbackAsync(QueryFeedback feedback, string userId);
    Task<List<string>> GetQuerySuggestionsAsync(string userId, string? context = null);
    Task<QueryResponse?> GetCachedQueryAsync(string queryHash);
    Task CacheQueryAsync(string queryHash, QueryResponse response, TimeSpan? expiry = null);
    Task InvalidateQueryCacheAsync(string pattern);
}

public interface IOpenAIService
{
    Task<string> GenerateSQLAsync(string prompt);
    Task<string> GenerateSQLAsync(string prompt, CancellationToken cancellationToken);
    Task<string> GenerateInsightAsync(string query, object[] data);
    Task<string> GenerateVisualizationConfigAsync(string query, ColumnInfo[] columns, object[] data);
    Task<double> CalculateConfidenceScoreAsync(string naturalLanguageQuery, string generatedSQL);
    Task<string[]> GenerateQuerySuggestionsAsync(string context, SchemaMetadata schema);
    Task<bool> ValidateQueryIntentAsync(string naturalLanguageQuery);
}

// New advanced AI/ML interfaces
public interface ISemanticAnalyzer
{
    Task<SemanticAnalysis> AnalyzeAsync(string naturalLanguageQuery);
    Task<float[]> GenerateEmbeddingAsync(string text);
    Task<SemanticSimilarity> CalculateSimilarityAsync(string query1, string query2);
    Task<List<SemanticEntity>> ExtractEntitiesAsync(string query);
    Task<QueryIntent> ClassifyIntentAsync(string query);
}

public interface IQueryClassifier
{
    Task<QueryClassification> ClassifyQueryAsync(string query);
    Task<QueryComplexityScore> AnalyzeComplexityAsync(string query);
    Task<List<string>> SuggestOptimizationsAsync(string query);
    Task<bool> RequiresJoinAsync(string query, SchemaMetadata schema);
}

public interface IContextManager
{
    Task<UserContext> GetUserContextAsync(string userId);
    Task UpdateUserContextAsync(string userId, QueryRequest request, QueryResponse response);
    Task<List<QueryPattern>> GetQueryPatternsAsync(string userId);
    Task<SchemaContext> GetRelevantSchemaAsync(string query, SchemaMetadata fullSchema);
}

public interface IQueryOptimizer
{
    Task<OptimizedQuery> OptimizeAsync(List<SqlCandidate> candidates, UserContext context);
    Task<List<SqlCandidate>> GenerateCandidatesAsync(SemanticAnalysis analysis, SchemaContext schema);
    Task<QueryPerformancePrediction> PredictPerformanceAsync(string sql, SchemaMetadata schema);
    Task<string> RewriteForPerformanceAsync(string sql);
}

public interface ISchemaService
{
    Task<SchemaMetadata> GetSchemaMetadataAsync(string? dataSource = null);
    Task<SchemaMetadata> RefreshSchemaMetadataAsync(string? dataSource = null);
    Task<string> GetSchemaSummaryAsync(string? dataSource = null);
    Task<TableMetadata?> GetTableMetadataAsync(string tableName, string? schema = null);
    Task<List<SchemaSuggestion>> GetSchemaSuggestionsAsync(string userId);
    Task<bool> ValidateTableAccessAsync(string tableName, string userId);
    Task<List<string>> GetAccessibleTablesAsync(string userId);
    Task UpdateTableStatisticsAsync(string tableName);
    Task<DataQualityScore> AssessDataQualityAsync(string tableName);
}

public interface ISqlQueryService
{
    Task<QueryResult> ExecuteSelectQueryAsync(string sql, QueryOptions? options = null);
    Task<QueryResult> ExecuteSelectQueryAsync(string sql, QueryOptions? options, CancellationToken cancellationToken);
    Task<bool> ValidateSqlAsync(string sql);
    Task<string> OptimizeSqlAsync(string sql);
    Task<QueryExecutionPlan> GetExecutionPlanAsync(string sql);
    Task<QueryPerformanceMetrics> GetQueryPerformanceAsync(string sql);
    Task<bool> TestConnectionAsync(string? dataSource = null);
    Task<List<string>> GetAvailableDataSourcesAsync();
}

public interface IPromptService
{
    Task<string> BuildQueryPromptAsync(string naturalLanguageQuery, SchemaMetadata schema, string? context = null);
    Task<string> BuildInsightPromptAsync(string query, QueryResult result);
    Task<string> BuildVisualizationPromptAsync(string query, QueryResult result);
    Task<PromptTemplate> GetPromptTemplateAsync(string templateName, string? version = null);
    Task<PromptTemplate> CreatePromptTemplateAsync(PromptTemplate template);
    Task<PromptTemplate> UpdatePromptTemplateAsync(PromptTemplate template);
    Task<List<PromptTemplate>> GetPromptTemplatesAsync();
    Task<PromptPerformanceMetrics> GetPromptPerformanceAsync(string templateName);
}

public interface ITuningService
{
    // Dashboard
    Task<TuningDashboardData> GetDashboardDataAsync();

    // Business Table Info
    Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync();
    Task<BusinessTableInfoDto?> GetBusinessTableAsync(long id);
    Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId);
    Task<BusinessTableInfoDto?> UpdateBusinessTableAsync(long id, CreateTableInfoRequest request, string userId);
    Task<bool> DeleteBusinessTableAsync(long id);

    // Query Patterns
    Task<List<QueryPatternDto>> GetQueryPatternsAsync();
    Task<QueryPatternDto?> GetQueryPatternAsync(long id);
    Task<QueryPatternDto> CreateQueryPatternAsync(CreateQueryPatternRequest request, string userId);
    Task<QueryPatternDto?> UpdateQueryPatternAsync(long id, CreateQueryPatternRequest request, string userId);
    Task<bool> DeleteQueryPatternAsync(long id);
    Task<string> TestQueryPatternAsync(long id, string naturalLanguageQuery);

    // Business Glossary
    Task<List<BusinessGlossaryDto>> GetGlossaryTermsAsync();
    Task<BusinessGlossaryDto> CreateGlossaryTermAsync(BusinessGlossaryDto request, string userId);
    Task<BusinessGlossaryDto?> UpdateGlossaryTermAsync(long id, BusinessGlossaryDto request, string userId);
    Task<bool> DeleteGlossaryTermAsync(long id);

    // AI Settings
    Task<List<AITuningSettingsDto>> GetAISettingsAsync();
    Task<AITuningSettingsDto?> UpdateAISettingAsync(long id, AITuningSettingsDto request, string userId);
}

public interface ICacheService
{
    Task<T?> GetAsync<T>(string key) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
    Task<long> IncrementAsync(string key, long value = 1);
    Task<TimeSpan?> GetTtlAsync(string key);
    Task<bool> SetIfNotExistsAsync<T>(string key, T value, TimeSpan? expiry = null) where T : class;
}

public interface IUserService
{
    Task<UserInfo?> GetUserAsync(string userId);
    Task<UserInfo> UpdateUserPreferencesAsync(string userId, UserPreferences preferences);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<bool> HasPermissionAsync(string userId, string permission);
    Task<UserSession> CreateSessionAsync(string userId, string? ipAddress = null, string? userAgent = null);
    Task<UserSession?> GetSessionAsync(string sessionId);
    Task UpdateSessionActivityAsync(string sessionId);
    Task EndSessionAsync(string sessionId);
    Task LogUserActivityAsync(UserActivity activity);
    Task<List<UserActivity>> GetUserActivityAsync(string userId, DateTime? from = null, DateTime? to = null);
    Task<int> GetTotalActiveUsersAsync();
}

public interface IAuditService
{
    Task LogAsync(string action, string userId, string? entityType = null, string? entityId = null,
                  object? details = null, string? ipAddress = null, string? userAgent = null);
    Task LogQueryAsync(string userId, string sessionId, string naturalLanguageQuery, string generatedSQL,
                       bool successful, int executionTimeMs, string? error = null);
    Task LogSecurityEventAsync(string eventType, string userId, string? details = null,
                               string? ipAddress = null, string severity = "Info");
    Task<List<AuditLogEntry>> GetAuditLogsAsync(string? userId = null, DateTime? from = null,
                                                DateTime? to = null, string? action = null);
    Task<SecurityReport> GenerateSecurityReportAsync(DateTime from, DateTime to);
    Task<UsageReport> GenerateUsageReportAsync(DateTime from, DateTime to);
}


