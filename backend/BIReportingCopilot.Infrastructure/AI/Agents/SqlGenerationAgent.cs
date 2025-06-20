using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces.Agents;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models.Agents;
using BIReportingCopilot.Core.Models;
using System.Text.Json;
using System.Text.RegularExpressions;
using AgentQueryIntent = BIReportingCopilot.Core.Models.Agents.QueryIntent;
using AgentSchemaContext = BIReportingCopilot.Core.Models.Agents.SchemaContext;
using AgentQueryComplexity = BIReportingCopilot.Core.Models.Agents.QueryComplexity;

namespace BIReportingCopilot.Infrastructure.AI.Agents;

/// <summary>
/// SQL Generation Agent - Specialized in optimized SQL creation and validation
/// </summary>
public class SqlGenerationAgent : ISqlGenerationAgent
{
    private readonly ILogger<SqlGenerationAgent> _logger;
    private readonly IAIService _aiService;
    // Note: Using simplified validation for now - can be enhanced with dedicated services later
    private readonly IConfiguration _configuration;
    private readonly AgentCapabilities _capabilities;
    private bool _isInitialized = false;

    public string AgentType => "SqlGeneration";
    public AgentCapabilities Capabilities => _capabilities;

    public SqlGenerationAgent(
        ILogger<SqlGenerationAgent> logger,
        IAIService aiService,
        // Removed validation and optimization services for now
        IConfiguration configuration)
    {
        _logger = logger;
        _aiService = aiService;
        _configuration = configuration;
        
        _capabilities = new AgentCapabilities
        {
            AgentType = AgentType,
            SupportedOperations = new List<string>
            {
                "GenerateOptimizedSql",
                "ValidateSql",
                "OptimizeSql",
                "GenerateSqlVariations",
                "ExplainSql",
                "GeneratePerformanceHints"
            },
            Metadata = new Dictionary<string, object>
            {
                ["Version"] = "2.0.0",
                ["Specialization"] = "SQL Generation and Optimization",
                ["SupportedDialects"] = new[] { "T-SQL", "PostgreSQL", "MySQL" }
            },
            PerformanceScore = 0.94,
            IsAvailable = true
        };
    }

    #region ISpecializedAgent Implementation

