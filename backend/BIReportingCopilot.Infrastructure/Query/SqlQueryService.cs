using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.DTOs;
using BIReportingCopilot.Core.Models.DTOs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Security;
using BIReportingCopilot.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using System.Text.Json;
using QueryPerformanceMetrics = BIReportingCopilot.Core.DTOs.QueryPerformanceMetrics;

namespace BIReportingCopilot.Infrastructure.Query;

public class SqlQueryService : ISqlQueryService
{
    private readonly ILogger<SqlQueryService> _logger;
    private readonly IConfiguration _configuration;
    private readonly BICopilotContext _context;
    private readonly ISqlQueryValidator _sqlValidator;
    private readonly IConnectionStringProvider _connectionStringProvider;

    public SqlQueryService(
        ILogger<SqlQueryService> logger,
        IConfiguration configuration,
        BICopilotContext context,
        ISqlQueryValidator sqlValidator,
        IConnectionStringProvider connectionStringProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _context = context;
        _sqlValidator = sqlValidator;
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<QueryResult> ExecuteSelectQueryAsync(string sql, QueryOptions? options = null)
    {
        return await ExecuteSelectQueryAsync(sql, options, CancellationToken.None);
    }

    public async Task<QueryResult> ExecuteSelectQueryAsync(string sql, QueryOptions? options, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            // Validate SQL first
            if (!await ValidateSqlAsync(sql))
            {
                throw new InvalidOperationException("SQL query validation failed");
            }

            var connectionString = await GetConnectionStringAsync(options?.DataSource);
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No valid connection string available");
            }

            var result = await ExecuteQueryInternalAsync(sql, connectionString, options, cancellationToken);

            stopwatch.Stop();
            result.Metadata.ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds;

            // Log performance metrics
            await LogQueryPerformanceAsync(sql, result, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            // Log detailed error information
            _logger.LogError(ex, "❌ SQL EXECUTION FAILED - SQL: {Sql}", sql);
            _logger.LogError("❌ ERROR TYPE: {ErrorType}", ex.GetType().Name);
            _logger.LogError("❌ ERROR MESSAGE: {ErrorMessage}", ex.Message);

            if (ex.InnerException != null)
            {
                _logger.LogError("❌ INNER EXCEPTION: {InnerException}", ex.InnerException.Message);
            }

            // Log SQL Server specific error details if available
            if (ex is SqlException sqlEx)
            {
                _logger.LogError("❌ SQL ERROR NUMBER: {ErrorNumber}", sqlEx.Number);
                _logger.LogError("❌ SQL ERROR SEVERITY: {Severity}", sqlEx.Class);
                _logger.LogError("❌ SQL ERROR STATE: {State}", sqlEx.State);
                _logger.LogError("❌ SQL ERROR LINE: {LineNumber}", sqlEx.LineNumber);
                _logger.LogError("❌ SQL ERROR PROCEDURE: {Procedure}", sqlEx.Procedure ?? "N/A");
            }

            var errorMessage = ex is SqlException sqlException
                ? $"SQL Error {sqlException.Number}: {sqlException.Message}"
                : ex.Message;

            return new QueryResult
            {
                Data = Array.Empty<object>(),
                Metadata = new QueryMetadata
                {
                    ColumnCount = 0,
                    RowCount = 0,
                    ExecutionTimeMs = (int)stopwatch.ElapsedMilliseconds,
                    Columns = Array.Empty<ColumnMetadata>(),
                    Error = errorMessage
                },
                IsSuccessful = false
            };
        }
    }

    public async Task<bool> ValidateSqlAsync(string sql)
    {
        try
        {
            var result = await _sqlValidator.ValidateQueryAsync(sql);

            if (!result.IsValid)
            {
                _logger.LogWarning("🔍 SQL VALIDATION FAILED - SQL: {Sql}", sql);
                _logger.LogWarning("🔍 SQL VALIDATION ERRORS: {Errors}", string.Join(", ", result.Errors));
                _logger.LogWarning("🔍 SQL VALIDATION WARNINGS: {Warnings}", string.Join(", ", result.Warnings));
                _logger.LogWarning("🔍 SQL VALIDATION SECURITY LEVEL: {SecurityLevel}", result.SecurityLevel);
            }
            else
            {
                _logger.LogDebug("✅ SQL VALIDATION PASSED - SQL: {Sql}", sql);
            }

            return result.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SQL validation failed for query: {Sql}", sql);
            return false;
        }
    }

