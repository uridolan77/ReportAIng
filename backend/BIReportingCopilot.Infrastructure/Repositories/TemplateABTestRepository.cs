using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using AutoMapper;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using System.Linq.Expressions;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for A/B testing operations
/// </summary>
public class TemplateABTestRepository : ITemplateABTestRepository
{
    private readonly BICopilotContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<TemplateABTestRepository> _logger;

    public TemplateABTestRepository(BICopilotContext context, IMapper mapper, ILogger<TemplateABTestRepository> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    #region IRepository Implementation

    public async Task<TemplateABTestEntity?> GetByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TemplateABTestEntity> CreateAsync(TemplateABTestEntity entity, CancellationToken cancellationToken = default)
    {
        _context.TemplateABTests.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<TemplateABTestEntity> UpdateAsync(TemplateABTestEntity entity, CancellationToken cancellationToken = default)
    {
        _context.TemplateABTests.Update(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity;
    }

    public async Task<bool> DeleteAsync(long id, CancellationToken cancellationToken = default)
    {
        var entity = await _context.TemplateABTests.FindAsync(new object[] { id }, cancellationToken);
        if (entity == null) return false;

        _context.TemplateABTests.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsAsync(long id, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AnyAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> FindAsync(Expression<Func<TemplateABTestEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(predicate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TemplateABTestEntity?> FirstOrDefaultAsync(Expression<Func<TemplateABTestEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .FirstOrDefaultAsync(predicate, cancellationToken);
    }

    public async Task<bool> AnyAsync(Expression<Func<TemplateABTestEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AnyAsync(predicate, cancellationToken);
    }

    public async Task<int> CountAsync(Expression<Func<TemplateABTestEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        if (predicate == null)
            return await _context.TemplateABTests.CountAsync(cancellationToken);
        return await _context.TemplateABTests.CountAsync(predicate, cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetPagedAsync(Expression<Func<TemplateABTestEntity, bool>> predicate, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(predicate)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<TemplateABTestEntity, TOrderBy>> orderBy, bool ascending = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TemplateABTestEntity> query = _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetOrderedAsync<TOrderBy>(Expression<Func<TemplateABTestEntity, bool>> predicate, Expression<Func<TemplateABTestEntity, TOrderBy>> orderBy, bool ascending = true, CancellationToken cancellationToken = default)
    {
        IQueryable<TemplateABTestEntity> query = _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(predicate);
        query = ascending ? query.OrderBy(orderBy) : query.OrderByDescending(orderBy);
        return await query.ToListAsync(cancellationToken);
    }

    public async Task<int> BulkInsertAsync(IEnumerable<TemplateABTestEntity> entities, CancellationToken cancellationToken = default)
    {
        await _context.TemplateABTests.AddRangeAsync(entities, cancellationToken);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkUpdateAsync(IEnumerable<TemplateABTestEntity> entities, CancellationToken cancellationToken = default)
    {
        _context.TemplateABTests.UpdateRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> BulkDeleteAsync(Expression<Func<TemplateABTestEntity, bool>> predicate, CancellationToken cancellationToken = default)
    {
        var entities = await _context.TemplateABTests.Where(predicate).ToListAsync(cancellationToken);
        _context.TemplateABTests.RemoveRange(entities);
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<TemplateABTestEntity?> GetByIdWithIncludesAsync(long id, params Expression<Func<TemplateABTestEntity, object>>[] includes)
    {
        var query = _context.TemplateABTests.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<List<TemplateABTestEntity>> FindWithIncludesAsync(Expression<Func<TemplateABTestEntity, bool>> predicate, params Expression<Func<TemplateABTestEntity, object>>[] includes)
    {
        var query = _context.TemplateABTests.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }
        return await query.Where(predicate).ToListAsync();
    }

    public IAsyncEnumerable<TemplateABTestEntity> GetAsyncEnumerable(Expression<Func<TemplateABTestEntity, bool>>? predicate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplateABTests.AsNoTracking();
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

    #region ITemplateABTestRepository Implementation

    public async Task<List<TemplateABTestEntity>> GetActiveTestsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(x => x.Status == "active")
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetTestsByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(x => x.Status == status)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetTestsByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(x => x.OriginalTemplateId == templateId || x.VariantTemplateId == templateId)
            .ToListAsync(cancellationToken);
    }

    public async Task<TemplateABTestEntity?> GetActiveTestForTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .FirstOrDefaultAsync(x => (x.OriginalTemplateId == templateId || x.VariantTemplateId == templateId) && x.Status == "active", cancellationToken);
    }

    public async Task<TemplateABTestEntity?> GetByTestNameAsync(string testName, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .FirstOrDefaultAsync(x => x.TestName == testName, cancellationToken);
    }

    // Test analytics
    public async Task<List<TemplateABTestEntity>> GetCompletedTestsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        var query = _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(x => x.Status == "completed");

        if (startDate.HasValue)
            query = query.Where(x => x.EndDate >= startDate.Value);
        if (endDate.HasValue)
            query = query.Where(x => x.EndDate <= endDate.Value);

        return await query.ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetTestsRequiringAnalysisAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(x => x.Status == "active" && x.StatisticalSignificance == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<TemplateABTestEntity>> GetExpiredTestsAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await _context.TemplateABTests.AsNoTracking()
            .Include(x => x.OriginalTemplate)
            .Include(x => x.VariantTemplate)
            .Where(x => x.Status == "active" && x.EndDate.HasValue && x.EndDate.Value < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<string, int>> GetTestCountsByStatusAsync(CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AsNoTracking()
            .GroupBy(x => x.Status)
            .ToDictionaryAsync(g => g.Key, g => g.Count(), cancellationToken);
    }

    // Statistical operations
    public async Task UpdateTestResultsAsync(long testId, decimal originalSuccessRate, decimal variantSuccessRate,
        decimal statisticalSignificance, long? winnerTemplateId, string testResults, CancellationToken cancellationToken = default)
    {
        var test = await _context.TemplateABTests.FindAsync(new object[] { testId }, cancellationToken);
        if (test != null)
        {
            test.OriginalSuccessRate = originalSuccessRate;
            test.VariantSuccessRate = variantSuccessRate;
            test.StatisticalSignificance = statisticalSignificance;
            test.WinnerTemplateId = winnerTemplateId;
            test.TestResults = testResults;
            test.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CompleteTestAsync(long testId, long winnerTemplateId, CancellationToken cancellationToken = default)
    {
        var test = await _context.TemplateABTests.FindAsync(new object[] { testId }, cancellationToken);
        if (test != null)
        {
            test.Status = "completed";
            test.WinnerTemplateId = winnerTemplateId;
            test.EndDate = DateTime.UtcNow;
            test.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task PauseTestAsync(long testId, CancellationToken cancellationToken = default)
    {
        var test = await _context.TemplateABTests.FindAsync(new object[] { testId }, cancellationToken);
        if (test != null)
        {
            test.Status = "paused";
            test.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task ResumeTestAsync(long testId, CancellationToken cancellationToken = default)
    {
        var test = await _context.TemplateABTests.FindAsync(new object[] { testId }, cancellationToken);
        if (test != null)
        {
            test.Status = "active";
            test.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task CancelTestAsync(long testId, string reason, CancellationToken cancellationToken = default)
    {
        var test = await _context.TemplateABTests.FindAsync(new object[] { testId }, cancellationToken);
        if (test != null)
        {
            test.Status = "cancelled";
            test.EndDate = DateTime.UtcNow;
            test.UpdatedDate = DateTime.UtcNow;
            test.TestResults = $"Cancelled: {reason}";
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    // Test validation
    public async Task<bool> HasActiveTestForTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        return await _context.TemplateABTests.AnyAsync(
            x => (x.OriginalTemplateId == templateId || x.VariantTemplateId == templateId) && x.Status == "active",
            cancellationToken);
    }

    public async Task<bool> CanCreateTestAsync(long originalTemplateId, long variantTemplateId, CancellationToken cancellationToken = default)
    {
        // Check if either template is already in an active test
        var hasActiveTest = await _context.TemplateABTests.AnyAsync(
            x => (x.OriginalTemplateId == originalTemplateId || x.VariantTemplateId == originalTemplateId ||
                  x.OriginalTemplateId == variantTemplateId || x.VariantTemplateId == variantTemplateId) &&
                 x.Status == "active",
            cancellationToken);

        return !hasActiveTest;
    }

    #endregion
}
