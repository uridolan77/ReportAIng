using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for classifying natural language queries
/// </summary>
public interface IQueryClassifier
{
    /// <summary>
    /// Classifies a natural language query
    /// </summary>
    Task<QueryClassification> ClassifyQueryAsync(string naturalLanguageQuery);

    /// <summary>
    /// Gets query complexity score
    /// </summary>
    Task<QueryComplexityScore> GetComplexityScoreAsync(string query);

    /// <summary>
    /// Determines query intent
    /// </summary>
    Task<QueryIntent> DetermineIntentAsync(string query);

    /// <summary>
    /// Identifies required tables for a query
    /// </summary>
    Task<List<string>> IdentifyRequiredTablesAsync(string query, SchemaMetadata schema);

    /// <summary>
    /// Suggests visualization type based on query
    /// </summary>
    Task<VisualizationType> SuggestVisualizationAsync(string query);
}
