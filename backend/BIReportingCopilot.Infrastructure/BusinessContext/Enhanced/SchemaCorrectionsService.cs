using System.Text;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.BusinessContext.Enhanced;

/// <summary>
/// Service for providing schema corrections and mappings to improve AI SQL generation
/// </summary>
public class SchemaCorrectionsService
{
    private readonly ILogger<SchemaCorrectionsService> _logger;

    public SchemaCorrectionsService(ILogger<SchemaCorrectionsService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get schema corrections and mappings for better AI understanding
    /// </summary>
    public string GetSchemaCorrections(ContextualBusinessSchema schema)
    {
        var corrections = new StringBuilder();
        
        corrections.AppendLine("## CRITICAL SCHEMA CORRECTIONS");
        corrections.AppendLine("**IMPORTANT: Use these EXACT column names and relationships:**");
        corrections.AppendLine();

        // Add table-specific corrections
        AddDailyActionsCorrections(corrections, schema);
        AddPlayerTableCorrections(corrections, schema);
        AddCountryTableCorrections(corrections, schema);
        AddCommonMistakes(corrections);

        return corrections.ToString();
    }

    private void AddDailyActionsCorrections(StringBuilder corrections, ContextualBusinessSchema schema)
    {
        corrections.AppendLine("### tbl_Daily_actions Table Corrections:");
        corrections.AppendLine("- ✅ **Date column**: Use `Date` (NOT TransactionDate)");
        corrections.AppendLine("- ✅ **Player reference**: Use `PlayerID` (bigint)");
        corrections.AppendLine("- ✅ **Deposits**: Use `Deposits` (decimal) for deposit amounts");
        corrections.AppendLine("- ✅ **Time filtering**: `Date = DATEADD(day, -1, GETDATE())` for yesterday");
        corrections.AppendLine();

        corrections.AppendLine("**Example for yesterday's data:**");
        corrections.AppendLine("```sql");
        corrections.AppendLine("WHERE da.Date = DATEADD(day, -1, GETDATE())");
        corrections.AppendLine("```");
        corrections.AppendLine();
    }

    private void AddPlayerTableCorrections(StringBuilder corrections, ContextualBusinessSchema schema)
    {
        corrections.AppendLine("### tbl_Daily_actions_players Table Corrections:");
        corrections.AppendLine("- ✅ **Player reference**: Use `PlayerID` (bigint)");
        corrections.AppendLine("- ❌ **NO CountryID**: This table does NOT have CountryID");
        corrections.AppendLine("- ⚠️ **Country relationship**: Need to find correct country reference");
        corrections.AppendLine();

        corrections.AppendLine("**Correct JOIN pattern:**");
        corrections.AppendLine("```sql");
        corrections.AppendLine("FROM tbl_Daily_actions da");
        corrections.AppendLine("JOIN tbl_Daily_actions_players dap ON da.PlayerID = dap.PlayerID");
        corrections.AppendLine("-- Note: Check actual foreign key relationship");
        corrections.AppendLine("```");
        corrections.AppendLine();
    }

    private void AddCountryTableCorrections(StringBuilder corrections, ContextualBusinessSchema schema)
    {
        corrections.AppendLine("### Country Relationship Corrections:");
        corrections.AppendLine("- ✅ **Country Table**: Use `tbl_Countries` for country reference data");
        corrections.AppendLine("- ✅ **Country JOIN**: `tbl_Daily_actions_players.CountryID = tbl_Countries.ID`");
        corrections.AppendLine("- ✅ **Country Filtering**: Use `tbl_Daily_actions_players.Country = 'United Kingdom'` for UK");
        corrections.AppendLine("- ✅ **Alternative**: Use `tbl_Daily_actions_players.CountryCode = 'GB'` for UK");
        corrections.AppendLine("- ⚠️ **Include tbl_Countries**: Must include tbl_Countries table for country filtering");
        corrections.AppendLine();

        corrections.AppendLine("**Correct Country JOIN pattern:**");
        corrections.AppendLine("```sql");
        corrections.AppendLine("FROM tbl_Daily_actions da");
        corrections.AppendLine("JOIN tbl_Daily_actions_players dap ON da.PlayerID = dap.PlayerID");
        corrections.AppendLine("WHERE dap.Country = 'United Kingdom' -- or dap.CountryCode = 'GB'");
        corrections.AppendLine("```");
        corrections.AppendLine();
    }

    private void AddCommonMistakes(StringBuilder corrections)
    {
        corrections.AppendLine("### Common Mistakes to Avoid:");
        corrections.AppendLine("1. ❌ **TransactionDate** → Use `Date` instead");
        corrections.AppendLine("2. ❌ **CountryCode/CountryName** → Find actual country column");
        corrections.AppendLine("3. ❌ **Wrong JOIN conditions** → Verify foreign key relationships");
        corrections.AppendLine("4. ❌ **Missing aggregation** → Use SUM() for deposit totals");
        corrections.AppendLine("5. ❌ **Missing GROUP BY** → Required when using aggregation");
        corrections.AppendLine();

        corrections.AppendLine("### Recommended Query Pattern for 'Top 10 depositors yesterday from UK':");
        corrections.AppendLine("```sql");
        corrections.AppendLine("SELECT TOP 10");
        corrections.AppendLine("    da.PlayerID,");
        corrections.AppendLine("    SUM(da.Deposits) AS TotalDeposits,");
        corrections.AppendLine("    dap.Country");
        corrections.AppendLine("FROM common.tbl_Daily_actions da");
        corrections.AppendLine("JOIN common.tbl_Daily_actions_players dap ON da.PlayerID = dap.PlayerID");
        corrections.AppendLine("WHERE da.Date = DATEADD(day, -1, GETDATE())");
        corrections.AppendLine("    AND da.Deposits > 0");
        corrections.AppendLine("    AND (dap.Country = 'United Kingdom' OR dap.CountryCode = 'GB')");
        corrections.AppendLine("GROUP BY da.PlayerID, dap.Country");
        corrections.AppendLine("ORDER BY TotalDeposits DESC;");
        corrections.AppendLine("```");
        corrections.AppendLine();
    }

    /// <summary>
    /// Get column mapping corrections for specific tables
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> GetColumnMappings()
    {
        return new Dictionary<string, Dictionary<string, string>>
        {
            ["tbl_Daily_actions"] = new Dictionary<string, string>
            {
                ["TransactionDate"] = "Date",
                ["transaction_date"] = "Date",
                ["ActivityDate"] = "Date",
                ["RecordDate"] = "Date"
            },
            ["tbl_Countries"] = new Dictionary<string, string>
            {
                ["CountryName"] = "UNKNOWN_COLUMN", // Need to investigate
                ["Country"] = "UNKNOWN_COLUMN",
                ["CountryCode"] = "UNKNOWN_COLUMN"
            }
        };
    }

    /// <summary>
    /// Get relationship corrections for proper JOINs
    /// </summary>
    public List<string> GetRelationshipCorrections()
    {
        return new List<string>
        {
            "tbl_Daily_actions.PlayerID = tbl_Daily_actions_players.PlayerID",
            "WARNING: Country relationship needs investigation",
            "VERIFY: All foreign key relationships before using"
        };
    }

    /// <summary>
    /// Get business logic corrections for deposit queries
    /// </summary>
    public List<string> GetBusinessLogicCorrections()
    {
        return new List<string>
        {
            "Use SUM(Deposits) for total deposit amounts",
            "Filter by Date = DATEADD(day, -1, GETDATE()) for yesterday",
            "Add WHERE Deposits > 0 to exclude zero deposits",
            "Use GROUP BY PlayerID when aggregating",
            "Use ORDER BY TotalDeposits DESC for top depositors"
        };
    }
}
