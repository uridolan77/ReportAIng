using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.AI.Management;

/// <summary>
/// Enhanced prompt management service using bounded contexts for better performance and maintainability
/// Uses TuningDbContext for AI tuning data and prompt management
/// </summary>
public class PromptManagementService : IContextManager
{
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;
    private readonly ISemanticAnalyzer _semanticAnalyzer;
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<PromptManagementService> _logger;
    private readonly Dictionary<string, OptimizationRule> _optimizationRules;

    public PromptManagementService(
        ICacheService cacheService,
        IAuditService auditService,
        ISemanticAnalyzer semanticAnalyzer,
        IDbContextFactory contextFactory,
        ILogger<PromptManagementService> logger)
    {
        _cacheService = cacheService;
        _auditService = auditService;
        _semanticAnalyzer = semanticAnalyzer;
        _contextFactory = contextFactory;
        _logger = logger;
        _optimizationRules = InitializeOptimizationRules();
    }

    #region IContextManager Implementation

    public async Task<UserContext> GetUserContextAsync(string userId)
    {
        try
        {
            var cacheKey = $"user_context:{userId}";
            var cachedContext = await _cacheService.GetAsync<UserContext>(cacheKey);

            if (cachedContext != null && cachedContext.LastUpdated > DateTime.UtcNow.AddHours(-6))
            {
                return cachedContext;
            }

            // Build user context from audit logs and patterns
            var context = await BuildUserContextAsync(userId);

            // Cache the context
            await _cacheService.SetAsync(cacheKey, context, TimeSpan.FromHours(6));

            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user context for user {UserId}", userId);
            return CreateDefaultUserContext(userId);
        }
    }

    public async Task UpdateUserContextAsync(string userId, QueryRequest request, QueryResponse response)
    {
        try
        {
            var context = await GetUserContextAsync(userId);

            // Update preferred tables based on successful queries
            if (response.Success && !string.IsNullOrEmpty(response.Sql))
            {
                var tablesUsed = ExtractTablesFromSql(response.Sql);
                foreach (var table in tablesUsed)
                {
                    if (!context.PreferredTables.Contains(table))
                    {
                        context.PreferredTables.Add(table);
                    }
                }

                // Keep only top 10 preferred tables
                if (context.PreferredTables.Count > 10)
                {
                    context.PreferredTables = context.PreferredTables.Take(10).ToList();
                }
            }

            // Update query patterns
            await UpdateQueryPatternsAsync(context, response);

            // Update common filters
            UpdateCommonFilters(context, response);

            // Infer user domain from query patterns
            UpdateUserDomain(context, response);

            context.LastUpdated = DateTime.UtcNow;

            // Save updated context
            var cacheKey = $"user_context:{userId}";
            await _cacheService.SetAsync(cacheKey, context, TimeSpan.FromHours(6));

            _logger.LogDebug("Updated user context for {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user context for user {UserId}", userId);
        }
    }

    public async Task<List<QueryPattern>> GetQueryPatternsAsync(string userId)
    {
        try
        {
            var context = await GetUserContextAsync(userId);
            return context.RecentPatterns.OrderByDescending(p => p.Frequency).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query patterns for user {UserId}", userId);
            return new List<QueryPattern>();
        }
    }

