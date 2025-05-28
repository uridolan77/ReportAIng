using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;

namespace BIReportingCopilot.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly BICopilotContext _context;
    private readonly ILogger<UserRepository> _logger;
    private readonly IPasswordHasher _passwordHasher;

    public UserRepository(BICopilotContext context, ILogger<UserRepository> logger, IPasswordHasher passwordHasher)
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

    public async Task<User?> GetByIdAsync(string userId)
    {
        try
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId);

            return userEntity != null ? MapToModel(userEntity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID {UserId}", userId);
            return null;
        }
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == username);

            return userEntity != null ? MapToModel(userEntity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username {Username}", username);
            return null;
        }
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        try
        {
            var userEntity = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email == email);

            return userEntity != null ? MapToModel(userEntity) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email {Email}", email);
            return null;
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

    public async Task<User> CreateUserAsync(User user)
    {
        try
        {
            user.Id = string.IsNullOrEmpty(user.Id) ? Guid.NewGuid().ToString() : user.Id;
            user.CreatedDate = DateTime.UtcNow;

            var userEntity = MapToEntity(user);
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

    private User MapToModel(UserEntity entity)
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

    private UserEntity MapToEntity(User model)
    {
        return new UserEntity
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
}
