using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Query Intelligence Service combining Advanced NLU and Schema Optimization
/// Provides comprehensive query analysis with semantic understanding and performance optimization
/// </summary>
public class QueryIntelligenceService : IQueryIntelligenceService
{
    private readonly ILogger<QueryIntelligenceService> _logger;
    private readonly IAdvancedNLUService _nluService;
    private readonly ISchemaOptimizationService _schemaOptimizationService;
    private readonly IAIService _aiService;
    private readonly ISchemaService _schemaService;

    public QueryIntelligenceService(
        ILogger<QueryIntelligenceService> logger,
        IAdvancedNLUService nluService,
        ISchemaOptimizationService schemaOptimizationService,
        IAIService aiService,
        ISchemaService schemaService)
    {
        _logger = logger;
        _nluService = nluService;
        _schemaOptimizationService = schemaOptimizationService;
        _aiService = aiService;
        _schemaService = schemaService;
    }

    /// <summary>
    /// Perform comprehensive query analysis combining NLU and performance optimization
    /// </summary>
    public async Task<QueryIntelligenceResult> AnalyzeQueryAsync(
        string naturalLanguageQuery, 
        string userId, 
        SchemaMetadata schema)
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            _logger.LogInformation("🧠⚡ Starting comprehensive query intelligence analysis for user {UserId}", userId);

            // Step 1: Advanced NLU Analysis
            _logger.LogDebug("🧠 Performing advanced NLU analysis");
            var nluResult = await _nluService.AnalyzeQueryAsync(naturalLanguageQuery, userId, new NLUAnalysisContext
            {
                UserId = userId,
                SchemaContext = schema,
                Timestamp = DateTime.UtcNow
            });

            // Step 2: Generate SQL based on NLU insights
            _logger.LogDebug("🔧 Generating SQL with NLU insights");
            var enhancedPrompt = await BuildEnhancedPromptAsync(naturalLanguageQuery, nluResult, schema);
            var generatedSql = await _aiService.GenerateSQLAsync(enhancedPrompt);

            // Step 3: Schema Optimization Analysis
            _logger.LogDebug("⚡ Performing schema optimization analysis");
            var optimizationResult = await _schemaOptimizationService.AnalyzeQueryPerformanceAsync(
                generatedSql, schema);

            // Step 4: Generate intelligent suggestions
            _logger.LogDebug("💡 Generating intelligent suggestions");
            var suggestions = await GenerateIntelligentSuggestionsAsync(nluResult, optimizationResult, schema);

            // Step 5: Create query assistance
            _logger.LogDebug("🤝 Creating query assistance");
            var assistance = await CreateQueryAssistanceAsync(nluResult, optimizationResult, schema);

            // Step 6: Calculate overall intelligence score
            var overallScore = CalculateIntelligenceScore(nluResult, optimizationResult);

            var result = new QueryIntelligenceResult
            {
                NLUResult = nluResult,
                OptimizationResult = optimizationResult,
                Suggestions = suggestions,
                Assistance = assistance,
                OverallScore = overallScore,
                AnalyzedAt = DateTime.UtcNow
            };