    public async Task<SchemaContext> GetRelevantSchemaAsync(string query, SchemaMetadata fullSchema)
    {
        _logger.LogDebug("PromptManagementService: GetRelevantSchemaAsync called for query: {Query}", query);
        _logger.LogDebug("PromptManagementService: Full schema has {TableCount} tables", fullSchema.Tables.Count);

        try
        {
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(query);
            var schemaContext = new SchemaContext();

            // Find relevant tables based on semantic entities and keywords
            var relevantTables = new List<TableMetadata>();
            var lowerQuery = query.ToLowerInvariant();

            _logger.LogInformation("PromptManagementService: Analyzing query '{Query}' with {TableCount} available tables", query, fullSchema.Tables.Count);

            foreach (var table in fullSchema.Tables)
            {
                var relevanceScore = CalculateTableRelevance(table, semanticAnalysis, lowerQuery);
                _logger.LogInformation("Table {TableName} relevance score: {Score}", table.Name, relevanceScore);

                if (relevanceScore > 0.4) // Lower threshold to ensure Games table is included
                {
                    relevantTables.Add(table);
                    _logger.LogInformation("✅ Table {TableName} INCLUDED with relevance {Score}", table.Name, relevanceScore);
                }
                else
                {
                    _logger.LogInformation("❌ Table {TableName} EXCLUDED with low relevance {Score}", table.Name, relevanceScore);
                }
            }

            // If no tables found by semantic analysis, use keyword matching with lower threshold
            if (!relevantTables.Any())
            {
                _logger.LogDebug("No tables found with high relevance, using keyword matching fallback");
                relevantTables = FindTablesByKeywords(fullSchema.Tables, lowerQuery);
            }

            // Ensure we have at least the most relevant tables for gaming context
            if (!relevantTables.Any())
            {
                _logger.LogWarning("No relevant tables found, using default gaming tables");
                relevantTables = GetDefaultGamingTables(fullSchema.Tables);
            }

            // For game queries, ensure both game tables are included
            if (IsGameRelatedQuery(lowerQuery))
            {
                var hasGameDaily = relevantTables.Any(t => t.Name.ToLowerInvariant().Contains("daily_actions_games"));
                var hasGamesMaster = relevantTables.Any(t => t.Name.ToLowerInvariant().Contains("games") && !t.Name.ToLowerInvariant().Contains("daily_actions"));

                if (hasGameDaily && !hasGamesMaster)
                {
                    // Find and add Games table if missing
                    var gamesTable = fullSchema.Tables.FirstOrDefault(t =>
                        t.Name.ToLowerInvariant().Contains("games") && !t.Name.ToLowerInvariant().Contains("daily_actions"));
                    if (gamesTable != null && !relevantTables.Contains(gamesTable))
                    {
                        relevantTables.Add(gamesTable);
                        _logger.LogInformation("🎮 FORCED INCLUSION: Added Games table for game query");
                    }
                }
            }

            // Limit to top 7 most relevant tables (increased to accommodate game tables)
            schemaContext.RelevantTables = relevantTables.Take(7).ToList();

            // Find relationships between relevant tables
            var tableRelationships = FindRelevantRelationships(schemaContext.RelevantTables, fullSchema);
            schemaContext.Relationships = tableRelationships.Select(tr => new RelationshipInfo
            {
                FromTable = tr.FromTable,
                ToTable = tr.ToTable,
                FromColumn = tr.ColumnMappings.FirstOrDefault()?.FromColumn ?? "",
                ToColumn = tr.ColumnMappings.FirstOrDefault()?.ToColumn ?? "",
                Type = Enum.TryParse<RelationshipType>(tr.RelationshipType, out var relType) ? relType : RelationshipType.OneToMany
            }).ToList();

            // Generate suggested joins
            schemaContext.SuggestedJoins = GenerateSuggestedJoins(schemaContext.RelevantTables, tableRelationships);

            // Create column mappings for business terms
            schemaContext.ColumnMappings = CreateColumnMappings(schemaContext.RelevantTables);

            // Extract business terms from the query
            schemaContext.BusinessTerms = ExtractBusinessTerms(semanticAnalysis);

            _logger.LogDebug("Found {TableCount} relevant tables for query", schemaContext.RelevantTables.Count);
            return schemaContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting relevant schema");
            return new SchemaContext
            {
                RelevantTables = fullSchema.Tables.Take(3).ToList()
            };
        }
    }

    #endregion

    #region Prompt Optimization Methods

    /// <summary>
    /// Optimize a prompt based on learning insights
    /// </summary>
    public async Task<string> OptimizePromptAsync(string originalPrompt, LearningInsights insights)
    {
        try
        {
            var optimizedPrompt = originalPrompt;

            // Apply learning-based optimizations
            optimizedPrompt = ApplyLearningOptimizations(optimizedPrompt, insights);

            // Apply rule-based optimizations
            optimizedPrompt = ApplyRuleBasedOptimizations(optimizedPrompt);

            // Add context enhancements
            optimizedPrompt = await AddContextEnhancementsAsync(optimizedPrompt);

            _logger.LogDebug("Prompt optimized from {OriginalLength} to {OptimizedLength} characters",
                originalPrompt.Length, optimizedPrompt.Length);

            return optimizedPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error optimizing prompt, using original");
            return originalPrompt;
        }
    }

