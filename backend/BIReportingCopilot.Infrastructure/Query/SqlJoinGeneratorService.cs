using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BIReportingCopilot.Infrastructure.Query;

/// <summary>
/// Service for generating optimal SQL JOINs based on foreign key relationships
/// </summary>
public class SqlJoinGeneratorService : ISqlJoinGeneratorService
{
    private readonly ILogger<SqlJoinGeneratorService> _logger;
    private readonly IForeignKeyRelationshipService _relationshipService;

    public SqlJoinGeneratorService(
        ILogger<SqlJoinGeneratorService> logger,
        IForeignKeyRelationshipService relationshipService)
    {
        _logger = logger;
        _relationshipService = relationshipService;
    }

    /// <summary>
    /// Generate SQL JOINs for a list of tables based on their relationships
    /// </summary>
    public async Task<SqlJoinResult> GenerateJoinsAsync(
        List<string> tableNames,
        string? primaryTable = null,
        JoinStrategy strategy = JoinStrategy.Optimal)
    {
        try
        {
            _logger.LogDebug("🔗 Generating JOINs for {Count} tables with strategy {Strategy}", 
                tableNames.Count, strategy);

            if (tableNames.Count <= 1)
            {
                return new SqlJoinResult
                {
                    Success = true,
                    JoinClause = string.Empty,
                    TableAliases = tableNames.ToDictionary(t => t, t => GetTableAlias(t)),
                    JoinPaths = new List<JoinPath>()
                };
            }

            // Get foreign key relationships for these tables
            var relationships = await _relationshipService.GetRelationshipsForTablesAsync(tableNames);
            
            // Generate join paths
            var joinPaths = await _relationshipService.GenerateJoinPathsAsync(tableNames);
            
            // Determine the primary table (main table to start from)
            var mainTable = DeterminePrimaryTable(tableNames, relationships, primaryTable);
            
            // Generate the JOIN clause based on strategy
            var joinClause = strategy switch
            {
                JoinStrategy.Optimal => await GenerateOptimalJoins(mainTable, tableNames, relationships, joinPaths),
                JoinStrategy.LeftJoin => await GenerateLeftJoins(mainTable, tableNames, relationships),
                JoinStrategy.InnerJoin => await GenerateInnerJoins(mainTable, tableNames, relationships),
                JoinStrategy.MinimalPath => await GenerateMinimalPathJoins(mainTable, tableNames, joinPaths),
                _ => await GenerateOptimalJoins(mainTable, tableNames, relationships, joinPaths)
            };

            // Generate table aliases
            var tableAliases = GenerateTableAliases(tableNames);

            var result = new SqlJoinResult
            {
                Success = true,
                JoinClause = joinClause,
                TableAliases = tableAliases,
                JoinPaths = joinPaths,
                PrimaryTable = mainTable,
                JoinStrategy = strategy,
                Metadata = new Dictionary<string, object>
                {
                    ["RelationshipCount"] = relationships.Count,
                    ["JoinPathCount"] = joinPaths.Count,
                    ["TableCount"] = tableNames.Count
                }
            };

            _logger.LogDebug("✅ Generated JOINs successfully for {Count} tables", tableNames.Count);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating JOINs for tables");
            return new SqlJoinResult
            {
                Success = false,
                Error = ex.Message,
                JoinClause = string.Empty,
                TableAliases = new Dictionary<string, string>(),
                JoinPaths = new List<JoinPath>()
            };
        }
    }

    /// <summary>
    /// Generate table aliases for SQL queries
    /// </summary>
    public Dictionary<string, string> GenerateTableAliases(List<string> tableNames)
    {
        var aliases = new Dictionary<string, string>();
        var usedAliases = new HashSet<string>();

        foreach (var tableName in tableNames)
        {
            var alias = GetTableAlias(tableName);
            
            // Ensure uniqueness
            var originalAlias = alias;
            var counter = 1;
            while (usedAliases.Contains(alias))
            {
                alias = $"{originalAlias}{counter}";
                counter++;
            }
            
            aliases[tableName] = alias;
            usedAliases.Add(alias);
        }

        return aliases;
    }

