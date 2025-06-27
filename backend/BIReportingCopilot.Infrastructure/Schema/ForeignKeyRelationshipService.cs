using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Extensions;
using BIReportingCopilot.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using System.Data;

namespace BIReportingCopilot.Infrastructure.Schema;

/// <summary>
/// Service for discovering and mapping foreign key relationships between database tables
/// </summary>
public class ForeignKeyRelationshipService : IForeignKeyRelationshipService
{
    private readonly ILogger<ForeignKeyRelationshipService> _logger;
    private readonly IConnectionStringProvider _connectionStringProvider;
    private readonly ICacheService _cacheService;

    public ForeignKeyRelationshipService(
        ILogger<ForeignKeyRelationshipService> logger,
        IConnectionStringProvider connectionStringProvider,
        ICacheService cacheService)
    {
        _logger = logger;
        _connectionStringProvider = connectionStringProvider;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Discover all foreign key relationships in the database
    /// </summary>
    public async Task<List<ForeignKeyRelationship>> DiscoverAllForeignKeyRelationshipsAsync(
        string connectionStringName = "BIDatabase")
    {
        var cacheKey = $"fk_relationships_{connectionStringName}";
        
        // Try cache first
        var (found, cachedRelationships) = await _cacheService.TryGetAsync<List<ForeignKeyRelationship>>(cacheKey);
        if (found && cachedRelationships != null)
        {
            _logger.LogDebug("Returning {Count} foreign key relationships from cache", cachedRelationships.Count);
            return cachedRelationships;
        }

        try
        {
            _logger.LogInformation("🔍 Discovering foreign key relationships from database");
            
            var connectionString = await _connectionStringProvider.GetConnectionStringAsync(connectionStringName);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found");
            }

            var relationships = new List<ForeignKeyRelationship>();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT 
                    fk.name AS ForeignKeyName,
                    tp.name AS ParentTable,
                    sp.name AS ParentSchema,
                    cp.name AS ParentColumn,
                    tr.name AS ReferencedTable,
                    sr.name AS ReferencedSchema,
                    cr.name AS ReferencedColumn,
                    fk.delete_referential_action_desc AS DeleteAction,
                    fk.update_referential_action_desc AS UpdateAction,
                    fk.is_disabled AS IsDisabled
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN sys.tables tp ON fkc.parent_object_id = tp.object_id
                INNER JOIN sys.schemas sp ON tp.schema_id = sp.schema_id
                INNER JOIN sys.columns cp ON fkc.parent_object_id = cp.object_id AND fkc.parent_column_id = cp.column_id
                INNER JOIN sys.tables tr ON fkc.referenced_object_id = tr.object_id
                INNER JOIN sys.schemas sr ON tr.schema_id = sr.schema_id
                INNER JOIN sys.columns cr ON fkc.referenced_object_id = cr.object_id AND fkc.referenced_column_id = cr.column_id
                ORDER BY fk.name, fkc.constraint_column_id";

            using var command = new SqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var relationship = new ForeignKeyRelationship
                {
                    ConstraintName = reader.GetString("ForeignKeyName"),
                    ParentTable = $"{reader.GetString("ParentSchema")}.{reader.GetString("ParentTable")}",
                    ParentColumn = reader.GetString("ParentColumn"),
                    ReferencedTable = $"{reader.GetString("ReferencedSchema")}.{reader.GetString("ReferencedTable")}",
                    ReferencedColumn = reader.GetString("ReferencedColumn"),
                    DeleteAction = reader.GetString("DeleteAction"),
                    UpdateAction = reader.GetString("UpdateAction"),
                    IsEnabled = !reader.GetBoolean("IsDisabled"),
                    RelationshipType = DetermineRelationshipType(
                        reader.GetString("ParentTable"), 
                        reader.GetString("ReferencedTable"),
                        reader.GetString("ParentColumn"))
                };

                relationships.Add(relationship);
            }

            _logger.LogInformation("✅ Discovered {Count} foreign key relationships", relationships.Count);

            // Cache the results for 1 hour
            await _cacheService.SetAsync(cacheKey, relationships, TimeSpan.FromHours(1));

            return relationships;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error discovering foreign key relationships");
            throw;
        }
    }

    /// <summary>
    /// Find related tables for a specific table through foreign key relationships
    /// </summary>
    public async Task<List<RelatedTableInfo>> FindRelatedTablesAsync(
        string tableName, 
        int maxDepth = 2,
        string connectionStringName = "BIDatabase")
    {
        try
        {
            _logger.LogDebug("🔍 Finding related tables for '{TableName}' with max depth {MaxDepth}", tableName, maxDepth);

            var allRelationships = await DiscoverAllForeignKeyRelationshipsAsync(connectionStringName);
            var relatedTables = new List<RelatedTableInfo>();
            var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            await FindRelatedTablesRecursive(tableName, allRelationships, relatedTables, visited, 0, maxDepth);

            _logger.LogDebug("✅ Found {Count} related tables for '{TableName}'", relatedTables.Count, tableName);
            return relatedTables.OrderByDescending(t => t.RelevanceScore).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error finding related tables for '{TableName}'", tableName);
            throw;
        }
    }

    /// <summary>
    /// Get foreign key relationships for specific tables
    /// </summary>
    public async Task<List<ForeignKeyRelationship>> GetRelationshipsForTablesAsync(
        List<string> tableNames,
        string connectionStringName = "BIDatabase")
    {
        try
        {
            var allRelationships = await DiscoverAllForeignKeyRelationshipsAsync(connectionStringName);
            
            var normalizedTableNames = tableNames.Select(NormalizeTableName).ToHashSet(StringComparer.OrdinalIgnoreCase);
            
            var relevantRelationships = allRelationships.Where(r =>
                normalizedTableNames.Contains(NormalizeTableName(r.ParentTable)) ||
                normalizedTableNames.Contains(NormalizeTableName(r.ReferencedTable)))
                .ToList();

            _logger.LogDebug("Found {Count} relationships for {TableCount} tables", 
                relevantRelationships.Count, tableNames.Count);

            return relevantRelationships;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting relationships for tables");
            throw;
        }
    }

    /// <summary>
    /// Generate optimal join paths between tables
    /// </summary>
    public async Task<List<JoinPath>> GenerateJoinPathsAsync(
        List<string> tableNames,
        string connectionStringName = "BIDatabase")
    {
        try
        {
            _logger.LogDebug("🔍 Generating join paths for {Count} tables", tableNames.Count);

            var relationships = await GetRelationshipsForTablesAsync(tableNames, connectionStringName);
            var joinPaths = new List<JoinPath>();

            // Build a graph of table relationships
            var tableGraph = BuildTableGraph(relationships);
            
            // Find optimal paths between all table pairs
            for (int i = 0; i < tableNames.Count; i++)
            {
                for (int j = i + 1; j < tableNames.Count; j++)
                {
                    var fromTable = NormalizeTableName(tableNames[i]);
                    var toTable = NormalizeTableName(tableNames[j]);
                    
                    var path = FindShortestPath(tableGraph, fromTable, toTable);
                    if (path != null)
                    {
                        joinPaths.Add(path);
                    }
                }
            }

            _logger.LogDebug("✅ Generated {Count} join paths", joinPaths.Count);
            return joinPaths.OrderBy(jp => jp.PathLength).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating join paths");
            throw;
        }
    }

    #region Private Helper Methods

    private async Task FindRelatedTablesRecursive(
        string currentTable,
        List<ForeignKeyRelationship> allRelationships,
        List<RelatedTableInfo> relatedTables,
        HashSet<string> visited,
        int currentDepth,
        int maxDepth)
    {
        if (currentDepth >= maxDepth || visited.Contains(currentTable))
            return;

        visited.Add(currentTable);
        var normalizedCurrentTable = NormalizeTableName(currentTable);

        // Find direct relationships (both directions)
        var directRelationships = allRelationships.Where(r =>
            NormalizeTableName(r.ParentTable).Equals(normalizedCurrentTable, StringComparison.OrdinalIgnoreCase) ||
            NormalizeTableName(r.ReferencedTable).Equals(normalizedCurrentTable, StringComparison.OrdinalIgnoreCase))
            .ToList();

        foreach (var relationship in directRelationships)
        {
            var relatedTableName = NormalizeTableName(relationship.ParentTable).Equals(normalizedCurrentTable, StringComparison.OrdinalIgnoreCase)
                ? relationship.ReferencedTable
                : relationship.ParentTable;

            if (!visited.Contains(relatedTableName))
            {
                var relevanceScore = CalculateRelevanceScore(relationship, currentDepth);
                var relationshipDirection = DetermineRelationshipDirection(relationship, currentTable);

                relatedTables.Add(new RelatedTableInfo
                {
                    TableName = relatedTableName,
                    RelevanceScore = relevanceScore,
                    RelationshipType = relationship.RelationshipType,
                    JoinColumn = relationshipDirection == "outgoing" ? relationship.ParentColumn : relationship.ReferencedColumn,
                    ReferencedColumn = relationshipDirection == "outgoing" ? relationship.ReferencedColumn : relationship.ParentColumn,
                    Distance = currentDepth + 1,
                    RelationshipDirection = relationshipDirection,
                    ConstraintName = relationship.ConstraintName
                });

                // Recursively find related tables
                await FindRelatedTablesRecursive(relatedTableName, allRelationships, relatedTables, visited, currentDepth + 1, maxDepth);
            }
        }
    }

    private string DetermineRelationshipType(string parentTable, string referencedTable, string parentColumn)
    {
        // Simple heuristics to determine relationship type
        if (parentColumn.ToLower().EndsWith("_id") || parentColumn.ToLower() == "id")
        {
            return "ManyToOne";
        }
        
        return "OneToMany";
    }

    private double CalculateRelevanceScore(ForeignKeyRelationship relationship, int depth)
    {
        var baseScore = 1.0 - (depth * 0.2); // Decrease score with distance
        
        // Boost score for enabled relationships
        if (relationship.IsEnabled)
            baseScore += 0.1;
            
        // Boost score for common relationship patterns
        if (relationship.ParentColumn.ToLower().Contains("id"))
            baseScore += 0.1;
            
        return Math.Max(baseScore, 0.1);
    }

    private string DetermineRelationshipDirection(ForeignKeyRelationship relationship, string currentTable)
    {
        var normalizedCurrentTable = NormalizeTableName(currentTable);
        var normalizedParentTable = NormalizeTableName(relationship.ParentTable);
        
        return normalizedParentTable.Equals(normalizedCurrentTable, StringComparison.OrdinalIgnoreCase) 
            ? "outgoing" 
            : "incoming";
    }

    private string NormalizeTableName(string tableName)
    {
        // Remove schema prefix if present and normalize case
        var parts = tableName.Split('.');
        return parts.Length > 1 ? parts[1].Trim() : tableName.Trim();
    }

    private Dictionary<string, List<(string Target, ForeignKeyRelationship Relationship)>> BuildTableGraph(
        List<ForeignKeyRelationship> relationships)
    {
        var graph = new Dictionary<string, List<(string Target, ForeignKeyRelationship Relationship)>>(StringComparer.OrdinalIgnoreCase);

        foreach (var rel in relationships)
        {
            var parentTable = NormalizeTableName(rel.ParentTable);
            var referencedTable = NormalizeTableName(rel.ReferencedTable);

            // Add bidirectional edges
            if (!graph.ContainsKey(parentTable))
                graph[parentTable] = new List<(string, ForeignKeyRelationship)>();
            if (!graph.ContainsKey(referencedTable))
                graph[referencedTable] = new List<(string, ForeignKeyRelationship)>();

            graph[parentTable].Add((referencedTable, rel));
            graph[referencedTable].Add((parentTable, rel));
        }

        return graph;
    }

    private JoinPath? FindShortestPath(
        Dictionary<string, List<(string Target, ForeignKeyRelationship Relationship)>> graph,
        string fromTable,
        string toTable)
    {
        if (fromTable.Equals(toTable, StringComparison.OrdinalIgnoreCase))
            return null;

        var queue = new Queue<(string Table, List<ForeignKeyRelationship> Path)>();
        var visited = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        queue.Enqueue((fromTable, new List<ForeignKeyRelationship>()));
        visited.Add(fromTable);

        while (queue.Count > 0)
        {
            var (currentTable, currentPath) = queue.Dequeue();

            if (!graph.ContainsKey(currentTable))
                continue;

            foreach (var (targetTable, relationship) in graph[currentTable])
            {
                if (targetTable.Equals(toTable, StringComparison.OrdinalIgnoreCase))
                {
                    var finalPath = new List<ForeignKeyRelationship>(currentPath) { relationship };
                    return CreateJoinPath(fromTable, toTable, finalPath);
                }

                if (!visited.Contains(targetTable))
                {
                    visited.Add(targetTable);
                    var newPath = new List<ForeignKeyRelationship>(currentPath) { relationship };
                    queue.Enqueue((targetTable, newPath));
                }
            }
        }

        return null; // No path found
    }

    private JoinPath CreateJoinPath(string fromTable, string toTable, List<ForeignKeyRelationship> relationships)
    {
        var joinConditions = relationships.Select(r => new JoinCondition
        {
            LeftTable = r.ParentTable,
            LeftColumn = r.ParentColumn,
            RightTable = r.ReferencedTable,
            RightColumn = r.ReferencedColumn,
            Operator = "="
        }).ToList();

        return new JoinPath
        {
            FromTable = fromTable,
            ToTable = toTable,
            JoinConditions = joinConditions,
            PathLength = relationships.Count,
            IsOptimal = relationships.Count <= 2, // Consider paths with 2 or fewer hops as optimal
            PerformanceScore = CalculatePathPerformanceScore(relationships)
        };
    }

    private double CalculatePathPerformanceScore(List<ForeignKeyRelationship> relationships)
    {
        var baseScore = 1.0;
        
        // Penalize longer paths
        baseScore -= (relationships.Count - 1) * 0.2;
        
        // Boost score for enabled relationships
        var enabledCount = relationships.Count(r => r.IsEnabled);
        baseScore += (enabledCount / (double)relationships.Count) * 0.2;
        
        return Math.Max(baseScore, 0.1);
    }

    #endregion
}
