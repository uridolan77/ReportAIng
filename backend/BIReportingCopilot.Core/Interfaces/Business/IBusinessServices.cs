using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using BusinessTableStatistics = BIReportingCopilot.Core.Models.BusinessTableStatistics;

namespace BIReportingCopilot.Core.Interfaces.Business;

/// <summary>
/// Business table management service interface
/// </summary>
public interface IBusinessTableManagementService
{
    Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync(CancellationToken cancellationToken = default);
    Task<List<BusinessTableInfoOptimizedDto>> GetBusinessTablesOptimizedAsync(CancellationToken cancellationToken = default);
    Task<BusinessTableInfoDto?> GetBusinessTableAsync(long id, CancellationToken cancellationToken = default);
    Task<string> CreateBusinessTableAsync(BusinessTableInfoDto table, CancellationToken cancellationToken = default);
    Task<BusinessTableInfoDto> CreateBusinessTableAsync(CreateTableInfoRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateBusinessTableAsync(BusinessTableInfoDto table, CancellationToken cancellationToken = default);
    Task<BusinessTableInfoDto?> UpdateBusinessTableAsync(long id, CreateTableInfoRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteBusinessTableAsync(string tableId, CancellationToken cancellationToken = default);
    Task<bool> DeleteBusinessTableAsync(long id, CancellationToken cancellationToken = default);
    Task<List<BusinessTableInfoDto>> SearchBusinessTablesAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<BusinessTableStatistics> GetTableStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Glossary management service interface
/// </summary>
public interface IGlossaryManagementService
{
    Task<List<BusinessGlossaryDto>> GetGlossaryTermsAsync(CancellationToken cancellationToken = default);
    Task<BusinessGlossaryDto?> GetGlossaryTermAsync(string termId, CancellationToken cancellationToken = default);
    Task<string> CreateGlossaryTermAsync(BusinessGlossaryDto term, CancellationToken cancellationToken = default);
    Task<BusinessGlossaryDto> CreateGlossaryTermAsync(BusinessGlossaryDto request, string userId, CancellationToken cancellationToken = default);
    Task<bool> UpdateGlossaryTermAsync(BusinessGlossaryDto term, CancellationToken cancellationToken = default);
    Task<BusinessGlossaryDto?> UpdateGlossaryTermAsync(long id, BusinessGlossaryDto request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteGlossaryTermAsync(string termId, CancellationToken cancellationToken = default);
    Task<bool> DeleteGlossaryTermAsync(long id, CancellationToken cancellationToken = default);
    Task<List<BusinessGlossaryDto>> SearchGlossaryTermsAsync(string searchTerm, CancellationToken cancellationToken = default);
    Task<BIReportingCopilot.Core.Models.GlossaryStatistics> GetGlossaryStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Query pattern management service interface
/// </summary>
public interface IQueryPatternManagementService
{
    Task<List<QueryPatternDto>> GetQueryPatternsAsync(CancellationToken cancellationToken = default);
    Task<QueryPatternDto?> GetQueryPatternAsync(long id, CancellationToken cancellationToken = default);
    Task<QueryPatternDto> CreateQueryPatternAsync(CreateQueryPatternRequest request, string userId, CancellationToken cancellationToken = default);
    Task<QueryPatternDto?> UpdateQueryPatternAsync(long id, CreateQueryPatternRequest request, string userId, CancellationToken cancellationToken = default);
    Task<bool> DeleteQueryPatternAsync(long id, CancellationToken cancellationToken = default);
    Task<string> TestQueryPatternAsync(long id, string naturalLanguageQuery, CancellationToken cancellationToken = default);
    Task<QueryPatternStatistics> GetPatternStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// AI tuning settings service interface
/// </summary>
public interface IAITuningSettingsService
{
    Task<AITuningSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<List<AITuningSettingsDto>> GetAISettingsAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateSettingsAsync(AITuningSettingsDto settings, CancellationToken cancellationToken = default);
    Task<AITuningSettingsDto?> UpdateAISettingAsync(long id, AITuningSettingsDto request, string userId, CancellationToken cancellationToken = default);
    Task<bool> ResetToDefaultsAsync(CancellationToken cancellationToken = default);
    Task<AITuningSettingsDto> GetDefaultSettingsAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateSettingsAsync(AITuningSettingsDto settings, CancellationToken cancellationToken = default);

    // Additional methods expected by Infrastructure services
    Task<bool> GetBooleanSettingAsync(string settingName);
}


