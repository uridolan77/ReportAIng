using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.AI;
using System.Security.Claims;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Enhanced query controller with advanced AI/ML capabilities
/// </summary>
[ApiController]
[Route("api/enhanced-query")]
[Authorize]
public class EnhancedQueryController : ControllerBase
{
    private readonly IQueryProcessor _queryProcessor;
    private readonly ISemanticAnalyzer _semanticAnalyzer;
    private readonly IQueryClassifier _queryClassifier;
    private readonly IContextManager _contextManager;
    private readonly ISqlQueryService _sqlQueryService;
    private readonly ILogger<EnhancedQueryController> _logger;

    public EnhancedQueryController(
        IQueryProcessor queryProcessor,
        ISemanticAnalyzer semanticAnalyzer,
        IQueryClassifier queryClassifier,
        IContextManager contextManager,
        ISqlQueryService sqlQueryService,
        ILogger<EnhancedQueryController> logger)
    {
        _queryProcessor = queryProcessor;
        _semanticAnalyzer = semanticAnalyzer;
        _queryClassifier = queryClassifier;
        _contextManager = contextManager;
        _sqlQueryService = sqlQueryService;
        _logger = logger;
    }

    /// <summary>
    /// Process a natural language query with enhanced AI capabilities
    /// </summary>
    /// <param name="request">Enhanced query request</param>
    /// <returns>Processed query with semantic analysis and optimization</returns>
    [HttpPost("process")]
    public async Task<ActionResult<EnhancedQueryResponse>> ProcessEnhancedQuery([FromBody] EnhancedQueryRequest request)
    {
        try
        {
            var userId = GetCurrentUserId();
            _logger.LogInformation("Processing enhanced query for user {UserId}: {Query}", userId, request.Query);

            // Process the query with enhanced AI pipeline
            var processedQuery = await _queryProcessor.ProcessQueryAsync(request.Query, userId);

            // Execute the optimized SQL if requested
            QueryResult? queryResult = null;
            if (request.ExecuteQuery)
            {
                try
                {
                    queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(processedQuery.Sql);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to execute optimized SQL, trying alternatives");
                    
                    // Try alternative queries if main query fails
                    foreach (var alternative in processedQuery.AlternativeQueries.Take(2))
                    {
                        try
                        {
                            queryResult = await _sqlQueryService.ExecuteSelectQueryAsync(alternative.Sql);
                            processedQuery.Sql = alternative.Sql; // Update to working SQL
                            break;
                        }
                        catch
                        {
                            continue; // Try next alternative
                        }
                    }
                }
            }

            var response = new EnhancedQueryResponse
            {
                ProcessedQuery = processedQuery,
                QueryResult = queryResult,
                SemanticAnalysis = new SemanticAnalysisResponse
                {
                    Intent = processedQuery.SemanticEntities.Any() ? 
                        processedQuery.Classification.Category.ToString() : "General",
                    Entities = processedQuery.SemanticEntities.Select(e => new EntityResponse
                    {
                        Text = e.Text,
                        Type = e.Type.ToString(),
                        Confidence = e.Confidence
                    }).ToList(),
                    Confidence = processedQuery.Confidence
                },
                Classification = new ClassificationResponse
                {
                    Category = processedQuery.Classification.Category.ToString(),
                    Complexity = processedQuery.Classification.Complexity.ToString(),
                    EstimatedExecutionTime = processedQuery.Classification.EstimatedExecutionTime,
                    RecommendedVisualization = processedQuery.Classification.RecommendedVisualization.ToString(),
                    OptimizationSuggestions = processedQuery.Classification.OptimizationSuggestions
                },
                Alternatives = processedQuery.AlternativeQueries.Select(alt => new AlternativeQueryResponse
                {
                    Sql = alt.Sql,
                    Score = alt.Score,
                    Reasoning = alt.Reasoning,
                    Strengths = alt.Strengths,
                    Weaknesses = alt.Weaknesses
                }).ToList(),
                Success = true,
                Timestamp = DateTime.UtcNow
            };

            _logger.LogInformation("Enhanced query processing completed successfully with confidence: {Confidence}", 
                processedQuery.Confidence);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing enhanced query");
            return StatusCode(500, new EnhancedQueryResponse
            {
                Success = false,
                ErrorMessage = "An error occurred while processing your query. Please try again.",
                Timestamp = DateTime.UtcNow
            });
        }
    }

