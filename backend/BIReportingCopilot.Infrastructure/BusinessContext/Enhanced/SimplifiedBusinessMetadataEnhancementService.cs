using BIReportingCopilot.Core.Interfaces.BusinessContext;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Simplified business metadata enhancement service without AI dependencies
/// Provides manual enhancement workflows and basic automation
/// </summary>
public class SimplifiedBusinessMetadataEnhancementService
{
    private readonly ILogger<SimplifiedBusinessMetadataEnhancementService> _logger;
    private readonly TuningDbContext _context;

    public SimplifiedBusinessMetadataEnhancementService(
        ILogger<SimplifiedBusinessMetadataEnhancementService> logger,
        TuningDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Enhance business metadata for tables with missing information
    /// </summary>
    public async Task<int> EnhanceBusinessMetadataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔧 [METADATA-ENHANCEMENT] Starting business metadata enhancement");

            var enhancedCount = 0;

            // 1. Enhance table metadata
            enhancedCount += await EnhanceTableMetadataAsync(cancellationToken);

            // 2. Enhance column metadata
            enhancedCount += await EnhanceColumnMetadataAsync(cancellationToken);

            // 3. Enhance glossary terms
            enhancedCount += await EnhanceGlossaryTermsAsync(cancellationToken);

            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("✅ [METADATA-ENHANCEMENT] Enhanced {Count} metadata items", enhancedCount);
            return enhancedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [METADATA-ENHANCEMENT] Failed to enhance business metadata");
            throw;
        }
    }

    /// <summary>
    /// Enhance table metadata with business-friendly information
    /// </summary>
    private async Task<int> EnhanceTableMetadataAsync(CancellationToken cancellationToken)
    {
        var enhancedCount = 0;

        var tablesNeedingEnhancement = await _context.BusinessTableInfo
            .Where(t => string.IsNullOrEmpty(t.BusinessPurpose) ||
                       string.IsNullOrEmpty(t.BusinessContext) ||
                       string.IsNullOrEmpty(t.DomainClassification) ||
                       string.IsNullOrEmpty(t.PrimaryUseCase))
            .ToListAsync(cancellationToken);

        foreach (var table in tablesNeedingEnhancement)
        {
            var enhanced = false;

            // Apply rule-based enhancements based on table name patterns
            if (string.IsNullOrEmpty(table.BusinessPurpose))
            {
                table.BusinessPurpose = GenerateBusinessPurpose(table.TableName);
                enhanced = true;
            }

            if (string.IsNullOrEmpty(table.BusinessContext))
            {
                table.BusinessContext = GenerateTableDescription(table.TableName, table.BusinessPurpose);
                enhanced = true;
            }

            if (string.IsNullOrEmpty(table.DomainClassification))
            {
                table.DomainClassification = GenerateBusinessDomain(table.TableName);
                enhanced = true;
            }

            if (string.IsNullOrEmpty(table.PrimaryUseCase))
            {
                table.PrimaryUseCase = GeneratePrimaryUseCase(table.TableName, table.BusinessPurpose);
                enhanced = true;
            }

            if (enhanced)
            {
                table.UpdatedDate = DateTime.UtcNow;
                enhancedCount++;
            }
        }

        return enhancedCount;
    }

    /// <summary>
    /// Enhance column metadata with business-friendly information
    /// </summary>
    private async Task<int> EnhanceColumnMetadataAsync(CancellationToken cancellationToken)
    {
        var enhancedCount = 0;

        var columnsNeedingEnhancement = await _context.BusinessColumnInfo
            .Where(c => string.IsNullOrEmpty(c.BusinessMeaning) ||
                       string.IsNullOrEmpty(c.BusinessContext) ||
                       string.IsNullOrEmpty(c.ValueExamples) ||
                       string.IsNullOrEmpty(c.NaturalLanguageAliases) ||
                       string.IsNullOrEmpty(c.SemanticContext))
            .ToListAsync(cancellationToken);

        foreach (var column in columnsNeedingEnhancement)
        {
            var enhanced = false;

            // Apply rule-based enhancements based on column name patterns
            if (string.IsNullOrEmpty(column.BusinessMeaning))
            {
                column.BusinessMeaning = GenerateBusinessFriendlyName(column.ColumnName);
                enhanced = true;
            }

            if (string.IsNullOrEmpty(column.BusinessContext))
            {
                column.BusinessContext = GenerateColumnDescription(column.ColumnName, column.BusinessDataType);
                enhanced = true;
            }

            // Generate example values for common patterns
            if (string.IsNullOrEmpty(column.ValueExamples))
            {
                column.ValueExamples = GenerateValueExamples(column.ColumnName, column.BusinessDataType);
                enhanced = true;
            }

            // Generate natural language aliases
            if (string.IsNullOrEmpty(column.NaturalLanguageAliases))
            {
                column.NaturalLanguageAliases = GenerateBusinessFriendlyColumnName(column.ColumnName);
                enhanced = true;
            }

            // Generate semantic context
            if (string.IsNullOrEmpty(column.SemanticContext))
            {
                column.SemanticContext = GenerateNaturalLanguageDescription(column.ColumnName, column.BusinessDataType);
                enhanced = true;
            }

            // Generate LLM prompt hints
            if (string.IsNullOrEmpty(column.LLMPromptHints))
            {
                column.LLMPromptHints = GenerateLLMPromptHints(column.ColumnName, column.BusinessDataType);
                enhanced = true;
            }

            if (enhanced)
            {
                column.UpdatedDate = DateTime.UtcNow;
                enhancedCount++;
            }
        }

        return enhancedCount;
    }

    /// <summary>
    /// Enhance glossary terms with additional context
    /// </summary>
    private async Task<int> EnhanceGlossaryTermsAsync(CancellationToken cancellationToken)
    {
        var enhancedCount = 0;

        var termsNeedingEnhancement = await _context.BusinessGlossary
            .Where(g => string.IsNullOrEmpty(g.Definition) || 
                       string.IsNullOrEmpty(g.Category))
            .ToListAsync(cancellationToken);

        foreach (var term in termsNeedingEnhancement)
        {
            var enhanced = false;

            if (string.IsNullOrEmpty(term.Category))
            {
                term.Category = DetermineTermCategory(term.Term);
                enhanced = true;
            }

            if (string.IsNullOrEmpty(term.Definition))
            {
                term.Definition = GenerateTermDefinition(term.Term, term.Category);
                enhanced = true;
            }

            if (enhanced)
            {
                term.UpdatedDate = DateTime.UtcNow;
                enhancedCount++;
            }
        }

        return enhancedCount;
    }

    /// <summary>
    /// Generate business purpose based on table name patterns
    /// </summary>
    private string GenerateBusinessPurpose(string tableName)
    {
        var lowerName = tableName.ToLower();

        return lowerName switch
        {
            var name when name.Contains("daily_actions") => "Daily player activity and transaction tracking",
            var name when name.Contains("players") => "Player profile and demographic information",
            var name when name.Contains("countries") => "Geographic reference data for countries",
            var name when name.Contains("currencies") => "Currency reference data and exchange rates",
            var name when name.Contains("games") => "Game catalog and configuration data",
            var name when name.Contains("transactions") => "Financial transaction records",
            var name when name.Contains("deposits") => "Player deposit transaction tracking",
            var name when name.Contains("withdrawals") => "Player withdrawal transaction tracking",
            var name when name.Contains("bonuses") => "Bonus and promotion tracking",
            var name when name.Contains("sessions") => "Player session and activity tracking",
            _ => $"Business data table for {tableName.Replace("tbl_", "").Replace("_", " ")}"
        };
    }

    /// <summary>
    /// Generate table description based on name and purpose
    /// </summary>
    private string GenerateTableDescription(string tableName, string? businessPurpose)
    {
        var purpose = businessPurpose ?? "data storage";
        return $"The {tableName} table is used for {purpose.ToLower()}. " +
               "This table contains important business information that supports operational and analytical reporting.";
    }

    /// <summary>
    /// Generate business-friendly column names
    /// </summary>
    private string GenerateBusinessFriendlyName(string columnName)
    {
        var friendlyName = columnName
            .Replace("_", " ")
            .Replace("Id", "ID")
            .Replace("Gbp", "GBP")
            .Replace("Usd", "USD")
            .Replace("Eur", "EUR");

        // Capitalize first letter of each word
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(friendlyName.ToLower());
    }

    /// <summary>
    /// Generate column descriptions based on name and data type
    /// </summary>
    private string GenerateColumnDescription(string columnName, string? dataType)
    {
        var lowerName = columnName.ToLower();
        var type = dataType ?? "data";

        return lowerName switch
        {
            var name when name.Contains("id") => $"Unique identifier for {name.Replace("_id", "").Replace("id", "")} records",
            var name when name.Contains("amount") => $"Monetary amount value stored as {type}",
            var name when name.Contains("date") => $"Date and time information for {name.Replace("_date", "").Replace("date", "")}",
            var name when name.Contains("count") => $"Numerical count of {name.Replace("_count", "").Replace("count", "")}",
            var name when name.Contains("name") => $"Name or title field stored as {type}",
            var name when name.Contains("code") => $"Code or identifier field stored as {type}",
            var name when name.Contains("status") => $"Status indicator field stored as {type}",
            var name when name.Contains("type") => $"Type classification field stored as {type}",
            _ => $"Business data field for {columnName.Replace("_", " ")} stored as {type}"
        };
    }

    /// <summary>
    /// Generate example values for columns
    /// </summary>
    private string GenerateValueExamples(string columnName, string? dataType)
    {
        var lowerName = columnName.ToLower();

        return lowerName switch
        {
            var name when name.Contains("country") => "UK, US, DE, FR, ES",
            var name when name.Contains("currency") => "GBP, USD, EUR, CAD",
            var name when name.Contains("amount") => "100.50, 250.00, 1500.75",
            var name when name.Contains("date") => "2024-01-15, 2024-02-20, 2024-03-10",
            var name when name.Contains("status") => "Active, Inactive, Pending",
            var name when name.Contains("type") => "Deposit, Withdrawal, Bonus",
            var name when name.Contains("id") => "1, 2, 3, 100, 1001",
            var name when name.Contains("name") => "Example Name, Sample Value, Test Data",
            _ => "Sample data values"
        };
    }

    /// <summary>
    /// Determine category for glossary terms
    /// </summary>
    private string DetermineTermCategory(string term)
    {
        var lowerTerm = term.ToLower();

        return lowerTerm switch
        {
            var t when t.Contains("deposit") || t.Contains("withdrawal") || t.Contains("transaction") => "Financial",
            var t when t.Contains("player") || t.Contains("user") || t.Contains("customer") => "Customer",
            var t when t.Contains("game") || t.Contains("slot") || t.Contains("casino") => "Gaming",
            var t when t.Contains("bonus") || t.Contains("promotion") || t.Contains("offer") => "Marketing",
            var t when t.Contains("country") || t.Contains("currency") || t.Contains("region") => "Geographic",
            var t when t.Contains("date") || t.Contains("time") || t.Contains("period") => "Temporal",
            _ => "General"
        };
    }

    /// <summary>
    /// Generate basic definitions for terms
    /// </summary>
    private string GenerateTermDefinition(string term, string? category)
    {
        var cat = category ?? "General";
        return $"{term} is a {cat.ToLower()} term used in business operations and reporting. " +
               "This term represents an important business concept that helps users understand data context.";
    }

    /// <summary>
    /// Generate business domain for tables
    /// </summary>
    private string GenerateBusinessDomain(string tableName)
    {
        var lowerName = tableName.ToLower();

        return lowerName switch
        {
            var name when name.Contains("player") || name.Contains("customer") || name.Contains("user") => "Customer Management",
            var name when name.Contains("deposit") || name.Contains("withdrawal") || name.Contains("transaction") || name.Contains("payment") => "Financial Operations",
            var name when name.Contains("game") || name.Contains("slot") || name.Contains("casino") || name.Contains("sport") => "Gaming Operations",
            var name when name.Contains("bonus") || name.Contains("promotion") || name.Contains("campaign") => "Marketing & Promotions",
            var name when name.Contains("country") || name.Contains("currency") || name.Contains("region") => "Geographic & Localization",
            var name when name.Contains("report") || name.Contains("analytics") || name.Contains("metric") => "Business Intelligence",
            var name when name.Contains("audit") || name.Contains("log") || name.Contains("history") => "Compliance & Auditing",
            _ => "Business Operations"
        };
    }

    /// <summary>
    /// Generate primary use case for tables
    /// </summary>
    private string GeneratePrimaryUseCase(string tableName, string? businessPurpose)
    {
        var lowerName = tableName.ToLower();
        var purpose = businessPurpose ?? "";

        return lowerName switch
        {
            var name when name.Contains("daily") => "Daily reporting and trend analysis",
            var name when name.Contains("player") => "Player analytics and customer lifecycle management",
            var name when name.Contains("transaction") => "Financial transaction tracking and reconciliation",
            var name when name.Contains("deposit") => "Deposit analysis and cash flow monitoring",
            var name when name.Contains("game") => "Gaming performance analysis and optimization",
            var name when name.Contains("bonus") => "Bonus effectiveness and marketing ROI analysis",
            var name when name.Contains("country") => "Geographic performance and localization analysis",
            var name when name.Contains("audit") => "Compliance reporting and audit trail maintenance",
            _ => purpose.Contains("track") ? "Data tracking and monitoring" : "Business analysis and reporting"
        };
    }

    /// <summary>
    /// Generate business-friendly column name
    /// </summary>
    private string GenerateBusinessFriendlyColumnName(string columnName)
    {
        var friendlyName = columnName
            .Replace("_", " ")
            .Replace("Id", "ID")
            .Replace("Gbp", "GBP")
            .Replace("Usd", "USD")
            .Replace("Eur", "EUR")
            .Replace("Ftd", "First Time Deposit")
            .Replace("Ftda", "First Time Deposit Amount")
            .Replace("Ggr", "Gross Gaming Revenue")
            .Replace("Ngr", "Net Gaming Revenue");

        // Capitalize first letter of each word
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(friendlyName.ToLower());
    }

    /// <summary>
    /// Generate natural language description for columns
    /// </summary>
    private string GenerateNaturalLanguageDescription(string columnName, string? dataType)
    {
        var lowerName = columnName.ToLower();
        var type = dataType ?? "data";

        return lowerName switch
        {
            var name when name.Contains("deposit") => "This field tracks the total amount of money deposited by players, representing cash inflow to the business.",
            var name when name.Contains("withdrawal") || name.Contains("cashout") => "This field records the amount of money withdrawn by players, representing cash outflow from the business.",
            var name when name.Contains("bet") => "This field captures the total amount wagered by players across all gaming activities.",
            var name when name.Contains("win") => "This field shows the total amount won by players from gaming activities.",
            var name when name.Contains("bonus") => "This field tracks bonus amounts awarded to players as part of promotional activities.",
            var name when name.Contains("player") && name.Contains("id") => "This field uniquely identifies each player in the system for tracking and analysis purposes.",
            var name when name.Contains("date") => "This field stores the date information for temporal analysis and time-based reporting.",
            var name when name.Contains("country") => "This field indicates the geographic location or nationality for regional analysis.",
            var name when name.Contains("currency") => "This field specifies the currency used for financial transactions and reporting.",
            var name when name.Contains("registration") => "This field indicates whether a player registered on this date, used for acquisition tracking.",
            var name when name.Contains("ftd") => "This field tracks first-time deposit events, crucial for measuring customer conversion rates.",
            _ => $"This field contains {type.ToLower()} information related to {columnName.Replace("_", " ")} for business analysis."
        };
    }

    /// <summary>
    /// Generate LLM prompt hints for columns
    /// </summary>
    private string GenerateLLMPromptHints(string columnName, string? dataType)
    {
        var lowerName = columnName.ToLower();

        return lowerName switch
        {
            var name when name.Contains("deposit") => "Use for cash flow analysis, player value assessment, and financial trend analysis",
            var name when name.Contains("withdrawal") => "Use for cash flow monitoring, player satisfaction analysis, and operational efficiency",
            var name when name.Contains("bet") => "Use for gaming activity analysis, player engagement metrics, and revenue calculations",
            var name when name.Contains("player") && name.Contains("id") => "Use as primary key for player-centric analysis and customer segmentation",
            var name when name.Contains("date") => "Use for time-series analysis, trend identification, and period comparisons",
            var name when name.Contains("country") => "Use for geographic analysis, regional performance, and market segmentation",
            var name when name.Contains("bonus") => "Use for marketing effectiveness analysis and promotional ROI calculations",
            var name when name.Contains("registration") => "Use for customer acquisition analysis and growth tracking",
            var name when name.Contains("ftd") => "Use for conversion analysis and customer acquisition quality assessment",
            var name when name.Contains("amount") => "Use for financial analysis, aggregations, and monetary calculations",
            _ => "Use for business analysis and reporting based on data context"
        };
    }
}