    public Task<string> OptimizeSqlAsync(string sql)
    {
        try
        {
            // Basic optimization - remove unnecessary whitespace and format
            var optimized = sql.Trim()
                .Replace("\r\n", " ")
                .Replace("\n", " ")
                .Replace("\t", " ");

            // Remove multiple spaces
            while (optimized.Contains("  "))
            {
                optimized = optimized.Replace("  ", " ");
            }

            _logger.LogDebug("Optimized SQL query from {OriginalLength} to {OptimizedLength} characters",
                sql.Length, optimized.Length);

            return Task.FromResult(optimized);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "SQL optimization failed, returning original query");
            return Task.FromResult(sql);
        }
    }

    public async Task<QueryExecutionPlan> GetExecutionPlanAsync(string sql)
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

            // Get execution plan
            var planCommand = new SqlCommand($"SET SHOWPLAN_XML ON; {sql}", connection);
            var planXml = await planCommand.ExecuteScalarAsync() as string ?? string.Empty;

            return new QueryExecutionPlan
            {
                PlanXml = planXml,
                EstimatedCost = ExtractCostFromPlan(planXml),
                Recommendations = GenerateRecommendations(planXml)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution plan for SQL: {Sql}", sql);
            return new QueryExecutionPlan
            {
                PlanXml = string.Empty,
                EstimatedCost = 0,
                Recommendations = new List<string> { "Unable to generate execution plan" }
            };
        }
    }

    public async Task<BIReportingCopilot.Core.DTOs.QueryPerformanceMetrics> GetQueryPerformanceAsync(string sql)
    {
        try
        {
            var connectionString = await GetConnectionStringAsync();
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("No connection string available");
            }

            var stopwatch = Stopwatch.StartNew();

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand(sql, connection);
            command.CommandTimeout = 30; // 30 second timeout

            await command.ExecuteNonQueryAsync();
            stopwatch.Stop();

            var performanceLevel = stopwatch.ElapsedMilliseconds switch
            {
                < 100 => "Excellent",
                < 500 => "Good",
                < 2000 => "Fair",
                _ => "Poor"
            };

            return new QueryPerformanceMetrics
            {
                ExecutionTime = stopwatch.Elapsed,
                PerformanceLevel = performanceLevel,
                QueryHash = ComputeQueryHash(sql),
                ExecutedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error measuring query performance for SQL: {Sql}", sql);
            return new QueryPerformanceMetrics
            {
                ExecutionTime = TimeSpan.Zero,
                PerformanceLevel = "Unknown",
                QueryHash = ComputeQueryHash(sql),
                ExecutedAt = DateTime.UtcNow
            };
        }
    }

    public async Task<bool> TestConnectionAsync(string? dataSource = null)
    {
        try
        {
            var connectionString = await GetConnectionStringAsync(dataSource);
            if (string.IsNullOrEmpty(connectionString))
            {
                return false;
            }

            using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = new SqlCommand("SELECT 1", connection);
            var result = await command.ExecuteScalarAsync();

            return result?.ToString() == "1";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Connection test failed for data source: {DataSource}", dataSource ?? "default");
            return false;
        }
    }

    public Task<List<string>> GetAvailableDataSourcesAsync()
    {
        var dataSources = new List<string>();

        // Add BI database connection if available
        var biConnection = _configuration.GetConnectionString("BIDatabase");
        if (!string.IsNullOrEmpty(biConnection))
        {
            dataSources.Add("BIDatabase");
        }

        // Add default connection if available
        var defaultConnection = _configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(defaultConnection))
        {
            dataSources.Add("DefaultConnection");
        }

        // Add any additional configured data sources
        var additionalSources = _configuration.GetSection("DataSources").GetChildren();
        foreach (var source in additionalSources)
        {
            if (!string.IsNullOrEmpty(source.Value))
            {
                dataSources.Add(source.Key);
            }
        }

        return Task.FromResult(dataSources);
    }

    private async Task<string?> GetConnectionStringAsync(string? dataSource = null)
    {
        if (string.IsNullOrEmpty(dataSource))
        {
            // Use BIDatabase as the primary connection for queries
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

    private async Task<QueryResult> ExecuteQueryInternalAsync(string sql, string connectionString, QueryOptions? options, CancellationToken cancellationToken)
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new SqlCommand(sql, connection);
        command.CommandTimeout = options?.TimeoutSeconds ?? 30;

        using var reader = await command.ExecuteReaderAsync(cancellationToken);

        var columns = new List<ColumnMetadata>();
        for (int i = 0; i < reader.FieldCount; i++)
        {
            columns.Add(new ColumnMetadata
            {
                Name = reader.GetName(i),
                DataType = reader.GetFieldType(i).Name
            });
        }

        var data = new List<object>();
        var maxRows = options?.MaxRows ?? 1000;
        var rowCount = 0;

        while (await reader.ReadAsync(cancellationToken) && rowCount < maxRows)
        {
            var row = new Dictionary<string, object?>();
            for (int i = 0; i < reader.FieldCount; i++)
            {
                row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }
            data.Add(row);
            rowCount++;
        }

        var result = new QueryResult
        {
            Data = data.ToArray(),
            Metadata = new QueryMetadata
            {
                ColumnCount = columns.Count,
                RowCount = rowCount,
                Columns = columns.ToArray()
            },
            IsSuccessful = true
        };

        // Log SQL execution results
        _logger.LogInformation("✅ SQL EXECUTION SUCCESSFUL - Rows: {RowCount}, Columns: {ColumnCount}",
            rowCount, columns.Count);

        // Log column information
        _logger.LogInformation("📊 SQL RESULT COLUMNS: {Columns}",
            string.Join(", ", columns.Select(c => $"{c.Name}({c.DataType})")));

        // Log first few rows for debugging (if any data)
        if (data.Count > 0)
        {
            var sampleRows = data.Take(3).ToList();
            _logger.LogInformation("📋 SQL RESULT SAMPLE ({SampleCount} of {TotalCount} rows): {SampleData}",
                sampleRows.Count, rowCount, System.Text.Json.JsonSerializer.Serialize(sampleRows, new System.Text.Json.JsonSerializerOptions { WriteIndented = false }));
        }
        else
        {
            _logger.LogInformation("📋 SQL RESULT: No data returned (empty result set)");
        }

        return result;
    }

    private async Task LogQueryPerformanceAsync(string sql, QueryResult result, long executionTimeMs)
    {
        try
        {
            var queryHash = ComputeQueryHash(sql);
            var performanceEntity = new QueryPerformanceEntity
            {
                QueryHash = queryHash,
                UserId = "system", // This should come from context
                ExecutionTimeMs = (int)executionTimeMs,
                RowsAffected = result.Metadata.RowCount,
                Timestamp = DateTime.UtcNow
            };

            _context.QueryPerformance.Add(performanceEntity);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log query performance metrics");
        }
    }

    private string ComputeQueryHash(string sql)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(sql.ToUpperInvariant().Trim()));
        return Convert.ToHexString(hash);
    }

    private double ExtractCostFromPlan(string planXml)
    {
        // Simple cost extraction - in a real implementation, parse the XML properly
        return 0.1;
    }

    private List<string> GenerateRecommendations(string planXml)
    {
        var recommendations = new List<string>();

        if (string.IsNullOrEmpty(planXml))
        {
            recommendations.Add("Unable to analyze query plan");
            return recommendations;
        }

        // Basic recommendations based on plan analysis
        if (planXml.Contains("TableScan"))
        {
            recommendations.Add("Consider adding indexes to avoid table scans");
        }

        if (planXml.Contains("Sort"))
        {
            recommendations.Add("Consider adding indexes to support sorting operations");
        }

        return recommendations;
    }

    // =============================================================================
    // MISSING INTERFACE METHOD IMPLEMENTATIONS
    // =============================================================================

    /// <summary>
    /// Execute SELECT query (ISqlQueryService interface)
    /// </summary>
    public async Task<QueryResult> ExecuteSelectQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await ExecuteSelectQueryAsync(sql, null, cancellationToken);
    }

    /// <summary>
    /// Validate SQL query (ISqlQueryService interface)
    /// </summary>
    public async Task<bool> ValidateSqlAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await ValidateSqlAsync(sql);
    }

    /// <summary>
    /// Execute query with proper interface signature
    /// </summary>
    public async Task<QueryResult> ExecuteQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await ExecuteSelectQueryAsync(sql, null, cancellationToken);
    }

    /// <summary>
    /// Validate query with proper interface signature
    /// </summary>
    public async Task<bool> ValidateQueryAsync(string sql, CancellationToken cancellationToken = default)
    {
        return await ValidateSqlAsync(sql);
    }

    /// <summary>
    /// Get query metadata
    /// </summary>
    public async Task<QueryMetadata> GetQueryMetadataAsync(string sql, CancellationToken cancellationToken = default)
    {
        try
        {
            // For metadata, we can do a quick validation and structure analysis
            var isValid = await ValidateQueryAsync(sql, cancellationToken);

            return new QueryMetadata
            {
                ColumnCount = 0, // Would need to analyze SQL to determine
                RowCount = 0,    // Unknown until execution
                Columns = Array.Empty<ColumnMetadata>(),
                DataSource = "BIDatabase",
                QueryTimestamp = DateTime.UtcNow,
                Error = isValid ? null : "Query validation failed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting query metadata for SQL: {Sql}", sql);
            return new QueryMetadata
            {
                ColumnCount = 0,
                RowCount = 0,
                Columns = Array.Empty<ColumnMetadata>(),
                Error = ex.Message
            };
        }
    }
}