    /// <summary>
    /// Analyze semantic content of a query without execution
    /// </summary>
    /// <param name="query">Natural language query to analyze</param>
    /// <returns>Semantic analysis results</returns>
    [HttpPost("analyze")]
    public async Task<ActionResult<SemanticAnalysisResponse>> AnalyzeQuery([FromBody] string query)
    {
        try
        {
            var analysis = await _semanticAnalyzer.AnalyzeAsync(query);
            
            var response = new SemanticAnalysisResponse
            {
                Intent = analysis.Intent.ToString(),
                Entities = analysis.Entities.Select(e => new EntityResponse
                {
                    Text = e.Text,
                    Type = e.Type.ToString(),
                    Confidence = e.Confidence,
                    StartPosition = e.StartPosition,
                    EndPosition = e.EndPosition
                }).ToList(),
                Keywords = analysis.Keywords,
                Confidence = analysis.ConfidenceScore,
                ProcessedQuery = analysis.ProcessedQuery,
                Metadata = analysis.Metadata
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing query semantics");
            return StatusCode(500, "Error analyzing query");
        }
    }

    /// <summary>
    /// Classify a query and predict its characteristics
    /// </summary>
    /// <param name="query">Natural language query to classify</param>
    /// <returns>Query classification results</returns>
    [HttpPost("classify")]
    public async Task<ActionResult<ClassificationResponse>> ClassifyQuery([FromBody] string query)
    {
        try
        {
            var classification = await _queryClassifier.ClassifyQueryAsync(query);
            
            var response = new ClassificationResponse
            {
                Category = classification.Category.ToString(),
                Complexity = classification.Complexity.ToString(),
                RequiredJoins = classification.RequiredJoins,
                PredictedTables = classification.PredictedTables,
                EstimatedExecutionTime = classification.EstimatedExecutionTime,
                RecommendedVisualization = classification.RecommendedVisualization.ToString(),
                ConfidenceScore = classification.ConfidenceScore,
                OptimizationSuggestions = classification.OptimizationSuggestions
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error classifying query");
            return StatusCode(500, "Error classifying query");
        }
    }

    /// <summary>
    /// Get intelligent query suggestions based on context
    /// </summary>
    /// <param name="context">Optional context for suggestions</param>
    /// <returns>List of suggested queries</returns>
    [HttpGet("suggestions")]
    public async Task<ActionResult<List<string>>> GetQuerySuggestions([FromQuery] string? context = null)
    {
        try
        {
            var userId = GetCurrentUserId();
            var suggestions = await _queryProcessor.GenerateQuerySuggestionsAsync(context ?? "", userId);
            
            return Ok(suggestions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating query suggestions");
            return StatusCode(500, "Error generating suggestions");
        }
    }

    /// <summary>
    /// Find similar queries based on semantic similarity
    /// </summary>
    /// <param name="query">Query to find similarities for</param>
    /// <param name="limit">Maximum number of similar queries to return</param>
    /// <returns>List of similar queries</returns>
    [HttpPost("similar")]
    public async Task<ActionResult<List<SimilarQueryResponse>>> FindSimilarQueries(
        [FromBody] string query, 
        [FromQuery] int limit = 5)
    {
        try
        {
            var userId = GetCurrentUserId();
            var similarQueries = await _queryProcessor.FindSimilarQueriesAsync(query, userId, limit);
            
            var response = similarQueries.Select(sq => new SimilarQueryResponse
            {
                Sql = sq.Sql,
                Explanation = sq.Explanation,
                Confidence = sq.Confidence,
                Classification = sq.Classification.Category.ToString()
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding similar queries");
            return StatusCode(500, "Error finding similar queries");
        }
    }

    /// <summary>
    /// Calculate semantic similarity between two queries
    /// </summary>
    /// <param name="request">Similarity calculation request</param>
    /// <returns>Similarity score and analysis</returns>
    [HttpPost("similarity")]
    public async Task<ActionResult<SimilarityResponse>> CalculateSimilarity([FromBody] SimilarityRequest request)
    {
        try
        {
            var similarity = await _semanticAnalyzer.CalculateSimilarityAsync(request.Query1, request.Query2);
            
            var response = new SimilarityResponse
            {
                SimilarityScore = similarity.SimilarityScore,
                CommonEntities = similarity.CommonEntities,
                CommonKeywords = similarity.CommonKeywords,
                Analysis = similarity.SimilarityScore > 0.8 ? "Very Similar" :
                          similarity.SimilarityScore > 0.6 ? "Similar" :
                          similarity.SimilarityScore > 0.4 ? "Somewhat Similar" : "Different"
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating similarity");
            return StatusCode(500, "Error calculating similarity");
        }
    }

    /// <summary>
    /// Get user's query context and patterns
    /// </summary>
    /// <returns>User context information</returns>
    [HttpGet("context")]
    public async Task<ActionResult<UserContextResponse>> GetUserContext()
    {
        try
        {
            var userId = GetCurrentUserId();
            var context = await _contextManager.GetUserContextAsync(userId);
            var patterns = await _contextManager.GetQueryPatternsAsync(userId);
            
            var response = new UserContextResponse
            {
                Domain = context.Domain,
                PreferredTables = context.PreferredTables,
                CommonFilters = context.CommonFilters,
                RecentPatterns = patterns.Take(10).Select(p => new QueryPatternResponse
                {
                    Pattern = p.Pattern,
                    Frequency = p.Frequency,
                    LastUsed = p.LastUsed,
                    Intent = p.Intent.ToString(),
                    AssociatedTables = p.AssociatedTables
                }).ToList(),
                LastUpdated = context.LastUpdated
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user context");
            return StatusCode(500, "Error retrieving user context");
        }
    }

    private string GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? 
               User.FindFirst("sub")?.Value ?? 
               "anonymous";
    }
}

// Response models for the enhanced query controller
public class EnhancedQueryRequest
{
    public string Query { get; set; } = string.Empty;
    public bool ExecuteQuery { get; set; } = true;
    public bool IncludeAlternatives { get; set; } = true;
    public bool IncludeSemanticAnalysis { get; set; } = true;
}

public class EnhancedQueryResponse
{
    public ProcessedQuery? ProcessedQuery { get; set; }
    public QueryResult? QueryResult { get; set; }
    public SemanticAnalysisResponse? SemanticAnalysis { get; set; }
    public ClassificationResponse? Classification { get; set; }
    public List<AlternativeQueryResponse> Alternatives { get; set; } = new();
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime Timestamp { get; set; }
}

public class SemanticAnalysisResponse
{
    public string Intent { get; set; } = string.Empty;
    public List<EntityResponse> Entities { get; set; } = new();
    public List<string> Keywords { get; set; } = new();
    public double Confidence { get; set; }
    public string ProcessedQuery { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public class EntityResponse
{
    public string Text { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public int StartPosition { get; set; }
    public int EndPosition { get; set; }
}

public class ClassificationResponse
{
    public string Category { get; set; } = string.Empty;
    public string Complexity { get; set; } = string.Empty;
    public List<string> RequiredJoins { get; set; } = new();
    public List<string> PredictedTables { get; set; } = new();
    public TimeSpan EstimatedExecutionTime { get; set; }
    public string RecommendedVisualization { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> OptimizationSuggestions { get; set; } = new();
}

public class AlternativeQueryResponse
{
    public string Sql { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Reasoning { get; set; } = string.Empty;
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
}

public class SimilarQueryResponse
{
    public string Sql { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string Classification { get; set; } = string.Empty;
}

public class SimilarityRequest
{
    public string Query1 { get; set; } = string.Empty;
    public string Query2 { get; set; } = string.Empty;
}

public class SimilarityResponse
{
    public double SimilarityScore { get; set; }
    public List<string> CommonEntities { get; set; } = new();
    public List<string> CommonKeywords { get; set; } = new();
    public string Analysis { get; set; } = string.Empty;
}

public class UserContextResponse
{
    public string Domain { get; set; } = string.Empty;
    public List<string> PreferredTables { get; set; } = new();
    public List<string> CommonFilters { get; set; } = new();
    public List<QueryPatternResponse> RecentPatterns { get; set; } = new();
    public DateTime LastUpdated { get; set; }
}

public class QueryPatternResponse
{
    public string Pattern { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public DateTime LastUsed { get; set; }
    public string Intent { get; set; } = string.Empty;
    public List<string> AssociatedTables { get; set; } = new();
}
