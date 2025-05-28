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
            Result = new QueryResult
            {
                Data = new object[0],
                Metadata = new QueryMetadata()
            },
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
        _response.Sql = sql;
        return this;
    }

    /// <summary>
    /// Set the data rows
    /// </summary>
    public QueryResponseBuilder WithData(object[] data)
    {
        if (_response.Result == null)
            _response.Result = new QueryResult();

        _response.Result.Data = data;
        _response.Result.Metadata.RowCount = data?.Length ?? 0;
        return this;
    }

    /// <summary>
    /// Add a single data row (for dictionary-based data)
    /// </summary>
    public QueryResponseBuilder AddDataRow(Dictionary<string, object> row)
    {
        if (_response.Result == null)
            _response.Result = new QueryResult();

        var currentData = _response.Result.Data?.ToList() ?? new List<object>();
        currentData.Add(row);
        _response.Result.Data = currentData.ToArray();
        _response.Result.Metadata.RowCount = currentData.Count;
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
    public QueryResponseBuilder WithColumns(IEnumerable<ColumnMetadata> columns)
    {
        if (_response.Result == null)
            _response.Result = new QueryResult();

        _response.Result.Metadata.Columns = columns.ToArray();
        _response.Result.Metadata.ColumnCount = _response.Result.Metadata.Columns.Length;
        return this;
    }

    /// <summary>
    /// Add a column
    /// </summary>
    public QueryResponseBuilder AddColumn(string name, string dataType, bool isNullable = true)
    {
        if (_response.Result == null)
            _response.Result = new QueryResult();

        var currentColumns = _response.Result.Metadata.Columns?.ToList() ?? new List<ColumnMetadata>();
        currentColumns.Add(new ColumnMetadata
        {
            Name = name,
            DataType = dataType,
            IsNullable = isNullable
        });
        _response.Result.Metadata.Columns = currentColumns.ToArray();
        _response.Result.Metadata.ColumnCount = currentColumns.Count;
        return this;
    }

    /// <summary>
    /// Set the execution time
    /// </summary>
    public QueryResponseBuilder WithExecutionTime(long milliseconds)
    {
        _response.ExecutionTimeMs = (int)milliseconds;
        if (_response.Result != null)
        {
            _response.Result.Metadata.ExecutionTimeMs = (int)milliseconds;
        }
        return this;
    }

    /// <summary>
    /// Set the execution time using TimeSpan
    /// </summary>
    public QueryResponseBuilder WithExecutionTime(TimeSpan duration)
    {
        var ms = (int)duration.TotalMilliseconds;
        _response.ExecutionTimeMs = ms;
        if (_response.Result != null)
        {
            _response.Result.Metadata.ExecutionTimeMs = ms;
        }
        return this;
    }

    /// <summary>
    /// Set the confidence score
    /// </summary>
    public QueryResponseBuilder WithConfidenceScore(double score)
    {
        _response.Confidence = Math.Max(0.0, Math.Min(1.0, score));
        return this;
    }

    /// <summary>
    /// Set the error message
    /// </summary>
    public QueryResponseBuilder WithError(string errorMessage)
    {
        _response.Success = false;
        _response.Error = errorMessage;
        if (_response.Result != null)
        {
            _response.Result.IsSuccessful = false;
            _response.Result.Metadata.Error = errorMessage;
        }
        return this;
    }

    /// <summary>
    /// Set the error from an exception
    /// </summary>
    public QueryResponseBuilder WithError(Exception exception)
    {
        _response.Success = false;
        _response.Error = exception.Message;
        if (_response.Result != null)
        {
            _response.Result.IsSuccessful = false;
            _response.Result.Metadata.Error = exception.Message;
        }
        return this;
    }

    /// <summary>
    /// Set the visualization config
    /// </summary>
    public QueryResponseBuilder WithVisualizationConfig(VisualizationConfig config)
    {
        _response.Visualization = config;
        return this;
    }

    /// <summary>
    /// Set suggestions
    /// </summary>
    public QueryResponseBuilder WithSuggestions(params string[] suggestions)
    {
        _response.Suggestions = suggestions;
        return this;
    }

    /// <summary>
    /// Set cached flag
    /// </summary>
    public QueryResponseBuilder WithCached(bool cached)
    {
        _response.Cached = cached;
        return this;
    }

    /// <summary>
    /// Mark as cached response
    /// </summary>
    public QueryResponseBuilder FromCache()
    {
        _response.Cached = true;
        return this;
    }

    /// <summary>
    /// Mark as fresh response
    /// </summary>
    public QueryResponseBuilder Fresh()
    {
        _response.Cached = false;
        return this;
    }

    /// <summary>
    /// Build the final QueryResponse
    /// </summary>
    public QueryResponse Build()
    {
        // Validate the response
        if (_response.Success && string.IsNullOrEmpty(_response.Sql))
        {
            throw new InvalidOperationException("Successful response must have SQL");
        }

        if (!_response.Success && string.IsNullOrEmpty(_response.Error))
        {
            throw new InvalidOperationException("Failed response must have Error message");
        }

        // Set final timestamp
        _response.Timestamp = DateTime.UtcNow;

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
            Tables = new List<TableMetadata>(),
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
    public SchemaMetadataBuilder AddTable(TableMetadata table)
    {
        _schema.Tables.Add(table);
        return this;
    }

    /// <summary>
    /// Add a table using builder
    /// </summary>
    public SchemaMetadataBuilder AddTable(Action<TableMetadataBuilder> configure)
    {
        var builder = new TableMetadataBuilder();
        configure(builder);
        _schema.Tables.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Add multiple tables
    /// </summary>
    public SchemaMetadataBuilder WithTables(IEnumerable<TableMetadata> tables)
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
/// Builder for creating TableMetadata objects
/// </summary>
public class TableMetadataBuilder
{
    private readonly TableMetadata _table;

    public TableMetadataBuilder()
    {
        _table = new TableMetadata
        {
            Columns = new List<ColumnMetadata>(),
            LastUpdated = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Set the table name
    /// </summary>
    public TableMetadataBuilder WithName(string name)
    {
        _table.Name = name;
        return this;
    }

    /// <summary>
    /// Set the schema name
    /// </summary>
    public TableMetadataBuilder WithSchema(string schema)
    {
        _table.Schema = schema;
        return this;
    }

    /// <summary>
    /// Set the description
    /// </summary>
    public TableMetadataBuilder WithDescription(string description)
    {
        _table.Description = description;
        return this;
    }

    /// <summary>
    /// Set the row count
    /// </summary>
    public TableMetadataBuilder WithRowCount(long rowCount)
    {
        _table.RowCount = rowCount;
        return this;
    }

    /// <summary>
    /// Add a column
    /// </summary>
    public TableMetadataBuilder AddColumn(ColumnMetadata column)
    {
        _table.Columns.Add(column);
        return this;
    }

    /// <summary>
    /// Add a column using builder
    /// </summary>
    public TableMetadataBuilder AddColumn(Action<ColumnMetadataBuilder> configure)
    {
        var builder = new ColumnMetadataBuilder();
        configure(builder);
        _table.Columns.Add(builder.Build());
        return this;
    }

    /// <summary>
    /// Add a simple column
    /// </summary>
    public TableMetadataBuilder AddColumn(string name, string dataType, bool isNullable = true, bool isPrimaryKey = false)
    {
        _table.Columns.Add(new ColumnMetadata
        {
            Name = name,
            DataType = dataType,
            IsNullable = isNullable,
            IsPrimaryKey = isPrimaryKey
        });
        return this;
    }

    /// <summary>
    /// Build the final TableMetadata
    /// </summary>
    public TableMetadata Build()
    {
        if (string.IsNullOrEmpty(_table.Name))
        {
            throw new InvalidOperationException("Table name is required");
        }

        return _table;
    }
}

/// <summary>
/// Builder for creating ColumnMetadata objects
/// </summary>
public class ColumnMetadataBuilder
{
    private readonly ColumnMetadata _column;

    public ColumnMetadataBuilder()
    {
        _column = new ColumnMetadata
        {
            SampleValues = new string[0]
        };
    }

    /// <summary>
    /// Set the column name
    /// </summary>
    public ColumnMetadataBuilder WithName(string name)
    {
        _column.Name = name;
        return this;
    }

    /// <summary>
    /// Set the data type
    /// </summary>
    public ColumnMetadataBuilder WithDataType(string dataType)
    {
        _column.DataType = dataType;
        return this;
    }

    /// <summary>
    /// Set nullable flag
    /// </summary>
    public ColumnMetadataBuilder IsNullable(bool isNullable = true)
    {
        _column.IsNullable = isNullable;
        return this;
    }

    /// <summary>
    /// Set primary key flag
    /// </summary>
    public ColumnMetadataBuilder IsPrimaryKey(bool isPrimaryKey = true)
    {
        _column.IsPrimaryKey = isPrimaryKey;
        return this;
    }

    /// <summary>
    /// Set the maximum length
    /// </summary>
    public ColumnMetadataBuilder WithMaxLength(int? maxLength)
    {
        _column.MaxLength = maxLength;
        return this;
    }

    /// <summary>
    /// Set the description
    /// </summary>
    public ColumnMetadataBuilder WithDescription(string description)
    {
        _column.Description = description;
        return this;
    }

    /// <summary>
    /// Add sample values
    /// </summary>
    public ColumnMetadataBuilder WithSampleValues(params string[] values)
    {
        _column.SampleValues = _column.SampleValues.Concat(values).ToArray();
        return this;
    }

    /// <summary>
    /// Build the final ColumnMetadata
    /// </summary>
    public ColumnMetadata Build()
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
