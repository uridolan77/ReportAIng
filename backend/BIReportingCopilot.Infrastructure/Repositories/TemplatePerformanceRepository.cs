using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for template performance metrics operations
/// </summary>
public class TemplatePerformanceRepository : BaseRepository<TemplatePerformanceMetricsEntity>, ITemplatePerformanceRepository
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(15);

    public TemplatePerformanceRepository(
        BICopilotContext context, 
        ILogger<TemplatePerformanceRepository> logger,
        IMemoryCache cache) : base(context, logger)
    {
        _cache = cache;
    }

    protected override long GetEntityId(TemplatePerformanceMetricsEntity entity) => entity.Id;
    protected override Expression<Func<TemplatePerformanceMetricsEntity, bool>> GetByIdPredicate(long id) => x => x.Id == id;

    #region Template-specific queries

    public async Task<TemplatePerformanceMetricsEntity?> GetByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"template_performance_by_template_{templateId}";
        
        if (_cache.TryGetValue<TemplatePerformanceMetricsEntity>(cacheKey, out var cached))
        {
            return cached;
        }

        try
        {
            var entity = await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);

            if (entity != null)
            {
                _cache.Set(cacheKey, entity, _cacheExpiry);
            }

            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance by template ID: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"template_performance_by_intent_{intentType}";
        
        if (_cache.TryGetValue<List<TemplatePerformanceMetricsEntity>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var entities = await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.IntentType == intentType)
                .OrderByDescending(x => x.SuccessRate)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, entities, _cacheExpiry);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance by intent type: {IntentType}", intentType);
            throw;
        }
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetByTemplateKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.TemplateKey == templateKey)
                .OrderByDescending(x => x.UpdatedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance by template key: {TemplateKey}", templateKey);
            throw;
        }
    }

    #endregion

    #region Performance analytics queries

    public async Task<List<TemplatePerformanceMetricsEntity>> GetTopPerformingTemplatesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"top_performing_templates_{count}";
        
        if (_cache.TryGetValue<List<TemplatePerformanceMetricsEntity>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var entities = await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.TotalUsages >= 10) // Minimum usage threshold
                .OrderByDescending(x => x.SuccessRate)
                .ThenByDescending(x => x.TotalUsages)
                .Take(count)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, entities, _cacheExpiry);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top performing templates");
            throw;
        }
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetUnderperformingTemplatesAsync(decimal threshold = 0.7m, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.SuccessRate < threshold && x.TotalUsages >= 5)
                .OrderBy(x => x.SuccessRate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting underperforming templates with threshold: {Threshold}", threshold);
            throw;
        }
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetByUsageDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.LastUsedDate >= startDate && x.LastUsedDate <= endDate)
                .OrderByDescending(x => x.LastUsedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template performance by usage date range: {StartDate} - {EndDate}", startDate, endDate);
            throw;
        }
    }

    public async Task<List<TemplatePerformanceMetricsEntity>> GetRecentlyUsedAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.LastUsedDate >= cutoffDate)
                .OrderByDescending(x => x.LastUsedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recently used templates for {Days} days", days);
            throw;
        }
    }

    #endregion

    #region Metrics aggregation

    public async Task<Dictionary<string, decimal>> GetSuccessRatesByIntentTypeAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "success_rates_by_intent_type";
        
        if (_cache.TryGetValue<Dictionary<string, decimal>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var results = await _dbSet.AsNoTracking()
                .GroupBy(x => x.IntentType)
                .Select(g => new { IntentType = g.Key, AvgSuccessRate = g.Average(x => x.SuccessRate) })
                .ToDictionaryAsync(x => x.IntentType, x => x.AvgSuccessRate, cancellationToken);

            _cache.Set(cacheKey, results, _cacheExpiry);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting success rates by intent type");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetUsageCountsByIntentTypeAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "usage_counts_by_intent_type";
        
        if (_cache.TryGetValue<Dictionary<string, int>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var results = await _dbSet.AsNoTracking()
                .GroupBy(x => x.IntentType)
                .Select(g => new { IntentType = g.Key, TotalUsage = g.Sum(x => x.TotalUsages) })
                .ToDictionaryAsync(x => x.IntentType, x => x.TotalUsage, cancellationToken);

            _cache.Set(cacheKey, results, _cacheExpiry);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting usage counts by intent type");
            throw;
        }
    }

    public async Task<decimal> GetAverageSuccessRateAsync(string? intentType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            
            if (!string.IsNullOrEmpty(intentType))
            {
                query = query.Where(x => x.IntentType == intentType);
            }

            return await query.AverageAsync(x => x.SuccessRate, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average success rate for intent type: {IntentType}", intentType);
            throw;
        }
    }

    public async Task<decimal> GetAverageProcessingTimeAsync(string? intentType = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking();
            
            if (!string.IsNullOrEmpty(intentType))
            {
                query = query.Where(x => x.IntentType == intentType);
            }

            return await query.AverageAsync(x => (decimal)x.AverageProcessingTimeMs, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting average processing time for intent type: {IntentType}", intentType);
            throw;
        }
    }

    #endregion

    #region Real-time updates

    public async Task IncrementUsageAsync(long templateId, bool wasSuccessful, decimal confidenceScore, int processingTimeMs, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);
            
            if (entity == null)
            {
                _logger.LogWarning("Template performance metrics not found for template ID: {TemplateId}", templateId);
                return;
            }

            // Update metrics
            entity.TotalUsages++;
            if (wasSuccessful)
            {
                entity.SuccessfulUsages++;
            }

            // Recalculate success rate
            entity.SuccessRate = entity.TotalUsages > 0 ? (decimal)entity.SuccessfulUsages / entity.TotalUsages : 0;

            // Update average confidence score (weighted average)
            var totalConfidence = entity.AverageConfidenceScore * (entity.TotalUsages - 1) + confidenceScore;
            entity.AverageConfidenceScore = totalConfidence / entity.TotalUsages;

            // Update average processing time (weighted average)
            var totalProcessingTime = entity.AverageProcessingTimeMs * (entity.TotalUsages - 1) + processingTimeMs;
            entity.AverageProcessingTimeMs = (int)(totalProcessingTime / entity.TotalUsages);

            entity.LastUsedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove($"template_performance_by_template_{templateId}");
            _cache.Remove("success_rates_by_intent_type");
            _cache.Remove("usage_counts_by_intent_type");

            _logger.LogInformation("Updated template performance metrics for template {TemplateId}", templateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing usage for template {TemplateId}", templateId);
            throw;
        }
    }

    public async Task UpdateUserRatingAsync(long templateId, decimal rating, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);
            
            if (entity == null)
            {
                _logger.LogWarning("Template performance metrics not found for template ID: {TemplateId}", templateId);
                return;
            }

            // Simple average for now - could be enhanced with weighted average
            entity.AverageUserRating = entity.AverageUserRating.HasValue 
                ? (entity.AverageUserRating.Value + rating) / 2 
                : rating;

            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove($"template_performance_by_template_{templateId}");

            _logger.LogInformation("Updated user rating for template {TemplateId}: {Rating}", templateId, rating);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user rating for template {TemplateId}", templateId);
            throw;
        }
    }

    public async Task RecalculateMetricsAsync(long templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);
            
            if (entity == null)
            {
                _logger.LogWarning("Template performance metrics not found for template ID: {TemplateId}", templateId);
                return;
            }

            // Recalculate success rate
            entity.SuccessRate = entity.TotalUsages > 0 ? (decimal)entity.SuccessfulUsages / entity.TotalUsages : 0;

            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove($"template_performance_by_template_{templateId}");

            _logger.LogInformation("Recalculated metrics for template {TemplateId}", templateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating metrics for template {TemplateId}", templateId);
            throw;
        }
    }

    public async Task UpdateLastUsedDateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var entity = await _dbSet.FirstOrDefaultAsync(x => x.TemplateId == templateId, cancellationToken);
            
            if (entity == null)
            {
                _logger.LogWarning("Template performance metrics not found for template ID: {TemplateId}", templateId);
                return;
            }

            entity.LastUsedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove($"template_performance_by_template_{templateId}");

            _logger.LogDebug("Updated last used date for template {TemplateId}", templateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last used date for template {TemplateId}", templateId);
            throw;
        }
    }

    #endregion
}
