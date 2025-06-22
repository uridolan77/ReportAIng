using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Interfaces.Analytics;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Security;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text;
using Microsoft.Data.SqlClient;
using CoreModels = BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.AI.Management;

public class PromptService : IPromptService
{
    private readonly ILogger<PromptService> _logger;
    private readonly BICopilotContext _context;
    private readonly ISecretsManagementService _secretsService;
    private readonly ISemanticLayerService? _semanticLayerService;
    private readonly IPromptGenerationLogsService? _promptLogsService;
    private readonly ITokenUsageAnalyticsService? _tokenAnalyticsService;

    public PromptService(
        ILogger<PromptService> logger,
        BICopilotContext context,
        ISecretsManagementService secretsService,
        ISemanticLayerService? semanticLayerService = null,
        IPromptGenerationLogsService? promptLogsService = null,
        ITokenUsageAnalyticsService? tokenAnalyticsService = null)
    {
        _logger = logger;
        _context = context;
        _secretsService = secretsService;
        _semanticLayerService = semanticLayerService;
        _promptLogsService = promptLogsService;
        _tokenAnalyticsService = tokenAnalyticsService;
    }

    public async Task<string> BuildQueryPromptAsync(string naturalLanguageQuery, SchemaMetadata schema, string? context = null)
    {
        try
        {
            var template = await GetPromptTemplateAsync("sql_generation");

            // Try to use semantic layer for enhanced schema description
            string schemaDescription;
            if (_semanticLayerService != null)
            {
                try
                {
                    _logger.LogInformation("🧠 Using semantic layer for schema description");
                    schemaDescription = await _semanticLayerService.GetBusinessFriendlySchemaAsync(naturalLanguageQuery, 1500);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "⚠️ Semantic layer failed, falling back to traditional schema description");
                    schemaDescription = await BuildEnhancedSchemaDescriptionAsync(schema);
                }
            }
            else
            {
                _logger.LogDebug("📋 Using traditional schema description (semantic layer not available)");
                schemaDescription = await BuildEnhancedSchemaDescriptionAsync(schema);
            }

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
                ContextProvided = !string.IsNullOrEmpty(context),
                SemanticLayerUsed = _semanticLayerService != null
            });

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building query prompt");
            return await GetFallbackQueryPromptAsync(naturalLanguageQuery, schema, context);
        }
    }

    public async Task<PromptDetails> BuildDetailedQueryPromptAsync(string naturalLanguageQuery, SchemaMetadata schema, string? context = null)
    {
        try
        {
            _logger.LogInformation("🔍 Building detailed prompt for query: {Query}", naturalLanguageQuery);
            var template = await GetPromptTemplateAsync("sql_generation");
            _logger.LogInformation("📋 Retrieved template: {TemplateName} v{Version}, Content length: {ContentLength}",
                template.Name, template.Version, template.Content?.Length ?? 0);

            var schemaDescription = await BuildEnhancedSchemaDescriptionAsync(schema);
            var businessRules = GetBusinessRulesForQuery(naturalLanguageQuery);
            var exampleQueries = GetRelevantExampleQueries(naturalLanguageQuery);
            var contextInfo = !string.IsNullOrEmpty(context) ? $"\nAdditional context: {context}" : "";

            var prompt = (template.Content ?? "")
                .Replace("{schema}", schemaDescription)
                .Replace("{question}", naturalLanguageQuery)
                .Replace("{context}", contextInfo)
                .Replace("{business_rules}", businessRules)
                .Replace("{examples}", exampleQueries);

            var sections = new List<PromptSection>
            {
                new PromptSection
                {
                    Name = "template",
                    Title = "Base Template",
                    Content = template.Content,
                    Type = "template",
                    Order = 1,
                    Metadata = new Dictionary<string, object>
                    {
                        ["templateName"] = template.Name,
                        ["templateVersion"] = template.Version,
                        ["description"] = template.Description ?? ""
                    }
                },
                new PromptSection
                {
                    Name = "user_question",
                    Title = "User Question",
                    Content = naturalLanguageQuery,
                    Type = "user_input",
                    Order = 2
                },
                new PromptSection
                {
                    Name = "schema",
                    Title = "Database Schema",
                    Content = schemaDescription,
                    Type = "schema",
                    Order = 3,
                    Metadata = new Dictionary<string, object>
                    {
                        ["tableCount"] = schema.Tables?.Count ?? 0,
                        ["totalColumns"] = schema.Tables?.Sum(t => t.Columns?.Count ?? 0) ?? 0
                    }
                },
                new PromptSection
                {
                    Name = "business_rules",
                    Title = "Business Rules",
                    Content = businessRules,
                    Type = "business_rules",
                    Order = 4
                },
                new PromptSection
                {
                    Name = "examples",
                    Title = "Example Queries",
                    Content = exampleQueries,
                    Type = "examples",
                    Order = 5
                }
            };

            if (!string.IsNullOrEmpty(contextInfo))
            {
                sections.Add(new PromptSection
                {
                    Name = "context",
                    Title = "Additional Context",
                    Content = contextInfo,
                    Type = "context",
                    Order = 6
                });
            }

            var variables = new Dictionary<string, string>
            {
                ["{schema}"] = schemaDescription,
                ["{question}"] = naturalLanguageQuery,
                ["{business_rules}"] = businessRules,
                ["{examples}"] = exampleQueries,
                ["{context}"] = contextInfo
            };

            var promptDetails = new PromptDetails
            {
                FullPrompt = prompt,
                TemplateName = template.Name,
                TemplateVersion = template.Version,
                Sections = sections.ToArray(),
                Variables = variables,
                TokenCount = EstimateTokenCount(prompt),
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogInformation("✅ Prompt details created successfully: Template={TemplateName}, Sections={SectionCount}, TokenCount={TokenCount}",
                promptDetails.TemplateName, promptDetails.Sections.Length, promptDetails.TokenCount);

            // Log prompt generation analytics
            await LogPromptGenerationAsync(naturalLanguageQuery, promptDetails, schema, context);

            return promptDetails;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building detailed query prompt, using fallback");
            var fallbackPromptDetails = new PromptDetails
            {
                FullPrompt = await GetFallbackQueryPromptAsync(naturalLanguageQuery, schema, context),
                TemplateName = "fallback",
                TemplateVersion = "1.0",
                Sections = new[]
                {
                    new PromptSection
                    {
                        Name = "fallback",
                        Title = "Fallback Prompt",
                        Content = await GetFallbackQueryPromptAsync(naturalLanguageQuery, schema, context),
                        Type = "fallback",
                        Order = 1
                    }
                },
                Variables = new Dictionary<string, string>(),
                TokenCount = 0,
                GeneratedAt = DateTime.UtcNow
            };

            _logger.LogWarning("⚠️ Using fallback prompt details: Template={TemplateName}, Sections={SectionCount}",
                fallbackPromptDetails.TemplateName, fallbackPromptDetails.Sections.Length);

            return fallbackPromptDetails;
        }
    }

    public async Task<string> BuildInsightPromptAsync(string query, QueryResult result)
    {
        try
        {
            var template = await GetPromptTemplateAsync("insight_generation");

            var dataPreview = GenerateDataPreview(result.Data);
            var columnInfo = string.Join(", ", result.Metadata.Columns.Select(c => $"{c.Name} ({c.DataType})"));

            var prompt = (template.Content ?? "")
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

            var prompt = (template.Content ?? "")
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

            // If not found and looking for sql_generation, try the legacy name
            if (entity == null && templateName == "sql_generation")
            {
                _logger.LogInformation("Template 'sql_generation' not found, trying legacy name 'BasicQueryGeneration'");
                entity = await _context.PromptTemplates
                    .Where(t => t.Name == "BasicQueryGeneration" && t.IsActive)
                    .OrderByDescending(t => t.CreatedDate)
                    .FirstOrDefaultAsync();
            }

            if (entity != null)
            {
                await IncrementUsageCountAsync(entity.Id);
                return MapToPromptTemplate(entity);
            }

            // Return default template if not found
            _logger.LogWarning("No template found for {TemplateName}, using default", templateName);
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
                .Where(q => q.ExecutionTimeMs > 0)
                .Select(q => q.ExecutionTimeMs)
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

    private async Task<string> BuildEnhancedSchemaDescriptionAsync(SchemaMetadata schema)
    {
        var description = new List<string>();

        // Use all tables from the schema (should already be filtered by ContextManager)
        var tablesToProcess = schema.Tables ?? new List<TableMetadata>();

        // Debug logging to see what schema we're actually using
        _logger.LogInformation("🔍 SCHEMA DEBUG - Building schema description with {TableCount} tables: {TableNames}",
            tablesToProcess.Count, string.Join(", ", tablesToProcess.Select(t => t.Name)));

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

            // Debug logging for tbl_Daily_actions specifically
            if (table.Name.ToLowerInvariant().Contains("daily_actions") && !table.Name.ToLowerInvariant().Contains("players"))
            {
                _logger.LogInformation("🔍 SCHEMA DEBUG - tbl_Daily_actions columns: {ColumnNames}",
                    string.Join(", ", table.Columns.Select(c => c.Name)));
            }

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

                // Add field values for specific columns (dynamically discovered)
                var fieldValues = await GetDynamicFieldValuesAsync(table.Name, column.Name);
                if (!string.IsNullOrEmpty(fieldValues))
                {
                    columnDesc += $" - Valid values: {fieldValues}";
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
            "tbl_daily_actions_games" => "Game-specific daily statistics table holding gaming metrics by player by game by day. Contains RealBetAmount, RealWinAmount, BonusBetAmount, BonusWinAmount, NetGamingRevenue, and session counts.",
            "games" => "Games master table containing game metadata including GameName, Provider, SubProvider, GameType. Join with tbl_Daily_actions_games using GameID - 1000000.",
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
            "status" => "Current player status - ONLY 'Active' or 'Blocked' are valid (use 'Blocked' for suspended players)",
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

    /// <summary>
    /// Gets the valid field values for specific table columns to help AI generate accurate SQL
    /// </summary>
    private string GetColumnFieldValues(string tableName, string columnName)
    {
        var tableKey = tableName.ToLower();
        var columnKey = columnName.ToLower();

        // Player status values - CRITICAL: Only Active and Blocked are valid in this database
        if (columnKey == "status" && tableKey.Contains("player"))
        {
            return "'Active', 'Blocked'";
        }

        // Bonus status values
        if (columnKey == "status" && tableKey.Contains("bonus"))
        {
            return "'Active', 'Expired', 'Used', 'Cancelled', 'Pending'";
        }

        // Transaction status values
        if (columnKey == "status" && tableKey.Contains("transaction"))
        {
            return "'Completed', 'Pending', 'Failed', 'Cancelled', 'Processing'";
        }

        // Payment method types
        if (columnKey.Contains("paymentmethod") || columnKey.Contains("payment_method"))
        {
            return "'CreditCard', 'Neteller', 'MoneyBookers', 'Skrill', 'PayPal', 'BankTransfer', 'Crypto', 'Other'";
        }

        // Action types for daily actions
        if (columnKey == "actiontype" || columnKey == "action_type")
        {
            return "'Deposit', 'Withdrawal', 'Bet', 'Win', 'Bonus', 'Registration', 'Login'";
        }

        // Game types
        if (columnKey == "gametype" || columnKey == "game_type")
        {
            return "'Slot', 'Table', 'Live', 'Sports', 'Virtual', 'Scratch', 'Bingo', 'Keno', 'Poker'";
        }

        // Currency codes (common ones)
        if (columnKey.Contains("currency") && !columnKey.Contains("id"))
        {
            return "'USD', 'EUR', 'GBP', 'CAD', 'AUD', 'SEK', 'NOK', 'DKK', 'CHF', 'JPY'";
        }

        // Boolean-like fields
        if (columnKey.Contains("active") || columnKey.Contains("enabled") || columnKey.Contains("verified"))
        {
            return "0 (false), 1 (true)";
        }

        // Gender field
        if (columnKey == "gender" || columnKey == "sex")
        {
            return "'M', 'F', 'Male', 'Female', 'Other', 'Unknown'";
        }

        // VIP levels
        if (columnKey.Contains("vip") || columnKey.Contains("level") || columnKey.Contains("tier"))
        {
            return "'Bronze', 'Silver', 'Gold', 'Platinum', 'Diamond', 'VIP1', 'VIP2', 'VIP3'";
        }

        // Risk levels
        if (columnKey.Contains("risk"))
        {
            return "'Low', 'Medium', 'High', 'Critical'";
        }

        // Account types
        if (columnKey.Contains("accounttype") || columnKey == "type")
        {
            return "'Real', 'Demo', 'Bonus', 'Test'";
        }

        // Verification status
        if (columnKey.Contains("verification") || columnKey.Contains("kyc"))
        {
            return "'Verified', 'Pending', 'Rejected', 'NotRequired'";
        }

        // Common providers (for games)
        if (columnKey == "provider" && tableKey.Contains("game"))
        {
            return "'NetEnt', 'Microgaming', 'Pragmatic Play', 'Evolution', 'Playtech', 'Yggdrasil', 'Play n GO', 'Red Tiger'";
        }

        // Language codes
        if (columnKey.Contains("language") || columnKey.Contains("locale"))
        {
            return "'en', 'de', 'fr', 'es', 'it', 'sv', 'no', 'da', 'fi'";
        }

        return ""; // No specific values defined for this column
    }

    /// <summary>
    /// Dynamically discovers field values from the database for better AI context
    /// Only queries for columns that are likely to have limited distinct values
    /// </summary>
    private async Task<string> GetDynamicFieldValuesAsync(string tableName, string columnName)
    {
        try
        {
            // Only check specific columns that are likely to have limited distinct values
            var columnKey = columnName.ToLower();
            var shouldCheckValues = columnKey == "status" ||
                                  columnKey.Contains("type") ||
                                  columnKey.Contains("method") ||
                                  columnKey.Contains("category") ||
                                  columnKey.Contains("level") ||
                                  columnKey.Contains("tier") ||
                                  columnKey == "gender" ||
                                  columnKey == "currency" ||
                                  columnKey == "provider";

            if (!shouldCheckValues)
            {
                return "";
            }

            var connectionString = await _secretsService.GetSecretAsync("DailyActionsDB--ConnectionString");
            if (string.IsNullOrEmpty(connectionString))
            {
                return "";
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Query to get distinct values for the column (limited to reasonable number)
            var sql = $@"
                SELECT DISTINCT TOP 10 [{columnName}]
                FROM [common].[{tableName}] WITH (NOLOCK)
                WHERE [{columnName}] IS NOT NULL
                  AND [{columnName}] != ''
                  AND LEN(CAST([{columnName}] AS NVARCHAR(MAX))) < 50
                ORDER BY [{columnName}]";

            using var command = new SqlCommand(sql, connection);
            var values = new List<string>();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync() && values.Count < 10)
            {
                var value = reader[columnName]?.ToString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    values.Add($"'{value}'");
                }
            }

            if (values.Count > 0)
            {
                _logger.LogInformation("🔍 Dynamic field values for {Table}.{Column}: {Values}",
                    tableName, columnName, string.Join(", ", values));
                return string.Join(", ", values);
            }

            return "";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get dynamic field values for {Table}.{Column}", tableName, columnName);
            return "";
        }
    }

    private string GetBusinessRulesForQuery(string naturalLanguageQuery)
    {
        var query = naturalLanguageQuery.ToLower();
        var rules = new List<string>();

        // Game-specific business rules
        if (IsGameRelatedQuery(query))
        {
            rules.Add("CRITICAL: For game analytics, use tbl_Daily_actions_games as the primary table");
            rules.Add("CRITICAL: ALWAYS join with Games table using: INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000");
            rules.Add("CRITICAL: NEVER show GameID numbers - ALWAYS show GameName from Games table");
            rules.Add("Games table contains: GameName, Provider, SubProvider, GameType - use these for readable results");
            rules.Add("Gaming metrics: RealBetAmount, RealWinAmount, BonusBetAmount, BonusWinAmount, NetGamingRevenue");
            rules.Add("Count metrics: NumberofRealBets, NumberofBonusBets, NumberofSessions, NumberofRealWins, NumberofBonusWins");
            rules.Add("For game analysis: SELECT g.GameName, g.Provider, g.GameType and GROUP BY these fields");
            rules.Add("Use SUM() for all gaming amount and count metrics");
            rules.Add("NetGamingRevenue = total bets - total wins (house edge calculation)");
            rules.Add("MANDATORY: Include Games table join for any query mentioning 'games', 'top games', or 'game performance'");
        }

        if (query.Contains("today"))
        {
            rules.Add("For 'today': Use WHERE Date = CAST(GETDATE() AS DATE) or WHERE GameDate = CAST(GETDATE() AS DATE)");
            rules.Add("Focus on tbl_Daily_actions as the primary source for today's data");
        }

        if (query.Contains("total") || query.Contains("sum"))
        {
            rules.Add("For 'totals': Use SUM() aggregation on Amount columns");
            rules.Add("Consider grouping by relevant dimensions (PlayerID, WhitelabelID, CountryID)");
        }

        if (query.Contains("last") && (query.Contains("day") || query.Contains("week")))
        {
            if (query.Contains("7 days") || query.Contains("week"))
            {
                rules.Add("For 'last 7 days' or 'last week': Use WHERE Date >= DATEADD(day, -7, CAST(GETDATE() AS DATE))");
            }
            else if (query.Contains("30 days") || query.Contains("month"))
            {
                rules.Add("For 'last 30 days' or 'last month': Use WHERE Date >= DATEADD(day, -30, CAST(GETDATE() AS DATE))");
            }
            else if (query.Contains("3 days"))
            {
                rules.Add("For 'last 3 days': Use WHERE Date >= DATEADD(day, -3, CAST(GETDATE() AS DATE))");
            }
            rules.Add("Always use Date column from tbl_Daily_actions or GameDate from tbl_Daily_actions_games for date filtering");
        }

        if (query.Contains("deposit"))
        {
            rules.Add("CRITICAL: For deposit queries, ALWAYS use 'Deposits' column from tbl_Daily_actions, NEVER use 'Amount'");
            rules.Add("CRITICAL: The column name is 'Deposits' (capital D), not 'deposits' or 'Amount'");
            rules.Add("For deposit totals: SUM(da.Deposits) - use this exact syntax");
            rules.Add("Deposits are financial amounts in the player's currency");
        }

        if (query.Contains("top") && query.Contains("player"))
        {
            rules.Add("For 'top players' queries: Use ORDER BY with the relevant metric DESC and LIMIT/TOP clause");
            rules.Add("Join tbl_Daily_actions with tbl_Daily_actions_players for player names when needed");
            rules.Add("Use SUM() aggregation for deposit amounts when showing top players by deposits");
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

        // Field value rules
        if (query.Contains("status") || query.Contains("active") || query.Contains("suspended") || query.Contains("blocked"))
        {
            rules.Add("CRITICAL: For player status queries, only 'Active' and 'Blocked' are valid values in this database");
            rules.Add("CRITICAL: When user asks for 'suspended' players, use 'Blocked' status instead");
            rules.Add("CRITICAL: Status values are case-sensitive strings: 'Active' or 'Blocked' only");
            rules.Add("CRITICAL: Use the exact values shown in the schema - they may vary by table");
            rules.Add("Example: WHERE Status = 'Blocked' (for suspended/blocked players)");
            rules.Add("Example: WHERE Status = 'Active' (for active players)");
        }

        if (query.Contains("payment") || query.Contains("method"))
        {
            rules.Add("For payment methods: Use 'CreditCard', 'Neteller', 'MoneyBookers', 'Skrill', 'PayPal', 'BankTransfer', 'Crypto', 'Other'");
        }

        // Terminology mapping rules
        if (query.Contains("suspended") || query.Contains("suspend"))
        {
            rules.Add("TERMINOLOGY MAPPING: 'Suspended' players should be queried using Status = 'Blocked'");
            rules.Add("CRITICAL: Never use 'Suspended' as a status value - it doesn't exist in this database");
            rules.Add("CRITICAL: Replace any reference to 'suspended' with 'Blocked' in WHERE clauses");
        }

        if (query.Contains("game") && (query.Contains("type") || query.Contains("category")))
        {
            rules.Add("For game types: Use 'Slot', 'Table', 'Live', 'Sports', 'Virtual', 'Scratch', 'Bingo', 'Keno', 'Poker'");
        }

        // Default rules if no specific patterns found
        if (rules.Count == 0)
        {
            rules.Add("Use tbl_Daily_actions as primary table for statistical queries");
            rules.Add("JOIN with reference tables (players, countries, currencies, whitelabels) for context");
            rules.Add("Use appropriate aggregations (SUM, COUNT, AVG) based on the question");
            rules.Add("CRITICAL: Always use exact field values as shown in schema - they are case-sensitive");
        }

        return string.Join("\n- ", rules);
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

    private int EstimateTokenCount(string text)
    {
        // Simple token estimation: roughly 4 characters per token
        return (int)Math.Ceiling(text.Length / 4.0);
    }

    private string GetRelevantExampleQueries(string naturalLanguageQuery)
    {
        var query = naturalLanguageQuery.ToLower();
        var examples = new List<string>();

        // Game-specific examples
        if (IsGameRelatedQuery(query))
        {
            examples.Add(@"
EXAMPLE: 'game performance by provider'
SQL: SELECT g.Provider, g.SubProvider, g.GameType,
            SUM(dag.RealBetAmount) AS RealBetAmount,
            SUM(dag.RealWinAmount) AS RealWinAmount,
            SUM(dag.NetGamingRevenue) AS NetGamingRevenue,
            SUM(dag.NumberofRealBets) AS NumberofRealBets
     FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
     INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
     WHERE dag.GameDate >= '2025-01-01'
     GROUP BY g.Provider, g.SubProvider, g.GameType
     ORDER BY NetGamingRevenue DESC");

            examples.Add(@"
EXAMPLE: 'top games by revenue'
SQL: SELECT TOP 10 g.GameName, g.Provider, g.GameType,
            SUM(dag.RealBetAmount) AS TotalBets,
            SUM(dag.RealWinAmount) AS TotalWins,
            SUM(dag.NetGamingRevenue) AS NetRevenue
     FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
     INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
     WHERE dag.GameDate >= DATEADD(day, -7, CAST(GETDATE() AS DATE))
     GROUP BY g.GameName, g.Provider, g.GameType
     ORDER BY NetRevenue DESC");

            examples.Add(@"
EXAMPLE: 'top 10 games by revenue this month'
SQL: SELECT TOP 10 g.GameName, g.Provider, g.GameType,
            SUM(dag.NetGamingRevenue) AS TotalRevenue,
            SUM(dag.RealBetAmount) AS TotalBets,
            SUM(dag.NumberofSessions) AS TotalSessions
     FROM [DailyActionsDB].[common].[tbl_Daily_actions_games] dag WITH (NOLOCK)
     INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
     WHERE dag.GameDate >= DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()), 0)
       AND dag.GameDate < DATEADD(MONTH, DATEDIFF(MONTH, 0, GETDATE()) + 1, 0)
     GROUP BY g.GameName, g.Provider, g.GameType
     ORDER BY TotalRevenue DESC");
        }

        if (query.Contains("total") && query.Contains("today"))
        {
            examples.Add(@"
EXAMPLE: 'totals today'
SQL: SELECT SUM(Deposits) as TotalDeposits, COUNT(DISTINCT PlayerID) as PlayerCount
     FROM common.tbl_Daily_actions
     WHERE Date = CAST(GETDATE() AS DATE)");
        }

        if (query.Contains("top") && query.Contains("player") && query.Contains("deposit"))
        {
            examples.Add(@"
EXAMPLE: 'Top 10 players by deposits in the last 7 days'
SQL: SELECT TOP 10 da.PlayerID, SUM(da.Deposits) as TotalDeposits
     FROM common.tbl_Daily_actions da WITH (NOLOCK)
     WHERE da.Date >= DATEADD(day, -7, CAST(GETDATE() AS DATE))
     GROUP BY da.PlayerID
     ORDER BY TotalDeposits DESC");
        }

        if (query.Contains("deposit") && (query.Contains("brand") || query.Contains("whitelabel")))
        {
            examples.Add(@"
EXAMPLE: 'deposits by brand today'
SQL: SELECT da.WhiteLabelID, SUM(da.Deposits) as TotalDeposits, COUNT(DISTINCT da.PlayerID) as PlayerCount
     FROM common.tbl_Daily_actions da WITH (NOLOCK)
     WHERE da.Date = CAST(GETDATE() AS DATE)
     GROUP BY da.WhiteLabelID
     ORDER BY TotalDeposits DESC");
        }

        if (query.Contains("player") && query.Contains("today"))
        {
            examples.Add(@"
EXAMPLE: 'player activity today'
SQL: SELECT da.PlayerID, p.Username, SUM(da.Deposits) as TotalDeposits, SUM(da.BetsReal + da.BetsBonus) as TotalBets
     FROM common.tbl_Daily_actions da WITH (NOLOCK)
     JOIN common.tbl_Daily_actions_players p WITH (NOLOCK) ON da.PlayerID = p.PlayerID
     WHERE da.Date = CAST(GETDATE() AS DATE)
     GROUP BY da.PlayerID, p.Username
     ORDER BY TotalDeposits DESC");
        }

        if (query.Contains("country") || query.Contains("region"))
        {
            examples.Add(@"
EXAMPLE: 'revenue by country today'
SQL: SELECT c.CountryName, SUM(da.Deposits) as Revenue, COUNT(DISTINCT da.PlayerID) as Players
     FROM common.tbl_Daily_actions da WITH (NOLOCK)
     JOIN common.tbl_Daily_actions_players p WITH (NOLOCK) ON da.PlayerID = p.PlayerID
     JOIN common.tbl_Countries c WITH (NOLOCK) ON p.CountryID = c.CountryID
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
     FROM common.tbl_Bonus_balances WITH (NOLOCK)
     WHERE Status = 'Active'");
        }

        if (query.Contains("brand") || query.Contains("whitelabel"))
        {
            examples.Add(@"
EXAMPLE: 'revenue by brand today'
SQL: SELECT w.Name as BrandName, SUM(da.Deposits) as Revenue, COUNT(DISTINCT da.PlayerID) as Players
     FROM common.tbl_Daily_actions da WITH (NOLOCK)
     JOIN common.tbl_Whitelabels w WITH (NOLOCK) ON da.WhitelabelID = w.WhitelabelID
     WHERE da.Date = CAST(GETDATE() AS DATE)
     GROUP BY w.Name
     ORDER BY Revenue DESC");
        }

        // Status-specific examples - CRITICAL for suspended/blocked mapping
        if (query.Contains("status") || query.Contains("suspended") || query.Contains("blocked"))
        {
            examples.Add(@"
EXAMPLE: 'suspended players by brand'
SQL: SELECT wl.LabelName AS Brand, COUNT(DISTINCT dap.PlayerID) AS BlockedPlayers
     FROM common.tbl_Daily_actions_players dap WITH (NOLOCK)
     JOIN common.tbl_White_labels wl WITH (NOLOCK) ON dap.CasinoID = wl.LabelID
     WHERE dap.Status = 'Blocked'
     GROUP BY wl.LabelName
     ORDER BY BlockedPlayers DESC");

            examples.Add(@"
EXAMPLE: 'players by status last 7 days'
SQL: SELECT dap.Status, COUNT(DISTINCT dap.PlayerID) AS PlayerCount
     FROM common.tbl_Daily_actions da WITH (NOLOCK)
     JOIN common.tbl_Daily_actions_players dap WITH (NOLOCK) ON da.PlayerID = dap.PlayerID
     WHERE da.Date >= DATEADD(DAY, -7, CAST(GETDATE() AS DATE))
     GROUP BY dap.Status
     ORDER BY PlayerCount DESC");
        }

        if (examples.Count == 0)
        {
            examples.Add(@"
EXAMPLE: 'basic daily statistics'
SQL: SELECT Date, SUM(Deposits) as TotalDeposits, COUNT(DISTINCT PlayerID) as PlayerCount
     FROM common.tbl_Daily_actions WITH (NOLOCK)
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
3. CRITICAL: For deposits, ALWAYS use 'Deposits' column, NEVER 'Amount'
4. CRITICAL: For game analytics, use tbl_Daily_actions_games and ALWAYS join with Games table
5. CRITICAL: Games table join: INNER JOIN dbo.Games g WITH (NOLOCK) ON g.GameID = dag.GameID - 1000000
6. CRITICAL: For game queries, NEVER show GameID - ALWAYS show g.GameName, g.Provider, g.GameType
7. Include appropriate WHERE clauses for filtering (Date or GameDate)
8. Use JOINs when querying multiple tables based on foreign key relationships
9. Add meaningful column aliases (e.g., SUM(RealBetAmount) AS TotalBets)
10. Add ORDER BY for logical sorting (usually by date DESC or amount DESC)
11. Use SELECT TOP 100 at the beginning to limit results (NEVER put TOP at the end)
12. Return only the SQL query without explanations or markdown formatting
13. Ensure all referenced columns exist in the schema
14. Always add WITH (NOLOCK) hint to all table references for better read performance
15. Format table hints as: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints
16. For game queries: SELECT g.GameName, g.Provider, g.GameType and GROUP BY these fields for readable results

CORRECT SQL STRUCTURE:
SELECT TOP 100 column1, column2, column3
FROM table1 t1 WITH (NOLOCK)
JOIN table2 t2 WITH (NOLOCK) ON t1.id = t2.id
WHERE condition
ORDER BY column1 DESC",
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

    private async Task<string> GetFallbackQueryPromptAsync(string naturalLanguageQuery, SchemaMetadata schema, string? context)
    {
        var schemaDescription = await BuildEnhancedSchemaDescriptionAsync(schema);
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
5. Limits results with SELECT TOP 100 at the beginning (NEVER put TOP at the end)
6. Uses clear column aliases
7. Always adds WITH (NOLOCK) hint to all table references for better read performance
8. Formats as: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints

CORRECT STRUCTURE: SELECT TOP 100 columns FROM table WITH (NOLOCK) WHERE condition ORDER BY column

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

    private PromptTemplate MapToPromptTemplate(BIReportingCopilot.Core.Models.PromptTemplateEntity entity)
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

    /// <summary>
    /// Build SQL generation prompt for streaming operations
    /// </summary>
    public async Task<string> BuildSQLGenerationPromptAsync(string prompt, SchemaMetadata? schema = null, CoreModels.QueryContext? context = null)
    {
        try
        {
            _logger.LogDebug("Building SQL generation prompt for streaming: {Prompt}", prompt);

            if (schema != null)
            {
                // Use the existing detailed prompt building logic
                var contextInfo = context?.BusinessDomain ?? "";
                return await BuildQueryPromptAsync(prompt, schema, contextInfo);
            }

            // Fallback for when no schema is provided
            var template = await GetPromptTemplateAsync("sql_generation");
            var businessDomain = context?.BusinessDomain ?? "";
            var enhancedPrompt = template.Content
                .Replace("{schema}", "No schema information available")
                .Replace("{question}", prompt)
                .Replace("{context}", businessDomain)
                .Replace("{business_rules}", GetBusinessRulesForQuery(prompt))
                .Replace("{examples}", GetRelevantExampleQueries(prompt));

            return enhancedPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building SQL generation prompt");
            return $"Generate SQL for: {prompt}";
        }
    }

    /// <summary>
    /// Build insight generation prompt for streaming operations
    /// </summary>
    public async Task<string> BuildInsightGenerationPromptAsync(string query, object[] data, CoreModels.AnalysisContext? context = null)
    {
        try
        {
            _logger.LogDebug("Building insight generation prompt for query: {Query}", query);

            var template = await GetPromptTemplateAsync("insight_generation");
            var dataPreview = GenerateDataPreview(data);

            var prompt = template.Content
                .Replace("{query}", query)
                .Replace("{data_preview}", dataPreview)
                .Replace("{column_info}", "Column information not available")
                .Replace("{row_count}", data.Length.ToString());

            // Add context-specific information if available
            if (context != null)
            {
                var contextInfo = "";
                if (!string.IsNullOrEmpty(context.BusinessGoal))
                {
                    contextInfo += $"\nBusiness Goal: {context.BusinessGoal}";
                }
                if (!string.IsNullOrEmpty(context.TimeFrame))
                {
                    contextInfo += $"\nTime Frame: {context.TimeFrame}";
                }
                if (context.KeyMetrics.Any())
                {
                    contextInfo += $"\nKey Metrics: {string.Join(", ", context.KeyMetrics)}";
                }
                if (contextInfo.Length > 0)
                {
                    prompt += contextInfo;
                }
            }

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building insight generation prompt");
            return $"Analyze the following query and provide insights: {query}";
        }
    }

    /// <summary>
    /// Build SQL explanation prompt for streaming operations
    /// </summary>
    public async Task<string> BuildSQLExplanationPromptAsync(string sql, CoreModels.StreamingQueryComplexity complexity = CoreModels.StreamingQueryComplexity.Medium)
    {
        try
        {
            _logger.LogDebug("Building SQL explanation prompt for complexity: {Complexity}", complexity);

            var template = await GetPromptTemplateAsync("sql_explanation");

            // If template doesn't exist, create a default one
            if (template.Content.Contains("Default template content"))
            {
                template = GetDefaultSQLExplanationTemplate();
            }

            var complexityLevel = complexity switch
            {
                CoreModels.StreamingQueryComplexity.Simple => "basic",
                CoreModels.StreamingQueryComplexity.Medium => "intermediate",
                CoreModels.StreamingQueryComplexity.Complex => "advanced",
                _ => "intermediate"
            };

            var detailLevel = complexity switch
            {
                CoreModels.StreamingQueryComplexity.Simple => "Provide a simple, high-level explanation suitable for beginners.",
                CoreModels.StreamingQueryComplexity.Medium => "Provide a detailed explanation with moderate technical depth.",
                CoreModels.StreamingQueryComplexity.Complex => "Provide a comprehensive, technical explanation with advanced details.",
                _ => "Provide a balanced explanation with appropriate technical detail."
            };

            var prompt = template.Content
                .Replace("{sql}", sql)
                .Replace("{complexity_level}", complexityLevel)
                .Replace("{detail_level}", detailLevel);

            return prompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error building SQL explanation prompt");
            return $"Explain the following SQL query in {complexity.ToString().ToLower()} terms: {sql}";
        }
    }

    /// <summary>
    /// Get default SQL explanation template
    /// </summary>
    private PromptTemplate GetDefaultSQLExplanationTemplate()
    {
        return new PromptTemplate
        {
            Name = "sql_explanation",
            Version = "1.0",
            Content = @"Explain the following SQL query in {complexity_level} terms:

SQL Query:
{sql}

{detail_level}

Focus on:
1. What the query does (purpose and goal)
2. How it works (step-by-step breakdown)
3. Key components (tables, joins, conditions, aggregations)
4. Expected results and business meaning

Provide a clear, structured explanation that matches the requested complexity level.",
            Description = "Default SQL explanation template",
            IsActive = true,
            CreatedBy = "System"
        };
    }

    #region Missing Interface Method Implementations

    /// <summary>
    /// Get prompt async (IPromptService interface)
    /// </summary>
    public async Task<string> GetPromptAsync(string promptKey, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📋 Getting prompt for key: {PromptKey}", promptKey);

            var template = await GetPromptTemplateAsync(promptKey);
            return template.Content ?? GetDefaultPromptContent(promptKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting prompt for key: {PromptKey}", promptKey);
            return GetDefaultPromptContent(promptKey);
        }
    }

    /// <summary>
    /// Format prompt async (IPromptService interface)
    /// </summary>
    public async Task<string> FormatPromptAsync(string template, Dictionary<string, object> variables, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("🎨 Formatting prompt template with {VariableCount} variables", variables.Count);

            var formattedPrompt = template;

            foreach (var variable in variables)
            {
                var placeholder = $"{{{variable.Key}}}";
                var value = variable.Value?.ToString() ?? "";
                formattedPrompt = formattedPrompt.Replace(placeholder, value);
            }

            _logger.LogDebug("✅ Prompt formatted successfully");
            return formattedPrompt;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error formatting prompt template");
            return template; // Return original template on error
        }
    }

    #endregion

    #region Helper Methods for Interface Implementations

    private string GetDefaultPromptContent(string promptKey)
    {
        return promptKey switch
        {
            "sql_generation" => "Generate a SQL query for: {question}\nSchema: {schema}\nContext: {context}",
            "insight_generation" => "Analyze the following data and provide insights: {data}\nQuery: {query}",
            "visualization_generation" => "Suggest appropriate visualizations for: {data}\nColumns: {columns}",
            _ => "Process the following request: {input}"
        };
    }

    /// <summary>
    /// Get prompt async with parameters (IPromptService interface overload)
    /// </summary>
    public async Task<string> GetPromptAsync(string promptKey, Dictionary<string, object>? parameters = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var template = await GetPromptAsync(promptKey, cancellationToken);

            if (parameters != null && parameters.Any())
            {
                return await FormatPromptAsync(template, parameters, cancellationToken);
            }

            return template;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting prompt with parameters for key: {PromptKey}", promptKey);
            return GetDefaultPromptContent(promptKey);
        }
    }

    /// <summary>
    /// Save prompt async (IPromptService interface)
    /// </summary>
    public async Task SavePromptAsync(string promptKey, string template, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("💾 Saving prompt template for key: {PromptKey}", promptKey);

            var promptTemplate = new PromptTemplate
            {
                Name = promptKey,
                Version = "1.0",
                Content = template,
                Description = $"Saved prompt template for {promptKey}",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow,
                Parameters = new Dictionary<string, object>()
            };

            await CreatePromptTemplateAsync(promptTemplate);
            _logger.LogInformation("✅ Prompt template saved successfully for key: {PromptKey}", promptKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error saving prompt template for key: {PromptKey}", promptKey);
            throw;
        }
    }

    /// <summary>
    /// Get prompt keys async (IPromptService interface)
    /// </summary>
    public async Task<List<string>> GetPromptKeysAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔑 Getting all prompt keys");

            var templates = await GetPromptTemplatesAsync();
            var keys = templates.Select(t => t.Name).Distinct().ToList();

            _logger.LogInformation("✅ Retrieved {Count} prompt keys", keys.Count);
            return keys;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting prompt keys");
            return new List<string> { "sql_generation", "explanation", "validation" };
        }
    }

    #endregion

    #region Phase 3: Enhanced Validation and Self-Correction Prompts

    /// <summary>
    /// Phase 3: Enhanced prompt engineering for semantic validation and self-correction
    /// </summary>
    public async Task<string> BuildSemanticValidationPromptAsync(
        string sql,
        string originalQuery,
        EnhancedSchemaResult schemaContext,
        CancellationToken cancellationToken = default)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("You are an expert SQL semantic validator with deep understanding of business intelligence.");
        prompt.AppendLine("Analyze the following SQL query for semantic alignment with the business intent.");
        prompt.AppendLine();

        prompt.AppendLine($"Business Question: {originalQuery}");
        prompt.AppendLine();

        prompt.AppendLine("Generated SQL:");
        prompt.AppendLine("```sql");
        prompt.AppendLine(sql);
        prompt.AppendLine("```");
        prompt.AppendLine();

        // Add semantic context from enhanced schema
        if (schemaContext.RelevantTables.Any())
        {
            prompt.AppendLine("Business Context:");
            foreach (var table in schemaContext.RelevantTables.Take(3))
            {
                prompt.AppendLine($"- {table.TableName}: {table.BusinessPurpose}");

                if (table.Columns.Any())
                {
                    var keyColumns = table.Columns.Where(c => c.RelevanceScore > 0.7).Take(3);
                    foreach (var column in keyColumns)
                    {
                        prompt.AppendLine($"  • {column.ColumnName}: {column.BusinessMeaning}");
                    }
                }
            }
            prompt.AppendLine();
        }

        prompt.AppendLine("Validate the SQL for:");
        prompt.AppendLine("1. Semantic alignment with business intent");
        prompt.AppendLine("2. Correct use of business terminology");
        prompt.AppendLine("3. Appropriate aggregations and calculations");
        prompt.AppendLine("4. Missing business context or filters");
        prompt.AppendLine();

        prompt.AppendLine("Provide your analysis in JSON format:");
        prompt.AppendLine("{");
        prompt.AppendLine("  \"alignmentScore\": 0.0-1.0,");
        prompt.AppendLine("  \"alignmentReason\": \"explanation\",");
        prompt.AppendLine("  \"semanticIssues\": [\"issue1\", \"issue2\"],");
        prompt.AppendLine("  \"businessTermValidation\": {");
        prompt.AppendLine("    \"correctTerms\": [\"term1\"],");
        prompt.AppendLine("    \"incorrectTerms\": [\"term2\"],");
        prompt.AppendLine("    \"suggestions\": {\"incorrect\": \"correct\"}");
        prompt.AppendLine("  }");
        prompt.AppendLine("}");

        return prompt.ToString();
    }

    /// <summary>
    /// Build enhanced SQL correction prompt with semantic context
    /// </summary>
    public async Task<string> BuildSqlCorrectionPromptAsync(
        string sql,
        string originalQuery,
        List<string> validationIssues,
        EnhancedSchemaResult schemaContext,
        CancellationToken cancellationToken = default)
    {
        var prompt = new StringBuilder();

        prompt.AppendLine("You are an expert SQL developer specializing in business intelligence and data analytics.");
        prompt.AppendLine("Your task is to correct the following SQL query based on validation issues.");
        prompt.AppendLine();

        prompt.AppendLine($"Original Business Question: {originalQuery}");
        prompt.AppendLine();

        prompt.AppendLine("SQL Query with Issues:");
        prompt.AppendLine("```sql");
        prompt.AppendLine(sql);
        prompt.AppendLine("```");
        prompt.AppendLine();

        prompt.AppendLine("Validation Issues to Address:");
        foreach (var issue in validationIssues)
        {
            prompt.AppendLine($"❌ {issue}");
        }
        prompt.AppendLine();

        // Add enhanced schema context
        if (schemaContext.RelevantTables.Any())
        {
            prompt.AppendLine("Available Schema and Business Context:");
            foreach (var table in schemaContext.RelevantTables)
            {
                prompt.AppendLine($"📊 Table: {table.TableName}");
                prompt.AppendLine($"   Purpose: {table.BusinessPurpose}");

                if (table.Columns.Any())
                {
                    prompt.AppendLine("   Key Columns:");
                    foreach (var column in table.Columns.Where(c => c.RelevanceScore > 0.6).Take(5))
                    {
                        prompt.AppendLine($"   • {column.ColumnName} ({column.DataType}): {column.BusinessMeaning}");

                        if (column.NaturalLanguageAliases.Any())
                        {
                            prompt.AppendLine($"     Aliases: {string.Join(", ", column.NaturalLanguageAliases)}");
                        }
                    }
                }
                prompt.AppendLine();
            }
        }

        prompt.AppendLine("Correction Guidelines:");
        prompt.AppendLine("✅ Maintain the original business intent");
        prompt.AppendLine("✅ Use correct table and column names from the schema");
        prompt.AppendLine("✅ Apply appropriate business logic and filters");
        prompt.AppendLine("✅ Follow SQL best practices and optimization");
        prompt.AppendLine("✅ Ensure semantic alignment with business terminology");
        prompt.AppendLine();

        prompt.AppendLine("Provide the corrected SQL query:");

        return prompt.ToString();
    }

    #endregion

    #region Analytics Logging

    /// <summary>
    /// Log prompt generation analytics for tracking and analysis
    /// </summary>
    private async Task LogPromptGenerationAsync(string userQuestion, PromptDetails promptDetails, SchemaMetadata schema, string? context)
    {
        try
        {
            if (_promptLogsService == null)
            {
                _logger.LogDebug("Prompt logging service not available, skipping analytics logging");
                return;
            }

            // Classify intent and domain from the user question
            var (intentType, domain) = ClassifyUserQuestion(userQuestion);

            // Calculate generation time (approximate since we don't track it precisely)
            var generationTimeMs = 100; // Default estimate

            // Extract entities from the question (simple keyword extraction)
            var extractedEntities = ExtractEntitiesFromQuestion(userQuestion);

            // Create the log request
            var logRequest = new PromptGenerationLogRequest
            {
                UserId = "system", // TODO: Get from current user context
                UserQuestion = userQuestion,
                GeneratedPrompt = promptDetails.FullPrompt,
                IntentType = intentType,
                Domain = domain,
                ConfidenceScore = CalculateConfidenceScore(promptDetails, schema),
                TablesUsed = schema.Tables?.Count ?? 0,
                GenerationTimeMs = generationTimeMs,
                TemplateUsed = promptDetails.TemplateName,
                WasSuccessful = true,
                ExtractedEntities = JsonSerializer.Serialize(extractedEntities),
                TimeContext = !string.IsNullOrEmpty(context) ? JsonSerializer.Serialize(new { context }) : null,
                TokensUsed = promptDetails.TokenCount,
                CostEstimate = CalculateCostEstimate(promptDetails.TokenCount),
                ModelUsed = "gpt-4", // TODO: Get from configuration
                SessionId = Guid.NewGuid().ToString(), // TODO: Get from current session
                RequestId = Guid.NewGuid().ToString()
            };

            // Log the prompt generation
            var logId = await _promptLogsService.LogPromptGenerationAsync(logRequest);

            // Record token usage analytics
            if (_tokenAnalyticsService != null)
            {
                await _tokenAnalyticsService.RecordTokenUsageAsync(
                    logRequest.UserId,
                    "prompt_generation",
                    intentType,
                    promptDetails.TokenCount,
                    logRequest.CostEstimate ?? 0);
            }

            _logger.LogDebug("✅ Logged prompt generation analytics: LogId={LogId}, Intent={Intent}, Domain={Domain}, Tokens={Tokens}",
                logId, intentType, domain, promptDetails.TokenCount);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Failed to log prompt generation analytics - continuing without logging");
        }
    }

    /// <summary>
    /// Classify user question into intent type and domain
    /// </summary>
    private (string intentType, string domain) ClassifyUserQuestion(string userQuestion)
    {
        var lowerQuestion = userQuestion.ToLower();

        // Simple intent classification
        string intentType = "ANALYSIS";
        if (lowerQuestion.Contains("show") || lowerQuestion.Contains("list") || lowerQuestion.Contains("display"))
            intentType = "REPORTING";
        else if (lowerQuestion.Contains("compare") || lowerQuestion.Contains("vs") || lowerQuestion.Contains("versus"))
            intentType = "COMPARATIVE";
        else if (lowerQuestion.Contains("trend") || lowerQuestion.Contains("over time") || lowerQuestion.Contains("growth"))
            intentType = "TREND_ANALYSIS";
        else if (lowerQuestion.Contains("total") || lowerQuestion.Contains("sum") || lowerQuestion.Contains("count"))
            intentType = "AGGREGATION";

        // Simple domain classification
        string domain = "General";
        if (lowerQuestion.Contains("sales") || lowerQuestion.Contains("revenue") || lowerQuestion.Contains("customer"))
            domain = "Sales";
        else if (lowerQuestion.Contains("finance") || lowerQuestion.Contains("cost") || lowerQuestion.Contains("profit"))
            domain = "Finance";
        else if (lowerQuestion.Contains("product") || lowerQuestion.Contains("inventory") || lowerQuestion.Contains("stock"))
            domain = "Product";
        else if (lowerQuestion.Contains("employee") || lowerQuestion.Contains("hr") || lowerQuestion.Contains("staff"))
            domain = "HR";

        return (intentType, domain);
    }

    /// <summary>
    /// Extract entities from user question (simple keyword extraction)
    /// </summary>
    private List<object> ExtractEntitiesFromQuestion(string userQuestion)
    {
        var entities = new List<object>();
        var lowerQuestion = userQuestion.ToLower();

        // Extract common business entities
        var entityKeywords = new Dictionary<string, string>
        {
            ["sales"] = "metric",
            ["revenue"] = "metric",
            ["profit"] = "metric",
            ["customer"] = "entity",
            ["product"] = "entity",
            ["order"] = "entity",
            ["employee"] = "entity",
            ["date"] = "temporal",
            ["month"] = "temporal",
            ["year"] = "temporal",
            ["quarter"] = "temporal"
        };

        foreach (var keyword in entityKeywords)
        {
            if (lowerQuestion.Contains(keyword.Key))
            {
                entities.Add(new { name = keyword.Key, type = keyword.Value });
            }
        }

        return entities;
    }

    /// <summary>
    /// Calculate confidence score based on prompt quality indicators
    /// </summary>
    private decimal CalculateConfidenceScore(PromptDetails promptDetails, SchemaMetadata schema)
    {
        decimal score = 0.5m; // Base score

        // Boost score based on template quality
        if (!string.IsNullOrEmpty(promptDetails.TemplateName) && promptDetails.TemplateName != "fallback")
            score += 0.2m;

        // Boost score based on schema richness
        if (schema.Tables?.Count > 0)
            score += 0.1m;

        // Boost score based on prompt completeness
        if (promptDetails.Sections?.Length > 3)
            score += 0.1m;

        // Boost score based on token count (reasonable length)
        if (promptDetails.TokenCount > 100 && promptDetails.TokenCount < 4000)
            score += 0.1m;

        return Math.Min(1.0m, score);
    }

    /// <summary>
    /// Calculate estimated cost based on token count
    /// </summary>
    private decimal CalculateCostEstimate(int tokenCount)
    {
        // Rough estimate: $0.03 per 1K tokens for GPT-4
        return (decimal)tokenCount / 1000 * 0.03m;
    }

    #endregion
}