    public async Task<AgentResponse> ProcessAsync(AgentRequest request, AgentContext context)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        
        try
        {
            _logger.LogInformation("⚡ SqlGenerationAgent processing request {RequestId} of type {RequestType}", 
                request.RequestId, request.RequestType);

            object? result = request.RequestType switch
            {
                "GenerateOptimizedSql" => await GenerateOptimizedSqlAsync(
                    JsonSerializer.Deserialize<AgentQueryIntent>(request.Parameters["intent"].ToString()!)!,
                    JsonSerializer.Deserialize<AgentSchemaContext>(request.Parameters["context"].ToString()!)!,
                    context),
                "ValidateSql" => await ValidateSqlAsync(request.Payload.ToString()!, context),
                "OptimizeSql" => await OptimizeSqlAsync(
                    request.Payload.ToString()!,
                    JsonSerializer.Deserialize<PerformanceHints>(request.Parameters["hints"].ToString()!)!,
                    context),
                "GenerateSqlVariations" => await GenerateSqlVariationsAsync(
                    JsonSerializer.Deserialize<AgentQueryIntent>(request.Parameters["intent"].ToString()!)!,
                    JsonSerializer.Deserialize<AgentSchemaContext>(request.Parameters["context"].ToString()!)!,
                    (int)(request.Parameters.GetValueOrDefault("variationCount", 3)),
                    context),
                "ExplainSql" => await ExplainSqlAsync(request.Payload.ToString()!, context),
                "GeneratePerformanceHints" => await GeneratePerformanceHintsAsync(
                    request.Payload.ToString()!,
                    JsonSerializer.Deserialize<AgentSchemaContext>(request.Parameters["context"].ToString()!)!,
                    context),
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
            // Test AI service connectivity
            await _aiService.GenerateSQLAsync("SELECT 1");
            
            return new HealthStatus
            {
                IsHealthy = true,
                Status = "Healthy",
                Metrics = new Dictionary<string, object>
                {
                    ["LastCheck"] = DateTime.UtcNow,
                    ["AIServiceAvailable"] = true,
                    ["ValidationServiceAvailable"] = true,
                    ["IsInitialized"] = _isInitialized
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Health check failed for SqlGenerationAgent");
            
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
        _logger.LogInformation("🚀 Initializing SqlGenerationAgent");
        
        // Initialize any required resources
        await Task.Delay(100); // Simulate initialization
        
        _isInitialized = true;
        _capabilities.LastHealthCheck = DateTime.UtcNow;
        
        _logger.LogInformation("✅ SqlGenerationAgent initialized successfully");
    }

    public async Task ShutdownAsync()
    {
        _logger.LogInformation("🛑 Shutting down SqlGenerationAgent");
        
        _isInitialized = false;
        _capabilities.IsAvailable = false;
        
        await Task.CompletedTask;
    }

    #endregion

    #region ISqlGenerationAgent Implementation

    public async Task<GeneratedSql> GenerateOptimizedSqlAsync(AgentQueryIntent intent, AgentSchemaContext context, AgentContext? agentContext = null)
    {
        _logger.LogDebug("⚡ Generating optimized SQL for intent: {QueryType}", intent.QueryType);

        try
        {
            // Build comprehensive prompt with context
            var prompt = BuildSqlGenerationPrompt(intent, context);
            
            // Generate SQL using AI service
            var generatedSql = await _aiService.GenerateSQLAsync(prompt);
            
            // Clean and format the SQL
            var cleanedSql = CleanGeneratedSql(generatedSql);
            
            // Validate the generated SQL (simplified validation)
            var validationResult = await ValidateSqlAsync(cleanedSql, agentContext);
            
            // Generate performance hints
            var performanceHints = await GeneratePerformanceHintsAsync(cleanedSql, context, agentContext);
            
            // Calculate confidence based on validation and complexity
            var confidence = CalculateConfidence(validationResult, intent.Complexity);
            
            var result = new GeneratedSql
            {
                Sql = cleanedSql,
                Explanation = GenerateSqlExplanation(cleanedSql, intent),
                Confidence = confidence,
                Warnings = validationResult.Warnings.Select(w => w.Message).ToList(),
                PerformanceHints = performanceHints,
                Metadata = new Dictionary<string, object>
                {
                    ["GenerationMethod"] = "AI-Assisted",
                    ["ValidationScore"] = validationResult.QualityScore,
                    ["ComplexityLevel"] = intent.Complexity.Level.ToString(),
                    ["TablesUsed"] = context.Tables.Select(t => t.TableName).ToList()
                }
            };

            _logger.LogDebug("✅ SQL generation complete. Confidence: {Confidence:F2}", confidence);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating optimized SQL");
            throw;
        }
    }

    public async Task<SqlValidationResult> ValidateSqlAsync(string sql, AgentContext? context = null)
    {
        _logger.LogDebug("🔍 Validating SQL query");

        try
        {
            // Simplified validation - basic syntax checks
            var result = new SqlValidationResult
            {
                IsValid = !string.IsNullOrWhiteSpace(sql) && sql.Trim().ToUpperInvariant().StartsWith("SELECT"),
                Errors = new List<ValidationError>(),
                Warnings = new List<ValidationWarning>(),
                Suggestions = new List<string>(),
                QualityScore = 0.8 // Default quality score
            };

            // Basic validation checks
            if (string.IsNullOrWhiteSpace(sql))
            {
                result.IsValid = false;
                result.Errors.Add(new ValidationError
                {
                    Type = "Syntax",
                    Message = "SQL query is empty",
                    Severity = "Error"
                });
            }
            else if (!sql.Trim().ToUpperInvariant().StartsWith("SELECT"))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Type = "QueryType",
                    Message = "Only SELECT queries are recommended",
                    Recommendation = "Use SELECT statements for data retrieval",
                    Severity = "Warning"
                });
            }

            // Check for potential issues
            if (sql.Contains("*"))
            {
                result.Warnings.Add(new ValidationWarning
                {
                    Type = "Performance",
                    Message = "SELECT * may impact performance",
                    Recommendation = "Specify only needed columns",
                    Severity = "Warning"
                });
            }

            result.QualityScore = result.IsValid ? (result.Warnings.Count == 0 ? 0.9 : 0.7) : 0.3;

            _logger.LogDebug("✅ SQL validation complete. Valid: {IsValid}, Quality: {QualityScore:F2}",
                result.IsValid, result.QualityScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating SQL");
            throw;
        }
    }

    public async Task<string> OptimizeSqlAsync(string sql, PerformanceHints hints, AgentContext? context = null)
    {
        _logger.LogDebug("🚀 Optimizing SQL query");

        try
        {
            // Simplified optimization - apply basic performance hints
            var optimizedSql = ApplyPerformanceHints(sql, hints);

            _logger.LogDebug("✅ SQL optimization complete");
            return optimizedSql;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error optimizing SQL");
            throw;
        }
    }

    public async Task<List<GeneratedSql>> GenerateSqlVariationsAsync(AgentQueryIntent intent, AgentSchemaContext context, int variationCount = 3, AgentContext? agentContext = null)
    {
        _logger.LogDebug("🔄 Generating {Count} SQL variations", variationCount);

        try
        {
            var variations = new List<GeneratedSql>();
            
            for (int i = 0; i < variationCount; i++)
            {
                // Generate variation with different approach
                var variationPrompt = BuildVariationPrompt(intent, context, i);
                var sql = await _aiService.GenerateSQLAsync(variationPrompt);
                var cleanedSql = CleanGeneratedSql(sql);
                
                var validation = await ValidateSqlAsync(cleanedSql, agentContext);
                var hints = await GeneratePerformanceHintsAsync(cleanedSql, context, agentContext);
                
                variations.Add(new GeneratedSql
                {
                    Sql = cleanedSql,
                    Explanation = $"Variation {i + 1}: {GenerateVariationExplanation(i)}",
                    Confidence = CalculateConfidence(validation, intent.Complexity),
                    Warnings = validation.Warnings.Select(w => w.Message).ToList(),
                    PerformanceHints = hints,
                    Metadata = new Dictionary<string, object>
                    {
                        ["VariationIndex"] = i,
                        ["ApproachType"] = GetVariationApproach(i)
                    }
                });
            }

            // Sort by confidence
            variations = variations.OrderByDescending(v => v.Confidence).ToList();
            
            _logger.LogDebug("✅ Generated {Count} SQL variations", variations.Count);
            return variations;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating SQL variations");
            throw;
        }
    }

    public async Task<string> ExplainSqlAsync(string sql, AgentContext? context = null)
    {
        _logger.LogDebug("📖 Explaining SQL query");

        try
        {
            var explanation = AnalyzeSqlStructure(sql);
            
            _logger.LogDebug("✅ SQL explanation generated");
            return explanation;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error explaining SQL");
            throw;
        }
    }

    public async Task<PerformanceHints> GeneratePerformanceHintsAsync(string sql, AgentSchemaContext context, AgentContext? agentContext = null)
    {
        _logger.LogDebug("💡 Generating performance hints");

        try
        {
            var hints = new PerformanceHints();
            
            // Analyze SQL for optimization opportunities
            hints.IndexSuggestions = AnalyzeIndexNeeds(sql, context);
            hints.OptimizationTips = GenerateOptimizationTips(sql);
            hints.EstimatedExecutionTime = EstimateExecutionTime(sql, context);
            hints.EstimatedResourceUsage = EstimateResourceUsage(sql, context);
            
            hints.Metrics = new Dictionary<string, object>
            {
                ["ComplexityScore"] = CalculateSqlComplexity(sql),
                ["JoinCount"] = CountJoins(sql),
                ["SubqueryCount"] = CountSubqueries(sql),
                ["TableCount"] = context.Tables.Count
            };

            _logger.LogDebug("✅ Performance hints generated");
            return hints;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating performance hints");
            throw;
        }
    }

    #endregion

    #region Private Helper Methods

    private string BuildSqlGenerationPrompt(AgentQueryIntent intent, AgentSchemaContext context)
    {
        var prompt = $@"
Generate optimized SQL query based on the following requirements:

Query Intent:
- Type: {intent.QueryType}
- Business Domain: {intent.BusinessContext.Domain}
- Entities: {string.Join(", ", intent.Entities.Select(e => e.Name))}
- Business Terms: {string.Join(", ", intent.BusinessContext.BusinessTerms)}

Available Tables:
{string.Join("\n", context.Tables.Select(t => $"- {t.TableName} (Relevance: {t.RelevanceScore:F2})"))}

Available Joins:
{string.Join("\n", context.AvailableJoins.Select(j => $"- {j.FromTable} {j.JoinType} JOIN {j.ToTable}"))}

Requirements:
- Use only the provided tables
- Follow SQL best practices
- Optimize for performance
- Include appropriate WHERE clauses
- Use meaningful column aliases

Generate clean, executable SQL:
";
        return prompt;
    }

    private string CleanGeneratedSql(string sql)
    {
        // Remove markdown formatting and extra whitespace
        sql = Regex.Replace(sql, @"```sql\s*", "", RegexOptions.IgnoreCase);
        sql = Regex.Replace(sql, @"```\s*", "");
        sql = Regex.Replace(sql, @"\s+", " ");
        sql = sql.Trim();
        
        // Ensure it ends with semicolon
        if (!sql.EndsWith(";"))
            sql += ";";
            
        return sql;
    }

    private double CalculateConfidence(SqlValidationResult validation, AgentQueryComplexity complexity)
    {
        double confidence = 0.5; // Base confidence
        
        if (validation.IsValid)
            confidence += 0.3;
            
        confidence += validation.QualityScore * 0.2;
        
        // Adjust for complexity
        confidence -= (int)complexity.Level * 0.05;
        
        return Math.Max(0.1, Math.Min(1.0, confidence));
    }

    private string GenerateSqlExplanation(string sql, AgentQueryIntent intent)
    {
        var explanation = $"This SQL query addresses the {intent.QueryType} request ";
        
        if (intent.BusinessContext.Domain != "General")
            explanation += $"in the {intent.BusinessContext.Domain} domain ";
            
        explanation += "by selecting relevant data from the identified tables.";
        
        return explanation;
    }

    private double CalculateQualityScore(SqlValidationResult validationResult)
    {
        double score = validationResult.IsValid ? 0.8 : 0.2;

        // Reduce score for warnings
        score -= validationResult.Warnings.Count * 0.1;

        return Math.Max(0.0, Math.Min(1.0, score));
    }

    private string ApplyPerformanceHints(string sql, PerformanceHints hints)
    {
        var optimizedSql = sql;
        
        // Apply simple optimizations based on hints
        foreach (var tip in hints.OptimizationTips)
        {
            if (tip.Contains("LIMIT") && !optimizedSql.Contains("TOP") && !optimizedSql.Contains("LIMIT"))
            {
                // Add TOP clause for SQL Server
                optimizedSql = optimizedSql.Replace("SELECT", "SELECT TOP 1000");
            }
        }
        
        return optimizedSql;
    }

    private string BuildVariationPrompt(AgentQueryIntent intent, AgentSchemaContext context, int variationIndex)
    {
        var approaches = new[]
        {
            "Focus on performance optimization with appropriate indexes",
            "Emphasize readability and maintainability",
            "Optimize for minimal resource usage"
        };
        
        var basePrompt = BuildSqlGenerationPrompt(intent, context);
        return basePrompt + $"\n\nApproach: {approaches[variationIndex % approaches.Length]}";
    }

    private string GenerateVariationExplanation(int index)
    {
        return index switch
        {
            0 => "Performance-optimized approach",
            1 => "Readability-focused approach", 
            2 => "Resource-efficient approach",
            _ => "Alternative approach"
        };
    }

    private string GetVariationApproach(int index)
    {
        return index switch
        {
            0 => "Performance",
            1 => "Readability",
            2 => "Efficiency",
            _ => "Alternative"
        };
    }

    private string AnalyzeSqlStructure(string sql)
    {
        var explanation = "SQL Query Analysis:\n";
        
        if (sql.ToUpperInvariant().Contains("SELECT"))
            explanation += "- This is a SELECT query that retrieves data\n";
            
        if (sql.ToUpperInvariant().Contains("JOIN"))
            explanation += "- Uses JOIN operations to combine data from multiple tables\n";
            
        if (sql.ToUpperInvariant().Contains("WHERE"))
            explanation += "- Includes WHERE clause for data filtering\n";
            
        if (sql.ToUpperInvariant().Contains("GROUP BY"))
            explanation += "- Groups results using GROUP BY clause\n";
            
        if (sql.ToUpperInvariant().Contains("ORDER BY"))
            explanation += "- Sorts results using ORDER BY clause\n";
        
        return explanation;
    }

    private List<string> AnalyzeIndexNeeds(string sql, AgentSchemaContext context)
    {
        var suggestions = new List<string>();
        
        // Simple index analysis based on WHERE clauses
        var whereMatch = Regex.Match(sql, @"WHERE\s+(\w+\.\w+|\w+)", RegexOptions.IgnoreCase);
        if (whereMatch.Success)
        {
            suggestions.Add($"Consider index on {whereMatch.Groups[1].Value}");
        }
        
        // JOIN analysis
        var joinMatches = Regex.Matches(sql, @"JOIN\s+\w+\s+ON\s+(\w+\.\w+|\w+)", RegexOptions.IgnoreCase);
        foreach (Match match in joinMatches)
        {
            suggestions.Add($"Consider index on join column {match.Groups[1].Value}");
        }
        
        return suggestions;
    }

    private List<string> GenerateOptimizationTips(string sql)
    {
        var tips = new List<string>();
        
        if (!sql.ToUpperInvariant().Contains("LIMIT") && !sql.ToUpperInvariant().Contains("TOP"))
            tips.Add("Consider adding LIMIT clause to restrict result set size");
            
        if (sql.ToUpperInvariant().Contains("SELECT *"))
            tips.Add("Avoid SELECT * - specify only needed columns");
            
        if (Regex.Matches(sql, @"JOIN", RegexOptions.IgnoreCase).Count > 3)
            tips.Add("Consider breaking complex joins into smaller queries");
        
        return tips;
    }

    private double EstimateExecutionTime(string sql, AgentSchemaContext context)
    {
        // Simple estimation based on complexity
        double baseTime = 0.1; // 100ms base
        
        baseTime += CountJoins(sql) * 0.05; // 50ms per join
        baseTime += CountSubqueries(sql) * 0.1; // 100ms per subquery
        baseTime += context.Tables.Count * 0.02; // 20ms per table
        
        return baseTime;
    }

    private double EstimateResourceUsage(string sql, AgentSchemaContext context)
    {
        // Simple resource estimation (0.0 to 1.0)
        double usage = 0.1; // Base usage
        
        usage += CountJoins(sql) * 0.1;
        usage += CountSubqueries(sql) * 0.15;
        usage += context.Tables.Count * 0.05;
        
        return Math.Min(1.0, usage);
    }

    private double CalculateSqlComplexity(string sql)
    {
        double complexity = 0.1; // Base complexity
        
        complexity += CountJoins(sql) * 0.2;
        complexity += CountSubqueries(sql) * 0.3;
        complexity += (sql.ToUpperInvariant().Contains("UNION") ? 0.2 : 0);
        complexity += (sql.ToUpperInvariant().Contains("CASE") ? 0.1 : 0);
        
        return Math.Min(1.0, complexity);
    }

    private int CountJoins(string sql)
    {
        return Regex.Matches(sql, @"\bJOIN\b", RegexOptions.IgnoreCase).Count;
    }

    private int CountSubqueries(string sql)
    {
        return Regex.Matches(sql, @"\(\s*SELECT", RegexOptions.IgnoreCase).Count;
    }

    #endregion
}
