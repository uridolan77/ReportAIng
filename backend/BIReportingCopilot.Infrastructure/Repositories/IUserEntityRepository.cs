using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Interfaces.Repository;

namespace BIReportingCopilot.Infrastructure.Repositories;

/// <summary>
/// Repository interface for entity-based user operations needed by MFA service
/// </summary>
public interface IUserEntityRepository : IGuidKeyRepository<UserEntity>
{
    // User entity-specific queries
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByUsernameAsync(string username);
}