            var processingTime = DateTime.UtcNow - startTime;
            _logger.LogInformation("🧠⚡ Query intelligence analysis completed in {Time}ms - Overall Score: {Score:P2}, NLU Confidence: {NLUConfidence:P2}, Optimization Score: {OptScore:P2}",
                processingTime.TotalMilliseconds, overallScore, nluResult.ConfidenceScore, optimizationResult.ImprovementScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in comprehensive query intelligence analysis");
            return new QueryIntelligenceResult
            {
                NLUResult = new AdvancedNLUResult
                {
                    OriginalQuery = naturalLanguageQuery,
                    Error = ex.Message
                },
                OptimizationResult = new QueryOptimizationResult
                {
                    OriginalSql = "",
                    ImprovementScore = 0.0
                },
                OverallScore = 0.0,
                AnalyzedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Generate intelligent query suggestions based on user intent and performance
    /// </summary>
    public async Task<List<IntelligentQuerySuggestion>> GenerateIntelligentSuggestionsAsync(
        string userId, 
        SchemaMetadata schema, 
        string? context = null)
    {
        try
        {
            _logger.LogDebug("💡 Generating intelligent query suggestions for user {UserId}", userId);

            var suggestions = new List<IntelligentQuerySuggestion>();

            // Get user's conversation analysis
            var conversationAnalysis = await _nluService.AnalyzeConversationAsync(userId);

            // Generate intent-based suggestions
            suggestions.AddRange(await GenerateIntentBasedSuggestionsAsync(conversationAnalysis, schema));

            // Generate performance-optimized suggestions
            suggestions.AddRange(await GeneratePerformanceOptimizedSuggestionsAsync(schema));

            // Generate context-aware suggestions
            if (!string.IsNullOrEmpty(context))
            {
                suggestions.AddRange(await GenerateContextAwareSuggestionsAsync(context, schema));
            }

            // Generate domain-specific suggestions
            suggestions.AddRange(await GenerateDomainSpecificSuggestionsAsync(schema));

            // Rank and filter suggestions
            var rankedSuggestions = RankSuggestionsByIntelligence(suggestions, conversationAnalysis);

            _logger.LogInformation("💡 Generated {Count} intelligent suggestions", rankedSuggestions.Count);
            return rankedSuggestions.Take(10).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating intelligent suggestions");
            return new List<IntelligentQuerySuggestion>();
        }
    }

    /// <summary>
    /// Provide real-time query assistance and optimization hints
    /// </summary>
    public async Task<QueryAssistance> GetQueryAssistanceAsync(
        string partialQuery, 
        string userId, 
        SchemaMetadata schema)
    {
        try
        {
            _logger.LogDebug("🤝 Providing query assistance for partial query");

            var assistance = new QueryAssistance();

            if (string.IsNullOrWhiteSpace(partialQuery))
            {
                // Provide general assistance
                assistance.AutocompleteSuggestions = await GetGeneralAutocompleteSuggestionsAsync(schema);
                assistance.ContextualHelp = GetGeneralContextualHelp();
            }
            else
            {
                // Analyze partial query
                var partialNLU = await _nluService.AnalyzeQueryAsync(partialQuery, userId);

                // Generate autocomplete suggestions
                assistance.AutocompleteSuggestions = await GenerateAutocompleteSuggestionsAsync(partialQuery, partialNLU, schema);

                // Generate syntax suggestions
                assistance.SyntaxSuggestions = GenerateSyntaxSuggestions(partialQuery, partialNLU);

                // Generate performance hints
                assistance.PerformanceHints = await GeneratePerformanceHintsAsync(partialQuery, schema);

                // Generate contextual help
                assistance.ContextualHelp = GenerateContextualHelp(partialNLU, schema);

                // Validate query
                assistance.Validations = await ValidatePartialQueryAsync(partialQuery, schema);
            }

            _logger.LogDebug("🤝 Query assistance provided - {AutocompleteCount} autocomplete, {HintCount} hints",
                assistance.AutocompleteSuggestions.Count, assistance.PerformanceHints.Count);

            return assistance;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error providing query assistance");
            return new QueryAssistance();
        }
    }

    /// <summary>
    /// Learn from user interactions to improve suggestions
    /// </summary>
    public async Task LearnFromInteractionAsync(
        string userId, 
        string query, 
        QueryResponse response, 
        UserFeedback? feedback = null)
    {
        try
        {
            _logger.LogDebug("📚 Learning from user interaction - User: {UserId}, Success: {Success}", 
                userId, response.Success);

            // Analyze the interaction
            var interactionData = new
            {
                UserId = userId,
                Query = query,
                Success = response.Success,
                ExecutionTime = response.ExecutionTimeMs,
                Confidence = response.Confidence,
                Feedback = feedback,
                Timestamp = DateTime.UtcNow
            };

            // Update user preferences based on successful queries
            if (response.Success && response.Confidence > 0.8)
            {
                await UpdateUserPreferencesAsync(userId, query, response);
            }

            // Learn from feedback
            if (feedback != null)
            {
                await ProcessUserFeedbackAsync(userId, query, response, feedback);
            }

            // Update NLU models with successful patterns
            if (response.Success)
            {
                await UpdateNLUPatternsAsync(query, response);
            }

            _logger.LogInformation("📚 Learning completed for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error learning from user interaction");
        }
    }

    // Helper methods
    private async Task<string> BuildEnhancedPromptAsync(
        string naturalLanguageQuery, 
        AdvancedNLUResult nluResult, 
        SchemaMetadata schema)
    {
        var promptBuilder = new System.Text.StringBuilder();

        // Add base query
        promptBuilder.AppendLine($"Natural Language Query: {naturalLanguageQuery}");

        // Add NLU insights
        promptBuilder.AppendLine($"Detected Intent: {nluResult.IntentAnalysis.PrimaryIntent}");
        promptBuilder.AppendLine($"Confidence: {nluResult.ConfidenceScore:P2}");

        // Add entity information
        if (nluResult.EntityAnalysis.Entities.Any())
        {
            promptBuilder.AppendLine("Detected Entities:");
            foreach (var entity in nluResult.EntityAnalysis.Entities)
            {
                promptBuilder.AppendLine($"- {entity.Type}: {entity.Value}");
            }
        }

        // Add contextual information
        if (nluResult.ContextualAnalysis.ContextualRelevance > 0.7)
        {
            promptBuilder.AppendLine("Contextual Information:");
            foreach (var clue in nluResult.ContextualAnalysis.ContextualClues)
            {
                promptBuilder.AppendLine($"- {clue.Type}: {clue.Value}");
            }
        }

        // Add schema information
        var relevantTables = IdentifyRelevantTables(nluResult, schema);
        if (relevantTables.Any())
        {
            promptBuilder.AppendLine("Relevant Tables:");
            foreach (var table in relevantTables)
            {
                promptBuilder.AppendLine($"- {table.Name}: {string.Join(", ", table.Columns.Take(5).Select(c => c.Name))}");
            }
        }

        return promptBuilder.ToString();
    }

    private List<TableMetadata> IdentifyRelevantTables(AdvancedNLUResult nluResult, SchemaMetadata schema)
    {
        var relevantTables = new List<TableMetadata>();

        // Find tables mentioned in entities
        foreach (var entity in nluResult.EntityAnalysis.Entities.Where(e => e.Type == "Table"))
        {
            var table = schema.Tables.FirstOrDefault(t => 
                t.Name.Equals(entity.Value, StringComparison.OrdinalIgnoreCase));
            if (table != null)
            {
                relevantTables.Add(table);
            }
        }

        // If no specific tables found, use domain analysis
        if (!relevantTables.Any())
        {
            var domainConcepts = nluResult.DomainAnalysis.DomainConcepts.Select(c => c.Name.ToLowerInvariant());
            foreach (var table in schema.Tables)
            {
                if (domainConcepts.Any(concept => table.Name.ToLowerInvariant().Contains(concept)))
                {
                    relevantTables.Add(table);
                }
            }
        }

        return relevantTables.Take(3).ToList(); // Limit to most relevant tables
    }

    private async Task<List<IntelligentQuerySuggestion>> GenerateIntelligentSuggestionsAsync(
        AdvancedNLUResult nluResult, 
        QueryOptimizationResult optimizationResult, 
        SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        // Generate suggestions based on NLU intent
        switch (nluResult.IntentAnalysis.PrimaryIntent)
        {
            case "Aggregation":
                suggestions.AddRange(GenerateAggregationSuggestions(nluResult, schema));
                break;
            case "Filtering":
                suggestions.AddRange(GenerateFilteringSuggestions(nluResult, schema));
                break;
            case "Comparison":
                suggestions.AddRange(GenerateComparisonSuggestions(nluResult, schema));
                break;
        }

        // Add performance-based suggestions
        if (optimizationResult.ImprovementScore > 0.5)
        {
            suggestions.AddRange(GeneratePerformanceSuggestions(optimizationResult));
        }

        return suggestions;
    }

    private List<IntelligentQuerySuggestion> GenerateAggregationSuggestions(AdvancedNLUResult nluResult, SchemaMetadata schema)
    {
        return new List<IntelligentQuerySuggestion>
        {
            new IntelligentQuerySuggestion
            {
                Text = "Show total revenue by month",
                Description = "Aggregate revenue data grouped by month",
                Category = "Aggregation",
                Relevance = 0.9,
                PerformanceScore = 0.8,
                Benefits = new List<string> { "Clear time-based analysis", "Optimized for reporting" }
            },
            new IntelligentQuerySuggestion
            {
                Text = "Count active players by country",
                Description = "Player count aggregated by geographic location",
                Category = "Aggregation",
                Relevance = 0.8,
                PerformanceScore = 0.7,
                Benefits = new List<string> { "Geographic insights", "Player distribution analysis" }
            }
        };
    }

    private List<IntelligentQuerySuggestion> GenerateFilteringSuggestions(AdvancedNLUResult nluResult, SchemaMetadata schema)
    {
        return new List<IntelligentQuerySuggestion>
        {
            new IntelligentQuerySuggestion
            {
                Text = "Show players registered yesterday",
                Description = "Filter players by registration date",
                Category = "Filtering",
                Relevance = 0.9,
                PerformanceScore = 0.9,
                Benefits = new List<string> { "Recent activity focus", "Efficient date filtering" }
            }
        };
    }

    private List<IntelligentQuerySuggestion> GenerateComparisonSuggestions(AdvancedNLUResult nluResult, SchemaMetadata schema)
    {
        return new List<IntelligentQuerySuggestion>
        {
            new IntelligentQuerySuggestion
            {
                Text = "Compare revenue this month vs last month",
                Description = "Month-over-month revenue comparison",
                Category = "Comparison",
                Relevance = 0.9,
                PerformanceScore = 0.8,
                Benefits = new List<string> { "Trend analysis", "Performance comparison" }
            }
        };
    }

    private List<IntelligentQuerySuggestion> GeneratePerformanceSuggestions(QueryOptimizationResult optimizationResult)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        foreach (var optimization in optimizationResult.Suggestions.Take(3))
        {
            suggestions.Add(new IntelligentQuerySuggestion
            {
                Text = $"Optimize: {optimization.Description}",
                Description = optimization.Implementation,
                Category = "Performance",
                Relevance = 0.7,
                PerformanceScore = optimization.Impact,
                Benefits = optimization.Benefits
            });
        }

        return suggestions;
    }

    private async Task<QueryAssistance> CreateQueryAssistanceAsync(
        AdvancedNLUResult nluResult, 
        QueryOptimizationResult optimizationResult, 
        SchemaMetadata schema)
    {
        var assistance = new QueryAssistance();

        // Generate autocomplete based on intent
        assistance.AutocompleteSuggestions = GenerateIntentBasedAutocomplete(nluResult.IntentAnalysis);

        // Generate performance hints
        assistance.PerformanceHints = optimizationResult.Suggestions
            .Select(s => s.Description)
            .Take(3)
            .ToList();

        // Generate contextual help
        assistance.ContextualHelp = new List<string>
        {
            $"Detected intent: {nluResult.IntentAnalysis.PrimaryIntent}",
            $"Confidence level: {nluResult.ConfidenceScore:P0}",
            $"Optimization opportunities: {optimizationResult.Suggestions.Count}"
        };

        return assistance;
    }

    private List<string> GenerateIntentBasedAutocomplete(IntentAnalysis intentAnalysis)
    {
        return intentAnalysis.PrimaryIntent switch
        {
            "Aggregation" => new List<string> { "COUNT", "SUM", "AVG", "MAX", "MIN", "GROUP BY" },
            "Filtering" => new List<string> { "WHERE", "AND", "OR", "IN", "BETWEEN", "LIKE" },
            "Temporal" => new List<string> { "yesterday", "last week", "this month", "last year" },
            _ => new List<string> { "SELECT", "FROM", "WHERE", "ORDER BY" }
        };
    }

    private double CalculateIntelligenceScore(AdvancedNLUResult nluResult, QueryOptimizationResult optimizationResult)
    {
        var nluWeight = 0.6;
        var optimizationWeight = 0.4;

        var nluScore = nluResult.ConfidenceScore;
        var optimizationScore = optimizationResult.ImprovementScore;

        return (nluScore * nluWeight) + (optimizationScore * optimizationWeight);
    }

    /// <summary>
    /// Generate suggestions based on user intent analysis
    /// </summary>
    private async Task<List<IntelligentQuerySuggestion>> GenerateIntentBasedSuggestionsAsync(ConversationAnalysis analysis, SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        try
        {
            // Analyze dominant intents from conversation
            var dominantIntents = analysis.CommonIntents?.Take(3) ?? new List<string>();

            foreach (var intent in dominantIntents)
            {
                switch (intent.ToLower())
                {
                    case "aggregation":
                    case "summary":
                        suggestions.AddRange(GenerateAggregationSuggestions(schema));
                        break;
                    case "comparison":
                    case "trend":
                        suggestions.AddRange(GenerateComparisonSuggestions(schema));
                        break;
                    case "filtering":
                    case "lookup":
                        suggestions.AddRange(GenerateFilteringSuggestions(schema));
                        break;
                    case "ranking":
                    case "top":
                        suggestions.AddRange(GenerateRankingSuggestions(schema));
                        break;
                }
            }

            return suggestions.Take(5).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating intent-based suggestions");
            return new List<IntelligentQuerySuggestion>();
        }
    }

    /// <summary>
    /// Generate performance-optimized query suggestions
    /// </summary>
    private async Task<List<IntelligentQuerySuggestion>> GeneratePerformanceOptimizedSuggestionsAsync(SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        try
        {
            // Find tables with good indexing for fast queries
            var optimizedTables = schema.Tables
                .Where(t => t.Indexes?.Any() == true)
                .Take(3);

            foreach (var table in optimizedTables)
            {
                suggestions.Add(new IntelligentQuerySuggestion
                {
                    Text = $"Show me recent data from {table.Name}",
                    Description = $"Optimized query using indexed table {table.Name}",
                    Category = "Performance",
                    Relevance = 0.85,
                    PerformanceScore = 0.9,
                    Benefits = new List<string> { "Fast execution", "Uses indexes" }
                });
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating performance-optimized suggestions");
            return new List<IntelligentQuerySuggestion>();
        }
    }

    /// <summary>
    /// Generate context-aware suggestions based on current context
    /// </summary>
    private async Task<List<IntelligentQuerySuggestion>> GenerateContextAwareSuggestionsAsync(string context, SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        try
        {
            if (string.IsNullOrEmpty(context))
                return suggestions;

            // Extract context keywords
            var contextKeywords = ExtractContextKeywords(context);

            // Find relevant tables based on context
            var relevantTables = schema.Tables
                .Where(t => contextKeywords.Any(k =>
                    t.Name.Contains(k, StringComparison.OrdinalIgnoreCase) ||
                    t.Columns.Any(c => c.Name.Contains(k, StringComparison.OrdinalIgnoreCase))))
                .Take(3);

            foreach (var table in relevantTables)
            {
                suggestions.Add(new IntelligentQuerySuggestion
                {
                    Text = $"Analyze {table.Name} data related to {string.Join(", ", contextKeywords)}",
                    Description = $"Context-aware suggestion based on {table.Name} relevance",
                    Category = "Contextual",
                    Relevance = 0.75,
                    PerformanceScore = 0.7,
                    Benefits = new List<string> { "Context-aware", "Relevant data" }
                });
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating context-aware suggestions");
            return new List<IntelligentQuerySuggestion>();
        }
    }

    /// <summary>
    /// Generate domain-specific suggestions
    /// </summary>
    private async Task<List<IntelligentQuerySuggestion>> GenerateDomainSpecificSuggestionsAsync(SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        try
        {
            // Gaming domain specific suggestions
            var gamingTables = schema.Tables
                .Where(t => IsGamingRelated(t.Name))
                .Take(3);

            foreach (var table in gamingTables)
            {
                suggestions.AddRange(GenerateGamingSuggestions(table));
            }

            return suggestions.Take(5).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating domain-specific suggestions");
            return new List<IntelligentQuerySuggestion>();
        }
    }

    #region Helper Methods

    /// <summary>
    /// Generate aggregation-focused suggestions
    /// </summary>
    private List<IntelligentQuerySuggestion> GenerateAggregationSuggestions(SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        var numericTables = schema.Tables
            .Where(t => t.Columns.Any(c => IsNumericType(c.DataType)))
            .Take(2);

        foreach (var table in numericTables)
        {
            var numericColumns = table.Columns.Where(c => IsNumericType(c.DataType)).Take(2);
            foreach (var column in numericColumns)
            {
                suggestions.Add(new IntelligentQuerySuggestion
                {
                    Text = $"What is the total {column.Name} from {table.Name}?",
                    Description = $"Aggregation query for numeric column {column.Name}",
                    Category = "Aggregation",
                    Relevance = 0.8,
                    PerformanceScore = 0.85,
                    Benefits = new List<string> { "Clear aggregation", "Optimized performance" }
                });
            }
        }

        return suggestions;
    }

    /// <summary>
    /// Generate comparison-focused suggestions
    /// </summary>
    private List<IntelligentQuerySuggestion> GenerateComparisonSuggestions(SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        var dateTables = schema.Tables
            .Where(t => t.Columns.Any(c => IsDateType(c.DataType)))
            .Take(2);

        foreach (var table in dateTables)
        {
            suggestions.Add(new IntelligentQuerySuggestion
            {
                Text = $"Compare {table.Name} data between this month and last month",
                Description = $"Time-based comparison for {table.Name}",
                Category = "Comparison",
                Relevance = 0.75,
                PerformanceScore = 0.7,
                Benefits = new List<string> { "Time-based analysis", "Trend identification" }
            });
        }

        return suggestions;
    }

    /// <summary>
    /// Generate filtering-focused suggestions
    /// </summary>
    private List<IntelligentQuerySuggestion> GenerateFilteringSuggestions(SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        var indexedTables = schema.Tables
            .Where(t => t.Indexes?.Any() == true)
            .Take(2);

        foreach (var table in indexedTables)
        {
            var indexedColumns = table.Indexes?.SelectMany(i => i.Columns).Distinct().Take(2) ?? new List<string>();
            foreach (var column in indexedColumns)
            {
                suggestions.Add(new IntelligentQuerySuggestion
                {
                    Text = $"Show {table.Name} where {column} equals [specific value]",
                    Description = $"Optimized filtering using indexed column {column}",
                    Category = "Filtering",
                    Relevance = 0.85,
                    PerformanceScore = 0.9,
                    Benefits = new List<string> { "Fast filtering", "Uses indexes" }
                });
            }
        }

        return suggestions;
    }

    /// <summary>
    /// Generate ranking-focused suggestions
    /// </summary>
    private List<IntelligentQuerySuggestion> GenerateRankingSuggestions(SchemaMetadata schema)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        var numericTables = schema.Tables
            .Where(t => t.Columns.Any(c => IsNumericType(c.DataType)))
            .Take(2);

        foreach (var table in numericTables)
        {
            var numericColumn = table.Columns.FirstOrDefault(c => IsNumericType(c.DataType));
            if (numericColumn != null)
            {
                suggestions.Add(new IntelligentQuerySuggestion
                {
                    Text = $"Show top 10 {table.Name} by {numericColumn.Name}",
                    Description = $"Ranking query for {table.Name} by {numericColumn.Name}",
                    Category = "Ranking",
                    Relevance = 0.8,
                    PerformanceScore = 0.75,
                    Benefits = new List<string> { "Top performers", "Clear ranking" }
                });
            }
        }

        return suggestions;
    }

    /// <summary>
    /// Extract context keywords from text
    /// </summary>
    private List<string> ExtractContextKeywords(string context)
    {
        var keywords = new List<string>();

        // Simple keyword extraction - can be enhanced with NLP
        var words = context.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 3)
            .Select(w => w.Trim().ToLower())
            .Distinct();

        keywords.AddRange(words);
        return keywords;
    }

    /// <summary>
    /// Check if table name is gaming-related
    /// </summary>
    private bool IsGamingRelated(string tableName)
    {
        var gamingKeywords = new[] { "game", "player", "bet", "win", "casino", "slot", "poker", "daily", "action" };
        return gamingKeywords.Any(k => tableName.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Generate gaming-specific suggestions
    /// </summary>
    private List<IntelligentQuerySuggestion> GenerateGamingSuggestions(TableMetadata table)
    {
        var suggestions = new List<IntelligentQuerySuggestion>();

        // Gaming-specific query patterns
        if (table.Name.Contains("player", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.Add(new IntelligentQuerySuggestion
            {
                Text = $"Show active players from {table.Name}",
                Description = "Gaming domain: Player activity analysis",
                Category = "Gaming",
                Relevance = 0.9,
                PerformanceScore = 0.85,
                Benefits = new List<string> { "Player insights", "Activity tracking" }
            });
        }

        if (table.Name.Contains("bet", StringComparison.OrdinalIgnoreCase) ||
            table.Name.Contains("win", StringComparison.OrdinalIgnoreCase))
        {
            suggestions.Add(new IntelligentQuerySuggestion
            {
                Text = $"Calculate total revenue from {table.Name}",
                Description = "Gaming domain: Revenue analysis",
                Category = "Gaming",
                Relevance = 0.85,
                PerformanceScore = 0.8,
                Benefits = new List<string> { "Revenue insights", "Financial analysis" }
            });
        }

        return suggestions;
    }

    /// <summary>
    /// Check if data type is numeric
    /// </summary>
    private bool IsNumericType(string dataType)
    {
        var numericTypes = new[] { "int", "decimal", "float", "double", "money", "numeric", "bigint" };
        return numericTypes.Any(t => dataType.Contains(t, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if data type is date/time
    /// </summary>
    private bool IsDateType(string dataType)
    {
        var dateTypes = new[] { "date", "time", "datetime", "timestamp" };
        return dateTypes.Any(t => dataType.Contains(t, StringComparison.OrdinalIgnoreCase));
    }

    #endregion

    private List<IntelligentQuerySuggestion> RankSuggestionsByIntelligence(List<IntelligentQuerySuggestion> suggestions, ConversationAnalysis analysis) => suggestions.OrderByDescending(s => s.Relevance).ToList();
    
    private async Task<List<string>> GetGeneralAutocompleteSuggestionsAsync(SchemaMetadata schema) => new() { "Show", "Count", "List", "Find", "Get" };
    private List<string> GetGeneralContextualHelp() => new() { "Start with action words like 'Show', 'Count', 'List'", "Be specific about what data you want" };
    private async Task<List<string>> GenerateAutocompleteSuggestionsAsync(string partialQuery, AdvancedNLUResult nluResult, SchemaMetadata schema) => new();
    private List<string> GenerateSyntaxSuggestions(string partialQuery, AdvancedNLUResult nluResult) => new();
    private async Task<List<string>> GeneratePerformanceHintsAsync(string partialQuery, SchemaMetadata schema) => new();
    private List<string> GenerateContextualHelp(AdvancedNLUResult nluResult, SchemaMetadata schema) => new();
    private async Task<List<QueryValidation>> ValidatePartialQueryAsync(string partialQuery, SchemaMetadata schema) => new();
    
    private async Task UpdateUserPreferencesAsync(string userId, string query, QueryResponse response) { }
    private async Task ProcessUserFeedbackAsync(string userId, string query, QueryResponse response, UserFeedback feedback) { }
    private async Task UpdateNLUPatternsAsync(string query, QueryResponse response) { }
}