    /// <summary>
    /// Validate that all required tables can be joined
    /// </summary>
    public async Task<JoinValidationResult> ValidateJoinabilityAsync(List<string> tableNames)
    {
        try
        {
            var relationships = await _relationshipService.GetRelationshipsForTablesAsync(tableNames);
            var joinPaths = await _relationshipService.GenerateJoinPathsAsync(tableNames);

            var connectedTables = new HashSet<string>();
            var isolatedTables = new List<string>();

            // Check connectivity
            if (joinPaths.Any())
            {
                foreach (var path in joinPaths)
                {
                    connectedTables.Add(NormalizeTableName(path.FromTable));
                    connectedTables.Add(NormalizeTableName(path.ToTable));
                }
            }

            foreach (var tableName in tableNames)
            {
                if (!connectedTables.Contains(NormalizeTableName(tableName)))
                {
                    isolatedTables.Add(tableName);
                }
            }

            return new JoinValidationResult
            {
                IsValid = isolatedTables.Count == 0,
                ConnectedTables = connectedTables.ToList(),
                IsolatedTables = isolatedTables,
                AvailableRelationships = relationships.Count,
                Warnings = isolatedTables.Count > 0 
                    ? new List<string> { $"Tables cannot be joined: {string.Join(", ", isolatedTables)}" }
                    : new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating joinability");
            return new JoinValidationResult
            {
                IsValid = false,
                Warnings = new List<string> { $"Validation error: {ex.Message}" }
            };
        }
    }

    #region Private Helper Methods

    private async Task<string> GenerateOptimalJoins(
        string primaryTable, 
        List<string> tableNames, 
        List<ForeignKeyRelationship> relationships,
        List<JoinPath> joinPaths)
    {
        var joinClause = new StringBuilder();
        var processedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { primaryTable };
        var remainingTables = tableNames.Where(t => !t.Equals(primaryTable, StringComparison.OrdinalIgnoreCase)).ToList();

        // Start with the primary table
        var primaryAlias = GetTableAlias(primaryTable);
        joinClause.AppendLine($"FROM [{NormalizeTableName(primaryTable)}] {primaryAlias} WITH (NOLOCK)");

        // Process tables in order of optimal join paths
        var sortedPaths = joinPaths.OrderBy(jp => jp.PathLength).ThenByDescending(jp => jp.PerformanceScore);

        foreach (var path in sortedPaths)
        {
            var fromTable = NormalizeTableName(path.FromTable);
            var toTable = NormalizeTableName(path.ToTable);

            string tableToJoin = null;
            string joinDirection = null;

            if (processedTables.Contains(fromTable) && !processedTables.Contains(toTable))
            {
                tableToJoin = toTable;
                joinDirection = "forward";
            }
            else if (processedTables.Contains(toTable) && !processedTables.Contains(fromTable))
            {
                tableToJoin = fromTable;
                joinDirection = "reverse";
            }

            if (tableToJoin != null && remainingTables.Any(t => NormalizeTableName(t).Equals(tableToJoin, StringComparison.OrdinalIgnoreCase)))
            {
                var joinType = DetermineOptimalJoinType(path, relationships);
                var joinConditions = GenerateJoinConditions(path, joinDirection);
                var tableAlias = GetTableAlias(tableToJoin);

                joinClause.AppendLine($"{joinType} JOIN [{tableToJoin}] {tableAlias} WITH (NOLOCK) ON {joinConditions}");
                
                processedTables.Add(tableToJoin);
                remainingTables.RemoveAll(t => NormalizeTableName(t).Equals(tableToJoin, StringComparison.OrdinalIgnoreCase));
            }
        }

        // Handle any remaining unconnected tables with CROSS JOIN (last resort)
        foreach (var remainingTable in remainingTables)
        {
            var tableAlias = GetTableAlias(remainingTable);
            joinClause.AppendLine($"CROSS JOIN [{NormalizeTableName(remainingTable)}] {tableAlias} WITH (NOLOCK)");
            _logger.LogWarning("⚠️ Using CROSS JOIN for unconnected table: {Table}", remainingTable);
        }

        return joinClause.ToString();
    }

    private async Task<string> GenerateLeftJoins(string primaryTable, List<string> tableNames, List<ForeignKeyRelationship> relationships)
    {
        var joinClause = new StringBuilder();
        var primaryAlias = GetTableAlias(primaryTable);
        joinClause.AppendLine($"FROM [{NormalizeTableName(primaryTable)}] {primaryAlias} WITH (NOLOCK)");

        foreach (var tableName in tableNames.Where(t => !t.Equals(primaryTable, StringComparison.OrdinalIgnoreCase)))
        {
            var relationship = FindRelationship(primaryTable, tableName, relationships);
            if (relationship != null)
            {
                var tableAlias = GetTableAlias(tableName);
                var joinCondition = GenerateJoinCondition(relationship, primaryTable, tableName);
                joinClause.AppendLine($"LEFT JOIN [{NormalizeTableName(tableName)}] {tableAlias} WITH (NOLOCK) ON {joinCondition}");
            }
        }

        return joinClause.ToString();
    }

    private async Task<string> GenerateInnerJoins(string primaryTable, List<string> tableNames, List<ForeignKeyRelationship> relationships)
    {
        var joinClause = new StringBuilder();
        var primaryAlias = GetTableAlias(primaryTable);
        joinClause.AppendLine($"FROM [{NormalizeTableName(primaryTable)}] {primaryAlias} WITH (NOLOCK)");

        foreach (var tableName in tableNames.Where(t => !t.Equals(primaryTable, StringComparison.OrdinalIgnoreCase)))
        {
            var relationship = FindRelationship(primaryTable, tableName, relationships);
            if (relationship != null)
            {
                var tableAlias = GetTableAlias(tableName);
                var joinCondition = GenerateJoinCondition(relationship, primaryTable, tableName);
                joinClause.AppendLine($"INNER JOIN [{NormalizeTableName(tableName)}] {tableAlias} WITH (NOLOCK) ON {joinCondition}");
            }
        }

        return joinClause.ToString();
    }

    private async Task<string> GenerateMinimalPathJoins(string primaryTable, List<string> tableNames, List<JoinPath> joinPaths)
    {
        var joinClause = new StringBuilder();
        var primaryAlias = GetTableAlias(primaryTable);
        joinClause.AppendLine($"FROM [{NormalizeTableName(primaryTable)}] {primaryAlias} WITH (NOLOCK)");

        // Use only the shortest paths
        var minimalPaths = joinPaths.Where(jp => jp.IsOptimal).OrderBy(jp => jp.PathLength);

        foreach (var path in minimalPaths)
        {
            foreach (var condition in path.JoinConditions)
            {
                var leftAlias = GetTableAlias(condition.LeftTable);
                var rightAlias = GetTableAlias(condition.RightTable);
                var joinCondition = $"{leftAlias}.{condition.LeftColumn} = {rightAlias}.{condition.RightColumn}";
                
                joinClause.AppendLine($"INNER JOIN [{NormalizeTableName(condition.RightTable)}] {rightAlias} WITH (NOLOCK) ON {joinCondition}");
            }
        }

        return joinClause.ToString();
    }

    private string DeterminePrimaryTable(List<string> tableNames, List<ForeignKeyRelationship> relationships, string? preferredPrimary)
    {
        if (!string.IsNullOrEmpty(preferredPrimary) && tableNames.Contains(preferredPrimary))
        {
            return preferredPrimary;
        }

        // Choose table with most relationships as primary
        var tableCounts = new Dictionary<string, int>();
        foreach (var table in tableNames)
        {
            var normalizedTable = NormalizeTableName(table);
            tableCounts[table] = relationships.Count(r => 
                NormalizeTableName(r.ParentTable).Equals(normalizedTable, StringComparison.OrdinalIgnoreCase) ||
                NormalizeTableName(r.ReferencedTable).Equals(normalizedTable, StringComparison.OrdinalIgnoreCase));
        }

        return tableCounts.OrderByDescending(kvp => kvp.Value).First().Key;
    }

    private string DetermineOptimalJoinType(JoinPath path, List<ForeignKeyRelationship> relationships)
    {
        // Default to INNER JOIN for optimal performance
        // Could be enhanced with business logic to determine when LEFT JOIN is needed
        return "INNER";
    }

    private string GenerateJoinConditions(JoinPath path, string direction)
    {
        var conditions = new List<string>();
        
        foreach (var condition in path.JoinConditions)
        {
            var leftAlias = GetTableAlias(condition.LeftTable);
            var rightAlias = GetTableAlias(condition.RightTable);
            
            if (direction == "reverse")
            {
                conditions.Add($"{rightAlias}.{condition.RightColumn} = {leftAlias}.{condition.LeftColumn}");
            }
            else
            {
                conditions.Add($"{leftAlias}.{condition.LeftColumn} = {rightAlias}.{condition.RightColumn}");
            }
        }

        return string.Join(" AND ", conditions);
    }

    private ForeignKeyRelationship? FindRelationship(string table1, string table2, List<ForeignKeyRelationship> relationships)
    {
        var normalized1 = NormalizeTableName(table1);
        var normalized2 = NormalizeTableName(table2);

        return relationships.FirstOrDefault(r =>
            (NormalizeTableName(r.ParentTable).Equals(normalized1, StringComparison.OrdinalIgnoreCase) &&
             NormalizeTableName(r.ReferencedTable).Equals(normalized2, StringComparison.OrdinalIgnoreCase)) ||
            (NormalizeTableName(r.ParentTable).Equals(normalized2, StringComparison.OrdinalIgnoreCase) &&
             NormalizeTableName(r.ReferencedTable).Equals(normalized1, StringComparison.OrdinalIgnoreCase)));
    }

    private string GenerateJoinCondition(ForeignKeyRelationship relationship, string table1, string table2)
    {
        var alias1 = GetTableAlias(table1);
        var alias2 = GetTableAlias(table2);
        
        var normalized1 = NormalizeTableName(table1);
        var normalizedParent = NormalizeTableName(relationship.ParentTable);

        if (normalized1.Equals(normalizedParent, StringComparison.OrdinalIgnoreCase))
        {
            return $"{alias1}.{relationship.ParentColumn} = {alias2}.{relationship.ReferencedColumn}";
        }
        else
        {
            return $"{alias1}.{relationship.ReferencedColumn} = {alias2}.{relationship.ParentColumn}";
        }
    }

    private string GetTableAlias(string tableName)
    {
        var normalized = NormalizeTableName(tableName);
        
        // Generate meaningful aliases
        if (normalized.StartsWith("tbl_", StringComparison.OrdinalIgnoreCase))
        {
            var withoutPrefix = normalized.Substring(4);
            var parts = withoutPrefix.Split('_');
            return string.Join("", parts.Select(p => p.Substring(0, Math.Min(2, p.Length)))).ToLower();
        }
        
        // For other tables, use first few characters
        return normalized.Length > 3 ? normalized.Substring(0, 3).ToLower() : normalized.ToLower();
    }

    private string NormalizeTableName(string tableName)
    {
        var parts = tableName.Split('.');
        return parts.Length > 1 ? parts[1].Trim() : tableName.Trim();
    }

    #endregion
}
