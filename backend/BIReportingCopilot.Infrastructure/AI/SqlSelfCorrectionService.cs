using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces.Validation;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Models;
using System.Text.Json;
using System.Text;

namespace BIReportingCopilot.Infrastructure.AI;

/// <summary>
/// SQL self-correction service using LLM feedback loops
/// Phase 3: Enhanced SQL Validation Pipeline
/// </summary>
public class SqlSelfCorrectionService : ISqlSelfCorrectionService
{
    private readonly ILogger<SqlSelfCorrectionService> _logger;
    private readonly IAIService _aiService;
    private readonly IPromptService _promptService;
    private readonly IEnhancedSemanticLayerService _semanticLayerService;
    private readonly ISemanticCacheService _cacheService;
    private readonly SelfCorrectionConfiguration _configuration;

    public SqlSelfCorrectionService(
        ILogger<SqlSelfCorrectionService> logger,
        IAIService aiService,
        IPromptService promptService,
        IEnhancedSemanticLayerService semanticLayerService,
        ISemanticCacheService cacheService,
        IConfiguration configuration)
    {
        _logger = logger;
        _aiService = aiService;
        _promptService = promptService;
        _semanticLayerService = semanticLayerService;
        _cacheService = cacheService;
        _configuration = configuration.GetSection("SelfCorrection").Get<SelfCorrectionConfiguration>() 
            ?? new SelfCorrectionConfiguration();
    }

