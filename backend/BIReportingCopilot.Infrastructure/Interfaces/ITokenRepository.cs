using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Interfaces;

/// <summary>
/// Infrastructure-specific token repository interface
/// </summary>
public interface ITokenRepository : BIReportingCopilot.Core.Interfaces.Repository.ITokenRepository
{
    // Infrastructure-specific token operations can be added here if needed
}
