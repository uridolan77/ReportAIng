using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Business;
// Using fully qualified names to avoid ambiguous references
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BusinessTableStatistics = BIReportingCopilot.Core.Models.BusinessTableStatistics;

namespace BIReportingCopilot.Infrastructure.Business;

/// <summary>
/// Service responsible for managing business table information and column metadata
/// </summary>
public class BusinessTableManagementService : IBusinessTableManagementService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<BusinessTableManagementService> _logger;
    private readonly IDistributedCache? _distributedCache;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(15); // Cache for 15 minutes

    public BusinessTableManagementService(
        BICopilotContext context,
        ILogger<BusinessTableManagementService> logger,
        IDistributedCache? distributedCache = null)
    {
        _context = context;
        _logger = logger;
        _distributedCache = distributedCache;
    }

    #region Business Table Operations

    public async Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync()
    {
        try
        {
            // Try cache first
            const string cacheKey = "business_tables_all";
            if (_distributedCache != null)
            {
                var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogDebug("✅ Business tables cache hit");
                    var cachedTables = JsonSerializer.Deserialize<List<BusinessTableInfoDto>>(cachedData);
                    if (cachedTables != null)
                    {
                        return cachedTables;
                    }
                }
            }

            _logger.LogDebug("🔍 Business tables cache miss, querying database");
            var tables = await _context.BusinessTableInfo
                .Include(t => t.Columns.Where(c => c.IsActive))
                .Where(t => t.IsActive)
                .OrderBy(t => t.SchemaName)
                .ThenBy(t => t.TableName)
                .AsNoTracking() // Add AsNoTracking for better performance
                .ToListAsync();

            var result = tables.Select(MapToDto).ToList();

            // Cache the result
            if (_distributedCache != null && result.Any())
            {
                try
                {
                    var serialized = JsonSerializer.Serialize(result);
                    await _distributedCache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheExpiration
                    });
                    _logger.LogDebug("💾 Cached {Count} business tables for {Minutes} minutes", result.Count, _cacheExpiration.TotalMinutes);
                }
                catch (Exception cacheEx)
                {
                    _logger.LogWarning(cacheEx, "Failed to cache business tables, continuing without cache");
                }
            }

            return result;
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
            // Try cache first
            const string cacheKey = "business_tables_optimized";
            if (_distributedCache != null)
            {
                var cachedData = await _distributedCache.GetStringAsync(cacheKey);
                if (!string.IsNullOrEmpty(cachedData))
                {
                    _logger.LogDebug("✅ Optimized business tables cache hit");
                    var cachedTables = JsonSerializer.Deserialize<List<BusinessTableInfoOptimizedDto>>(cachedData);
                    if (cachedTables != null)
                    {
                        return cachedTables;
                    }
                }
            }

            _logger.LogDebug("🔍 Optimized business tables cache miss, querying database");
            var result = await _context.BusinessTableInfo
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

            // Cache the result
            if (_distributedCache != null && result.Any())
            {
                try
                {
                    var serialized = JsonSerializer.Serialize(result);
                    await _distributedCache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = _cacheExpiration
                    });
                    _logger.LogDebug("💾 Cached {Count} optimized business tables for {Minutes} minutes", result.Count, _cacheExpiration.TotalMinutes);
                }
                catch (Exception cacheEx)
                {
                    _logger.LogWarning(cacheEx, "Failed to cache optimized business tables, continuing without cache");
                }
            }

            return result;
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

            // Invalidate cache
            await InvalidateBusinessTablesCache();

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

            // Invalidate cache
            await InvalidateBusinessTablesCache();

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

            // Invalidate cache
            await InvalidateBusinessTablesCache();

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

    #region Statistics

    public async Task<BusinessTableStatistics> GetTableStatisticsAsync()
    {
        try
        {
            var stats = await _context.BusinessTableInfo
                .Where(t => t.IsActive)
                .Select(t => new
                {
                    TableId = t.Id,
                    SchemaTable = $"{t.SchemaName}.{t.TableName}",
                    UpdatedDate = t.UpdatedDate,
                    ColumnCount = t.Columns.Count(c => c.IsActive)
                })
                .ToListAsync();

            var totalColumns = await _context.BusinessColumnInfo.Where(c => c.IsActive).CountAsync();

            var recentlyUpdatedTables = stats
                .Where(t => t.UpdatedDate.HasValue)
                .OrderByDescending(t => t.UpdatedDate)
                .Take(5)
                .Select(t => t.SchemaTable)
                .ToList();

            return new BIReportingCopilot.Core.DTOs.BusinessTableStatistics
            {
                TableId = "summary",
                TableName = "Business Tables Summary",
                TotalRecords = stats.Count,
                TotalColumns = totalColumns,
                LastUpdated = DateTime.UtcNow,
                AverageQueryTime = 0.0,
                QueryCount = 0,
                AdditionalMetrics = new Dictionary<string, object>
                {
                    ["TotalTables"] = stats.Count,
                    ["ActiveTables"] = stats.Count,
                    ["RecentlyUpdatedTables"] = recentlyUpdatedTables
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table statistics");
            throw;
        }
    }

    #endregion

    #region Cache Management

    /// <summary>
    /// Invalidate business tables cache when data changes
    /// </summary>
    private async Task InvalidateBusinessTablesCache()
    {
        if (_distributedCache != null)
        {
            try
            {
                // Invalidate both full and optimized caches
                await _distributedCache.RemoveAsync("business_tables_all");
                await _distributedCache.RemoveAsync("business_tables_optimized");
                _logger.LogDebug("🗑️ Invalidated business tables cache (both full and optimized)");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to invalidate business tables cache");
            }
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

        // Helper method to safely deserialize JSON arrays
        List<string> DeserializeStringList(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new List<string>();
            try
            {
                return JsonSerializer.Deserialize<List<string>>(json) ?? new List<string>();
            }
            catch { return new List<string>(); }
        }

        // Helper method to safely deserialize JSON objects
        object DeserializeObject(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new object();
            try
            {
                return JsonSerializer.Deserialize<object>(json) ?? new object();
            }
            catch { return new object(); }
        }

        return new BusinessTableInfoDto
        {
            Id = entity.Id,
            TableId = entity.Id.ToString(), // Use Id as TableId
            TableName = entity.TableName,
            SchemaName = entity.SchemaName,
            BusinessName = !string.IsNullOrEmpty(entity.NaturalLanguageAliases)
                ? DeserializeStringList(entity.NaturalLanguageAliases).FirstOrDefault() ?? entity.TableName
                : entity.TableName, // Use first natural language alias or fallback to TableName
            BusinessPurpose = entity.BusinessPurpose,
            BusinessContext = entity.BusinessContext,
            PrimaryUseCase = entity.PrimaryUseCase,
            CommonQueryPatterns = commonQueryPatterns,
            BusinessRules = entity.BusinessRules,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            CreatedBy = entity.CreatedBy,
            UpdatedDate = entity.UpdatedDate,
            UpdatedBy = entity.UpdatedBy,
            DomainClassification = entity.DomainClassification ?? string.Empty,
            NaturalLanguageAliases = DeserializeStringList(entity.NaturalLanguageAliases),
            BusinessProcesses = DeserializeStringList(entity.BusinessProcesses),
            AnalyticalUseCases = DeserializeStringList(entity.AnalyticalUseCases),
            ReportingCategories = DeserializeStringList(entity.ReportingCategories),
            VectorSearchKeywords = DeserializeStringList(entity.VectorSearchKeywords),
            BusinessGlossaryTerms = DeserializeStringList(entity.BusinessGlossaryTerms),
            LLMContextHints = DeserializeStringList(entity.LLMContextHints),
            QueryComplexityHints = DeserializeStringList(entity.QueryComplexityHints),
            SemanticRelationships = DeserializeObject(entity.SemanticRelationships),
            UsagePatterns = DeserializeObject(entity.UsagePatterns),
            DataQualityIndicators = DeserializeObject(entity.DataQualityIndicators),
            RelationshipSemantics = DeserializeObject(entity.RelationshipSemantics),
            DataGovernancePolicies = DeserializeObject(entity.DataGovernancePolicies),
            ImportanceScore = entity.ImportanceScore,
            UsageFrequency = entity.UsageFrequency,
            SemanticCoverageScore = entity.SemanticCoverageScore,
            LastAnalyzed = entity.LastAnalyzed,
            BusinessOwner = entity.BusinessOwner ?? string.Empty,
            Columns = entity.Columns?.Select(MapColumnToBusinessColumnInfo).ToList() ?? new List<BusinessColumnInfo>()
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
            Id = (int)entity.Id,
            ColumnName = entity.ColumnName,
            BusinessMeaning = entity.BusinessMeaning,
            BusinessContext = entity.BusinessContext,
            DataExamples = dataExamples,
            ValidationRules = entity.ValidationRules,
            IsKeyColumn = entity.IsKeyColumn,
            IsActive = entity.IsActive
        };
    }

    private static BusinessColumnInfo MapColumnToBusinessColumnInfo(BusinessColumnInfoEntity entity)
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

        return new BusinessColumnInfo
        {
            ColumnName = entity.ColumnName,
            BusinessMeaning = entity.BusinessMeaning,
            BusinessContext = entity.BusinessContext,
            DataExamples = dataExamples,
            ValidationRules = entity.ValidationRules,
            IsKeyColumn = entity.IsKeyColumn
        };
    }

    #endregion

    // =============================================================================
    // MISSING INTERFACE METHOD IMPLEMENTATIONS
    // =============================================================================

    /// <summary>
    /// Get all business tables with cancellation token support
    /// </summary>
    public async Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync(CancellationToken cancellationToken = default)
    {
        return await GetBusinessTablesAsync();
    }

    /// <summary>
    /// Get business table by ID with cancellation token support
    /// </summary>
    public async Task<BusinessTableInfoDto?> GetBusinessTableAsync(string tableId, CancellationToken cancellationToken = default)
    {
        return await GetBusinessTableAsync(tableId);
    }

    /// <summary>
    /// Create business table with cancellation token support
    /// </summary>
    public async Task<string> CreateBusinessTableAsync(BusinessTableInfoDto tableInfo, CancellationToken cancellationToken = default)
    {
        // Convert DTO to CreateTableInfoRequest
        var request = new CreateTableInfoRequest
        {
            TableName = tableInfo.TableName,
            SchemaName = tableInfo.SchemaName,
            BusinessPurpose = tableInfo.BusinessPurpose,
            BusinessContext = tableInfo.BusinessContext,
            PrimaryUseCase = tableInfo.PrimaryUseCase,
            CommonQueryPatterns = tableInfo.CommonQueryPatterns,
            BusinessRules = tableInfo.BusinessRules,
            Columns = tableInfo.Columns ?? new List<BusinessColumnInfo>()
        };

        var created = await CreateBusinessTableAsync(request, "system");
        return created.Id.ToString();
    }

    /// <summary>
    /// Update business table with cancellation token support
    /// </summary>
    public async Task<bool> UpdateBusinessTableAsync(BusinessTableInfoDto tableInfo, CancellationToken cancellationToken = default)
    {
        // Convert DTO to CreateTableInfoRequest
        var request = new CreateTableInfoRequest
        {
            TableName = tableInfo.TableName,
            SchemaName = tableInfo.SchemaName,
            BusinessPurpose = tableInfo.BusinessPurpose,
            BusinessContext = tableInfo.BusinessContext,
            PrimaryUseCase = tableInfo.PrimaryUseCase,
            CommonQueryPatterns = tableInfo.CommonQueryPatterns,
            BusinessRules = tableInfo.BusinessRules,
            Columns = tableInfo.Columns?.Select(c => new BusinessColumnInfo
            {
                ColumnName = c.ColumnName,
                BusinessMeaning = c.BusinessMeaning,
                BusinessContext = c.BusinessContext,
                DataType = c.DataType ?? "varchar",
                IsKeyColumn = c.IsKeyColumn,
                DataExamples = c.DataExamples ?? new List<string>(),
                IsKey = c.IsKeyColumn,
                IsRequired = c.IsKeyColumn
            }).ToList() ?? new List<BusinessColumnInfo>()
        };

        var updated = await UpdateBusinessTableAsync(tableInfo.Id, request, "system");
        return updated != null;
    }

    /// <summary>
    /// Delete business table with cancellation token support
    /// </summary>
    public async Task<bool> DeleteBusinessTableAsync(string tableId, CancellationToken cancellationToken = default)
    {
        return await DeleteBusinessTableAsync(tableId);
    }

    /// <summary>
    /// Search business tables
    /// </summary>
    public async Task<List<BusinessTableInfoDto>> SearchBusinessTablesAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return await GetBusinessTablesAsync(cancellationToken);
            }

            var searchTermLower = searchTerm.ToLowerInvariant();

            var tables = await _context.BusinessTableInfo
                .Include(t => t.Columns)
                .Where(t => t.IsActive && (
                    t.TableName.ToLower().Contains(searchTermLower) ||
                    t.SchemaName.ToLower().Contains(searchTermLower) ||
                    t.BusinessPurpose.ToLower().Contains(searchTermLower) ||
                    t.BusinessContext.ToLower().Contains(searchTermLower) ||
                    t.PrimaryUseCase.ToLower().Contains(searchTermLower)
                ))
                .OrderBy(t => t.SchemaName)
                .ThenBy(t => t.TableName)
                .ToListAsync(cancellationToken);

            return tables.Select(MapToDto).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching business tables with term: {SearchTerm}", searchTerm);
            throw;
        }
    }

    /// <summary>
    /// Get optimized business tables with cancellation token support
    /// </summary>
    public async Task<List<BusinessTableInfoOptimizedDto>> GetBusinessTablesOptimizedAsync(CancellationToken cancellationToken = default)
    {
        return await GetBusinessTablesOptimizedAsync();
    }

    /// <summary>
    /// Get business table by long ID with cancellation token support
    /// </summary>
    public async Task<BusinessTableInfoDto?> GetBusinessTableAsync(long id, CancellationToken cancellationToken = default)
    {
        return await GetBusinessTableAsync(id);
    }

    /// <summary>
    /// Create business table with request object and user ID
    /// </summary>
    public async Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId, CancellationToken cancellationToken = default)
    {
        return await CreateBusinessTableAsync(request, userId);
    }

    /// <summary>
    /// Update business table with request object and user ID
    /// </summary>
    public async Task<BusinessTableInfoDto?> UpdateBusinessTableAsync(long id, CreateTableInfoRequest request, string userId, CancellationToken cancellationToken = default)
    {
        return await UpdateBusinessTableAsync(id, request, userId);
    }

    /// <summary>
    /// Delete business table by long ID with cancellation token support
    /// </summary>
    public async Task<bool> DeleteBusinessTableAsync(long id, CancellationToken cancellationToken = default)
    {
        return await DeleteBusinessTableAsync(id);
    }

    /// <summary>
    /// Get table statistics with cancellation token support
    /// </summary>
    public async Task<BusinessTableStatistics> GetTableStatisticsAsync(CancellationToken cancellationToken = default)
    {
        return await GetTableStatisticsAsync();
    }

    #region Column Operations

    /// <summary>
    /// Get specific column by ID
    /// </summary>
    public async Task<BusinessColumnInfoDto?> GetColumnAsync(long columnId, CancellationToken cancellationToken = default)
    {
        try
        {
            var columnEntity = await _context.BusinessColumnInfo
                .Include(c => c.TableInfo)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.IsActive, cancellationToken);

            if (columnEntity == null)
                return null;

            return MapColumnToDto(columnEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting column {ColumnId}", columnId);
            throw;
        }
    }

    /// <summary>
    /// Update specific column
    /// </summary>
    public async Task<BusinessColumnInfoDto?> UpdateColumnAsync(long columnId, UpdateColumnRequest request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var columnEntity = await _context.BusinessColumnInfo
                .Include(c => c.TableInfo)
                .FirstOrDefaultAsync(c => c.Id == columnId && c.IsActive, cancellationToken);

            if (columnEntity == null)
                return null;

            // Update column properties
            columnEntity.ColumnName = request.ColumnName;
            columnEntity.BusinessMeaning = request.BusinessMeaning;
            columnEntity.BusinessContext = request.BusinessContext;
            // columnEntity.BusinessPurpose = request.BusinessPurpose; // TODO: Add to entity
            columnEntity.DataExamples = JsonSerializer.Serialize(request.DataExamples);
            columnEntity.ValidationRules = request.ValidationRules;
            // columnEntity.BusinessRules = request.BusinessRules; // TODO: Add to entity
            columnEntity.PreferredAggregation = request.PreferredAggregation;
            // columnEntity.DataGovernanceLevel = request.DataGovernanceLevel; // TODO: Add to entity
            // columnEntity.LastBusinessReview = request.LastBusinessReview; // TODO: Add to entity
            columnEntity.DataQualityScore = (decimal)request.DataQualityScore;
            columnEntity.UsageFrequency = (decimal)request.UsageFrequency;
            columnEntity.SemanticRelevanceScore = (decimal)request.SemanticRelevanceScore;
            // columnEntity.ImportanceScore = request.ImportanceScore; // TODO: Add to entity
            columnEntity.IsActive = request.IsActive;
            columnEntity.IsKeyColumn = request.IsKeyColumn;
            columnEntity.IsSensitiveData = request.IsSensitiveData;
            columnEntity.IsCalculatedField = request.IsCalculatedField;
            columnEntity.BusinessDataType = request.BusinessDataType;
            // columnEntity.NaturalLanguageDescription = request.NaturalLanguageDescription; // TODO: Add to entity
            columnEntity.UpdatedBy = userId;
            columnEntity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            // Invalidate cache for the parent table
            await InvalidateBusinessTablesCache();

            return MapColumnToDto(columnEntity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating column {ColumnId}", columnId);
            throw;
        }
    }

    #endregion
}
