using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using BIReportingCopilot.Infrastructure.Interfaces;
using ContextType = BIReportingCopilot.Infrastructure.Data.Contexts.ContextType;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Authentication;

/// <summary>
/// Enhanced UserService using bounded contexts for better performance and maintainability
/// </summary>
public class UserService : IUserService
{
    private readonly ILogger<UserService> _logger;
    private readonly IDbContextFactory _contextFactory;
    private readonly BIReportingCopilot.Core.Interfaces.Security.IAuditService _auditService;

    public UserService(
        ILogger<UserService> logger,
        IDbContextFactory contextFactory,
        BIReportingCopilot.Core.Interfaces.Security.IAuditService auditService)
    {
        _logger = logger;
        _contextFactory = contextFactory;
        _auditService = auditService;
    }

    public async Task<UserInfo?> GetUserAsync(string userId)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;

                var userEntity = await securityContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

                if (userEntity == null)
                {
                    return null;
                }

                var preferences = await securityContext.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);

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
            });
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
                existingPrefs = new Infrastructure.Data.Entities.UserPreferencesEntity
                {
                    UserId = userId,
                    CreatedDate = DateTime.UtcNow
                };
                await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
                {
                    var userContext = (SecurityDbContext)dbContext;
                    userContext.UserPreferences.Add(existingPrefs);
                    await userContext.SaveChangesAsync();
                });
            }

            // Update preferences
            existingPrefs.PreferredChartTypes = string.Join(",", preferences.PreferredChartTypes);
            existingPrefs.DefaultDateRange = preferences.DefaultDateRange;
            existingPrefs.MaxRowsPerQuery = preferences.MaxRowsPerQuery;
            existingPrefs.EnableQuerySuggestions = preferences.EnableQuerySuggestions;
            existingPrefs.EnableAutoVisualization = preferences.EnableAutoVisualization;
            existingPrefs.NotificationSettings = JsonSerializer.Serialize(preferences.NotificationSettings);
            existingPrefs.UpdatedDate = DateTime.UtcNow;

            await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var userContext = (SecurityDbContext)dbContext;
                // Update the existing preferences entity
                var existingEntity = await userContext.UserPreferences
                    .FirstOrDefaultAsync(p => p.UserId == userId);
                if (existingEntity != null)
                {
                    existingEntity.PreferredChartTypes = existingPrefs.PreferredChartTypes;
                    existingEntity.DefaultDateRange = existingPrefs.DefaultDateRange;
                    existingEntity.MaxRowsPerQuery = existingPrefs.MaxRowsPerQuery;
                    existingEntity.EnableQuerySuggestions = existingPrefs.EnableQuerySuggestions;
                    existingEntity.EnableAutoVisualization = existingPrefs.EnableAutoVisualization;
                    existingEntity.NotificationSettings = existingPrefs.NotificationSettings;
                    existingEntity.UpdatedDate = existingPrefs.UpdatedDate;
                }
                await userContext.SaveChangesAsync();
            });

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
            var user = await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var userContext = (SecurityDbContext)dbContext;
                return await userContext.Users
                    .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);
            });

            if (user == null)
            {
                return new List<string>();
            }

            if (user == null)
            {
                return new List<string>();
            }

            var roles = user.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
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
            var sessionEntity = new Infrastructure.Data.Entities.UserSessionEntity
            {
                SessionId = sessionId,
                UserId = userId,
                StartTime = DateTime.UtcNow,
                LastActivity = DateTime.UtcNow,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                IsActive = true
            };

            await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var userContext = (SecurityDbContext)dbContext;
                userContext.UserSessions.Add(sessionEntity);
                await userContext.SaveChangesAsync();
            });

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
            var sessionEntity = await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var userContext = (SecurityDbContext)dbContext;
                return await userContext.UserSessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);
            });

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
            await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var userContext = (SecurityDbContext)dbContext;
                var sessionEntity = await userContext.UserSessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId && s.IsActive);

                if (sessionEntity != null)
                {
                    sessionEntity.LastActivity = DateTime.UtcNow;
                    await userContext.SaveChangesAsync();
                }
            });
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
            await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var userContext = (SecurityDbContext)dbContext;
                var sessionEntity = await userContext.UserSessions
                    .FirstOrDefaultAsync(s => s.SessionId == sessionId);

                if (sessionEntity != null)
                {
                    sessionEntity.IsActive = false;
                    sessionEntity.UpdatedDate = DateTime.UtcNow;
                    await userContext.SaveChangesAsync();

                    await _auditService.LogAsync("SESSION_ENDED", sessionEntity.UserId, "UserSession", sessionId);
                }
            });
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
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var userContext = (SecurityDbContext)dbContext;
                var query = userContext.AuditLog.Where(a => a.UserId == userId);

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
            });
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
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
            {
                var userContext = (SecurityDbContext)dbContext;
                return await userContext.Users.CountAsync(u => u.IsActive);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total active users count");
            return 0;
        }
    }

    private async Task<Infrastructure.Data.Entities.UserPreferencesEntity?> GetUserPreferencesEntityAsync(string userId)
    {
        return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async dbContext =>
        {
            var userContext = (SecurityDbContext)dbContext;
            return await userContext.UserPreferences
                .FirstOrDefaultAsync(p => p.UserId == userId);
        });
    }

    private UserPreferences MapToUserPreferences(Infrastructure.Data.Entities.UserPreferencesEntity? entity)
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

    #region Missing Interface Method Implementations

    /// <summary>
    /// Get user by ID (IUserService interface)
    /// </summary>
    public async Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;
                var userEntity = await securityContext.Users
                    .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);

                return userEntity != null ? MapToUser(userEntity) : null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", id);
            return null;
        }
    }

    /// <summary>
    /// Get user by email (IUserService interface)
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;
                var userEntity = await securityContext.Users
                    .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);

                return userEntity != null ? MapToUser(userEntity) : null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    /// <summary>
    /// Get user by username (IUserService interface)
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;
                var userEntity = await securityContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive, cancellationToken);

                return userEntity != null ? MapToUser(userEntity) : null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username: {Username}", username);
            return null;
        }
    }

    /// <summary>
    /// Validate user credentials (IUserService interface)
    /// </summary>
    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;
                var userEntity = await securityContext.Users
                    .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

                if (userEntity != null && VerifyPassword(password, userEntity.PasswordHash))
                {
                    // Update last login
                    userEntity.LastLoginDate = DateTime.UtcNow;
                    await securityContext.SaveChangesAsync();

                    return MapToUser(userEntity);
                }

                return null;
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating credentials for username: {Username}", username);
            return null;
        }
    }

    /// <summary>
    /// Create user (IUserService interface)
    /// </summary>
    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            var userEntity = new UserEntity
            {
                Id = user.Id ?? Guid.NewGuid().ToString(),
                Username = user.Username,
                Email = user.Email,
                DisplayName = user.DisplayName,
                PasswordHash = string.Empty, // PasswordHash should be set separately for security
                Roles = string.Join(",", user.Roles),
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };

            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;
                securityContext.Users.Add(userEntity);
                await securityContext.SaveChangesAsync(cancellationToken);

                await _auditService.LogAsync("USER_CREATED", userEntity.Id, "User", userEntity.Id);

                return MapToUser(userEntity);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", user.Username);
            throw;
        }
    }

    /// <summary>
    /// Update user (IUserService interface)
    /// </summary>
    public async Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;
                var userEntity = await securityContext.Users
                    .FirstOrDefaultAsync(u => u.Id == user.Id, cancellationToken);

                if (userEntity == null)
                    throw new InvalidOperationException($"User not found: {user.Id}");

                userEntity.Username = user.Username;
                userEntity.Email = user.Email;
                userEntity.DisplayName = user.DisplayName;
                userEntity.Roles = string.Join(",", user.Roles);
                userEntity.UpdatedDate = DateTime.UtcNow;

                await securityContext.SaveChangesAsync(cancellationToken);

                await _auditService.LogAsync("USER_UPDATED", userEntity.Id, "User", userEntity.Id);

                return MapToUser(userEntity);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Delete user (IUserService interface)
    /// </summary>
    public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;
                var userEntity = await securityContext.Users
                    .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

                if (userEntity != null)
                {
                    userEntity.IsActive = false;
                    userEntity.UpdatedDate = DateTime.UtcNow;
                    await securityContext.SaveChangesAsync(cancellationToken);

                    await _auditService.LogAsync("USER_DELETED", id, "User", id);
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", id);
            throw;
        }
    }

    /// <summary>
    /// Update user (IUserService interface - alternative signature)
    /// </summary>
    public async Task<User> UpdateUserAsync(User user)
    {
        return await UpdateAsync(user);
    }

    #endregion

    #region Helper Methods

    private User MapToUser(BIReportingCopilot.Infrastructure.Data.Entities.UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            DisplayName = entity.DisplayName,
            // PasswordHash is not included in User model for security reasons
            Roles = entity.Roles?.Split(',', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>(),
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            LastLoginDate = entity.LastLoginDate
        };
    }

    private bool VerifyPassword(string password, string hash)
    {
        // Simple implementation - in production, use proper password hashing
        // Simple hash comparison - in production, use proper password hashing library
        return password.GetHashCode().ToString() == hash;
    }

    #endregion
}
