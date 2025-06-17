using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Interfaces;

/// <summary>
/// Infrastructure-specific schema management service interface
/// </summary>
public interface ISchemaManagementService
{
    // Schema metadata operations
    Task<List<SchemaMetadata>> GetSchemasAsync(string userId, CancellationToken cancellationToken = default);
    Task<SchemaMetadata?> GetSchemaMetadataAsync(string schemaId, string userId, CancellationToken cancellationToken = default);
    Task<List<TableMetadata>> GetTablesAsync(string schemaId, string userId, CancellationToken cancellationToken = default);
    Task<TableMetadata?> GetTableMetadataAsync(string tableId, string userId, CancellationToken cancellationToken = default);
    Task RefreshSchemaAsync(string schemaId, string userId, CancellationToken cancellationToken = default);
    Task<List<string>> GetDatabasesAsync(string userId, CancellationToken cancellationToken = default);
    
    // Schema application operations
    Task<BusinessSchemaVersionDto> ApplyToSchemaAsync(ApplyToSchemaRequest request, string userId);
}
