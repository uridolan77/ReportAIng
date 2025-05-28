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
                Tables = new List<TableInfo>(),
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
                        var shardedTable = new TableInfo
                        {
                            Name = table.Name,
                            Schema = table.Schema,
                            Columns = table.Columns.ToList(),
                            RowCount = table.RowCount,
                            Description = table.Description
                        };

                        // Add shard metadata
                        shardedTable.Metadata = table.Metadata ?? new Dictionary<string, object>();
                        shardedTable.Metadata["ShardId"] = result.ShardId;
                        shardedTable.Metadata["IsSharded"] = true;

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

    public async Task<TableInfo?> GetTableInfoAsync(string tableName, string? schemaName = null)
    {
        try
        {
            var cacheKey = $"sharded_table:{schemaName}:{tableName}";
            var cachedTable = await _cacheService.GetAsync<TableInfo>(cacheKey);
            
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
                // Add sharding metadata
                tableInfo.Metadata = tableInfo.Metadata ?? new Dictionary<string, object>();
                tableInfo.Metadata["ShardId"] = primaryShard.ShardId;
                tableInfo.Metadata["IsSharded"] = targetShards.Count > 1;
                tableInfo.Metadata["ShardCount"] = targetShards.Count;

                if (targetShards.Count > 1)
                {
                    tableInfo.Metadata["AllShards"] = targetShards.Select(s => s.ShardId).ToArray();
                    
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
            await _cacheService.RemoveByPatternAsync("sharded_table:*");
            await _cacheService.RemoveByPatternAsync("sharded_table_names:*");

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

    private void MergeTableInfo(TableInfo existingTable, TableInfo newTable, string shardId)
    {
        try
        {
            // Merge row counts
            existingTable.RowCount += newTable.RowCount;

            // Merge metadata
            existingTable.Metadata = existingTable.Metadata ?? new Dictionary<string, object>();
            
            if (!existingTable.Metadata.ContainsKey("AllShards"))
            {
                existingTable.Metadata["AllShards"] = new List<string> { (string)existingTable.Metadata["ShardId"] };
                existingTable.Metadata["IsSharded"] = true;
            }

            var allShards = (List<string>)existingTable.Metadata["AllShards"];
            if (!allShards.Contains(shardId))
            {
                allShards.Add(shardId);
            }

            existingTable.Metadata["ShardCount"] = allShards.Count;

            // Merge column information if needed
            foreach (var newColumn in newTable.Columns)
            {
                var existingColumn = existingTable.Columns.FirstOrDefault(c => 
                    c.Name.Equals(newColumn.Name, StringComparison.OrdinalIgnoreCase));

                if (existingColumn == null)
                {
                    existingTable.Columns.Add(newColumn);
                }
                // Could merge column statistics here if needed
            }
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