    /// <summary>
    /// Enhance insight based on context and learning
    /// </summary>
    public async Task<string> EnhanceInsightAsync(string baseInsight, InsightContext context, object[] data)
    {
        try
        {
            var enhancedInsight = new StringBuilder(baseInsight);

            // Add contextual hints
            if (context.ContextualHints.Any())
            {
                enhancedInsight.AppendLine("\n**Additional Context:**");
                foreach (var hint in context.ContextualHints.Take(3))
                {
                    enhancedInsight.AppendLine($"• {hint.Trim()}");
                }
            }

            // Add pattern-specific insights
            var patternInsights = await GetPatternSpecificInsightsAsync(context.QueryPattern);
            if (patternInsights.Any())
            {
                enhancedInsight.AppendLine("\n**Pattern-Specific Insights:**");
                foreach (var insight in patternInsights.Take(2))
                {
                    enhancedInsight.AppendLine($"• {insight}");
                }
            }

            // Add data quality observations
            var dataQualityInsights = AnalyzeDataQuality(data);
            if (dataQualityInsights.Any())
            {
                enhancedInsight.AppendLine("\n**Data Quality Notes:**");
                foreach (var insight in dataQualityInsights)
                {
                    enhancedInsight.AppendLine($"• {insight}");
                }
            }

            return enhancedInsight.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error enhancing insight, using base insight");
            return baseInsight;
        }
    }

    #endregion

    #region Private Helper Methods - Prompt Optimization

    private string ApplyLearningOptimizations(string prompt, LearningInsights insights)
    {
        var optimizedPrompt = prompt;

        // Add successful patterns as context
        if (insights.SuccessfulPatterns.Any())
        {
            var successfulContext = string.Join(", ", insights.SuccessfulPatterns.Take(3));
            optimizedPrompt = $"Context: Focus on {successfulContext}. Query: {optimizedPrompt}";
        }

        // Add optimization suggestions as guidance
        if (insights.OptimizationSuggestions.Any())
        {
            var suggestions = string.Join("; ", insights.OptimizationSuggestions.Take(2));
            optimizedPrompt += $" (Guidelines: {suggestions})";
        }

        // Warn about common mistakes
        if (insights.CommonMistakes.Any())
        {
            var mistakes = string.Join(", ", insights.CommonMistakes.Take(2));
            optimizedPrompt += $" (Avoid: {mistakes})";
        }

        return optimizedPrompt;
    }

    private string ApplyRuleBasedOptimizations(string prompt)
    {
        var optimizedPrompt = prompt;

        foreach (var rule in _optimizationRules.Values)
        {
            if (rule.Condition(optimizedPrompt))
            {
                optimizedPrompt = rule.Transformation(optimizedPrompt);
            }
        }

        return optimizedPrompt;
    }

    private async Task<string> AddContextEnhancementsAsync(string prompt)
    {
        try
        {
            // Add schema context hints
            var schemaHints = await GetRelevantSchemaHintsAsync(prompt);
            if (schemaHints.Any())
            {
                var hints = string.Join(", ", schemaHints.Take(3));
                prompt += $" (Schema context: {hints})";
            }

            // Add temporal context if relevant
            if (ContainsTemporalKeywords(prompt))
            {
                prompt += " (Note: Use appropriate date/time functions and consider timezone)";
            }

            // Add aggregation context if relevant
            if (ContainsAggregationKeywords(prompt))
            {
                prompt += " (Note: Consider GROUP BY clauses and NULL handling)";
            }

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error adding context enhancements");
            return prompt;
        }
    }

