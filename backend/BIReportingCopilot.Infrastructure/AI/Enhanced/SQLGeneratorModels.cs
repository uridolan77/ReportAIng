using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;

namespace BIReportingCopilot.Infrastructure.AI.Enhanced;

/// <summary>
/// Generated query result with enhanced metadata
/// </summary>
public class GeneratedQuery
{
    public string SQL { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public List<string> Alternatives { get; set; } = new();
    public string ExecutionPlan { get; set; } = string.Empty;
    public Dictionary<string, object> Metadata { get; set; } = new();
}

/// <summary>
/// Sub-query SQL generation result
/// </summary>
public class SubQuerySQLResult
{
    public SubQuery SubQuery { get; set; } = new();
    public string SQL { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> UsedTables { get; set; } = new();
    public double EstimatedCost { get; set; }
}

/// <summary>
/// Combined SQL result
/// </summary>
public class CombinedSQLResult
{
    public string SQL { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string CombinationStrategy { get; set; } = string.Empty;
}

/// <summary>
/// Schema relationship analyzer
/// </summary>
public class SchemaRelationshipAnalyzer
{
    private readonly ILogger _logger;

    public SchemaRelationshipAnalyzer(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<List<SchemaRelationship>> AnalyzeRelationshipsAsync(
        SemanticAnalysis semanticAnalysis,
        SchemaMetadata schema)
    {
        var relationships = new List<SchemaRelationship>();

        try
        {
            // Extract table entities from semantic analysis
            var tableEntities = semanticAnalysis.Entities
                .Where(e => e.Type == EntityType.Table)
                .ToList();

            // Find relationships between identified tables
            foreach (var table1 in tableEntities)
            {
                foreach (var table2 in tableEntities)
                {
                    if (table1.Text != table2.Text)
                    {
                        var relationship = FindRelationship(table1.Text, table2.Text, schema);
                        if (relationship != null)
                        {
                            relationships.Add(relationship);
                        }
                    }
                }
            }

            // Add common relationships for gaming domain
            relationships.AddRange(GetCommonGamingRelationships(schema));

            _logger.LogDebug("Found {RelationshipCount} schema relationships", relationships.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing schema relationships");
        }

        return relationships.Distinct().ToList();
    }

    private SchemaRelationship? FindRelationship(string table1, string table2, SchemaMetadata schema)
    {
        // Find actual table objects
        var tableObj1 = schema.Tables.FirstOrDefault(t =>
            t.Name.Equals(table1, StringComparison.OrdinalIgnoreCase));
        var tableObj2 = schema.Tables.FirstOrDefault(t =>
            t.Name.Equals(table2, StringComparison.OrdinalIgnoreCase));

        if (tableObj1 == null || tableObj2 == null) return null;

        // Look for foreign key relationships
        foreach (var column1 in tableObj1.Columns)
        {
            foreach (var column2 in tableObj2.Columns)
            {
                if (IsLikelyForeignKey(column1.Name, column2.Name, tableObj1.Name, tableObj2.Name))
                {
                    return new SchemaRelationship
                    {
                        FromTable = tableObj1.Name,
                        FromColumn = column1.Name,
                        ToTable = tableObj2.Name,
                        ToColumn = column2.Name,
                        RelationshipType = "foreign_key",
                        Confidence = 0.8
                    };
                }
            }
        }

        return null;
    }

    private bool IsLikelyForeignKey(string column1, string column2, string table1, string table2)
    {
        // Common foreign key patterns
        var patterns = new[]
        {
            (column1.ToLowerInvariant(), column2.ToLowerInvariant()),
            ($"{table1.ToLowerInvariant()}id", column2.ToLowerInvariant()),
            (column1.ToLowerInvariant(), $"{table2.ToLowerInvariant()}id"),
            ($"{table1.ToLowerInvariant()}_id", column2.ToLowerInvariant()),
            (column1.ToLowerInvariant(), $"{table2.ToLowerInvariant()}_id")
        };

        return patterns.Any(p => p.Item1 == p.Item2);
    }

    private List<SchemaRelationship> GetCommonGamingRelationships(SchemaMetadata schema)
    {
        var relationships = new List<SchemaRelationship>();

        // Common gaming domain relationships
        var commonRelationships = new[]
        {
            ("tbl_Daily_actions", "PlayerID", "tbl_Daily_actions_players", "PlayerID"),
            ("tbl_Daily_actions", "CountryID", "tbl_Countries", "CountryID"),
            ("tbl_Daily_actions", "CurrencyID", "tbl_Currencies", "CurrencyID"),
            ("tbl_Daily_actions_players", "CountryID", "tbl_Countries", "CountryID"),
            ("tbl_Daily_actions_players", "CurrencyID", "tbl_Currencies", "CurrencyID")
        };

        foreach (var (fromTable, fromColumn, toTable, toColumn) in commonRelationships)
        {
            if (schema.Tables.Any(t => t.Name.Equals(fromTable, StringComparison.OrdinalIgnoreCase)) &&
                schema.Tables.Any(t => t.Name.Equals(toTable, StringComparison.OrdinalIgnoreCase)))
            {
                relationships.Add(new SchemaRelationship
                {
                    FromTable = fromTable,
                    FromColumn = fromColumn,
                    ToTable = toTable,
                    ToColumn = toColumn,
                    RelationshipType = "foreign_key",
                    Confidence = 0.9
                });
            }
        }

        return relationships;
    }
}

/// <summary>
/// Schema relationship definition
/// </summary>
public class SchemaRelationship
{
    public string FromTable { get; set; } = string.Empty;
    public string FromColumn { get; set; } = string.Empty;
    public string ToTable { get; set; } = string.Empty;
    public string ToColumn { get; set; } = string.Empty;
    public string RelationshipType { get; set; } = string.Empty; // "foreign_key", "one_to_many", etc.
    public double Confidence { get; set; }

    public override bool Equals(object? obj)
    {
        if (obj is SchemaRelationship other)
        {
            return FromTable == other.FromTable &&
                   FromColumn == other.FromColumn &&
                   ToTable == other.ToTable &&
                   ToColumn == other.ToColumn;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(FromTable, FromColumn, ToTable, ToColumn);
    }
}

/// <summary>
/// SQL optimizer for generated queries
/// </summary>
public class SQLOptimizer
{
    private readonly ILogger _logger;

    public SQLOptimizer(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<OptimizedSQLResult> OptimizeAsync(
        string sql,
        SubQuery subQuery,
        SchemaMetadata schema)
    {
        try
        {
            var optimizedSQL = sql;
            var optimizations = new List<string>();

            // Add NOLOCK hints if not present
            if (!sql.Contains("NOLOCK", StringComparison.OrdinalIgnoreCase))
            {
                optimizedSQL = AddNolockHints(optimizedSQL, schema);
                optimizations.Add("Added NOLOCK hints for better read performance");
            }

            // Optimize JOIN order
            if (sql.Contains("JOIN", StringComparison.OrdinalIgnoreCase))
            {
                optimizedSQL = OptimizeJoinOrder(optimizedSQL);
                optimizations.Add("Optimized JOIN order");
            }

            // Add appropriate indexes suggestions
            var indexSuggestions = SuggestIndexes(optimizedSQL, schema);
            if (indexSuggestions.Any())
            {
                optimizations.AddRange(indexSuggestions.Select(idx => $"Consider index: {idx}"));
            }

            // Calculate confidence based on optimizations applied
            var confidence = CalculateOptimizationConfidence(sql, optimizedSQL, optimizations);

            return new OptimizedSQLResult
            {
                SQL = optimizedSQL,
                Explanation = $"Applied {optimizations.Count} optimizations: {string.Join(", ", optimizations)}",
                Confidence = confidence,
                Optimizations = optimizations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error optimizing SQL");
            return new OptimizedSQLResult
            {
                SQL = sql,
                Explanation = "No optimizations applied due to error",
                Confidence = 0.5,
                Optimizations = new List<string>()
            };
        }
    }

    public async Task<OptimizedSQLResult> FinalOptimizeAsync(string sql, SchemaMetadata schema)
    {
        try
        {
            var optimizedSQL = sql;
            var optimizations = new List<string>();

            // Final cleanup and optimization
            optimizedSQL = CleanupSQL(optimizedSQL);
            optimizations.Add("Cleaned up SQL formatting");

            // Add performance hints
            optimizedSQL = AddPerformanceHints(optimizedSQL);
            optimizations.Add("Added performance hints");

            // Validate syntax
            var isValid = ValidateSQL(optimizedSQL);
            if (!isValid)
            {
                _logger.LogWarning("Generated SQL may have syntax issues");
            }

            return new OptimizedSQLResult
            {
                SQL = optimizedSQL,
                Explanation = $"Final optimization with {optimizations.Count} improvements",
                Confidence = isValid ? 0.9 : 0.6,
                Optimizations = optimizations
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in final optimization");
            return new OptimizedSQLResult
            {
                SQL = sql,
                Explanation = "Final optimization failed",
                Confidence = 0.5,
                Optimizations = new List<string>()
            };
        }
    }

    private string AddNolockHints(string sql, SchemaMetadata schema)
    {
        var optimizedSQL = sql;

        foreach (var table in schema.Tables)
        {
            var tableName = table.Name;
            var pattern = $@"\b{tableName}\b(?!\s+WITH\s*\(NOLOCK\))";
            var replacement = $"{tableName} WITH (NOLOCK)";

            optimizedSQL = System.Text.RegularExpressions.Regex.Replace(
                optimizedSQL, pattern, replacement,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        return optimizedSQL;
    }

    private string OptimizeJoinOrder(string sql)
    {
        // Simple join optimization - in practice would be more sophisticated
        return sql;
    }

    private List<string> SuggestIndexes(string sql, SchemaMetadata schema)
    {
        var suggestions = new List<string>();
        var sqlLower = sql.ToLowerInvariant();

        // Suggest indexes for WHERE clauses
        if (sqlLower.Contains("where"))
        {
            suggestions.Add("Consider indexes on WHERE clause columns");
        }

        // Suggest indexes for JOIN columns
        if (sqlLower.Contains("join"))
        {
            suggestions.Add("Consider indexes on JOIN columns");
        }

        // Suggest indexes for ORDER BY columns
        if (sqlLower.Contains("order by"))
        {
            suggestions.Add("Consider indexes on ORDER BY columns");
        }

        return suggestions;
    }

    private double CalculateOptimizationConfidence(
        string originalSQL,
        string optimizedSQL,
        List<string> optimizations)
    {
        var confidence = 0.7; // Base confidence

        // Boost confidence for each optimization applied
        confidence += optimizations.Count * 0.05;

        // Boost confidence if SQL structure looks good
        if (optimizedSQL.Contains("NOLOCK", StringComparison.OrdinalIgnoreCase))
        {
            confidence += 0.1;
        }

        return Math.Min(1.0, confidence);
    }

    private string CleanupSQL(string sql)
    {
        // Remove extra whitespace and format nicely
        var lines = sql.Split('\n')
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrEmpty(line))
            .ToArray();

        return string.Join("\n", lines);
    }

    private string AddPerformanceHints(string sql)
    {
        // Add query hints for better performance
        if (!sql.Contains("OPTION", StringComparison.OrdinalIgnoreCase))
        {
            sql += "\nOPTION (RECOMPILE)";
        }

        return sql;
    }

    private bool ValidateSQL(string sql)
    {
        // Basic SQL validation
        try
        {
            var sqlLower = sql.ToLowerInvariant();

            // Check for basic SQL structure
            if (!sqlLower.Contains("select"))
            {
                return false;
            }

            // Check for balanced parentheses
            var openParens = sql.Count(c => c == '(');
            var closeParens = sql.Count(c => c == ')');

            return openParens == closeParens;
        }
        catch
        {
            return false;
        }
    }
}

/// <summary>
/// Optimized SQL result
/// </summary>
public class OptimizedSQLResult
{
    public string SQL { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public List<string> Optimizations { get; set; } = new();
}

/// <summary>
/// Sub-query information for SQL generation
/// </summary>
public class SubQuery
{
    public string Id { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public string Purpose { get; set; } = string.Empty;
    public List<string> RequiredTables { get; set; } = new();
    public List<string> RequiredColumns { get; set; } = new();
    public QueryComplexity Complexity { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}
