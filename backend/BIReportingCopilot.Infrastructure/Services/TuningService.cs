using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Services;

public class TuningService : ITuningService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<TuningService> _logger;

    public TuningService(BICopilotContext context, ILogger<TuningService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Dashboard

    public async Task<TuningDashboardData> GetDashboardDataAsync()
    {
        try
        {
            var totalTables = await _context.BusinessTableInfo.CountAsync(t => t.IsActive);
            var totalColumns = await _context.BusinessColumnInfo.CountAsync(c => c.IsActive);
            var totalPatterns = await _context.QueryPatterns.CountAsync(p => p.IsActive);
            var totalGlossaryTerms = await _context.BusinessGlossary.CountAsync(g => g.IsActive);
            var activePromptTemplates = await _context.PromptTemplates.CountAsync(p => p.IsActive);

            var recentlyUpdatedTables = await _context.BusinessTableInfo
                .Where(t => t.IsActive && t.UpdatedDate.HasValue)
                .OrderByDescending(t => t.UpdatedDate)
                .Take(5)
                .Select(t => $"{t.SchemaName}.{t.TableName}")
                .ToListAsync();

            var mostUsedPatterns = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.UsageCount)
                .Take(5)
                .Select(p => p.PatternName)
                .ToListAsync();

            var patternUsageStats = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .GroupBy(p => p.Priority)
                .Select(g => new { Priority = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => $"Priority {x.Priority}", x => x.Count);

            return new TuningDashboardData
            {
                TotalTables = totalTables,
                TotalColumns = totalColumns,
                TotalPatterns = totalPatterns,
                TotalGlossaryTerms = totalGlossaryTerms,
                ActivePromptTemplates = activePromptTemplates,
                RecentlyUpdatedTables = recentlyUpdatedTables,
                MostUsedPatterns = mostUsedPatterns,
                PatternUsageStats = patternUsageStats
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tuning dashboard data");
            throw;
        }
    }

    #endregion

    #region Business Table Info

    public async Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync()
    {
        try
        {
            var tables = await _context.BusinessTableInfo
                .Include(t => t.Columns.Where(c => c.IsActive))
                .Where(t => t.IsActive)
                .OrderBy(t => t.SchemaName)
                .ThenBy(t => t.TableName)
                .ToListAsync();

            return tables.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business tables");
            throw;
        }
    }

    public async Task<BusinessTableInfoDto?> GetBusinessTableAsync(long id)
    {
        try
        {
            var table = await _context.BusinessTableInfo
                .Include(t => t.Columns.Where(c => c.IsActive))
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            return table != null ? MapToDto(table) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting business table {TableId}", id);
            throw;
        }
    }

    public async Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId)
    {
        try
        {
            var entity = new BusinessTableInfoEntity
            {
                TableName = request.TableName,
                SchemaName = request.SchemaName,
                BusinessPurpose = request.BusinessPurpose,
                BusinessContext = request.BusinessContext,
                PrimaryUseCase = request.PrimaryUseCase,
                CommonQueryPatterns = JsonSerializer.Serialize(request.CommonQueryPatterns),
                BusinessRules = request.BusinessRules,
                IsActive = true,
                CreatedBy = userId,
                CreatedDate = DateTime.UtcNow
            };

            _context.BusinessTableInfo.Add(entity);
            await _context.SaveChangesAsync();

            // Add columns
            foreach (var columnRequest in request.Columns)
            {
                var columnEntity = new BusinessColumnInfoEntity
                {
                    TableInfoId = entity.Id,
                    ColumnName = columnRequest.ColumnName,
                    BusinessMeaning = columnRequest.BusinessMeaning,
                    BusinessContext = columnRequest.BusinessContext,
                    DataExamples = JsonSerializer.Serialize(columnRequest.DataExamples),
                    ValidationRules = columnRequest.ValidationRules,
                    IsKeyColumn = columnRequest.IsKeyColumn,
                    IsActive = true,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.BusinessColumnInfo.Add(columnEntity);
            }

            await _context.SaveChangesAsync();

            // Reload with columns
            var createdTable = await GetBusinessTableAsync(entity.Id);
            _logger.LogInformation("Created business table: {SchemaName}.{TableName}", entity.SchemaName, entity.TableName);

            return createdTable!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating business table");
            throw;
        }
    }

    public async Task<BusinessTableInfoDto?> UpdateBusinessTableAsync(long id, CreateTableInfoRequest request, string userId)
    {
        try
        {
            var entity = await _context.BusinessTableInfo
                .Include(t => t.Columns)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);

            if (entity == null)
                return null;

            entity.TableName = request.TableName;
            entity.SchemaName = request.SchemaName;
            entity.BusinessPurpose = request.BusinessPurpose;
            entity.BusinessContext = request.BusinessContext;
            entity.PrimaryUseCase = request.PrimaryUseCase;
            entity.CommonQueryPatterns = JsonSerializer.Serialize(request.CommonQueryPatterns);
            entity.BusinessRules = request.BusinessRules;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.UtcNow;

            // Update columns - remove existing and add new ones
            _context.BusinessColumnInfo.RemoveRange(entity.Columns);

            foreach (var columnRequest in request.Columns)
            {
                var columnEntity = new BusinessColumnInfoEntity
                {
                    TableInfoId = entity.Id,
                    ColumnName = columnRequest.ColumnName,
                    BusinessMeaning = columnRequest.BusinessMeaning,
                    BusinessContext = columnRequest.BusinessContext,
                    DataExamples = JsonSerializer.Serialize(columnRequest.DataExamples),
                    ValidationRules = columnRequest.ValidationRules,
                    IsKeyColumn = columnRequest.IsKeyColumn,
                    IsActive = true,
                    CreatedBy = userId,
                    CreatedDate = DateTime.UtcNow
                };

                _context.BusinessColumnInfo.Add(columnEntity);
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated business table: {SchemaName}.{TableName}", entity.SchemaName, entity.TableName);
            return await GetBusinessTableAsync(id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating business table {TableId}", id);
            throw;
        }
    }

    public async Task<bool> DeleteBusinessTableAsync(long id)
    {
        try
        {
            var entity = await _context.BusinessTableInfo.FindAsync(id);
            if (entity == null || !entity.IsActive)
                return false;

            entity.IsActive = false;
            entity.UpdatedDate = DateTime.UtcNow;

            // Also deactivate columns
            var columns = await _context.BusinessColumnInfo
                .Where(c => c.TableInfoId == id)
                .ToListAsync();

            foreach (var column in columns)
            {
                column.IsActive = false;
                column.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted business table {TableId}", id);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting business table {TableId}", id);
            throw;
        }
    }

    #endregion

    #region Helper Methods

    private static BusinessTableInfoDto MapToDto(BusinessTableInfoEntity entity)
    {
        var commonQueryPatterns = new List<string>();
        if (!string.IsNullOrEmpty(entity.CommonQueryPatterns))
        {
            try
            {
                commonQueryPatterns = JsonSerializer.Deserialize<List<string>>(entity.CommonQueryPatterns) ?? new List<string>();
            }
            catch { /* Ignore deserialization errors */ }
        }

        return new BusinessTableInfoDto
        {
            Id = entity.Id,
            TableName = entity.TableName,
            SchemaName = entity.SchemaName,
            BusinessPurpose = entity.BusinessPurpose,
            BusinessContext = entity.BusinessContext,
            PrimaryUseCase = entity.PrimaryUseCase,
            CommonQueryPatterns = commonQueryPatterns,
            BusinessRules = entity.BusinessRules,
            IsActive = entity.IsActive,
            Columns = entity.Columns?.Select(MapColumnToDto).ToList() ?? new List<BusinessColumnInfoDto>()
        };
    }

    private static BusinessColumnInfoDto MapColumnToDto(BusinessColumnInfoEntity entity)
    {
        var dataExamples = new List<string>();
        if (!string.IsNullOrEmpty(entity.DataExamples))
        {
            try
            {
                dataExamples = JsonSerializer.Deserialize<List<string>>(entity.DataExamples) ?? new List<string>();
            }
            catch { /* Ignore deserialization errors */ }
        }

        return new BusinessColumnInfoDto
        {
            Id = entity.Id,
            ColumnName = entity.ColumnName,
            BusinessMeaning = entity.BusinessMeaning,
            BusinessContext = entity.BusinessContext,
            DataExamples = dataExamples,
            ValidationRules = entity.ValidationRules,
            IsKeyColumn = entity.IsKeyColumn,
            IsActive = entity.IsActive
        };
    }

    #endregion

    #region Query Patterns

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

    #endregion

    #region Business Glossary

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

    #endregion

    #region AI Settings

    public async Task<List<AITuningSettingsDto>> GetAISettingsAsync()
    {
        try
        {
            var settings = await _context.AITuningSettings
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.SettingKey)
                .ToListAsync();

            return settings.Select(MapSettingToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI settings");
            throw;
        }
    }

    public async Task<AITuningSettingsDto?> UpdateAISettingAsync(long id, AITuningSettingsDto request, string userId)
    {
        try
        {
            var entity = await _context.AITuningSettings
                .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

            if (entity == null)
                return null;

            entity.SettingValue = request.SettingValue;
            entity.Description = request.Description;
            entity.UpdatedBy = userId;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated AI setting: {SettingKey}", entity.SettingKey);
            return MapSettingToDto(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI setting {SettingId}", id);
            throw;
        }
    }

    #endregion

    #region Additional Helper Methods

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
            IsActive = entity.IsActive
        };
    }

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
            IsActive = entity.IsActive
        };
    }

    private static AITuningSettingsDto MapSettingToDto(AITuningSettingsEntity entity)
    {
        return new AITuningSettingsDto
        {
            Id = entity.Id,
            SettingKey = entity.SettingKey,
            SettingValue = entity.SettingValue,
            Description = entity.Description,
            Category = entity.Category,
            DataType = entity.DataType,
            IsActive = entity.IsActive
        };
    }

    #endregion
}
