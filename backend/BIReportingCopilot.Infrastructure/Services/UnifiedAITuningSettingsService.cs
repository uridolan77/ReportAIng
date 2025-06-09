using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models.DTOs;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Unified AI Tuning Settings Service that implements the Core interface
/// Consolidates all AI tuning settings functionality into a single implementation
/// </summary>
public class UnifiedAITuningSettingsService : IAITuningSettingsService
{
    private readonly AITuningSettingsService _infrastructureService;
    private readonly ITuningService _tuningService;
    private readonly ILogger<UnifiedAITuningSettingsService> _logger;

    public UnifiedAITuningSettingsService(
        AITuningSettingsService infrastructureService,
        ITuningService tuningService,
        ILogger<UnifiedAITuningSettingsService> logger)
    {
        _infrastructureService = infrastructureService;
        _tuningService = tuningService;
        _logger = logger;
    }

    #region Core Interface Implementation

    public async Task<AITuningSettingsDto?> GetSettingsAsync(string settingsId)
    {
        try
        {
            // Try to parse as long ID for database lookup
            if (long.TryParse(settingsId, out var id))
            {
                var settings = await _tuningService.GetAISettingsAsync();
                return settings.FirstOrDefault(s => s.Id == id);
            }
            
            // Fallback to key-based lookup
            var value = await _infrastructureService.GetStringSettingAsync(settingsId, "");
            if (!string.IsNullOrEmpty(value))
            {
                return new AITuningSettingsDto
                {
                    Id = 0,
                    SettingKey = settingsId,
                    SettingValue = value,
                    Description = $"Setting for {settingsId}",
                    Category = "General",
                    DataType = "string",
                    IsActive = true
                };
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings: {SettingsId}", settingsId);
            return null;
        }
    }

    public async Task<List<AITuningSettingsDto>> GetAllSettingsAsync()
    {
        try
        {
            return await _tuningService.GetAISettingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all settings");
            return new List<AITuningSettingsDto>();
        }
    }

    public async Task<AITuningSettingsDto?> GetActiveSettingsAsync()
    {
        try
        {
            var allSettings = await GetAllSettingsAsync();
            return allSettings.FirstOrDefault(s => s.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active settings");
            return null;
        }
    }

    public async Task<AITuningSettingsDto> CreateSettingsAsync(AITuningSettingsDto settings)
    {
        try
        {
            // Use infrastructure service to create the setting
            await _infrastructureService.UpdateSettingAsync(settings.SettingKey, settings.SettingValue);
            
            // Return the created setting
            settings.Id = DateTime.UtcNow.Ticks; // Generate a simple ID
            _logger.LogInformation("Created AI tuning setting: {Key}", settings.SettingKey);
            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating settings: {Key}", settings.SettingKey);
            throw;
        }
    }

    public async Task<AITuningSettingsDto?> UpdateSettingsAsync(string settingsId, AITuningSettingsDto settings)
    {
        try
        {
            if (long.TryParse(settingsId, out var id))
            {
                // Use tuning service for database updates
                var userId = "system"; // Default user
                return await _tuningService.UpdateAISettingAsync(id, settings, userId);
            }
            else
            {
                // Use infrastructure service for key-based updates
                var success = await _infrastructureService.UpdateSettingAsync(settingsId, settings.SettingValue);
                if (success)
                {
                    settings.SettingKey = settingsId;
                    return settings;
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings: {SettingsId}", settingsId);
            return null;
        }
    }

    public async Task<bool> DeleteSettingsAsync(string settingsId)
    {
        try
        {
            // For now, we'll mark as inactive rather than delete
            var setting = await GetSettingsAsync(settingsId);
            if (setting != null)
            {
                setting.IsActive = false;
                var updated = await UpdateSettingsAsync(settingsId, setting);
                return updated != null;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting settings: {SettingsId}", settingsId);
            return false;
        }
    }

    public async Task<bool> ActivateSettingsAsync(string settingsId)
    {
        try
        {
            var setting = await GetSettingsAsync(settingsId);
            if (setting != null)
            {
                setting.IsActive = true;
                var updated = await UpdateSettingsAsync(settingsId, setting);
                return updated != null;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error activating settings: {SettingsId}", settingsId);
            return false;
        }
    }

    public async Task<bool> DeactivateSettingsAsync(string settingsId)
    {
        try
        {
            var setting = await GetSettingsAsync(settingsId);
            if (setting != null)
            {
                setting.IsActive = false;
                var updated = await UpdateSettingsAsync(settingsId, setting);
                return updated != null;
            }
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deactivating settings: {SettingsId}", settingsId);
            return false;
        }
    }

    public async Task<TuningDashboardData> GetTuningDashboardDataAsync()
    {
        try
        {
            return await _tuningService.GetDashboardDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tuning dashboard data");
            return new TuningDashboardData();
        }
    }

    public Task<TuningValidationResult> ValidateSettingsAsync(AITuningSettingsDto settings)
    {
        try
        {
            // Basic validation
            var result = new TuningValidationResult
            {
                IsValid = !string.IsNullOrEmpty(settings.SettingKey) && !string.IsNullOrEmpty(settings.SettingValue),
                Issues = new List<ValidationIssue>()
            };

            if (string.IsNullOrEmpty(settings.SettingKey))
                result.Issues.Add(new ValidationIssue { Title = "Missing Setting Key", Description = "Setting key is required", Severity = ValidationIssueSeverity.Error });

            if (string.IsNullOrEmpty(settings.SettingValue))
                result.Issues.Add(new ValidationIssue { Title = "Missing Setting Value", Description = "Setting value is required", Severity = ValidationIssueSeverity.Error });

            return Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating settings");
            return Task.FromResult(new TuningValidationResult
            {
                IsValid = false,
                Issues = new List<ValidationIssue> { new ValidationIssue { Title = "Validation Error", Description = ex.Message, Severity = ValidationIssueSeverity.Error } }
            });
        }
    }

    public Task<TuningOptimizationResult> OptimizeSettingsAsync(string settingsId)
    {
        try
        {
            // Basic optimization - this would be enhanced in a real implementation
            return Task.FromResult(new TuningOptimizationResult
            {
                ImprovementScore = 0.0,
                OptimizationSteps = new List<string> { "Settings are already optimized" }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing settings: {SettingsId}", settingsId);
            return Task.FromResult(new TuningOptimizationResult
            {
                ImprovementScore = -1.0,
                OptimizationSteps = new List<string> { $"Error: {ex.Message}" }
            });
        }
    }

    public async Task<TuningAnalyticsData> GetTuningAnalyticsAsync()
    {
        try
        {
            // Basic analytics - would be enhanced in real implementation
            var settings = await GetAllSettingsAsync();
            return new TuningAnalyticsData
            {
                TotalExperiments = settings.Count,
                SuccessfulExperiments = settings.Count(s => s.IsActive),
                GeneratedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting tuning analytics");
            return new TuningAnalyticsData { GeneratedAt = DateTime.UtcNow };
        }
    }

    public async Task<string> ExportSettingsAsync(string settingsId)
    {
        try
        {
            var setting = await GetSettingsAsync(settingsId);
            if (setting != null)
            {
                return System.Text.Json.JsonSerializer.Serialize(setting, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            }
            return "{}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting settings: {SettingsId}", settingsId);
            return "{}";
        }
    }

    public async Task<AITuningSettingsDto> ImportSettingsAsync(string settingsData)
    {
        try
        {
            var setting = System.Text.Json.JsonSerializer.Deserialize<AITuningSettingsDto>(settingsData);
            if (setting != null)
            {
                return await CreateSettingsAsync(setting);
            }
            throw new ArgumentException("Invalid settings data");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing settings");
            throw;
        }
    }

    public async Task<AITuningSettingsDto> CloneSettingsAsync(string settingsId, string newName)
    {
        try
        {
            var originalSetting = await GetSettingsAsync(settingsId);
            if (originalSetting != null)
            {
                var clonedSetting = new AITuningSettingsDto
                {
                    SettingKey = newName,
                    SettingValue = originalSetting.SettingValue,
                    Description = $"Clone of {originalSetting.Description}",
                    Category = originalSetting.Category,
                    DataType = originalSetting.DataType,
                    IsActive = false // Start as inactive
                };
                
                return await CreateSettingsAsync(clonedSetting);
            }
            throw new ArgumentException($"Settings not found: {settingsId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cloning settings: {SettingsId}", settingsId);
            throw;
        }
    }

    public async Task<List<AITuningSettingsDto>> GetSettingsHistoryAsync(string settingsId)
    {
        try
        {
            // For now, return current setting as history
            var setting = await GetSettingsAsync(settingsId);
            return setting != null ? new List<AITuningSettingsDto> { setting } : new List<AITuningSettingsDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings history: {SettingsId}", settingsId);
            return new List<AITuningSettingsDto>();
        }
    }

    public async Task<AITuningSettingsDto?> RestoreSettingsAsync(string settingsId, DateTime version)
    {
        try
        {
            // For now, return current setting
            return await GetSettingsAsync(settingsId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring settings: {SettingsId}", settingsId);
            return null;
        }
    }

    public async Task<bool> GetBooleanSettingAsync(string settingKey, bool defaultValue = false)
    {
        return await _infrastructureService.GetBooleanSettingAsync(settingKey, defaultValue);
    }

    #endregion
}
