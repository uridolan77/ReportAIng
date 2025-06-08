using BIReportingCopilot.Core.Models.DTOs;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for managing business table information and column metadata
/// </summary>
public interface IBusinessTableManagementService
{
    // Business Table Operations
    Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync();
    Task<List<BusinessTableInfoOptimizedDto>> GetBusinessTablesOptimizedAsync();
    Task<BusinessTableInfoDto?> GetBusinessTableAsync(long id);
    Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId);
    Task<BusinessTableInfoDto?> UpdateBusinessTableAsync(long id, CreateTableInfoRequest request, string userId);
    Task<bool> DeleteBusinessTableAsync(long id);

    // Statistics
    Task<BusinessTableStatistics> GetTableStatisticsAsync();
}

/// <summary>
/// Statistics for business tables
/// </summary>
public class BusinessTableStatistics
{
    public int TotalTables { get; set; }
    public int TotalColumns { get; set; }
    public List<string> RecentlyUpdatedTables { get; set; } = new();
    public double AverageColumnsPerTable { get; set; }
}
