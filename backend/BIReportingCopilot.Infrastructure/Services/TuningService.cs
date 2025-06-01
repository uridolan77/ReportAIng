using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Performance;

namespace BIReportingCopilot.Infrastructure.Services;

public class TuningService : ITuningService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<TuningService> _logger;
    private readonly IBusinessContextAutoGenerator _autoGenerator;
    private readonly StreamingDataService _streamingService;
    private readonly ISchemaService _schemaService;
    private readonly IConfiguration _configuration;

    public TuningService(
        BICopilotContext context,
        ILogger<TuningService> logger,
        IBusinessContextAutoGenerator autoGenerator,
        StreamingDataService streamingService,
        ISchemaService schemaService,
        IConfiguration configuration)
    {
        _context = context;
        _logger = logger;
        _autoGenerator = autoGenerator;
        _streamingService = streamingService;
        _schemaService = schemaService;
        _configuration = configuration;
    }

    #region Dashboard

    public async Task<TuningDashboardData> GetDashboardDataAsync()
    {
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Use a single query with multiple aggregations to reduce database round trips
            var dashboardStats = await _context.BusinessTableInfo
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    TableId = t.Id,
                    SchemaTable = $"{t.SchemaName}.{t.TableName}",
                    UpdatedDate = t.UpdatedDate,
                    ColumnCount = t.Columns.Count(c => c.IsActive)
                })
                .ToListAsync();

            // Get all other counts sequentially to avoid DbContext concurrency issues
            var columnCount = await _context.BusinessColumnInfo.Where(c => c.IsActive).CountAsync();
            var patternCount = await _context.QueryPatterns.Where(p => p.IsActive).CountAsync();
            var glossaryCount = await _context.BusinessGlossary.Where(g => g.IsActive).CountAsync();
            var templateCount = await _context.PromptTemplates.Where(p => p.IsActive).CountAsync();

            // Get pattern data
            var patterns = await _context.QueryPatterns
                .Where(p => p.IsActive)
                .Select(p => new { p.PatternName, p.UsageCount, p.Priority })
                .ToListAsync();

            var counts = new[] { columnCount, patternCount, glossaryCount, templateCount };

            var recentlyUpdatedTables = dashboardStats
                .Where(t => t.UpdatedDate.HasValue)
                .OrderByDescending(t => t.UpdatedDate)
                .Take(5)
                .Select(t => t.SchemaTable)
                .ToList();

            var mostUsedPatterns = patterns
                .OrderByDescending(p => p.UsageCount)
                .Take(5)
                .Select(p => p.PatternName)
                .ToList();

            var patternUsageStats = patterns
                .GroupBy(p => p.Priority)
                .ToDictionary(g => $"Priority {g.Key}", g => g.Count());

            stopwatch.Stop();

            return new TuningDashboardData
            {
                TotalTables = dashboardStats.Count,
                TotalColumns = counts[0],
                TotalPatterns = counts[1],
                TotalGlossaryTerms = counts[2],
                ActivePromptTemplates = counts[3],
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

    /// <summary>
    /// Optimized version that only loads necessary fields for better performance
    /// </summary>
    public async Task<List<BusinessTableInfoOptimizedDto>> GetBusinessTablesOptimizedAsync()
    {
        try
        {
            return await _context.BusinessTableInfo
                .Where(t => t.IsActive)
                .Select(t => new BusinessTableInfoOptimizedDto
                {
                    Id = t.Id,
                    TableName = t.TableName,
                    SchemaName = t.SchemaName,
                    BusinessPurpose = t.BusinessPurpose,
                    BusinessContext = t.BusinessContext,
                    IsActive = t.IsActive,
                    UpdatedDate = t.UpdatedDate,
                    UpdatedBy = t.UpdatedBy,
                    ColumnCount = t.Columns.Count(c => c.IsActive),
                    CreatedDate = t.CreatedDate
                })
                .AsNoTracking()
                .OrderBy(t => t.SchemaName)
                .ThenBy(t => t.TableName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting optimized business tables");
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

    #region Auto-Generation

    public async Task<AutoGenerationResponse> AutoGenerateBusinessContextAsync(AutoGenerationRequest request, string userId)
    {
        var startTime = DateTime.UtcNow;
        var response = new AutoGenerationResponse();

        try
        {
            _logger.LogInformation("Starting auto-generation for user {UserId}", userId);

            // Refresh schema cache to ensure we have the latest database structure
            _logger.LogInformation("Refreshing schema cache to ensure latest database structure");
            try
            {
                await _schemaService.RefreshSchemaAsync();
                _logger.LogInformation("Schema cache refreshed successfully");

                // Debug: Log actual column counts from schema
                var schema = await _schemaService.GetSchemaMetadataAsync();
                var tablesToProcess = request.SpecificTables ?? schema.Tables.Select(t => $"{t.Schema}.{t.Name}").ToList();
                foreach (var table in schema.Tables.Where(t => tablesToProcess.Contains($"{t.Schema}.{t.Name}")))
                {
                    _logger.LogInformation("DEBUG: Table {Schema}.{Table} has {ColumnCount} columns: {Columns}",
                        table.Schema, table.Name, table.Columns.Count,
                        string.Join(", ", table.Columns.Take(10).Select(c => c.Name)));
                }

                // Additional debug: Test direct SQL query to verify column counts
                try
                {
                    var connectionString = await GetConnectionStringAsync();
                    if (!string.IsNullOrEmpty(connectionString))
                    {
                        using var connection = new Microsoft.Data.SqlClient.SqlConnection(connectionString);
                        await connection.OpenAsync();

                        var testQuery = @"
                            SELECT TABLE_NAME, COUNT(*) as ColumnCount
                            FROM INFORMATION_SCHEMA.COLUMNS
                            WHERE TABLE_SCHEMA = 'common'
                            AND TABLE_NAME IN ('tbl_Daily_actions', 'tbl_Daily_actions_players', 'tbl_Countries', 'tbl_Currencies', 'tbl_White_labels')
                            GROUP BY TABLE_NAME
                            ORDER BY TABLE_NAME";

                        using var command = new Microsoft.Data.SqlClient.SqlCommand(testQuery, connection);
                        using var reader = await command.ExecuteReaderAsync();

                        _logger.LogInformation("DEBUG: Direct SQL column count verification:");
                        while (await reader.ReadAsync())
                        {
                            _logger.LogInformation("DEBUG: {TableName} = {ColumnCount} columns (direct SQL)",
                                reader["TABLE_NAME"], reader["ColumnCount"]);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not perform direct SQL column count verification");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not refresh schema cache, continuing with existing cache");
            }

            if (request.GenerateTableContexts)
            {
                _logger.LogInformation("Generating table contexts for {TableCount} tables...",
                    request.SpecificTables?.Count ?? 0);

                List<AutoGeneratedTableContext> tableContexts;
                if (request.SpecificTables?.Any() == true)
                {
                    // Generate contexts for specific tables only
                    tableContexts = new List<AutoGeneratedTableContext>();
                    foreach (var tableSpec in request.SpecificTables)
                    {
                        var parts = tableSpec.Split('.');
                        var tableName = parts.Length > 1 ? parts[1] : parts[0];
                        var schemaName = parts.Length > 1 ? parts[0] : "common";

                        try
                        {
                            var context = await _autoGenerator.GenerateTableContextAsync(tableName, schemaName);
                            tableContexts.Add(context);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to generate context for table {Schema}.{Table}", schemaName, tableName);
                            response.Warnings.Add($"Failed to generate context for table {schemaName}.{tableName}: {ex.Message}");
                        }
                    }
                }
                else
                {
                    // Generate contexts for all tables
                    tableContexts = await _autoGenerator.GenerateTableContextsAsync();
                }

                response.GeneratedTableContexts = tableContexts;
                response.TotalTablesProcessed = tableContexts.Count;
                response.TotalColumnsProcessed = tableContexts.Sum(t => t.Columns.Count);
            }

            if (request.GenerateGlossaryTerms)
            {
                _logger.LogInformation("Generating glossary terms...");
                var glossaryTerms = await _autoGenerator.GenerateGlossaryTermsAsync();
                response.GeneratedGlossaryTerms = glossaryTerms;
                response.TotalTermsGenerated = glossaryTerms.Count;
            }

            if (request.AnalyzeRelationships)
            {
                _logger.LogInformation("Analyzing table relationships...");
                response.RelationshipAnalysis = await _autoGenerator.AnalyzeTableRelationshipsAsync();
            }

            response.ProcessingTime = DateTime.UtcNow - startTime;
            response.Success = true;

            _logger.LogInformation("Auto-generation completed in {Duration}ms", response.ProcessingTime.TotalMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during auto-generation");
            response.Success = false;
            response.Errors.Add($"Auto-generation failed: {ex.Message}");
            response.ProcessingTime = DateTime.UtcNow - startTime;
            return response;
        }
    }

    public async Task<List<AutoGeneratedTableContext>> AutoGenerateTableContextsAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Auto-generating table contexts for user {UserId}", userId);
            return await _autoGenerator.GenerateTableContextsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-generating table contexts");
            throw;
        }
    }

    public async Task<List<AutoGeneratedGlossaryTerm>> AutoGenerateGlossaryTermsAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Auto-generating glossary terms for user {UserId}", userId);
            return await _autoGenerator.GenerateGlossaryTermsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-generating glossary terms");
            throw;
        }
    }

    public async Task<BusinessRelationshipAnalysis> AutoGenerateRelationshipAnalysisAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Auto-generating relationship analysis for user {UserId}", userId);
            return await _autoGenerator.AnalyzeTableRelationshipsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-generating relationship analysis");
            throw;
        }
    }

    public async Task<AutoGeneratedTableContext> AutoGenerateTableContextAsync(string tableName, string schemaName, string userId)
    {
        try
        {
            _logger.LogInformation("Auto-generating context for table {Schema}.{Table} for user {UserId}", schemaName, tableName, userId);
            return await _autoGenerator.GenerateTableContextAsync(tableName, schemaName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-generating context for table {Schema}.{Table}", schemaName, tableName);
            throw;
        }
    }

    public async Task ApplyAutoGeneratedContextAsync(AutoGenerationResponse autoGenerated, string userId)
    {
        try
        {
            _logger.LogInformation("Applying auto-generated context for user {UserId}", userId);

            // Apply table contexts
            foreach (var tableContext in autoGenerated.GeneratedTableContexts)
            {
                await ApplyTableContextAsync(tableContext, userId);
            }

            // Apply glossary terms
            foreach (var glossaryTerm in autoGenerated.GeneratedGlossaryTerms)
            {
                await ApplyGlossaryTermAsync(glossaryTerm, userId);
            }

            _logger.LogInformation("Applied {TableCount} table contexts and {TermCount} glossary terms",
                autoGenerated.GeneratedTableContexts.Count, autoGenerated.GeneratedGlossaryTerms.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying auto-generated context");
            throw;
        }
    }

    private async Task ApplyTableContextAsync(AutoGeneratedTableContext context, string userId)
    {
        try
        {
            // Check if table already exists
            var existingTable = await _context.BusinessTableInfo
                .FirstOrDefaultAsync(t => t.TableName == context.TableName &&
                                         t.SchemaName == context.SchemaName &&
                                         t.IsActive);

            if (existingTable != null)
            {
                _logger.LogInformation("Table {Schema}.{Table} already exists, skipping", context.SchemaName, context.TableName);
                return;
            }

            var request = new CreateTableInfoRequest
            {
                TableName = context.TableName,
                SchemaName = context.SchemaName,
                BusinessPurpose = context.BusinessPurpose,
                BusinessContext = context.BusinessContext,
                PrimaryUseCase = context.PrimaryUseCase,
                CommonQueryPatterns = context.CommonQueryPatterns,
                BusinessRules = context.BusinessRules,
                Columns = context.Columns.Select(c => new CreateColumnInfoRequest
                {
                    ColumnName = c.ColumnName,
                    BusinessMeaning = c.BusinessName,
                    BusinessContext = c.BusinessDescription,
                    DataExamples = c.SampleValues,
                    ValidationRules = string.Join("; ", c.BusinessRules),
                    IsKeyColumn = c.IsPrimaryKey || c.IsForeignKey
                }).ToList()
            };

            await CreateBusinessTableAsync(request, userId);
            _logger.LogInformation("Applied auto-generated context for table {Schema}.{Table}", context.SchemaName, context.TableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying table context for {Schema}.{Table}", context.SchemaName, context.TableName);
        }
    }

    private async Task ApplyGlossaryTermAsync(AutoGeneratedGlossaryTerm term, string userId)
    {
        try
        {
            // Check if term already exists
            var existingTerm = await _context.BusinessGlossary
                .FirstOrDefaultAsync(g => g.Term.ToLower() == term.Term.ToLower() && g.IsActive);

            if (existingTerm != null)
            {
                _logger.LogInformation("Glossary term '{Term}' already exists, skipping", term.Term);
                return;
            }

            var glossaryDto = new BusinessGlossaryDto
            {
                Term = term.Term,
                Definition = term.Definition,
                BusinessContext = term.BusinessContext,
                Synonyms = term.Synonyms,
                RelatedTerms = term.RelatedTerms,
                Category = term.Category
            };

            await CreateGlossaryTermAsync(glossaryDto, userId);
            _logger.LogInformation("Applied auto-generated glossary term: {Term}", term.Term);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying glossary term: {Term}", term.Term);
        }
    }

    #endregion

    #region Helper Methods

    private async Task<string?> GetConnectionStringAsync()
    {
        // Use the BIDatabase connection string (same as schema service)
        try
        {
            return _configuration.GetConnectionString("BIDatabase");
        }
        catch
        {
            return null;
        }
    }

    #endregion
}