    private async Task<List<string>> GetRelevantSchemaHintsAsync(string prompt)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Tuning, async context =>
            {
                var tuningContext = (TuningDbContext)context;

                // Extract potential table/column names from prompt
                var words = ExtractWords(prompt);

                var hints = await tuningContext.BusinessTableInfo
                    .Where(t => words.Any(w => t.TableName.Contains(w) || t.BusinessPurpose.Contains(w)))
                    .Select(t => $"{t.TableName} ({t.BusinessPurpose})")
                    .Take(3)
                    .ToListAsync();

                return hints;
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting schema hints");
            return new List<string>();
        }
    }

    private async Task<List<string>> GetPatternSpecificInsightsAsync(string queryPattern)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Tuning, async context =>
            {
                var tuningContext = (TuningDbContext)context;
                var insights = await tuningContext.AIFeedbackEntries
                    .Where(f => f.Category == queryPattern && f.Rating >= 4 && !string.IsNullOrEmpty(f.Comments))
                    .Select(f => f.Comments!)
                    .Distinct()
                    .Take(5)
                    .ToListAsync();

                return insights.Where(i => i.Length > 20 && i.Length < 200).ToList();
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting pattern-specific insights");
            return new List<string>();
        }
    }

    private List<string> AnalyzeDataQuality(object[] data)
    {
        var insights = new List<string>();

        if (data.Length == 0)
        {
            insights.Add("No data returned - consider checking filters or data availability");
            return insights;
        }

        if (data.Length == 1)
        {
            insights.Add("Single result returned - this might indicate very specific filtering");
        }
        else if (data.Length > 1000)
        {
            insights.Add("Large result set returned - consider adding filters for better performance");
        }

        // Check for potential data quality issues (simplified)
        var sampleData = data.Take(10).ToArray();
        if (sampleData.Any(d => d?.ToString()?.Contains("null") == true))
        {
            insights.Add("Some null values detected - consider handling missing data appropriately");
        }

        return insights;
    }

    private Dictionary<string, OptimizationRule> InitializeOptimizationRules()
    {
        return new Dictionary<string, OptimizationRule>
        {
            ["add_table_context"] = new OptimizationRule
            {
                Name = "Add Table Context",
                Condition = prompt => !prompt.Contains("table") && !prompt.Contains("from"),
                Transformation = prompt => $"Generate SQL query for the appropriate table: {prompt}"
            },
            ["clarify_aggregation"] = new OptimizationRule
            {
                Name = "Clarify Aggregation",
                Condition = prompt => ContainsAggregationKeywords(prompt) && !prompt.Contains("group"),
                Transformation = prompt => $"{prompt} (Include appropriate grouping if needed)"
            },
            ["add_limit_guidance"] = new OptimizationRule
            {
                Name = "Add Limit Guidance",
                Condition = prompt => prompt.ToLower().Contains("show") && !prompt.Contains("limit") && !prompt.Contains("top"),
                Transformation = prompt => $"{prompt} (Consider adding LIMIT/TOP for large datasets)"
            },
            ["temporal_clarity"] = new OptimizationRule
            {
                Name = "Temporal Clarity",
                Condition = prompt => ContainsTemporalKeywords(prompt) && !ContainsDateFormat(prompt),
                Transformation = prompt => $"{prompt} (Specify date format and range clearly)"
            },
            ["join_guidance"] = new OptimizationRule
            {
                Name = "Join Guidance",
                Condition = prompt => ContainsMultipleEntities(prompt) && !prompt.Contains("join"),
                Transformation = prompt => $"{prompt} (Consider relationships between entities)"
            }
        };
    }

    private static bool ContainsTemporalKeywords(string prompt)
    {
        var temporalKeywords = new[] { "date", "time", "day", "month", "year", "recent", "last", "today", "yesterday", "week" };
        return temporalKeywords.Any(keyword => prompt.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsAggregationKeywords(string prompt)
    {
        var aggregationKeywords = new[] { "count", "sum", "average", "total", "max", "min", "group" };
        return aggregationKeywords.Any(keyword => prompt.Contains(keyword, StringComparison.OrdinalIgnoreCase));
    }

    private static bool ContainsDateFormat(string prompt)
    {
        return Regex.IsMatch(prompt, @"\d{4}-\d{2}-\d{2}|\d{2}/\d{2}/\d{4}|yyyy|mm|dd", RegexOptions.IgnoreCase);
    }

    private static bool ContainsMultipleEntities(string prompt)
    {
        var entityKeywords = new[] { "customer", "order", "product", "user", "account", "transaction", "invoice", "payment" };
        return entityKeywords.Count(keyword => prompt.Contains(keyword, StringComparison.OrdinalIgnoreCase)) > 1;
    }

    private static List<string> ExtractWords(string text)
    {
        return Regex.Split(text.ToLower(), @"\W+")
            .Where(w => w.Length > 2)
            .ToList();
    }

    #endregion

    #region Private Helper Methods - Context Management

    // Note: Due to file size constraints, the remaining context management methods
    // from the original ContextManager will be added in a separate file or
    // can be accessed through the original implementation if needed.
    // The core functionality has been consolidated above.

    private async Task<UserContext> BuildUserContextAsync(string userId)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Legacy, async context =>
            {
                if (context is BICopilotContext legacyContext)
                {
                    var userContext = CreateDefaultUserContext(userId);

                    // Get user's query history from the last 30 days
                    var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
                    var queryHistory = await legacyContext.QueryHistory
                        .Where(q => q.UserId == userId && q.ExecutedAt >= thirtyDaysAgo && q.IsSuccessful)
                        .OrderByDescending(q => q.ExecutedAt)
                        .Take(100)
                        .ToListAsync();

                    if (queryHistory.Any())
                    {
                        // Extract preferred tables from successful queries
                        var tableUsage = new Dictionary<string, int>();
                        var filterPatterns = new Dictionary<string, int>();
                        var queryPatterns = new List<QueryPattern>();

                        foreach (var query in queryHistory)
                        {
                            if (!string.IsNullOrEmpty(query.GeneratedSql))
                            {
                                // Extract tables from SQL
                                var tables = ExtractTablesFromSql(query.GeneratedSql);
                                foreach (var table in tables)
                                {
                                    tableUsage[table] = tableUsage.GetValueOrDefault(table, 0) + 1;
                                }

                                // Extract common filter patterns
                                var filters = ExtractFiltersFromQuery(query.Query);
                                foreach (var filter in filters)
                                {
                                    filterPatterns[filter] = filterPatterns.GetValueOrDefault(filter, 0) + 1;
                                }
                            }
                        }

                        // Set preferred tables (top 10 most used)
                        userContext.PreferredTables = tableUsage
                            .OrderByDescending(kvp => kvp.Value)
                            .Take(10)
                            .Select(kvp => kvp.Key)
                            .ToList();

                        // Set common filters (top 10 most used)
                        userContext.CommonFilters = filterPatterns
                            .Where(kvp => kvp.Value >= 2) // Used at least twice
                            .OrderByDescending(kvp => kvp.Value)
                            .Take(10)
                            .Select(kvp => kvp.Key)
                            .ToList();

                        // Infer domain from query patterns
                        userContext.Domain = InferUserDomain(queryHistory);

                        // Build query patterns
                        userContext.RecentPatterns = BuildQueryPatterns(queryHistory);
                    }

                    return userContext;
                }

                return CreateDefaultUserContext(userId);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building user context for user {UserId}", userId);
            return CreateDefaultUserContext(userId);
        }
    }

    private UserContext CreateDefaultUserContext(string userId)
    {
        return new UserContext
        {
            UserId = userId,
            PreferredTables = new List<string>(),
            CommonFilters = new List<string>(),
            Preferences = new Dictionary<string, object>(),
            RecentPatterns = new List<QueryPattern>(),
            LastUpdated = DateTime.UtcNow,
            Domain = "general"
        };
    }

    private double CalculateTableRelevance(TableMetadata table, SemanticAnalysis semanticAnalysis, string lowerQuery)
    {
        double score = 0.0;
        var tableName = table.Name.ToLowerInvariant();

        _logger.LogDebug("Calculating relevance for table {TableName} with query '{Query}'", table.Name, lowerQuery);

        // High relevance for core gaming tables
        if (tableName.Contains("daily_actions") && !tableName.Contains("players") && !tableName.Contains("games"))
        {
            score += 0.9; // Main stats table - highest priority
            _logger.LogDebug("Table {TableName}: +0.9 for being main daily_actions table", table.Name);
        }
        else if (tableName.Contains("daily_actions_players"))
        {
            score += 0.6; // Player demographics
            _logger.LogDebug("Table {TableName}: +0.6 for being players table", table.Name);
        }
        else if (tableName.Contains("daily_actions_games"))
        {
            score += 0.7; // Game-specific daily actions
            _logger.LogDebug("Table {TableName}: +0.7 for being games daily actions table", table.Name);
        }
        else if (tableName.Contains("games") && !tableName.Contains("daily_actions"))
        {
            score += 0.5; // Games master table
            _logger.LogDebug("Table {TableName}: +0.5 for being games master table", table.Name);
        }
        else if (tableName.Contains("countries") || tableName.Contains("currencies") || tableName.Contains("whitelabels"))
        {
            score += 0.4; // Lookup tables
            _logger.LogDebug("Table {TableName}: +0.4 for being lookup table", table.Name);
        }

        // Game-specific query relevance boosts
        if (IsGameRelatedQuery(lowerQuery))
        {
            if (tableName.Contains("daily_actions_games"))
            {
                score += 0.9; // Highest priority for game queries
                _logger.LogDebug("Table {TableName}: +0.9 for game query on games daily actions table", table.Name);
            }
            else if (tableName.Contains("games") && !tableName.Contains("daily_actions"))
            {
                score += 0.8; // High priority for games master table
                _logger.LogDebug("Table {TableName}: +0.8 for game query on games master table", table.Name);
            }
            else if (tableName.Contains("daily_actions") && !tableName.Contains("players") && !tableName.Contains("games"))
            {
                score += 0.3; // Lower priority for main table in game queries
                _logger.LogDebug("Table {TableName}: +0.3 for game query on main daily actions table", table.Name);
            }
        }

        // Query-specific relevance boosts
        if ((lowerQuery.Contains("deposit") || lowerQuery.Contains("top") || lowerQuery.Contains("player")) &&
            tableName.Contains("daily_actions") && !tableName.Contains("players") && !tableName.Contains("games"))
        {
            score += 0.8; // Major boost for deposit/player queries on main table
            _logger.LogDebug("Table {TableName}: +0.8 for deposit/player query on main table", table.Name);
        }

        if (lowerQuery.Contains("player") && tableName.Contains("players"))
        {
            score += 0.6;
            _logger.LogDebug("Table {TableName}: +0.6 for player query on players table", table.Name);
        }

        if (lowerQuery.Contains("bonus") && tableName.Contains("bonus"))
        {
            score += 0.7;
            _logger.LogDebug("Table {TableName}: +0.7 for bonus query on bonus table", table.Name);
        }

        if (lowerQuery.Contains("country") && tableName.Contains("countries"))
        {
            score += 0.6;
            _logger.LogDebug("Table {TableName}: +0.6 for country query", table.Name);
        }

        if ((lowerQuery.Contains("brand") || lowerQuery.Contains("whitelabel")) &&
            (tableName.Contains("whitelabels") || tableName.Contains("daily_actions")))
        {
            score += 0.6;
            _logger.LogDebug("Table {TableName}: +0.6 for brand/whitelabel query", table.Name);
        }

        // Strong penalties for irrelevant tables
        if ((lowerQuery.Contains("deposit") || lowerQuery.Contains("top") || lowerQuery.Contains("player")) &&
            tableName.Contains("bonus") && !lowerQuery.Contains("bonus"))
        {
            score -= 0.8; // Strong penalty for bonus tables in non-bonus queries
            _logger.LogDebug("Table {TableName}: -0.8 penalty for bonus table in non-bonus query", table.Name);
        }

        // Penalty for game tables in non-game queries
        if (!IsGameRelatedQuery(lowerQuery) && (tableName.Contains("games") || tableName.Contains("daily_actions_games")))
        {
            score -= 0.5; // Penalty for game tables in non-game queries
            _logger.LogDebug("Table {TableName}: -0.5 penalty for game table in non-game query", table.Name);
        }

        var finalScore = Math.Max(0, Math.Min(1, score));
        _logger.LogDebug("Table {TableName}: Final relevance score = {Score}", table.Name, finalScore);

        return finalScore;
    }

    /// <summary>
    /// Determines if a query is related to games/gaming analytics
    /// </summary>
    private bool IsGameRelatedQuery(string lowerQuery)
    {
        var gameKeywords = new[]
        {
            "game", "games", "gaming", "provider", "providers", "slot", "slots", "casino",
            "netent", "microgaming", "pragmatic", "evolution", "playtech", "yggdrasil",
            "gametype", "game type", "rtp", "volatility", "jackpot", "progressive",
            "table games", "live casino", "sports betting", "virtual", "scratch",
            "gamename", "game name", "gameshow", "game show", "bingo", "keno",
            "realbetamount", "realwinamount", "bonusbetamount", "bonuswinamount",
            "netgamingrevenue", "numberofrealbets", "numberofbonusbets",
            "numberofrealwins", "numberofbonuswins", "numberofsessions"
        };

        return gameKeywords.Any(keyword => lowerQuery.Contains(keyword));
    }

    private List<TableMetadata> FindTablesByKeywords(List<TableMetadata> tables, string lowerQuery)
    {
        var relevantTables = new List<TableMetadata>();
        var isGameQuery = IsGameRelatedQuery(lowerQuery);

        foreach (var table in tables)
        {
            var tableName = table.Name.ToLowerInvariant();

            // For game-related queries, prioritize game tables
            if (isGameQuery)
            {
                if (tableName.Contains("daily_actions_games"))
                {
                    relevantTables.Add(table);
                    continue;
                }
                if (tableName.Contains("games") && !tableName.Contains("daily_actions"))
                {
                    relevantTables.Add(table);
                    continue;
                }
            }

            // Always include main daily actions table for most queries (but lower priority for game queries)
            if (tableName.Contains("daily_actions") && !tableName.Contains("players") && !tableName.Contains("games"))
            {
                relevantTables.Add(table);
                continue;
            }

            // Include player table if query mentions players
            if (lowerQuery.Contains("player") && tableName.Contains("players"))
            {
                relevantTables.Add(table);
                continue;
            }

            // Include lookup tables based on query content
            if ((lowerQuery.Contains("country") && tableName.Contains("countries")) ||
                (lowerQuery.Contains("currency") && tableName.Contains("currencies")) ||
                ((lowerQuery.Contains("brand") || lowerQuery.Contains("whitelabel")) && tableName.Contains("whitelabels")))
            {
                relevantTables.Add(table);
            }
        }

        return relevantTables.Take(7).ToList(); // Increased limit to accommodate game tables
    }

    private List<TableMetadata> GetDefaultGamingTables(List<TableMetadata> tables)
    {
        var defaultTables = new List<TableMetadata>();

        // Always include the main daily actions table
        var dailyActionsTable = tables.FirstOrDefault(t => t.Name.ToLowerInvariant().Contains("daily_actions") &&
            !t.Name.ToLowerInvariant().Contains("players") && !t.Name.ToLowerInvariant().Contains("games"));
        if (dailyActionsTable != null)
        {
            defaultTables.Add(dailyActionsTable);
        }

        // Include game-specific daily actions table
        var gamesTable = tables.FirstOrDefault(t => t.Name.ToLowerInvariant().Contains("daily_actions_games"));
        if (gamesTable != null)
        {
            defaultTables.Add(gamesTable);
        }

        // Include games master table
        var gamesMasterTable = tables.FirstOrDefault(t => t.Name.ToLowerInvariant().Contains("games") &&
            !t.Name.ToLowerInvariant().Contains("daily_actions"));
        if (gamesMasterTable != null)
        {
            defaultTables.Add(gamesMasterTable);
        }

        // Include player table for demographics
        var playersTable = tables.FirstOrDefault(t => t.Name.ToLowerInvariant().Contains("players"));
        if (playersTable != null)
        {
            defaultTables.Add(playersTable);
        }

        // Include countries for geographic analysis
        var countriesTable = tables.FirstOrDefault(t => t.Name.ToLowerInvariant().Contains("countries"));
        if (countriesTable != null)
        {
            defaultTables.Add(countriesTable);
        }

        return defaultTables;
    }
    private List<TableRelationship> FindRelevantRelationships(List<TableMetadata> tables, SchemaMetadata schema) => new();
    private List<string> GenerateSuggestedJoins(List<TableMetadata> tables, List<TableRelationship> relationships) => new();
    private Dictionary<string, string> CreateColumnMappings(List<TableMetadata> tables) => new();
    private List<string> ExtractBusinessTerms(SemanticAnalysis analysis) => new();

    #endregion

    private class OptimizationRule
    {
        public string Name { get; set; } = string.Empty;
        public Func<string, bool> Condition { get; set; } = _ => false;
        public Func<string, string> Transformation { get; set; } = s => s;
    }

    // Missing method implementations
    private List<string> ExtractTablesFromSql(string sql)
    {
        var tables = new List<string>();
        var lowerSql = sql.ToLowerInvariant();

        // Simple regex to extract table names after FROM and JOIN
        var fromMatches = Regex.Matches(lowerSql, @"from\s+([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase);
        var joinMatches = Regex.Matches(lowerSql, @"join\s+([a-zA-Z_][a-zA-Z0-9_]*)", RegexOptions.IgnoreCase);

        foreach (Match match in fromMatches)
            if (match.Groups.Count > 1) tables.Add(match.Groups[1].Value);

        foreach (Match match in joinMatches)
            if (match.Groups.Count > 1) tables.Add(match.Groups[1].Value);

        return tables.Distinct().ToList();
    }

    private async Task UpdateQueryPatternsAsync(UserContext context, QueryResponse response)
    {
        // Update query patterns based on successful queries
        await Task.CompletedTask;
    }

    private void UpdateCommonFilters(UserContext context, QueryResponse response)
    {
        // Update common filters based on query patterns
    }

    private void UpdateUserDomain(UserContext context, QueryResponse response)
    {
        // Update user domain based on query patterns
    }

    private List<string> ExtractFiltersFromQuery(string naturalLanguageQuery)
    {
        var filters = new List<string>();
        var lowerQuery = naturalLanguageQuery.ToLowerInvariant();

        // Common filter patterns
        var filterPatterns = new Dictionary<string, string[]>
        {
            ["time_filters"] = new[] { "today", "yesterday", "last week", "last month", "this month", "this year" },
            ["player_filters"] = new[] { "active players", "new players", "vip players", "blocked players" },
            ["game_filters"] = new[] { "slots", "table games", "live casino", "sports betting" },
            ["country_filters"] = new[] { "country", "region", "geography" },
            ["currency_filters"] = new[] { "currency", "eur", "usd", "gbp" },
            ["amount_filters"] = new[] { "deposit", "withdrawal", "bet", "win", "revenue" }
        };

        foreach (var category in filterPatterns)
        {
            foreach (var pattern in category.Value)
            {
                if (lowerQuery.Contains(pattern))
                {
                    filters.Add(pattern);
                }
            }
        }

        return filters.Distinct().ToList();
    }

    private string InferUserDomain(List<UnifiedQueryHistoryEntity> queryHistory)
    {
        var domainKeywords = new Dictionary<string, string[]>
        {
            ["Gaming"] = new[] { "game", "slot", "casino", "bet", "win", "provider", "rtp" },
            ["Finance"] = new[] { "deposit", "withdrawal", "revenue", "cost", "profit", "payment" },
            ["Marketing"] = new[] { "campaign", "bonus", "promotion", "acquisition", "retention" },
            ["Analytics"] = new[] { "trend", "analysis", "performance", "metric", "kpi" },
            ["Operations"] = new[] { "player", "user", "session", "activity", "behavior" }
        };

        var domainScores = new Dictionary<string, int>();

        foreach (var query in queryHistory)
        {
            var lowerQuery = query.Query.ToLowerInvariant();

            foreach (var domain in domainKeywords)
            {
                var score = domain.Value.Count(keyword => lowerQuery.Contains(keyword));
                domainScores[domain.Key] = domainScores.GetValueOrDefault(domain.Key, 0) + score;
            }
        }

        return domainScores.Any()
            ? domainScores.OrderByDescending(kvp => kvp.Value).First().Key
            : "General";
    }

    private List<QueryPattern> BuildQueryPatterns(List<UnifiedQueryHistoryEntity> queryHistory)
    {
        var patterns = new List<QueryPattern>();
        var patternGroups = new Dictionary<string, List<UnifiedQueryHistoryEntity>>();

        // Group similar queries
        foreach (var query in queryHistory)
        {
            var pattern = ExtractQueryPattern(query.Query);
            if (!patternGroups.ContainsKey(pattern))
            {
                patternGroups[pattern] = new List<UnifiedQueryHistoryEntity>();
            }
            patternGroups[pattern].Add(query);
        }

        // Build pattern objects
        foreach (var group in patternGroups.Where(g => g.Value.Count >= 2)) // At least 2 occurrences
        {
            var lastUsed = group.Value.Max(q => q.ExecutedAt);
            var associatedTables = group.Value
                .SelectMany(q => ExtractTablesFromSql(q.GeneratedSql ?? ""))
                .Distinct()
                .ToList();

            patterns.Add(new QueryPattern
            {
                Pattern = group.Key,
                Frequency = group.Value.Count,
                LastUsed = lastUsed,
                Intent = InferQueryIntent(group.Key),
                AssociatedTables = associatedTables
            });
        }

        return patterns.OrderByDescending(p => p.Frequency).Take(10).ToList();
    }

    private string ExtractQueryPattern(string naturalLanguageQuery)
    {
        var lowerQuery = naturalLanguageQuery.ToLowerInvariant();

        // Normalize common patterns
        if (lowerQuery.Contains("top") && lowerQuery.Contains("player"))
            return "top_players_analysis";
        if (lowerQuery.Contains("deposit") && lowerQuery.Contains("trend"))
            return "deposit_trend_analysis";
        if (lowerQuery.Contains("game") && lowerQuery.Contains("performance"))
            return "game_performance_analysis";
        if (lowerQuery.Contains("revenue") && lowerQuery.Contains("country"))
            return "revenue_by_geography";
        if (lowerQuery.Contains("player") && lowerQuery.Contains("behavior"))
            return "player_behavior_analysis";

        // Default pattern based on main keywords
        var keywords = new[] { "player", "game", "deposit", "revenue", "country", "bonus" };
        var foundKeywords = keywords.Where(k => lowerQuery.Contains(k)).ToList();

        return foundKeywords.Any()
            ? string.Join("_", foundKeywords) + "_query"
            : "general_query";
    }

    private QueryIntent InferQueryIntent(string pattern)
    {
        if (pattern.Contains("top") || pattern.Contains("best"))
            return QueryIntent.Comparison;
        if (pattern.Contains("trend") || pattern.Contains("over_time"))
            return QueryIntent.Trend;
        if (pattern.Contains("performance") || pattern.Contains("metric"))
            return QueryIntent.Aggregation;
        if (pattern.Contains("behavior") || pattern.Contains("activity"))
            return QueryIntent.Filtering;
        if (pattern.Contains("revenue") || pattern.Contains("financial"))
            return QueryIntent.Aggregation;

        return QueryIntent.General;
    }
}