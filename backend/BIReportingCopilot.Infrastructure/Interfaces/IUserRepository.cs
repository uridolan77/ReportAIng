using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Interfaces;

/// <summary>
/// Infrastructure-specific user repository interface
/// </summary>
public interface IUserRepository : BIReportingCopilot.Core.Interfaces.Repository.IUserRepository
{
    // Infrastructure-specific user operations
    new Task DeleteAsync(string id, CancellationToken cancellationToken = default);
}
