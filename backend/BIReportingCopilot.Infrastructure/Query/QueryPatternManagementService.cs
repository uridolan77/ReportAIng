using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Service responsible for managing query patterns and templates
/// </summary>
public class QueryPatternManagementService : IQueryPatternManagementService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<QueryPatternManagementService> _logger;

    public QueryPatternManagementService(
        BICopilotContext context,
        ILogger<QueryPatternManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Query Pattern Operations

    public async Task<List<QueryPatternDto>> GetQueryPatternsAsync()
    {
        try
        {
            var patterns = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.PatternName)
                .ToListAsync();

            return patterns.Select(MapPatternToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query patterns");
            throw;
        }
    }

    public async Task<QueryPatternDto?> GetQueryPatternAsync(long id)
    {
        try
        {
            var pattern = await _context.QueryPatterns
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            return pattern != null ? MapPatternToDto(pattern) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query pattern {PatternId}", id);
            throw;
        }
    }

    public async Task<QueryPatternDto> CreateQueryPatternAsync(CreateQueryPatternRequest request, string userId)
    {
        try
        {
            var entity = new QueryPatternEntity
            {
                PatternName = request.PatternName,
                NaturalLanguagePattern = request.NaturalLanguagePattern,
                SqlTemplate = request.SqlTemplate,
                Description = request.Description,
                BusinessContext = request.BusinessContext,
                Keywords = JsonSerializer.Serialize(request.Keywords),
                RequiredTables = JsonSerializer.Serialize(request.RequiredTables),
                Priority = request.Priority,
                IsActive = true,
                UsageCount = 0,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.QueryPatterns.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created query pattern: {PatternName}", entity.PatternName);
            return MapPatternToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating query pattern");
            throw;
        }
    }

    public async Task<QueryPatternDto?> UpdateQueryPatternAsync(long id, CreateQueryPatternRequest request, string userId)
    {
        try
        {
            var entity = await _context.QueryPatterns
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (entity == null)
                return null;

            entity.PatternName = request.PatternName;
            entity.NaturalLanguagePattern = request.NaturalLanguagePattern;
            entity.SqlTemplate = request.SqlTemplate;
            entity.Description = request.Description;
            entity.BusinessContext = request.BusinessContext;
            entity.Keywords = JsonSerializer.Serialize(request.Keywords);
            entity.RequiredTables = JsonSerializer.Serialize(request.RequiredTables);
            entity.Priority = request.Priority;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated query pattern: {PatternName}", entity.PatternName);
            return MapPatternToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating query pattern {PatternId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteQueryPatternAsync(long id)
    {
        try
        {
            var entity = await _context.QueryPatterns.FindAsync(id);
            if (entity == null || !entity.IsActive)
                return false;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted query pattern {PatternId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting query pattern {PatternId}", id);
            throw;
        }
    }

    public async Task<string> TestQueryPatternAsync(long id, string naturalLanguageQuery)
    {
        try
        {
            var pattern = await _context.QueryPatterns
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);

            if (pattern == null)
                return "Pattern not found";

            // Simple pattern matching test
            var keywords = JsonSerializer.Deserialize<List<string>>(pattern.Keywords) ?? new List<string>();
            var matchedKeywords = keywords.Where(k => naturalLanguageQuery.ToLower().Contains(k.ToLower())).ToList();

            var result = $"Pattern: {pattern.PatternName}\n";
            result += $"Matched Keywords: {string.Join(", ", matchedKeywords)}\n";
            result += $"Match Score: {matchedKeywords.Count}/{keywords.Count}\n";
            result += $"SQL Template:\n{pattern.SqlTemplate}";

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing query pattern {PatternId}", id);
            throw;
        }
    }

    public async Task<QueryPatternStatistics> GetPatternStatisticsAsync()
    {
        try
        {
            var patterns = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .Select(p => new { p.PatternName, p.UsageCount, p.Priority })
                .ToListAsync();

            var mostUsedPatterns = patterns
                .OrderByDescending(p => p.UsageCount)
                .Take(5)
                .Select(p => p.PatternName)
                .ToList();

            var patternUsageStats = patterns
                .GroupBy(p => p.Priority)
                .ToDictionary(g => $"Priority {g.Key}", g => g.Count());

            return new QueryPatternStatistics
            {
                TotalPatterns = patterns.Count,
                MostUsedPatterns = mostUsedPatterns,
                PatternUsageStats = patternUsageStats,
                AverageUsageCount = patterns.Count > 0 ? patterns.Average(p => p.UsageCount) : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting pattern statistics");
            throw;
        }
    }

    public async Task IncrementPatternUsageAsync(long patternId)
    {
        try
        {
            var pattern = await _context.QueryPatterns.FindAsync(patternId);
            if (pattern != null && pattern.IsActive)
            {
                pattern.UsageCount++;
                pattern.LastUsedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                
                _logger.LogDebug("Incremented usage count for pattern {PatternId} to {UsageCount}", 
                    patternId, pattern.UsageCount);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error incrementing pattern usage for {PatternId}", patternId);
            // Don't throw - this is not critical
        }
    }

    #endregion

    #region Helper Methods

    private static QueryPatternDto MapPatternToDto(QueryPatternEntity entity)
    {
        var keywords = JsonSerializer.Deserialize<List<string>>(entity.Keywords) ?? new List<string>();
        var requiredTables = JsonSerializer.Deserialize<List<string>>(entity.RequiredTables) ?? new List<string>();

        return new QueryPatternDto
        {
            Id = entity.Id,
            PatternName = entity.PatternName,
            NaturalLanguagePattern = entity.NaturalLanguagePattern,
            SqlTemplate = entity.SqlTemplate,
            Description = entity.Description,
            BusinessContext = entity.BusinessContext,
            Keywords = keywords,
            RequiredTables = requiredTables,
            Priority = entity.Priority,
            IsActive = entity.IsActive,
            UsageCount = entity.UsageCount,
            LastUsedDate = entity.LastUsedDate
        };
    }

    #endregion
}
