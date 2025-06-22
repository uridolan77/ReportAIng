using BIReportingCopilot.Core.Models;
using System.Threading.Tasks;

namespace BIReportingCopilot.Core.Interfaces.Repository
{
    /// <summary>
    /// Repository interface for user operations extending generic repository
    /// </summary>
    public interface IUserRepository : IStringKeyRepository<User>
    {
        // User-specific queries
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);

        // Authentication and authorization
        Task<User?> ValidateCredentialsAsync(string username, string password);
        Task<List<string>> GetUserPermissionsAsync(string userId);

        // Legacy method for backward compatibility
        Task<User> UpdateUserAsync(User user);
    }
}
