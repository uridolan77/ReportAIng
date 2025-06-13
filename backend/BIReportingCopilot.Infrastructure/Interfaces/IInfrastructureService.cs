using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Interfaces.Monitoring;
using BIReportingCopilot.Core.Interfaces.Messaging;
using BIReportingCopilot.Core.Interfaces.Streaming;
using BIReportingCopilot.Core.Interfaces.Visualization;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Interfaces;

/// <summary>
/// Base interface for infrastructure services
/// </summary>
public interface IInfrastructureService
{
    /// <summary>
    /// Initialize the service
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Get service health status
    /// </summary>
    Task<bool> IsHealthyAsync();

    /// <summary>
    /// Cleanup resources
    /// </summary>
    Task CleanupAsync();
}

/// <summary>
/// Interface for repository pattern
/// </summary>
public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(object id);
    Task<IEnumerable<T>> GetAllAsync();
    Task<T> AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(object id);
    Task<bool> ExistsAsync(object id);
}

/// <summary>
/// Interface for unit of work pattern
/// </summary>
public interface IUnitOfWork : IDisposable
{
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

/// <summary>
/// Interface for caching services
/// </summary>
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task<bool> ExistsAsync(string key);
    Task ClearAsync();
}

/// <summary>
/// Query cache service interface
/// </summary>
public interface IQueryCacheService
{
    Task<QueryResult?> GetCachedQueryResultAsync(string queryHash, CancellationToken cancellationToken = default);
    Task SaveQueryResultAsync(string queryHash, QueryResult result, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task InvalidateQueryCacheAsync(string pattern, CancellationToken cancellationToken = default);
    Task<Core.Interfaces.Query.CacheStatistics> GetCacheStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// User service interface
/// </summary>
public interface IUserService
{
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> ValidateCredentialsAsync(string username, string password);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<User> UpdateUserAsync(User user);
}

/// <summary>
/// User repository interface
/// </summary>
public interface IUserRepository
{
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> ValidateCredentialsAsync(string username, string password);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<User> UpdateUserAsync(User user);
}

/// <summary>
/// Token repository interface
/// </summary>
public interface ITokenRepository
{
    Task StoreRefreshTokenAsync(string userId, string token, DateTime expiresAt);
    Task<RefreshTokenInfo?> GetRefreshTokenAsync(string token);
    Task RevokeRefreshTokenAsync(string token);
    Task<string?> GetUserIdFromRefreshTokenAsync(string token);
}



/// <summary>
/// Interface for background job services
/// </summary>
public interface IBackgroundJobService
{
    Task ScheduleJobAsync(string jobName, Func<Task> job, TimeSpan delay);
    Task ScheduleRecurringJobAsync(string jobName, Func<Task> job, string cronExpression);
    Task CancelJobAsync(string jobName);
    Task<bool> IsJobRunningAsync(string jobName);
}

/// <summary>
/// Interface for notification services
/// </summary>
public interface INotificationService
{
    Task SendNotificationAsync(string userId, string title, string message, Dictionary<string, object>? metadata = null);
    Task SendBroadcastAsync(string title, string message, Dictionary<string, object>? metadata = null);
    Task<IEnumerable<object>> GetNotificationsAsync(string userId, int skip = 0, int take = 50);
    Task MarkAsReadAsync(string userId, string notificationId);
}

/// <summary>
/// Interface for audit logging
/// </summary>
public interface IAuditService
{
    Task LogActionAsync(string userId, string action, string entityType, string entityId, Dictionary<string, object>? metadata = null);
    Task LogErrorAsync(string userId, string error, string? stackTrace = null, Dictionary<string, object>? metadata = null);
    Task<IEnumerable<object>> GetAuditLogsAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
}

// =============================================================================
// MISSING INTERFACE ALIASES FOR INFRASTRUCTURE COMPATIBILITY
// =============================================================================

/// <summary>
/// Schema service interface
/// </summary>
public interface ISchemaService
{
    Task<SchemaMetadata> GetSchemaAsync(CancellationToken cancellationToken = default);
    Task RefreshSchemaAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetTableNamesAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetColumnNamesAsync(string tableName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Schema management service interface
/// </summary>
public interface ISchemaManagementService
{
    Task<SchemaMetadata> GetSchemaMetadataAsync(CancellationToken cancellationToken = default);
    Task<List<TableMetadata>> GetTablesAsync(CancellationToken cancellationToken = default);
    Task<TableMetadata?> GetTableMetadataAsync(string tableName, CancellationToken cancellationToken = default);
    Task RefreshSchemaAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetDatabasesAsync(CancellationToken cancellationToken = default);
}



/// <summary>
/// Email service interface
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
    Task SendEmailAsync(List<string> to, string subject, string body, CancellationToken cancellationToken = default);
}

/// <summary>
/// SMS service interface
/// </summary>
public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);
}

/// <summary>
/// Tuning service interface
/// </summary>
public interface ITuningService
{
    Task<TuningResult> OptimizeAsync(TuningRequest request, CancellationToken cancellationToken = default);
    Task<TuningStatus> GetStatusAsync(string tuningId, CancellationToken cancellationToken = default);
    Task<TuningResult> TuneQueryAsync(TuningRequest request, CancellationToken cancellationToken = default);
    Task<List<TuningResult>> GetTuningHistoryAsync(string? userId = null, CancellationToken cancellationToken = default);
    Task<TuningStatus> GetTuningStatusAsync(string tuningId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Context manager interface
/// </summary>
public interface IContextManager
{
    Task<string> GetContextAsync(string key, CancellationToken cancellationToken = default);
    Task SetContextAsync(string key, string value, CancellationToken cancellationToken = default);
    Task RemoveContextAsync(string key, CancellationToken cancellationToken = default);
    Task<Dictionary<string, string>> GetAllContextAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Semantic analyzer interface
/// </summary>
public interface ISemanticAnalyzer
{
    Task<SemanticAnalysisResult> AnalyzeAsync(string text, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query classifier interface
/// </summary>
public interface IQueryClassifier
{
    Task<QueryClassificationResult> ClassifyAsync(string query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Query optimizer interface
/// </summary>
public interface IQueryOptimizer
{
    Task<QueryOptimizationResult> OptimizeAsync(string query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Prompt service interface
/// </summary>
public interface IPromptService
{
    Task<string> GetPromptAsync(string promptKey, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default);
    Task SavePromptAsync(string promptKey, string template, CancellationToken cancellationToken = default);
    Task<List<string>> GetPromptKeysAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Password hasher interface
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    string GenerateSalt();
}

/// <summary>
/// Metrics collector interface
/// </summary>
public interface IMetricsCollector
{
    Task RecordMetricAsync(string name, double value, Dictionary<string, string>? tags = null);
    Task RecordCounterAsync(string name, long value = 1, Dictionary<string, string>? tags = null);
    Task RecordTimingAsync(string name, TimeSpan duration, Dictionary<string, string>? tags = null);
    Task<Dictionary<string, object>> GetMetricsAsync(string? filter = null);
}

// Duplicate IEmailService and ISmsService removed - using the ones defined above

// Duplicate ISchemaManagementService removed - using the one defined above

// Duplicate IUserRepository removed - using the one defined above

// Duplicate ITokenRepository removed - using the one defined above

// Duplicate IMfaChallengeRepository removed - using the one defined above

/// <summary>
/// MFA service interface
/// </summary>
public interface IMfaService
{
    Task<MfaChallengeResult> CreateChallengeAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> ValidateChallengeAsync(string challengeId, string code, CancellationToken cancellationToken = default);
}

// Duplicate IUserService removed - using the one defined above
