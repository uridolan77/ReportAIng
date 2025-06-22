using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.Infrastructure.BusinessContext;

/// <summary>
/// Service for semantic matching and similarity calculations
/// </summary>
public class SemanticMatchingService : ISemanticMatchingService
{
    private readonly IBusinessTableManagementService _businessTableService;
    private readonly IAIService _aiService;
    private readonly IVectorSearchService _vectorSearchService;
    private readonly ILogger<SemanticMatchingService> _logger;

    public SemanticMatchingService(
        IBusinessTableManagementService businessTableService,
        IAIService aiService,
        IVectorSearchService vectorSearchService,
        ILogger<SemanticMatchingService> logger)
    {
        _businessTableService = businessTableService;
        _aiService = aiService;
        _vectorSearchService = vectorSearchService;
        _logger = logger;
    }

    public async Task<List<(string term, double score)>> FindSimilarTermsAsync(
        string searchTerm, 
        int topK = 5)
    {
        try
        {
            _logger.LogInformation("Finding similar terms for: {SearchTerm}", searchTerm);

            // Generate embedding for search term
            var searchEmbedding = await GenerateEmbeddingAsync(searchTerm);

            // Use vector search to find similar terms - placeholder implementation
            await Task.CompletedTask;
            var results = new List<(string Content, double Score)>();

            return results.Select(r => (r.Content, r.Score)).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar terms for: {SearchTerm}", searchTerm);
            return new List<(string, double)>();
        }
    }

    public async Task<double> CalculateSemanticSimilarityAsync(string text1, string text2)
    {
        try
        {
            // Generate embeddings for both texts
            var embedding1 = await GenerateEmbeddingAsync(text1);
            var embedding2 = await GenerateEmbeddingAsync(text2);

            // Calculate cosine similarity
            return CalculateCosineSimilarity(embedding1, embedding2);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating semantic similarity");
            return 0.0;
        }
    }

    public async Task<List<BusinessTableInfoDto>> SemanticTableSearchAsync(
        string query, 
        int topK = 10)
    {
        try
        {
            _logger.LogInformation("Performing semantic table search for: {Query}", query);

            // Get all business tables
            var allTables = await _businessTableService.GetBusinessTablesAsync();

            // Score tables based on semantic similarity
            var scoredTables = new List<(BusinessTableInfoDto table, double score)>();

            foreach (var table in allTables)
            {
                // Create searchable text from table metadata
                var tableText = $"{table.BusinessPurpose} {table.BusinessContext} {table.PrimaryUseCase}";
                
                // Calculate similarity
                var similarity = await CalculateSemanticSimilarityAsync(query, tableText);
                
                if (similarity > 0.3) // Minimum threshold
                {
                    scoredTables.Add((table, similarity));
                }
            }

            // Return top K results
            return scoredTables
                .OrderByDescending(x => x.score)
                .Take(topK)
                .Select(x => x.table)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in semantic table search");
            return new List<BusinessTableInfoDto>();
        }
    }

    public async Task<List<BusinessColumnInfo>> SemanticColumnSearchAsync(
        string query, 
        long? tableId = null, 
        int topK = 10)
    {
        try
        {
            _logger.LogInformation("Performing semantic column search for: {Query}", query);

            var allColumns = new List<BusinessColumnInfo>();

            if (tableId.HasValue)
            {
                // Get columns for specific table
                var table = await _businessTableService.GetBusinessTableAsync(tableId.Value);
                if (table?.Columns != null)
                {
                    allColumns.AddRange(table.Columns);
                }
            }
            else
            {
                // Get columns from all tables (this would be optimized in real implementation)
                var allTables = await _businessTableService.GetBusinessTablesAsync();
                foreach (var table in allTables.Take(20)) // Limit to avoid performance issues
                {
                    var tableDetails = await _businessTableService.GetBusinessTableAsync(table.Id);
                    if (tableDetails?.Columns != null)
                    {
                        allColumns.AddRange(tableDetails.Columns);
                    }
                }
            }

            // Score columns based on semantic similarity
            var scoredColumns = new List<(BusinessColumnInfo column, double score)>();

            foreach (var column in allColumns)
            {
                // Create searchable text from column metadata
                var columnText = $"{column.BusinessMeaning} {column.BusinessContext} {column.ColumnName}";
                
                // Calculate similarity
                var similarity = await CalculateSemanticSimilarityAsync(query, columnText);
                
                if (similarity > 0.3) // Minimum threshold
                {
                    scoredColumns.Add((column, similarity));
                }
            }

            // Return top K results
            return scoredColumns
                .OrderByDescending(x => x.score)
                .Take(topK)
                .Select(x => x.column)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in semantic column search");
            return new List<BusinessColumnInfo>();
        }
    }

    public async Task<double[]> GenerateEmbeddingAsync(string text)
    {
        try
        {
            // Use AI service to generate embeddings
            // This would typically use OpenAI's embedding API
            // Use AI service to generate embeddings - placeholder implementation
            // This would typically use OpenAI's embedding API
            await Task.CompletedTask;
            return new double[1536]; // OpenAI embedding dimension
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text");
            // Return a default embedding vector
            return new double[1536]; // OpenAI embedding dimension
        }
    }

    // Helper methods
    private double CalculateCosineSimilarity(double[] vector1, double[] vector2)
    {
        if (vector1.Length != vector2.Length)
        {
            throw new ArgumentException("Vectors must have the same length");
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
}
