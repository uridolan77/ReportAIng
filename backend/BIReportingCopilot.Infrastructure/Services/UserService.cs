using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly BICopilotContext _context;
    private readonly IAuditService _auditService;

    public UserService(
        ILogger<UserService> logger,
        BICopilotContext context,
        IAuditService auditService)
    {
        _logger = logger;
        _context = context;
        _auditService = auditService;
    }

    public async Task<UserInfo?> GetUserAsync(string userId)
    {
        try
        {
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (userEntity == null)
            {
                return null;
            }

            var preferences = await GetUserPreferencesEntityAsync(userId);

            return new UserInfo
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                DisplayName = userEntity.DisplayName,
                Email = userEntity.Email,
                Roles = userEntity.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries),
                Permissions = await GetUserPermissionsAsync(userId),
                Preferences = MapToUserPreferences(preferences),
                LastLogin = userEntity.LastLoginDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user: {UserId}", userId);
            return null;
        }
    }

    public async Task<UserInfo> UpdateUserPreferencesAsync(string userId, UserPreferences preferences)
    {
        try
        {
            var existingPrefs = await GetUserPreferencesEntityAsync(userId);

            if (existingPrefs == null)
            {
                existingPrefs = new UserPreferencesEntity
                {
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                };
                _context.UserPreferences.Add(existingPrefs);
            }

            // Update preferences
            existingPrefs.PreferredChartTypes = string.Join(",", preferences.PreferredChartTypes);
            existingPrefs.DefaultDateRange = preferences.DefaultDateRange;
            existingPrefs.MaxRowsPerQuery = preferences.MaxRowsPerQuery;
            existingPrefs.EnableQuerySuggestions = preferences.EnableQuerySuggestions;
            existingPrefs.EnableAutoVisualization = preferences.EnableAutoVisualization;
            existingPrefs.NotificationSettings = JsonSerializer.Serialize(preferences.NotificationSettings);
            existingPrefs.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await _auditService.LogAsync("USER_PREFERENCES_UPDATED", userId, "UserPreferences", userId);

            var userInfo = await GetUserAsync(userId);
            return userInfo ?? throw new InvalidOperationException("Failed to retrieve updated user info");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user preferences for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<List<string>> GetUserPermissionsAsync(string userId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null)
            {
                return new List<string>();
            }

            var roles = user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var permissions = new List<string>();

            // Map roles to permissions
            foreach (var role in roles)
            {
                permissions.AddRange(GetPermissionsForRole(role.Trim()));
            }

            return permissions.Distinct().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting permissions for user: {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<bool> HasPermissionAsync(string userId, string permission)
    {
        try
        {
            var permissions = await GetUserPermissionsAsync(userId);
            return permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission {Permission} for user: {UserId}", permission, userId);
            return false;
        }
    }

    public async Task<UserSession> CreateSessionAsync(string userId, string? ipAddress = null, string? userAgent = null)
    {
        try
        {
            var sessionId = Guid.NewGuid().ToString();
            var sessionEntity = new UserSessionEntity
            {
                SessionId = sessionId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsActive = true
            };

            _context.UserSessions.Add(sessionEntity);
            await _context.SaveChangesAsync();

            await _auditService.LogAsync("SESSION_CREATED", userId, "UserSession", sessionId,
                new { IpAddress = ipAddress, UserAgent = userAgent }, ipAddress, userAgent);

            return new UserSession
            {
                SessionId = sessionId,
                UserId = userId,
                StartTime = sessionEntity.StartTime,
                LastActivity = sessionEntity.LastActivity,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsActive = true
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating session for user: {UserId}", userId);
            throw;
        }
    }

    public async Task<UserSession?> GetSessionAsync(string sessionId)
    {
        try
        {
            var sessionEntity = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (sessionEntity == null)
            {
                return null;
            }

            return new UserSession
            {
                SessionId = sessionEntity.SessionId,
                UserId = sessionEntity.UserId,
                StartTime = sessionEntity.StartTime,
                LastActivity = sessionEntity.LastActivity,
                IpAddress = sessionEntity.IpAddress,
                UserAgent = sessionEntity.UserAgent,
                IsActive = sessionEntity.IsActive
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting session: {SessionId}", sessionId);
            return null;
        }
    }

    public async Task UpdateSessionActivityAsync(string sessionId)
    {
        try
        {
            var sessionEntity = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

            if (sessionEntity != null)
            {
                sessionEntity.LastActivity = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating session activity: {SessionId}", sessionId);
        }
    }

    public async Task EndSessionAsync(string sessionId)
    {
        try
        {
            var sessionEntity = await _context.UserSessions
                .FirstOrDefaultAsync(s => s.SessionId == sessionId);

            if (sessionEntity != null)
            {
                sessionEntity.IsActive = false;
                sessionEntity.UpdatedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                await _auditService.LogAsync("SESSION_ENDED", sessionEntity.UserId, "UserSession", sessionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ending session: {SessionId}", sessionId);
        }
    }

    public async Task LogUserActivityAsync(UserActivity activity)
    {
        try
        {
            await _auditService.LogAsync(
                activity.Action,
                activity.UserId,
                activity.EntityType,
                activity.EntityId,
                activity.Details,
                activity.IpAddress,
                activity.UserAgent
            );

            _logger.LogInformation("User activity logged: {Action} for user: {UserId}",
                activity.Action, activity.UserId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging user activity for user: {UserId}", activity.UserId);
        }
    }

    public async Task<List<UserActivity>> GetUserActivityAsync(string userId, DateTime? from = null, DateTime? to = null)
    {
        try
        {
            var query = _context.AuditLog.Where(a => a.UserId == userId);

            if (from.HasValue)
            {
                query = query.Where(a => a.Timestamp >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(a => a.Timestamp <= to.Value);
            }

            var auditEntries = await query
                .OrderByDescending(a => a.Timestamp)
                .Take(100) // Limit to last 100 activities
                .ToListAsync();

            return auditEntries.Select(a => new UserActivity
            {
                Id = a.Id.ToString(),
                UserId = a.UserId,
                Action = a.Action,
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Details = !string.IsNullOrEmpty(a.Details) ?
                    JsonSerializer.Deserialize<Dictionary<string, object>>(a.Details) : null,
                IpAddress = a.IpAddress,
                UserAgent = a.UserAgent,
                Timestamp = a.Timestamp,
                SessionId = string.Empty // Not available in audit log
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user activity for user: {UserId}", userId);
            return new List<UserActivity>();
        }
    }

    public async Task<int> GetTotalActiveUsersAsync()
    {
        try
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total active users count");
            return 0;
        }
    }

    private async Task<UserPreferencesEntity?> GetUserPreferencesEntityAsync(string userId)
    {
        return await _context.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId);
    }

    private UserPreferences MapToUserPreferences(UserPreferencesEntity? entity)
    {
        if (entity == null)
        {
            return new UserPreferences();
        }

        var preferences = new UserPreferences
        {
            PreferredChartTypes = entity.PreferredChartTypes?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            DefaultDateRange = entity.DefaultDateRange ?? "last_30_days",
            MaxRowsPerQuery = entity.MaxRowsPerQuery,
            EnableQuerySuggestions = entity.EnableQuerySuggestions,
            EnableAutoVisualization = entity.EnableAutoVisualization
        };

        if (!string.IsNullOrEmpty(entity.NotificationSettings))
        {
            try
            {
                var notificationDict = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.NotificationSettings)
                    ?? new Dictionary<string, object>();

                preferences.NotificationSettings = new NotificationSettings
                {
                    Email = notificationDict.ContainsKey("Email") && Convert.ToBoolean(notificationDict["Email"]),
                    Slack = notificationDict.ContainsKey("Slack") && Convert.ToBoolean(notificationDict["Slack"]),
                    Teams = notificationDict.ContainsKey("Teams") && Convert.ToBoolean(notificationDict["Teams"]),
                    InApp = notificationDict.ContainsKey("InApp") && Convert.ToBoolean(notificationDict["InApp"])
                };
            }
            catch
            {
                preferences.NotificationSettings = new NotificationSettings();
            }
        }

        return preferences;
    }

    private List<string> GetPermissionsForRole(string role)
    {
        return role.ToLowerInvariant() switch
        {
            "admin" => new List<string>
            {
                "query.execute",
                "query.history.view_all",
                "schema.view",
                "schema.refresh",
                "users.manage",
                "audit.view",
                "system.configure"
            },
            "analyst" => new List<string>
            {
                "query.execute",
                "query.history.view_own",
                "schema.view",
                "visualization.create"
            },
            "viewer" => new List<string>
            {
                "query.execute",
                "query.history.view_own",
                "schema.view"
            },
            _ => new List<string>
            {
                "query.execute",
                "query.history.view_own"
            }
        };
    }
}
