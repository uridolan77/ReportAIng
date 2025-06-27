using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for discovering and managing foreign key relationships between database tables
/// </summary>
public interface IForeignKeyRelationshipService
{
    /// <summary>
    /// Discover all foreign key relationships in the database
    /// </summary>
    /// <param name="connectionStringName">Name of the connection string to use</param>
    /// <returns>List of all foreign key relationships</returns>
    Task<List<ForeignKeyRelationship>> DiscoverAllForeignKeyRelationshipsAsync(
        string connectionStringName = "BIDatabase");

    /// <summary>
    /// Find related tables for a specific table through foreign key relationships
    /// </summary>
    /// <param name="tableName">Name of the table to find relationships for</param>
    /// <param name="maxDepth">Maximum depth to traverse relationships</param>
    /// <param name="connectionStringName">Name of the connection string to use</param>
    /// <returns>List of related tables with relationship information</returns>
    Task<List<RelatedTableInfo>> FindRelatedTablesAsync(
        string tableName, 
        int maxDepth = 2,
        string connectionStringName = "BIDatabase");

    /// <summary>
    /// Get foreign key relationships for specific tables
    /// </summary>
    /// <param name="tableNames">List of table names to get relationships for</param>
    /// <param name="connectionStringName">Name of the connection string to use</param>
    /// <returns>List of foreign key relationships involving the specified tables</returns>
    Task<List<ForeignKeyRelationship>> GetRelationshipsForTablesAsync(
        List<string> tableNames,
        string connectionStringName = "BIDatabase");

    /// <summary>
    /// Generate optimal join paths between tables
    /// </summary>
    /// <param name="tableNames">List of table names to generate join paths for</param>
    /// <param name="connectionStringName">Name of the connection string to use</param>
    /// <returns>List of optimal join paths between the tables</returns>
    Task<List<JoinPath>> GenerateJoinPathsAsync(
        List<string> tableNames,
        string connectionStringName = "BIDatabase");
}
