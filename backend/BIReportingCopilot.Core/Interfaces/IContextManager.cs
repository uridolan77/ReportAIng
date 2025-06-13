using BIReportingCopilot.Core.Models;
using UserContext = BIReportingCopilot.Core.Models.UserContext;
using SchemaContext = BIReportingCopilot.Core.Models.SchemaContext;
using QueryPattern = BIReportingCopilot.Core.Models.QueryPattern;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for managing user and schema context
/// </summary>
public interface IContextManager
{
    /// <summary>
    /// Gets user context for a specific user
    /// </summary>
    Task<UserContext> GetUserContextAsync(string userId);

    /// <summary>
    /// Gets relevant schema information based on a query
    /// </summary>
    Task<SchemaContext> GetRelevantSchemaAsync(string query, SchemaMetadata fullSchema);

    /// <summary>
    /// Gets query patterns for a user
    /// </summary>
    Task<List<QueryPattern>> GetQueryPatternsAsync(string userId);

    /// <summary>
    /// Updates user context with new information
    /// </summary>
    Task UpdateUserContextAsync(string userId, UserContext context);

    /// <summary>
    /// Caches schema context for future use
    /// </summary>
    Task CacheSchemaContextAsync(string query, SchemaContext context);

    /// <summary>
    /// Gets cached schema context if available
    /// </summary>
    Task<SchemaContext?> GetCachedSchemaContextAsync(string query);
}
