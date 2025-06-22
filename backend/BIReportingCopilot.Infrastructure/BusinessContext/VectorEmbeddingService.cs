using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Text.Json;
using System.Text;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models.BusinessContext;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BIReportingCopilot.Infrastructure.BusinessContext;

/// <summary>
/// Service for generating and managing vector embeddings for semantic search
/// </summary>
public class VectorEmbeddingService : IVectorEmbeddingService
{
    private readonly ILogger<VectorEmbeddingService> _logger;
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _connectionString;
    private readonly string _openAiApiKey;
    private readonly string _embeddingModel;

    public VectorEmbeddingService(
        ILogger<VectorEmbeddingService> logger,
        IConfiguration configuration,
        HttpClient httpClient)
    {
        _logger = logger;
        _configuration = configuration;
        _httpClient = httpClient;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
        _openAiApiKey = configuration["OpenAI:ApiKey"] ?? throw new InvalidOperationException("OpenAI API key not found");
        _embeddingModel = configuration["OpenAI:EmbeddingModel"] ?? "text-embedding-ada-002";
        
        // Configure HttpClient for OpenAI
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_openAiApiKey}");
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "BIReportingCopilot/1.0");
    }

    public async Task<float[]> GenerateEmbeddingAsync(string text, string modelVersion = "text-embedding-ada-002", CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("Generating embedding for text: {Text}", text.Substring(0, Math.Min(100, text.Length)));

            var requestBody = new
            {
                input = text,
                model = modelVersion
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("https://api.openai.com/v1/embeddings", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
            var embeddingResponse = JsonSerializer.Deserialize<OpenAIEmbeddingResponse>(responseJson);

            if (embeddingResponse?.Data?.FirstOrDefault()?.Embedding == null)
            {
                throw new InvalidOperationException("Failed to generate embedding: Invalid response from OpenAI");
            }

            _logger.LogDebug("Successfully generated embedding with {Dimensions} dimensions", embeddingResponse.Data[0].Embedding.Length);
            return embeddingResponse.Data[0].Embedding;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating embedding for text");
            throw;
        }
    }

    public async Task<long> StoreEmbeddingAsync(string entityType, long entityId, float[] embedding, string sourceText, string modelVersion = "text-embedding-ada-002", CancellationToken cancellationToken = default)
    {
        try
        {
            var embeddingJson = JsonSerializer.Serialize(embedding);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            const string sql = @"
                INSERT INTO VectorEmbeddings (EntityType, EntityId, EmbeddingVector, ModelVersion, SourceText, VectorDimensions, CreatedDate, IsActive)
                OUTPUT INSERTED.Id
                VALUES (@EntityType, @EntityId, @EmbeddingVector, @ModelVersion, @SourceText, @VectorDimensions, @CreatedDate, 1)";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@EntityType", entityType);
            command.Parameters.AddWithValue("@EntityId", entityId);
            command.Parameters.AddWithValue("@EmbeddingVector", embeddingJson);
            command.Parameters.AddWithValue("@ModelVersion", modelVersion);
            command.Parameters.AddWithValue("@SourceText", sourceText.Length > 1000 ? sourceText.Substring(0, 1000) : sourceText);
            command.Parameters.AddWithValue("@VectorDimensions", embedding.Length);
            command.Parameters.AddWithValue("@CreatedDate", DateTime.UtcNow);

            var result = await command.ExecuteScalarAsync(cancellationToken);
            var embeddingId = Convert.ToInt64(result);

            _logger.LogDebug("Stored embedding {EmbeddingId} for {EntityType} {EntityId}", embeddingId, entityType, entityId);
            return embeddingId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error storing embedding for {EntityType} {EntityId}", entityType, entityId);
            throw;
        }
    }

    public async Task<List<VectorSearchResult>> FindSimilarAsync(float[] queryEmbedding, string entityType, int topK = 10, double similarityThreshold = 0.7, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get all embeddings for the entity type
            const string sql = @"
                SELECT Id, EntityId, EmbeddingVector, SourceText
                FROM VectorEmbeddings 
                WHERE EntityType = @EntityType AND IsActive = 1";

            using var command = new SqlCommand(sql, connection);
            command.Parameters.AddWithValue("@EntityType", entityType);

            var results = new List<VectorSearchResult>();

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var embeddingJson = reader.GetString("EmbeddingVector");
                var storedEmbedding = JsonSerializer.Deserialize<float[]>(embeddingJson);
                
                if (storedEmbedding != null)
                {
                    var similarity = CalculateCosineSimilarity(queryEmbedding, storedEmbedding);
                    
                    if (similarity >= similarityThreshold)
                    {
                        results.Add(new VectorSearchResult
                        {
                            EntityId = reader.GetInt64("EntityId"),
                            EntityType = entityType,
                            SimilarityScore = similarity,
                            SourceText = reader.GetString("SourceText")
                        });
                    }
                }
            }

            // Sort by similarity and take top K
            return results.OrderByDescending(r => r.SimilarityScore).Take(topK).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar embeddings for {EntityType}", entityType);
            throw;
        }
    }

    public async Task<List<VectorSearchResult>> FindSimilarByTextAsync(string queryText, string entityType, int topK = 10, double similarityThreshold = 0.7, CancellationToken cancellationToken = default)
    {
        var queryEmbedding = await GenerateEmbeddingAsync(queryText, _embeddingModel, cancellationToken);
        return await FindSimilarAsync(queryEmbedding, entityType, topK, similarityThreshold, cancellationToken);
    }

    public double CalculateCosineSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            throw new ArgumentException("Vectors must have the same length");

        double dotProduct = 0;
        double magnitude1 = 0;
        double magnitude2 = 0;

        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
            magnitude1 += vector1[i] * vector1[i];
            magnitude2 += vector2[i] * vector2[i];
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    public async Task<int> GeneratePromptTemplateEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting prompt template embedding generation");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get all prompt templates that don't have embeddings
            const string sql = @"
                SELECT pt.Id, pt.Name, pt.Description, pt.Content, pt.TemplateKey, pt.IntentType, pt.Tags
                FROM PromptTemplates pt
                LEFT JOIN VectorEmbeddings ve ON ve.EntityType = 'PromptTemplate' AND ve.EntityId = pt.Id
                WHERE pt.IsActive = 1 AND ve.Id IS NULL";

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var templates = new List<(long Id, string Text)>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetInt64("Id");
                var name = reader.IsDBNull("Name") ? "" : reader.GetString("Name");
                var description = reader.IsDBNull("Description") ? "" : reader.GetString("Description");
                var content = reader.IsDBNull("Content") ? "" : reader.GetString("Content");
                var templateKey = reader.IsDBNull("TemplateKey") ? "" : reader.GetString("TemplateKey");
                var intentType = reader.IsDBNull("IntentType") ? "" : reader.GetString("IntentType");
                var tags = reader.IsDBNull("Tags") ? "" : reader.GetString("Tags");

                // Combine relevant text for embedding
                var combinedText = $"{name} {description} {templateKey} {intentType} {tags}".Trim();
                templates.Add((id, combinedText));
            }

            reader.Close();

            int generatedCount = 0;
            foreach (var (id, text) in templates)
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    var embedding = await GenerateEmbeddingAsync(text, _embeddingModel, cancellationToken);
                    await StoreEmbeddingAsync("PromptTemplate", id, embedding, text, _embeddingModel, cancellationToken);
                    generatedCount++;
                    
                    _logger.LogDebug("Generated embedding for prompt template {TemplateId}", id);
                    
                    // Small delay to avoid rate limiting
                    await Task.Delay(100, cancellationToken);
                }
            }

            _logger.LogInformation("Generated {Count} prompt template embeddings", generatedCount);
            return generatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prompt template embeddings");
            throw;
        }
    }

    public async Task<int> GenerateQueryExampleEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting query example embedding generation");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get all query examples that don't have embeddings
            const string sql = @"
                SELECT qe.Id, qe.NaturalLanguageQuery, qe.IntentType, qe.Domain, qe.BusinessContext, qe.SemanticTags
                FROM QueryExamples qe
                LEFT JOIN VectorEmbeddings ve ON ve.EntityType = 'QueryExample' AND ve.EntityId = qe.Id
                WHERE qe.IsActive = 1 AND ve.Id IS NULL";

            using var command = new SqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync(cancellationToken);

            var examples = new List<(long Id, string Text)>();
            while (await reader.ReadAsync(cancellationToken))
            {
                var id = reader.GetInt64("Id");
                var query = reader.GetString("NaturalLanguageQuery");
                var intentType = reader.GetString("IntentType");
                var domain = reader.GetString("Domain");
                var businessContext = reader.GetString("BusinessContext");
                var semanticTags = reader.IsDBNull("SemanticTags") ? "" : reader.GetString("SemanticTags");

                // Combine relevant text for embedding
                var combinedText = $"{query} {intentType} {domain} {businessContext} {semanticTags}".Trim();
                examples.Add((id, combinedText));
            }

            reader.Close();

            int generatedCount = 0;
            foreach (var (id, text) in examples)
            {
                var embedding = await GenerateEmbeddingAsync(text, _embeddingModel, cancellationToken);
                await StoreEmbeddingAsync("QueryExample", id, embedding, text, _embeddingModel, cancellationToken);
                generatedCount++;
                
                _logger.LogDebug("Generated embedding for query example {ExampleId}", id);
                
                // Small delay to avoid rate limiting
                await Task.Delay(100, cancellationToken);
            }

            _logger.LogInformation("Generated {Count} query example embeddings", generatedCount);
            return generatedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query example embeddings");
            throw;
        }
    }

    public async Task<int> GenerateBusinessMetadataEmbeddingsAsync(CancellationToken cancellationToken = default)
    {
        // Implementation for business metadata embeddings
        // This would generate embeddings for BusinessTableInfo, BusinessColumnInfo, etc.
        _logger.LogInformation("Business metadata embedding generation not yet implemented");
        return 0;
    }

    public async Task<List<RelevantPromptTemplate>> FindRelevantPromptTemplatesAsync(string userQuery, string? intentType = null, int topK = 3, CancellationToken cancellationToken = default)
    {
        try
        {
            var similarResults = await FindSimilarByTextAsync(userQuery, "PromptTemplate", topK * 2, 0.5, cancellationToken);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var relevantTemplates = new List<RelevantPromptTemplate>();

            foreach (var result in similarResults)
            {
                const string sql = @"
                    SELECT Id, Name, Content, TemplateKey, IntentType, Priority, Tags
                    FROM PromptTemplates 
                    WHERE Id = @TemplateId AND IsActive = 1";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@TemplateId", result.EntityId);

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    var template = new RelevantPromptTemplate
                    {
                        TemplateId = reader.GetInt64("Id"),
                        Name = reader.GetString("Name"),
                        Content = reader.GetString("Content"),
                        TemplateKey = reader.IsDBNull("TemplateKey") ? "" : reader.GetString("TemplateKey"),
                        IntentType = reader.IsDBNull("IntentType") ? "" : reader.GetString("IntentType"),
                        Priority = reader.IsDBNull("Priority") ? 100 : reader.GetInt32("Priority"),
                        RelevanceScore = result.SimilarityScore,
                        Tags = reader.IsDBNull("Tags") ? new List<string>() : reader.GetString("Tags").Split(',').ToList()
                    };

                    // Filter by intent type if specified
                    if (string.IsNullOrEmpty(intentType) || template.IntentType.Equals(intentType, StringComparison.OrdinalIgnoreCase))
                    {
                        relevantTemplates.Add(template);
                    }
                }
            }

            return relevantTemplates.OrderByDescending(t => t.RelevanceScore).ThenByDescending(t => t.Priority).Take(topK).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding relevant prompt templates for query: {Query}", userQuery);
            throw;
        }
    }

    public async Task<List<RelevantQueryExample>> FindRelevantQueryExamplesAsync(string userQuery, string? domain = null, int topK = 5, CancellationToken cancellationToken = default)
    {
        try
        {
            var similarResults = await FindSimilarByTextAsync(userQuery, "QueryExample", topK * 2, 0.5, cancellationToken);
            
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            var relevantExamples = new List<RelevantQueryExample>();

            foreach (var result in similarResults)
            {
                const string sql = @"
                    SELECT Id, NaturalLanguageQuery, GeneratedSql, IntentType, Domain, SuccessRate, BusinessContext, UsedTables
                    FROM QueryExamples 
                    WHERE Id = @ExampleId AND IsActive = 1";

                using var command = new SqlCommand(sql, connection);
                command.Parameters.AddWithValue("@ExampleId", result.EntityId);

                using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (await reader.ReadAsync(cancellationToken))
                {
                    var example = new RelevantQueryExample
                    {
                        ExampleId = reader.GetInt64("Id"),
                        NaturalLanguageQuery = reader.GetString("NaturalLanguageQuery"),
                        GeneratedSql = reader.GetString("GeneratedSql"),
                        IntentType = reader.GetString("IntentType"),
                        Domain = reader.GetString("Domain"),
                        SuccessRate = reader.GetDecimal("SuccessRate"),
                        BusinessContext = reader.GetString("BusinessContext"),
                        RelevanceScore = result.SimilarityScore,
                        UsedTables = reader.GetString("UsedTables").Split(',').ToList()
                    };

                    // Filter by domain if specified
                    if (string.IsNullOrEmpty(domain) || example.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase))
                    {
                        relevantExamples.Add(example);
                    }
                }
            }

            return relevantExamples.OrderByDescending(e => e.RelevanceScore).ThenByDescending(e => e.SuccessRate).Take(topK).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding relevant query examples for query: {Query}", userQuery);
            throw;
        }
    }
}

// Helper classes for OpenAI API response
internal class OpenAIEmbeddingResponse
{
    public OpenAIEmbeddingData[]? Data { get; set; }
}

internal class OpenAIEmbeddingData
{
    public float[]? Embedding { get; set; }
}
