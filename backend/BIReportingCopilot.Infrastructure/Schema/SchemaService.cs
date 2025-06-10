using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using BIReportingCopilot.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BIReportingCopilot.Infrastructure.Schema;

/// <summary>
/// Enhanced schema service using bounded contexts for better performance and maintainability
/// Uses SchemaDbContext for schema metadata persistence and caching
/// </summary>
public class SchemaService : ISchemaService
{
    private readonly ILogger<SchemaService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IDbContextFactory _contextFactory;
    private readonly IConnectionStringProvider _connectionStringProvider;
    private readonly IDistributedCache? _distributedCache;
    private readonly TimeSpan _distributedCacheExpiration = TimeSpan.FromHours(24);

    public SchemaService(
        ILogger<SchemaService> logger,
        IConfiguration configuration,
        IDbContextFactory contextFactory,
        IConnectionStringProvider connectionStringProvider,
        IDistributedCache? distributedCache = null)
    {
        _logger = logger;
        _configuration = configuration;
        _contextFactory = contextFactory;
        _connectionStringProvider = connectionStringProvider;
        _distributedCache = distributedCache;
    }

    public async Task<SchemaMetadata> GetSchemaMetadataAsync(string? dataSource = null)
    {
        try
        {
            // Try distributed cache first (fastest)
            var distributedCachedSchema = await GetDistributedCachedSchemaAsync(dataSource);
            if (distributedCachedSchema != null)
            {
                _logger.LogDebug("Returning schema from distributed cache");
                return distributedCachedSchema;
            }

            // Try database cache second (medium speed)
            var cachedSchema = await GetCachedSchemaAsync(dataSource);
            if (cachedSchema != null)
            {
                // Store in distributed cache for next time
                await SetDistributedCachedSchemaAsync(cachedSchema, dataSource);
                return cachedSchema;
            }

            // If not cached anywhere, refresh and return (slowest)
            return await RefreshSchemaMetadataAsync(dataSource);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting schema metadata for data source: {DataSource}", dataSource);
            return new SchemaMetadata { DatabaseName = dataSource ?? "Unknown" };
        }
    }

