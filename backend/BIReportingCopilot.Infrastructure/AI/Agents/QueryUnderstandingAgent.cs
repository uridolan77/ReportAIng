using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces.Agents;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Models.Agents;
using BIReportingCopilot.Core.Models;
using AgentQueryIntent = BIReportingCopilot.Core.Models.Agents.QueryIntent;
using AgentQueryComplexity = BIReportingCopilot.Core.Models.Agents.QueryComplexity;
using AgentBusinessContext = BIReportingCopilot.Core.Models.Agents.BusinessContext;
using AgentComplexityLevel = BIReportingCopilot.Core.Models.Agents.ComplexityLevel;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.AI.Agents;

/// <summary>
/// Query Understanding Agent - Specialized in natural language interpretation
/// </summary>
public class QueryUnderstandingAgent : IQueryUnderstandingAgent
{
    private readonly ILogger<QueryUnderstandingAgent> _logger;
    private readonly IAIService _llmService;
    private readonly IConfiguration _configuration;
    private readonly AgentCapabilities _capabilities;
    private bool _isInitialized = false;

    public string AgentType => "QueryUnderstanding";
    public AgentCapabilities Capabilities => _capabilities;

    public QueryUnderstandingAgent(
        ILogger<QueryUnderstandingAgent> logger,
        IAIService llmService,
        IConfiguration configuration)
    {
        _logger = logger;
        _llmService = llmService;
        _configuration = configuration;
        
        _capabilities = new AgentCapabilities
        {
            AgentType = AgentType,
            SupportedOperations = new List<string>
            {
                "AnalyzeIntent",
                "AssessComplexity", 
                "DetectAmbiguities",
                "ExtractEntities",
                "ClassifyQueryType"
            },
            Metadata = new Dictionary<string, object>
            {
                ["Version"] = "2.0.0",
                ["Specialization"] = "Natural Language Understanding",
                ["SupportedLanguages"] = new[] { "English", "SQL" }
            },
            PerformanceScore = 0.95,
            IsAvailable = true
        };
    }

    #region ISpecializedAgent Implementation

    public async Task<AgentResponse> ProcessAsync(AgentRequest request, AgentContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("🧠 QueryUnderstandingAgent processing request {RequestId} of type {RequestType}", 
                request.RequestId, request.RequestType);

            object? result = request.RequestType switch
            {
                "AnalyzeIntent" => await AnalyzeIntentAsync(request.Payload.ToString()!, context),
                "AssessComplexity" => await AssessComplexityAsync(request.Payload.ToString()!, context),
                "DetectAmbiguities" => await DetectAmbiguitiesAsync(request.Payload.ToString()!, context),
                "ExtractEntities" => await ExtractEntitiesAsync(request.Payload.ToString()!, context),
                "ClassifyQueryType" => await ClassifyQueryTypeAsync(request.Payload.ToString()!, context),
                _ => throw new NotSupportedException($"Request type {request.RequestType} is not supported")
            };

            stopwatch.Stop();

            return new AgentResponse
            {
                RequestId = request.RequestId,
                Success = true,
                Result = result,
                ExecutionTime = stopwatch.Elapsed,
                Metadata = new Dictionary<string, object>
                {
                    ["AgentType"] = AgentType,
                    ["ProcessedAt"] = DateTime.UtcNow
                }
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ Error processing request {RequestId}", request.RequestId);
            
            return new AgentResponse
            {
                RequestId = request.RequestId,
                Success = false,
                ErrorMessage = ex.Message,
                ExecutionTime = stopwatch.Elapsed
            };
        }
    }

