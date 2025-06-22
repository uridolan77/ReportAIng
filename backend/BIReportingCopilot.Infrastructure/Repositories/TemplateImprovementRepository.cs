using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for template improvement suggestions operations
/// </summary>
public class TemplateImprovementRepository : BaseRepository<TemplateImprovementSuggestionEntity>, ITemplateImprovementRepository
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(10);

    public TemplateImprovementRepository(
        BICopilotContext context, 
        ILogger<TemplateImprovementRepository> logger,
        IMemoryCache cache) : base(context, logger)
    {
        _cache = cache;
    }

    protected override long GetEntityId(TemplateImprovementSuggestionEntity entity) => entity.Id;
    protected override Expression<Func<TemplateImprovementSuggestionEntity, bool>> GetByIdPredicate(long id) => x => x.Id == id;

    #region Suggestion management queries

    public async Task<List<TemplateImprovementSuggestionEntity>> GetByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.TemplateId == templateId)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting improvement suggestions by template ID: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<List<TemplateImprovementSuggestionEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"improvement_suggestions_by_status_{status}";
        
        if (_cache.TryGetValue<List<TemplateImprovementSuggestionEntity>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var suggestions = await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, suggestions, _cacheExpiry);
            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting improvement suggestions by status: {Status}", status);
            throw;
        }
    }

    public async Task<List<TemplateImprovementSuggestionEntity>> GetBySuggestionTypeAsync(string suggestionType, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.SuggestionType == suggestionType)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting improvement suggestions by type: {SuggestionType}", suggestionType);
            throw;
        }
    }

    public async Task<List<TemplateImprovementSuggestionEntity>> GetPendingReviewAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "pending_improvement_suggestions";
        
        if (_cache.TryGetValue<List<TemplateImprovementSuggestionEntity>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var suggestions = await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.Status == "pending")
                .OrderByDescending(x => x.ExpectedImprovementPercent)
                .ThenByDescending(x => x.BasedOnDataPoints)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, suggestions, _cacheExpiry);
            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pending improvement suggestions");
            throw;
        }
    }

    public async Task<List<TemplateImprovementSuggestionEntity>> GetByReviewerAsync(string reviewerId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.ReviewedBy == reviewerId)
                .OrderByDescending(x => x.ReviewedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting improvement suggestions by reviewer: {ReviewerId}", reviewerId);
            throw;
        }
    }

    #endregion

    #region Review workflow

    public async Task ApproveSuggestionAsync(long suggestionId, string reviewedBy, string? reviewComments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestion = await _dbSet.FirstOrDefaultAsync(x => x.Id == suggestionId, cancellationToken);
            
            if (suggestion == null)
            {
                _logger.LogWarning("Improvement suggestion not found for ID: {SuggestionId}", suggestionId);
                return;
            }

            suggestion.Status = "approved";
            suggestion.ReviewedBy = reviewedBy;
            suggestion.ReviewedDate = DateTime.UtcNow;
            suggestion.ReviewComments = reviewComments;
            suggestion.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            InvalidateCache();

            _logger.LogInformation("Approved improvement suggestion {SuggestionId} by {ReviewedBy}", suggestionId, reviewedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error approving improvement suggestion {SuggestionId}", suggestionId);
            throw;
        }
    }

    public async Task RejectSuggestionAsync(long suggestionId, string reviewedBy, string? reviewComments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestion = await _dbSet.FirstOrDefaultAsync(x => x.Id == suggestionId, cancellationToken);
            
            if (suggestion == null)
            {
                _logger.LogWarning("Improvement suggestion not found for ID: {SuggestionId}", suggestionId);
                return;
            }

            suggestion.Status = "rejected";
            suggestion.ReviewedBy = reviewedBy;
            suggestion.ReviewedDate = DateTime.UtcNow;
            suggestion.ReviewComments = reviewComments;
            suggestion.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            InvalidateCache();

            _logger.LogInformation("Rejected improvement suggestion {SuggestionId} by {ReviewedBy}", suggestionId, reviewedBy);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error rejecting improvement suggestion {SuggestionId}", suggestionId);
            throw;
        }
    }

    public async Task ImplementSuggestionAsync(long suggestionId, CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestion = await _dbSet.FirstOrDefaultAsync(x => x.Id == suggestionId, cancellationToken);
            
            if (suggestion == null)
            {
                _logger.LogWarning("Improvement suggestion not found for ID: {SuggestionId}", suggestionId);
                return;
            }

            suggestion.Status = "implemented";
            suggestion.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            InvalidateCache();

            _logger.LogInformation("Implemented improvement suggestion {SuggestionId}", suggestionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error implementing improvement suggestion {SuggestionId}", suggestionId);
            throw;
        }
    }

    public async Task UpdateSuggestionStatusAsync(long suggestionId, string status, string? reviewComments = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var suggestion = await _dbSet.FirstOrDefaultAsync(x => x.Id == suggestionId, cancellationToken);
            
            if (suggestion == null)
            {
                _logger.LogWarning("Improvement suggestion not found for ID: {SuggestionId}", suggestionId);
                return;
            }

            suggestion.Status = status;
            suggestion.ReviewComments = reviewComments ?? suggestion.ReviewComments;
            suggestion.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            InvalidateCache();

            _logger.LogInformation("Updated improvement suggestion {SuggestionId} status to {Status}", suggestionId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating improvement suggestion {SuggestionId} status", suggestionId);
            throw;
        }
    }

    #endregion

    #region Analytics and reporting

    public async Task<Dictionary<string, int>> GetSuggestionCountsByTypeAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "suggestion_counts_by_type";
        
        if (_cache.TryGetValue<Dictionary<string, int>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var results = await _dbSet.AsNoTracking()
                .GroupBy(x => x.SuggestionType)
                .Select(g => new { Type = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);

            _cache.Set(cacheKey, results, _cacheExpiry);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestion counts by type");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetSuggestionCountsByStatusAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "suggestion_counts_by_status";
        
        if (_cache.TryGetValue<Dictionary<string, int>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var results = await _dbSet.AsNoTracking()
                .GroupBy(x => x.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Status, x => x.Count, cancellationToken);

            _cache.Set(cacheKey, results, _cacheExpiry);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestion counts by status");
            throw;
        }
    }

    public async Task<List<TemplateImprovementSuggestionEntity>> GetHighImpactSuggestionsAsync(decimal minImprovementPercent = 10m, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.ExpectedImprovementPercent >= minImprovementPercent && x.Status == "pending")
                .OrderByDescending(x => x.ExpectedImprovementPercent)
                .ThenByDescending(x => x.BasedOnDataPoints)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting high impact suggestions with minimum improvement: {MinImprovement}%", minImprovementPercent);
            throw;
        }
    }

    public async Task<List<TemplateImprovementSuggestionEntity>> GetRecentSuggestionsAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.CreatedDate >= cutoffDate)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recent suggestions for {Days} days", days);
            throw;
        }
    }

    public async Task<List<TemplateImprovementSuggestionEntity>> GetSuggestionsByDataPointsAsync(int minDataPoints, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.Template)
                .Where(x => x.BasedOnDataPoints >= minDataPoints)
                .OrderByDescending(x => x.BasedOnDataPoints)
                .ThenByDescending(x => x.ExpectedImprovementPercent)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestions by minimum data points: {MinDataPoints}", minDataPoints);
            throw;
        }
    }

    #endregion

    #region Suggestion validation

    public async Task<bool> HasPendingSuggestionsForTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(x => x.TemplateId == templateId && x.Status == "pending", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if template {TemplateId} has pending suggestions", templateId);
            throw;
        }
    }

    public async Task<int> GetSuggestionCountForTemplateAsync(long templateId, string? status = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking().Where(x => x.TemplateId == templateId);
            
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(x => x.Status == status);
            }

            return await query.CountAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting suggestion count for template {TemplateId} with status {Status}", templateId, status);
            throw;
        }
    }

    #endregion

    #region Private methods

    private void InvalidateCache()
    {
        _cache.Remove("pending_improvement_suggestions");
        _cache.Remove("suggestion_counts_by_type");
        _cache.Remove("suggestion_counts_by_status");
        
        // Remove status-specific caches
        var statuses = new[] { "pending", "approved", "rejected", "implemented" };
        foreach (var status in statuses)
        {
            _cache.Remove($"improvement_suggestions_by_status_{status}");
        }
    }

    #endregion
}
