using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for enhanced query processing with comprehensive tracking
/// </summary>
public interface IEnhancedQueryProcessingService
{
    /// <summary>
    /// Process a query with comprehensive tracking and enhanced SQL generation
    /// </summary>
    /// <param name="userQuestion">User's natural language question</param>
    /// <param name="userId">User identifier</param>
    /// <param name="traceId">Optional trace identifier for tracking</param>
    /// <returns>Enhanced query result with generated SQL and metadata</returns>
    Task<EnhancedQueryResult> ProcessQueryAsync(
        string userQuestion,
        string userId,
        string? traceId = null);
}
