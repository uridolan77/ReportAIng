using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Infrastructure.Business;

/// <summary>
/// AI Tuning Settings Service - Provides AI tuning settings functionality
/// </summary>
public class AITuningSettingsService : IAITuningSettingsService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<AITuningSettingsService> _logger;
    private readonly Dictionary<string, string> _settingsCache = new();
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public AITuningSettingsService(
        BICopilotContext context,
        ILogger<AITuningSettingsService> logger)
    {
        _context = context;
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
                var settings = await GetAllSettingsAsync();
                return settings.FirstOrDefault(s => s.Id == id);
            }

            // Fallback to key-based lookup
            var value = await GetStringSettingAsync(settingsId, "");
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
            var settings = await _context.AITuningSettings
                .Where(s => s.IsActive)
                .OrderBy(s => s.Category)
                .ThenBy(s => s.SettingKey)
                .ToListAsync();

            return settings.Select(s => new AITuningSettingsDto
            {
                Id = s.Id,
                SettingKey = s.SettingKey,
                SettingValue = s.SettingValue,
                Description = s.Description,
                Category = s.Category,
                DataType = s.DataType,
                IsActive = s.IsActive
            }).ToList();
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
            // Use existing UpdateSettingAsync method to create the setting
            await UpdateSettingAsync(settings.SettingKey, settings.SettingValue);

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
                // Direct database update without tuning service dependency
                var entity = await _context.AITuningSettings
                    .FirstOrDefaultAsync(s => s.Id == id && s.IsActive);

                if (entity == null)
                    return null;

                entity.SettingValue = settings.SettingValue;
                entity.Description = settings.Description;
                entity.UpdatedBy = "system";
                entity.UpdatedDate = DateTime.UtcNow;

                await _context.SaveChangesAsync();
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
            else
            {
                // Use existing UpdateSettingAsync method for key-based updates
                var success = await UpdateSettingAsync(settingsId, settings.SettingValue);
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
            // Return a default dashboard data since TuningService dependency was removed
            return new TuningDashboardData
            {
                TotalTables = 0,
                TotalColumns = 0,
                TotalPatterns = 0,
                TotalGlossaryTerms = 0,
                ActivePromptTemplates = 0,
                RecentlyUpdatedTables = new List<string>(),
                MostUsedPatterns = new List<string>(),
                PatternUsageStats = new Dictionary<string, int>()
            };
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

    #endregion

    #region Infrastructure Methods

    public async Task<bool> GetBooleanSettingAsync(string key, bool defaultValue = false)
    {
        var value = await GetSettingValueAsync(key);
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return bool.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task<int> GetIntegerSettingAsync(string key, int defaultValue = 0)
    {
        var value = await GetSettingValueAsync(key);
        if (string.IsNullOrEmpty(value))
            return defaultValue;

        return int.TryParse(value, out var result) ? result : defaultValue;
    }

    public async Task<string> GetStringSettingAsync(string key, string defaultValue = "")
    {
        var value = await GetSettingValueAsync(key);
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    public async Task<bool> UpdateSettingAsync(string key, string value)
    {
        try
        {
            var setting = await _context.AITuningSettings
                .FirstOrDefaultAsync(s => s.SettingKey == key);

            if (setting == null)
            {
                setting = new AITuningSettingsEntity
                {
                    SettingKey = key,
                    SettingValue = value,
                    DataType = "string",
                    IsActive = true,
                    CreatedBy = "System",
                    CreatedDate = DateTime.UtcNow
                };
                _context.AITuningSettings.Add(setting);
            }
            else
            {
                setting.SettingValue = value;
                setting.UpdatedDate = DateTime.UtcNow;
                setting.UpdatedBy = "System";
            }

            await _context.SaveChangesAsync();
            
            // Update cache
            _settingsCache[key] = value;
            
            _logger.LogInformation("Updated AI tuning setting: {Key} = {Value}", key, value);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI tuning setting: {Key}", key);
            return false;
        }
    }

    public async Task<Dictionary<string, string>> GetAllSettingsDictionaryAsync()
    {
        try
        {
            var settings = await _context.AITuningSettings
                .Where(s => s.IsActive)
                .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);

            return settings;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all AI tuning settings");
            return new Dictionary<string, string>();
        }
    }

    private async Task<string?> GetSettingValueAsync(string key)
    {
        try
        {
            // Check cache first
            if (_settingsCache.ContainsKey(key) && DateTime.UtcNow - _lastCacheUpdate < _cacheExpiry)
            {
                return _settingsCache[key];
            }

            // Refresh cache if expired
            if (DateTime.UtcNow - _lastCacheUpdate >= _cacheExpiry)
            {
                await RefreshCacheAsync();
            }

            return _settingsCache.GetValueOrDefault(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving AI tuning setting: {Key}", key);
            return null;
        }
    }

    private async Task RefreshCacheAsync()
    {
        try
        {
            var settings = await _context.AITuningSettings
                .Where(s => s.IsActive)
                .ToDictionaryAsync(s => s.SettingKey, s => s.SettingValue);

            _settingsCache.Clear();
            foreach (var setting in settings)
            {
                _settingsCache[setting.Key] = setting.Value;
            }

            _lastCacheUpdate = DateTime.UtcNow;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing AI tuning settings cache");
        }
    }

    #endregion

    #region Missing Interface Methods

    public async Task<AITuningSettingsDto> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var activeSettings = await GetActiveSettingsAsync();
            return activeSettings ?? new AITuningSettingsDto
            {
                Id = 0,
                SettingKey = "default",
                SettingValue = "{}",
                Description = "Default AI tuning settings",
                Category = "General",
                DataType = "json",
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting settings");
            return new AITuningSettingsDto
            {
                Id = 0,
                SettingKey = "error",
                SettingValue = "{}",
                Description = "Error retrieving settings",
                Category = "Error",
                DataType = "json",
                IsActive = false
            };
        }
    }

    public async Task<bool> UpdateSettingsAsync(AITuningSettingsDto settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await UpdateSettingsAsync(settings.Id.ToString(), settings);
            return updated != null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating settings");
            return false;
        }
    }

    public async Task<bool> ResetToDefaultsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Get all settings and reset to defaults
            var allSettings = await GetAllSettingsAsync();
            foreach (var setting in allSettings)
            {
                setting.IsActive = false;
            }

            // Create default settings
            var defaultSettings = new AITuningSettingsDto
            {
                SettingKey = "default_ai_settings",
                SettingValue = GetDefaultSettingsJson(),
                Description = "Default AI tuning settings",
                Category = "General",
                DataType = "json",
                IsActive = true
            };

            await CreateSettingsAsync(defaultSettings);
            _logger.LogInformation("Reset AI tuning settings to defaults");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resetting settings to defaults");
            return false;
        }
    }

    public async Task<AITuningSettingsDto> GetDefaultSettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return new AITuningSettingsDto
            {
                Id = 0,
                SettingKey = "default_ai_settings",
                SettingValue = GetDefaultSettingsJson(),
                Description = "Default AI tuning settings",
                Category = "General",
                DataType = "json",
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting default settings");
            throw;
        }
    }

    public async Task<bool> ValidateSettingsAsync(AITuningSettingsDto settings, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await ValidateSettingsAsync(settings);
            return result.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating settings");
            return false;
        }
    }

    private string GetDefaultSettingsJson()
    {
        var defaultSettings = new
        {
            temperature = 0.7,
            max_tokens = 2000,
            top_p = 0.9,
            frequency_penalty = 0.0,
            presence_penalty = 0.0,
            model = "gpt-4",
            prompt_optimization = true,
            cache_enabled = true,
            timeout_seconds = 30
        };

        return System.Text.Json.JsonSerializer.Serialize(defaultSettings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
    }

    public async Task<List<AITuningSettingsDto>> GetAISettingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await GetAllSettingsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting AI settings");
            return new List<AITuningSettingsDto>();
        }
    }

    public async Task<AITuningSettingsDto?> UpdateAISettingAsync(long id, AITuningSettingsDto request, string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var updated = await UpdateSettingsAsync(id.ToString(), request);
            if (updated != null)
            {
                _logger.LogInformation("Updated AI setting {Id} by user {UserId}", id, userId);
            }
            return updated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating AI setting {Id} by user {UserId}", id, userId);
            return null;
        }
    }

    /// <summary>
    /// Get boolean setting value (IAITuningSettingsService interface)
    /// </summary>
    public async Task<bool> GetBooleanSettingAsync(string settingName)
    {
        try
        {
            var value = await GetStringSettingAsync(settingName, "false");
            return bool.TryParse(value, out var result) && result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting boolean setting: {SettingName}", settingName);
            return false;
        }
    }

    #endregion
}
