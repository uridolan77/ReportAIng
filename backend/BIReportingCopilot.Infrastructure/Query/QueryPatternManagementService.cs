using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models.Statistics;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Query;

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

    public async Task<BIReportingCopilot.Core.Models.Statistics.QueryPatternStatistics> GetPatternStatisticsAsync()
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

            return new BIReportingCopilot.Core.Models.Statistics.QueryPatternStatistics
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

    #region Missing Interface Method Implementations

    /// <summary>
    /// Get patterns async (IQueryPatternManagementService interface)
    /// </summary>
    public async Task<List<QueryPattern>> GetPatternsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📋 Getting query patterns");

            var patterns = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.PatternName)
                .ToListAsync(cancellationToken);

            return patterns.Select(MapEntityToPattern).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting query patterns");
            return new List<QueryPattern>();
        }
    }

    /// <summary>
    /// Get pattern by ID async (IQueryPatternManagementService interface)
    /// </summary>
    public async Task<QueryPattern?> GetPatternByIdAsync(string patternId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📋 Getting query pattern by ID: {PatternId}", patternId);

            if (!long.TryParse(patternId, out var id))
            {
                _logger.LogWarning("Invalid pattern ID format: {PatternId}", patternId);
                return null;
            }

            var pattern = await _context.QueryPatterns
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

            return pattern != null ? MapEntityToPattern(pattern) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting query pattern by ID: {PatternId}", patternId);
            return null;
        }
    }

    /// <summary>
    /// Create pattern async (IQueryPatternManagementService interface)
    /// </summary>
    public async Task<string> CreatePatternAsync(QueryPattern pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🆕 Creating query pattern: {PatternName}", pattern.Name);

            var entity = new QueryPatternEntity
            {
                PatternName = pattern.Name,
                NaturalLanguagePattern = pattern.Pattern,
                SqlTemplate = pattern.SqlTemplate,
                Description = pattern.Description,
                BusinessContext = pattern.BusinessContext ?? "",
                Keywords = JsonSerializer.Serialize(pattern.Keywords ?? new List<string>()),
                RequiredTables = JsonSerializer.Serialize(pattern.RequiredTables ?? new List<string>()),
                Priority = pattern.Priority,
                IsActive = true,
                UsageCount = 0,
                CreatedBy = "system",
                CreatedDate = DateTime.UtcNow
            };

            _context.QueryPatterns.Add(entity);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Created query pattern: {PatternName} with ID: {PatternId}", pattern.Name, entity.Id);
            return entity.Id.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error creating query pattern: {PatternName}", pattern.Name);
            throw;
        }
    }

    /// <summary>
    /// Update pattern async (IQueryPatternManagementService interface)
    /// </summary>
    public async Task<bool> UpdatePatternAsync(QueryPattern pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔄 Updating query pattern: {PatternName}", pattern.Name);

            if (!long.TryParse(pattern.Id, out var id))
            {
                throw new ArgumentException($"Invalid pattern ID format: {pattern.Id}");
            }

            var entity = await _context.QueryPatterns
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);

            if (entity == null)
            {
                throw new InvalidOperationException($"Query pattern with ID {pattern.Id} not found");
            }

            entity.PatternName = pattern.Name;
            entity.NaturalLanguagePattern = pattern.Pattern;
            entity.SqlTemplate = pattern.SqlTemplate;
            entity.Description = pattern.Description;
            entity.BusinessContext = pattern.BusinessContext ?? "";
            entity.Keywords = JsonSerializer.Serialize(pattern.Keywords ?? new List<string>());
            entity.RequiredTables = JsonSerializer.Serialize(pattern.RequiredTables ?? new List<string>());
            entity.Priority = pattern.Priority;
            entity.UpdatedBy = "system";
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Updated query pattern: {PatternName}", pattern.Name);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating query pattern: {PatternName}", pattern.Name);
            throw;
        }
    }

    /// <summary>
    /// Delete pattern async (IQueryPatternManagementService interface)
    /// </summary>
    public async Task<bool> DeletePatternAsync(string patternId, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🗑️ Deleting query pattern: {PatternId}", patternId);

            if (!long.TryParse(patternId, out var id))
            {
                _logger.LogWarning("Invalid pattern ID format: {PatternId}", patternId);
                return false;
            }

            var entity = await _context.QueryPatterns.FindAsync(new object[] { id }, cancellationToken);
            if (entity == null || !entity.IsActive)
            {
                _logger.LogWarning("Query pattern not found or already deleted: {PatternId}", patternId);
                return false;
            }

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ Deleted query pattern: {PatternId}", patternId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting query pattern: {PatternId}", patternId);
            return false;
        }
    }

    /// <summary>
    /// Find similar patterns async (IQueryPatternManagementService interface)
    /// </summary>
    public async Task<List<QueryPattern>> FindSimilarPatternsAsync(string query, double threshold = 0.7, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🔍 Finding similar patterns for query: {Query} with threshold: {Threshold}", query, threshold);

            var patterns = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);

            var similarPatterns = new List<QueryPattern>();
            var queryLower = query.ToLowerInvariant();

            foreach (var pattern in patterns)
            {
                var keywords = JsonSerializer.Deserialize<List<string>>(pattern.Keywords) ?? new List<string>();
                var matchedKeywords = keywords.Count(k => queryLower.Contains(k.ToLowerInvariant()));
                var similarity = keywords.Count > 0 ? (double)matchedKeywords / keywords.Count : 0;

                if (similarity >= threshold)
                {
                    var queryPattern = MapEntityToPattern(pattern);
                    queryPattern.Similarity = similarity;
                    similarPatterns.Add(queryPattern);
                }
            }

            var result = similarPatterns.OrderByDescending(p => p.Similarity).ToList();
            _logger.LogDebug("✅ Found {Count} similar patterns", result.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finding similar patterns");
            return new List<QueryPattern>();
        }
    }

    #endregion

    #region Helper Methods for Interface Implementations

    private static QueryPattern MapEntityToPattern(QueryPatternEntity entity)
    {
        var keywords = JsonSerializer.Deserialize<List<string>>(entity.Keywords) ?? new List<string>();
        var requiredTables = JsonSerializer.Deserialize<List<string>>(entity.RequiredTables) ?? new List<string>();

        return new QueryPattern
        {
            Id = entity.Id.ToString(),
            Name = entity.PatternName,
            Pattern = entity.NaturalLanguagePattern,
            SqlTemplate = entity.SqlTemplate,
            Description = entity.Description,
            BusinessContext = entity.BusinessContext,
            Keywords = keywords,
            RequiredTables = requiredTables,
            Priority = entity.Priority,
            UsageCount = entity.UsageCount,
            CreatedAt = entity.CreatedDate,
            UpdatedAt = entity.UpdatedDate,
            LastUsedAt = entity.LastUsedDate,
            Similarity = 0.0 // Default value, will be set during similarity calculations
        };
    }

    #endregion
}
