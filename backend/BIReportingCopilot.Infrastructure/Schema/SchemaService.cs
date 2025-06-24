using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Business;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using BIReportingCopilot.Infrastructure.Configuration;
using ContextType = BIReportingCopilot.Infrastructure.Data.Contexts.ContextType;
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
            var schemaMetadata = await GetSchemaMetadataAsync((string?)null);
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

    public async Task<List<Core.Models.SchemaSuggestion>> GetSchemaSuggestionsAsync(string userId)
    {
        try
        {
            var suggestions = new List<Core.Models.SchemaSuggestion>();
            var schema = await GetSchemaMetadataAsync((string?)null);

            // Generate suggestions based on table names and common patterns
            foreach (var table in schema.Tables.Take(5))
            {
                suggestions.Add(new Core.Models.SchemaSuggestion
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
            return new List<Core.Models.SchemaSuggestion>();
        }
    }

    public async Task<bool> ValidateTableAccessAsync(string tableName, string userId)
    {
        try
        {
            // Basic implementation - in production, implement proper access control
            var schema = await GetSchemaMetadataAsync((string?)null);
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
            var schema = await GetSchemaMetadataAsync((string?)null);
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
            var tableMetadata = await GetTableMetadataAsync(tableName, (string?)null);
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
            // For now, use DefaultConnection instead of BIDatabase to avoid Azure Key Vault issues
            // TODO: Implement proper Azure Key Vault resolution for BIDatabase connection
            var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrEmpty(defaultConnection))
            {
                _logger.LogInformation("Using DefaultConnection for schema operations (BIDatabase requires Azure Key Vault setup)");
                return defaultConnection;
            }

            // Fallback to BIDatabase if DefaultConnection is not available
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
            var schemaMetadata = await GetSchemaMetadataAsync((string?)null);
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

    #region Missing Interface Method Implementations

    /// <summary>
    /// Get schema async (ISchemaService interface)
    /// </summary>
    public async Task<SchemaMetadata> GetSchemaAsync(CancellationToken cancellationToken = default)
    {
        return await GetSchemaMetadataAsync((string?)null);
    }

    /// <summary>
    /// Refresh schema async (ISchemaService interface)
    /// </summary>
    public async Task<SchemaMetadata> RefreshSchemaAsync(CancellationToken cancellationToken = default)
    {
        await RefreshSchemaMetadataAsync((string?)null);
        return await GetSchemaMetadataAsync(cancellationToken);
    }

    /// <summary>
    /// Get table names async (ISchemaService interface)
    /// </summary>
    public async Task<List<string>> GetTableNamesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schema = await GetSchemaMetadataAsync((string?)null);
            return schema.Tables.Select(t => $"{t.Schema}.{t.Name}").ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting table names");
            return new List<string>();
        }
    }

    /// <summary>
    /// Get column names async (ISchemaService interface)
    /// </summary>
    public async Task<List<string>> GetColumnNamesAsync(string tableName, CancellationToken cancellationToken = default)
    {
        try
        {
            var schema = await GetSchemaMetadataAsync((string?)null);
            var table = schema.Tables.FirstOrDefault(t =>
                t.Name.Equals(tableName, StringComparison.OrdinalIgnoreCase) ||
                $"{t.Schema}.{t.Name}".Equals(tableName, StringComparison.OrdinalIgnoreCase));

            return table?.Columns.Select(c => c.Name).ToList() ?? new List<string>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting column names for table: {TableName}", tableName);
            return new List<string>();
        }
    }

    /// <summary>
    /// Get schema metadata async (ISchemaService interface)
    /// </summary>
    public async Task<SchemaMetadata> GetSchemaMetadataAsync(CancellationToken cancellationToken = default)
    {
        return await GetSchemaMetadataAsync((string?)null);
    }

    /// <summary>
    /// Get table metadata async (ISchemaService interface)
    /// </summary>
    public async Task<TableMetadata?> GetTableMetadataAsync(string tableName, CancellationToken cancellationToken = default)
    {
        return await GetTableMetadataAsync(tableName, (string?)null);
    }

    /// <summary>
    /// Refresh schema metadata async (ISchemaService interface)
    /// </summary>
    public async Task<SchemaMetadata> RefreshSchemaMetadataAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔄 Refreshing schema metadata");

            // Clear cache
            // Clear distributed cache if available
            await ClearDistributedCachedSchemaAsync(null);

            // Get fresh schema metadata
            var schema = await GetSchemaMetadataAsync(cancellationToken);

            _logger.LogInformation("✅ Schema metadata refreshed successfully");
            return schema;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error refreshing schema metadata");
            throw;
        }
    }

    /// <summary>
    /// Get schema metadata async (ISchemaService interface - with connection name)
    /// </summary>
    public async Task<SchemaMetadata> GetSchemaMetadataAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        return await GetSchemaMetadataAsync(connectionName);
    }

    /// <summary>
    /// Get table metadata async (ISchemaService interface - with connection name)
    /// </summary>
    public async Task<TableMetadata?> GetTableMetadataAsync(string tableName, string connectionName, CancellationToken cancellationToken = default)
    {
        return await GetTableMetadataAsync(tableName, null);
    }

    /// <summary>
    /// Refresh schema metadata async (ISchemaService interface - with connection name)
    /// </summary>
    public async Task RefreshSchemaMetadataAsync(string connectionName, CancellationToken cancellationToken = default)
    {
        await RefreshSchemaMetadataAsync(connectionName);
    }

    /// <summary>
    /// Get table relationships async (ISchemaService interface)
    /// </summary>
    public async Task<List<TableRelationship>> GetTableRelationshipsAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schema = await GetSchemaMetadataAsync(cancellationToken);
            // Convert RelationshipMetadata to TableRelationship
            return schema.Relationships?.Select(r => new TableRelationship
            {
                FromTable = r.ParentTable,
                ToTable = r.ChildTable,
                RelationshipType = r.RelationshipType,
                ColumnMappings = new List<ColumnMapping>
                {
                    new ColumnMapping
                    {
                        FromColumn = r.ParentColumn,
                        ToColumn = r.ChildColumn
                    }
                }
            }).ToList() ?? new List<TableRelationship>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting table relationships");
            return new List<TableRelationship>();
        }
    }

    /// <summary>
    /// Validate schema async (ISchemaService interface)
    /// </summary>
    public async Task<SchemaValidationResult> ValidateSchemaAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schema = await GetSchemaMetadataAsync(cancellationToken);
            var errors = new List<string>();
            var warnings = new List<string>();

            // Basic validation
            if (schema.Tables.Count == 0)
            {
                errors.Add("No tables found in schema");
            }

            foreach (var table in schema.Tables)
            {
                if (table.Columns.Count == 0)
                {
                    warnings.Add($"Table {table.Name} has no columns");
                }
            }

            return new SchemaValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors,
                Warnings = warnings,
                ValidatedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating schema");
            return new SchemaValidationResult
            {
                IsValid = false,
                Errors = new List<string> { ex.Message },
                Warnings = new List<string>(),
                ValidatedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get accessible tables async (ISchemaService interface)
    /// </summary>
    public async Task<List<string>> GetAccessibleTablesAsync(string userId, CancellationToken cancellationToken = default)
    {
        return await GetAccessibleTablesAsync(userId);
    }

    /// <summary>
    /// Get schema suggestions async (ISchemaService interface)
    /// </summary>
    public async Task<List<Core.Interfaces.Schema.SchemaSuggestion>> GetSchemaSuggestionsAsync(string userId, CancellationToken cancellationToken = default)
    {
        var modelSuggestions = await GetSchemaSuggestionsAsync(userId);
        // Convert from Core.Models.SchemaSuggestion to Core.Interfaces.Schema.SchemaSuggestion
        return modelSuggestions.Select(s => new Core.Interfaces.Schema.SchemaSuggestion
        {
            Id = Guid.NewGuid().ToString(),
            Name = s.Name,
            Title = s.Name,
            Description = s.Description ?? "No description available",
            Type = Core.Interfaces.Schema.SchemaSuggestionType.PerformanceImprovement, // Default value
            ConfidenceScore = s.Confidence
        }).ToList();
    }

    /// <summary>
    /// Assess data quality async (ISchemaService interface)
    /// </summary>
    public async Task<DataQualityAssessment> AssessDataQualityAsync(string tableName, CancellationToken cancellationToken = default)
    {
        try
        {
            var qualityScore = await AssessDataQualityAsync(tableName);
            return new DataQualityAssessment
            {
                TableName = tableName,
                OverallScore = qualityScore.OverallScore,
                Issues = new List<Core.Interfaces.Schema.DataQualityIssue>(), // Empty for now
                Recommendations = new List<string>(), // Empty for now
                AssessedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error assessing data quality for table: {TableName}", tableName);
            return new DataQualityAssessment
            {
                TableName = tableName,
                OverallScore = 0,
                Issues = new List<Core.Interfaces.Schema.DataQualityIssue>(),
                Recommendations = new List<string>(),
                AssessedAt = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Get schema summary async (ISchemaService interface)
    /// </summary>
    public async Task<SchemaSummary> GetSchemaSummaryAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var schema = await GetSchemaMetadataAsync(cancellationToken);
            
            return new SchemaSummary
            {
                TotalTables = schema.Tables.Count,
                TotalColumns = schema.Tables.Sum(t => t.Columns.Count),
                TotalRelationships = schema.Relationships?.Count ?? 0,
                TableNames = schema.Tables.Select(t => t.Name).ToList(),
                LastUpdated = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error getting schema summary");
            return new SchemaSummary
            {
                TotalTables = 0,
                TotalColumns = 0,
                TotalRelationships = 0,
                TableNames = new List<string>(),
                LastUpdated = DateTime.UtcNow
            };
        }
    }

    #endregion
}
