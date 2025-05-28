# Data Integration and ETL Pipeline Architecture

## Overview
The AI-Powered BI Reporting Copilot requires a robust data integration layer to connect with various data sources, process schema metadata, and maintain data freshness for optimal query performance.

## Architecture Components

### 1. Data Source Connectors

#### Supported Data Sources
- **Primary**: Microsoft SQL Server (existing BI database)
- **Secondary**: PostgreSQL, MySQL, Oracle
- **Cloud**: Azure SQL Database, Amazon RDS
- **Data Warehouses**: Azure Synapse, Snowflake, BigQuery
- **APIs**: REST APIs, GraphQL endpoints
- **Files**: CSV, Excel, JSON, Parquet

#### Connection Management
```csharp
public interface IDataSourceConnector
{
    Task<bool> TestConnectionAsync(DataSourceConfig config);
    Task<SchemaMetadata> ExtractSchemaAsync(DataSourceConfig config);
    Task<QueryResult> ExecuteQueryAsync(string query, DataSourceConfig config);
    Task<DataSourceHealth> GetHealthStatusAsync(DataSourceConfig config);
}

public class SqlServerConnector : IDataSourceConnector
{
    private readonly ILogger<SqlServerConnector> _logger;
    private readonly IConnectionPoolManager _connectionPool;

    public async Task<SchemaMetadata> ExtractSchemaAsync(DataSourceConfig config)
    {
        using var connection = await _connectionPool.GetConnectionAsync(config);
        
        var schema = new SchemaMetadata
        {
            DatabaseName = config.DatabaseName,
            Tables = await ExtractTablesAsync(connection),
            Views = await ExtractViewsAsync(connection),
            Relationships = await ExtractRelationshipsAsync(connection),
            LastUpdated = DateTime.UtcNow
        };

        return schema;
    }
}
```

### 2. Schema Discovery and Metadata Management

#### Automated Schema Discovery
```sql
-- Enhanced schema extraction with business context
WITH TableInfo AS (
    SELECT 
        t.TABLE_SCHEMA,
        t.TABLE_NAME,
        t.TABLE_TYPE,
        ISNULL(ep.value, '') AS TABLE_DESCRIPTION
    FROM INFORMATION_SCHEMA.TABLES t
    LEFT JOIN sys.tables st ON st.name = t.TABLE_NAME
    LEFT JOIN sys.extended_properties ep ON ep.major_id = st.object_id 
        AND ep.minor_id = 0 AND ep.name = 'MS_Description'
    WHERE t.TABLE_TYPE = 'BASE TABLE'
),
ColumnInfo AS (
    SELECT 
        c.TABLE_SCHEMA,
        c.TABLE_NAME,
        c.COLUMN_NAME,
        c.DATA_TYPE,
        c.IS_NULLABLE,
        c.COLUMN_DEFAULT,
        c.CHARACTER_MAXIMUM_LENGTH,
        c.NUMERIC_PRECISION,
        c.NUMERIC_SCALE,
        ISNULL(ep.value, '') AS COLUMN_DESCRIPTION,
        CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_PRIMARY_KEY,
        CASE WHEN fk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS IS_FOREIGN_KEY
    FROM INFORMATION_SCHEMA.COLUMNS c
    LEFT JOIN sys.columns sc ON sc.name = c.COLUMN_NAME
    LEFT JOIN sys.tables st ON st.name = c.TABLE_NAME AND st.object_id = sc.object_id
    LEFT JOIN sys.extended_properties ep ON ep.major_id = st.object_id 
        AND ep.minor_id = sc.column_id AND ep.name = 'MS_Description'
    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE pk ON pk.TABLE_NAME = c.TABLE_NAME 
        AND pk.COLUMN_NAME = c.COLUMN_NAME 
        AND pk.CONSTRAINT_NAME LIKE 'PK_%'
    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE fk ON fk.TABLE_NAME = c.TABLE_NAME 
        AND fk.COLUMN_NAME = c.COLUMN_NAME 
        AND fk.CONSTRAINT_NAME LIKE 'FK_%'
)
SELECT 
    ti.TABLE_SCHEMA,
    ti.TABLE_NAME,
    ti.TABLE_DESCRIPTION,
    ci.COLUMN_NAME,
    ci.DATA_TYPE,
    ci.IS_NULLABLE,
    ci.COLUMN_DESCRIPTION,
    ci.IS_PRIMARY_KEY,
    ci.IS_FOREIGN_KEY
FROM TableInfo ti
INNER JOIN ColumnInfo ci ON ti.TABLE_NAME = ci.TABLE_NAME
ORDER BY ti.TABLE_NAME, ci.COLUMN_NAME;
```

