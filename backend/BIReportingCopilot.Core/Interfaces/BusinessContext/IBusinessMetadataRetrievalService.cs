using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Core.Interfaces.BusinessContext;

/// <summary>
/// Service for retrieving relevant business metadata based on context analysis
/// </summary>
public interface IBusinessMetadataRetrievalService
{
    /// <summary>
    /// Get relevant business metadata for a given context profile
    /// </summary>
    /// <param name="profile">Business context profile</param>
    /// <param name="maxTables">Maximum number of tables to return</param>
    /// <returns>Contextual business schema</returns>
    Task<ContextualBusinessSchema> GetRelevantBusinessMetadataAsync(
        BusinessContextProfile profile,
        int maxTables = 5);

    /// <summary>
    /// Find relevant tables based on business context
    /// </summary>
    /// <param name="profile">Business context profile</param>
    /// <param name="maxTables">Maximum number of tables to return</param>
    /// <returns>List of relevant business tables</returns>
    Task<List<BusinessTableInfoDto>> FindRelevantTablesAsync(
        BusinessContextProfile profile, 
        int maxTables = 5);

    /// <summary>
    /// Find relevant columns for specific tables
    /// </summary>
    /// <param name="tableIds">List of table IDs</param>
    /// <param name="profile">Business context profile</param>
    /// <returns>List of relevant columns</returns>
    Task<List<BusinessColumnInfo>> FindRelevantColumnsAsync(
        List<long> tableIds, 
        BusinessContextProfile profile);

    /// <summary>
    /// Find relevant glossary terms
    /// </summary>
    /// <param name="businessTerms">List of business terms to search</param>
    /// <returns>List of relevant glossary terms</returns>
    Task<List<BusinessGlossaryDto>> FindRelevantGlossaryTermsAsync(
        List<string> businessTerms);

    /// <summary>
    /// Discover relationships between tables
    /// </summary>
    /// <param name="tableNames">List of table names</param>
    /// <returns>List of table relationships</returns>
    Task<List<TableRelationship>> DiscoverTableRelationshipsAsync(
        List<string> tableNames);
}
