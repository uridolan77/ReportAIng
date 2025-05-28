using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Builders;

/// <summary>
/// Builder for creating QueryResponse objects with fluent API
/// </summary>
public class QueryResponseBuilder
{
    private readonly QueryResponse _response;

    public QueryResponseBuilder()
    {
        _response = new QueryResponse
        {
            Success = true,
            Data = new List<Dictionary<string, object>>(),
            Columns = new List<ColumnInfo>(),
            Metadata = new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Set the success status
    /// </summary>
    public QueryResponseBuilder WithSuccess(bool success)
    {
        _response.Success = success;
        return this;
    }

    /// <summary>
    /// Set the generated SQL
    /// </summary>
    public QueryResponseBuilder WithGeneratedSQL(string sql)
    {
        _response.GeneratedSQL = sql;
        return this;
    }

    /// <summary>
    /// Set the data rows
    /// </summary>
    public QueryResponseBuilder WithData(IEnumerable<Dictionary<string, object>> data)
    {
        _response.Data = data.ToList();
        _response.RowCount = _response.Data.Count;
        return this;
    }

    /// <summary>
    /// Add a single data row
    /// </summary>
    public QueryResponseBuilder AddDataRow(Dictionary<string, object> row)
    {
        _response.Data.Add(row);
        _response.RowCount = _response.Data.Count;
        return this;
    }

    /// <summary>
    /// Add a data row using anonymous object
    /// </summary>
    public QueryResponseBuilder AddDataRow(object row)
    {
        var dictionary = row.GetType()
            .GetProperties()
            .ToDictionary(prop => prop.Name, prop => prop.GetValue(row) ?? DBNull.Value);
        
        return AddDataRow(dictionary);
    }

    /// <summary>
    /// Set the column information
    /// </summary>
    public QueryResponseBuilder WithColumns(IEnumerable<ColumnInfo> columns)
    {
        _response.Columns = columns.ToList();
        return this;
    }

    /// <summary>
    /// Add a column
    /// </summary>
    public QueryResponseBuilder AddColumn(string name, string dataType, bool isNullable = true)
    {
        _response.Columns.Add(new ColumnInfo
        {
            Name = name,
            DataType = dataType,
            IsNullable = isNullable
        });
        return this;
    }

    /// <summary>
    /// Set the execution time
    /// </summary>
    public QueryResponseBuilder WithExecutionTime(long milliseconds)
    {
        _response.ExecutionTimeMs = milliseconds;
        return this;
    }

    /// <summary>
    /// Set the execution time using TimeSpan
    /// </summary>
    public QueryResponseBuilder WithExecutionTime(TimeSpan duration)
    {
        _response.ExecutionTimeMs = (long)duration.TotalMilliseconds;
        return this;
    }

    /// <summary>
    /// Set the confidence score
    /// </summary>
    public QueryResponseBuilder WithConfidenceScore(double score)
    {
        _response.ConfidenceScore = Math.Max(0.0, Math.Min(1.0, score));
        return this;
    }

    /// <summary>
    /// Set the error message
    /// </summary>
    public QueryResponseBuilder WithError(string errorMessage)
    {
        _response.Success = false;
        _response.ErrorMessage = errorMessage;
        return this;
    }

    /// <summary>
    /// Set the error from an exception
    /// </summary>
    public QueryResponseBuilder WithError(Exception exception)
    {
        _response.Success = false;
        _response.ErrorMessage = exception.Message;
        return this;
    }

    /// <summary>
    /// Add metadata
    /// </summary>
    public QueryResponseBuilder WithMetadata(string key, object value)
    {
        _response.Metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Add multiple metadata entries
    /// </summary>
    public QueryResponseBuilder WithMetadata(Dictionary<string, object> metadata)
    {
        foreach (var kvp in metadata)
        {
            _response.Metadata[kvp.Key] = kvp.Value;
        }
        return this;
    }

    /// <summary>
    /// Set the explanation
    /// </summary>
    public QueryResponseBuilder WithExplanation(string explanation)
    {
        _response.Explanation = explanation;
        return this;
    }

    /// <summary>
    /// Set the visualization config
    /// </summary>
    public QueryResponseBuilder WithVisualizationConfig(string config)
    {
        _response.VisualizationConfig = config;
        return this;
    }

    /// <summary>
    /// Set cache information
    /// </summary>
    public QueryResponseBuilder WithCacheInfo(bool fromCache, string? cacheKey = null, DateTime? expiresAt = null)
    {
        WithMetadata("FromCache", fromCache);
        if (cacheKey != null)
            WithMetadata("CacheKey", cacheKey);
        if (expiresAt.HasValue)
            WithMetadata("CacheExpiresAt", expiresAt.Value);
        return this;
    }

    /// <summary>
    /// Set query suggestions
    /// </summary>
    public QueryResponseBuilder WithSuggestions(IEnumerable<string> suggestions)
    {
        WithMetadata("Suggestions", suggestions.ToList());
        return this;
    }

    /// <summary>
    /// Mark as cached response
    /// </summary>
    public QueryResponseBuilder FromCache(string cacheKey, DateTime expiresAt)
    {
        return WithCacheInfo(true, cacheKey, expiresAt);
    }

    /// <summary>
    /// Mark as fresh response
    /// </summary>
    public QueryResponseBuilder Fresh()
    {
        return WithCacheInfo(false);
    }

    /// <summary>
    /// Build the final QueryResponse
    /// </summary>
    public QueryResponse Build()
    {
        // Validate the response
        if (_response.Success && string.IsNullOrEmpty(_response.GeneratedSQL))
        {
            throw new InvalidOperationException("Successful response must have GeneratedSQL");
        }

        if (!_response.Success && string.IsNullOrEmpty(_response.ErrorMessage))
        {
            throw new InvalidOperationException("Failed response must have ErrorMessage");
        }

        // Set final metadata
        _response.Metadata["BuildTimestamp"] = DateTime.UtcNow;
        
        return _response;
    }

    /// <summary>
    /// Create a builder for a successful response
    /// </summary>
    public static QueryResponseBuilder Success()
    {
        return new QueryResponseBuilder().WithSuccess(true);
    }

    /// <summary>
    /// Create a builder for a failed response
    /// </summary>
    public static QueryResponseBuilder Failure(string errorMessage)
    {
        return new QueryResponseBuilder().WithError(errorMessage);
    }

    /// <summary>
    /// Create a builder for a failed response from exception
    /// </summary>
    public static QueryResponseBuilder Failure(Exception exception)
    {
        return new QueryResponseBuilder().WithError(exception);
    }
}

/// <summary>
/// Builder for creating SchemaMetadata objects
/// </summary>
public class SchemaMetadataBuilder
{
    private readonly SchemaMetadata _schema;

    public SchemaMetadataBuilder()
    {
        _schema = new SchemaMetadata
        {
            Tables = new List<TableInfo>(),
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Set the database name
    /// </summary>
    public SchemaMetadataBuilder WithDatabaseName(string databaseName)
    {
        _schema.DatabaseName = databaseName;
        return this;
    }

    /// <summary>
    /// Add a table
    /// </summary>
    public SchemaMetadataBuilder AddTable(TableInfo table)
    {
        _schema.Tables.Add(table);
        return this;
    }

    /// <summary>
    /// Add a table using builder
    /// </summary>
    public SchemaMetadataBuilder AddTable(Action<TableInfoBuilder> configure)
    {
        var builder = new TableInfoBuilder();
        configure(builder);
        _schema.Tables.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Add multiple tables
    /// </summary>
    public SchemaMetadataBuilder WithTables(IEnumerable<TableInfo> tables)
    {
        _schema.Tables.AddRange(tables);
        return this;
    }

    /// <summary>
    /// Set the last updated timestamp
    /// </summary>
    public SchemaMetadataBuilder WithLastUpdated(DateTime lastUpdated)
    {
        _schema.LastUpdated = lastUpdated;
        return this;
    }

    /// <summary>
    /// Build the final SchemaMetadata
    /// </summary>
    public SchemaMetadata Build()
    {
        if (string.IsNullOrEmpty(_schema.DatabaseName))
        {
            throw new InvalidOperationException("DatabaseName is required");
        }

        return _schema;
    }
}

/// <summary>
/// Builder for creating TableInfo objects
/// </summary>
public class TableInfoBuilder
{
    private readonly TableInfo _table;

    public TableInfoBuilder()
    {
        _table = new TableInfo
        {
            Columns = new List<ColumnInfo>(),
            Metadata = new Dictionary<string, object>()
        };
    }

    /// <summary>
    /// Set the table name
    /// </summary>
    public TableInfoBuilder WithName(string name)
    {
        _table.Name = name;
        return this;
    }

    /// <summary>
    /// Set the schema name
    /// </summary>
    public TableInfoBuilder WithSchema(string schema)
    {
        _table.Schema = schema;
        return this;
    }

    /// <summary>
    /// Set the description
    /// </summary>
    public TableInfoBuilder WithDescription(string description)
    {
        _table.Description = description;
        return this;
    }

    /// <summary>
    /// Set the row count
    /// </summary>
    public TableInfoBuilder WithRowCount(long rowCount)
    {
        _table.RowCount = rowCount;
        return this;
    }

    /// <summary>
    /// Add a column
    /// </summary>
    public TableInfoBuilder AddColumn(ColumnInfo column)
    {
        _table.Columns.Add(column);
        return this;
    }

    /// <summary>
    /// Add a column using builder
    /// </summary>
    public TableInfoBuilder AddColumn(Action<ColumnInfoBuilder> configure)
    {
        var builder = new ColumnInfoBuilder();
        configure(builder);
        _table.Columns.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Add a simple column
    /// </summary>
    public TableInfoBuilder AddColumn(string name, string dataType, bool isNullable = true, bool isPrimaryKey = false)
    {
        _table.Columns.Add(new ColumnInfo
        {
            Name = name,
            DataType = dataType,
            IsNullable = isNullable,
            IsPrimaryKey = isPrimaryKey
        });
        return this;
    }

    /// <summary>
    /// Add metadata
    /// </summary>
    public TableInfoBuilder WithMetadata(string key, object value)
    {
        _table.Metadata[key] = value;
        return this;
    }

    /// <summary>
    /// Build the final TableInfo
    /// </summary>
    public TableInfo Build()
    {
        if (string.IsNullOrEmpty(_table.Name))
        {
            throw new InvalidOperationException("Table name is required");
        }

        return _table;
    }
}

/// <summary>
/// Builder for creating ColumnInfo objects
/// </summary>
public class ColumnInfoBuilder
{
    private readonly ColumnInfo _column;

    public ColumnInfoBuilder()
    {
        _column = new ColumnInfo
        {
            SampleValues = new List<string>()
        };
    }

    /// <summary>
    /// Set the column name
    /// </summary>
    public ColumnInfoBuilder WithName(string name)
    {
        _column.Name = name;
        return this;
    }

    /// <summary>
    /// Set the data type
    /// </summary>
    public ColumnInfoBuilder WithDataType(string dataType)
    {
        _column.DataType = dataType;
        return this;
    }

    /// <summary>
    /// Set nullable flag
    /// </summary>
    public ColumnInfoBuilder IsNullable(bool isNullable = true)
    {
        _column.IsNullable = isNullable;
        return this;
    }

    /// <summary>
    /// Set primary key flag
    /// </summary>
    public ColumnInfoBuilder IsPrimaryKey(bool isPrimaryKey = true)
    {
        _column.IsPrimaryKey = isPrimaryKey;
        return this;
    }

    /// <summary>
    /// Set the maximum length
    /// </summary>
    public ColumnInfoBuilder WithMaxLength(int? maxLength)
    {
        _column.MaxLength = maxLength;
        return this;
    }

    /// <summary>
    /// Set the description
    /// </summary>
    public ColumnInfoBuilder WithDescription(string description)
    {
        _column.Description = description;
        return this;
    }

    /// <summary>
    /// Add sample values
    /// </summary>
    public ColumnInfoBuilder WithSampleValues(params string[] values)
    {
        _column.SampleValues.AddRange(values);
        return this;
    }

    /// <summary>
    /// Build the final ColumnInfo
    /// </summary>
    public ColumnInfo Build()
    {
        if (string.IsNullOrEmpty(_column.Name))
        {
            throw new InvalidOperationException("Column name is required");
        }

        if (string.IsNullOrEmpty(_column.DataType))
        {
            throw new InvalidOperationException("Column data type is required");
        }

        return _column;
    }
}
