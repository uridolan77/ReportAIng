using Microsoft.AspNetCore.Mvc;
using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Core.Models.BusinessContext;
using BIReportingCopilot.Core.DTOs;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for business context analysis and prompt generation
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BusinessContextController : ControllerBase
{
    private readonly IBusinessContextAnalyzer _contextAnalyzer;
    private readonly IBusinessMetadataRetrievalService _metadataService;
    private readonly IContextualPromptBuilder _promptBuilder;
    private readonly IVectorEmbeddingService _vectorService;
    private readonly ILogger<BusinessContextController> _logger;

    public BusinessContextController(
        IBusinessContextAnalyzer contextAnalyzer,
        IBusinessMetadataRetrievalService metadataService,
        IContextualPromptBuilder promptBuilder,
        IVectorEmbeddingService vectorService,
        ILogger<BusinessContextController> logger)
    {
        _contextAnalyzer = contextAnalyzer;
        _metadataService = metadataService;
        _promptBuilder = promptBuilder;
        _vectorService = vectorService;
        _logger = logger;
    }

    /// <summary>
    /// Analyze business context from a natural language question
    /// </summary>
    [HttpPost("analyze")]
    public async Task<ActionResult<BusinessContextProfile>> AnalyzeBusinessContext(
        [FromBody] AnalyzeContextRequest request)
    {
        try
        {
            _logger.LogInformation("Analyzing business context for question: {Question}", request.UserQuestion);

            var profile = await _contextAnalyzer.AnalyzeUserQuestionAsync(
                request.UserQuestion, 
                request.UserId);

            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing business context");
            return StatusCode(500, new { error = "Failed to analyze business context", details = ex.Message });
        }
    }

    /// <summary>
    /// Get relevant business metadata for a context profile
    /// </summary>
    [HttpPost("metadata")]
    public async Task<ActionResult<ContextualBusinessSchema>> GetRelevantMetadata(
        [FromBody] GetMetadataRequest request)
    {
        try
        {
            _logger.LogInformation("Retrieving business metadata for intent: {Intent}", 
                request.ContextProfile.Intent.Type);

            var schema = await _metadataService.GetRelevantBusinessMetadataAsync(
                request.ContextProfile, 
                request.MaxTables);

            return Ok(schema);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving business metadata");
            return StatusCode(500, new { error = "Failed to retrieve business metadata", details = ex.Message });
        }
    }

    /// <summary>
    /// Generate a business-aware prompt for LLM consumption
    /// </summary>
    [HttpPost("prompt")]
    public async Task<ActionResult<BusinessAwarePromptResponse>> GenerateBusinessAwarePrompt(
        [FromBody] BusinessPromptRequest request)
    {
        try
        {
            _logger.LogInformation("Generating business-aware prompt for: {Question}", request.UserQuestion);

            // Step 1: Analyze business context
            var profile = await _contextAnalyzer.AnalyzeUserQuestionAsync(
                request.UserQuestion, 
                request.UserId);

            // Step 2: Get relevant metadata
            var schema = await _metadataService.GetRelevantBusinessMetadataAsync(
                profile, 
                request.MaxTables);

            // Step 3: Build contextual prompt
            var prompt = await _promptBuilder.BuildBusinessAwarePromptAsync(
                request.UserQuestion, 
                profile, 
                schema);

            var response = new BusinessAwarePromptResponse
            {
                GeneratedPrompt = prompt,
                ContextProfile = profile,
                UsedSchema = schema,
                ConfidenceScore = profile.ConfidenceScore,
                Warnings = GenerateWarnings(profile, schema),
                Metadata = new Dictionary<string, object>
                {
                    ["processingTimeMs"] = 0, // Would be calculated
                    ["tokensEstimate"] = EstimateTokens(prompt),
                    ["templateUsed"] = profile.Intent.Type.ToString().ToLower(),
                    ["schemaComplexity"] = schema.Complexity.ToString()
                }
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating business-aware prompt");
            return StatusCode(500, new { error = "Failed to generate prompt", details = ex.Message });
        }
    }

    /// <summary>
    /// Classify business intent of a question
    /// </summary>
    [HttpPost("intent")]
    public async Task<ActionResult<QueryIntent>> ClassifyIntent(
        [FromBody] ClassifyIntentRequest request)
    {
        try
        {
            var intent = await _contextAnalyzer.ClassifyBusinessIntentAsync(request.Query ?? request.UserQuestion);
            return Ok(intent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying business intent");
            return StatusCode(500, new { error = "Failed to classify intent", details = ex.Message });
        }
    }

    /// <summary>
    /// Extract business entities from a question
    /// </summary>
    [HttpPost("entities")]
    public async Task<ActionResult<List<BusinessEntity>>> ExtractEntities(
        [FromBody] ExtractEntitiesRequest request)
    {
        try
        {
            var entities = await _contextAnalyzer.ExtractBusinessEntitiesAsync(request.Query ?? request.UserQuestion);
            return Ok(entities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting business entities");
            return StatusCode(500, new { error = "Failed to extract entities", details = ex.Message });
        }
    }

    /// <summary>
    /// Find relevant tables based on business context
    /// </summary>
    [HttpPost("tables")]
    public async Task<ActionResult<List<BusinessTableInfoDto>>> FindRelevantTables(
        [FromBody] FindTablesRequest request)
    {
        try
        {
            var tables = await _metadataService.FindRelevantTablesAsync(
                request.ContextProfile, 
                request.MaxTables);
            return Ok(tables);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding relevant tables");
            return StatusCode(500, new { error = "Failed to find tables", details = ex.Message });
        }
    }

    // Helper methods
    private List<string> GenerateWarnings(BusinessContextProfile profile, ContextualBusinessSchema schema)
    {
        var warnings = new List<string>();

        if (profile.ConfidenceScore < 0.7)
        {
            warnings.Add("Low confidence in business context analysis");
        }

        if (!schema.RelevantTables.Any())
        {
            warnings.Add("No relevant tables found for the query");
        }

        if (schema.Complexity == SchemaComplexity.VeryComplex)
        {
            warnings.Add("Query involves complex schema relationships");
        }

        return warnings;
    }

    private int EstimateTokens(string text)
    {
        // Simple token estimation (roughly 4 characters per token)
        return text.Length / 4;
    }

    /// <summary>
    /// Generate vector embeddings for prompt templates
    /// </summary>
    /// <returns>Number of embeddings generated</returns>
    [HttpPost("embeddings/prompt-templates")]
    public async Task<IActionResult> GeneratePromptTemplateEmbeddings()
    {
        try
        {
            _logger.LogInformation("Starting prompt template embedding generation");

            var count = await _vectorService.GeneratePromptTemplateEmbeddingsAsync();

            return Ok(new {
                message = "Prompt template embeddings generated successfully",
                embeddingsGenerated = count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating prompt template embeddings");
            return StatusCode(500, new {
                error = "Failed to generate prompt template embeddings",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Generate vector embeddings for query examples
    /// </summary>
    /// <returns>Number of embeddings generated</returns>
    [HttpPost("embeddings/query-examples")]
    public async Task<IActionResult> GenerateQueryExampleEmbeddings()
    {
        try
        {
            _logger.LogInformation("Starting query example embedding generation");

            var count = await _vectorService.GenerateQueryExampleEmbeddingsAsync();

            return Ok(new {
                message = "Query example embeddings generated successfully",
                embeddingsGenerated = count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query example embeddings");
            return StatusCode(500, new {
                error = "Failed to generate query example embeddings",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Find relevant prompt templates for a query
    /// </summary>
    /// <param name="request">Template search request</param>
    /// <returns>Relevant prompt templates</returns>
    [HttpPost("embeddings/relevant-templates")]
    public async Task<IActionResult> FindRelevantTemplates([FromBody] TemplateSearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserQuery))
            {
                return BadRequest(new { error = "User query is required" });
            }

            _logger.LogInformation("Finding relevant templates for query: {Query}", request.UserQuery);

            var templates = await _vectorService.FindRelevantPromptTemplatesAsync(
                request.UserQuery,
                request.IntentType,
                request.TopK);

            return Ok(new {
                templates = templates,
                totalFound = templates.Count,
                userQuery = request.UserQuery,
                intentType = request.IntentType,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding relevant templates");
            return StatusCode(500, new {
                error = "Failed to find relevant templates",
                details = ex.Message
            });
        }
    }

    /// <summary>
    /// Find relevant query examples for a query
    /// </summary>
    /// <param name="request">Example search request</param>
    /// <returns>Relevant query examples</returns>
    [HttpPost("embeddings/relevant-examples")]
    public async Task<IActionResult> FindRelevantExamples([FromBody] ExampleSearchRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.UserQuery))
            {
                return BadRequest(new { error = "User query is required" });
            }

            _logger.LogInformation("Finding relevant examples for query: {Query}", request.UserQuery);

            var examples = await _vectorService.FindRelevantQueryExamplesAsync(
                request.UserQuery,
                request.Domain,
                request.TopK);

            return Ok(new {
                examples = examples,
                totalFound = examples.Count,
                userQuery = request.UserQuery,
                domain = request.Domain,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding relevant examples");
            return StatusCode(500, new {
                error = "Failed to find relevant examples",
                details = ex.Message
            });
        }
    }
}

// Request/Response DTOs
public class AnalyzeContextRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public string? UserId { get; set; }
}

public class GetMetadataRequest
{
    public BusinessContextProfile ContextProfile { get; set; } = new();
    public int MaxTables { get; set; } = 5;
}

public class BusinessPromptRequest
{
    public string UserQuestion { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public BusinessDomain? PreferredDomain { get; set; }
    public PromptComplexityLevel ComplexityLevel { get; set; } = PromptComplexityLevel.Standard;
    public bool IncludeExamples { get; set; } = true;
    public bool IncludeBusinessRules { get; set; } = true;
    public int MaxTables { get; set; } = 5;
    public int MaxTokens { get; set; } = 4000;
}

public class BusinessAwarePromptResponse
{
    public string GeneratedPrompt { get; set; } = string.Empty;
    public BusinessContextProfile ContextProfile { get; set; } = new();
    public ContextualBusinessSchema UsedSchema { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public List<string> Warnings { get; set; } = new();
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class ClassifyIntentRequest
{
    public string Query { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty; // For backward compatibility
}

public class ExtractEntitiesRequest
{
    public string Query { get; set; } = string.Empty;
    public string UserQuestion { get; set; } = string.Empty; // For backward compatibility
}

public class FindTablesRequest
{
    public BusinessContextProfile ContextProfile { get; set; } = new();
    public int MaxTables { get; set; } = 5;
}

public enum PromptComplexityLevel
{
    Basic,
    Standard,
    Advanced,
    Expert
}

// Vector search request DTOs
public class VectorSearchRequest
{
    public string QueryText { get; set; } = string.Empty;
    public string EntityType { get; set; } = "PromptTemplate";
    public int TopK { get; set; } = 10;
    public double SimilarityThreshold { get; set; } = 0.7;
}

public class TemplateSearchRequest
{
    public string UserQuery { get; set; } = string.Empty;
    public string? IntentType { get; set; }
    public int TopK { get; set; } = 3;
}

public class ExampleSearchRequest
{
    public string UserQuery { get; set; } = string.Empty;
    public string? Domain { get; set; }
    public int TopK { get; set; } = 5;
}
