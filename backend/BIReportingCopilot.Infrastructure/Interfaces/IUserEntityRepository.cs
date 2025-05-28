using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Interfaces;

/// <summary>
/// Repository interface for entity-based user operations needed by MFA service
/// </summary>
public interface IUserEntityRepository
{
    Task<UserEntity?> GetByIdAsync(Guid userId);
    Task UpdateAsync(UserEntity user);
    Task<UserEntity?> GetByEmailAsync(string email);
    Task<UserEntity?> GetByUsernameAsync(string username);
    Task<UserEntity> CreateAsync(UserEntity user);
    Task<bool> DeleteAsync(Guid userId);
}
