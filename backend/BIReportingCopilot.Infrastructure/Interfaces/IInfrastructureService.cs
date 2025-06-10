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
