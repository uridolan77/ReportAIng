using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.Interfaces.BusinessContext;

/// <summary>
/// Service for generating and managing vector embeddings for semantic search
/// </summary>
public interface IVectorEmbeddingService
{
    /// <summary>
    /// Generate vector embedding for text content
    /// </summary>
    /// <param name="text">Text to generate embedding for</param>
    /// <param name="modelVersion">Embedding model version</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Vector embedding as float array</returns>
    Task<float[]> GenerateEmbeddingAsync(string text, string modelVersion = "text-embedding-ada-002", CancellationToken cancellationToken = default);

    /// <summary>
    /// Store vector embedding in database
    /// </summary>
    /// <param name="entityType">Type of entity (e.g., "PromptTemplate", "QueryExample")</param>
    /// <param name="entityId">ID of the entity</param>
    /// <param name="embedding">Vector embedding</param>
    /// <param name="sourceText">Original text that was embedded</param>
    /// <param name="modelVersion">Model version used</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>ID of stored embedding</returns>
    Task<long> StoreEmbeddingAsync(string entityType, long entityId, float[] embedding, string sourceText, string modelVersion = "text-embedding-ada-002", CancellationToken cancellationToken = default);

    /// <summary>
    /// Find similar entities using vector similarity search
    /// </summary>
    /// <param name="queryEmbedding">Query vector embedding</param>
    /// <param name="entityType">Type of entities to search</param>
    /// <param name="topK">Number of top results to return</param>
    /// <param name="similarityThreshold">Minimum similarity threshold (0-1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of similar entities with similarity scores</returns>
    Task<List<VectorSearchResult>> FindSimilarAsync(float[] queryEmbedding, string entityType, int topK = 10, double similarityThreshold = 0.7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find similar entities by text query
    /// </summary>
    /// <param name="queryText">Text query to search for</param>
    /// <param name="entityType">Type of entities to search</param>
    /// <param name="topK">Number of top results to return</param>
    /// <param name="similarityThreshold">Minimum similarity threshold (0-1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of similar entities with similarity scores</returns>
    Task<List<VectorSearchResult>> FindSimilarByTextAsync(string queryText, string entityType, int topK = 10, double similarityThreshold = 0.7, CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculate cosine similarity between two vectors
    /// </summary>
    /// <param name="vector1">First vector</param>
    /// <param name="vector2">Second vector</param>
    /// <returns>Cosine similarity score (0-1)</returns>
    double CalculateCosineSimilarity(float[] vector1, float[] vector2);

    /// <summary>
    /// Generate embeddings for all prompt templates
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of embeddings generated</returns>
    Task<int> GeneratePromptTemplateEmbeddingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate embeddings for all query examples
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of embeddings generated</returns>
    Task<int> GenerateQueryExampleEmbeddingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate embeddings for business metadata
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of embeddings generated</returns>
    Task<int> GenerateBusinessMetadataEmbeddingsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Find most relevant prompt templates for a query
    /// </summary>
    /// <param name="userQuery">User's natural language query</param>
    /// <param name="intentType">Query intent type</param>
    /// <param name="topK">Number of templates to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant prompt templates</returns>
    Task<List<RelevantPromptTemplate>> FindRelevantPromptTemplatesAsync(string userQuery, string? intentType = null, int topK = 3, CancellationToken cancellationToken = default);

    /// <summary>
    /// Find most relevant query examples for a query
    /// </summary>
    /// <param name="userQuery">User's natural language query</param>
    /// <param name="domain">Business domain</param>
    /// <param name="topK">Number of examples to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of relevant query examples</returns>
    Task<List<RelevantQueryExample>> FindRelevantQueryExamplesAsync(string userQuery, string? domain = null, int topK = 5, CancellationToken cancellationToken = default);
}
