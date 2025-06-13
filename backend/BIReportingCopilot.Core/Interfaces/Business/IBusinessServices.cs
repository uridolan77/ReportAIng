using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Business;

/// <summary>
/// Business table management service interface
/// </summary>
public interface IBusinessTableManagementService
{
    Task<List<BusinessTableInfoDto>> GetBusinessTablesAsync(CancellationToken cancellationToken = default);
    Task<BusinessTableInfoDto?> GetBusinessTableAsync(string tableId, CancellationToken cancellationToken = default);
    Task<string> CreateBusinessTableAsync(BusinessTableInfoDto table, CancellationToken cancellationToken = default);
    Task<bool> UpdateBusinessTableAsync(BusinessTableInfoDto table, CancellationToken cancellationToken = default);
    Task<bool> DeleteBusinessTableAsync(string tableId, CancellationToken cancellationToken = default);
    Task<List<BusinessTableInfoDto>> SearchBusinessTablesAsync(string searchTerm, CancellationToken cancellationToken = default);
}

/// <summary>
/// Glossary management service interface
/// </summary>
public interface IGlossaryManagementService
{
    Task<List<BusinessGlossaryDto>> GetGlossaryTermsAsync(CancellationToken cancellationToken = default);
    Task<BusinessGlossaryDto?> GetGlossaryTermAsync(string termId, CancellationToken cancellationToken = default);
    Task<string> CreateGlossaryTermAsync(BusinessGlossaryDto term, CancellationToken cancellationToken = default);
    Task<bool> UpdateGlossaryTermAsync(BusinessGlossaryDto term, CancellationToken cancellationToken = default);
    Task<bool> DeleteGlossaryTermAsync(string termId, CancellationToken cancellationToken = default);
    Task<List<BusinessGlossaryDto>> SearchGlossaryTermsAsync(string searchTerm, CancellationToken cancellationToken = default);
}

/// <summary>
/// AI tuning settings service interface
/// </summary>
public interface IAITuningSettingsService
{
    Task<AITuningSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default);
    Task<bool> UpdateSettingsAsync(AITuningSettingsDto settings, CancellationToken cancellationToken = default);
    Task<bool> ResetToDefaultsAsync(CancellationToken cancellationToken = default);
    Task<AITuningSettingsDto> GetDefaultSettingsAsync(CancellationToken cancellationToken = default);
    Task<bool> ValidateSettingsAsync(AITuningSettingsDto settings, CancellationToken cancellationToken = default);
}


