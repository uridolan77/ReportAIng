using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for advanced query processing with semantic analysis and optimization
/// </summary>
public interface IQueryProcessor
{
    /// <summary>
    /// Process a natural language query with semantic analysis and optimization
    /// </summary>
    Task<ProcessedQuery> ProcessQueryAsync(string naturalLanguageQuery, string userId);

    /// <summary>
    /// Generate contextual query suggestions based on user context and schema
    /// </summary>
    Task<List<string>> GenerateQuerySuggestionsAsync(string context, string userId);

    /// <summary>
    /// Calculate semantic similarity between two queries
    /// </summary>
    Task<double> CalculateSemanticSimilarityAsync(string query1, string query2);

    /// <summary>
    /// Find similar queries based on semantic analysis
    /// </summary>
    Task<List<ProcessedQuery>> FindSimilarQueriesAsync(string query, string userId, int limit = 5);
}
