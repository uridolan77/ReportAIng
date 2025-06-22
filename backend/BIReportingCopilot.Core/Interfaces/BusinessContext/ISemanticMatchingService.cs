using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Core.Interfaces.BusinessContext;

/// <summary>
/// Service for semantic matching and similarity calculations
/// </summary>
public interface ISemanticMatchingService
{
    /// <summary>
    /// Find semantically similar terms
    /// </summary>
    /// <param name="searchTerm">Term to search for</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>List of similar terms with scores</returns>
    Task<List<(string term, double score)>> FindSimilarTermsAsync(
        string searchTerm, 
        int topK = 5);

    /// <summary>
    /// Calculate semantic similarity between two texts
    /// </summary>
    /// <param name="text1">First text</param>
    /// <param name="text2">Second text</param>
    /// <returns>Similarity score between 0 and 1</returns>
    Task<double> CalculateSemanticSimilarityAsync(string text1, string text2);

    /// <summary>
    /// Perform semantic search on business tables
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>List of relevant business tables</returns>
    Task<List<BusinessTableInfoDto>> SemanticTableSearchAsync(
        string query, 
        int topK = 10);

    /// <summary>
    /// Perform semantic search on business columns
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="tableId">Optional table ID to limit search</param>
    /// <param name="topK">Number of top results to return</param>
    /// <returns>List of relevant business columns</returns>
    Task<List<BusinessColumnInfo>> SemanticColumnSearchAsync(
        string query, 
        long? tableId = null, 
        int topK = 10);

    /// <summary>
    /// Generate embedding vector for text
    /// </summary>
    /// <param name="text">Text to generate embedding for</param>
    /// <returns>Embedding vector</returns>
    Task<double[]> GenerateEmbeddingAsync(string text);
}
