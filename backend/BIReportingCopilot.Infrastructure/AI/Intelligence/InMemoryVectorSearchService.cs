using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models;
using System.Collections.Concurrent;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI.Intelligence;

/// <summary>
/// In-memory vector search service for semantic similarity matching
/// Provides high-performance vector operations with cosine similarity
/// Can be extended to integrate with external vector databases (Pinecone, Weaviate, etc.)
/// </summary>
public class InMemoryVectorSearchService : IVectorSearchService
{
    private readonly ILogger<InMemoryVectorSearchService> _logger;
    private readonly IAIService _aiService;
    private readonly ConcurrentDictionary<string, VectorEntry> _vectorIndex;
    private readonly object _indexLock = new object();
    private VectorSearchMetrics _metrics;

    public InMemoryVectorSearchService(
        ILogger<InMemoryVectorSearchService> logger,
        IAIService aiService)
    {
        _logger = logger;
        _aiService = aiService;
        _vectorIndex = new ConcurrentDictionary<string, VectorEntry>();
        _metrics = new VectorSearchMetrics();
    }

    /// <summary>
    /// Generate embedding vector using AI service
    /// </summary>
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🧠 Generating embedding for text: {Text}", text.Substring(0, Math.Min(50, text.Length)));
            
            var startTime = DateTime.UtcNow;
            
            // Use AI service to generate embeddings
            // For now, we'll create a simple hash-based embedding as a placeholder
            // In production, this would call OpenAI's embedding API or similar
            var embedding = await GenerateSimpleEmbeddingAsync(text);
            
            var processingTime = DateTime.UtcNow - startTime;
            _logger.LogDebug("🧠 Embedding generated in {Time}ms, dimensions: {Dimensions}", 
                processingTime.TotalMilliseconds, embedding.Length);
            