    public async Task<SchemaMetadata> RefreshSchemaMetadataAsync(string? dataSource = null)
    {
        try
        {
            // Clear both database and distributed cache first
            await ClearCachedSchemaAsync(dataSource);
            await ClearDistributedCachedSchemaAsync(dataSource);

            var connectionString = await GetConnectionStringAsync(dataSource);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No connection string available");
            }

            var schema = await ExtractSchemaFromDatabaseAsync(connectionString);

            // Cache the schema in both database and distributed cache
            await CacheSchemaAsync(schema, dataSource);
            await SetDistributedCachedSchemaAsync(schema, dataSource);

            _logger.LogInformation("Schema metadata refreshed for data source: {DataSource}", dataSource ?? "default");
            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schema metadata for data source: {DataSource}", dataSource);
            throw;
        }
    }

    public async Task<string> GetSchemaSummaryAsync(string? dataSource = null)
    {
        try
        {
            var schema = await GetSchemaMetadataAsync(dataSource);
            var summary = new List<string>();

            foreach (var table in schema.Tables.Take(10)) // Limit to first 10 tables
            {
                var columnSummary = string.Join(", ", table.Columns.Take(5).Select(c =>
                    $"{c.Name} {c.DataType}{(c.IsPrimaryKey ? " PK" : "")}{(c.IsForeignKey ? " FK" : "")}"));

                if (table.Columns.Count > 5)
                {
                    columnSummary += $", ... and {table.Columns.Count - 5} more columns";
                }

                summary.Add($"{table.Schema}.{table.Name}({columnSummary})");
            }

            if (schema.Tables.Count > 10)
            {
                summary.Add($"... and {schema.Tables.Count - 10} more tables");
            }

            return string.Join("\n", summary);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating schema summary for data source: {DataSource}", dataSource);
            return "Unable to generate schema summary";
        }
    }

    public async Task<TableMetadata?> GetTableMetadataAsync(string tableName, string? schema = null)
    {
        try
        {
            var schemaMetadata = await GetSchemaMetadataAsync();
            return schemaMetadata.Tables.FirstOrDefault(t =>
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) &&
                (schema == null || t.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table metadata for {Schema}.{Table}", schema, tableName);
            return null;
        }
    }

    public async Task<List<SchemaSuggestion>> GetSchemaSuggestionsAsync(string userId)
    {
        try
        {
            var suggestions = new List<SchemaSuggestion>();
            var schema = await GetSchemaMetadataAsync();

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
            // Basic implementation - in production, implement proper access control
            var schema = await GetSchemaMetadataAsync();
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
            var schema = await GetSchemaMetadataAsync();
            // Basic implementation - return all tables
            // In production, filter based on user permissions
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
            var connectionString = await GetConnectionStringAsync();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No connection string available");
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Update statistics for the table
            var command = new SqlCommand($"UPDATE STATISTICS [{tableName}]", connection);
            await command.ExecuteNonQueryAsync();

            _logger.LogInformation("Updated statistics for table: {TableName}", tableName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating statistics for table: {TableName}", tableName);
        }
    }

    public async Task<DataQualityScore> AssessDataQualityAsync(string tableName)
    {
        try
        {
            var connectionString = await GetConnectionStringAsync();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No connection string available");
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            // Basic data quality assessment
            var totalRowsCommand = new SqlCommand($"SELECT COUNT(*) FROM [{tableName}]", connection);
            var totalRows = (int)(await totalRowsCommand.ExecuteScalarAsync() ?? 0);

            if (totalRows == 0)
            {
                return new DataQualityScore
                {
                    OverallScore = 0,
                    CompletenessScore = 0,
                    UniquenessScore = 0,
                    ValidityScore = 0
                };
            }

            // Calculate completeness (non-null values)
            var tableMetadata = await GetTableMetadataAsync(tableName);
            var completenessScore = await CalculateCompletenessScore(connection, tableName, tableMetadata, totalRows);

            // Calculate uniqueness (for columns that should be unique)
            var uniquenessScore = await CalculateUniquenessScore(connection, tableName, tableMetadata, totalRows);

            // Basic validity score (simplified)
            var validityScore = 0.85; // Placeholder

            var overallScore = (completenessScore + uniquenessScore + validityScore) / 3;

            return new DataQualityScore
            {
                OverallScore = overallScore,
                CompletenessScore = completenessScore,
                UniquenessScore = uniquenessScore,
                ValidityScore = validityScore
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assessing data quality for table: {TableName}", tableName);
            return new DataQualityScore
            {
                OverallScore = 0.5,
                CompletenessScore = 0.5,
                UniquenessScore = 0.5,
                ValidityScore = 0.5
            };
        }
    }

    private async Task<string?> GetConnectionStringAsync(string? dataSource = null)
    {
        if (string.IsNullOrEmpty(dataSource))
        {
            // Use BIDatabase as the primary connection for schema operations
            return await _connectionStringProvider.GetConnectionStringAsync("BIDatabase");
        }

        try
        {
            return await _connectionStringProvider.GetConnectionStringAsync(dataSource);
        }
        catch
        {
            // Fallback to configuration if connection string provider fails
            return _configuration.GetConnectionString(dataSource) ??
                   _configuration.GetSection("DataSources")[dataSource];
        }
    }

    private async Task<SchemaMetadata?> GetCachedSchemaAsync(string? dataSource)
    {
        try
        {
            return await _contextFactory.ExecuteWithContextAsync(ContextType.Schema, async context =>
            {
                var schemaContext = (SchemaDbContext)context;
                var databaseName = dataSource ?? "default";

                var cachedEntities = await schemaContext.SchemaMetadata
                    .Where(s => s.DatabaseName == databaseName && s.IsActive)
                    .ToListAsync();

                if (!cachedEntities.Any())
                {
                    return null;
                }

                // Check if cache is still valid (less than 1 hour old)
                var latestUpdate = cachedEntities.Max(s => s.LastUpdated);
                if (DateTime.UtcNow - latestUpdate > TimeSpan.FromHours(1))
                {
                    return null;
                }

                return BuildSchemaFromEntities(cachedEntities, databaseName);
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving cached schema");
            return null;
        }
    }

    private async Task CacheSchemaAsync(SchemaMetadata schema, string? dataSource)
    {
        try
        {
            await _contextFactory.ExecuteWithContextAsync(ContextType.Schema, async context =>
            {
                var schemaContext = (SchemaDbContext)context;
                var databaseName = dataSource ?? "default";

                // Remove old cached entries
                var oldEntries = await schemaContext.SchemaMetadata
                    .Where(s => s.DatabaseName == databaseName)
                    .ToListAsync();

                schemaContext.SchemaMetadata.RemoveRange(oldEntries);

                // Add new entries
                foreach (var table in schema.Tables)
                {
                    foreach (var column in table.Columns)
                    {
                        var entity = new SchemaMetadataEntity
                        {
                            DatabaseName = databaseName,
                            SchemaName = table.Schema,
                            TableName = table.Name,
                            ColumnName = column.Name,
                            DataType = column.DataType,
                            IsNullable = column.IsNullable,
                            IsPrimaryKey = column.IsPrimaryKey,
                            IsForeignKey = column.IsForeignKey,
                            BusinessDescription = column.Description,
                            SemanticTags = column.SemanticTags.Any() ? JsonSerializer.Serialize(column.SemanticTags) : null,
                            SampleValues = column.SampleValues.Any() ? JsonSerializer.Serialize(column.SampleValues) : null,
                            LastUpdated = DateTime.UtcNow,
                            IsActive = true
                        };

                        schemaContext.SchemaMetadata.Add(entity);
                    }
                }

                await schemaContext.SaveChangesAsync();
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching schema metadata");
        }
    }

    private async Task ClearCachedSchemaAsync(string? dataSource)
    {
        try
        {
            await _contextFactory.ExecuteWithContextAsync(ContextType.Schema, async context =>
            {
                var schemaContext = (SchemaDbContext)context;
                var databaseName = dataSource ?? "default";
                var oldEntries = await schemaContext.SchemaMetadata
                    .Where(s => s.DatabaseName == databaseName)
                    .ToListAsync();

                schemaContext.SchemaMetadata.RemoveRange(oldEntries);
                await schemaContext.SaveChangesAsync();
            });

            _logger.LogInformation("Cleared cached schema for data source: {DataSource}", dataSource ?? "default");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing cached schema");
        }
    }

    private async Task<SchemaMetadata> ExtractSchemaFromDatabaseAsync(string connectionString)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();

        var schema = new SchemaMetadata
        {
            DatabaseName = connection.Database,
            LastUpdated = DateTime.UtcNow
        };

        // Get tables and columns
        var tablesQuery = @"
            SELECT
                t.TABLE_SCHEMA,
                t.TABLE_NAME,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_PRIMARY_KEY,
                CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_FOREIGN_KEY
            FROM INFORMATION_SCHEMA.TABLES t
            INNER JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            LEFT JOIN (
                SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ) pk ON c.TABLE_SCHEMA = pk.TABLE_SCHEMA AND c.TABLE_NAME = pk.TABLE_NAME AND c.COLUMN_NAME = pk.COLUMN_NAME
            LEFT JOIN (
                SELECT ku.TABLE_SCHEMA, ku.TABLE_NAME, ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'FOREIGN KEY'
            ) fk ON c.TABLE_SCHEMA = fk.TABLE_SCHEMA AND c.TABLE_NAME = fk.TABLE_NAME AND c.COLUMN_NAME = fk.COLUMN_NAME
            WHERE t.TABLE_TYPE = 'BASE TABLE'
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";

        using var command = new SqlCommand(tablesQuery, connection);
        using var reader = await command.ExecuteReaderAsync();

        var tableDict = new Dictionary<string, TableMetadata>();

        while (await reader.ReadAsync())
        {
            var tableKey = $"{reader["TABLE_SCHEMA"]}.{reader["TABLE_NAME"]}";

            if (!tableDict.ContainsKey(tableKey))
            {
                tableDict[tableKey] = new TableMetadata
                {
                    Name = reader["TABLE_NAME"].ToString()!,
                    Schema = reader["TABLE_SCHEMA"].ToString()!,
                    LastUpdated = DateTime.UtcNow
                };
            }

            var table = tableDict[tableKey];
            table.Columns.Add(new ColumnMetadata
            {
                Name = reader["COLUMN_NAME"].ToString()!,
                DataType = reader["DATA_TYPE"].ToString()!,
                IsNullable = reader["IS_NULLABLE"].ToString() == "YES",
                IsPrimaryKey = Convert.ToBoolean(reader["IS_PRIMARY_KEY"]),
                IsForeignKey = Convert.ToBoolean(reader["IS_FOREIGN_KEY"])
            });
        }

        schema.Tables = tableDict.Values.ToList();
        return schema;
    }

    private SchemaMetadata BuildSchemaFromEntities(List<SchemaMetadataEntity> entities, string databaseName)
    {
        var schema = new SchemaMetadata
        {
            DatabaseName = databaseName,
            LastUpdated = entities.Max(e => e.LastUpdated)
        };

        var tableGroups = entities.GroupBy(e => new { e.SchemaName, e.TableName });

        foreach (var tableGroup in tableGroups)
        {
            var table = new TableMetadata
            {
                Name = tableGroup.Key.TableName,
                Schema = tableGroup.Key.SchemaName,
                LastUpdated = tableGroup.Max(e => e.LastUpdated)
            };

            foreach (var columnEntity in tableGroup)
            {
                var column = new ColumnMetadata
                {
                    Name = columnEntity.ColumnName,
                    DataType = columnEntity.DataType,
                    IsNullable = columnEntity.IsNullable,
                    IsPrimaryKey = columnEntity.IsPrimaryKey,
                    IsForeignKey = columnEntity.IsForeignKey,
                    Description = columnEntity.BusinessDescription
                };

                if (!string.IsNullOrEmpty(columnEntity.SemanticTags))
                {
                    try
                    {
                        column.SemanticTags = JsonSerializer.Deserialize<string[]>(columnEntity.SemanticTags) ?? Array.Empty<string>();
                    }
                    catch { /* Ignore deserialization errors */ }
                }

                if (!string.IsNullOrEmpty(columnEntity.SampleValues))
                {
                    try
                    {
                        column.SampleValues = JsonSerializer.Deserialize<string[]>(columnEntity.SampleValues) ?? Array.Empty<string>();
                    }
                    catch { /* Ignore deserialization errors */ }
                }

                table.Columns.Add(column);
            }

            schema.Tables.Add(table);
        }

        return schema;
    }

    private List<string> GenerateSampleQueries(TableMetadata table)
    {
        var queries = new List<string>
        {
            $"Show me all data from {table.Name}",
            $"Count records in {table.Name}",
            $"Show me recent {table.Name} records"
        };

        // Add column-specific queries
        var dateColumns = table.Columns.Where(c => c.DataType.Contains("date") || c.DataType.Contains("time")).Take(1);
        foreach (var dateCol in dateColumns)
        {
            queries.Add($"Show me {table.Name} records from last month");
        }

        var numericColumns = table.Columns.Where(c => c.DataType.Contains("int") || c.DataType.Contains("decimal") || c.DataType.Contains("float")).Take(1);
        foreach (var numCol in numericColumns)
        {
            queries.Add($"What's the average {numCol.Name} in {table.Name}?");
        }

        return queries.Take(5).ToList();
    }

    private async Task<double> CalculateCompletenessScore(SqlConnection connection, string tableName, TableMetadata? tableMetadata, int totalRows)
    {
        if (tableMetadata == null || totalRows == 0) return 0;

        var nullableColumns = tableMetadata.Columns.Where(c => c.IsNullable).ToList();
        if (!nullableColumns.Any()) return 1.0;

        var completenessScores = new List<double>();

        foreach (var column in nullableColumns.Take(5)) // Check first 5 nullable columns
        {
            try
            {
                var nullCountCommand = new SqlCommand($"SELECT COUNT(*) FROM [{tableName}] WHERE [{column.Name}] IS NULL", connection);
                var nullCount = (int)(await nullCountCommand.ExecuteScalarAsync() ?? 0);
                var completeness = 1.0 - ((double)nullCount / totalRows);
                completenessScores.Add(completeness);
            }
            catch
            {
                completenessScores.Add(0.5); // Default if we can't check
            }
        }

        return completenessScores.Any() ? completenessScores.Average() : 0.5;
    }

    private async Task<double> CalculateUniquenessScore(SqlConnection connection, string tableName, TableMetadata? tableMetadata, int totalRows)
    {
        if (tableMetadata == null || totalRows == 0) return 0;

        var primaryKeyColumns = tableMetadata.Columns.Where(c => c.IsPrimaryKey).ToList();
        if (!primaryKeyColumns.Any()) return 0.8; // Default if no PK

        try
        {
            var pkColumn = primaryKeyColumns.First();
            var distinctCountCommand = new SqlCommand($"SELECT COUNT(DISTINCT [{pkColumn.Name}]) FROM [{tableName}]", connection);
            var distinctCount = (int)(await distinctCountCommand.ExecuteScalarAsync() ?? 0);
            return (double)distinctCount / totalRows;
        }
        catch
        {
            return 0.8; // Default if we can't check
        }
    }

    // Additional methods for compatibility
    public async Task<SchemaMetadata> GetSchemaAsync(string? dataSource = null)
    {
        return await GetSchemaMetadataAsync(dataSource);
    }

    public async Task<TableMetadata?> GetTableInfoAsync(string tableName, string? schema = null)
    {
        return await GetTableMetadataAsync(tableName, schema);
    }

    public async Task<List<string>> GetTableNamesAsync(string? schema = null)
    {
        try
        {
            var schemaMetadata = await GetSchemaMetadataAsync();
            var tables = schemaMetadata.Tables.AsQueryable();

            if (!string.IsNullOrEmpty(schema))
            {
                tables = tables.Where(t => t.Schema.Equals(schema, StringComparison.OrdinalIgnoreCase));
            }

            return tables.Select(t => t.Name).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting table names for schema: {Schema}", schema);
            return new List<string>();
        }
    }

    public async Task RefreshSchemaAsync(string? dataSource = null)
    {
        await RefreshSchemaMetadataAsync(dataSource);
    }

    #region Distributed Cache Methods

    private async Task<SchemaMetadata?> GetDistributedCachedSchemaAsync(string? dataSource)
    {
        if (_distributedCache == null)
            return null;

        try
        {
            var cacheKey = $"schema:metadata:{dataSource ?? "default"}";
            var cachedData = await _distributedCache.GetStringAsync(cacheKey);

            if (!string.IsNullOrEmpty(cachedData))
            {
                return JsonSerializer.Deserialize<SchemaMetadata>(cachedData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error retrieving schema from distributed cache");
        }

        return null;
    }

    private async Task SetDistributedCachedSchemaAsync(SchemaMetadata schema, string? dataSource)
    {
        if (_distributedCache == null)
            return;

        try
        {
            var cacheKey = $"schema:metadata:{dataSource ?? "default"}";
            var serialized = JsonSerializer.Serialize(schema);

            await _distributedCache.SetStringAsync(cacheKey, serialized, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _distributedCacheExpiration
            });
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error caching schema in distributed cache");
        }
    }

    private async Task ClearDistributedCachedSchemaAsync(string? dataSource)
    {
        if (_distributedCache == null)
            return;

        try
        {
            var cacheKey = $"schema:metadata:{dataSource ?? "default"}";
            await _distributedCache.RemoveAsync(cacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error clearing schema from distributed cache");
        }
    }

    #endregion
}
