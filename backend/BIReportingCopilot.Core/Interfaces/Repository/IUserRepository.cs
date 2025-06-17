using BIReportingCopilot.Core.Models;
using System.Threading.Tasks;

namespace BIReportingCopilot.Core.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task<User> UpdateAsync(User user, CancellationToken cancellationToken = default);
        Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> ExistsAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> ExistsByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<User?> ValidateCredentialsAsync(string username, string password);
        Task<List<string>> GetUserPermissionsAsync(string userId);
        Task<User> UpdateUserAsync(User user);
    }
}
