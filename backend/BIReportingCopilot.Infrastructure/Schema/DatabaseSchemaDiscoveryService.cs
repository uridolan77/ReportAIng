using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Configuration;

namespace BIReportingCopilot.Infrastructure.Schema;

/// <summary>
/// Service for discovering database schema directly from target BI databases
/// </summary>
public class DatabaseSchemaDiscoveryService
{
    private readonly ILogger<DatabaseSchemaDiscoveryService> _logger;
    private readonly IConnectionStringProvider _connectionStringProvider;

    public DatabaseSchemaDiscoveryService(
        ILogger<DatabaseSchemaDiscoveryService> logger,
        IConnectionStringProvider connectionStringProvider)
    {
        _logger = logger;
        _connectionStringProvider = connectionStringProvider;
    }    /// <summary>
    /// Discover complete schema from target BI database
    /// </summary>
    public async Task<SchemaDiscoveryResult> DiscoverSchemaAsync(string connectionStringName = "BIDatabase")
    {
        try
        {
            _logger.LogInformation("🔍 Starting schema discovery for {ConnectionString}", connectionStringName);            var connectionString = await _connectionStringProvider.GetConnectionStringAsync(connectionStringName);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException($"Connection string '{connectionStringName}' not found");
            }

            _logger.LogDebug("Connection string resolved for schema discovery");

            var result = new SchemaDiscoveryResult
            {
                DatabaseName = GetDatabaseNameFromConnectionString(connectionString),
                DiscoveredAt = DateTime.UtcNow,
                Tables = new List<DiscoveredTable>()
            };

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            _logger.LogInformation("✓ Connected to database: {DatabaseName}", result.DatabaseName);

            // Discover all tables
            var tables = await DiscoverTablesAsync(connection);
            result.Tables = tables;

            // Discover relationships
            result.Relationships = await DiscoverRelationshipsAsync(connection);

            _logger.LogInformation("✅ Schema discovery completed. Found {TableCount} tables, {RelationshipCount} relationships", 
                tables.Count, result.Relationships.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error during schema discovery");
            throw;
        }
    }

    /// <summary>
    /// Discover all tables and their columns
    /// </summary>
    private async Task<List<DiscoveredTable>> DiscoverTablesAsync(SqlConnection connection)
    {
        var tables = new List<DiscoveredTable>();

        // Query to get all tables with their columns
        var sql = @"
            SELECT 
                t.TABLE_SCHEMA,
                t.TABLE_NAME,
                t.TABLE_TYPE,
                c.COLUMN_NAME,
                c.DATA_TYPE,
                c.IS_NULLABLE,
                c.COLUMN_DEFAULT,
                c.CHARACTER_MAXIMUM_LENGTH,
                c.NUMERIC_PRECISION,
                c.NUMERIC_SCALE,
                c.ORDINAL_POSITION,
                CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IS_PRIMARY_KEY,
                CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END as IS_FOREIGN_KEY,
                fk.REFERENCED_TABLE_SCHEMA,
                fk.REFERENCED_TABLE_NAME,
                fk.REFERENCED_COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLES t
            LEFT JOIN INFORMATION_SCHEMA.COLUMNS c ON t.TABLE_NAME = c.TABLE_NAME AND t.TABLE_SCHEMA = c.TABLE_SCHEMA
            LEFT JOIN (
                SELECT 
                    ku.TABLE_SCHEMA,
                    ku.TABLE_NAME, 
                    ku.COLUMN_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku 
                    ON tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME 
                    AND tc.TABLE_SCHEMA = ku.TABLE_SCHEMA
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ) pk ON c.TABLE_SCHEMA = pk.TABLE_SCHEMA AND c.TABLE_NAME = pk.TABLE_NAME AND c.COLUMN_NAME = pk.COLUMN_NAME
            LEFT JOIN (
                SELECT 
                    ku.TABLE_SCHEMA,
                    ku.TABLE_NAME,
                    ku.COLUMN_NAME,
                    ccu.TABLE_SCHEMA as REFERENCED_TABLE_SCHEMA,
                    ccu.TABLE_NAME as REFERENCED_TABLE_NAME,
                    ccu.COLUMN_NAME as REFERENCED_COLUMN_NAME
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku 
                    ON rc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                    AND rc.CONSTRAINT_SCHEMA = ku.TABLE_SCHEMA
                JOIN INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu 
                    ON rc.UNIQUE_CONSTRAINT_NAME = ccu.CONSTRAINT_NAME
                    AND rc.UNIQUE_CONSTRAINT_SCHEMA = ccu.TABLE_SCHEMA
            ) fk ON c.TABLE_SCHEMA = fk.TABLE_SCHEMA AND c.TABLE_NAME = fk.TABLE_NAME AND c.COLUMN_NAME = fk.COLUMN_NAME
            WHERE t.TABLE_TYPE = 'BASE TABLE'
            ORDER BY t.TABLE_SCHEMA, t.TABLE_NAME, c.ORDINAL_POSITION";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        var currentTable = (DiscoveredTable?)null;

        while (await reader.ReadAsync())
        {
            var schemaName = reader.GetString("TABLE_SCHEMA");
            var tableName = reader.GetString("TABLE_NAME");
            var tableKey = $"{schemaName}.{tableName}";

            // Create new table if needed
            if (currentTable == null || currentTable.FullName != tableKey)
            {
                currentTable = new DiscoveredTable
                {
                    SchemaName = schemaName,
                    TableName = tableName,
                    FullName = tableKey,
                    Columns = new List<DiscoveredColumn>(),
                    RowCount = 0 // Will be populated separately if needed
                };
                tables.Add(currentTable);
            }

            // Add column if it exists (some tables might have no columns in edge cases)
            if (!reader.IsDBNull("COLUMN_NAME"))
            {
                var column = new DiscoveredColumn
                {
                    ColumnName = reader.GetString("COLUMN_NAME"),
                    DataType = reader.GetString("DATA_TYPE"),
                    IsNullable = reader.GetString("IS_NULLABLE") == "YES",
                    DefaultValue = reader.IsDBNull("COLUMN_DEFAULT") ? null : reader.GetString("COLUMN_DEFAULT"),
                    MaxLength = reader.IsDBNull("CHARACTER_MAXIMUM_LENGTH") ? null : reader.GetInt32("CHARACTER_MAXIMUM_LENGTH"),
                    Precision = reader.IsDBNull("NUMERIC_PRECISION") ? null : reader.GetByte("NUMERIC_PRECISION"),
                    Scale = reader.IsDBNull("NUMERIC_SCALE") ? null : reader.GetInt32("NUMERIC_SCALE"),
                    OrdinalPosition = reader.GetInt32("ORDINAL_POSITION"),
                    IsPrimaryKey = reader.GetInt32("IS_PRIMARY_KEY") == 1,
                    IsForeignKey = reader.GetInt32("IS_FOREIGN_KEY") == 1,
                    ReferencedTable = reader.IsDBNull("REFERENCED_TABLE_NAME") ? null : 
                        $"{reader.GetString("REFERENCED_TABLE_SCHEMA")}.{reader.GetString("REFERENCED_TABLE_NAME")}",
                    ReferencedColumn = reader.IsDBNull("REFERENCED_COLUMN_NAME") ? null : reader.GetString("REFERENCED_COLUMN_NAME")
                };

                currentTable.Columns.Add(column);
            }
        }

        _logger.LogInformation("🔍 Discovered {TableCount} tables", tables.Count);
        return tables;
    }