#### Semantic Enhancement Pipeline
```csharp
public class SemanticEnhancementService
{
    private readonly IOpenAIService _openAI;
    private readonly ISchemaRepository _schemaRepository;

    public async Task<EnhancedSchemaMetadata> EnhanceSchemaAsync(SchemaMetadata rawSchema)
    {
        var enhancedSchema = new EnhancedSchemaMetadata();

        foreach (var table in rawSchema.Tables)
        {
            var enhancedTable = await EnhanceTableMetadataAsync(table);
            enhancedSchema.Tables.Add(enhancedTable);
        }

        return enhancedSchema;
    }

    private async Task<EnhancedTableMetadata> EnhanceTableMetadataAsync(TableMetadata table)
    {
        var prompt = $@"
        Analyze this database table and provide semantic information:
        
        Table: {table.Name}
        Columns: {string.Join(", ", table.Columns.Select(c => $"{c.Name} ({c.DataType})"))}
        
        Provide:
        1. Business purpose of this table
        2. Semantic tags for each column (e.g., 'date', 'currency', 'identifier', 'metric')
        3. Suggested natural language aliases
        4. Data quality indicators
        
        Return as JSON.";

        var response = await _openAI.GenerateCompletionAsync(prompt);
        var semanticInfo = JsonSerializer.Deserialize<SemanticTableInfo>(response);

        return new EnhancedTableMetadata
        {
            Name = table.Name,
            BusinessPurpose = semanticInfo.BusinessPurpose,
            Columns = table.Columns.Select(c => EnhanceColumnMetadata(c, semanticInfo)).ToList(),
            SemanticTags = semanticInfo.SemanticTags,
            NaturalLanguageAliases = semanticInfo.Aliases
        };
    }
}
```

### 3. Data Quality and Validation Pipeline

#### Data Quality Checks
```csharp
public class DataQualityService
{
    public async Task<DataQualityReport> AssessDataQualityAsync(string tableName)
    {
        var checks = new List<DataQualityCheck>
        {
            new CompletenessCheck(),
            new UniquenessCheck(),
            new ValidityCheck(),
            new ConsistencyCheck(),
            new TimelinessCheck()
        };

        var report = new DataQualityReport { TableName = tableName };

        foreach (var check in checks)
        {
            var result = await check.ExecuteAsync(tableName);
            report.CheckResults.Add(result);
        }

        return report;
    }
}

public class CompletenessCheck : IDataQualityCheck
{
    public async Task<DataQualityResult> ExecuteAsync(string tableName)
    {
        var sql = $@"
            SELECT 
                COLUMN_NAME,
                COUNT(*) as TotalRows,
                COUNT(COLUMN_NAME) as NonNullRows,
                CAST(COUNT(COLUMN_NAME) * 100.0 / COUNT(*) AS DECIMAL(5,2)) as CompletenessPercentage
            FROM {tableName}
            CROSS JOIN INFORMATION_SCHEMA.COLUMNS 
            WHERE TABLE_NAME = '{tableName}'
            GROUP BY COLUMN_NAME";

        // Execute and return results
        return new DataQualityResult
        {
            CheckType = "Completeness",
            Status = "Passed", // or "Failed" based on thresholds
            Details = "All columns have >95% completeness"
        };
    }
}
```

### 4. Real-time Data Synchronization

#### Change Data Capture (CDC)
```csharp
public class ChangeDataCaptureService
{
    private readonly IServiceBus _serviceBus;
    private readonly ISchemaRepository _schemaRepository;

    public async Task StartCDCMonitoringAsync(string tableName)
    {
        // Enable CDC on the table if not already enabled
        await EnableCDCAsync(tableName);

        // Start monitoring for changes
        var cancellationToken = new CancellationTokenSource();
        
        _ = Task.Run(async () =>
        {
            while (!cancellationToken.Token.IsCancellationRequested)
            {
                var changes = await GetTableChangesAsync(tableName);
                
                if (changes.Any())
                {
                    await ProcessChangesAsync(tableName, changes);
                    await NotifySchemaChangesAsync(tableName, changes);
                }

                await Task.Delay(TimeSpan.FromMinutes(5), cancellationToken.Token);
            }
        }, cancellationToken.Token);
    }

    private async Task ProcessChangesAsync(string tableName, List<ChangeRecord> changes)
    {
        foreach (var change in changes)
        {
            switch (change.OperationType)
            {
                case "INSERT":
                case "UPDATE":
                    await UpdateSchemaStatisticsAsync(tableName);
                    break;
                case "DELETE":
                    await UpdateRowCountAsync(tableName);
                    break;
            }
        }

        // Invalidate related query cache
        await InvalidateQueryCacheAsync(tableName);
    }
}
```

