using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Interfaces;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository for entity-based user operations
/// </summary>
public class UserEntityRepository : IUserEntityRepository
{    private readonly BICopilotContext _context;
    private readonly ILogger<UserEntityRepository> _logger;

    public UserEntityRepository(BICopilotContext context, ILogger<UserEntityRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UserEntity?> GetByIdAsync(Guid userId)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId.ToString() && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by ID: {UserId}", userId);
            return null;
        }
    }

    public async Task UpdateAsync(UserEntity user)
    {
        try
        {
            user.UpdatedDate = DateTime.UtcNow;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Updated user: {UserId}", user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user: {UserId}", user.Id);
            throw;
        }
    }

    public async Task<UserEntity?> GetByEmailAsync(string email)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by email: {Email}", email);
            return null;
        }
    }

    public async Task<UserEntity?> GetByUsernameAsync(string username)
    {
        try
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user by username: {Username}", username);
            return null;
        }
    }

    public async Task<UserEntity> CreateAsync(UserEntity user)
    {
        try
        {
            user.Id = string.IsNullOrEmpty(user.Id) ? Guid.NewGuid().ToString() : user.Id;
            user.CreatedDate = DateTime.UtcNow;
            
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Created user: {UserId}", user.Id);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating user: {Username}", user.Username);
            throw;
        }
    }

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
            
            _logger.LogInformation("Deleted user: {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting user: {UserId}", userId);
            return false;
        }
    }
}
