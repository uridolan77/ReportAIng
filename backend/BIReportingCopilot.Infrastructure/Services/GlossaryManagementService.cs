using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Service responsible for managing business glossary terms and definitions
/// </summary>
public class GlossaryManagementService : IGlossaryManagementService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<GlossaryManagementService> _logger;

    public GlossaryManagementService(
        BICopilotContext context,
        ILogger<GlossaryManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Glossary Operations

    public async Task<List<BusinessGlossaryDto>> GetGlossaryTermsAsync()
    {
        try
        {
            var terms = await _context.BusinessGlossary
                .Where(g => g.IsActive)
                .OrderBy(g => g.Category)
                .ThenBy(g => g.Term)
                .ToListAsync();

            return terms.Select(MapGlossaryToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary terms");
            throw;
        }
    }

    public async Task<List<BusinessGlossaryDto>> GetGlossaryTermsByCategoryAsync(string category)
    {
        try
        {
            var terms = await _context.BusinessGlossary
                .Where(g => g.IsActive && g.Category == category)
                .OrderBy(g => g.Term)
                .ToListAsync();

            return terms.Select(MapGlossaryToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary terms for category {Category}", category);
            throw;
        }
    }

    public async Task<BusinessGlossaryDto?> GetGlossaryTermAsync(long id)
    {
        try
        {
            var term = await _context.BusinessGlossary
                .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

            return term != null ? MapGlossaryToDto(term) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary term {TermId}", id);
            throw;
        }
    }

    public async Task<BusinessGlossaryDto?> FindGlossaryTermByNameAsync(string termName)
    {
        try
        {
            var term = await _context.BusinessGlossary
                .FirstOrDefaultAsync(g => g.Term.ToLower() == termName.ToLower() && g.IsActive);

            return term != null ? MapGlossaryToDto(term) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding glossary term by name {TermName}", termName);
            throw;
        }
    }

    public async Task<BusinessGlossaryDto> CreateGlossaryTermAsync(BusinessGlossaryDto request, string userId)
    {
        try
        {
            var entity = new BusinessGlossaryEntity
            {
                Term = request.Term,
                Definition = request.Definition,
                BusinessContext = request.BusinessContext,
                Synonyms = JsonSerializer.Serialize(request.Synonyms),
                RelatedTerms = JsonSerializer.Serialize(request.RelatedTerms),
                Category = request.Category,
                IsActive = true,
                UsageCount = 0,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.BusinessGlossary.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created glossary term: {Term}", entity.Term);
            return MapGlossaryToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating glossary term");
            throw;
        }
    }

    public async Task<BusinessGlossaryDto?> UpdateGlossaryTermAsync(long id, BusinessGlossaryDto request, string userId)
    {
        try
        {
            var entity = await _context.BusinessGlossary
                .FirstOrDefaultAsync(g => g.Id == id && g.IsActive);

            if (entity == null)
                return null;

            entity.Term = request.Term;
            entity.Definition = request.Definition;
            entity.BusinessContext = request.BusinessContext;
            entity.Synonyms = JsonSerializer.Serialize(request.Synonyms);
            entity.RelatedTerms = JsonSerializer.Serialize(request.RelatedTerms);
            entity.Category = request.Category;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated glossary term: {Term}", entity.Term);
            return MapGlossaryToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating glossary term {TermId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteGlossaryTermAsync(long id)
    {
        try
        {
            var entity = await _context.BusinessGlossary.FindAsync(id);
            if (entity == null || !entity.IsActive)
                return false;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted glossary term {TermId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting glossary term {TermId}", id);
            throw;
        }
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        try
        {
            return await _context.BusinessGlossary
                .Where(g => g.IsActive && !string.IsNullOrEmpty(g.Category))
                .Select(g => g.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary categories");
            throw;
        }
    }

    public async Task<List<BusinessGlossaryDto>> SearchGlossaryTermsAsync(string searchTerm)
    {
        try
        {
            var terms = await _context.BusinessGlossary
                .Where(g => g.IsActive && 
                    (g.Term.Contains(searchTerm) || 
                     g.Definition.Contains(searchTerm) ||
                     g.BusinessContext.Contains(searchTerm)))
                .OrderBy(g => g.Term)
                .ToListAsync();

            return terms.Select(MapGlossaryToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching glossary terms with {SearchTerm}", searchTerm);
            throw;
        }
    }

    public async Task<GlossaryStatistics> GetGlossaryStatisticsAsync()
    {
        try
        {
            var terms = await _context.BusinessGlossary
                .Where(g => g.IsActive)
                .Select(g => new { g.Category, g.UsageCount })
                .ToListAsync();

            var categoryStats = terms
                .GroupBy(t => t.Category ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Count());

            return new GlossaryStatistics
            {
                TotalTerms = terms.Count,
                CategoryStats = categoryStats,
                AverageUsageCount = terms.Count > 0 ? terms.Average(t => t.UsageCount) : 0,
                TotalCategories = categoryStats.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting glossary statistics");
            throw;
        }
    }

    public async Task IncrementTermUsageAsync(long termId)
    {
        try
        {
            var term = await _context.BusinessGlossary.FindAsync(termId);
            if (term != null && term.IsActive)
            {
                term.UsageCount++;
                term.LastUsedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Incremented usage count for term {TermId} to {UsageCount}", 
                    termId, term.UsageCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error incrementing term usage for {TermId}", termId);
            // Don't throw - this is not critical
        }
    }

    #endregion

    #region Helper Methods

    private static BusinessGlossaryDto MapGlossaryToDto(BusinessGlossaryEntity entity)
    {
        var synonyms = JsonSerializer.Deserialize<List<string>>(entity.Synonyms) ?? new List<string>();
        var relatedTerms = JsonSerializer.Deserialize<List<string>>(entity.RelatedTerms) ?? new List<string>();

        return new BusinessGlossaryDto
        {
            Id = entity.Id,
            Term = entity.Term,
            Definition = entity.Definition,
            BusinessContext = entity.BusinessContext,
            Synonyms = synonyms,
            RelatedTerms = relatedTerms,
            Category = entity.Category,
            IsActive = entity.IsActive,
            UsageCount = entity.UsageCount,
            LastUsedDate = entity.LastUsedDate
        };
    }

    #endregion
}
