using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using System.Collections.Concurrent;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Sharded schema service for handling large datasets across multiple database shards
/// </summary>
public class ShardedSchemaService : ISchemaService
{
    private readonly ILogger<ShardedSchemaService> _logger;
    private readonly ShardingConfiguration _config;
    private readonly ConcurrentDictionary<string, ISchemaService> _shardServices;
    private readonly IServiceProvider _serviceProvider;
    private readonly ICacheService _cacheService;
    private readonly ShardRouter _shardRouter;

    public ShardedSchemaService(
        ILogger<ShardedSchemaService> logger,
        IOptions<ShardingConfiguration> config,
        IServiceProvider serviceProvider,
        ICacheService cacheService)
    {
        _logger = logger;
        _config = config.Value;
        _serviceProvider = serviceProvider;
        _cacheService = cacheService;
        _shardServices = new ConcurrentDictionary<string, ISchemaService>();
        _shardRouter = new ShardRouter(_config, logger);
    }

    public async Task<SchemaMetadata> GetSchemaAsync(string? databaseName = null)
    {
        try
        {
            var cacheKey = $"sharded_schema:{databaseName ?? "default"}";
            var cachedSchema = await _cacheService.GetAsync<SchemaMetadata>(cacheKey);

            if (cachedSchema != null)
            {
                _logger.LogDebug("Retrieved sharded schema from cache for database: {Database}", databaseName);
                return cachedSchema;
            }

            _logger.LogInformation("Building sharded schema for database: {Database}", databaseName);

            var aggregatedSchema = new SchemaMetadata
            {
                DatabaseName = databaseName ?? "ShardedDatabase",
                Tables = new List<TableMetadata>(),
                LastUpdated = DateTime.UtcNow
            };

            // Get schema from all shards in parallel
            var shardTasks = _config.Shards.Select(async shard =>
            {
                try
                {
                    var shardService = await GetShardServiceAsync(shard.ShardId);
                    var shardSchema = await shardService.GetSchemaAsync(shard.DatabaseName);

                    return new { ShardId = shard.ShardId, Schema = shardSchema };
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting schema from shard {ShardId}", shard.ShardId);
                    return null;
                }
            });

            var shardResults = await Task.WhenAll(shardTasks);

            // Aggregate schemas from all shards
            foreach (var result in shardResults.Where(r => r != null))
            {
                foreach (var table in result!.Schema.Tables)
                {
                    var existingTable = aggregatedSchema.Tables.FirstOrDefault(t =>
                        t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase));

                    if (existingTable == null)
                    {
                        // Add new table with shard information
                        var shardedTable = new TableMetadata
                        {
                            Name = table.Name,
                            Schema = table.Schema,
                            Columns = table.Columns,
                            RowCount = table.RowCount,
                            Description = table.Description
                        };

                        // Note: TableMetadata doesn't have Metadata property, so we'll skip metadata for now
                        // In a real implementation, you might want to extend TableMetadata or use a different approach

                        aggregatedSchema.Tables.Add(shardedTable);
                    }
                    else
                    {
                        // Merge table information from multiple shards
                        MergeTableInfo(existingTable, table, result.ShardId);
                    }
                }
            }

            // Cache the aggregated schema
            await _cacheService.SetAsync(cacheKey, aggregatedSchema, TimeSpan.FromMinutes(_config.SchemaCacheMinutes));

            _logger.LogInformation("Built sharded schema with {TableCount} tables from {ShardCount} shards",
                aggregatedSchema.Tables.Count, _config.Shards.Count);

            return aggregatedSchema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sharded schema for database: {Database}", databaseName);
            throw;
        }
    }

    public async Task<TableMetadata?> GetTableInfoAsync(string tableName, string? schemaName = null)
    {
        try
        {
            var cacheKey = $"sharded_table:{schemaName}:{tableName}";
            var cachedTable = await _cacheService.GetAsync<TableMetadata>(cacheKey);

            if (cachedTable != null)
            {
                return cachedTable;
            }

            // Determine which shard(s) contain this table
            var targetShards = await _shardRouter.GetShardsForTableAsync(tableName, schemaName);

            if (!targetShards.Any())
            {
                _logger.LogWarning("Table {Schema}.{Table} not found in any shard", schemaName, tableName);
                return null;
            }

            // Get table info from the first shard that contains it
            var primaryShard = targetShards.First();
            var shardService = await GetShardServiceAsync(primaryShard.ShardId);
            var tableInfo = await shardService.GetTableInfoAsync(tableName, schemaName);

            if (tableInfo != null)
            {
                // Add sharding metadata - TableMetadata doesn't have Metadata property, so we'll skip this for now
                // tableInfo.Metadata = tableInfo.Metadata ?? new Dictionary<string, object>();
                // tableInfo.Metadata["ShardId"] = primaryShard.ShardId;
                // tableInfo.Metadata["IsSharded"] = targetShards.Count > 1;
                // tableInfo.Metadata["ShardCount"] = targetShards.Count;

                if (targetShards.Count > 1)
                {
                    // tableInfo.Metadata["AllShards"] = targetShards.Select(s => s.ShardId).ToArray();

                    // Aggregate row counts from all shards
                    var totalRowCount = await GetTotalRowCountAsync(tableName, schemaName, targetShards);
                    tableInfo.RowCount = totalRowCount;
                }

                await _cacheService.SetAsync(cacheKey, tableInfo, TimeSpan.FromMinutes(_config.TableCacheMinutes));
            }

            return tableInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table info for {Schema}.{Table}", schemaName, tableName);
            throw;
        }
    }

    public async Task<List<string>> GetTableNamesAsync(string? schemaName = null)
    {
        try
        {
            var cacheKey = $"sharded_table_names:{schemaName ?? "all"}";
            var cachedNames = await _cacheService.GetAsync<List<string>>(cacheKey);

            if (cachedNames != null)
            {
                return cachedNames;
            }

            var allTableNames = new HashSet<string>();

            // Get table names from all shards
            var shardTasks = _config.Shards.Select(async shard =>
            {
                try
                {
                    var shardService = await GetShardServiceAsync(shard.ShardId);
                    return await shardService.GetTableNamesAsync(schemaName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error getting table names from shard {ShardId}", shard.ShardId);
                    return new List<string>();
                }
            });

            var shardResults = await Task.WhenAll(shardTasks);

            foreach (var tableNames in shardResults)
            {
                foreach (var tableName in tableNames)
                {
                    allTableNames.Add(tableName);
                }
            }

            var result = allTableNames.ToList();
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_config.TableCacheMinutes));

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table names for schema: {Schema}", schemaName);
            throw;
        }
    }

    public async Task RefreshSchemaAsync(string? databaseName = null)
    {
        try
        {
            _logger.LogInformation("Refreshing sharded schema for database: {Database}", databaseName);

            // Clear cache
            var cacheKey = $"sharded_schema:{databaseName ?? "default"}";
            await _cacheService.RemoveAsync(cacheKey);

            // Refresh schema on all shards in parallel
            var refreshTasks = _config.Shards.Select(async shard =>
            {
                try
                {
                    var shardService = await GetShardServiceAsync(shard.ShardId);
                    await shardService.RefreshSchemaAsync(shard.DatabaseName);
                    _logger.LogDebug("Refreshed schema on shard {ShardId}", shard.ShardId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error refreshing schema on shard {ShardId}", shard.ShardId);
                }
            });

            await Task.WhenAll(refreshTasks);

            // Clear related caches
            await _cacheService.RemovePatternAsync("sharded_table:*");
            await _cacheService.RemovePatternAsync("sharded_table_names:*");

            _logger.LogInformation("Completed sharded schema refresh for database: {Database}", databaseName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing sharded schema for database: {Database}", databaseName);
            throw;
        }
    }

    private async Task<ISchemaService> GetShardServiceAsync(string shardId)
    {
        return _shardServices.GetOrAdd(shardId, id =>
        {
            // Create a new schema service instance for this shard
            // In a real implementation, this would be configured with shard-specific connection strings
            var shardService = _serviceProvider.GetRequiredService<ISchemaService>();
            _logger.LogDebug("Created schema service for shard {ShardId}", id);
            return shardService;
        });
    }

    private void MergeTableInfo(TableMetadata existingTable, TableMetadata newTable, string shardId)
    {
        try
        {
            // Merge row counts
            existingTable.RowCount += newTable.RowCount;

            // TableMetadata doesn't have Metadata property, so we'll skip metadata merging for now
            // In a real implementation, you might want to extend TableMetadata or use a different approach

            // Merge column information if needed
            var existingColumns = existingTable.Columns?.ToList() ?? new List<ColumnMetadata>();
            var newColumns = newTable.Columns?.ToList() ?? new List<ColumnMetadata>();

            foreach (var newColumn in newColumns)
            {
                var existingColumn = existingColumns.FirstOrDefault(c =>
                    c.Name.Equals(newColumn.Name, StringComparison.OrdinalIgnoreCase));

                if (existingColumn == null)
                {
                    existingColumns.Add(newColumn);
                }
                // Could merge column statistics here if needed
            }

            existingTable.Columns = existingColumns;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error merging table info for table {Table} from shard {ShardId}",
                existingTable.Name, shardId);
        }
    }

    private async Task<long> GetTotalRowCountAsync(string tableName, string? schemaName, List<ShardInfo> shards)
    {
        try
        {
            var rowCountTasks = shards.Select(async shard =>
            {
                try
                {
                    var shardService = await GetShardServiceAsync(shard.ShardId);
                    var tableInfo = await shardService.GetTableInfoAsync(tableName, schemaName);
                    return tableInfo?.RowCount ?? 0;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error getting row count from shard {ShardId} for table {Table}",
                        shard.ShardId, tableName);
                    return 0L;
                }
            });

            var rowCounts = await Task.WhenAll(rowCountTasks);
            return rowCounts.Sum();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total row count for table {Table}", tableName);
            return 0;
        }
    }

    // Missing ISchemaService interface methods
    public async Task<SchemaMetadata> GetSchemaMetadataAsync(string? dataSource = null)
    {
        return await GetSchemaAsync(dataSource);
    }

    public async Task<SchemaMetadata> RefreshSchemaMetadataAsync(string? dataSource = null)
    {
        await RefreshSchemaAsync(dataSource);
        return await GetSchemaAsync(dataSource);
    }

    public async Task<string> GetSchemaSummaryAsync(string? dataSource = null)
    {
        try
        {
            var schema = await GetSchemaAsync(dataSource);
            var summary = $"Database: {schema.DatabaseName}\n";
            summary += $"Tables: {schema.Tables.Count}\n";
            summary += $"Last Updated: {schema.LastUpdated:yyyy-MM-dd HH:mm:ss}\n\n";

            summary += "Tables:\n";
            foreach (var table in schema.Tables.Take(10))
            {
                summary += $"- {table.Schema}.{table.Name} ({table.Columns?.Count() ?? 0} columns, {table.RowCount} rows)\n";
            }

            if (schema.Tables.Count > 10)
            {
                summary += $"... and {schema.Tables.Count - 10} more tables\n";
            }

            return summary;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating schema summary for data source: {DataSource}", dataSource);
            return $"Error generating schema summary: {ex.Message}";
        }
    }

    public async Task<TableMetadata?> GetTableMetadataAsync(string tableName, string? schema = null)
    {
        return await GetTableInfoAsync(tableName, schema);
    }

    public async Task<List<SchemaSuggestion>> GetSchemaSuggestionsAsync(string userId)
    {
        try
        {
            var suggestions = new List<SchemaSuggestion>();
            var schema = await GetSchemaAsync();

            // Generate suggestions based on table names and common patterns
            foreach (var table in schema.Tables.Take(5))
            {
                suggestions.Add(new SchemaSuggestion
                {
                    Type = "table",
                    Name = table.Name,
                    Description = table.Description ?? $"Query data from {table.Name}",
                    Confidence = 0.8,
                    SampleQueries = GenerateSampleQueries(table)
                });
            }

            return suggestions;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema suggestions for user: {UserId}", userId);
            return new List<SchemaSuggestion>();
        }
    }

    public async Task<bool> ValidateTableAccessAsync(string tableName, string userId)
    {
        try
        {
            var schema = await GetSchemaAsync();
            return schema.Tables.Any(t => t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating table access for user {UserId} and table {Table}", userId, tableName);
            return false;
        }
    }

    public async Task<List<string>> GetAccessibleTablesAsync(string userId)
    {
        try
        {
            var schema = await GetSchemaAsync();
            // Basic implementation - return all tables
            // In production, filter based on user permissions and shard access
            return schema.Tables.Select(t => $"{t.Schema}.{t.Name}").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting accessible tables for user: {UserId}", userId);
            return new List<string>();
        }
    }

    public async Task UpdateTableStatisticsAsync(string tableName)
    {
        try
        {
            _logger.LogInformation("Updating table statistics for sharded table: {TableName}", tableName);

            // Update statistics on all shards that contain this table
            var targetShards = await _shardRouter.GetShardsForTableAsync(tableName);

            var updateTasks = targetShards.Select(async shard =>
            {
                try
                {
                    var shardService = await GetShardServiceAsync(shard.ShardId);
                    await shardService.UpdateTableStatisticsAsync(tableName);
                    _logger.LogDebug("Updated statistics for table {TableName} on shard {ShardId}", tableName, shard.ShardId);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating statistics for table {TableName} on shard {ShardId}", tableName, shard.ShardId);
                }
            });

            await Task.WhenAll(updateTasks);

            // Clear cache to force refresh
            var cacheKey = $"sharded_table:*:{tableName}";
            await _cacheService.RemovePatternAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating table statistics for sharded table: {TableName}", tableName);
        }
    }

    public async Task<DataQualityScore> AssessDataQualityAsync(string tableName)
    {
        try
        {
            _logger.LogInformation("Assessing data quality for sharded table: {TableName}", tableName);

            // Get data quality scores from all shards that contain this table
            var targetShards = await _shardRouter.GetShardsForTableAsync(tableName);

            if (!targetShards.Any())
            {
                return new DataQualityScore
                {
                    OverallScore = 0,
                    CompletenessScore = 0,
                    UniquenessScore = 0,
                    ValidityScore = 0
                };
            }

            var qualityTasks = targetShards.Select(async shard =>
            {
                try
                {
                    var shardService = await GetShardServiceAsync(shard.ShardId);
                    return await shardService.AssessDataQualityAsync(tableName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error assessing data quality for table {TableName} on shard {ShardId}", tableName, shard.ShardId);
                    return new DataQualityScore { OverallScore = 0 };
                }
            });

            var qualityScores = await Task.WhenAll(qualityTasks);

            // Aggregate quality scores across shards
            var validScores = qualityScores.Where(s => s.OverallScore > 0).ToList();

            if (!validScores.Any())
            {
                return new DataQualityScore
                {
                    OverallScore = 0,
                    CompletenessScore = 0,
                    UniquenessScore = 0,
                    ValidityScore = 0
                };
            }

            return new DataQualityScore
            {
                OverallScore = validScores.Average(s => s.OverallScore),
                CompletenessScore = validScores.Average(s => s.CompletenessScore),
                UniquenessScore = validScores.Average(s => s.UniquenessScore),
                ValidityScore = validScores.Average(s => s.ValidityScore)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing data quality for sharded table: {TableName}", tableName);
            return new DataQualityScore { OverallScore = 0 };
        }
    }

    private List<string> GenerateSampleQueries(TableMetadata table)
    {
        var queries = new List<string>();

        if (table.Columns?.Any() == true)
        {
            queries.Add($"SELECT TOP 10 * FROM {table.Schema}.{table.Name}");
            queries.Add($"SELECT COUNT(*) FROM {table.Schema}.{table.Name}");

            var firstColumn = table.Columns.First();
            queries.Add($"SELECT {firstColumn.Name} FROM {table.Schema}.{table.Name} GROUP BY {firstColumn.Name}");
        }

        return queries;
    }
}

/// <summary>
/// Shard router for determining which shards contain specific tables
/// </summary>
public class ShardRouter
{
    private readonly ShardingConfiguration _config;
    private readonly ILogger _logger;
    private readonly Dictionary<string, List<ShardInfo>> _tableShardMap;

    public ShardRouter(ShardingConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
        _tableShardMap = new Dictionary<string, List<ShardInfo>>();
        BuildTableShardMap();
    }

    public async Task<List<ShardInfo>> GetShardsForTableAsync(string tableName, string? schemaName = null)
    {
        var tableKey = $"{schemaName ?? "dbo"}.{tableName}".ToLower();

        if (_tableShardMap.TryGetValue(tableKey, out var shards))
        {
            return shards;
        }

        // If not in map, check all shards (fallback)
        _logger.LogDebug("Table {Table} not found in shard map, checking all shards", tableKey);
        return _config.Shards.ToList();
    }

    private void BuildTableShardMap()
    {
        // Build mapping based on configuration
        foreach (var shard in _config.Shards)
        {
            foreach (var tablePattern in shard.TablePatterns)
            {
                var shardList = _tableShardMap.GetValueOrDefault(tablePattern.ToLower(), new List<ShardInfo>());
                shardList.Add(shard);
                _tableShardMap[tablePattern.ToLower()] = shardList;
            }
        }

        _logger.LogInformation("Built shard map with {TableCount} table patterns across {ShardCount} shards",
            _tableShardMap.Count, _config.Shards.Count);
    }
}

/// <summary>
/// Sharding configuration
/// </summary>
public class ShardingConfiguration
{
    public List<ShardInfo> Shards { get; set; } = new();
    public int SchemaCacheMinutes { get; set; } = 30;
    public int TableCacheMinutes { get; set; } = 15;
    public string ShardingStrategy { get; set; } = "TableBased"; // TableBased, HashBased, RangeBased
    public int MaxConcurrentShardOperations { get; set; } = 10;
}

/// <summary>
/// Shard information
/// </summary>
public class ShardInfo
{
    public string ShardId { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public List<string> TablePatterns { get; set; } = new(); // Tables that exist in this shard
    public bool IsActive { get; set; } = true;
    public int Priority { get; set; } = 1; // For load balancing
    public Dictionary<string, object> Metadata { get; set; } = new();
}
