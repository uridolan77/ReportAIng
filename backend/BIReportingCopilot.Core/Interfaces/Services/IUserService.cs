using BIReportingCopilot.Core.Models;
using System.Threading.Tasks;

namespace BIReportingCopilot.Core.Interfaces.Services
{
    public interface IUserService
    {
        Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default);
        Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default);
        Task DeleteUserAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> ValidateUserCredentialsAsync(string username, string password);
        Task<bool> UserExistsAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
        
        // Additional methods referenced in controllers
        Task<User?> GetUserAsync(string userId, CancellationToken cancellationToken = default);
        Task UpdateUserPreferencesAsync(string userId, object preferences, CancellationToken cancellationToken = default);
        Task<IEnumerable<object>> GetUserActivityAsync(string userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<string>> GetUserPermissionsAsync(string userId, CancellationToken cancellationToken = default);
        Task LogUserActivityAsync(string userId, string activity, CancellationToken cancellationToken = default);
    }
}
