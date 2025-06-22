using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Interfaces;

/// <summary>
/// Infrastructure-specific user repository interface
/// </summary>
public interface IUserRepository
{
    // Core user operations
    Task<User?> ValidateCredentialsAsync(string username, string password);
    Task<List<string>> GetUserPermissionsAsync(string userId);
    Task<List<string>> GetUserRolesAsync(string userId);
    Task<User?> GetUserByIdAsync(string userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<User?> GetUserByEmailAsync(string email);
    Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<User> CreateUserAsync(User user, string passwordHash);
    Task<User> CreateUserWithPasswordAsync(User user, string password);
    Task<bool> UpdatePasswordAsync(string userId, string newPassword);
    Task<User> UpdateUserAsync(User user);
    Task<bool> DeleteUserAsync(string userId);

    // Infrastructure-specific user operations
    Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
