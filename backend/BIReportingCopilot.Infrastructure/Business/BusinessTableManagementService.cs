using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Models.Statistics;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Business;

/// <summary>
/// Service responsible for managing business table information and column metadata
/// </summary>
public class BusinessTableManagementService : IBusinessTableManagementService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<BusinessTableManagementService> _logger;

    public BusinessTableManagementService(
        BICopilotContext context,
        ILogger<BusinessTableManagementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    #region Business Table Operations

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

            return new BusinessTableStatistics
            {
                TotalTables = stats.Count,
                TotalColumns = totalColumns,
                RecentlyUpdatedTables = recentlyUpdatedTables,
                AverageColumnsPerTable = stats.Count > 0 ? stats.Average(s => s.ColumnCount) : 0
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table statistics");
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
            Columns = tableInfo.Columns?.Select(c => new CreateColumnInfoRequest
            {
                ColumnName = c.ColumnName,
                BusinessMeaning = c.BusinessMeaning,
                BusinessContext = c.BusinessContext,
                DataExamples = c.DataExamples,
                ValidationRules = c.ValidationRules,
                IsKeyColumn = c.IsKeyColumn
            }).ToList() ?? new List<CreateColumnInfoRequest>()
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
            Columns = tableInfo.Columns?.Select(c => new CreateColumnInfoRequest
            {
                ColumnName = c.ColumnName,
                BusinessMeaning = c.BusinessMeaning,
                BusinessContext = c.BusinessContext,
                DataExamples = c.DataExamples,
                ValidationRules = c.ValidationRules,
                IsKeyColumn = c.IsKeyColumn
            }).ToList() ?? new List<CreateColumnInfoRequest>()
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
}
