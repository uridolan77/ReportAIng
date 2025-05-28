using Hangfire;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Jobs;

public interface ISchemaRefreshJob
{
    Task RefreshAllSchemas();
    Task RefreshSchemaForDataSource(string dataSource);
}

public class SchemaRefreshJob : ISchemaRefreshJob
{
    private readonly ISchemaService _schemaService;
    private readonly IAuditService _auditService;
    private readonly ICacheService _cacheService;
    private readonly IQueryService _queryService;
    private readonly ILogger<SchemaRefreshJob> _logger;
    private readonly IHubContext<Hub> _hubContext;

    public SchemaRefreshJob(
        ISchemaService schemaService,
        IAuditService auditService,
        ICacheService cacheService,
        IQueryService queryService,
        ILogger<SchemaRefreshJob> logger,
        IHubContext<Hub> hubContext)
    {
        _schemaService = schemaService;
        _auditService = auditService;
        _cacheService = cacheService;
        _queryService = queryService;
        _logger = logger;
        _hubContext = hubContext;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task RefreshAllSchemas()
    {
        _logger.LogInformation("Starting schema refresh job");

        try
        {
            // For now, refresh the default data source
            // In a real implementation, you'd get this from configuration
            var dataSources = new[] { "default" };

            foreach (var dataSource in dataSources)
            {
                await RefreshSchemaForDataSource(dataSource);
            }

            await _auditService.LogAsync(
                "SCHEMA_REFRESH_COMPLETED",
                "System",
                "SchemaMetadata",
                null,
                new { DataSourceCount = dataSources.Length });

            // Notify all connected clients
            await _hubContext.Clients.All.SendAsync(
                "SystemAlert",
                new {
                    Type = "SchemaRefresh",
                    Message = "Database schema has been refreshed",
                    Level = "info"
                });

            _logger.LogInformation("Schema refresh job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in schema refresh job");
            throw;
        }
    }

    public async Task RefreshSchemaForDataSource(string dataSource)
    {
        _logger.LogInformation("Refreshing schema for data source: {DataSource}", dataSource);

        try
        {
            var oldSchema = await _schemaService.GetSchemaMetadataAsync(dataSource);
            var newSchema = await _schemaService.RefreshSchemaMetadataAsync(dataSource);

            // Detect changes
            var changes = DetectSchemaChanges(oldSchema, newSchema);

            if (changes.Any())
            {
                _logger.LogWarning("Schema changes detected: {ChangeCount} changes", changes.Count);

                foreach (var change in changes)
                {
                    await _auditService.LogAsync(
                        "SCHEMA_CHANGE_DETECTED",
                        "System",
                        "SchemaMetadata",
                        change.ObjectName,
                        change);
                }

                // Invalidate affected query caches
                await InvalidateAffectedCaches(changes);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing schema for data source: {DataSource}", dataSource);
            throw;
        }
    }

    private List<SchemaChangeEvent> DetectSchemaChanges(
        SchemaMetadata oldSchema,
        SchemaMetadata newSchema)
    {
        var changes = new List<SchemaChangeEvent>();

        // Detect new tables
        var newTables = newSchema.Tables
            .Where(t => !oldSchema.Tables.Any(ot => ot.Name == t.Name))
            .Select(t => new SchemaChangeEvent
            {
                Id = Guid.NewGuid().ToString(),
                ChangeType = "TABLE_ADDED",
                ObjectName = t.Name,
                ObjectType = "TABLE",
                NewValue = new Dictionary<string, object> { ["table"] = t }
            });

        changes.AddRange(newTables);

        // Detect removed tables
        var removedTables = oldSchema.Tables
            .Where(t => !newSchema.Tables.Any(nt => nt.Name == t.Name))
            .Select(t => new SchemaChangeEvent
            {
                Id = Guid.NewGuid().ToString(),
                ChangeType = "TABLE_REMOVED",
                ObjectName = t.Name,
                ObjectType = "TABLE",
                OldValue = new Dictionary<string, object> { ["table"] = t }
            });

        changes.AddRange(removedTables);

        // Detect column changes
        foreach (var newTable in newSchema.Tables)
        {
            var oldTable = oldSchema.Tables.FirstOrDefault(t => t.Name == newTable.Name);
            if (oldTable != null)
            {
                var columnChanges = DetectColumnChanges(oldTable, newTable);
                changes.AddRange(columnChanges);
            }
        }

        return changes;
    }

    private List<SchemaChangeEvent> DetectColumnChanges(
        TableMetadata oldTable,
        TableMetadata newTable)
    {
        var changes = new List<SchemaChangeEvent>();

        // Detect new columns
        var newColumns = newTable.Columns
            .Where(c => !oldTable.Columns.Any(oc => oc.Name == c.Name))
            .Select(c => new SchemaChangeEvent
            {
                Id = Guid.NewGuid().ToString(),
                ChangeType = "COLUMN_ADDED",
                ObjectName = $"{newTable.Name}.{c.Name}",
                ObjectType = "COLUMN",
                NewValue = new Dictionary<string, object> { ["column"] = c }
            });

        changes.AddRange(newColumns);

        // Detect modified columns
        foreach (var newColumn in newTable.Columns)
        {
            var oldColumn = oldTable.Columns.FirstOrDefault(c => c.Name == newColumn.Name);
            if (oldColumn != null && !ColumnsEqual(oldColumn, newColumn))
            {
                changes.Add(new SchemaChangeEvent
                {
                    Id = Guid.NewGuid().ToString(),
                    ChangeType = "COLUMN_MODIFIED",
                    ObjectName = $"{newTable.Name}.{newColumn.Name}",
                    ObjectType = "COLUMN",
                    OldValue = new Dictionary<string, object> { ["column"] = oldColumn },
                    NewValue = new Dictionary<string, object> { ["column"] = newColumn }
                });
            }
        }

        return changes;
    }

    private bool ColumnsEqual(ColumnMetadata col1, ColumnMetadata col2)
    {
        return col1.DataType == col2.DataType &&
               col1.IsNullable == col2.IsNullable &&
               col1.MaxLength == col2.MaxLength;
    }

    private async Task InvalidateAffectedCaches(List<SchemaChangeEvent> changes)
    {
        try
        {
            _logger.LogInformation("Invalidating caches affected by {ChangeCount} schema changes", changes.Count);

            // Invalidate query caches that might be affected by schema changes
            foreach (var change in changes)
            {
                if (change.ObjectType == "TABLE")
                {
                    // Invalidate all query caches that reference this table
                    await _queryService.InvalidateQueryCacheAsync($"*{change.ObjectName}*");
                    _logger.LogInformation("Invalidated query cache for table: {TableName}", change.ObjectName);
                }
                else if (change.ObjectType == "COLUMN")
                {
                    // Extract table name from column reference (format: table.column)
                    var tableName = change.ObjectName.Split('.')[0];
                    await _queryService.InvalidateQueryCacheAsync($"*{tableName}*");
                    _logger.LogInformation("Invalidated query cache for table due to column change: {TableName}", tableName);
                }
            }

            // Invalidate schema-related caches
            await _cacheService.RemovePatternAsync("schema:*");
            await _cacheService.RemovePatternAsync("table:*");

            _logger.LogInformation("Schema and table metadata caches invalidated");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error invalidating caches after schema changes");
        }
    }
}

public class SchemaChangeEvent
{
    public string Id { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string ObjectName { get; set; } = string.Empty;
    public string ObjectType { get; set; } = string.Empty;
    public Dictionary<string, object>? OldValue { get; set; }
    public Dictionary<string, object>? NewValue { get; set; }
}
