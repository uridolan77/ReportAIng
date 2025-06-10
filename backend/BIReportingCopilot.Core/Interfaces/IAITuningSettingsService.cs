using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for AI tuning settings service
/// </summary>
public interface IAITuningSettingsService
{
    /// <summary>
    /// Get AI tuning settings by ID
    /// </summary>
    Task<AITuningSettingsDto?> GetSettingsAsync(string settingsId);

    /// <summary>
    /// Get all AI tuning settings
    /// </summary>
    Task<List<AITuningSettingsDto>> GetAllSettingsAsync();

    /// <summary>
    /// Get active AI tuning settings
    /// </summary>
    Task<AITuningSettingsDto?> GetActiveSettingsAsync();

    /// <summary>
    /// Create new AI tuning settings
    /// </summary>
    Task<AITuningSettingsDto> CreateSettingsAsync(AITuningSettingsDto settings);

    /// <summary>
    /// Update AI tuning settings
    /// </summary>
    Task<AITuningSettingsDto?> UpdateSettingsAsync(string settingsId, AITuningSettingsDto settings);

    /// <summary>
    /// Delete AI tuning settings
    /// </summary>
    Task<bool> DeleteSettingsAsync(string settingsId);

    /// <summary>
    /// Activate AI tuning settings
    /// </summary>
    Task<bool> ActivateSettingsAsync(string settingsId);

    /// <summary>
    /// Deactivate AI tuning settings
    /// </summary>
    Task<bool> DeactivateSettingsAsync(string settingsId);

    /// <summary>
    /// Get tuning dashboard data
    /// </summary>
    Task<TuningDashboardData> GetTuningDashboardDataAsync();

    /// <summary>
    /// Validate AI tuning settings
    /// </summary>
    Task<TuningValidationResult> ValidateSettingsAsync(AITuningSettingsDto settings);

    /// <summary>
    /// Optimize AI tuning settings
    /// </summary>
    Task<TuningOptimizationResult> OptimizeSettingsAsync(string settingsId);

    /// <summary>
    /// Get tuning analytics
    /// </summary>
    Task<TuningAnalyticsData> GetTuningAnalyticsAsync();

    /// <summary>
    /// Export AI tuning settings
    /// </summary>
    Task<string> ExportSettingsAsync(string settingsId);

    /// <summary>
    /// Import AI tuning settings
    /// </summary>
    Task<AITuningSettingsDto> ImportSettingsAsync(string settingsData);

    /// <summary>
    /// Clone AI tuning settings
    /// </summary>
    Task<AITuningSettingsDto> CloneSettingsAsync(string settingsId, string newName);

    /// <summary>
    /// Get settings history
    /// </summary>
    Task<List<AITuningSettingsDto>> GetSettingsHistoryAsync(string settingsId);

    /// <summary>
    /// Restore settings from history
    /// </summary>
    Task<AITuningSettingsDto?> RestoreSettingsAsync(string settingsId, DateTime version);

    /// <summary>
    /// Get boolean setting value by key
    /// </summary>
    Task<bool> GetBooleanSettingAsync(string settingKey, bool defaultValue = false);
}
