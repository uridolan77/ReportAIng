using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for template improvement suggestions
/// </summary>
public class TemplateImprovementSuggestionRepository : IRepository<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, long>, ITemplateImprovementRepository
{
    private readonly BICopilotContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TemplateImprovementSuggestionRepository> _logger;

    public TemplateImprovementSuggestionRepository(BICopilotContext context, IMapper mapper, ILogger<TemplateImprovementSuggestionRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region IRepository Implementation

    public async Task<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity;
    }

    public async Task<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity> CreateAsync(BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity entity, CancellationToken cancellationToken = default)
    {
        _context.TemplateImprovementSuggestions.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity> UpdateAsync(BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity entity, CancellationToken cancellationToken = default)
    {
        _context.TemplateImprovementSuggestions.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplateImprovementSuggestions.FindAsync(id);
        if (entity != null)
        {
            _context.TemplateImprovementSuggestions.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateImprovementSuggestions.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .ToListAsync(cancellationToken);
        return entities;
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> FindAsync(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await GetAllAsync(cancellationToken);
        return entities.Where(predicate.Compile()).ToList();
    }

    public async Task<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity?> FirstOrDefaultAsync(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        return entities.FirstOrDefault();
    }

    public async Task<bool> AnyAsync(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        return entities.Any();
    }

    public async Task<int> CountAsync(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _context.TemplateImprovementSuggestions.CountAsync(cancellationToken);
        }
        var entities = await FindAsync(predicate, cancellationToken);
        return entities.Count;
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return entities;
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetPagedAsync(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        return entities.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, TOrderBy>> orderBy, bool descending = false, CancellationToken cancellationToken = default)
    {
        var entities = await GetAllAsync(cancellationToken);
        return descending ? entities.OrderByDescending(orderBy.Compile()).ToList() : entities.OrderBy(orderBy.Compile()).ToList();
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>> predicate, Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, TOrderBy>> orderBy, bool descending = false, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        return descending ? entities.OrderByDescending(orderBy.Compile()).ToList() : entities.OrderBy(orderBy.Compile()).ToList();
    }

    public async Task<int> BulkInsertAsync(IEnumerable<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.TemplateImprovementSuggestions.AddRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.TemplateImprovementSuggestions.UpdateRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkDeleteAsync(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        _context.TemplateImprovementSuggestions.RemoveRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity?> GetByIdWithIncludesAsync(long id, params Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, object>>[] includes)
    {
        return await GetByIdAsync(id, default);
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> FindWithIncludesAsync(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>> predicate, params Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, object>>[] includes)
    {
        return await FindAsync(predicate, default);
    }

    public IAsyncEnumerable<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity> GetAsyncEnumerable(Expression<Func<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplateImprovementSuggestions.AsNoTracking().Include(x => x.Template);
        return query.Select(e => _mapper.Map<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>(e)).AsAsyncEnumerable();
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

    #region ITemplateImprovementRepository Implementation

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.TemplateId == templateId)
            .ToListAsync(cancellationToken);
        return entities;
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.Status == status)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetBySuggestionTypeAsync(string suggestionType, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.SuggestionType == suggestionType)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetPendingReviewAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.Status == "pending")
            .ToListAsync(cancellationToken);
        return entities;
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetPendingSuggestionsAsync(CancellationToken cancellationToken = default)
    {
        return await GetPendingReviewAsync(cancellationToken);
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetByReviewerAsync(string reviewer, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.ReviewedBy == reviewer)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetHighImpactSuggestionsAsync(decimal minImpact, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.ExpectedImprovementPercent >= minImpact)
            .ToListAsync(cancellationToken);
        return entities;
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetRecentSuggestionsAsync(int days, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.CreatedDate >= cutoffDate)
            .ToListAsync(cancellationToken);
        return entities;
    }

    public async Task<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>> GetSuggestionsByDataPointsAsync(int minDataPoints, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .Include(x => x.Template)
            .Where(x => x.BasedOnDataPoints >= minDataPoints)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.TemplateImprovementSuggestionEntity>>(entities);
    }

    #endregion

    #region Additional ITemplateImprovementRepository Methods

    public async Task ApproveSuggestionAsync(long suggestionId, string approvedBy, string? approvalNotes = null, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplateImprovementSuggestions.FirstOrDefaultAsync(x => x.Id == suggestionId, cancellationToken);

        if (entity != null)
        {
            entity.Status = "approved";
            entity.ReviewedBy = approvedBy;
            entity.ReviewNotes = approvalNotes;
            entity.ReviewedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RejectSuggestionAsync(long suggestionId, string rejectedBy, string? rejectionReason = null, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplateImprovementSuggestions.FirstOrDefaultAsync(x => x.Id == suggestionId, cancellationToken);

        if (entity != null)
        {
            entity.Status = "rejected";
            entity.ReviewedBy = rejectedBy;
            entity.ReviewNotes = rejectionReason;
            entity.ReviewedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ImplementSuggestionAsync(long suggestionId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplateImprovementSuggestions.FirstOrDefaultAsync(x => x.Id == suggestionId, cancellationToken);

        if (entity != null)
        {
            entity.Status = "implemented";
            entity.ImplementedDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateSuggestionStatusAsync(long suggestionId, string status, string? notes = null, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplateImprovementSuggestions.FirstOrDefaultAsync(x => x.Id == suggestionId, cancellationToken);

        if (entity != null)
        {
            entity.Status = status;
            if (!string.IsNullOrEmpty(notes))
            {
                entity.ReviewNotes = notes;
            }
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<Dictionary<string, int>> GetSuggestionCountsByTypeAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .GroupBy(x => x.SuggestionType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return results.ToDictionary(x => x.Type, x => x.Count);
    }

    public async Task<Dictionary<string, int>> GetSuggestionCountsByStatusAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.TemplateImprovementSuggestions.AsNoTracking()
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return results.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<bool> HasPendingSuggestionsForTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateImprovementSuggestions.AsNoTracking()
            .AnyAsync(x => x.TemplateId == templateId && x.Status == "pending", cancellationToken);
    }

    public async Task<int> GetSuggestionCountForTemplateAsync(long templateId, string? status = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplateImprovementSuggestions.AsNoTracking()
            .Where(x => x.TemplateId == templateId);

        if (!string.IsNullOrEmpty(status))
        {
            query = query.Where(x => x.Status == status);
        }

        return await query.CountAsync(cancellationToken);
    }

    #endregion

    #region Additional IEnhancedPromptTemplateRepository Methods

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TemplateKey == templateKey, cancellationToken);
        return entity != null ? _mapper.Map<BIReportingCopilot.Core.Models.PromptTemplateEntity>(entity) : null;
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IntentType == intentType)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetByPriorityRangeAsync(int minPriority, int maxPriority, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.Priority >= minPriority && x.Priority <= maxPriority)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> SearchByTagsAsync(List<string> tags, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => tags.Any(tag => x.Tags != null && x.Tags.Contains(tag)))
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.Name.Contains(searchTerm))
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }



    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> SelectTemplateForUserAsync(string intentType, string userId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IntentType == intentType && x.IsActive)
            .OrderByDescending(x => x.Priority)
            .ToListAsync(cancellationToken);
        var coreEntities = _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
        return coreEntities.FirstOrDefault();
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetTemplatesEligibleForTestingAsync(string intentType, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IntentType == intentType && x.IsActive)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetRandomTemplateByIntentAsync(string intentType, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IntentType == intentType && x.IsActive)
            .ToListAsync(cancellationToken);
        var coreEntities = _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
        if (coreEntities.Any())
        {
            var random = new Random();
            return coreEntities[random.Next(coreEntities.Count)];
        }
        return null;
    }

    public async Task<Dictionary<string, int>> GetTemplateCountsByIntentTypeAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.PromptTemplates.AsNoTracking()
            .GroupBy(x => x.IntentType)
            .Select(g => new { IntentType = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return results.ToDictionary(x => x.IntentType, x => x.Count);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetMostUsedTemplatesAsync(int count, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .OrderByDescending(x => x.UsageCount)
            .Take(count)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetRecentlyUpdatedTemplatesAsync(int days, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.UpdatedDate >= cutoffDate)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetTemplatesNeedingReviewAsync(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-90); // Templates not reviewed in 90 days
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.LastBusinessReviewDate == null || x.LastBusinessReviewDate < cutoffDate)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task ActivateTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.IsActive = true;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeactivateTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateUsageCountAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.UsageCount++;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateSuccessRateAsync(long templateId, decimal successRate, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.SuccessRate = successRate;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateLastBusinessReviewAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.LastBusinessReviewDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetTemplateVersionsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.TemplateKey == templateKey)
            .OrderByDescending(x => x.Version)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetLatestVersionAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.TemplateKey == templateKey)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
        return entity != null ? _mapper.Map<BIReportingCopilot.Core.Models.PromptTemplateEntity>(entity) : null;
    }

    public async Task<string> GenerateNextVersionAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var latestVersion = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.TemplateKey == templateKey)
            .MaxAsync(x => (string?)x.Version, cancellationToken) ?? "1.0";

        // Simple version increment logic
        if (decimal.TryParse(latestVersion, out var version))
        {
            return (version + 0.1m).ToString("F1");
        }

        return "1.0";
    }

    #endregion
}

/// <summary>
/// Repository implementation for prompt templates
/// </summary>
public class EnhancedPromptTemplateRepository : IRepository<BIReportingCopilot.Core.Models.PromptTemplateEntity, long>, IEnhancedPromptTemplateRepository
{
    private readonly BICopilotContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<EnhancedPromptTemplateRepository> _logger;

    public EnhancedPromptTemplateRepository(BICopilotContext context, IMapper mapper, ILogger<EnhancedPromptTemplateRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region IRepository Implementation

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return entity != null ? _mapper.Map<BIReportingCopilot.Core.Models.PromptTemplateEntity>(entity) : null;
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity> CreateAsync(BIReportingCopilot.Core.Models.PromptTemplateEntity entity, CancellationToken cancellationToken = default)
    {
        var infraEntity = entity; // No mapping needed since we're using Core entities directly
        _context.PromptTemplates.Add(infraEntity);
        await _context.SaveChangesAsync(cancellationToken);
        return infraEntity; // No mapping needed since we're using Core entities directly
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity> UpdateAsync(BIReportingCopilot.Core.Models.PromptTemplateEntity entity, CancellationToken cancellationToken = default)
    {
        _context.PromptTemplates.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FindAsync(id);
        if (entity != null)
        {
            _context.PromptTemplates.Remove(entity);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
        return false;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.PromptTemplates.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> FindAsync(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await GetAllAsync(cancellationToken);
        return entities.Where(predicate.Compile()).ToList();
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> FirstOrDefaultAsync(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        return entities.FirstOrDefault();
    }

    public async Task<bool> AnyAsync(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        return entities.Any();
    }

    public async Task<int> CountAsync(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
        {
            return await _context.PromptTemplates.CountAsync(cancellationToken);
        }
        var entities = await FindAsync(predicate, cancellationToken);
        return entities.Count;
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetPagedAsync(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>> predicate, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        return entities.Skip((page - 1) * pageSize).Take(pageSize).ToList();
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, TOrderBy>> orderBy, bool descending = false, CancellationToken cancellationToken = default)
    {
        var entities = await GetAllAsync(cancellationToken);
        return descending ? entities.OrderByDescending(orderBy.Compile()).ToList() : entities.OrderBy(orderBy.Compile()).ToList();
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>> predicate, Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, TOrderBy>> orderBy, bool descending = false, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        return descending ? entities.OrderByDescending(orderBy.Compile()).ToList() : entities.OrderBy(orderBy.Compile()).ToList();
    }

    public async Task<int> BulkInsertAsync(IEnumerable<BIReportingCopilot.Core.Models.PromptTemplateEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.PromptTemplates.AddRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<BIReportingCopilot.Core.Models.PromptTemplateEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.PromptTemplates.UpdateRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkDeleteAsync(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await FindAsync(predicate, cancellationToken);
        _context.PromptTemplates.RemoveRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetByIdWithIncludesAsync(long id, params Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, object>>[] includes)
    {
        return await GetByIdAsync(id, default);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> FindWithIncludesAsync(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>> predicate, params Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, object>>[] includes)
    {
        return await FindAsync(predicate, default);
    }

    public IAsyncEnumerable<BIReportingCopilot.Core.Models.PromptTemplateEntity> GetAsyncEnumerable(Expression<Func<BIReportingCopilot.Core.Models.PromptTemplateEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.PromptTemplates.AsNoTracking();
        return query.Select(e => _mapper.Map<BIReportingCopilot.Core.Models.PromptTemplateEntity>(e)).AsAsyncEnumerable();
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

    #region IEnhancedPromptTemplateRepository Implementation

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.AsNoTracking()
            .FirstOrDefaultAsync(x => x.TemplateKey == templateKey, cancellationToken);
        return entity != null ? _mapper.Map<BIReportingCopilot.Core.Models.PromptTemplateEntity>(entity) : null;
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IsActive)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IntentType == intentType)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetByPriorityRangeAsync(int minPriority, int maxPriority, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.Priority >= minPriority && x.Priority <= maxPriority)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> SearchByTagsAsync(List<string> tags, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => tags.Any(tag => x.Tags != null && x.Tags.Contains(tag)))
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.Name.Contains(searchTerm))
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetWithPerformanceMetricsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetWithABTestsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetWithImprovementSuggestionsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetWithAllRelationshipsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> SelectTemplateForUserAsync(string intentType, string userId, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IntentType == intentType && x.IsActive)
            .OrderByDescending(x => x.Priority)
            .ToListAsync(cancellationToken);
        var coreEntities = _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
        return coreEntities.FirstOrDefault();
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetTemplatesEligibleForTestingAsync(string intentType, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IntentType == intentType && x.IsActive)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetRandomTemplateByIntentAsync(string intentType, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.IntentType == intentType && x.IsActive)
            .ToListAsync(cancellationToken);
        var coreEntities = _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
        if (coreEntities.Any())
        {
            var random = new Random();
            return coreEntities[random.Next(coreEntities.Count)];
        }
        return null;
    }

    public async Task<Dictionary<string, int>> GetTemplateCountsByIntentTypeAsync(CancellationToken cancellationToken = default)
    {
        var results = await _context.PromptTemplates.AsNoTracking()
            .GroupBy(x => x.IntentType)
            .Select(g => new { IntentType = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return results.ToDictionary(x => x.IntentType, x => x.Count);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetMostUsedTemplatesAsync(int count, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .OrderByDescending(x => x.UsageCount)
            .Take(count)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetRecentlyUpdatedTemplatesAsync(int days, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.UpdatedDate >= cutoffDate)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetTemplatesNeedingReviewAsync(CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-90); // Templates not reviewed in 90 days
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.LastBusinessReviewDate == null || x.LastBusinessReviewDate < cutoffDate)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task ActivateTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.IsActive = true;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task DeactivateTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateUsageCountAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.UsageCount++;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateSuccessRateAsync(long templateId, decimal successRate, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.SuccessRate = successRate;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task UpdateLastBusinessReviewAsync(long templateId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.FirstOrDefaultAsync(x => x.Id == templateId, cancellationToken);
        if (entity != null)
        {
            entity.LastBusinessReviewDate = DateTime.UtcNow;
            entity.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> GetTemplateVersionsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var entities = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.TemplateKey == templateKey)
            .OrderByDescending(x => x.Version)
            .ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<BIReportingCopilot.Core.Models.PromptTemplateEntity?> GetLatestVersionAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.TemplateKey == templateKey)
            .OrderByDescending(x => x.Version)
            .FirstOrDefaultAsync(cancellationToken);
        return entity != null ? _mapper.Map<BIReportingCopilot.Core.Models.PromptTemplateEntity>(entity) : null;
    }

    public async Task<string> GenerateNextVersionAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var latestVersion = await _context.PromptTemplates.AsNoTracking()
            .Where(x => x.TemplateKey == templateKey)
            .MaxAsync(x => (string?)x.Version, cancellationToken) ?? "1.0";

        // Simple version increment logic
        if (decimal.TryParse(latestVersion, out var version))
        {
            return (version + 0.1m).ToString("F1");
        }

        return "1.0";
    }

    public async Task<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>> SearchTemplatesAsync(BIReportingCopilot.Core.Models.TemplateSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _context.PromptTemplates.AsQueryable();

        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            query = query.Where(t => t.Name.Contains(criteria.SearchTerm) ||
                                   t.Description.Contains(criteria.SearchTerm) ||
                                   t.Content.Contains(criteria.SearchTerm));
        }

        if (!string.IsNullOrEmpty(criteria.IntentType))
        {
            query = query.Where(t => t.IntentType == criteria.IntentType);
        }

        if (criteria.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == criteria.IsActive.Value);
        }

        if (criteria.CreatedAfter.HasValue)
        {
            query = query.Where(t => t.CreatedDate >= criteria.CreatedAfter.Value);
        }

        if (criteria.CreatedBefore.HasValue)
        {
            query = query.Where(t => t.CreatedDate <= criteria.CreatedBefore.Value);
        }

        if (!string.IsNullOrEmpty(criteria.CreatedBy))
        {
            query = query.Where(t => t.CreatedBy == criteria.CreatedBy);
        }

        // Apply sorting
        if (!string.IsNullOrEmpty(criteria.SortBy))
        {
            switch (criteria.SortBy.ToLower())
            {
                case "name":
                    query = criteria.SortDescending ? query.OrderByDescending(t => t.Name) : query.OrderBy(t => t.Name);
                    break;
                case "created":
                    query = criteria.SortDescending ? query.OrderByDescending(t => t.CreatedDate) : query.OrderBy(t => t.CreatedDate);
                    break;
                case "updated":
                    query = criteria.SortDescending ? query.OrderByDescending(t => t.UpdatedDate) : query.OrderBy(t => t.UpdatedDate);
                    break;
                default:
                    query = query.OrderByDescending(t => t.CreatedDate);
                    break;
            }
        }
        else
        {
            query = query.OrderByDescending(t => t.CreatedDate);
        }

        // Apply pagination
        query = query.Skip((criteria.Page - 1) * criteria.PageSize).Take(criteria.PageSize);

        var entities = await query.ToListAsync(cancellationToken);
        return _mapper.Map<List<BIReportingCopilot.Core.Models.PromptTemplateEntity>>(entities);
    }

    public async Task<int> GetSearchCountAsync(BIReportingCopilot.Core.Models.TemplateSearchCriteria criteria, CancellationToken cancellationToken = default)
    {
        var query = _context.PromptTemplates.AsQueryable();

        if (!string.IsNullOrEmpty(criteria.SearchTerm))
        {
            query = query.Where(t => t.Name.Contains(criteria.SearchTerm) ||
                                   t.Description.Contains(criteria.SearchTerm) ||
                                   t.Content.Contains(criteria.SearchTerm));
        }

        if (!string.IsNullOrEmpty(criteria.IntentType))
        {
            query = query.Where(t => t.IntentType == criteria.IntentType);
        }

        if (criteria.IsActive.HasValue)
        {
            query = query.Where(t => t.IsActive == criteria.IsActive.Value);
        }

        if (criteria.CreatedAfter.HasValue)
        {
            query = query.Where(t => t.CreatedDate >= criteria.CreatedAfter.Value);
        }

        if (criteria.CreatedBefore.HasValue)
        {
            query = query.Where(t => t.CreatedDate <= criteria.CreatedBefore.Value);
        }

        if (!string.IsNullOrEmpty(criteria.CreatedBy))
        {
            query = query.Where(t => t.CreatedBy == criteria.CreatedBy);
        }

        return await query.CountAsync(cancellationToken);
    }

    #endregion

}
