using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Schema;

/// <summary>
/// Business schema service interface for managing versioned schema definitions
/// </summary>
public interface IBusinessSchemaService
{
    #region Business Schemas
    
    Task<List<BusinessSchemaDto>> GetBusinessSchemasAsync(CancellationToken cancellationToken = default);
    Task<BusinessSchemaDto?> GetBusinessSchemaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<BusinessSchemaDto> CreateBusinessSchemaAsync(CreateBusinessSchemaRequest request, string userId, CancellationToken cancellationToken = default);
    Task<BusinessSchemaDto?> UpdateBusinessSchemaAsync(Guid id, UpdateBusinessSchemaRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteBusinessSchemaAsync(Guid id, CancellationToken cancellationToken = default);
    Task<bool> SetDefaultSchemaAsync(Guid id, string userId, CancellationToken cancellationToken = default);
    Task<BusinessSchemaDto?> GetDefaultSchemaAsync(CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Schema Versions
    
    Task<List<BusinessSchemaVersionDto>> GetSchemaVersionsAsync(Guid schemaId, CancellationToken cancellationToken = default);
    Task<BusinessSchemaVersionDto?> GetSchemaVersionAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task<BusinessSchemaVersionDto?> GetCurrentSchemaVersionAsync(Guid schemaId, CancellationToken cancellationToken = default);
    Task<BusinessSchemaVersionDto> CreateSchemaVersionAsync(Guid schemaId, CreateSchemaVersionRequest request, string userId, CancellationToken cancellationToken = default);
    Task<BusinessSchemaVersionDto?> UpdateSchemaVersionAsync(Guid versionId, CreateSchemaVersionRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> SetCurrentVersionAsync(Guid versionId, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSchemaVersionAsync(Guid versionId, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Table Contexts
    
    Task<List<SchemaTableContextDto>> GetTableContextsAsync(Guid schemaVersionId, CancellationToken cancellationToken = default);
    Task<SchemaTableContextDto?> GetTableContextAsync(Guid tableContextId, CancellationToken cancellationToken = default);
    Task<SchemaTableContextDto?> GetTableContextByNameAsync(Guid schemaVersionId, string schemaName, string tableName, CancellationToken cancellationToken = default);
    Task<SchemaTableContextDto> CreateTableContextAsync(Guid schemaVersionId, CreateTableContextRequest request, string userId, CancellationToken cancellationToken = default);
    Task<SchemaTableContextDto?> UpdateTableContextAsync(Guid tableContextId, CreateTableContextRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteTableContextAsync(Guid tableContextId, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Column Contexts
    
    Task<List<SchemaColumnContextDto>> GetColumnContextsAsync(Guid tableContextId, CancellationToken cancellationToken = default);
    Task<SchemaColumnContextDto?> GetColumnContextAsync(Guid columnContextId, CancellationToken cancellationToken = default);
    Task<SchemaColumnContextDto> CreateColumnContextAsync(Guid tableContextId, CreateColumnContextRequest request, string userId, CancellationToken cancellationToken = default);
    Task<SchemaColumnContextDto?> UpdateColumnContextAsync(Guid columnContextId, CreateColumnContextRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteColumnContextAsync(Guid columnContextId, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Glossary Terms
    
    Task<List<SchemaGlossaryTermDto>> GetSchemaGlossaryTermsAsync(Guid schemaVersionId, CancellationToken cancellationToken = default);
    Task<SchemaGlossaryTermDto?> GetSchemaGlossaryTermAsync(Guid termId, CancellationToken cancellationToken = default);
    Task<SchemaGlossaryTermDto> CreateSchemaGlossaryTermAsync(Guid schemaVersionId, SchemaGlossaryTermDto request, string userId, CancellationToken cancellationToken = default);
    Task<SchemaGlossaryTermDto?> UpdateSchemaGlossaryTermAsync(Guid termId, SchemaGlossaryTermDto request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSchemaGlossaryTermAsync(Guid termId, CancellationToken cancellationToken = default);
    Task<List<SchemaGlossaryTermDto>> SearchSchemaGlossaryTermsAsync(Guid schemaVersionId, string searchTerm, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Relationships
    
    Task<List<SchemaRelationshipDto>> GetSchemaRelationshipsAsync(Guid schemaVersionId, CancellationToken cancellationToken = default);
    Task<SchemaRelationshipDto?> GetSchemaRelationshipAsync(Guid relationshipId, CancellationToken cancellationToken = default);
    Task<SchemaRelationshipDto> CreateSchemaRelationshipAsync(Guid schemaVersionId, SchemaRelationshipDto request, string userId, CancellationToken cancellationToken = default);
    Task<SchemaRelationshipDto?> UpdateSchemaRelationshipAsync(Guid relationshipId, SchemaRelationshipDto request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteSchemaRelationshipAsync(Guid relationshipId, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region User Preferences
    
    Task<List<UserSchemaPreferenceDto>> GetUserSchemaPreferencesAsync(string userId, CancellationToken cancellationToken = default);
    Task<UserSchemaPreferenceDto?> GetUserDefaultSchemaAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> SetUserDefaultSchemaAsync(string userId, Guid schemaId, Guid? versionId = null, CancellationToken cancellationToken = default);
    Task<bool> UpdateUserSchemaUsageAsync(string userId, Guid schemaVersionId, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region AI Generation and Import
    
    Task<SchemaGenerationResultDto> GenerateBusinessContextAsync(Guid schemaVersionId, GenerateContextRequest request, string userId, CancellationToken cancellationToken = default);
    Task<SchemaImportResultDto> ImportBusinessContextAsync(Guid schemaVersionId, ImportContextRequest request, string userId, CancellationToken cancellationToken = default);
    Task<SchemaGenerationResultDto> AutoGenerateGlossaryTermsAsync(Guid schemaVersionId, string userId, CancellationToken cancellationToken = default);
    Task<SchemaGenerationResultDto> AutoDetectRelationshipsAsync(Guid schemaVersionId, string userId, CancellationToken cancellationToken = default);
    
    #endregion
    
    #region Analytics and Statistics
    
    Task<SchemaStatisticsDto> GetSchemaStatisticsAsync(Guid schemaId, CancellationToken cancellationToken = default);
    Task<SchemaVersionStatisticsDto> GetVersionStatisticsAsync(Guid versionId, CancellationToken cancellationToken = default);
    Task<List<SchemaUsageAnalyticsDto>> GetSchemaUsageAnalyticsAsync(Guid schemaId, DateTime fromDate, DateTime toDate, CancellationToken cancellationToken = default);
      #endregion
}