### 5. Performance Optimization

#### Query Performance Monitoring
```csharp
public class QueryPerformanceService
{
    public async Task<QueryOptimizationSuggestions> AnalyzeQueryPerformanceAsync(string sql)
    {
        var suggestions = new QueryOptimizationSuggestions();

        // Analyze execution plan
        var executionPlan = await GetExecutionPlanAsync(sql);
        suggestions.AddRange(AnalyzeExecutionPlan(executionPlan));

        // Check for missing indexes
        var missingIndexes = await GetMissingIndexSuggestionsAsync(sql);
        suggestions.AddRange(missingIndexes);

        // Analyze query complexity
        var complexity = AnalyzeQueryComplexity(sql);
        if (complexity.Score > 0.8)
        {
            suggestions.Add(new OptimizationSuggestion
            {
                Type = "Complexity",
                Description = "Consider breaking down this complex query into simpler parts",
                Impact = "High"
            });
        }

        return suggestions;
    }
}
```

### 6. Data Lineage and Impact Analysis

#### Lineage Tracking
```csharp
public class DataLineageService
{
    public async Task<DataLineageGraph> BuildLineageGraphAsync(string tableName)
    {
        var graph = new DataLineageGraph();

        // Find upstream dependencies
        var upstreamTables = await FindUpstreamTablesAsync(tableName);
        foreach (var upstream in upstreamTables)
        {
            graph.AddEdge(upstream, tableName, "feeds_into");
        }

        // Find downstream dependencies
        var downstreamTables = await FindDownstreamTablesAsync(tableName);
        foreach (var downstream in downstreamTables)
        {
            graph.AddEdge(tableName, downstream, "feeds_into");
        }

        // Add query dependencies
        var relatedQueries = await FindQueriesUsingTableAsync(tableName);
        foreach (var query in relatedQueries)
        {
            graph.AddEdge(tableName, query.Id, "used_by_query");
        }

        return graph;
    }

    public async Task<ImpactAnalysisResult> AnalyzeImpactAsync(string tableName, ChangeType changeType)
    {
        var lineage = await BuildLineageGraphAsync(tableName);
        var impactedEntities = new List<string>();

        // Traverse the graph to find all impacted entities
        TraverseGraph(lineage, tableName, impactedEntities);

        return new ImpactAnalysisResult
        {
            SourceTable = tableName,
            ChangeType = changeType,
            ImpactedTables = impactedEntities.Where(e => e.StartsWith("table_")).ToList(),
            ImpactedQueries = impactedEntities.Where(e => e.StartsWith("query_")).ToList(),
            EstimatedImpactLevel = CalculateImpactLevel(impactedEntities.Count)
        };
    }
}
```

### 7. Configuration and Deployment

#### Data Source Configuration
```json
{
  "DataSources": {
    "Primary": {
      "Type": "SqlServer",
      "ConnectionString": "Server=185.64.56.157;Database=DailyActionsDB;Integrated Security=true;",
      "ReadOnly": true,
      "MaxConnections": 10,
      "CommandTimeout": 30,
      "EnableCDC": true,
      "SchemaRefreshInterval": "01:00:00"
    },
    "Warehouse": {
      "Type": "AzureSynapse",
      "ConnectionString": "Server=synapse.sql.azuresynapse.net;Database=warehouse;",
      "ReadOnly": true,
      "MaxConnections": 5,
      "CommandTimeout": 120
    }
  },
  "ETL": {
    "SchemaRefreshSchedule": "0 2 * * *", // Daily at 2 AM
    "DataQualityCheckSchedule": "0 */6 * * *", // Every 6 hours
    "MaxConcurrentJobs": 3,
    "RetryAttempts": 3,
    "RetryDelaySeconds": 30
  }
}
```

## Monitoring and Alerting

### Key Metrics to Monitor
- Schema refresh success rate
- Data quality scores
- Query performance metrics
- Connection pool utilization
- ETL job execution times
- Data freshness indicators

### Alert Conditions
- Schema changes detected
- Data quality below threshold
- Connection failures
- ETL job failures
- Performance degradation

This architecture ensures reliable, scalable, and maintainable data integration for the BI Reporting Copilot system.