    public async Task<HealthStatus> GetHealthStatusAsync()
    {
        try
        {
            // Test LLM service connectivity
            var testPrompt = "Test connectivity";
            await _llmService.GenerateSQLAsync(testPrompt);
            
            return new HealthStatus
            {
                IsHealthy = true,
                Status = "Healthy",
                Metrics = new Dictionary<string, object>
                {
                    ["LastCheck"] = DateTime.UtcNow,
                    ["LLMServiceAvailable"] = true,
                    ["IsInitialized"] = _isInitialized
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for QueryUnderstandingAgent");
            
            return new HealthStatus
            {
                IsHealthy = false,
                Status = "Unhealthy",
                Issues = new List<string> { ex.Message }
            };
        }
    }

    public async Task InitializeAsync(Dictionary<string, object> configuration)
    {
        _logger.LogInformation("🚀 Initializing QueryUnderstandingAgent");
        
        // Initialize any required resources
        await Task.Delay(100); // Simulate initialization
        
        _isInitialized = true;
        _capabilities.LastHealthCheck = DateTime.UtcNow;
        
        _logger.LogInformation("✅ QueryUnderstandingAgent initialized successfully");
    }

    public async Task ShutdownAsync()
    {
        _logger.LogInformation("🛑 Shutting down QueryUnderstandingAgent");
        
        _isInitialized = false;
        _capabilities.IsAvailable = false;
        
        await Task.CompletedTask;
    }

    #endregion

    #region IQueryUnderstandingAgent Implementation

    public async Task<AgentQueryIntent> AnalyzeIntentAsync(string naturalLanguage, AgentContext? context = null)
    {
        _logger.LogDebug("🔍 Analyzing intent for query: {Query}", naturalLanguage);

        try
        {
            // Extract entities first
            var entities = await ExtractEntitiesAsync(naturalLanguage, context);
            
            // Classify query type
            var queryType = await ClassifyQueryTypeAsync(naturalLanguage, context);
            
            // Extract business context
            var businessContext = await ExtractBusinessContextAsync(naturalLanguage);
            
            // Assess complexity
            var complexity = await AssessComplexityAsync(naturalLanguage, context);
            
            // Detect ambiguities
            var ambiguities = await DetectAmbiguitiesAsync(naturalLanguage, context);
            
            // Calculate overall confidence
            var confidence = CalculateConfidence(entities, queryType, businessContext);

            var intent = new AgentQueryIntent
            {
                QueryType = queryType,
                Entities = entities,
                BusinessContext = businessContext,
                Confidence = confidence,
                Ambiguities = ambiguities,
                Complexity = complexity,
                Metadata = new Dictionary<string, object>
                {
                    ["OriginalQuery"] = naturalLanguage,
                    ["ProcessedAt"] = DateTime.UtcNow,
                    ["AgentVersion"] = "2.0.0"
                }
            };

            _logger.LogDebug("✅ Intent analysis complete. Type: {QueryType}, Confidence: {Confidence:F2}", 
                queryType, confidence);

            return intent;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error analyzing intent for query: {Query}", naturalLanguage);
            throw;
        }
    }

    public async Task<AgentQueryComplexity> AssessComplexityAsync(string query, AgentContext? context = null)
    {
        _logger.LogDebug("📊 Assessing complexity for query: {Query}", query);

        try
        {
            var complexity = new AgentQueryComplexity();
            
            // Analyze query patterns
            var lowerQuery = query.ToLowerInvariant();
            
            // Count tables (rough estimation)
            var tableKeywords = new[] { "from", "join", "into", "update", "delete from" };
            complexity.TableCount = tableKeywords.Sum(keyword => 
                Regex.Matches(lowerQuery, $@"\b{keyword}\s+\w+", RegexOptions.IgnoreCase).Count);
            
            // Count joins
            complexity.JoinCount = Regex.Matches(lowerQuery, @"\bjoin\b", RegexOptions.IgnoreCase).Count;
            
            // Count aggregations
            var aggregationFunctions = new[] { "sum", "count", "avg", "max", "min", "group by" };
            complexity.AggregationCount = aggregationFunctions.Sum(func => 
                Regex.Matches(lowerQuery, $@"\b{func}\b", RegexOptions.IgnoreCase).Count);
            
            // Count subqueries
            complexity.SubqueryCount = Regex.Matches(lowerQuery, @"\(.*select.*\)", RegexOptions.IgnoreCase).Count;
            
            // Check for window functions
            complexity.HasWindowFunctions = Regex.IsMatch(lowerQuery, @"\bover\s*\(", RegexOptions.IgnoreCase);
            
            // Check for recursive CTEs
            complexity.HasRecursiveCTE = Regex.IsMatch(lowerQuery, @"\bwith\s+recursive\b", RegexOptions.IgnoreCase);
            
            // Calculate complexity score
            complexity.ComplexityScore = CalculateComplexityScore(complexity);
            
            // Determine complexity level
            complexity.Level = complexity.ComplexityScore switch
            {
                < 0.3 => AgentComplexityLevel.Low,
                < 0.6 => AgentComplexityLevel.Medium,
                < 0.8 => AgentComplexityLevel.High,
                _ => AgentComplexityLevel.VeryHigh
            };

            complexity.Factors = new Dictionary<string, object>
            {
                ["TableCount"] = complexity.TableCount,
                ["JoinCount"] = complexity.JoinCount,
                ["AggregationCount"] = complexity.AggregationCount,
                ["SubqueryCount"] = complexity.SubqueryCount,
                ["HasWindowFunctions"] = complexity.HasWindowFunctions,
                ["HasRecursiveCTE"] = complexity.HasRecursiveCTE
            };

            _logger.LogDebug("✅ Complexity assessment complete. Level: {Level}, Score: {Score:F2}", 
                complexity.Level, complexity.ComplexityScore);

            return complexity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error assessing complexity for query: {Query}", query);
            throw;
        }
    }

    public async Task<List<QueryAmbiguity>> DetectAmbiguitiesAsync(string query, AgentContext? context = null)
    {
        _logger.LogDebug("🔍 Detecting ambiguities in query: {Query}", query);

        var ambiguities = new List<QueryAmbiguity>();

        try
        {
            // Use LLM to detect ambiguities
            var prompt = $@"
Analyze the following natural language query for ambiguities:
Query: ""{query}""

Identify potential ambiguities such as:
1. Unclear entity references
2. Temporal ambiguities (time periods)
3. Measurement ambiguities (units, metrics)
4. Scope ambiguities (which data to include)

Return a JSON array of ambiguities with type, description, and possible resolutions.
";

            var response = await _llmService.GenerateSQLAsync(prompt);

            // Parse LLM response (simplified - in production, use more robust parsing)
            if (!string.IsNullOrEmpty(response))
            {
                // Add basic ambiguity detection logic
                if (query.ToLowerInvariant().Contains("last") || query.ToLowerInvariant().Contains("recent"))
                {
                    ambiguities.Add(new QueryAmbiguity
                    {
                        Type = "TemporalAmbiguity",
                        Description = "Time period 'last' or 'recent' is ambiguous",
                        PossibleResolutions = new List<string> 
                        { 
                            "Last 7 days", 
                            "Last 30 days", 
                            "Last quarter" 
                        },
                        Severity = 0.7
                    });
                }
            }

            _logger.LogDebug("✅ Ambiguity detection complete. Found {Count} ambiguities", ambiguities.Count);
            return ambiguities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error detecting ambiguities for query: {Query}", query);
            return ambiguities; // Return partial results
        }
    }

    public async Task<List<EntityReference>> ExtractEntitiesAsync(string query, AgentContext? context = null)
    {
        _logger.LogDebug("🏷️ Extracting entities from query: {Query}", query);

        var entities = new List<EntityReference>();

        try
        {
            // Simple entity extraction (in production, use NLP libraries or LLM)
            var words = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var word in words)
            {
                var cleanWord = word.Trim('?', '.', ',', '!').ToLowerInvariant();
                
                // Detect potential table/entity names (simplified logic)
                if (IsLikelyEntityName(cleanWord))
                {
                    entities.Add(new EntityReference
                    {
                        Name = cleanWord,
                        Type = "Table", // Simplified - could be Table, Column, etc.
                        Confidence = 0.8,
                        Aliases = new List<string>()
                    });
                }
            }

            _logger.LogDebug("✅ Entity extraction complete. Found {Count} entities", entities.Count);
            return entities;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error extracting entities from query: {Query}", query);
            return entities; // Return partial results
        }
    }

    public async Task<string> ClassifyQueryTypeAsync(string query, AgentContext? context = null)
    {
        _logger.LogDebug("🏷️ Classifying query type for: {Query}", query);

        try
        {
            var lowerQuery = query.ToLowerInvariant();
            
            // Simple classification logic
            if (lowerQuery.Contains("show") || lowerQuery.Contains("list") || lowerQuery.Contains("get"))
                return "SELECT";
            
            if (lowerQuery.Contains("total") || lowerQuery.Contains("sum") || lowerQuery.Contains("count"))
                return "AGGREGATION";
            
            if (lowerQuery.Contains("compare") || lowerQuery.Contains("vs") || lowerQuery.Contains("versus"))
                return "COMPARISON";
            
            if (lowerQuery.Contains("trend") || lowerQuery.Contains("over time") || lowerQuery.Contains("change"))
                return "TREND_ANALYSIS";
            
            return "SELECT"; // Default
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error classifying query type: {Query}", query);
            return "UNKNOWN";
        }
    }

    #endregion

    #region Private Helper Methods

    private async Task<AgentBusinessContext> ExtractBusinessContextAsync(string query)
    {
        var context = new AgentBusinessContext();
        
        var lowerQuery = query.ToLowerInvariant();
        
        // Simple domain detection
        if (lowerQuery.Contains("sales") || lowerQuery.Contains("revenue") || lowerQuery.Contains("customer"))
            context.Domain = "Sales";
        else if (lowerQuery.Contains("finance") || lowerQuery.Contains("cost") || lowerQuery.Contains("budget"))
            context.Domain = "Finance";
        else if (lowerQuery.Contains("marketing") || lowerQuery.Contains("campaign") || lowerQuery.Contains("lead"))
            context.Domain = "Marketing";
        else
            context.Domain = "General";
        
        // Extract business terms (simplified)
        var businessTerms = new[] { "revenue", "profit", "customer", "sales", "cost", "budget", "roi" };
        context.BusinessTerms = businessTerms.Where(term => lowerQuery.Contains(term)).ToList();
        
        return context;
    }

    private double CalculateConfidence(List<EntityReference> entities, string queryType, AgentBusinessContext businessContext)
    {
        double confidence = 0.5; // Base confidence
        
        // Increase confidence based on entities found
        confidence += Math.Min(entities.Count * 0.1, 0.3);
        
        // Increase confidence if query type is clear
        if (queryType != "UNKNOWN")
            confidence += 0.2;
        
        // Increase confidence if business context is identified
        if (!string.IsNullOrEmpty(businessContext.Domain) && businessContext.Domain != "General")
            confidence += 0.1;
        
        return Math.Min(confidence, 1.0);
    }

    private double CalculateComplexityScore(AgentQueryComplexity complexity)
    {
        double score = 0.0;
        
        // Weight different complexity factors
        score += complexity.TableCount * 0.1;
        score += complexity.JoinCount * 0.15;
        score += complexity.AggregationCount * 0.1;
        score += complexity.SubqueryCount * 0.2;
        
        if (complexity.HasWindowFunctions) score += 0.2;
        if (complexity.HasRecursiveCTE) score += 0.3;
        
        return Math.Min(score, 1.0);
    }

    private bool IsLikelyEntityName(string word)
    {
        // Simple heuristics for entity detection
        if (word.Length < 3) return false;
        if (char.IsDigit(word[0])) return false;
        
        // Common business entity patterns
        var entityPatterns = new[] { "customer", "order", "product", "sale", "user", "account", "transaction" };
        return entityPatterns.Any(pattern => word.Contains(pattern));
    }

    #endregion
}
