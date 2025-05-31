using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.AI;

public class ContextManager : IContextManager
{
    private readonly ICacheService _cacheService;
    private readonly IAuditService _auditService;
    private readonly ISemanticAnalyzer _semanticAnalyzer;
    private readonly ILogger<ContextManager> _logger;

    public ContextManager(
        ICacheService cacheService,
        IAuditService auditService,
        ISemanticAnalyzer semanticAnalyzer,
        ILogger<ContextManager> logger)
    {
        _cacheService = cacheService;
        _auditService = auditService;
        _semanticAnalyzer = semanticAnalyzer;
        _logger = logger;
    }

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
            await UpdateQueryPatternsAsync(context, request.Question, response);

            // Update common filters
            UpdateCommonFilters(context, request.Question);

            // Infer user domain from query patterns
            UpdateUserDomain(context);

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
        _logger.LogError("🚀🚀🚀 ContextManager: GetRelevantSchemaAsync called for query: {Query}", query);
        _logger.LogError("🚀🚀🚀 ContextManager: Full schema has {TableCount} tables", fullSchema.Tables.Count);

        try
        {
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(query);
            var schemaContext = new SchemaContext();

            // Find relevant tables based on semantic entities and keywords
            var relevantTables = new List<TableMetadata>();
            var lowerQuery = query.ToLowerInvariant();

            _logger.LogInformation("ContextManager: Analyzing query with semantic analyzer");

            foreach (var table in fullSchema.Tables)
            {
                var relevanceScore = CalculateTableRelevance(table, semanticAnalysis, lowerQuery);
                if (relevanceScore > 0.5) // Higher threshold for better relevance
                {
                    relevantTables.Add(table);
                    _logger.LogDebug("Table {TableName} included with relevance {Score}", table.Name, relevanceScore);
                }
                else if (relevanceScore > 0.3)
                {
                    _logger.LogDebug("Table {TableName} excluded with low relevance {Score}", table.Name, relevanceScore);
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

            // Limit to top 5 most relevant tables
            schemaContext.RelevantTables = relevantTables.Take(5).ToList();

            // Find relationships between relevant tables
            schemaContext.Relationships = FindRelevantRelationships(schemaContext.RelevantTables, fullSchema);

            // Generate suggested joins
            schemaContext.SuggestedJoins = GenerateSuggestedJoins(schemaContext.RelevantTables, schemaContext.Relationships);

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

    private async Task<UserContext> BuildUserContextAsync(string userId)
    {
        var context = CreateDefaultUserContext(userId);

        try
        {
            // Get recent audit logs to build context
            var recentLogs = await _auditService.GetAuditLogsAsync(
                userId: userId,
                from: DateTime.UtcNow.AddDays(-30),
                to: DateTime.UtcNow,
                action: "QueryExecuted"
            );

            // Analyze query patterns from audit logs
            var queryPatterns = new Dictionary<string, QueryPattern>();

            foreach (var log in recentLogs.Take(100)) // Limit to recent 100 queries
            {
                if (log.Details is JsonElement details && details.TryGetProperty("query", out var queryElement))
                {
                    var query = queryElement.GetString();
                    if (!string.IsNullOrEmpty(query))
                    {
                        var normalizedQuery = NormalizeQuery(query);

                        if (queryPatterns.ContainsKey(normalizedQuery))
                        {
                            queryPatterns[normalizedQuery].Frequency++;
                            queryPatterns[normalizedQuery].LastUsed = log.Timestamp;
                        }
                        else
                        {
                            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(query);
                            queryPatterns[normalizedQuery] = new QueryPattern
                            {
                                Pattern = normalizedQuery,
                                Frequency = 1,
                                LastUsed = log.Timestamp,
                                Intent = semanticAnalysis.Intent,
                                AssociatedTables = ExtractTablesFromQuery(query)
                            };
                        }
                    }
                }
            }

            context.RecentPatterns = queryPatterns.Values
                .OrderByDescending(p => p.Frequency)
                .Take(20)
                .ToList();

            // Extract preferred tables from patterns
            context.PreferredTables = context.RecentPatterns
                .SelectMany(p => p.AssociatedTables)
                .GroupBy(t => t)
                .OrderByDescending(g => g.Count())
                .Take(10)
                .Select(g => g.Key)
                .ToList();

            // Infer user domain
            UpdateUserDomain(context);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error building user context from audit logs for user {UserId}", userId);
        }

        return context;
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

    private List<string> ExtractTablesFromSql(string sql)
    {
        var tables = new List<string>();
        var lowerSql = sql.ToLowerInvariant();

        // Simple regex to find table names after FROM and JOIN
        var fromMatches = System.Text.RegularExpressions.Regex.Matches(lowerSql, @"from\s+(\w+)");
        var joinMatches = System.Text.RegularExpressions.Regex.Matches(lowerSql, @"join\s+(\w+)");

        foreach (System.Text.RegularExpressions.Match match in fromMatches)
        {
            tables.Add(match.Groups[1].Value);
        }

        foreach (System.Text.RegularExpressions.Match match in joinMatches)
        {
            tables.Add(match.Groups[1].Value);
        }

        return tables.Distinct().ToList();
    }

    private List<string> ExtractTablesFromQuery(string query)
    {
        var tables = new List<string>();
        var lowerQuery = query.ToLowerInvariant();

        // Common table name patterns
        var tableKeywords = new Dictionary<string, string>
        {
            ["customer"] = "customers",
            ["order"] = "orders",
            ["product"] = "products",
            ["user"] = "users",
            ["sale"] = "sales",
            ["transaction"] = "transactions",
            ["payment"] = "payments",
            ["invoice"] = "invoices"
        };

        foreach (var (keyword, tableName) in tableKeywords)
        {
            if (lowerQuery.Contains(keyword))
            {
                tables.Add(tableName);
            }
        }

        return tables.Distinct().ToList();
    }

    private async Task UpdateQueryPatternsAsync(UserContext context, string query, QueryResponse response)
    {
        var normalizedQuery = NormalizeQuery(query);
        var existingPattern = context.RecentPatterns.FirstOrDefault(p => p.Pattern == normalizedQuery);

        if (existingPattern != null)
        {
            existingPattern.Frequency++;
            existingPattern.LastUsed = DateTime.UtcNow;
        }
        else
        {
            var semanticAnalysis = await _semanticAnalyzer.AnalyzeAsync(query);
            var newPattern = new QueryPattern
            {
                Pattern = normalizedQuery,
                Frequency = 1,
                LastUsed = DateTime.UtcNow,
                Intent = semanticAnalysis.Intent,
                AssociatedTables = ExtractTablesFromSql(response.Sql ?? "")
            };

            context.RecentPatterns.Add(newPattern);
        }

        // Keep only top 20 patterns
        context.RecentPatterns = context.RecentPatterns
            .OrderByDescending(p => p.Frequency)
            .Take(20)
            .ToList();
    }

    private void UpdateCommonFilters(UserContext context, string query)
    {
        var lowerQuery = query.ToLowerInvariant();

        // Extract common filter patterns
        var filterPatterns = new[]
        {
            "last week", "last month", "last year", "this week", "this month", "this year",
            "active", "inactive", "pending", "completed", "cancelled",
            "greater than", "less than", "between"
        };

        foreach (var pattern in filterPatterns)
        {
            if (lowerQuery.Contains(pattern) && !context.CommonFilters.Contains(pattern))
            {
                context.CommonFilters.Add(pattern);
            }
        }

        // Keep only top 10 filters
        if (context.CommonFilters.Count > 10)
        {
            context.CommonFilters = context.CommonFilters.Take(10).ToList();
        }
    }

    private void UpdateUserDomain(UserContext context)
    {
        var domainKeywords = new Dictionary<string, List<string>>
        {
            ["sales"] = new() { "sales", "revenue", "orders", "customers", "products" },
            ["marketing"] = new() { "campaigns", "leads", "conversions", "marketing", "advertising" },
            ["finance"] = new() { "payments", "invoices", "transactions", "accounting", "budget" },
            ["hr"] = new() { "employees", "payroll", "benefits", "performance", "hiring" },
            ["operations"] = new() { "inventory", "supply", "logistics", "operations", "warehouse" }
        };

        var domainScores = new Dictionary<string, int>();

        foreach (var (domain, keywords) in domainKeywords)
        {
            var score = 0;
            foreach (var pattern in context.RecentPatterns)
            {
                foreach (var keyword in keywords)
                {
                    if (pattern.Pattern.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                    {
                        score += pattern.Frequency;
                    }
                }
            }
            domainScores[domain] = score;
        }

        var topDomain = domainScores.OrderByDescending(kvp => kvp.Value).FirstOrDefault();
        if (topDomain.Value > 0)
        {
            context.Domain = topDomain.Key;
        }
    }

    private string NormalizeQuery(string query)
    {
        // Remove specific values and normalize to pattern
        var normalized = query.ToLowerInvariant();

        // Replace numbers with placeholder
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\b\d+\b", "N");

        // Replace dates with placeholder
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\b\d{4}-\d{2}-\d{2}\b", "DATE");

        // Replace quoted strings with placeholder
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"'[^']*'", "STRING");

        return normalized.Trim();
    }

    private double CalculateTableRelevance(TableMetadata table, SemanticAnalysis semanticAnalysis, string lowerQuery)
    {
        var relevance = 0.0;
        var tableName = table.Name.ToLowerInvariant();



        // Direct table name mention (highest priority)
        if (lowerQuery.Contains(tableName))
        {
            relevance += 1.0;
        }

        // Semantic entity matching (high priority)
        foreach (var entity in semanticAnalysis.Entities)
        {
            if (entity.Type == EntityType.Table && entity.Text.Equals(tableName, StringComparison.OrdinalIgnoreCase))
            {
                relevance += 0.9;
            }
        }

        // Business domain matching (high priority for gaming/casino context)
        var domainRelevance = CalculateDomainRelevance(table, lowerQuery);
        relevance += domainRelevance;

        // Primary business entity matching (medium-high priority)
        var businessEntityRelevance = CalculateBusinessEntityRelevance(table, lowerQuery);
        relevance += businessEntityRelevance;

        // Column name matching (medium priority, but weighted by importance)
        var columnRelevance = CalculateColumnRelevance(table, lowerQuery);
        relevance += columnRelevance;

        // Keyword matching (lower priority, more selective)
        var keywordRelevance = CalculateKeywordRelevance(table, semanticAnalysis.Keywords, lowerQuery);
        relevance += keywordRelevance;

        // GLOBAL BONUS TABLE FILTER: Force bonus tables to zero relevance for non-bonus queries
        var isBonusQuery = lowerQuery.Contains("bonus") ||
                          lowerQuery.Contains("promotion") ||
                          lowerQuery.Contains("reward") ||
                          lowerQuery.Contains("incentive") ||
                          lowerQuery.Contains("offer");

        var isBonusTable = table.Name.ToLowerInvariant().Contains("bonus");

        if (isBonusTable && !isBonusQuery)
        {
            relevance = 0.0;
        }

        return Math.Min(1.0, relevance);
    }

    private double CalculateDomainRelevance(TableMetadata table, string lowerQuery)
    {
        var tableName = table.Name.ToLowerInvariant();
        var relevance = 0.0;

        // Gaming/Casino domain-specific table priorities
        var primaryTables = new Dictionary<string, double>
        {
            ["daily_actions"] = 0.8,
            ["tbl_daily_actions"] = 0.8,
            ["players"] = 0.7,
            ["tbl_daily_actions_players"] = 0.7,
            ["users"] = 0.6,
            ["transactions"] = 0.6
        };

        var secondaryTables = new Dictionary<string, double>
        {
            ["deposits"] = 0.5,
            ["withdrawals"] = 0.5,
            ["bets"] = 0.5,
            ["games"] = 0.4
        };

        // Bonus tables - only relevant for bonus-specific queries
        var bonusTables = new Dictionary<string, double>
        {
            ["bonuses"] = 0.4,
            ["bonus_balances"] = 0.4
        };

        // Check primary tables first
        foreach (var (pattern, score) in primaryTables)
        {
            if (tableName.Contains(pattern))
            {
                relevance = Math.Max(relevance, score);
            }
        }

        // Check secondary tables only if no primary match
        if (relevance == 0.0)
        {
            foreach (var (pattern, score) in secondaryTables)
            {
                if (tableName.Contains(pattern))
                {
                    relevance = Math.Max(relevance, score);
                }
            }
        }

        // Check bonus tables only if query explicitly mentions bonus-related terms
        if (relevance == 0.0)
        {
            var isBonusQuery = lowerQuery.Contains("bonus") ||
                              lowerQuery.Contains("promotion") ||
                              lowerQuery.Contains("reward") ||
                              lowerQuery.Contains("incentive") ||
                              lowerQuery.Contains("offer");

            if (isBonusQuery)
            {
                foreach (var (pattern, score) in bonusTables)
                {
                    if (tableName.Contains(pattern))
                    {
                        relevance = Math.Max(relevance, score);
                    }
                }
            }
        }

        return relevance;
    }

    private double CalculateBusinessEntityRelevance(TableMetadata table, string lowerQuery)
    {
        var relevance = 0.0;

        // Map query intent to relevant table types
        var intentMappings = new Dictionary<string, List<string>>
        {
            ["player"] = new() { "players", "daily_actions_players", "users", "daily_actions" },
            ["count"] = new() { "daily_actions", "players", "transactions" },
            ["active"] = new() { "players", "daily_actions", "users" },
            ["yesterday"] = new() { "daily_actions", "transactions", "bets" },
            ["deposit"] = new() { "daily_actions", "transactions", "deposits" },
            ["revenue"] = new() { "daily_actions", "transactions", "bets" },
            ["casino"] = new() { "daily_actions", "games", "bets" },
            ["sports"] = new() { "daily_actions", "bets" }
        };

        // Bonus-specific intent mappings (only applied when bonus terms are present)
        var bonusIntentMappings = new Dictionary<string, List<string>>
        {
            ["bonus"] = new() { "bonuses", "bonus_balances" },
            ["promotion"] = new() { "bonuses", "bonus_balances" },
            ["reward"] = new() { "bonuses", "bonus_balances" },
            ["incentive"] = new() { "bonuses", "bonus_balances" },
            ["offer"] = new() { "bonuses", "bonus_balances" }
        };

        // Apply standard intent mappings
        foreach (var (intent, tablePatterns) in intentMappings)
        {
            if (lowerQuery.Contains(intent))
            {
                foreach (var pattern in tablePatterns)
                {
                    if (table.Name.ToLowerInvariant().Contains(pattern))
                    {
                        relevance += 0.4;
                        break; // Only count once per intent
                    }
                }
            }
        }

        // Apply bonus intent mappings only if bonus terms are present in query
        var hasBonusTerms = lowerQuery.Contains("bonus") ||
                           lowerQuery.Contains("promotion") ||
                           lowerQuery.Contains("reward") ||
                           lowerQuery.Contains("incentive") ||
                           lowerQuery.Contains("offer");

        if (hasBonusTerms)
        {
            foreach (var (intent, tablePatterns) in bonusIntentMappings)
            {
                if (lowerQuery.Contains(intent))
                {
                    foreach (var pattern in tablePatterns)
                    {
                        if (table.Name.ToLowerInvariant().Contains(pattern))
                        {
                            relevance += 0.4;
                            break; // Only count once per intent
                        }
                    }
                }
            }
        }

        return Math.Min(0.8, relevance);
    }

    private double CalculateColumnRelevance(TableMetadata table, string lowerQuery)
    {
        var relevance = 0.0;
        var importantColumns = 0;
        var totalMatches = 0;

        foreach (var column in table.Columns)
        {
            var columnName = column.Name.ToLowerInvariant();

            if (lowerQuery.Contains(columnName))
            {
                totalMatches++;

                // Weight by column importance
                if (IsBusinessCriticalColumn(columnName))
                {
                    relevance += 0.4;
                    importantColumns++;
                }
                else if (IsCommonColumn(columnName))
                {
                    relevance += 0.1; // Lower weight for common columns like PlayerID
                }
                else
                {
                    relevance += 0.2;
                }
            }
        }

        // Bonus for tables with multiple relevant columns
        if (totalMatches > 1)
        {
            relevance += 0.1;
        }

        // Bonus for tables with business-critical columns
        if (importantColumns > 0)
        {
            relevance += 0.2;
        }

        return Math.Min(0.6, relevance);
    }

    private double CalculateKeywordRelevance(TableMetadata table, List<string> keywords, string lowerQuery)
    {
        var relevance = 0.0;
        var tableName = table.Name.ToLowerInvariant();

        foreach (var keyword in keywords.Where(k => k.Length > 3))
        {
            // Table name contains keyword
            if (tableName.Contains(keyword.ToLowerInvariant()))
            {
                relevance += 0.15;
            }

            // Important columns contain keyword
            var matchingColumns = table.Columns.Where(c =>
                c.Name.ToLowerInvariant().Contains(keyword.ToLowerInvariant()) &&
                IsBusinessCriticalColumn(c.Name.ToLowerInvariant()));

            relevance += matchingColumns.Count() * 0.1;
        }

        return Math.Min(0.3, relevance);
    }

    private bool IsBusinessCriticalColumn(string columnName)
    {
        var criticalColumns = new HashSet<string>
        {
            "date", "actiondate", "timestamp", "createdate",
            "amount", "deposits", "withdrawals", "bets", "wins",
            "status", "active", "playerid", "userid",
            "revenue", "profit", "balance"
        };

        return criticalColumns.Any(critical => columnName.Contains(critical));
    }

    private bool IsCommonColumn(string columnName)
    {
        var commonColumns = new HashSet<string>
        {
            "id", "playerid", "userid", "currencyid", "status", "createdate", "updatedate"
        };

        return commonColumns.Contains(columnName);
    }

    private List<TableMetadata> FindTablesByKeywords(List<TableMetadata> tables, string lowerQuery)
    {
        var relevantTables = new List<(TableMetadata table, double score)>();

        foreach (var table in tables)
        {
            var score = 0.0;
            var tableName = table.Name.ToLowerInvariant();

            // Direct table name match
            if (lowerQuery.Contains(tableName))
            {
                score += 1.0;
            }

            // Business-critical column matches
            var criticalColumnMatches = table.Columns.Count(c =>
                lowerQuery.Contains(c.Name.ToLowerInvariant()) &&
                IsBusinessCriticalColumn(c.Name.ToLowerInvariant()));
            score += criticalColumnMatches * 0.3;

            // Common column matches (lower weight)
            var commonColumnMatches = table.Columns.Count(c =>
                lowerQuery.Contains(c.Name.ToLowerInvariant()) &&
                IsCommonColumn(c.Name.ToLowerInvariant()));
            score += commonColumnMatches * 0.1;

            if (score > 0.2) // Only include tables with meaningful matches
            {
                relevantTables.Add((table, score));
            }
        }

        return relevantTables
            .OrderByDescending(x => x.score)
            .Take(3)
            .Select(x => x.table)
            .ToList();
    }

    private List<TableMetadata> GetDefaultGamingTables(List<TableMetadata> tables)
    {
        var defaultTableNames = new[]
        {
            "tbl_Daily_actions",
            "tbl_Daily_actions_players",
            "Players",
            "Users"
        };

        var defaultTables = new List<TableMetadata>();

        foreach (var tableName in defaultTableNames)
        {
            var table = tables.FirstOrDefault(t =>
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
            if (table != null)
            {
                defaultTables.Add(table);
            }
        }

        // If still no matches, return first few tables
        if (!defaultTables.Any())
        {
            defaultTables = tables.Take(2).ToList();
        }

        return defaultTables;
    }

    private List<RelationshipInfo> FindRelevantRelationships(List<TableMetadata> tables, SchemaMetadata fullSchema)
    {
        var relationships = new List<RelationshipInfo>();
        var tableNames = tables.Select(t => t.Name).ToHashSet();

        // Simple foreign key detection based on naming conventions
        foreach (var table in tables)
        {
            foreach (var column in table.Columns)
            {
                if (column.Name.EndsWith("Id", StringComparison.OrdinalIgnoreCase) &&
                    column.Name.Length > 2)
                {
                    var referencedTableName = column.Name.Substring(0, column.Name.Length - 2);
                    var referencedTable = tables.FirstOrDefault(t =>
                        t.Name.Equals(referencedTableName, StringComparison.OrdinalIgnoreCase) ||
                        t.Name.Equals(referencedTableName + "s", StringComparison.OrdinalIgnoreCase));

                    if (referencedTable != null)
                    {
                        relationships.Add(new RelationshipInfo
                        {
                            FromTable = table.Name,
                            ToTable = referencedTable.Name,
                            FromColumn = column.Name,
                            ToColumn = "Id", // Assume primary key is "Id"
                            Type = RelationshipType.ManyToOne,
                            Confidence = 0.8
                        });
                    }
                }
            }
        }

        return relationships;
    }

    private List<string> GenerateSuggestedJoins(List<TableMetadata> tables, List<RelationshipInfo> relationships)
    {
        var joins = new List<string>();

        foreach (var relationship in relationships)
        {
            var joinClause = $"{relationship.FromTable}.{relationship.FromColumn} = {relationship.ToTable}.{relationship.ToColumn}";
            joins.Add(joinClause);
        }

        return joins;
    }

    private Dictionary<string, string> CreateColumnMappings(List<TableMetadata> tables)
    {
        var mappings = new Dictionary<string, string>();

        foreach (var table in tables)
        {
            foreach (var column in table.Columns)
            {
                var businessTerm = ConvertToBusinessTerm(column.Name);
                if (!string.IsNullOrEmpty(businessTerm))
                {
                    mappings[businessTerm] = $"{table.Name}.{column.Name}";
                }
            }
        }

        return mappings;
    }

    private List<string> ExtractBusinessTerms(SemanticAnalysis semanticAnalysis)
    {
        return semanticAnalysis.Keywords
            .Where(k => k.Length > 3 && !IsCommonWord(k))
            .ToList();
    }

    private string ConvertToBusinessTerm(string columnName)
    {
        // Convert technical column names to business terms
        var businessTerms = new Dictionary<string, string>
        {
            ["customer_id"] = "customer",
            ["order_date"] = "order date",
            ["total_amount"] = "total",
            ["created_at"] = "created date",
            ["updated_at"] = "updated date"
        };

        var lowerColumnName = columnName.ToLowerInvariant();
        return businessTerms.GetValueOrDefault(lowerColumnName, "");
    }

    private bool IsCommonWord(string word)
    {
        var commonWords = new HashSet<string> { "data", "info", "value", "item", "record", "entry" };
        return commonWords.Contains(word.ToLowerInvariant());
    }
}