    /// <summary>
    /// Discover foreign key relationships
    /// </summary>
    private async Task<List<DiscoveredRelationship>> DiscoverRelationshipsAsync(SqlConnection connection)
    {
        var relationships = new List<DiscoveredRelationship>();

        var sql = @"
            SELECT 
                fk.name as CONSTRAINT_NAME,
                tp.name as PARENT_TABLE,
                sp.name as PARENT_SCHEMA,
                cp.name as PARENT_COLUMN,
                tr.name as REFERENCED_TABLE,
                sr.name as REFERENCED_SCHEMA,
                cr.name as REFERENCED_COLUMN
            FROM sys.foreign_keys fk
            INNER JOIN sys.tables tp ON fk.parent_object_id = tp.object_id
            INNER JOIN sys.schemas sp ON tp.schema_id = sp.schema_id
            INNER JOIN sys.tables tr ON fk.referenced_object_id = tr.object_id
            INNER JOIN sys.schemas sr ON tr.schema_id = sr.schema_id
            INNER JOIN sys.foreign_key_columns fkc ON fkc.constraint_object_id = fk.object_id
            INNER JOIN sys.columns cp ON fkc.parent_column_id = cp.column_id AND fkc.parent_object_id = cp.object_id
            INNER JOIN sys.columns cr ON fkc.referenced_column_id = cr.column_id AND fkc.referenced_object_id = cr.object_id
            ORDER BY fk.name";

        using var command = new SqlCommand(sql, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            relationships.Add(new DiscoveredRelationship
            {
                ConstraintName = reader.GetString("CONSTRAINT_NAME"),
                ParentTable = $"{reader.GetString("PARENT_SCHEMA")}.{reader.GetString("PARENT_TABLE")}",
                ParentColumn = reader.GetString("PARENT_COLUMN"),
                ReferencedTable = $"{reader.GetString("REFERENCED_SCHEMA")}.{reader.GetString("REFERENCED_TABLE")}",
                ReferencedColumn = reader.GetString("REFERENCED_COLUMN")
            });
        }

        return relationships;
    }

    /// <summary>
    /// Resolve Azure Key Vault placeholders in connection string    /// <summary>
    /// Extract database name from connection string
    /// </summary>
    private string GetDatabaseNameFromConnectionString(string connectionString)
    {
        try
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            return builder.InitialCatalog ?? "Unknown";
        }
        catch
        {
            return "Unknown";
        }
    }
}

/// <summary>
/// Result of schema discovery operation
/// </summary>
public class SchemaDiscoveryResult
{
    public string DatabaseName { get; set; } = string.Empty;
    public DateTime DiscoveredAt { get; set; }
    public List<DiscoveredTable> Tables { get; set; } = new();
    public List<DiscoveredRelationship> Relationships { get; set; } = new();
}

/// <summary>
/// Discovered table information
/// </summary>
public class DiscoveredTable
{
    public string SchemaName { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public List<DiscoveredColumn> Columns { get; set; } = new();
    public long RowCount { get; set; }
}

/// <summary>
/// Discovered column information
/// </summary>
public class DiscoveredColumn
{
    public string ColumnName { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public bool IsNullable { get; set; }
    public string? DefaultValue { get; set; }
    public int? MaxLength { get; set; }
    public byte? Precision { get; set; }
    public int? Scale { get; set; }
    public int OrdinalPosition { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public string? ReferencedTable { get; set; }
    public string? ReferencedColumn { get; set; }
}

/// <summary>
/// Discovered relationship information
/// </summary>
public class DiscoveredRelationship
{
    public string ConstraintName { get; set; } = string.Empty;
    public string ParentTable { get; set; } = string.Empty;
    public string ParentColumn { get; set; } = string.Empty;
    public string ReferencedTable { get; set; } = string.Empty;
    public string ReferencedColumn { get; set; } = string.Empty;
}
