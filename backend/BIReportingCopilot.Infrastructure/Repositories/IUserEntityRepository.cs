using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Interfaces.Repository;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository interface for entity-based user operations needed by MFA service
/// </summary>
public interface IUserEntityRepository
{
    // Basic CRUD operations for UserEntity
    Task<UserEntity?> GetByIdAsync(Guid id);
    Task<UserEntity> CreateAsync(UserEntity entity);
    Task UpdateAsync(UserEntity entity);
    Task<bool> DeleteAsync(Guid id);

    // User entity-specific queries
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByUsernameAsync(string username);
}
