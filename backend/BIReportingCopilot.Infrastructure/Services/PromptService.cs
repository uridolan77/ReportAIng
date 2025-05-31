using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Services;

public class PromptService : IPromptService
{
    private readonly ILogger<PromptService> _logger;
    private readonly BICopilotContext _context;

    public PromptService(ILogger<PromptService> logger, BICopilotContext context)
    {
        _logger = logger;
        _context = context;
    }

    public async Task<string> BuildQueryPromptAsync(string naturalLanguageQuery, SchemaMetadata schema, string? context = null)
    {
        try
        {
            var template = await GetPromptTemplateAsync("sql_generation");

            var schemaDescription = BuildEnhancedSchemaDescription(schema);
            var businessRules = GetBusinessRulesForQuery(naturalLanguageQuery);
            var exampleQueries = GetRelevantExampleQueries(naturalLanguageQuery);
            var contextInfo = !string.IsNullOrEmpty(context) ? $"\nAdditional context: {context}" : "";

            var prompt = template.Content
                .Replace("{schema}", schemaDescription)
                .Replace("{question}", naturalLanguageQuery)
                .Replace("{context}", contextInfo)
                .Replace("{business_rules}", businessRules)
                .Replace("{examples}", exampleQueries);

            _logger.LogDebug("Built enhanced query prompt for question: {Question}", naturalLanguageQuery);

            // Log detailed prompt information for admin debugging
            await LogPromptDetailsAsync("sql_generation", naturalLanguageQuery, prompt, new
            {
                SchemaTablesCount = schema.Tables?.Count ?? 0,
                BusinessRulesApplied = businessRules,
                ExampleQueriesIncluded = exampleQueries,
                ContextProvided = !string.IsNullOrEmpty(context)
            });

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building query prompt");
            return GetFallbackQueryPrompt(naturalLanguageQuery, schema, context);
        }
    }

    public async Task<string> BuildInsightPromptAsync(string query, QueryResult result)
    {
        try
        {
            var template = await GetPromptTemplateAsync("insight_generation");

            var dataPreview = GenerateDataPreview(result.Data);
            var columnInfo = string.Join(", ", result.Metadata.Columns.Select(c => $"{c.Name} ({c.DataType})"));

            var prompt = template.Content
                .Replace("{query}", query)
                .Replace("{data_preview}", dataPreview)
                .Replace("{column_info}", columnInfo)
                .Replace("{row_count}", result.Metadata.RowCount.ToString());

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building insight prompt");
            return GetFallbackInsightPrompt(query, result);
        }
    }

    public async Task<string> BuildVisualizationPromptAsync(string query, QueryResult result)
    {
        try
        {
            var template = await GetPromptTemplateAsync("visualization_generation");

            var columnInfo = string.Join(", ", result.Metadata.Columns.Select(c => $"{c.Name} ({c.DataType})"));
            var dataCharacteristics = AnalyzeDataCharacteristics(result);

            var prompt = template.Content
                .Replace("{query}", query)
                .Replace("{column_info}", columnInfo)
                .Replace("{row_count}", result.Metadata.RowCount.ToString())
                .Replace("{data_characteristics}", dataCharacteristics);

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building visualization prompt");
            return GetFallbackVisualizationPrompt(query, result);
        }
    }