            return embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating embedding for text");
            throw;
        }
    }

    /// <summary>
    /// Store query embedding with metadata
    /// </summary>
    public Task<string> StoreQueryEmbeddingAsync(
        string queryText,
        string sqlQuery,
        QueryResponse response,
        float[] embedding,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddingId = Guid.NewGuid().ToString();

            var vectorEntry = new VectorEntry
            {
                Id = embeddingId,
                QueryText = queryText,
                SqlQuery = sqlQuery,
                Response = response,
                Embedding = embedding,
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                AccessCount = 0,
                Metadata = new Dictionary<string, object>
                {
                    ["query_length"] = queryText.Length,
                    ["sql_length"] = sqlQuery.Length,
                    ["execution_time"] = response.ExecutionTimeMs,
                    ["confidence"] = response.Confidence
                }
            };

            _vectorIndex.TryAdd(embeddingId, vectorEntry);

            lock (_indexLock)
            {
                _metrics.TotalEmbeddings++;
            }

            _logger.LogDebug("💾 Stored embedding {Id} for query: {Query}",
                embeddingId, queryText.Substring(0, Math.Min(50, queryText.Length)));

            return Task.FromResult(embeddingId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error storing query embedding");
            throw;
        }
    }

    /// <summary>
    /// Find semantically similar queries using cosine similarity
    /// </summary>
    public Task<List<SemanticSearchResult>> FindSimilarQueriesAsync(
        float[] queryEmbedding,
        double similarityThreshold = 0.8,
        int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var startTime = DateTime.UtcNow;
            var results = new List<SemanticSearchResult>();

            _logger.LogDebug("🔍 Searching {Count} embeddings with threshold {Threshold:P2}",
                _vectorIndex.Count, similarityThreshold);

            // Calculate similarity with all stored embeddings
            var similarities = new List<(string Id, double Similarity, VectorEntry Entry)>();

            foreach (var kvp in _vectorIndex)
            {
                var similarity = CalculateCosineSimilarity(queryEmbedding, kvp.Value.Embedding);
                if (similarity >= similarityThreshold)
                {
                    similarities.Add((kvp.Key, similarity, kvp.Value));
                }
            }

            // Sort by similarity and take top results
            var topResults = similarities
                .OrderByDescending(s => s.Similarity)
                .Take(maxResults)
                .ToList();

            foreach (var (id, similarity, entry) in topResults)
            {
                // Update access statistics
                entry.LastAccessed = DateTime.UtcNow;
                entry.AccessCount++;

                results.Add(new SemanticSearchResult
                {
                    EmbeddingId = id,
                    OriginalQuery = entry.QueryText,
                    SqlQuery = entry.SqlQuery,
                    CachedResponse = entry.Response,
                    SimilarityScore = similarity,
                    CreatedAt = entry.CreatedAt,
                    LastAccessed = entry.LastAccessed,
                    AccessCount = entry.AccessCount,
                    Metadata = entry.Metadata
                });
            }

            var searchTime = DateTime.UtcNow - startTime;

            lock (_indexLock)
            {
                _metrics.TotalSearches++;
                _metrics.AverageSearchTime = (_metrics.AverageSearchTime * (_metrics.TotalSearches - 1) + searchTime.TotalMilliseconds) / _metrics.TotalSearches;
            }

            _logger.LogDebug("🔍 Found {Count} similar queries in {Time}ms",
                results.Count, searchTime.TotalMilliseconds);

            return Task.FromResult(results);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finding similar queries");
            return Task.FromResult(new List<SemanticSearchResult>());
        }
    }

    /// <summary>
    /// Find similar queries by text (convenience method)
    /// </summary>
    public async Task<List<SemanticSearchResult>> FindSimilarQueriesByTextAsync(
        string queryText, 
        double similarityThreshold = 0.8, 
        int maxResults = 5,
        CancellationToken cancellationToken = default)
    {
        var embedding = await GenerateEmbeddingAsync(queryText, cancellationToken);
        return await FindSimilarQueriesAsync(embedding, similarityThreshold, maxResults, cancellationToken);
    }

    /// <summary>
    /// Calculate cosine similarity between two vectors
    /// </summary>
    public double CalculateCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
        {
            throw new ArgumentException("Vectors must have the same dimensions");
        }

        double dotProduct = 0.0;
        double magnitude1 = 0.0;
        double magnitude2 = 0.0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0.0 || magnitude2 == 0.0)
        {
            return 0.0;
        }

        return dotProduct / (magnitude1 * magnitude2);
    }

    /// <summary>
    /// Update embedding for existing query
    /// </summary>
    public Task<bool> UpdateQueryEmbeddingAsync(
        string embeddingId,
        float[] newEmbedding,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (_vectorIndex.TryGetValue(embeddingId, out var entry))
            {
                entry.Embedding = newEmbedding;
                entry.LastAccessed = DateTime.UtcNow;

                _logger.LogDebug("🔄 Updated embedding {Id}", embeddingId);
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating embedding {Id}", embeddingId);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Delete embedding by ID
    /// </summary>
    public Task<bool> DeleteQueryEmbeddingAsync(string embeddingId, CancellationToken cancellationToken = default)
    {
        try
        {
            var removed = _vectorIndex.TryRemove(embeddingId, out _);

            if (removed)
            {
                lock (_indexLock)
                {
                    _metrics.TotalEmbeddings--;
                }

                _logger.LogDebug("🗑️ Deleted embedding {Id}", embeddingId);
            }

            return Task.FromResult(removed);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error deleting embedding {Id}", embeddingId);
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Get performance metrics
    /// </summary>
    public Task<VectorSearchMetrics> GetMetricsAsync(CancellationToken cancellationToken = default)
    {
        lock (_indexLock)
        {
            _metrics.IndexSizeBytes = _vectorIndex.Sum(kvp => EstimateEntrySize(kvp.Value));
            _metrics.LastOptimized = DateTime.UtcNow; // Would track actual optimization

            return Task.FromResult(new VectorSearchMetrics
            {
                TotalEmbeddings = _metrics.TotalEmbeddings,
                TotalSearches = _metrics.TotalSearches,
                AverageSearchTime = _metrics.AverageSearchTime,
                CacheHitRate = _metrics.TotalSearches > 0 ? _metrics.TotalEmbeddings / (double)_metrics.TotalSearches : 0.0,
                LastOptimized = _metrics.LastOptimized,
                IndexSizeBytes = _metrics.IndexSizeBytes,
                PerformanceMetrics = new Dictionary<string, object>
                {
                    ["memory_usage_mb"] = GC.GetTotalMemory(false) / 1024.0 / 1024.0,
                    ["index_entries"] = _vectorIndex.Count,
                    ["avg_embedding_size"] = _vectorIndex.Any() ? _vectorIndex.First().Value.Embedding.Length : 0
                }
            });
        }
    }

    /// <summary>
    /// Batch generate embeddings for multiple texts
    /// </summary>
    public async Task<List<BatchEmbeddingResult>> GenerateBatchEmbeddingsAsync(
        List<string> texts,
        CancellationToken cancellationToken = default)
    {
        var results = new List<BatchEmbeddingResult>();

        foreach (var text in texts)
        {
            var startTime = DateTime.UtcNow;
            try
            {
                var embedding = await GenerateEmbeddingAsync(text, cancellationToken);
                results.Add(new BatchEmbeddingResult
                {
                    Text = text,
                    Embedding = embedding,
                    Success = true,
                    ProcessingTime = DateTime.UtcNow - startTime
                });
            }
            catch (Exception ex)
            {
                results.Add(new BatchEmbeddingResult
                {
                    Text = text,
                    Embedding = Array.Empty<float>(),
                    Success = false,
                    Error = ex.Message,
                    ProcessingTime = DateTime.UtcNow - startTime
                });
            }
        }

        return results;
    }

    /// <summary>
    /// Optimize vector index for better performance
    /// </summary>
    public Task OptimizeIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Optimizing vector index with {Count} entries", _vectorIndex.Count);

            // Remove expired entries
            var expiredEntries = _vectorIndex
                .Where(kvp => DateTime.UtcNow - kvp.Value.LastAccessed > TimeSpan.FromDays(30))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var expiredId in expiredEntries)
            {
                _vectorIndex.TryRemove(expiredId, out _);
            }

            lock (_indexLock)
            {
                _metrics.LastOptimized = DateTime.UtcNow;
                _metrics.TotalEmbeddings = _vectorIndex.Count;
            }

            _logger.LogInformation("🔧 Index optimization complete - Removed {Count} expired entries", expiredEntries.Count);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing vector index");
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Clear the entire vector index
    /// </summary>
    public Task<bool> ClearIndexAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var count = _vectorIndex.Count;
            _vectorIndex.Clear();

            lock (_indexLock)
            {
                _metrics.TotalEmbeddings = 0;
                _metrics.TotalSearches = 0;
                _metrics.AverageSearchTime = 0;
                _metrics.IndexSizeBytes = 0;
            }

            _logger.LogInformation("🗑️ Cleared vector index - Removed {Count} entries", count);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error clearing vector index");
            return Task.FromResult(false);
        }
    }

    /// <summary>
    /// Invalidate embeddings by pattern
    /// </summary>
    public Task InvalidateByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            var entriesToRemove = _vectorIndex
                .Where(kvp => kvp.Value.QueryText.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
                             kvp.Value.SqlQuery.Contains(pattern, StringComparison.OrdinalIgnoreCase))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var entryId in entriesToRemove)
            {
                _vectorIndex.TryRemove(entryId, out _);
            }

            lock (_indexLock)
            {
                _metrics.TotalEmbeddings = _vectorIndex.Count;
            }

            _logger.LogInformation("🗑️ Invalidated {Count} entries matching pattern: {Pattern}", entriesToRemove.Count, pattern);
            return Task.CompletedTask;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error invalidating entries by pattern: {Pattern}", pattern);
            return Task.CompletedTask;
        }
    }

    /// <summary>
    /// Generate simple embedding based on text features (placeholder for real embedding API)
    /// </summary>
    private Task<float[]> GenerateSimpleEmbeddingAsync(string text)
    {
        // This is a simplified embedding generation for demonstration
        // In production, this would call OpenAI's embedding API or similar service

        var words = text.ToLowerInvariant()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Take(100) // Limit to prevent huge embeddings
            .ToArray();

        var embedding = new float[384]; // Common embedding dimension

        // Simple hash-based embedding (not semantically meaningful, just for demo)
        for (int i = 0; i < words.Length && i < embedding.Length; i++)
        {
            var hash = words[i].GetHashCode();
            embedding[i] = (float)(hash % 1000) / 1000.0f; // Normalize to [0,1]
        }

        // Add some randomness based on text characteristics
        var textHash = text.GetHashCode();
        var random = new Random(textHash);
        for (int i = words.Length; i < embedding.Length; i++)
        {
            embedding[i] = (float)random.NextDouble() * 0.1f; // Small random values
        }

        // Normalize the vector
        var magnitude = Math.Sqrt(embedding.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < embedding.Length; i++)
            {
                embedding[i] /= (float)magnitude;
            }
        }

        return Task.FromResult(embedding);
    }

    private long EstimateEntrySize(VectorEntry entry)
    {
        return sizeof(float) * entry.Embedding.Length +
               entry.QueryText.Length * sizeof(char) +
               entry.SqlQuery.Length * sizeof(char) +
               1024; // Approximate overhead
    }

    /// <summary>
    /// Internal vector entry structure
    /// </summary>
    private class VectorEntry
    {
        public string Id { get; set; } = string.Empty;
        public string QueryText { get; set; } = string.Empty;
        public string SqlQuery { get; set; } = string.Empty;
        public QueryResponse Response { get; set; } = new();
        public float[] Embedding { get; set; } = Array.Empty<float>();
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccessed { get; set; }
        public int AccessCount { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new();
    }

    #region Missing Interface Method Implementations

    /// <summary>
    /// Index document (IVectorSearchService interface)
    /// </summary>
    public async Task<string> IndexDocumentAsync(string documentId, string content, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📄 Indexing document: {DocumentId}", documentId);

            var embedding = await GenerateEmbeddingAsync(content, cancellationToken);

            var vectorEntry = new VectorEntry
            {
                Id = documentId,
                QueryText = content,
                SqlQuery = string.Empty, // Not applicable for documents
                Response = null, // Not applicable for documents
                Embedding = embedding,
                CreatedAt = DateTime.UtcNow,
                LastAccessed = DateTime.UtcNow,
                AccessCount = 0,
                Metadata = metadata ?? new Dictionary<string, object>()
            };

            _vectorIndex.AddOrUpdate(documentId, vectorEntry, (key, oldValue) => vectorEntry);

            lock (_indexLock)
            {
                _metrics.TotalEmbeddings++;
            }

            _logger.LogDebug("✅ Document indexed: {DocumentId}", documentId);
            return documentId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error indexing document: {DocumentId}", documentId);
            throw;
        }
    }

    /// <summary>
    /// Search documents (IVectorSearchService interface)
    /// </summary>
    public async Task<List<BIReportingCopilot.Core.Interfaces.AI.VectorSearchResult>> SearchAsync(string query, int maxResults = 10, double threshold = 0.7, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🔍 Searching documents for query: {Query}", query);

            var queryEmbedding = await GenerateEmbeddingAsync(query, cancellationToken);
            var similarities = new List<(string Id, double Similarity, VectorEntry Entry)>();

            foreach (var kvp in _vectorIndex)
            {
                var similarity = CalculateCosineSimilarity(queryEmbedding, kvp.Value.Embedding);
                if (similarity >= threshold)
                {
                    similarities.Add((kvp.Key, similarity, kvp.Value));
                }
            }

            var results = similarities
                .OrderByDescending(s => s.Similarity)
                .Take(maxResults)
                .Select(s => new BIReportingCopilot.Core.Interfaces.AI.VectorSearchResult
                {
                    DocumentId = s.Id,
                    Content = s.Entry.QueryText,
                    Score = s.Similarity,
                    Metadata = s.Entry.Metadata
                })
                .ToList();

            lock (_indexLock)
            {
                _metrics.TotalSearches++;
            }

            _logger.LogDebug("✅ Found {Count} documents", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error searching documents");
            return new List<BIReportingCopilot.Core.Interfaces.AI.VectorSearchResult>();
        }
    }

    /// <summary>
    /// Search similar documents (IVectorSearchService interface)
    /// </summary>
    public async Task<List<BIReportingCopilot.Core.Interfaces.AI.VectorSearchResult>> SearchSimilarAsync(string documentId, int maxResults = 10, double threshold = 0.7, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🔍 Searching similar documents to: {DocumentId}", documentId);

            if (!_vectorIndex.TryGetValue(documentId, out var sourceEntry))
            {
                _logger.LogWarning("⚠️ Document not found: {DocumentId}", documentId);
                return new List<BIReportingCopilot.Core.Interfaces.AI.VectorSearchResult>();
            }

            var similarities = new List<(string Id, double Similarity, VectorEntry Entry)>();

            foreach (var kvp in _vectorIndex)
            {
                if (kvp.Key == documentId) continue; // Skip self

                var similarity = CalculateCosineSimilarity(sourceEntry.Embedding, kvp.Value.Embedding);
                if (similarity >= threshold)
                {
                    similarities.Add((kvp.Key, similarity, kvp.Value));
                }
            }

            var results = similarities
                .OrderByDescending(s => s.Similarity)
                .Take(maxResults)
                .Select(s => new BIReportingCopilot.Core.Interfaces.AI.VectorSearchResult
                {
                    DocumentId = s.Id,
                    Content = s.Entry.QueryText,
                    Score = s.Similarity,
                    Metadata = s.Entry.Metadata
                })
                .ToList();

            _logger.LogDebug("✅ Found {Count} similar documents", results.Count);
            return results;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error searching similar documents");
            return new List<BIReportingCopilot.Core.Interfaces.AI.VectorSearchResult>();
        }
    }

    /// <summary>
    /// Delete document (IVectorSearchService interface)
    /// </summary>
    public async Task<bool> DeleteDocumentAsync(string documentId, CancellationToken cancellationToken = default)
    {
        return await DeleteQueryEmbeddingAsync(documentId, cancellationToken);
    }

    /// <summary>
    /// Update document (IVectorSearchService interface)
    /// </summary>
    public async Task<bool> UpdateDocumentAsync(string documentId, string content, Dictionary<string, object>? metadata = null, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🔄 Updating document: {DocumentId}", documentId);

            var newEmbedding = await GenerateEmbeddingAsync(content, cancellationToken);

            if (_vectorIndex.TryGetValue(documentId, out var existingEntry))
            {
                existingEntry.QueryText = content;
                existingEntry.Embedding = newEmbedding;
                existingEntry.LastAccessed = DateTime.UtcNow;
                existingEntry.Metadata = metadata ?? existingEntry.Metadata;

                _logger.LogDebug("✅ Document updated: {DocumentId}", documentId);
                return true;
            }

            _logger.LogWarning("⚠️ Document not found for update: {DocumentId}", documentId);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating document: {DocumentId}", documentId);
            return false;
        }
    }

    /// <summary>
    /// Get statistics (IVectorSearchService interface)
    /// </summary>
    public async Task<BIReportingCopilot.Core.Interfaces.AI.VectorSearchStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var metrics = await GetMetricsAsync(cancellationToken);

            return new BIReportingCopilot.Core.Interfaces.AI.VectorSearchStatistics
            {
                TotalDocuments = metrics.TotalEmbeddings,
                TotalSearches = metrics.TotalSearches,
                AverageSearchTime = metrics.AverageSearchTime,
                IndexSize = metrics.IndexSizeBytes,
                LastIndexUpdate = metrics.LastOptimized
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting statistics");
            return new BIReportingCopilot.Core.Interfaces.AI.VectorSearchStatistics
            {
                LastIndexUpdate = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Store query embedding with simple signature (IVectorSearchService interface)
    /// </summary>
    public async Task<string> StoreQueryEmbeddingAsync(string queryId, string query, float[] embedding, CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = new Dictionary<string, object>
            {
                ["type"] = "query",
                ["queryId"] = queryId,
                ["timestamp"] = DateTime.UtcNow
            };

            return await IndexDocumentAsync(queryId, query, metadata, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error storing query embedding: {QueryId}", queryId);
            throw;
        }
    }

    /// <summary>
    /// Find similar queries (IVectorSearchService interface)
    /// </summary>
    public async Task<List<SemanticSearchResult>> FindSimilarQueriesAsync(string query, double threshold = 0.8, int maxResults = 5, CancellationToken cancellationToken = default)
    {
        return await FindSimilarQueriesByTextAsync(query, threshold, maxResults, cancellationToken);
    }

    #endregion
}