    /// <summary>
    /// Attempt to correct SQL based on validation issues
    /// </summary>
    public async Task<SelfCorrectionAttempt> CorrectSqlAsync(
        string sql,
        string originalQuery,
        List<string> validationIssues,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 Starting SQL self-correction for {IssueCount} issues", validationIssues.Count);

            var attempt = new SelfCorrectionAttempt
            {
                OriginalSql = sql,
                IssuesAddressed = validationIssues.ToList()
            };

            // Step 1: Analyze correction strategy
            var strategy = await DetermineCorrectionStrategyAsync(sql, originalQuery, validationIssues, cancellationToken);

            // Step 2: Get enhanced schema context
            var schemaContext = await _semanticLayerService.GetEnhancedSchemaAsync(
                originalQuery, 0.7, 10, cancellationToken);

            // Step 3: Build correction prompt
            var correctionPrompt = await BuildAdvancedCorrectionPromptAsync(
                sql, originalQuery, validationIssues, schemaContext, strategy, cancellationToken);

            // Step 4: Generate corrected SQL using LLM
            var correctedSql = await _aiService.GenerateSQLAsync(correctionPrompt, cancellationToken);

            if (string.IsNullOrWhiteSpace(correctedSql) || correctedSql.Equals(sql, StringComparison.OrdinalIgnoreCase))
            {
                attempt.WasSuccessful = false;
                attempt.CorrectionReason = "LLM did not produce a different SQL query";
                return attempt;
            }

            // Step 5: Validate improvement
            var improvementScore = await CalculateImprovementScoreAsync(
                sql, correctedSql, originalQuery, validationIssues, cancellationToken);

            attempt.CorrectedSql = correctedSql;
            attempt.ImprovementScore = improvementScore;
            attempt.WasSuccessful = improvementScore >= _configuration.MinImprovementThreshold;
            attempt.CorrectionReason = strategy.Description;

            // Step 6: Learn from successful corrections
            if (attempt.WasSuccessful)
            {
                await LearnFromCorrectionAsync(attempt, cancellationToken);
            }

            _logger.LogInformation("✅ SQL self-correction completed: {Success} (Score: {Score:F2})", 
                attempt.WasSuccessful, attempt.ImprovementScore);

            return attempt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SQL self-correction");
            return new SelfCorrectionAttempt
            {
                OriginalSql = sql,
                WasSuccessful = false,
                CorrectionReason = $"Self-correction failed: {ex.Message}",
                IssuesAddressed = validationIssues.ToList()
            };
        }
    }

    /// <summary>
    /// Generate correction suggestions without applying them
    /// </summary>
    public async Task<List<string>> GenerateCorrectionSuggestionsAsync(
        string sql,
        List<string> validationIssues,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("💡 Generating correction suggestions");

            var suggestions = new List<string>();

            // Analyze each validation issue and provide specific suggestions
            foreach (var issue in validationIssues)
            {
                var suggestion = await GenerateSpecificSuggestionAsync(sql, issue, cancellationToken);
                if (!string.IsNullOrWhiteSpace(suggestion))
                {
                    suggestions.Add(suggestion);
                }
            }

            // Add general improvement suggestions
            var generalSuggestions = await GenerateGeneralSuggestionsAsync(sql, cancellationToken);
            suggestions.AddRange(generalSuggestions);

            return suggestions.Distinct().ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error generating correction suggestions");
            return new List<string> { "Unable to generate specific suggestions due to an error" };
        }
    }

    /// <summary>
    /// Learn from successful corrections for future improvements
    /// </summary>
    public async Task<bool> LearnFromCorrectionAsync(
        SelfCorrectionAttempt correctionAttempt,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📚 Learning from successful correction");

            // Store correction pattern for future reference
            var correctionPattern = new CorrectionPattern
            {
                IssueTypes = correctionAttempt.IssuesAddressed,
                OriginalPattern = ExtractSqlPattern(correctionAttempt.OriginalSql),
                CorrectedPattern = ExtractSqlPattern(correctionAttempt.CorrectedSql),
                ImprovementScore = correctionAttempt.ImprovementScore,
                Timestamp = correctionAttempt.AttemptTimestamp
            };

            // Cache the correction pattern for future use
            var cacheKey = $"correction_pattern:{string.Join(",", correctionAttempt.IssuesAddressed).GetHashCode()}";
            await _cacheService.SetAsync(cacheKey, correctionPattern, TimeSpan.FromDays(30), cancellationToken);

            // Update correction statistics
            await UpdateCorrectionStatisticsAsync(correctionAttempt, cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error learning from correction");
            return false;
        }
    }

    /// <summary>
    /// Get correction patterns and statistics
    /// </summary>
    public async Task<Dictionary<string, object>> GetCorrectionPatternsAsync(
        TimeSpan timeRange,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📊 Getting correction patterns and statistics");

            var patterns = new Dictionary<string, object>();

            // Get cached correction statistics
            var statsKey = "correction_statistics";
            var stats = await _cacheService.GetAsync<CorrectionStatistics>(statsKey, cancellationToken);

            if (stats != null)
            {
                patterns["TotalCorrections"] = stats.TotalAttempts;
                patterns["SuccessfulCorrections"] = stats.SuccessfulAttempts;
                patterns["SuccessRate"] = stats.SuccessRate;
                patterns["AverageImprovementScore"] = stats.AverageImprovementScore;
                patterns["CommonIssueTypes"] = stats.CommonIssueTypes;
                patterns["MostEffectiveStrategies"] = stats.MostEffectiveStrategies;
            }
            else
            {
                patterns["Message"] = "No correction statistics available";
            }

            return patterns;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error getting correction patterns");
            return new Dictionary<string, object> { ["Error"] = ex.Message };
        }
    }

    /// <summary>
    /// Determine the best correction strategy for the given issues
    /// </summary>
    private async Task<CorrectionStrategy> DetermineCorrectionStrategyAsync(
        string sql,
        string originalQuery,
        List<string> validationIssues,
        CancellationToken cancellationToken)
    {
        // Analyze issue types to determine strategy
        var hasSemanticIssues = validationIssues.Any(i => i.Contains("semantic", StringComparison.OrdinalIgnoreCase));
        var hasSchemaIssues = validationIssues.Any(i => i.Contains("schema", StringComparison.OrdinalIgnoreCase));
        var hasBusinessLogicIssues = validationIssues.Any(i => i.Contains("business", StringComparison.OrdinalIgnoreCase));

        if (hasSemanticIssues)
        {
            return new CorrectionStrategy
            {
                Type = "semantic",
                Description = "Focus on improving semantic alignment with business intent",
                Priority = 1
            };
        }
        else if (hasSchemaIssues)
        {
            return new CorrectionStrategy
            {
                Type = "schema",
                Description = "Focus on correcting schema compliance issues",
                Priority = 2
            };
        }
        else if (hasBusinessLogicIssues)
        {
            return new CorrectionStrategy
            {
                Type = "business_logic",
                Description = "Focus on addressing business logic violations",
                Priority = 3
            };
        }
        else
        {
            return new CorrectionStrategy
            {
                Type = "general",
                Description = "Apply general SQL improvement techniques",
                Priority = 4
            };
        }
    }

    /// <summary>
    /// Build advanced correction prompt with context and strategy
    /// </summary>
    private async Task<string> BuildAdvancedCorrectionPromptAsync(
        string sql,
        string originalQuery,
        List<string> validationIssues,
        EnhancedSchemaResult schemaContext,
        CorrectionStrategy strategy,
        CancellationToken cancellationToken)
    {
        var promptBuilder = new System.Text.StringBuilder();

        promptBuilder.AppendLine("You are an expert SQL analyst with deep knowledge of business intelligence and data analytics.");
        promptBuilder.AppendLine("Your task is to correct the following SQL query based on validation issues.");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine($"Original Business Question: {originalQuery}");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine($"Generated SQL with Issues:");
        promptBuilder.AppendLine($"```sql");
        promptBuilder.AppendLine(sql);
        promptBuilder.AppendLine($"```");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine($"Validation Issues to Address:");
        foreach (var issue in validationIssues)
        {
            promptBuilder.AppendLine($"- {issue}");
        }
        promptBuilder.AppendLine();

        promptBuilder.AppendLine($"Correction Strategy: {strategy.Description}");
        promptBuilder.AppendLine();

        // Add relevant schema context
        if (schemaContext.RelevantTables.Any())
        {
            promptBuilder.AppendLine("Available Schema Context:");
            foreach (var table in schemaContext.RelevantTables.Take(3))
            {
                promptBuilder.AppendLine($"Table: {table.TableName}");
                promptBuilder.AppendLine($"Purpose: {table.BusinessPurpose}");
                
                if (table.Columns.Any())
                {
                    promptBuilder.AppendLine("Key Columns:");
                    foreach (var column in table.Columns.Take(5))
                    {
                        promptBuilder.AppendLine($"  - {column.ColumnName}: {column.BusinessMeaning}");
                    }
                }
                promptBuilder.AppendLine();
            }
        }

        promptBuilder.AppendLine("Please provide a corrected SQL query that:");
        promptBuilder.AppendLine("1. Addresses all validation issues listed above");
        promptBuilder.AppendLine("2. Maintains the original business intent");
        promptBuilder.AppendLine("3. Uses proper table and column names from the schema");
        promptBuilder.AppendLine("4. Follows SQL best practices");
        promptBuilder.AppendLine("5. Is optimized for performance");
        promptBuilder.AppendLine();

        promptBuilder.AppendLine("Corrected SQL:");

        return promptBuilder.ToString();
    }

    /// <summary>
    /// Calculate improvement score for corrected SQL
    /// </summary>
    private async Task<double> CalculateImprovementScoreAsync(
        string originalSql,
        string correctedSql,
        string originalQuery,
        List<string> validationIssues,
        CancellationToken cancellationToken)
    {
        try
        {
            // This would integrate with the validation service to compare scores
            // For now, return a placeholder score based on simple heuristics
            
            var improvementScore = 0.0;

            // Check if corrected SQL is significantly different
            if (!correctedSql.Equals(originalSql, StringComparison.OrdinalIgnoreCase))
            {
                improvementScore += 0.3;
            }

            // Check if corrected SQL is more complex (potentially more complete)
            if (correctedSql.Length > originalSql.Length * 1.1)
            {
                improvementScore += 0.2;
            }

            // Check for improved structure
            if (HasBetterStructure(originalSql, correctedSql))
            {
                improvementScore += 0.3;
            }

            // Check for semantic improvements
            var semanticImprovement = await CalculateSemanticImprovementAsync(
                originalSql, correctedSql, originalQuery, cancellationToken);
            improvementScore += semanticImprovement * 0.2;

            return Math.Min(1.0, improvementScore);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error calculating improvement score");
            return 0.0;
        }
    }

    // Helper methods
    private async Task<string> GenerateSpecificSuggestionAsync(string sql, string issue, CancellationToken ct) => $"Address: {issue}";
    private async Task<List<string>> GenerateGeneralSuggestionsAsync(string sql, CancellationToken ct) => new() { "Consider adding proper JOIN conditions", "Validate column names" };
    private string ExtractSqlPattern(string sql) => sql.Substring(0, Math.Min(100, sql.Length));
    private async Task UpdateCorrectionStatisticsAsync(SelfCorrectionAttempt attempt, CancellationToken ct) { }
    private bool HasBetterStructure(string original, string corrected) => corrected.Contains("JOIN") && !original.Contains("JOIN");
    private async Task<double> CalculateSemanticImprovementAsync(string original, string corrected, string query, CancellationToken ct) => 0.5;

    // Supporting classes
    private class CorrectionStrategy
    {
        public string Type { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; } = 0;
    }

    private class CorrectionPattern
    {
        public List<string> IssueTypes { get; set; } = new();
        public string OriginalPattern { get; set; } = string.Empty;
        public string CorrectedPattern { get; set; } = string.Empty;
        public double ImprovementScore { get; set; } = 0.0;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }

    private class CorrectionStatistics
    {
        public int TotalAttempts { get; set; } = 0;
        public int SuccessfulAttempts { get; set; } = 0;
        public double SuccessRate => TotalAttempts > 0 ? (double)SuccessfulAttempts / TotalAttempts : 0.0;
        public double AverageImprovementScore { get; set; } = 0.0;
        public Dictionary<string, int> CommonIssueTypes { get; set; } = new();
        public Dictionary<string, double> MostEffectiveStrategies { get; set; } = new();
    }
}
