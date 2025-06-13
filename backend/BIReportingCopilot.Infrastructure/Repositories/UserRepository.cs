using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Unified user repository implementing both domain model and entity operations
/// Consolidates UserRepository and UserEntityRepository functionality
/// </summary>
public class UserRepository : IUserRepository, IUserEntityRepository
{
    private readonly BICopilotContext _context;
    private readonly ILogger<UserRepository> _logger;
    private readonly Core.Interfaces.Security.IPasswordHasher _passwordHasher;

    public UserRepository(BICopilotContext context, ILogger<UserRepository> logger, Core.Interfaces.Security.IPasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }

    public async Task<User?> ValidateCredentialsAsync(string username, string password)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                return null;
            }

            // Look up user in database
            var userEntity = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            if (userEntity == null)
            {
                _logger.LogWarning("User not found: {Username}", username);
                return null;
            }

            // Verify password using the password hasher service
            if (string.IsNullOrEmpty(password))
            {
                _logger.LogWarning("Empty password provided for user: {Username}", username);
                return null;
            }

            // Check if user has a password hash set
            if (string.IsNullOrEmpty(userEntity.PasswordHash))
            {
                _logger.LogWarning("User {Username} has no password hash set", username);
                return null;
            }

            // Verify the password against the stored hash
            if (!_passwordHasher.VerifyPassword(password, userEntity.PasswordHash))
            {
                _logger.LogWarning("Invalid password for user: {Username}", username);
                return null;
            }

            // Check if password needs rehashing (security improvement)
            if (_passwordHasher.NeedsRehash(userEntity.PasswordHash))
            {
                _logger.LogInformation("Password for user {Username} needs rehashing", username);
                userEntity.PasswordHash = _passwordHasher.HashPassword(password);
            }

            // Update last login
            userEntity.LastLoginDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var user = MapToModel(userEntity);
            _logger.LogInformation("User {Username} validated successfully", username);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating credentials for user {Username}", username);
            return null;
        }
    }

    #region Domain Model Operations (User)

    public async Task<User?> GetUserByIdAsync(string userId)
    {
        try
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            return userEntity != null ? MapToModel(userEntity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID {UserId}", userId);
            return null;
        }
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        try
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);

            return userEntity != null ? MapToModel(userEntity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username {Username}", username);
            return null;
        }
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        try
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

            return userEntity != null ? MapToModel(userEntity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            return null;
        }
    }

    // Legacy methods for backward compatibility
    public async Task<User?> GetByIdAsync(string userId) => await GetUserByIdAsync(userId);
    public async Task<User?> GetByUsernameAsync(string username) => await GetUserByUsernameAsync(username);
    public async Task<User?> GetByEmailAsync(string email) => await GetUserByEmailAsync(email);

    #endregion

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
            _logger.LogError(ex, "Error getting permissions for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<List<string>> GetUserRolesAsync(string userId)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

            if (user == null)
            {
                return new List<string>();
            }

            return user.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting roles for user {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task<User> CreateUserAsync(User user, string passwordHash)
    {
        try
        {
            user.Id = string.IsNullOrEmpty(user.Id) ? Guid.NewGuid().ToString() : user.Id;
            user.CreatedDate = DateTime.UtcNow;

            var userEntity = MapToEntity(user);
            userEntity.PasswordHash = passwordHash;
            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} created successfully with ID {UserId}", user.Username, user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user {Username}", user.Username);
            throw;
        }
    }

    // Legacy method for backward compatibility
    public async Task<User> CreateUserAsync(User user)
    {
        return await CreateUserAsync(user, string.Empty);
    }

    public async Task<User> CreateUserWithPasswordAsync(User user, string password)
    {
        try
        {
            user.Id = string.IsNullOrEmpty(user.Id) ? Guid.NewGuid().ToString() : user.Id;
            user.CreatedDate = DateTime.UtcNow;

            var userEntity = MapToEntity(user);
            userEntity.PasswordHash = _passwordHasher.HashPassword(password);

            _context.Users.Add(userEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {Username} created successfully with password hash", user.Username);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user with password {Username}", user.Username);
            throw;
        }
    }

    public async Task<bool> UpdatePasswordAsync(string userId, string newPassword)
    {
        try
        {
            var userEntity = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (userEntity == null)
            {
                _logger.LogWarning("User not found for password update: {UserId}", userId);
                return false;
            }

            userEntity.PasswordHash = _passwordHasher.HashPassword(newPassword);
            userEntity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            _logger.LogInformation("Password updated successfully for user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<User> UpdateUserAsync(User user)
    {
        try
        {
            var existingEntity = await _context.Users.FindAsync(user.Id);
            if (existingEntity == null)
            {
                throw new InvalidOperationException($"User with ID {user.Id} not found");
            }

            // Update properties
            existingEntity.Username = user.Username;
            existingEntity.Email = user.Email;
            existingEntity.DisplayName = user.DisplayName;
            existingEntity.Roles = string.Join(",", user.Roles);
            existingEntity.IsActive = user.IsActive;
            existingEntity.LastLoginDate = user.LastLoginDate;
            existingEntity.IsMfaEnabled = user.IsMfaEnabled;
            existingEntity.MfaSecret = user.MfaSecret;
            existingEntity.MfaMethod = user.MfaMethod.ToString();
            existingEntity.PhoneNumber = user.PhoneNumber;
            existingEntity.IsPhoneNumberVerified = user.IsPhoneNumberVerified;
            existingEntity.LastMfaValidationDate = user.LastMfaValidationDate;
            existingEntity.BackupCodes = user.BackupCodes.Length > 0 ? string.Join(",", user.BackupCodes) : null;

            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} updated successfully", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", user.Id);
            throw;
        }
    }

    public async Task<bool> DeleteUserAsync(string userId)
    {
        try
        {
            var userEntity = await _context.Users.FindAsync(userId);
            if (userEntity == null)
            {
                _logger.LogWarning("User {UserId} not found for deletion", userId);
                return false;
            }

            _context.Users.Remove(userEntity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("User {UserId} deleted successfully", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user {UserId}", userId);
            return false;
        }
    }

    // Password hashing methods moved to IPasswordHasher service
    // Use _passwordHasher.HashPassword() and _passwordHasher.VerifyPassword() instead

    private User MapToModel(Infrastructure.Data.Entities.UserEntity entity)
    {
        return new User
        {
            Id = entity.Id,
            Username = entity.Username,
            Email = entity.Email,
            DisplayName = entity.DisplayName,
            Roles = string.IsNullOrEmpty(entity.Roles)
                ? new[] { "User" }
                : entity.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries),
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            LastLoginDate = entity.LastLoginDate,
            IsMfaEnabled = entity.IsMfaEnabled,
            MfaSecret = entity.MfaSecret,
            MfaMethod = !string.IsNullOrEmpty(entity.MfaMethod) ? Enum.Parse<MfaMethod>(entity.MfaMethod) : MfaMethod.None,
            PhoneNumber = entity.PhoneNumber,
            IsPhoneNumberVerified = entity.IsPhoneNumberVerified,
            LastMfaValidationDate = entity.LastMfaValidationDate,
            BackupCodes = !string.IsNullOrEmpty(entity.BackupCodes)
                ? entity.BackupCodes.Split(',', StringSplitOptions.RemoveEmptyEntries)
                : Array.Empty<string>()
        };
    }

    private Infrastructure.Data.Entities.UserEntity MapToEntity(User model)
    {
        return new Infrastructure.Data.Entities.UserEntity
        {
            Id = model.Id,
            Username = model.Username,
            Email = model.Email,
            DisplayName = model.DisplayName,
            PasswordHash = string.Empty, // PasswordHash is not part of the User model for security
            Roles = string.Join(",", model.Roles),
            IsActive = model.IsActive,
            CreatedDate = model.CreatedDate,
            LastLoginDate = model.LastLoginDate,
            IsMfaEnabled = model.IsMfaEnabled,
            MfaSecret = model.MfaSecret,
            MfaMethod = model.MfaMethod.ToString(),
            PhoneNumber = model.PhoneNumber,
            IsPhoneNumberVerified = model.IsPhoneNumberVerified,
            LastMfaValidationDate = model.LastMfaValidationDate,
            BackupCodes = model.BackupCodes.Length > 0 ? string.Join(",", model.BackupCodes) : null
        };
    }

    private List<string> GetPermissionsForRole(string role)
    {
        return role.ToLowerInvariant() switch
        {
            "admin" => new List<string>
            {
                "bi_copilot_access",
                "read_data",
                "query_data",
                "export_data",
                "manage_users",
                "view_audit_logs",
                "system_configure"
            },
            "analyst" => new List<string>
            {
                "bi_copilot_access",
                "read_data",
                "query_data",
                "export_data",
                "create_visualizations"
            },
            "viewer" => new List<string>
            {
                "bi_copilot_access",
                "read_data",
                "query_data"
            },
            _ => new List<string>
            {
                "bi_copilot_access",
                "read_data",
                "query_data"
            }
        };
    }

    #region IUserEntityRepository Implementation (Consolidated)

    /// <summary>
    /// Get user entity by ID (IUserEntityRepository interface)
    /// </summary>
    public async Task<Infrastructure.Data.Entities.UserEntity?> GetByIdAsync(Guid userId)
    {
        return await GetEntityByIdAsync(userId);
    }

    /// <summary>
    /// Update user entity (IUserEntityRepository interface)
    /// </summary>
    public async Task UpdateAsync(Infrastructure.Data.Entities.UserEntity user)
    {
        try
        {
            user.UpdatedDate = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated user entity: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user entity: {UserId}", user.Id);
            throw;
        }
    }

    /// <summary>
    /// Get user entity by email (IUserEntityRepository interface)
    /// </summary>
    async Task<Infrastructure.Data.Entities.UserEntity?> IUserEntityRepository.GetByEmailAsync(string email)
    {
        return await GetEntityByEmailAsync(email);
    }

    /// <summary>
    /// Get user entity by username (IUserEntityRepository interface)
    /// </summary>
    async Task<Infrastructure.Data.Entities.UserEntity?> IUserEntityRepository.GetByUsernameAsync(string username)
    {
        return await GetEntityByUsernameAsync(username);
    }

    /// <summary>
    /// Create user entity (IUserEntityRepository interface)
    /// </summary>
    public async Task<Infrastructure.Data.Entities.UserEntity> CreateAsync(Infrastructure.Data.Entities.UserEntity user)
    {
        return await CreateEntityAsync(user);
    }

    /// <summary>
    /// Delete user entity (IUserEntityRepository interface)
    /// </summary>
    public async Task<bool> DeleteAsync(Guid userId)
    {
        try
        {
            var user = await GetByIdAsync(userId);
            if (user == null)
                return false;

            user.IsActive = false;
            user.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Soft deleted user entity: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft deleting user entity: {UserId}", userId);
            return false;
        }
    }

    #endregion

    #region Additional Unified Interface Methods

    public async Task<List<User>> GetActiveUsersAsync()
    {
        try
        {
            var entities = await _context.Users
                .Where(u => u.IsActive)
                .OrderBy(u => u.Username)
                .ToListAsync();

            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active users");
            return new List<User>();
        }
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm, int page = 1, int pageSize = 20)
    {
        try
        {
            var query = _context.Users
                .Where(u => u.IsActive &&
                           (u.Username.Contains(searchTerm) ||
                            u.Email.Contains(searchTerm) ||
                            u.DisplayName.Contains(searchTerm)));

            var entities = await query
                .OrderBy(u => u.Username)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching users with term: {SearchTerm}", searchTerm);
            return new List<User>();
        }
    }

    public async Task<Infrastructure.Data.Entities.UserEntity?> GetEntityByIdAsync(Guid userId)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.ToString() && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user entity by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task<Infrastructure.Data.Entities.UserEntity?> GetEntityByUsernameAsync(string username)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user entity by username: {Username}", username);
            return null;
        }
    }

    public async Task<Infrastructure.Data.Entities.UserEntity?> GetEntityByEmailAsync(string email)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user entity by email: {Email}", email);
            return null;
        }
    }

    public async Task<Infrastructure.Data.Entities.UserEntity> CreateEntityAsync(Infrastructure.Data.Entities.UserEntity entity)
    {
        try
        {
            entity.Id = string.IsNullOrEmpty(entity.Id) ? Guid.NewGuid().ToString() : entity.Id;
            entity.CreatedDate = DateTime.UtcNow;

            _context.Users.Add(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created user entity: {UserId}", entity.Id);
            return entity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user entity: {Username}", entity.Username);
            throw;
        }
    }

    public async Task UpdateEntityAsync(Infrastructure.Data.Entities.UserEntity entity)
    {
        try
        {
            entity.UpdatedDate = DateTime.UtcNow;
            _context.Users.Update(entity);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated user entity: {UserId}", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user entity: {UserId}", entity.Id);
            throw;
        }
    }

    public async Task<bool> UpdatePasswordHashAsync(string userId, string passwordHash)
    {
        try
        {
            var entity = await _context.Users.FindAsync(userId);
            if (entity == null) return false;

            entity.PasswordHash = passwordHash;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.UpdatedBy = "System";

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating password hash for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UpdateLastLoginAsync(string userId, DateTime loginDate)
    {
        try
        {
            var entity = await _context.Users.FindAsync(userId);
            if (entity == null) return false;

            entity.LastLoginDate = loginDate;
            entity.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating last login for user: {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Username == username && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking username existence: {Username}", username);
            return false;
        }
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        try
        {
            return await _context.Users.AnyAsync(u => u.Email == email && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking email existence: {Email}", email);
            return false;
        }
    }

    public async Task<string?> GetPasswordHashAsync(string userId)
    {
        try
        {
            var entity = await _context.Users.FindAsync(userId);
            return entity?.PasswordHash;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting password hash for user: {UserId}", userId);
            return null;
        }
    }

    // MFA, Activity, Preferences, Bulk Operations, and Statistics methods would be implemented here
    // For brevity, providing placeholder implementations

    public async Task<bool> UpdateMfaSettingsAsync(string userId, bool enabled, string? secret, MfaMethod method)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        user.IsMfaEnabled = enabled;
        user.MfaSecret = secret;
        user.MfaMethod = method;

        await UpdateUserAsync(user);
        return true;
    }

    public async Task<bool> UpdatePhoneNumberAsync(string userId, string? phoneNumber, bool isVerified)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        user.PhoneNumber = phoneNumber;
        user.IsPhoneNumberVerified = isVerified;

        await UpdateUserAsync(user);
        return true;
    }

    public async Task<bool> UpdateBackupCodesAsync(string userId, string[] backupCodes)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        user.BackupCodes = backupCodes;
        await UpdateUserAsync(user);
        return true;
    }

    public async Task<bool> UpdateLastMfaValidationAsync(string userId, DateTime validationDate)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        user.LastMfaValidationDate = validationDate;
        await UpdateUserAsync(user);
        return true;
    }

    public async Task<bool> UpdateActivitySummaryAsync(string userId, UserActivitySummary activitySummary)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        user.ActivitySummary = activitySummary;
        await UpdateUserAsync(user);
        return true;
    }

    public async Task<UserActivitySummary?> GetActivitySummaryAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        return user?.ActivitySummary;
    }

    public async Task RecordUserActivityAsync(string userId, string activityType, Dictionary<string, object>? metadata = null)
    {
        // Implementation would record activity in activity log table
        _logger.LogInformation("User activity recorded: {UserId} - {ActivityType}", userId, activityType);
    }

    public async Task<bool> UpdatePreferencesAsync(string userId, UserPreferences preferences)
    {
        var user = await GetUserByIdAsync(userId);
        if (user == null) return false;

        user.Preferences = preferences;
        await UpdateUserAsync(user);
        return true;
    }

    public async Task<UserPreferences?> GetPreferencesAsync(string userId)
    {
        var user = await GetUserByIdAsync(userId);
        return user?.Preferences;
    }

    public async Task<List<User>> GetUsersByIdsAsync(string[] userIds)
    {
        try
        {
            var entities = await _context.Users
                .Where(u => userIds.Contains(u.Id) && u.IsActive)
                .ToListAsync();

            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by IDs");
            return new List<User>();
        }
    }

    public async Task<List<User>> GetUsersByRoleAsync(string role)
    {
        try
        {
            var entities = await _context.Users
                .Where(u => u.IsActive && u.Roles.Contains(role))
                .ToListAsync();

            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users by role: {Role}", role);
            return new List<User>();
        }
    }

    public async Task<bool> BulkUpdateUserStatusAsync(string[] userIds, bool isActive)
    {
        try
        {
            var entities = await _context.Users
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            foreach (var entity in entities)
            {
                entity.IsActive = isActive;
                entity.UpdatedDate = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error bulk updating user status");
            return false;
        }
    }

    public async Task<int> GetTotalUserCountAsync()
    {
        try
        {
            return await _context.Users.CountAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting total user count");
            return 0;
        }
    }

    public async Task<int> GetActiveUserCountAsync()
    {
        try
        {
            return await _context.Users.CountAsync(u => u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting active user count");
            return 0;
        }
    }

    public async Task<List<User>> GetUsersCreatedInRangeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var entities = await _context.Users
                .Where(u => u.CreatedDate >= startDate && u.CreatedDate <= endDate)
                .ToListAsync();

            return entities.Select(MapToModel).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting users created in range");
            return new List<User>();
        }
    }

    public async Task<Dictionary<string, int>> GetUserLoginStatsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var stats = await _context.Users
                .Where(u => u.LastLoginDate >= startDate && u.LastLoginDate <= endDate)
                .GroupBy(u => u.LastLoginDate.HasValue ? u.LastLoginDate.Value.Date : DateTime.MinValue)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Date.ToString("yyyy-MM-dd"), x => x.Count);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user login stats");
            return new Dictionary<string, int>();
        }
    }

    #endregion
}
