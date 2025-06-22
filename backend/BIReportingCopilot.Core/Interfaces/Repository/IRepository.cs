using System.Linq.Expressions;

namespace BIReportingCopilot.Core.Interfaces.Repository;

/// <summary>
/// Generic repository interface providing common CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
public interface IRepository<TEntity, TKey> where TEntity : class
{
    // Basic CRUD operations
    Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default);
    Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default);

    // Query operations
    Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);
    Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    // Paging operations
    Task<List<TEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default);

    // Ordering operations
    Task<List<TEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<TEntity, TOrderBy>> orderBy, bool ascending = true, CancellationToken cancellationToken = default);
    Task<List<TEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TOrderBy>> orderBy, bool ascending = true, CancellationToken cancellationToken = default);

    // Bulk operations
    Task<int> BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default);
    Task<int> BulkDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default);

    // Include operations for navigation properties
    Task<TEntity?> GetByIdWithIncludesAsync(TKey id, params Expression<Func<TEntity, object>>[] includes);
    Task<List<TEntity>> FindWithIncludesAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes);

    // Async enumerable for large datasets
    IAsyncEnumerable<TEntity> GetAsyncEnumerable(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default);

    // Transaction support
    Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default);
    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}

/// <summary>
/// Generic repository interface with long primary key (most common case)
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IRepository<TEntity> : IRepository<TEntity, long> where TEntity : class
{
}

/// <summary>
/// Repository interface for entities with string primary keys
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IStringKeyRepository<TEntity> : IRepository<TEntity, string> where TEntity : class
{
}

/// <summary>
/// Repository interface for entities with GUID primary keys
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public interface IGuidKeyRepository<TEntity> : IRepository<TEntity, Guid> where TEntity : class
{
}
