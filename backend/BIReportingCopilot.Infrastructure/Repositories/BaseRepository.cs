using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Base repository implementation providing common CRUD operations
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The primary key type</typeparam>
public abstract class BaseRepository<TEntity, TKey> : IRepository<TEntity, TKey> where TEntity : class
{
    protected readonly BICopilotContext _context;
    protected readonly DbSet<TEntity> _dbSet;
    protected readonly ILogger _logger;

    protected BaseRepository(BICopilotContext context, ILogger logger)
    {
        _context = context;
        _dbSet = context.Set<TEntity>();
        _logger = logger;
    }

    // Abstract method to get entity ID - must be implemented by derived classes
    protected abstract TKey GetEntityId(TEntity entity);
    protected abstract Expression<Func<TEntity, bool>> GetByIdPredicate(TKey id);

    #region Basic CRUD Operations

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(GetByIdPredicate(id), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by ID: {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            var entry = await _dbSet.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Created {EntityType} with ID: {Id}", typeof(TEntity).Name, GetEntityId(entity));
            return entry.Entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating {EntityType}", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Updated {EntityType} with ID: {Id}", typeof(TEntity).Name, GetEntityId(entity));
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating {EntityType} with ID: {Id}", typeof(TEntity).Name, GetEntityId(entity));
            throw;
        }
    }

    public virtual async Task<bool> DeleteAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FirstOrDefaultAsync(GetByIdPredicate(id), cancellationToken);
            if (entity == null)
            {
                _logger.LogWarning("{EntityType} with ID {Id} not found for deletion", typeof(TEntity).Name, id);
                return false;
            }

            _dbSet.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Deleted {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    public virtual async Task<bool> ExistsAsync(TKey id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(GetByIdPredicate(id), cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking existence of {EntityType} with ID: {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    #endregion

    #region Query Operations

    public virtual async Task<List<TEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking().ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<List<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Where(predicate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding {EntityType} entities with predicate", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<TEntity?> FirstOrDefaultAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting first {EntityType} entity with predicate", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<bool> AnyAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(predicate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if any {EntityType} entities exist with predicate", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<int> CountAsync(Expression<Func<TEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            if (predicate != null)
                query = query.Where(predicate);
                
            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error counting {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    #endregion

    #region Paging Operations

    public virtual async Task<List<TEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged {EntityType} entities (page: {Page}, size: {PageSize})", typeof(TEntity).Name, page, pageSize);
            throw;
        }
    }

    public virtual async Task<List<TEntity>> GetPagedAsync(Expression<Func<TEntity, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Where(predicate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting paged {EntityType} entities with predicate (page: {Page}, size: {PageSize})", typeof(TEntity).Name, page, pageSize);
            throw;
        }
    }

    #endregion

    #region Ordering Operations

    public virtual async Task<List<TEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<TEntity, TOrderBy>> orderBy, bool ascending = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            
            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ordered {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<List<TEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, TOrderBy>> orderBy, bool ascending = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking().Where(predicate);
            query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
            
            return await query.ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting ordered {EntityType} entities with predicate", typeof(TEntity).Name);
            throw;
        }
    }

    #endregion

    #region Bulk Operations

    public virtual async Task<int> BulkInsertAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            await _dbSet.AddRangeAsync(entities, cancellationToken);
            var count = await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Bulk inserted {Count} {EntityType} entities", count, typeof(TEntity).Name);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk inserting {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<int> BulkUpdateAsync(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
    {
        try
        {
            _dbSet.UpdateRange(entities);
            var count = await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Bulk updated {Count} {EntityType} entities", count, typeof(TEntity).Name);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    public virtual async Task<int> BulkDeleteAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        try
        {
            var entities = await _dbSet.Where(predicate).ToListAsync(cancellationToken);
            _dbSet.RemoveRange(entities);
            var count = await _context.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Bulk deleted {Count} {EntityType} entities", count, typeof(TEntity).Name);
            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk deleting {EntityType} entities", typeof(TEntity).Name);
            throw;
        }
    }

    #endregion

    #region Include Operations

    public virtual async Task<TEntity?> GetByIdWithIncludesAsync(TKey id, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.FirstOrDefaultAsync(GetByIdPredicate(id));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting {EntityType} by ID with includes: {Id}", typeof(TEntity).Name, id);
            throw;
        }
    }

    public virtual async Task<List<TEntity>> FindWithIncludesAsync(Expression<Func<TEntity, bool>> predicate, params Expression<Func<TEntity, object>>[] includes)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            
            foreach (var include in includes)
            {
                query = query.Include(include);
            }
            
            return await query.Where(predicate).ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding {EntityType} entities with includes", typeof(TEntity).Name);
            throw;
        }
    }

    #endregion

    #region Async Enumerable

    public virtual async IAsyncEnumerable<TEntity> GetAsyncEnumerable(Expression<Func<TEntity, bool>>? predicate = null, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var query = _dbSet.AsNoTracking();
        if (predicate != null)
            query = query.Where(predicate);

        await foreach (var entity in query.AsAsyncEnumerable().WithCancellation(cancellationToken))
        {
            yield return entity;
        }
    }

    #endregion

    #region Transaction Support

    public virtual async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var result = await operation();
            await transaction.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public virtual async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            await operation();
            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    #endregion
}

/// <summary>
/// Base repository for entities with long primary keys
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
public abstract class BaseRepository<TEntity> : BaseRepository<TEntity, long>, IRepository<TEntity> where TEntity : class
{
    protected BaseRepository(BICopilotContext context, ILogger logger) : base(context, logger)
    {
    }
}
