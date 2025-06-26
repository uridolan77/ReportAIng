using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Text.Json;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Schema;

/// <summary>
/// Service for automatically populating business metadata for tables and columns
/// </summary>
public class BusinessMetadataPopulationService
{
    private readonly ILogger<BusinessMetadataPopulationService> _logger;
    private readonly BICopilotContext _context;
    private readonly IAIService _aiService;
    private readonly string _connectionString;

    public BusinessMetadataPopulationService(
        ILogger<BusinessMetadataPopulationService> logger,
        BICopilotContext context,
        IAIService aiService,
        IConfiguration configuration)
    {
        _logger = logger;
        _context = context;
        _aiService = aiService;
        _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentException("Connection string not found");
    }

    /// <summary>
    /// Populate business metadata for the specific relevant gaming tables
    /// </summary>
    public async Task<BusinessMetadataPopulationResult> PopulateRelevantTablesAsync(bool useAI = true, bool overwriteExisting = false)
    {
        _logger.LogInformation("🎯 Starting targeted population for relevant gaming tables");

        var result = new BusinessMetadataPopulationResult();

        // Define the specific tables from relevant_tables.md
        var relevantTables = new List<DatabaseTableInfo>
        {
            new() { SchemaName = "common", TableName = "tbl_Countries" },
            new() { SchemaName = "common", TableName = "tbl_Currencies" },
            new() { SchemaName = "common", TableName = "tbl_Daily_actions" },
            new() { SchemaName = "common", TableName = "tbl_Daily_actions_games" },
            new() { SchemaName = "common", TableName = "tbl_Daily_actions_players" },
            new() { SchemaName = "common", TableName = "tbl_Daily_actionsGBP_transactions" },
            new() { SchemaName = "common", TableName = "tbl_White_labels" },
            new() { SchemaName = "dbo", TableName = "Games" }
        };

        try
        {
            _logger.LogInformation("📊 Processing {TableCount} relevant gaming tables", relevantTables.Count);

            foreach (var table in relevantTables)
            {
                try
                {
                    var tableResult = await PopulateTableMetadataAsync(table, useAI, overwriteExisting);
                    result.ProcessedTables.Add(tableResult);

                    if (tableResult.Success)
                        result.SuccessCount++;
                    else
                        result.ErrorCount++;

                    _logger.LogInformation("✅ Processed relevant table: {Schema}.{Table} - Success: {Success}",
                        table.SchemaName, table.TableName, tableResult.Success);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing relevant table: {Schema}.{Table}", table.SchemaName, table.TableName);
                    result.ErrorCount++;
                    result.ProcessedTables.Add(new TableMetadataResult
                    {
                        SchemaName = table.SchemaName,
                        TableName = table.TableName,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            result.Success = true;
            _logger.LogInformation("🎉 Relevant tables metadata population completed: {Success} successful, {Errors} errors",
                result.SuccessCount, result.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fatal error during relevant tables metadata population");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Automatically populate business metadata for all tables in the database
    /// </summary>
    public async Task<BusinessMetadataPopulationResult> PopulateAllTablesAsync(bool useAI = true, bool overwriteExisting = false)
    {
        _logger.LogInformation("🚀 Starting automatic business metadata population");
        
        var result = new BusinessMetadataPopulationResult();
        
        try
        {
            // Get all tables from the database
            var tables = await GetDatabaseTablesAsync();
            _logger.LogInformation("📊 Found {TableCount} tables to analyze", tables.Count);

            foreach (var table in tables)
            {
                try
                {
                    var tableResult = await PopulateTableMetadataAsync(table, useAI, overwriteExisting);
                    result.ProcessedTables.Add(tableResult);
                    
                    if (tableResult.Success)
                        result.SuccessCount++;
                    else
                        result.ErrorCount++;
                        
                    _logger.LogInformation("✅ Processed table: {Schema}.{Table} - Success: {Success}", 
                        table.SchemaName, table.TableName, tableResult.Success);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing table: {Schema}.{Table}", table.SchemaName, table.TableName);
                    result.ErrorCount++;
                    result.ProcessedTables.Add(new TableMetadataResult
                    {
                        SchemaName = table.SchemaName,
                        TableName = table.TableName,
                        Success = false,
                        ErrorMessage = ex.Message
                    });
                }
            }

            result.Success = true;
            _logger.LogInformation("🎉 Metadata population completed: {Success} successful, {Errors} errors", 
                result.SuccessCount, result.ErrorCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Fatal error during metadata population");
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Populate metadata for a specific table
    /// </summary>
    public async Task<TableMetadataResult> PopulateTableMetadataAsync(DatabaseTableInfo table, bool useAI = true, bool overwriteExisting = false)
    {
        var result = new TableMetadataResult
        {
            SchemaName = table.SchemaName,
            TableName = table.TableName
        };

        try
        {
            // Check if table metadata already exists
            var existingTable = await _context.BusinessTableInfo
                .Include(t => t.Columns)
                .FirstOrDefaultAsync(t => t.SchemaName == table.SchemaName && t.TableName == table.TableName);

            if (existingTable != null && !overwriteExisting)
            {
                _logger.LogInformation("⏭️ Skipping existing table: {Schema}.{Table}", table.SchemaName, table.TableName);
                result.Success = true;
                result.Skipped = true;
                return result;
            }

            // Get detailed table information
            var tableDetails = await GetTableDetailsAsync(table);
            
            // Generate business metadata
            BusinessTableMetadata businessMetadata;
            if (useAI)
            {
                businessMetadata = await GenerateAIBusinessMetadataAsync(tableDetails);
            }
            else
            {
                businessMetadata = GenerateRuleBasedBusinessMetadata(tableDetails);
            }

            // Create or update table metadata
            if (existingTable == null)
            {
                existingTable = new BusinessTableInfoEntity
                {
                    SchemaName = table.SchemaName,
                    TableName = table.TableName,
                    CreatedDate = DateTime.UtcNow,
                    CreatedBy = "AutoPopulation"
                };
                _context.BusinessTableInfo.Add(existingTable);
            }

            // Update table metadata
            UpdateTableEntity(existingTable, businessMetadata);

            // Update column metadata
            await UpdateColumnMetadataAsync(existingTable, tableDetails, businessMetadata, useAI);

            await _context.SaveChangesAsync();

            result.Success = true;
            result.BusinessMetadata = businessMetadata;
            
            _logger.LogInformation("✅ Successfully populated metadata for {Schema}.{Table}", table.SchemaName, table.TableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error populating metadata for {Schema}.{Table}", table.SchemaName, table.TableName);
            result.Success = false;
            result.ErrorMessage = ex.Message;
        }

        return result;
    }

    /// <summary>
    /// Generate business metadata using AI analysis
    /// </summary>
    private async Task<BusinessTableMetadata> GenerateAIBusinessMetadataAsync(TableDetails tableDetails)
    {
        try
        {
            var prompt = BuildAnalysisPrompt(tableDetails);
            
            var aiResponse = await _aiService.GenerateSQLAsync(prompt);

            return ParseAIResponse(aiResponse, tableDetails);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "AI analysis failed for {Table}, falling back to rule-based", tableDetails.TableName);
            return GenerateRuleBasedBusinessMetadata(tableDetails);
        }
    }

    /// <summary>
    /// Generate business metadata using rule-based analysis
    /// </summary>
    private BusinessTableMetadata GenerateRuleBasedBusinessMetadata(TableDetails tableDetails)
    {
        var metadata = new BusinessTableMetadata
        {
            TableName = tableDetails.TableName,
            SchemaName = tableDetails.SchemaName
        };

        // Analyze table name for business purpose
        metadata.BusinessPurpose = InferBusinessPurpose(tableDetails.TableName);
        metadata.DomainClassification = InferDomainClassification(tableDetails.TableName, tableDetails.Columns);
        metadata.NaturalLanguageAliases = GenerateNaturalLanguageAliases(tableDetails.TableName);
        metadata.ImportanceScore = CalculateImportanceScore(tableDetails);

        // Analyze columns for business context
        foreach (var column in tableDetails.Columns)
        {
            var columnMetadata = new BusinessColumnMetadata
            {
                ColumnName = column.ColumnName,
                BusinessMeaning = InferColumnBusinessMeaning(column),
                BusinessDataType = InferBusinessDataType(column),
                SemanticTags = InferSemanticTags(column),
                IsSensitiveData = IsSensitiveData(column),
                IsKeyColumn = column.IsPrimaryKey || column.IsForeignKey
            };
            
            metadata.Columns.Add(columnMetadata);
        }

        return metadata;
    }

    /// <summary>
    /// Get all tables from the database
    /// </summary>
    private async Task<List<DatabaseTableInfo>> GetDatabaseTablesAsync()
    {
        var tables = new List<DatabaseTableInfo>();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT 
                TABLE_SCHEMA as SchemaName,
                TABLE_NAME as TableName,
                (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = t.TABLE_SCHEMA AND TABLE_NAME = t.TABLE_NAME) as ColumnCount
            FROM INFORMATION_SCHEMA.TABLES t
            WHERE TABLE_TYPE = 'BASE TABLE'
            AND TABLE_SCHEMA NOT IN ('sys', 'information_schema')
            ORDER BY TABLE_SCHEMA, TABLE_NAME";
            
        using var command = new SqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            tables.Add(new DatabaseTableInfo
            {
                SchemaName = reader.GetString(0),
                TableName = reader.GetString(1),
                ColumnCount = reader.GetInt32(2)
            });
        }
        
        return tables;
    }

    /// <summary>
    /// Get detailed information about a specific table
    /// </summary>
    private async Task<TableDetails> GetTableDetailsAsync(DatabaseTableInfo table)
    {
        var details = new TableDetails
        {
            SchemaName = table.SchemaName,
            TableName = table.TableName
        };

        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync();
        
        var query = @"
            SELECT 
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                c.COLUMN_DEFAULT,
                c.CHARACTER_MAXIMUM_LENGTH,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsPrimaryKey,
                CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IsForeignKey
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.TABLE_SCHEMA = @SchemaName AND tc.TABLE_NAME = @TableName AND tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ) pk ON c.COLUMN_NAME = pk.COLUMN_NAME
            LEFT JOIN (
                SELECT ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.TABLE_SCHEMA = @SchemaName AND tc.TABLE_NAME = @TableName AND tc.CONSTRAINT_TYPE = 'FOREIGN KEY'
            ) fk ON c.COLUMN_NAME = fk.COLUMN_NAME
            WHERE c.TABLE_SCHEMA = @SchemaName AND c.TABLE_NAME = @TableName
            ORDER BY c.ORDINAL_POSITION";
            
        using var command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@SchemaName", table.SchemaName);
        command.Parameters.AddWithValue("@TableName", table.TableName);
        
        using var reader = await command.ExecuteReaderAsync();
        
        while (await reader.ReadAsync())
        {
            details.Columns.Add(new ColumnDetails
            {
                ColumnName = reader.GetString(0),
                DataType = reader.GetString(1),
                IsNullable = reader.GetString(2) == "YES",
                DefaultValue = reader.IsDBNull(3) ? null : reader.GetString(3),
                MaxLength = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                IsPrimaryKey = reader.GetInt32(5) == 1,
                IsForeignKey = reader.GetInt32(6) == 1
            });
        }
        
        return details;
    }

    // Helper methods for rule-based analysis
    private string InferBusinessPurpose(string tableName)
    {
        var name = tableName.ToLower();
        
        if (name.Contains("user") || name.Contains("customer") || name.Contains("player"))
            return "Stores user/customer/player information and related data";
        if (name.Contains("transaction") || name.Contains("payment") || name.Contains("financial"))
            return "Manages financial transactions and payment processing";
        if (name.Contains("game") || name.Contains("bet") || name.Contains("activity"))
            return "Tracks gaming activities and betting operations";
        if (name.Contains("daily") || name.Contains("summary") || name.Contains("report"))
            return "Contains aggregated data for reporting and analytics";
        if (name.Contains("log") || name.Contains("audit") || name.Contains("history"))
            return "Maintains historical records and audit trails";
        if (name.Contains("config") || name.Contains("setting") || name.Contains("parameter"))
            return "Stores system configuration and settings";
            
        return "Business purpose to be defined based on table analysis";
    }

    private string InferDomainClassification(string tableName, List<ColumnDetails> columns)
    {
        var name = tableName.ToLower();
        var columnNames = string.Join(" ", columns.Select(c => c.ColumnName.ToLower()));
        
        if (name.Contains("financial") || name.Contains("payment") || name.Contains("transaction") || 
            columnNames.Contains("amount") || columnNames.Contains("balance") || columnNames.Contains("revenue"))
            return "Finance";
        if (name.Contains("user") || name.Contains("customer") || name.Contains("player") ||
            columnNames.Contains("email") || columnNames.Contains("phone") || columnNames.Contains("address"))
            return "Customer";
        if (name.Contains("game") || name.Contains("bet") || name.Contains("activity") ||
            columnNames.Contains("bet") || columnNames.Contains("win") || columnNames.Contains("loss"))
            return "Gaming";
        if (name.Contains("report") || name.Contains("analytics") || name.Contains("summary"))
            return "Analytics";
            
        return "General";
    }

    private List<string> GenerateNaturalLanguageAliases(string tableName)
    {
        var aliases = new List<string>();
        
        // Convert technical names to business-friendly alternatives
        var name = tableName.ToLower().Replace("tbl_", "").Replace("_", " ");
        aliases.Add(char.ToUpper(name[0]) + name.Substring(1));
        
        // Add common business variations
        if (name.Contains("daily"))
            aliases.Add("Daily Reports");
        if (name.Contains("user"))
            aliases.AddRange(new[] { "Users", "Customers", "Players" });
        if (name.Contains("transaction"))
            aliases.AddRange(new[] { "Transactions", "Payments", "Financial Records" });
            
        return aliases.Distinct().ToList();
    }

    private decimal CalculateImportanceScore(TableDetails tableDetails)
    {
        decimal score = 0.5m; // Base score
        
        // Increase importance based on table characteristics
        if (tableDetails.Columns.Any(c => c.ColumnName.ToLower().Contains("amount") || c.ColumnName.ToLower().Contains("revenue")))
            score += 0.2m;
        if (tableDetails.Columns.Any(c => c.IsPrimaryKey))
            score += 0.1m;
        if (tableDetails.Columns.Count > 10)
            score += 0.1m;
        if (tableDetails.TableName.ToLower().Contains("daily") || tableDetails.TableName.ToLower().Contains("summary"))
            score += 0.2m;
            
        return Math.Min(score, 1.0m);
    }

    // Gaming-specific AI analysis methods
    private string BuildAnalysisPrompt(TableDetails tableDetails)
    {
        var prompt = $@"
You are a business analyst expert in gaming/casino operations. Analyze this database table and provide business metadata.

TABLE: {tableDetails.SchemaName}.{tableDetails.TableName}
COLUMNS: {string.Join(", ", tableDetails.Columns.Select(c => $"{c.ColumnName} ({c.DataType})"))}

This is from a gaming/casino platform. Based on the table name and columns, provide:

1. BUSINESS_PURPOSE: Clear 1-2 sentence description of what this table stores
2. DOMAIN_CLASSIFICATION: Choose from: Gaming, Finance, Customer, Analytics, Reference, Security
3. IMPORTANCE_SCORE: 0.0-1.0 (1.0 = critical like revenue data, 0.5 = supporting data)
4. NATURAL_LANGUAGE_ALIASES: 3-5 business-friendly names separated by |
5. BUSINESS_CONTEXT: Detailed explanation of how this table is used in gaming operations

Gaming Context:
- tbl_Daily_actions = core revenue/activity table
- PlayerID = unique player identifier
- Money columns = financial transactions
- Gaming verticals: Casino, Sports, Live, Bingo
- Key metrics: GGR, NGR, deposits, withdrawals, bets, wins

Format response as:
BUSINESS_PURPOSE: [description]
DOMAIN_CLASSIFICATION: [domain]
IMPORTANCE_SCORE: [score]
NATURAL_LANGUAGE_ALIASES: [alias1|alias2|alias3]
BUSINESS_CONTEXT: [detailed context]
";
        return prompt;
    }

    private BusinessTableMetadata ParseAIResponse(string response, TableDetails tableDetails)
    {
        var metadata = new BusinessTableMetadata
        {
            SchemaName = tableDetails.SchemaName,
            TableName = tableDetails.TableName
        };

        try
        {
            // Parse AI response
            var lines = response.Split('\n', StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.StartsWith("BUSINESS_PURPOSE:", StringComparison.OrdinalIgnoreCase))
                    metadata.BusinessPurpose = line.Substring("BUSINESS_PURPOSE:".Length).Trim();
                else if (line.StartsWith("DOMAIN_CLASSIFICATION:", StringComparison.OrdinalIgnoreCase))
                    metadata.DomainClassification = line.Substring("DOMAIN_CLASSIFICATION:".Length).Trim();
                else if (line.StartsWith("IMPORTANCE_SCORE:", StringComparison.OrdinalIgnoreCase))
                {
                    if (decimal.TryParse(line.Substring("IMPORTANCE_SCORE:".Length).Trim(), out var score))
                        metadata.ImportanceScore = Math.Max(0.0m, Math.Min(1.0m, score));
                }
                else if (line.StartsWith("NATURAL_LANGUAGE_ALIASES:", StringComparison.OrdinalIgnoreCase))
                {
                    var aliases = line.Substring("NATURAL_LANGUAGE_ALIASES:".Length).Trim();
                    metadata.NaturalLanguageAliases = aliases.Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(a => a.Trim()).ToList();
                }
            }

            // Fallback to rule-based if AI parsing failed
            if (string.IsNullOrEmpty(metadata.BusinessPurpose))
            {
                var ruleBasedMetadata = GenerateRuleBasedBusinessMetadata(tableDetails);
                metadata.BusinessPurpose = ruleBasedMetadata.BusinessPurpose;
                metadata.DomainClassification = ruleBasedMetadata.DomainClassification;
                metadata.ImportanceScore = ruleBasedMetadata.ImportanceScore;
                metadata.NaturalLanguageAliases = ruleBasedMetadata.NaturalLanguageAliases;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error parsing AI response, using rule-based fallback");
            return GenerateRuleBasedBusinessMetadata(tableDetails);
        }

        return metadata;
    }

    private void UpdateTableEntity(BusinessTableInfoEntity entity, BusinessTableMetadata metadata)
    {
        entity.BusinessPurpose = metadata.BusinessPurpose;
        entity.DomainClassification = metadata.DomainClassification;
        entity.NaturalLanguageAliases = JsonSerializer.Serialize(metadata.NaturalLanguageAliases);
        entity.ImportanceScore = metadata.ImportanceScore;
        entity.UpdatedDate = DateTime.UtcNow;
        entity.UpdatedBy = "AutoPopulation";
        entity.LastAnalyzed = DateTime.UtcNow;
    }

    private async Task UpdateColumnMetadataAsync(BusinessTableInfoEntity table, TableDetails details, BusinessTableMetadata metadata, bool useAI)
    {
        try
        {
            _logger.LogInformation("🔧 Updating column metadata for {Schema}.{Table} with {ColumnCount} columns",
                table.SchemaName, table.TableName, details.Columns.Count);

            // Remove existing columns if overwriting
            var existingColumns = await _context.BusinessColumnInfo
                .Where(c => c.TableInfoId == table.Id)
                .ToListAsync();

            if (existingColumns.Any())
            {
                _context.BusinessColumnInfo.RemoveRange(existingColumns);
                _logger.LogInformation("🗑️ Removed {Count} existing columns for {Schema}.{Table}",
                    existingColumns.Count, table.SchemaName, table.TableName);
            }

            // Add new column metadata
            foreach (var column in details.Columns)
            {
                var businessMeaning = InferColumnBusinessMeaning(column);
                var businessDataType = InferBusinessDataType(column);
                var semanticTags = InferSemanticTags(column);
                var sampleValues = await GetSampleValuesAsync(table.SchemaName, table.TableName, column.ColumnName);

                var columnEntity = new BusinessColumnInfoEntity
                {
                    TableInfoId = table.Id,
                    ColumnName = column.ColumnName,
                    BusinessMeaning = businessMeaning,
                    BusinessContext = $"Column from {table.SchemaName}.{table.TableName}",
                    BusinessDataType = businessDataType,
                    DataExamples = JsonSerializer.Serialize(sampleValues),
                    ValueExamples = JsonSerializer.Serialize(sampleValues),
                    SemanticTags = JsonSerializer.Serialize(semanticTags),
                    NaturalLanguageAliases = JsonSerializer.Serialize(new[] { column.ColumnName }),
                    IsKeyColumn = column.IsPrimaryKey || column.IsForeignKey,
                    IsSensitiveData = IsSensitiveData(column),
                    IsActive = true,
                    CreatedBy = "AutoPopulation",
                    CreatedDate = DateTime.UtcNow,
                    SemanticRelevanceScore = CalculateColumnRelevanceScore(column, businessMeaning)
                };

                _context.BusinessColumnInfo.Add(columnEntity);
            }

            _logger.LogInformation("✅ Added {Count} column metadata entries for {Schema}.{Table}",
                details.Columns.Count, table.SchemaName, table.TableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error updating column metadata for {Schema}.{Table}",
                table.SchemaName, table.TableName);
            throw;
        }
    }

    // Additional helper methods
    private string InferColumnBusinessMeaning(ColumnDetails column)
    {
        var columnName = column.ColumnName.ToLower();

        // Gaming domain specific mappings
        if (columnName.Contains("deposit")) return "Deposit amount or deposit-related information";
        if (columnName.Contains("country")) return "Country or geographical location";
        if (columnName.Contains("user") || columnName.Contains("player")) return "User or player identifier";
        if (columnName.Contains("action") || columnName.Contains("activity")) return "User action or activity data";
        if (columnName.Contains("date") || columnName.Contains("time")) return "Date or time information";
        if (columnName.Contains("amount") || columnName.Contains("value")) return "Monetary amount or numeric value";
        if (columnName.Contains("currency")) return "Currency information";
        if (columnName.Contains("game")) return "Game-related information";
        if (columnName.Contains("transaction")) return "Financial transaction data";
        if (columnName.Contains("id")) return "Unique identifier";

        return $"Business data for {column.ColumnName}";
    }

    private string InferBusinessDataType(ColumnDetails column)
    {
        var sqlType = column.DataType.ToLower();
        var columnName = column.ColumnName.ToLower();

        // Map SQL types to business types
        if (sqlType.Contains("money") || sqlType.Contains("decimal") || sqlType.Contains("numeric"))
        {
            if (columnName.Contains("amount") || columnName.Contains("deposit") || columnName.Contains("value"))
                return "Currency";
            return "Decimal";
        }
        if (sqlType.Contains("int") || sqlType.Contains("bigint")) return "Integer";
        if (sqlType.Contains("varchar") || sqlType.Contains("nvarchar") || sqlType.Contains("text")) return "Text";
        if (sqlType.Contains("date") || sqlType.Contains("time")) return "DateTime";
        if (sqlType.Contains("bit")) return "Boolean";

        return column.DataType; // Fallback to SQL type
    }

    private List<string> InferSemanticTags(ColumnDetails column)
    {
        var tags = new List<string>();
        var columnName = column.ColumnName.ToLower();

        if (column.IsPrimaryKey) tags.Add("PrimaryKey");
        if (column.IsForeignKey) tags.Add("ForeignKey");
        if (columnName.Contains("id")) tags.Add("Identifier");
        if (columnName.Contains("date") || columnName.Contains("time")) tags.Add("Temporal");
        if (columnName.Contains("amount") || columnName.Contains("deposit") || columnName.Contains("value")) tags.Add("Financial");
        if (columnName.Contains("country") || columnName.Contains("location")) tags.Add("Geographic");
        if (columnName.Contains("user") || columnName.Contains("player")) tags.Add("UserData");
        if (columnName.Contains("game")) tags.Add("Gaming");
        if (IsSensitiveData(column)) tags.Add("Sensitive");

        return tags;
    }

    private bool IsSensitiveData(ColumnDetails column)
    {
        var columnName = column.ColumnName.ToLower();
        return columnName.Contains("email") || columnName.Contains("phone") ||
               columnName.Contains("password") || columnName.Contains("ssn") ||
               columnName.Contains("credit") || columnName.Contains("card");
    }

    private decimal CalculateColumnRelevanceScore(ColumnDetails column, string businessMeaning)
    {
        decimal score = 0.5m; // Base score

        // Increase score for key columns
        if (column.IsPrimaryKey || column.IsForeignKey) score += 0.2m;

        // Increase score for commonly used business columns
        var columnName = column.ColumnName.ToLower();
        if (columnName.Contains("id") || columnName.Contains("date") ||
            columnName.Contains("amount") || columnName.Contains("user") ||
            columnName.Contains("country") || columnName.Contains("deposit")) score += 0.3m;

        return Math.Min(1.0m, score);
    }

    private async Task<List<string>> GetSampleValuesAsync(string schemaName, string tableName, string columnName)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = $@"
                SELECT DISTINCT TOP 5 [{columnName}]
                FROM [{schemaName}].[{tableName}]
                WHERE [{columnName}] IS NOT NULL
                ORDER BY [{columnName}]";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            var values = new List<string>();
            while (await reader.ReadAsync() && values.Count < 5)
            {
                var value = reader.GetValue(0)?.ToString();
                if (!string.IsNullOrEmpty(value))
                {
                    values.Add(value);
                }
            }

            return values;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not get sample values for {Schema}.{Table}.{Column}",
                schemaName, tableName, columnName);
            return new List<string>();
        }
    }
}

// Supporting classes
public class BusinessMetadataPopulationResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public List<TableMetadataResult> ProcessedTables { get; set; } = new();
    public int SuccessCount { get; set; }
    public int ErrorCount { get; set; }
}

public class TableMetadataResult
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public bool Skipped { get; set; }
    public string? ErrorMessage { get; set; }
    public BusinessTableMetadata? BusinessMetadata { get; set; }
}

public class DatabaseTableInfo
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int ColumnCount { get; set; }
}

public class TableDetails
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public List<ColumnDetails> Columns { get; set; } = new();
}

public class ColumnDetails
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
    public int? MaxLength { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
}

public class BusinessTableMetadata
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string BusinessPurpose { get; set; } = string.Empty;
    public string DomainClassification { get; set; } = string.Empty;
    public List<string> NaturalLanguageAliases { get; set; } = new();
    public decimal ImportanceScore { get; set; } = 0.5m;
    public List<BusinessColumnMetadata> Columns { get; set; } = new();
}

public class BusinessColumnMetadata
{
    public string ColumnName { get; set; } = string.Empty;
    public string BusinessMeaning { get; set; } = string.Empty;
    public string BusinessDataType { get; set; } = string.Empty;
    public List<string> SemanticTags { get; set; } = new();
    public bool IsSensitiveData { get; set; }
    public bool IsKeyColumn { get; set; }
}
