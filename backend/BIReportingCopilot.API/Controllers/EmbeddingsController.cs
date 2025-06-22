using Microsoft.AspNetCore.Mvc;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using Microsoft.Data.SqlClient;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for managing vector embeddings and semantic search
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EmbeddingsController : ControllerBase
{
    private readonly IVectorEmbeddingService _vectorService;
    private readonly ILogger<EmbeddingsController> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public EmbeddingsController(
        IVectorEmbeddingService vectorService,
        ILogger<EmbeddingsController> logger,
        IConfiguration configuration)
    {
        _vectorService = vectorService;
        _logger = logger;
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string not found");
    }

    /// <summary>
    /// Get embeddings dashboard data
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetEmbeddingsDashboard()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            // Get prompt templates count
            var promptTemplatesQuery = @"
                SELECT 
                    COUNT(*) as TotalTemplates,
                    COUNT(CASE WHEN ve.Id IS NOT NULL THEN 1 END) as WithEmbeddings,
                    COUNT(CASE WHEN ve.Id IS NULL THEN 1 END) as WithoutEmbeddings
                FROM PromptTemplates pt
                LEFT JOIN VectorEmbeddings ve ON ve.EntityType = 'PromptTemplate' AND ve.EntityId = pt.Id
                WHERE pt.IsActive = 1";

            using var cmd1 = new SqlCommand(promptTemplatesQuery, connection);
            using var reader1 = await cmd1.ExecuteReaderAsync();
            
            var promptTemplateStats = new { TotalTemplates = 0, WithEmbeddings = 0, WithoutEmbeddings = 0 };
            if (await reader1.ReadAsync())
            {
                promptTemplateStats = new
                {
                    TotalTemplates = Convert.ToInt32(reader1["TotalTemplates"]),
                    WithEmbeddings = Convert.ToInt32(reader1["WithEmbeddings"]),
                    WithoutEmbeddings = Convert.ToInt32(reader1["WithoutEmbeddings"])
                };
            }
            reader1.Close();

            // Get query examples count
            var queryExamplesQuery = @"
                SELECT 
                    COUNT(*) as TotalExamples,
                    COUNT(CASE WHEN ve.Id IS NOT NULL THEN 1 END) as WithEmbeddings,
                    COUNT(CASE WHEN ve.Id IS NULL THEN 1 END) as WithoutEmbeddings
                FROM QueryExamples qe
                LEFT JOIN VectorEmbeddings ve ON ve.EntityType = 'QueryExample' AND ve.EntityId = qe.Id
                WHERE qe.IsActive = 1";

            using var cmd2 = new SqlCommand(queryExamplesQuery, connection);
            using var reader2 = await cmd2.ExecuteReaderAsync();
            
            var queryExampleStats = new { TotalExamples = 0, WithEmbeddings = 0, WithoutEmbeddings = 0 };
            if (await reader2.ReadAsync())
            {
                queryExampleStats = new
                {
                    TotalExamples = Convert.ToInt32(reader2["TotalExamples"]),
                    WithEmbeddings = Convert.ToInt32(reader2["WithEmbeddings"]),
                    WithoutEmbeddings = Convert.ToInt32(reader2["WithoutEmbeddings"])
                };
            }
            reader2.Close();

            // Get recent embeddings
            var recentEmbeddingsQuery = @"
                SELECT TOP 10
                    ve.EntityType,
                    ve.EntityId,
                    ve.ModelVersion,
                    ve.CreatedDate,
                    ve.VectorDimensions,
                    CASE 
                        WHEN ve.EntityType = 'PromptTemplate' THEN pt.Name
                        WHEN ve.EntityType = 'QueryExample' THEN LEFT(qe.NaturalLanguageQuery, 50) + '...'
                        ELSE 'Unknown'
                    END as EntityName
                FROM VectorEmbeddings ve
                LEFT JOIN PromptTemplates pt ON ve.EntityType = 'PromptTemplate' AND ve.EntityId = pt.Id
                LEFT JOIN QueryExamples qe ON ve.EntityType = 'QueryExample' AND ve.EntityId = qe.Id
                WHERE ve.IsActive = 1
                ORDER BY ve.CreatedDate DESC";

            using var cmd3 = new SqlCommand(recentEmbeddingsQuery, connection);
            using var reader3 = await cmd3.ExecuteReaderAsync();
            
            var recentEmbeddings = new List<object>();
            while (await reader3.ReadAsync())
            {
                recentEmbeddings.Add(new
                {
                    EntityType = reader3["EntityType"].ToString() ?? "",
                    EntityId = Convert.ToInt64(reader3["EntityId"]),
                    EntityName = reader3["EntityName"].ToString() ?? "",
                    ModelVersion = reader3["ModelVersion"].ToString() ?? "",
                    CreatedDate = Convert.ToDateTime(reader3["CreatedDate"]),
                    VectorDimensions = Convert.ToInt32(reader3["VectorDimensions"])
                });
            }

            return Ok(new
            {
                promptTemplates = promptTemplateStats,
                queryExamples = queryExampleStats,
                recentEmbeddings = recentEmbeddings,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting embeddings dashboard data");
            return StatusCode(500, new { error = "Failed to get dashboard data", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate embeddings for prompt templates
    /// </summary>
    [HttpPost("generate/prompt-templates")]
    public async Task<IActionResult> GeneratePromptTemplateEmbeddings()
    {
        try
        {
            _logger.LogInformation("Starting prompt template embedding generation via UI");
            
            var count = await _vectorService.GeneratePromptTemplateEmbeddingsAsync();
            
            return Ok(new { 
                success = true,
                message = "Prompt template embeddings generated successfully",
                embeddingsGenerated = count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prompt template embeddings");
            return StatusCode(500, new { 
                success = false,
                error = "Failed to generate prompt template embeddings", 
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Generate embeddings for query examples
    /// </summary>
    [HttpPost("generate/query-examples")]
    public async Task<IActionResult> GenerateQueryExampleEmbeddings()
    {
        try
        {
            _logger.LogInformation("Starting query example embedding generation via UI");
            
            var count = await _vectorService.GenerateQueryExampleEmbeddingsAsync();
            
            return Ok(new { 
                success = true,
                message = "Query example embeddings generated successfully",
                embeddingsGenerated = count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query example embeddings");
            return StatusCode(500, new { 
                success = false,
                error = "Failed to generate query example embeddings", 
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Test semantic search with a sample query
    /// </summary>
    [HttpPost("test/semantic-search")]
    public async Task<IActionResult> TestSemanticSearch([FromBody] SemanticSearchTestRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "Query is required" });
            }

            _logger.LogInformation("Testing semantic search for query: {Query}", request.Query);

            // Find relevant templates
            var templates = await _vectorService.FindRelevantPromptTemplatesAsync(
                request.Query, 
                request.IntentType, 
                request.MaxResults);

            // Find relevant examples
            var examples = await _vectorService.FindRelevantQueryExamplesAsync(
                request.Query, 
                request.Domain, 
                request.MaxResults);

            return Ok(new
            {
                success = true,
                query = request.Query,
                relevantTemplates = templates.Select(t => new
                {
                    t.TemplateId,
                    t.TemplateKey,
                    t.Name,
                    t.IntentType,
                    t.RelevanceScore,
                    t.Priority,
                    t.Tags
                }),
                relevantExamples = examples.Select(e => new
                {
                    e.ExampleId,
                    e.NaturalLanguageQuery,
                    e.IntentType,
                    e.Domain,
                    e.RelevanceScore,
                    e.SuccessRate,
                    e.BusinessContext
                }),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing semantic search");
            return StatusCode(500, new { 
                success = false,
                error = "Failed to test semantic search", 
                details = ex.Message 
            });
        }
    }

    /// <summary>
    /// Get list of prompt templates with embedding status
    /// </summary>
    [HttpGet("prompt-templates")]
    public async Task<IActionResult> GetPromptTemplatesWithEmbeddings()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    pt.Id,
                    pt.Name,
                    pt.TemplateKey,
                    pt.IntentType,
                    pt.Priority,
                    pt.Tags,
                    pt.CreatedDate,
                    CASE WHEN ve.Id IS NOT NULL THEN 1 ELSE 0 END as HasEmbedding,
                    ve.CreatedDate as EmbeddingCreatedDate,
                    ve.ModelVersion as EmbeddingModel
                FROM PromptTemplates pt
                LEFT JOIN VectorEmbeddings ve ON ve.EntityType = 'PromptTemplate' AND ve.EntityId = pt.Id AND ve.IsActive = 1
                WHERE pt.IsActive = 1
                ORDER BY pt.Priority DESC, pt.Name";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var templates = new List<object>();
            while (await reader.ReadAsync())
            {
                templates.Add(new
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    Name = reader["Name"].ToString() ?? "",
                    TemplateKey = reader["TemplateKey"] == DBNull.Value ? null : reader["TemplateKey"].ToString(),
                    IntentType = reader["IntentType"] == DBNull.Value ? null : reader["IntentType"].ToString(),
                    Priority = reader["Priority"] == DBNull.Value ? 100 : Convert.ToInt32(reader["Priority"]),
                    Tags = reader["Tags"] == DBNull.Value ? null : reader["Tags"].ToString(),
                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                    HasEmbedding = Convert.ToBoolean(reader["HasEmbedding"]),
                    EmbeddingCreatedDate = reader["EmbeddingCreatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EmbeddingCreatedDate"]),
                    EmbeddingModel = reader["EmbeddingModel"] == DBNull.Value ? null : reader["EmbeddingModel"].ToString()
                });
            }

            var withEmbeddingsCount = 0;
            foreach (var template in templates)
            {
                var hasEmbedding = template.GetType().GetProperty("HasEmbedding")?.GetValue(template);
                if (hasEmbedding is bool hasEmbeddingBool && hasEmbeddingBool)
                {
                    withEmbeddingsCount++;
                }
            }

            return Ok(new
            {
                templates = templates,
                totalCount = templates.Count,
                withEmbeddings = withEmbeddingsCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt templates with embeddings");
            return StatusCode(500, new { error = "Failed to get prompt templates", details = ex.Message });
        }
    }

    /// <summary>
    /// Get list of query examples with embedding status
    /// </summary>
    [HttpGet("query-examples")]
    public async Task<IActionResult> GetQueryExamplesWithEmbeddings()
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    qe.Id,
                    qe.NaturalLanguageQuery,
                    qe.IntentType,
                    qe.Domain,
                    qe.SuccessRate,
                    qe.UsageCount,
                    qe.CreatedDate,
                    CASE WHEN ve.Id IS NOT NULL THEN 1 ELSE 0 END as HasEmbedding,
                    ve.CreatedDate as EmbeddingCreatedDate,
                    ve.ModelVersion as EmbeddingModel
                FROM QueryExamples qe
                LEFT JOIN VectorEmbeddings ve ON ve.EntityType = 'QueryExample' AND ve.EntityId = qe.Id AND ve.IsActive = 1
                WHERE qe.IsActive = 1
                ORDER BY qe.SuccessRate DESC, qe.UsageCount DESC";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var examples = new List<object>();
            while (await reader.ReadAsync())
            {
                examples.Add(new
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    NaturalLanguageQuery = reader["NaturalLanguageQuery"].ToString() ?? "",
                    IntentType = reader["IntentType"].ToString() ?? "",
                    Domain = reader["Domain"].ToString() ?? "",
                    SuccessRate = Convert.ToDecimal(reader["SuccessRate"]),
                    UsageCount = Convert.ToInt32(reader["UsageCount"]),
                    CreatedDate = Convert.ToDateTime(reader["CreatedDate"]),
                    HasEmbedding = Convert.ToBoolean(reader["HasEmbedding"]),
                    EmbeddingCreatedDate = reader["EmbeddingCreatedDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EmbeddingCreatedDate"]),
                    EmbeddingModel = reader["EmbeddingModel"] == DBNull.Value ? null : reader["EmbeddingModel"].ToString()
                });
            }

            var withEmbeddingsCount = 0;
            foreach (var example in examples)
            {
                var hasEmbedding = example.GetType().GetProperty("HasEmbedding")?.GetValue(example);
                if (hasEmbedding is bool hasEmbeddingBool && hasEmbeddingBool)
                {
                    withEmbeddingsCount++;
                }
            }

            return Ok(new
            {
                examples = examples,
                totalCount = examples.Count,
                withEmbeddings = withEmbeddingsCount,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query examples with embeddings");
            return StatusCode(500, new { error = "Failed to get query examples", details = ex.Message });
        }
    }
}

// Request DTOs
public class SemanticSearchTestRequest
{
    public string Query { get; set; } = string.Empty;
    public string? IntentType { get; set; }
    public string? Domain { get; set; }
    public int MaxResults { get; set; } = 5;
}
