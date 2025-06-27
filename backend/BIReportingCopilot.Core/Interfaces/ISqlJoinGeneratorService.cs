using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for generating SQL JOINs based on foreign key relationships
/// </summary>
public interface ISqlJoinGeneratorService
{
    /// <summary>
    /// Generate SQL JOINs for a list of tables based on their relationships
    /// </summary>
    /// <param name="tableNames">List of table names to join</param>
    /// <param name="primaryTable">Optional primary table to start from</param>
    /// <param name="strategy">JOIN generation strategy</param>
    /// <returns>SQL JOIN result with generated clause and metadata</returns>
    Task<SqlJoinResult> GenerateJoinsAsync(
        List<string> tableNames,
        string? primaryTable = null,
        JoinStrategy strategy = JoinStrategy.Optimal);

    /// <summary>
    /// Generate table aliases for SQL queries
    /// </summary>
    /// <param name="tableNames">List of table names</param>
    /// <returns>Dictionary mapping table names to aliases</returns>
    Dictionary<string, string> GenerateTableAliases(List<string> tableNames);

    /// <summary>
    /// Validate that all required tables can be joined
    /// </summary>
    /// <param name="tableNames">List of table names to validate</param>
    /// <returns>Validation result with connectivity information</returns>
    Task<JoinValidationResult> ValidateJoinabilityAsync(List<string> tableNames);
}