    public async Task<PromptTemplate> GetPromptTemplateAsync(string templateName, string? version = null)
    {
        try
        {
            var query = _context.PromptTemplates
                .Where(t => t.Name == templateName && t.IsActive);

            if (!string.IsNullOrEmpty(version))
            {
                query = query.Where(t => t.Version == version);
            }

            var entity = await query
                .OrderByDescending(t => t.CreatedDate)
                .FirstOrDefaultAsync();

            if (entity != null)
            {
                await IncrementUsageCountAsync(entity.Id);
                return MapToPromptTemplate(entity);
            }

            // Return default template if not found
            return GetDefaultTemplate(templateName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt template: {TemplateName}", templateName);
            return GetDefaultTemplate(templateName);
        }
    }

    public async Task<PromptTemplate> CreatePromptTemplateAsync(PromptTemplate template)
    {
        try
        {
            var entity = new PromptTemplateEntity
            {
                Name = template.Name,
                Version = template.Version,
                Content = template.Content,
                Description = template.Description,
                IsActive = template.IsActive,
                CreatedBy = template.CreatedBy,
                CreatedDate = DateTime.UtcNow,
                UsageCount = 0,
                Parameters = JsonSerializer.Serialize(template.Parameters)
            };

            _context.PromptTemplates.Add(entity);
            await _context.SaveChangesAsync();

            template.CreatedDate = entity.CreatedDate;
            _logger.LogInformation("Created prompt template: {TemplateName} v{Version}", template.Name, template.Version);

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating prompt template: {TemplateName}", template.Name);
            throw;
        }
    }

    public async Task<PromptTemplate> UpdatePromptTemplateAsync(PromptTemplate template)
    {
        try
        {
            var entity = await _context.PromptTemplates
                .FirstOrDefaultAsync(t => t.Name == template.Name && t.Version == template.Version);

            if (entity == null)
            {
                throw new InvalidOperationException($"Prompt template {template.Name} v{template.Version} not found");
            }

            entity.Content = template.Content;
            entity.Description = template.Description;
            entity.IsActive = template.IsActive;
            entity.UpdatedDate = DateTime.UtcNow;
            entity.Parameters = JsonSerializer.Serialize(template.Parameters);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated prompt template: {TemplateName} v{Version}", template.Name, template.Version);
            return MapToPromptTemplate(entity);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating prompt template: {TemplateName}", template.Name);
            throw;
        }
    }

    public async Task<List<PromptTemplate>> GetPromptTemplatesAsync()
    {
        try
        {
            var entities = await _context.PromptTemplates
                .Where(t => t.IsActive)
                .OrderBy(t => t.Name)
                .ThenByDescending(t => t.CreatedDate)
                .ToListAsync();

            return entities.Select(MapToPromptTemplate).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt templates");
            return new List<PromptTemplate>();
        }
    }

    public async Task<PromptPerformanceMetrics> GetPromptPerformanceAsync(string templateName)
    {
        try
        {
            var template = await _context.PromptTemplates
                .Where(t => t.Name == templateName && t.IsActive)
                .OrderByDescending(t => t.CreatedDate)
                .FirstOrDefaultAsync();

            if (template == null)
            {
                return new PromptPerformanceMetrics
                {
                    TemplateName = templateName,
                    UsageCount = 0,
                    SuccessRate = 0,
                    AverageResponseTime = TimeSpan.Zero
                };
            }

            // Calculate success rate based on query history
            var recentQueries = await _context.QueryHistory
                .Where(q => q.QueryTimestamp >= DateTime.UtcNow.AddDays(-30))
                .ToListAsync();

            var successfulQueries = recentQueries.Count(q => q.IsSuccessful);
            var totalQueries = recentQueries.Count;
            var successRate = totalQueries > 0 ? (double)successfulQueries / totalQueries : 0;

            var averageResponseTime = recentQueries
                .Where(q => q.ExecutionTimeMs.HasValue)
                .Select(q => q.ExecutionTimeMs!.Value)
                .DefaultIfEmpty(0)
                .Average();

            return new PromptPerformanceMetrics
            {
                TemplateName = templateName,
                UsageCount = template.UsageCount,
                SuccessRate = successRate,
                AverageResponseTime = TimeSpan.FromMilliseconds(averageResponseTime)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting prompt performance for template: {TemplateName}", templateName);
            return new PromptPerformanceMetrics
            {
                TemplateName = templateName,
                UsageCount = 0,
                SuccessRate = 0,
                AverageResponseTime = TimeSpan.Zero
            };
        }
    }

    private string BuildSchemaDescription(SchemaMetadata schema)
    {
        var description = new List<string>();

        foreach (var table in schema.Tables.Take(20)) // Limit to first 20 tables
        {
            var tableDesc = $"{table.Schema}.{table.Name}";
            if (!string.IsNullOrEmpty(table.Description))
            {
                tableDesc += $" - {table.Description}";
            }

            var columns = table.Columns.Take(10).Select(c =>
                $"{c.Name} {c.DataType}{(c.IsPrimaryKey ? " PK" : "")}{(c.IsForeignKey ? " FK" : "")}");

            if (table.Columns.Count > 10)
            {
                columns = columns.Concat(new[] { $"... and {table.Columns.Count - 10} more columns" });
            }

            tableDesc += $" ({string.Join(", ", columns)})";
            description.Add(tableDesc);
        }

        if (schema.Tables.Count > 20)
        {
            description.Add($"... and {schema.Tables.Count - 20} more tables");
        }

        return string.Join("\n", description);
    }

    private string BuildEnhancedSchemaDescription(SchemaMetadata schema)
    {
        var description = new List<string>();

        // Use all tables from the schema (should already be filtered by ContextManager)
        var tablesToProcess = schema.Tables ?? new List<TableMetadata>();



        foreach (var table in tablesToProcess)
        {
            var tableDesc = $"TABLE: {table.Schema}.{table.Name}";

            // Add business description if available
            if (!string.IsNullOrEmpty(table.Description))
            {
                tableDesc += $"\n  Purpose: {table.Description}";
            }

            // Add business context based on table name
            var businessContext = GetTableBusinessContext(table.Name);
            if (!string.IsNullOrEmpty(businessContext))
            {
                tableDesc += $"\n  Business Context: {businessContext}";
            }

            // Add columns with enhanced information
            tableDesc += "\n  Columns:";
            foreach (var column in table.Columns.Take(15))
            {
                var columnDesc = $"    - {column.Name} ({column.DataType})";
                if (column.IsPrimaryKey) columnDesc += " [PRIMARY KEY]";
                if (column.IsForeignKey) columnDesc += " [FOREIGN KEY]";
                if (!column.IsNullable) columnDesc += " [NOT NULL]";

                // Add business meaning for common column names
                var businessMeaning = GetColumnBusinessMeaning(column.Name);
                if (!string.IsNullOrEmpty(businessMeaning))
                {
                    columnDesc += $" - {businessMeaning}";
                }

                tableDesc += $"\n{columnDesc}";
            }

            if (table.Columns.Count > 15)
            {
                tableDesc += $"\n    ... and {table.Columns.Count - 15} more columns";
            }

            description.Add(tableDesc);
        }

        // Note: No need to show "more tables" message since we're using filtered relevant tables

        return string.Join("\n\n", description);
    }

    private string GetTableBusinessContext(string tableName)
    {
        return tableName.ToLower() switch
        {
            "tbl_daily_actions" => "Main statistics table holding all player statistics aggregated by player by day. Core table for daily reporting and player activity analysis.",
            "tbl_daily_actions_players" => "Player master data table containing all player information, demographics, and account details.",
            "tbl_bonus_balances" => "Tracks bonus amounts and balances for players, linked to daily actions that trigger bonus calculations.",
            "whitelabels" => "Metadata table defining different casino brands/operators within the platform.",
            "countries" => "Reference table for country codes, names, and geographical information for player segmentation.",
            "currencies" => "Reference table for supported currencies, exchange rates, and currency-specific business rules.",
            _ when tableName.Contains("daily") => "Daily aggregated data for reporting and analytics",
            _ when tableName.Contains("player") => "Player-related information and demographics",
            _ when tableName.Contains("bonus") => "Bonus and promotional data",
            _ when tableName.Contains("balance") => "Financial balance and transaction data",
            _ => ""
        };
    }

    private string GetColumnBusinessMeaning(string columnName)
    {
        return columnName.ToLower() switch
        {
            "id" => "Primary key - unique record identifier",
            "playerid" => "Unique identifier for a player in the system",
            "date" => "Business date (not timestamp) when the activity occurred",
            "whitelabelid" => "Casino brand/operator identifier (correct spelling)",
            "registration" => "Number of new player registrations on this date",
            "ftd" => "First Time Deposit - number of players making their first deposit",
            "ftda" => "First Time Deposit Amount - total amount of first deposits",
            "deposits" => "Total deposit amount for this player on this date",
            "depositscreditcard" => "Deposits made via credit card",
            "depositsneteller" => "Deposits made via Neteller",
            "depositsmoneybookers" => "Deposits made via MoneyBookers (Skrill)",
            "depositsother" => "Deposits made via other payment methods",
            "cashoutrequests" => "Total amount of withdrawal requests",
            "paidcashouts" => "Total amount of paid withdrawals",
            "chargebacks" => "Chargeback amounts",
            "bonuses" => "Total bonus amounts awarded",
            "collectedbonuses" => "Bonus amounts actually collected by players",
            "expiredbonuses" => "Bonus amounts that expired",
            "betsreal" => "Real money bets placed",
            "betsbonus" => "Bonus money bets placed",
            "winsreal" => "Real money winnings",
            "winsbonus" => "Bonus money winnings",
            "betssport" => "Sports betting amounts",
            "winssport" => "Sports betting winnings",
            "betscasino" => "Casino game betting amounts",
            "winscasino" => "Casino game winnings",
            "amount" => "Financial value in the player's currency",
            "totalamount" => "Sum of all financial values",
            "bonusbalanceid" => "Links to specific bonus balance record",
            "countryid" => "Player's country for geographical analysis",
            "currencyid" => "Currency used for this transaction/player",
            "actiontype" => "Type of player activity (deposit, bet, withdrawal, etc.)",
            "userid" => "Same as PlayerID - unique player identifier",
            "registrationdate" => "When the player first registered",
            "lastlogindate" => "Most recent player login timestamp",
            "status" => "Current status (active, inactive, suspended, etc.)",
            "balance" => "Current account balance",
            "totaldeposits" => "Lifetime sum of all deposits",
            "totalwithdraws" => "Lifetime sum of all withdrawals",
            "totalbets" => "Lifetime sum of all bets placed",
            "totalwins" => "Lifetime sum of all winnings",
            _ when columnName.Contains("total") => "Aggregated sum value",
            _ when columnName.Contains("count") => "Number of occurrences",
            _ when columnName.Contains("date") => "Date/timestamp field",
            _ when columnName.Contains("id") => "Unique identifier or foreign key reference",
            _ when columnName.Contains("deposits") => "Deposit-related financial amount",
            _ when columnName.Contains("bets") => "Betting-related amount",
            _ when columnName.Contains("wins") => "Winnings-related amount",
            _ when columnName.Contains("bonus") => "Bonus-related amount",
            _ => ""
        };
    }

    private string GetBusinessRulesForQuery(string naturalLanguageQuery)
    {
        var query = naturalLanguageQuery.ToLower();
        var rules = new List<string>();

        if (query.Contains("today"))
        {
            rules.Add("For 'today': Use WHERE Date = CAST(GETDATE() AS DATE)");
            rules.Add("Focus on tbl_Daily_actions as the primary source for today's data");
        }

        if (query.Contains("total") || query.Contains("sum"))
        {
            rules.Add("For 'totals': Use SUM() aggregation on Amount columns");
            rules.Add("Consider grouping by relevant dimensions (PlayerID, WhitelabelID, CountryID)");
        }

        if (query.Contains("player"))
        {
            rules.Add("For player data: JOIN tbl_Daily_actions with tbl_Daily_actions_players on PlayerID");
            rules.Add("Include player demographics from tbl_Daily_actions_players when relevant");
        }

        if (query.Contains("bonus"))
        {
            rules.Add("For bonus data: JOIN with tbl_Bonus_balances using BonusBalanceID");
            rules.Add("Bonus amounts are stored in tbl_Bonus_balances.Amount");
        }

        if (query.Contains("country") || query.Contains("region"))
        {
            rules.Add("For geographical analysis: JOIN with countries table using CountryID");
            rules.Add("Use country names for better readability in results");
        }

        if (query.Contains("currency"))
        {
            rules.Add("For currency analysis: JOIN with currencies table using CurrencyID");
            rules.Add("Include currency codes in results for clarity");
        }

        if (query.Contains("brand") || query.Contains("operator") || query.Contains("whitelabel"))
        {
            rules.Add("For brand analysis: JOIN with whitelabels table using WhitelabelID");
            rules.Add("Include whitelabel names for business context");
        }

        // Default rules if no specific patterns found
        if (rules.Count == 0)
        {
            rules.Add("Use tbl_Daily_actions as primary table for statistical queries");
            rules.Add("JOIN with reference tables (players, countries, currencies, whitelabels) for context");
            rules.Add("Use appropriate aggregations (SUM, COUNT, AVG) based on the question");
        }

        return string.Join("\n- ", rules);
    }

    private string GetRelevantExampleQueries(string naturalLanguageQuery)
    {
        var query = naturalLanguageQuery.ToLower();
        var examples = new List<string>();

        if (query.Contains("total") && query.Contains("today"))
        {
            examples.Add(@"
EXAMPLE: 'totals today'
SQL: SELECT SUM(Deposits) as TotalDeposits, COUNT(DISTINCT PlayerID) as PlayerCount
     FROM common.tbl_Daily_actions
     WHERE Date = CAST(GETDATE() AS DATE)");
        }

        if (query.Contains("deposit") && (query.Contains("brand") || query.Contains("whitelabel")))
        {
            examples.Add(@"
EXAMPLE: 'deposits by brand today'
SQL: SELECT da.WhiteLabelID, SUM(da.Deposits) as TotalDeposits, COUNT(DISTINCT da.PlayerID) as PlayerCount
     FROM common.tbl_Daily_actions da
     WHERE da.Date = CAST(GETDATE() AS DATE)
     GROUP BY da.WhiteLabelID
     ORDER BY TotalDeposits DESC");
        }

        if (query.Contains("player") && query.Contains("today"))
        {
            examples.Add(@"
EXAMPLE: 'player activity today'
SQL: SELECT da.PlayerID, p.Username, SUM(da.Amount) as TotalAmount,
            SUM(da.TotalDeposits) as Deposits, SUM(da.TotalBets) as Bets
     FROM common.tbl_Daily_actions da
     JOIN common.tbl_Daily_actions_players p ON da.PlayerID = p.PlayerID
     WHERE da.Date = CAST(GETDATE() AS DATE)
     GROUP BY da.PlayerID, p.Username
     ORDER BY TotalAmount DESC");
        }

        if (query.Contains("country") || query.Contains("region"))
        {
            examples.Add(@"
EXAMPLE: 'revenue by country today'
SQL: SELECT c.CountryName, SUM(da.Amount) as Revenue, COUNT(DISTINCT da.PlayerID) as Players
     FROM common.tbl_Daily_actions da
     JOIN common.tbl_Daily_actions_players p ON da.PlayerID = p.PlayerID
     JOIN common.countries c ON p.CountryID = c.CountryID
     WHERE da.Date = CAST(GETDATE() AS DATE)
     GROUP BY c.CountryName
     ORDER BY Revenue DESC");
        }

        if (query.Contains("bonus"))
        {
            examples.Add(@"
EXAMPLE: 'bonus totals'
SQL: SELECT SUM(Amount) as TotalBonusAmount, COUNT(*) as BonusCount,
            AVG(Amount) as AverageBonusAmount
     FROM common.tbl_Bonus_balances
     WHERE Status = 'Active'");
        }

        if (query.Contains("brand") || query.Contains("whitelabel"))
        {
            examples.Add(@"
EXAMPLE: 'revenue by brand today'
SQL: SELECT w.Name as BrandName, SUM(da.Amount) as Revenue, COUNT(DISTINCT da.PlayerID) as Players
     FROM common.tbl_Daily_actions da
     JOIN common.whitelabels w ON da.WhitelabelID = w.WhitelabelID
     WHERE da.Date = CAST(GETDATE() AS DATE)
     GROUP BY w.Name
     ORDER BY Revenue DESC");
        }

        if (examples.Count == 0)
        {
            examples.Add(@"
EXAMPLE: 'basic daily statistics'
SQL: SELECT Date, SUM(Amount) as TotalAmount, COUNT(DISTINCT PlayerID) as PlayerCount
     FROM common.tbl_Daily_actions
     WHERE Date >= DATEADD(day, -7, CAST(GETDATE() AS DATE))
     GROUP BY Date
     ORDER BY Date DESC");
        }

        return string.Join("\n", examples);
    }

    private string GenerateDataPreview(object[] data)
    {
        if (data.Length == 0) return "No data returned";

        var preview = data.Take(3).Select((item, index) => $"Row {index + 1}: {JsonSerializer.Serialize(item)}");
        var result = string.Join("\n", preview);

        if (data.Length > 3)
        {
            result += $"\n... and {data.Length - 3} more rows";
        }

        return result;
    }

    private string AnalyzeDataCharacteristics(QueryResult result)
    {
        var characteristics = new List<string>();

        var numericColumns = result.Metadata.Columns.Count(c =>
            c.DataType.Contains("int") || c.DataType.Contains("decimal") || c.DataType.Contains("float"));

        var dateColumns = result.Metadata.Columns.Count(c =>
            c.DataType.Contains("date") || c.DataType.Contains("time"));

        var textColumns = result.Metadata.Columns.Count(c =>
            c.DataType.Contains("varchar") || c.DataType.Contains("char") || c.DataType.Contains("text"));

        characteristics.Add($"Columns: {result.Metadata.ColumnCount} total");
        characteristics.Add($"Numeric columns: {numericColumns}");
        characteristics.Add($"Date/time columns: {dateColumns}");
        characteristics.Add($"Text columns: {textColumns}");
        characteristics.Add($"Rows: {result.Metadata.RowCount}");

        return string.Join(", ", characteristics);
    }

    private PromptTemplate GetDefaultTemplate(string templateName)
    {
        return templateName switch
        {
            "sql_generation" => new PromptTemplate
            {
                Name = "sql_generation",
                Version = "2.0",
                Content = @"You are an expert SQL developer specializing in business intelligence and gaming/casino data analysis.

BUSINESS DOMAIN CONTEXT:
- This is a gaming/casino database tracking player activities, bonuses, and financial transactions
- 'Daily actions' refer to player activities that occurred on a specific date
- 'Totals' usually mean aggregated amounts, counts, or sums of financial values
- 'Today' means the current date (use CAST(GETDATE() AS DATE) for today's date)
- 'Bonus balances' are financial amounts related to player bonuses
- Players perform actions that may trigger bonus calculations

DATABASE SCHEMA:
{schema}

USER QUESTION: {question}
{context}

BUSINESS LOGIC RULES:
{business_rules}

EXAMPLE QUERIES:
{examples}

TECHNICAL RULES:
1. Only use SELECT statements - never INSERT, UPDATE, DELETE
2. Use proper table and column names exactly as shown in schema
3. Include appropriate WHERE clauses for filtering
4. Use JOINs when querying multiple tables based on foreign key relationships
5. Add meaningful column aliases (e.g., SUM(Amount) AS TotalAmount)
6. Add ORDER BY for logical sorting (usually by date DESC or amount DESC)
7. Use TOP 100 to limit results unless user specifies otherwise
8. Return only the SQL query without explanations or markdown formatting
9. Ensure all referenced columns exist in the schema
10. Always add WITH (NOLOCK) hint to all table references for better read performance
11. Format table hints as: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints",
                Description = "Enhanced SQL generation template with business context",
                IsActive = true,
                CreatedBy = "System"
            },
            "insight_generation" => new PromptTemplate
            {
                Name = "insight_generation",
                Version = "1.0",
                Content = @"Analyze the following query results and provide business insights:

Query: {query}
Data preview: {data_preview}
Columns: {column_info}
Total rows: {row_count}

Provide 2-3 key insights focusing on:
1. Notable patterns or trends
2. Business implications
3. Potential areas for further investigation

Keep insights concise and actionable.",
                Description = "Default insight generation template",
                IsActive = true,
                CreatedBy = "System"
            },
            "visualization_generation" => new PromptTemplate
            {
                Name = "visualization_generation",
                Version = "1.0",
                Content = @"Based on the query and data structure, suggest the best visualization:

Query: {query}
Columns: {column_info}
Data characteristics: {data_characteristics}

Return JSON configuration with:
- type: (bar, line, pie, table, scatter, etc.)
- title: descriptive title
- xAxis: column for x-axis (if applicable)
- yAxis: column for y-axis (if applicable)
- groupBy: column for grouping (if applicable)

Return only valid JSON.",
                Description = "Default visualization generation template",
                IsActive = true,
                CreatedBy = "System"
            },
            _ => new PromptTemplate
            {
                Name = templateName,
                Version = "1.0",
                Content = "Default template content for {templateName}",
                Description = $"Default template for {templateName}",
                IsActive = true,
                CreatedBy = "System"
            }
        };
    }

    private string GetFallbackQueryPrompt(string naturalLanguageQuery, SchemaMetadata schema, string? context)
    {
        var schemaDescription = BuildEnhancedSchemaDescription(schema);
        var businessRules = GetBusinessRulesForQuery(naturalLanguageQuery);

        return $@"You are an expert SQL developer specializing in gaming/casino business intelligence.

BUSINESS CONTEXT:
- Gaming/casino database with player activities, bonuses, and transactions
- 'Today' = CAST(GETDATE() AS DATE)
- 'Totals' = SUM() aggregations of amounts/values
- Focus on daily actions and bonus relationships

DATABASE SCHEMA:
{schemaDescription}

BUSINESS RULES:
{businessRules}

USER QUESTION: {naturalLanguageQuery}
{(string.IsNullOrEmpty(context) ? "" : $"ADDITIONAL CONTEXT: {context}")}

Generate a SQL SELECT query that:
1. Uses proper table/column names from schema
2. Includes appropriate WHERE clauses for date filtering
3. Uses meaningful JOINs based on relationships
4. Returns aggregated totals when requested
5. Limits results with TOP 100
6. Uses clear column aliases
7. Always adds WITH (NOLOCK) hint to all table references for better read performance
8. Formats as: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints

Return only the SQL query without explanations.";
    }

    private string GetFallbackInsightPrompt(string query, QueryResult result)
    {
        var dataPreview = GenerateDataPreview(result.Data);
        return $@"Analyze this query result and provide insights:

Query: {query}
Data: {dataPreview}

Provide 2-3 key business insights.";
    }

    private string GetFallbackVisualizationPrompt(string query, QueryResult result)
    {
        var columnInfo = string.Join(", ", result.Metadata.Columns.Select(c => $"{c.Name} ({c.DataType})"));
        return $@"Suggest visualization for:

Query: {query}
Columns: {columnInfo}
Rows: {result.Metadata.RowCount}

Return JSON with visualization config.";
    }

    private PromptTemplate MapToPromptTemplate(PromptTemplateEntity entity)
    {
        var parameters = new Dictionary<string, object>();
        if (!string.IsNullOrEmpty(entity.Parameters))
        {
            try
            {
                parameters = JsonSerializer.Deserialize<Dictionary<string, object>>(entity.Parameters)
                    ?? new Dictionary<string, object>();
            }
            catch { /* Ignore deserialization errors */ }
        }

        return new PromptTemplate
        {
            Name = entity.Name,
            Version = entity.Version,
            Content = entity.Content,
            Description = entity.Description,
            IsActive = entity.IsActive,
            CreatedBy = entity.CreatedBy ?? "Unknown",
            CreatedDate = entity.CreatedDate,
            UsageCount = entity.UsageCount,
            Parameters = parameters
        };
    }

    private async Task IncrementUsageCountAsync(long templateId)
    {
        try
        {
            var entity = await _context.PromptTemplates.FindAsync(templateId);
            if (entity != null)
            {
                entity.UsageCount++;
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to increment usage count for template: {TemplateId}", templateId);
        }
    }

    private async Task LogPromptDetailsAsync(string promptType, string userQuery, string fullPrompt, object metadata)
    {
        try
        {
            var promptLog = new PromptLogEntity
            {
                PromptType = promptType,
                UserQuery = userQuery,
                FullPrompt = fullPrompt,
                Metadata = JsonSerializer.Serialize(metadata),
                CreatedDate = DateTime.UtcNow,
                PromptLength = fullPrompt.Length
            };

            _context.PromptLogs.Add(promptLog);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging prompt details for type: {PromptType}", promptType);
        }
    }
}
