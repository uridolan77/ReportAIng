using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for A/B testing operations
/// </summary>
public class TemplateABTestRepository : BaseRepository<TemplateABTestEntity>, ITemplateABTestRepository
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(10);

    public TemplateABTestRepository(
        BICopilotContext context, 
        ILogger<TemplateABTestRepository> logger,
        IMemoryCache cache) : base(context, logger)
    {
        _cache = cache;
    }

    protected override long GetEntityId(TemplateABTestEntity entity) => entity.Id;
    protected override Expression<Func<TemplateABTestEntity, bool>> GetByIdPredicate(long id) => x => x.Id == id;

    #region Test management queries

    public async Task<List<TemplateABTestEntity>> GetActiveTestsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "active_ab_tests";
        
        if (_cache.TryGetValue<List<TemplateABTestEntity>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var tests = await _dbSet.AsNoTracking()
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Include(x => x.WinnerTemplate)
                .Where(x => x.Status == "active")
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, tests, _cacheExpiry);
            return tests;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active A/B tests");
            throw;
        }
    }

    public async Task<List<TemplateABTestEntity>> GetTestsByStatusAsync(string status, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Include(x => x.WinnerTemplate)
                .Where(x => x.Status == status)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B tests by status: {Status}", status);
            throw;
        }
    }

    public async Task<List<TemplateABTestEntity>> GetTestsByTemplateIdAsync(long templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Include(x => x.WinnerTemplate)
                .Where(x => x.OriginalTemplateId == templateId || x.VariantTemplateId == templateId)
                .OrderByDescending(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B tests by template ID: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<TemplateABTestEntity?> GetActiveTestForTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Include(x => x.WinnerTemplate)
                .FirstOrDefaultAsync(x => 
                    (x.OriginalTemplateId == templateId || x.VariantTemplateId == templateId) && 
                    x.Status == "active", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active A/B test for template ID: {TemplateId}", templateId);
            throw;
        }
    }

    public async Task<TemplateABTestEntity?> GetByTestNameAsync(string testName, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Include(x => x.WinnerTemplate)
                .FirstOrDefaultAsync(x => x.TestName == testName, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B test by name: {TestName}", testName);
            throw;
        }
    }

    #endregion

    #region Test analytics

    public async Task<List<TemplateABTestEntity>> GetCompletedTestsAsync(DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var query = _dbSet.AsNoTracking()
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Include(x => x.WinnerTemplate)
                .Where(x => x.Status == "completed");

            if (startDate.HasValue)
            {
                query = query.Where(x => x.CompletedDate >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(x => x.CompletedDate <= endDate.Value);
            }

            return await query
                .OrderByDescending(x => x.CompletedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting completed A/B tests");
            throw;
        }
    }

    public async Task<List<TemplateABTestEntity>> GetTestsRequiringAnalysisAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-7); // Tests running for at least a week
            
            return await _dbSet.AsNoTracking()
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Where(x => x.Status == "active" && 
                           x.StartDate <= cutoffDate &&
                           !x.StatisticalSignificance.HasValue)
                .OrderBy(x => x.StartDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting A/B tests requiring analysis");
            throw;
        }
    }

    public async Task<List<TemplateABTestEntity>> GetExpiredTestsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var now = DateTime.UtcNow;
            
            return await _dbSet.AsNoTracking()
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Where(x => x.Status == "active" && 
                           x.EndDate.HasValue && 
                           x.EndDate.Value <= now)
                .OrderBy(x => x.EndDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting expired A/B tests");
            throw;
        }
    }

    public async Task<Dictionary<string, int>> GetTestCountsByStatusAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "ab_test_counts_by_status";
        
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
            _logger.LogError(ex, "Error getting A/B test counts by status");
            throw;
        }
    }

    #endregion

    #region Statistical operations

    public async Task UpdateTestResultsAsync(long testId, decimal originalSuccessRate, decimal variantSuccessRate, 
        decimal statisticalSignificance, long? winnerTemplateId, string testResults, CancellationToken cancellationToken = default)
    {
        try
        {
            var test = await _dbSet.FirstOrDefaultAsync(x => x.Id == testId, cancellationToken);
            
            if (test == null)
            {
                _logger.LogWarning("A/B test not found for ID: {TestId}", testId);
                return;
            }

            test.OriginalSuccessRate = originalSuccessRate;
            test.VariantSuccessRate = variantSuccessRate;
            test.StatisticalSignificance = statisticalSignificance;
            test.WinnerTemplateId = winnerTemplateId;
            test.TestResults = testResults;
            test.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove("active_ab_tests");
            _cache.Remove("ab_test_counts_by_status");

            _logger.LogInformation("Updated A/B test results for test {TestId}", testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating A/B test results for test {TestId}", testId);
            throw;
        }
    }

    public async Task CompleteTestAsync(long testId, long winnerTemplateId, CancellationToken cancellationToken = default)
    {
        try
        {
            var test = await _dbSet.FirstOrDefaultAsync(x => x.Id == testId, cancellationToken);
            
            if (test == null)
            {
                _logger.LogWarning("A/B test not found for ID: {TestId}", testId);
                return;
            }

            test.Status = "completed";
            test.WinnerTemplateId = winnerTemplateId;
            test.CompletedDate = DateTime.UtcNow;
            test.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove("active_ab_tests");
            _cache.Remove("ab_test_counts_by_status");

            _logger.LogInformation("Completed A/B test {TestId} with winner template {WinnerTemplateId}", testId, winnerTemplateId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error completing A/B test {TestId}", testId);
            throw;
        }
    }

    public async Task PauseTestAsync(long testId, CancellationToken cancellationToken = default)
    {
        try
        {
            var test = await _dbSet.FirstOrDefaultAsync(x => x.Id == testId, cancellationToken);
            
            if (test == null)
            {
                _logger.LogWarning("A/B test not found for ID: {TestId}", testId);
                return;
            }

            test.Status = "paused";
            test.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove("active_ab_tests");
            _cache.Remove("ab_test_counts_by_status");

            _logger.LogInformation("Paused A/B test {TestId}", testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error pausing A/B test {TestId}", testId);
            throw;
        }
    }

    public async Task ResumeTestAsync(long testId, CancellationToken cancellationToken = default)
    {
        try
        {
            var test = await _dbSet.FirstOrDefaultAsync(x => x.Id == testId, cancellationToken);
            
            if (test == null)
            {
                _logger.LogWarning("A/B test not found for ID: {TestId}", testId);
                return;
            }

            test.Status = "active";
            test.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove("active_ab_tests");
            _cache.Remove("ab_test_counts_by_status");

            _logger.LogInformation("Resumed A/B test {TestId}", testId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resuming A/B test {TestId}", testId);
            throw;
        }
    }

    public async Task CancelTestAsync(long testId, string reason, CancellationToken cancellationToken = default)
    {
        try
        {
            var test = await _dbSet.FirstOrDefaultAsync(x => x.Id == testId, cancellationToken);
            
            if (test == null)
            {
                _logger.LogWarning("A/B test not found for ID: {TestId}", testId);
                return;
            }

            test.Status = "cancelled";
            test.TestResults = $"Cancelled: {reason}";
            test.CompletedDate = DateTime.UtcNow;
            test.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            _cache.Remove("active_ab_tests");
            _cache.Remove("ab_test_counts_by_status");

            _logger.LogInformation("Cancelled A/B test {TestId}: {Reason}", testId, reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling A/B test {TestId}", testId);
            throw;
        }
    }

    #endregion

    #region Test validation

    public async Task<bool> HasActiveTestForTemplateAsync(long templateId, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .AnyAsync(x => 
                    (x.OriginalTemplateId == templateId || x.VariantTemplateId == templateId) && 
                    x.Status == "active", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if template {TemplateId} has active test", templateId);
            throw;
        }
    }

    public async Task<bool> CanCreateTestAsync(long originalTemplateId, long variantTemplateId, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if either template is already in an active test
            var hasActiveTest = await _dbSet.AsNoTracking()
                .AnyAsync(x => 
                    (x.OriginalTemplateId == originalTemplateId || x.VariantTemplateId == originalTemplateId ||
                     x.OriginalTemplateId == variantTemplateId || x.VariantTemplateId == variantTemplateId) && 
                    x.Status == "active", cancellationToken);

            return !hasActiveTest;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if A/B test can be created for templates {OriginalTemplateId} and {VariantTemplateId}", 
                originalTemplateId, variantTemplateId);
            throw;
        }
    }

    #endregion
}
