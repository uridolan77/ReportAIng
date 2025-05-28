using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Services;

public interface IAITuningSettingsService
{
    Task<bool> GetBooleanSettingAsync(string key, bool defaultValue = false);
    Task<int> GetIntegerSettingAsync(string key, int defaultValue = 0);
    Task<string> GetStringSettingAsync(string key, string defaultValue = "");
    Task<bool> UpdateSettingAsync(string key, string value);
    Task<Dictionary<string, string>> GetAllSettingsAsync();
}

public class AITuningSettingsService : IAITuningSettingsService
{
    private readonly BICopilotContext _context;
    private readonly ILogger<AITuningSettingsService> _logger;
    private readonly Dictionary<string, string> _settingsCache = new();
    private DateTime _lastCacheUpdate = DateTime.MinValue;
    private readonly TimeSpan _cacheExpiry = TimeSpan.FromMinutes(5);

    public AITuningSettingsService(BICopilotContext context, ILogger<AITuningSettingsService> logger)
    {
        _context = context;
        _logger = logger;
    }

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

    public async Task<Dictionary<string, string>> GetAllSettingsAsync()
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
}
