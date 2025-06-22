using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Repository;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Enhanced repository implementation for prompt templates with template management features
/// </summary>
public class EnhancedPromptTemplateRepository : BaseRepository<PromptTemplateEntity>, IEnhancedPromptTemplateRepository
{
    private readonly IMemoryCache _cache;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromHours(1);
    private readonly Random _random = new();

    public EnhancedPromptTemplateRepository(
        BICopilotContext context, 
        ILogger<EnhancedPromptTemplateRepository> logger,
        IMemoryCache cache) : base(context, logger)
    {
        _cache = cache;
    }

    protected override long GetEntityId(PromptTemplateEntity entity) => entity.Id;
    protected override Expression<Func<PromptTemplateEntity, bool>> GetByIdPredicate(long id) => x => x.Id == id;

    #region Template-specific queries

    public async Task<PromptTemplateEntity?> GetByKeyAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"template_by_key_{templateKey}";
        
        if (_cache.TryGetValue<PromptTemplateEntity>(cacheKey, out var cached))
        {
            return cached;
        }

        try
        {
            var template = await _dbSet.AsNoTracking()
                .FirstOrDefaultAsync(x => x.TemplateKey == templateKey && x.IsActive, cancellationToken);

            if (template != null)
            {
                _cache.Set(cacheKey, template, _cacheExpiry);
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template by key: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> GetActiveTemplatesAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "active_templates";
        
        if (_cache.TryGetValue<List<PromptTemplateEntity>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var templates = await _dbSet.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderBy(x => x.IntentType)
                .ThenBy(x => x.Priority)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, templates, _cacheExpiry);
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active templates");
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> GetByIntentTypeAsync(string intentType, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"templates_by_intent_{intentType}";
        
        if (_cache.TryGetValue<List<PromptTemplateEntity>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var templates = await _dbSet.AsNoTracking()
                .Where(x => x.IntentType == intentType && x.IsActive)
                .OrderBy(x => x.Priority)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, templates, _cacheExpiry);
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates by intent type: {IntentType}", intentType);
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> GetByPriorityRangeAsync(int minPriority, int maxPriority, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Where(x => x.Priority >= minPriority && x.Priority <= maxPriority && x.IsActive)
                .OrderBy(x => x.Priority)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates by priority range: {MinPriority}-{MaxPriority}", minPriority, maxPriority);
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> SearchByTagsAsync(List<string> tags, CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = new List<PromptTemplateEntity>();
            
            foreach (var tag in tags)
            {
                var tagTemplates = await _dbSet.AsNoTracking()
                    .Where(x => x.Tags != null && x.Tags.Contains(tag) && x.IsActive)
                    .ToListAsync(cancellationToken);
                
                templates.AddRange(tagTemplates);
            }

            return templates.Distinct().OrderBy(x => x.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching templates by tags: {Tags}", string.Join(", ", tags));
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedSearch = searchTerm.ToLower();
            
            return await _dbSet.AsNoTracking()
                .Where(x => x.IsActive && 
                    (x.Name.ToLower().Contains(normalizedSearch) ||
                     x.BusinessFriendlyName != null && x.BusinessFriendlyName.ToLower().Contains(normalizedSearch) ||
                     x.Description != null && x.Description.ToLower().Contains(normalizedSearch)))
                .OrderBy(x => x.Name)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching templates by name: {SearchTerm}", searchTerm);
            throw;
        }
    }

    #endregion

    #region Template with relationships

    public async Task<PromptTemplateEntity?> GetWithPerformanceMetricsAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.PerformanceMetrics)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template with performance metrics: {Id}", id);
            throw;
        }
    }

    public async Task<PromptTemplateEntity?> GetWithABTestsAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.OriginalABTests)
                .Include(x => x.VariantABTests)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template with A/B tests: {Id}", id);
            throw;
        }
    }

    public async Task<PromptTemplateEntity?> GetWithImprovementSuggestionsAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.ImprovementSuggestions)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template with improvement suggestions: {Id}", id);
            throw;
        }
    }

    public async Task<PromptTemplateEntity?> GetWithAllRelationshipsAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Include(x => x.PerformanceMetrics)
                .Include(x => x.OriginalABTests)
                .Include(x => x.VariantABTests)
                .Include(x => x.ImprovementSuggestions)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template with all relationships: {Id}", id);
            throw;
        }
    }

    #endregion

    #region Template selection for A/B testing

    public async Task<PromptTemplateEntity?> SelectTemplateForUserAsync(string userId, string intentType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get active A/B tests for this intent type
            var activeTests = await _context.TemplateABTests
                .Include(x => x.OriginalTemplate)
                .Include(x => x.VariantTemplate)
                .Where(x => x.Status == "active" && 
                           (x.OriginalTemplate.IntentType == intentType || x.VariantTemplate.IntentType == intentType))
                .ToListAsync(cancellationToken);

            // If there are active tests, participate in A/B testing
            if (activeTests.Any())
            {
                var test = activeTests.First(); // Simple selection - could be enhanced with user-based selection
                var useVariant = _random.Next(100) < test.TrafficSplitPercent;
                
                return useVariant ? test.VariantTemplate : test.OriginalTemplate;
            }

            // No active tests, return best performing template
            var bestTemplate = await _dbSet.AsNoTracking()
                .Include(x => x.PerformanceMetrics)
                .Where(x => x.IntentType == intentType && x.IsActive)
                .OrderByDescending(x => x.SuccessRate)
                .ThenByDescending(x => x.UsageCount)
                .FirstOrDefaultAsync(cancellationToken);

            return bestTemplate;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error selecting template for user {UserId} with intent {IntentType}", userId, intentType);
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> GetTemplatesEligibleForTestingAsync(string intentType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Get templates that are not currently in active A/B tests
            var templatesInActiveTests = await _context.TemplateABTests
                .Where(x => x.Status == "active")
                .Select(x => new { x.OriginalTemplateId, x.VariantTemplateId })
                .ToListAsync(cancellationToken);

            var excludedIds = templatesInActiveTests
                .SelectMany(x => new[] { x.OriginalTemplateId, x.VariantTemplateId })
                .Distinct()
                .ToList();

            return await _dbSet.AsNoTracking()
                .Where(x => x.IntentType == intentType && 
                           x.IsActive && 
                           !excludedIds.Contains(x.Id) &&
                           x.UsageCount >= 10) // Minimum usage threshold
                .OrderByDescending(x => x.SuccessRate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates eligible for testing with intent: {IntentType}", intentType);
            throw;
        }
    }

    public async Task<PromptTemplateEntity?> GetRandomTemplateByIntentAsync(string intentType, CancellationToken cancellationToken = default)
    {
        try
        {
            var templates = await GetByIntentTypeAsync(intentType, cancellationToken);
            
            if (!templates.Any())
                return null;

            var randomIndex = _random.Next(templates.Count);
            return templates[randomIndex];
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting random template by intent: {IntentType}", intentType);
            throw;
        }
    }

    #endregion

    #region Template analytics

    public async Task<Dictionary<string, int>> GetTemplateCountsByIntentTypeAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "template_counts_by_intent";
        
        if (_cache.TryGetValue<Dictionary<string, int>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var results = await _dbSet.AsNoTracking()
                .Where(x => x.IsActive)
                .GroupBy(x => x.IntentType)
                .Select(g => new { IntentType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.IntentType ?? "Unknown", x => x.Count, cancellationToken);

            _cache.Set(cacheKey, results, _cacheExpiry);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template counts by intent type");
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> GetMostUsedTemplatesAsync(int count = 10, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"most_used_templates_{count}";
        
        if (_cache.TryGetValue<List<PromptTemplateEntity>>(cacheKey, out var cached))
        {
            return cached!;
        }

        try
        {
            var templates = await _dbSet.AsNoTracking()
                .Where(x => x.IsActive)
                .OrderByDescending(x => x.UsageCount)
                .Take(count)
                .ToListAsync(cancellationToken);

            _cache.Set(cacheKey, templates, _cacheExpiry);
            return templates;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting most used templates");
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> GetRecentlyUpdatedTemplatesAsync(int days = 30, CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        
        try
        {
            return await _dbSet.AsNoTracking()
                .Where(x => x.IsActive && x.UpdatedDate >= cutoffDate)
                .OrderByDescending(x => x.UpdatedDate)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recently updated templates for {Days} days", days);
            throw;
        }
    }

    public async Task<List<PromptTemplateEntity>> GetTemplatesNeedingReviewAsync(CancellationToken cancellationToken = default)
    {
        var reviewThreshold = DateTime.UtcNow.AddDays(-90); // Templates not reviewed in 90 days
        
        try
        {
            return await _dbSet.AsNoTracking()
                .Where(x => x.IsActive && 
                           (x.LastBusinessReview == null || x.LastBusinessReview < reviewThreshold))
                .OrderBy(x => x.LastBusinessReview)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting templates needing review");
            throw;
        }
    }

    #endregion

    #region Template management

    public async Task ActivateTemplateAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            if (template == null)
            {
                _logger.LogWarning("Template not found for activation: {Id}", id);
                return;
            }

            template.IsActive = true;
            template.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            InvalidateCache();

            _logger.LogInformation("Activated template {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating template {Id}", id);
            throw;
        }
    }

    public async Task DeactivateTemplateAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            if (template == null)
            {
                _logger.LogWarning("Template not found for deactivation: {Id}", id);
                return;
            }

            template.IsActive = false;
            template.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            InvalidateCache();

            _logger.LogInformation("Deactivated template {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating template {Id}", id);
            throw;
        }
    }

    public async Task UpdateUsageCountAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            if (template == null)
            {
                _logger.LogWarning("Template not found for usage count update: {Id}", id);
                return;
            }

            template.UsageCount++;
            template.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogDebug("Updated usage count for template {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating usage count for template {Id}", id);
            throw;
        }
    }

    public async Task UpdateSuccessRateAsync(long id, decimal successRate, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            if (template == null)
            {
                _logger.LogWarning("Template not found for success rate update: {Id}", id);
                return;
            }

            template.SuccessRate = successRate;
            template.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated success rate for template {Id}: {SuccessRate}", id, successRate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating success rate for template {Id}", id);
            throw;
        }
    }

    public async Task UpdateLastBusinessReviewAsync(long id, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await _dbSet.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
            
            if (template == null)
            {
                _logger.LogWarning("Template not found for business review update: {Id}", id);
                return;
            }

            template.LastBusinessReview = DateTime.UtcNow;
            template.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated last business review for template {Id}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last business review for template {Id}", id);
            throw;
        }
    }

    #endregion

    #region Template versioning

    public async Task<List<PromptTemplateEntity>> GetTemplateVersionsAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Where(x => x.TemplateKey == templateKey)
                .OrderByDescending(x => x.Version)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting template versions for key: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<PromptTemplateEntity?> GetLatestVersionAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _dbSet.AsNoTracking()
                .Where(x => x.TemplateKey == templateKey && x.IsActive)
                .OrderByDescending(x => x.Version)
                .FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting latest version for template key: {TemplateKey}", templateKey);
            throw;
        }
    }

    public async Task<string> GenerateNextVersionAsync(string templateKey, CancellationToken cancellationToken = default)
    {
        try
        {
            var latestVersion = await _dbSet.AsNoTracking()
                .Where(x => x.TemplateKey == templateKey)
                .OrderByDescending(x => x.Version)
                .Select(x => x.Version)
                .FirstOrDefaultAsync(cancellationToken);

            if (string.IsNullOrEmpty(latestVersion))
            {
                return "1.0";
            }

            // Simple version increment logic - could be enhanced
            if (decimal.TryParse(latestVersion, out var version))
            {
                return (version + 0.1m).ToString("F1");
            }

            return "1.0";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating next version for template key: {TemplateKey}", templateKey);
            throw;
        }
    }

    #endregion

    #region Private methods

    private void InvalidateCache()
    {
        _cache.Remove("active_templates");
        _cache.Remove("template_counts_by_intent");
        
        // Could be enhanced to remove specific intent-based caches
    }

    #endregion
}
