using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using System.Linq.Expressions;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for template performance metrics operations
/// </summary>
public class TemplatePerformanceRepository : ITemplatePerformanceRepository
{
    private readonly BICopilotContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TemplatePerformanceRepository> _logger;

    public TemplatePerformanceRepository(BICopilotContext context, IMapper mapper, ILogger<TemplatePerformanceRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region IRepository Implementation

    public async Task<TemplatePerformanceMetricsEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplatePerformanceMetrics.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity;
    }

    public async Task<TemplatePerformanceMetricsEntity> CreateAsync(TemplatePerformanceMetricsEntity entity, CancellationToken cancellationToken = default)
    {
        _context.TemplatePerformanceMetrics.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TemplatePerformanceMetricsEntity> UpdateAsync(TemplatePerformanceMetricsEntity entity, CancellationToken cancellationToken = default)
    {
        _context.TemplatePerformanceMetrics.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplatePerformanceMetrics.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null) return false;

        _context.TemplatePerformanceMetrics.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> FindAsync(Expression<Func<TemplatePerformanceMetricsEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<TemplatePerformanceMetricsEntity?> FirstOrDefaultAsync(Expression<Func<TemplatePerformanceMetricsEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TemplatePerformanceMetricsEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<TemplatePerformanceMetricsEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _context.TemplatePerformanceMetrics.CountAsync(cancellationToken);
        return await _context.TemplatePerformanceMetrics.CountAsync(predicate, cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetPagedAsync(Expression<Func<TemplatePerformanceMetricsEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .Where(predicate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<TemplatePerformanceMetricsEntity, TOrderBy>> orderBy, bool ascending = true, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplatePerformanceMetrics.AsNoTracking();
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<TemplatePerformanceMetricsEntity, bool>> predicate, Expression<Func<TemplatePerformanceMetricsEntity, TOrderBy>> orderBy, bool ascending = true, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplatePerformanceMetrics.AsNoTracking().Where(predicate);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> BulkInsertAsync(IEnumerable<TemplatePerformanceMetricsEntity> entities, CancellationToken cancellationToken = default)
    {
        await _context.TemplatePerformanceMetrics.AddRangeAsync(entities, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<TemplatePerformanceMetricsEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.TemplatePerformanceMetrics.UpdateRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkDeleteAsync(Expression<Func<TemplatePerformanceMetricsEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplatePerformanceMetrics.Where(predicate).ToListAsync(cancellationToken);
        _context.TemplatePerformanceMetrics.RemoveRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TemplatePerformanceMetricsEntity?> GetByIdWithIncludesAsync(long id, params Expression<Func<TemplatePerformanceMetricsEntity, object>>[] includes)
    {
        var query = _context.TemplatePerformanceMetrics.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> FindWithIncludesAsync(Expression<Func<TemplatePerformanceMetricsEntity, bool>> predicate, params Expression<Func<TemplatePerformanceMetricsEntity, object>>[] includes)
    {
        var query = _context.TemplatePerformanceMetrics.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.Where(predicate).ToListAsync();
    }

    public IAsyncEnumerable<TemplatePerformanceMetricsEntity> GetAsyncEnumerable(Expression<Func<TemplatePerformanceMetricsEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplatePerformanceMetrics.AsNoTracking();
        if (predicate != null)
            query = query.Where(predicate);
        return query.AsAsyncEnumerable();
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(Func<Task<TResult>> operation, CancellationToken cancellationToken = default)
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

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
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

    #region ITemplatePerformanceRepository Implementation

    public async Task<TemplatePerformanceMetricsEntity?> GetByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .Where(x => x.IntentType == intentType)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetByTemplateKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .Where(x => x.TemplateKey == templateKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetTopPerformingTemplatesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .OrderByDescending(x => x.SuccessRate)
            .ThenByDescending(x => x.TotalUsages)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetUnderperformingTemplatesAsync(decimal threshold = 0.7m, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .Where(x => x.SuccessRate < threshold && x.TotalUsages > 0)
            .OrderBy(x => x.SuccessRate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetByUsageDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .Where(x => x.LastUsedDate >= startDate && x.LastUsedDate <= endDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetRecentlyUsedAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .Where(x => x.LastUsedDate >= cutoffDate)
            .OrderByDescending(x => x.LastUsedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetUsageCountsByIntentTypeAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .GroupBy(x => x.IntentType)
            .ToDictionaryAsync(g => g.Key, g => g.Sum(x => x.TotalUsages), cancellationToken);
    }

    public async Task<decimal> GetAverageSuccessRateAsync(string? intentType = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplatePerformanceMetrics.AsNoTracking();
        if (!string.IsNullOrEmpty(intentType))
            query = query.Where(x => x.IntentType == intentType);

        var metrics = await query.Where(x => x.TotalUsages > 0).ToListAsync(cancellationToken);
        if (!metrics.Any()) return 0;

        return metrics.Average(x => x.SuccessRate);
    }

    public async Task<Dictionary<string, decimal>> GetSuccessRatesByIntentTypeAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TemplatePerformanceMetrics.AsNoTracking()
            .Where(x => x.TotalUsages > 0)
            .GroupBy(x => x.IntentType)
            .ToDictionaryAsync(g => g.Key, g => g.Average(x => x.SuccessRate), cancellationToken);
    }

    public async Task<decimal> GetAverageProcessingTimeAsync(string? intentType = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplatePerformanceMetrics.AsNoTracking();
        if (!string.IsNullOrEmpty(intentType))
            query = query.Where(x => x.IntentType == intentType);

        var metrics = await query.Where(x => x.TotalUsages > 0).ToListAsync(cancellationToken);
        if (!metrics.Any()) return 0;

        return (decimal)metrics.Average(x => x.AverageProcessingTimeMs);
    }

    // Real-time updates
    public async Task IncrementUsageAsync(long templateId, bool wasSuccessful, decimal confidenceScore, int processingTimeMs, CancellationToken cancellationToken = default)
    {
        var entity = await GetByTemplateIdAsync(templateId, cancellationToken);
        if (entity == null)
        {
            // Create new performance metrics entity
            entity = new TemplatePerformanceMetricsEntity
            {
                TemplateId = templateId,
                TemplateKey = $"template_{templateId}", // Would need to get actual key
                IntentType = "Unknown", // Would need to get from template
                TotalUsages = 1,
                SuccessfulUsages = wasSuccessful ? 1 : 0,
                SuccessRate = wasSuccessful ? 1.0m : 0.0m,
                AverageConfidenceScore = confidenceScore,
                AverageProcessingTimeMs = processingTimeMs,
                LastUsedDate = DateTime.UtcNow,
                AnalysisDate = DateTime.UtcNow,
                CreatedBy = "System"
            };
            await CreateAsync(entity, cancellationToken);
        }
        else
        {
            // Update existing metrics
            entity.TotalUsages++;
            if (wasSuccessful) entity.SuccessfulUsages++;
            entity.SuccessRate = entity.TotalUsages > 0 ? (decimal)entity.SuccessfulUsages / entity.TotalUsages : 0;

            // Update average confidence score
            entity.AverageConfidenceScore = ((entity.AverageConfidenceScore * (entity.TotalUsages - 1)) + confidenceScore) / entity.TotalUsages;

            // Update average processing time
            entity.AverageProcessingTimeMs = (int)(((entity.AverageProcessingTimeMs * (entity.TotalUsages - 1)) + processingTimeMs) / entity.TotalUsages);

            entity.LastUsedDate = DateTime.UtcNow;
            entity.AnalysisDate = DateTime.UtcNow;

            await UpdateAsync(entity, cancellationToken);
        }
    }

    public async Task UpdateUserRatingAsync(long templateId, decimal rating, CancellationToken cancellationToken = default)
    {
        var entity = await GetByTemplateIdAsync(templateId, cancellationToken);
        if (entity != null)
        {
            entity.AverageUserRating = rating; // Simplified - would need proper averaging logic
            await UpdateAsync(entity, cancellationToken);
        }
    }

    public async Task RecalculateMetricsAsync(long templateId, CancellationToken cancellationToken = default)
    {
        // This would involve recalculating all metrics from raw usage data
        // For now, just update the analysis date
        var entity = await GetByTemplateIdAsync(templateId, cancellationToken);
        if (entity != null)
        {
            entity.AnalysisDate = DateTime.UtcNow;
            await UpdateAsync(entity, cancellationToken);
        }
    }

    public async Task UpdateLastUsedDateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await GetByTemplateIdAsync(templateId, cancellationToken);
        if (entity != null)
        {
            entity.LastUsedDate = DateTime.UtcNow;
            await UpdateAsync(entity, cancellationToken);
        }
    }

    #endregion
}
