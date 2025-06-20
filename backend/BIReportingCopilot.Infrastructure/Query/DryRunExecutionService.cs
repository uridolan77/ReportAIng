using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Data.SqlClient;
using BIReportingCopilot.Core.Interfaces.Validation;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Models;
using System.Data;
using System.Diagnostics;

namespace BIReportingCopilot.Infrastructure.Query;

/// <summary>
/// Dry-run execution service for SQL validation without full execution
/// Phase 3: Enhanced SQL Validation Pipeline
/// </summary>
public class DryRunExecutionService : IDryRunExecutionService
{
    private readonly ILogger<DryRunExecutionService> _logger;
    private readonly ISqlQueryService _sqlQueryService;
    private readonly string _connectionString;

    public DryRunExecutionService(
        ILogger<DryRunExecutionService> logger,
        ISqlQueryService sqlQueryService,
        IConfiguration configuration)
    {
        _logger = logger;
        _sqlQueryService = sqlQueryService;
        _connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
    }

    /// <summary>
    /// Execute SQL in dry-run mode for validation
    /// </summary>
    public async Task<DryRunExecutionResult> ExecuteDryRunAsync(
        string sql,
        int maxRows = 1000,
        TimeSpan? maxExecutionTime = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Starting dry-run execution for SQL validation");

            var result = new DryRunExecutionResult();
            var stopwatch = Stopwatch.StartNew();

            // Step 1: Syntax validation
            var syntaxValid = await ValidateSyntaxAsync(sql, cancellationToken);
            if (!syntaxValid)
            {
                result.CanExecute = false;
                result.ExecutionErrors.Add("SQL syntax validation failed");
                return result;
            }

            result.CanExecute = true;

            // Step 2: Execution plan analysis (without execution)
            try
            {
                result.ExecutionPlan = await AnalyzeExecutionPlanAsync(sql, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Could not generate execution plan");
                result.ExecutionWarnings.Add("Execution plan analysis failed");
            }

            // Step 3: Limited execution with safeguards
            try
            {
                var limitedSql = ApplyExecutionLimits(sql, maxRows);
                var executionTimeout = maxExecutionTime ?? TimeSpan.FromSeconds(30);

                using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                cts.CancelAfter(executionTimeout);

                var queryResult = await ExecuteLimitedQueryAsync(limitedSql, cts.Token);
                
                result.ExecutedSuccessfully = true;
                result.EstimatedRowCount = queryResult.RowCount;
                result.PerformanceMetrics = await EstimatePerformanceMetricsAsync(sql, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                result.ExecutionErrors.Add("Query execution timed out during dry-run");
                result.ExecutedSuccessfully = false;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "⚠️ Dry-run execution failed");
                result.ExecutionErrors.Add($"Execution failed: {ex.Message}");
                result.ExecutedSuccessfully = false;
            }

            stopwatch.Stop();
            result.EstimatedExecutionTime = stopwatch.Elapsed;

            _logger.LogInformation("✅ Dry-run execution completed: {Success}", result.ExecutedSuccessfully);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in dry-run execution");
            return new DryRunExecutionResult
            {
                CanExecute = false,
                ExecutedSuccessfully = false,
                ExecutionErrors = new List<string> { $"Dry-run failed: {ex.Message}" }
            };
        }
    }

    /// <summary>
    /// Analyze SQL execution plan without executing
    /// </summary>
    public async Task<string> AnalyzeExecutionPlanAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("📊 Analyzing execution plan");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get estimated execution plan
            var planQuery = $"SET SHOWPLAN_XML ON; {sql}; SET SHOWPLAN_XML OFF;";
            
            using var command = new SqlCommand(planQuery, connection);
            command.CommandTimeout = 30;

            var executionPlan = await command.ExecuteScalarAsync(cancellationToken) as string;
            
            return executionPlan ?? "Execution plan not available";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Could not analyze execution plan");
            return $"Execution plan analysis failed: {ex.Message}";
        }
    }

    /// <summary>
    /// Estimate query performance metrics
    /// </summary>
    public async Task<Dictionary<string, object>> EstimatePerformanceMetricsAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        var metrics = new Dictionary<string, object>();

        try
        {
            _logger.LogDebug("📈 Estimating performance metrics");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Get query statistics
            var statsQuery = $@"
                SET STATISTICS IO ON;
                SET STATISTICS TIME ON;
                {ApplyExecutionLimits(sql, 1)}; -- Execute with minimal data
                SET STATISTICS IO OFF;
                SET STATISTICS TIME OFF;";

            using var command = new SqlCommand(statsQuery, connection);
            command.CommandTimeout = 30;

            var stopwatch = Stopwatch.StartNew();
            await command.ExecuteNonQueryAsync(cancellationToken);
            stopwatch.Stop();

            metrics["EstimatedExecutionTime"] = stopwatch.Elapsed.TotalMilliseconds;
            metrics["QueryComplexity"] = AnalyzeQueryComplexity(sql);
            metrics["TableCount"] = CountTablesInQuery(sql);
            metrics["JoinCount"] = CountJoinsInQuery(sql);
            metrics["HasAggregation"] = HasAggregation(sql);
            metrics["HasSubqueries"] = HasSubqueries(sql);

            return metrics;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Could not estimate performance metrics");
            metrics["Error"] = ex.Message;
            return metrics;
        }
    }

    /// <summary>
    /// Validate SQL syntax without execution
    /// </summary>
    public async Task<bool> ValidateSyntaxAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogDebug("✅ Validating SQL syntax");

            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);

            // Use SET PARSEONLY to validate syntax without execution
            var syntaxQuery = $"SET PARSEONLY ON; {sql}; SET PARSEONLY OFF;";
            
            using var command = new SqlCommand(syntaxQuery, connection);
            command.CommandTimeout = 10;

            await command.ExecuteNonQueryAsync(cancellationToken);
            
            return true;
        }
        catch (SqlException ex)
        {
            _logger.LogWarning("⚠️ SQL syntax validation failed: {Error}", ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Unexpected error in syntax validation");
            return false;
        }
    }

    /// <summary>
    /// Execute limited query for dry-run validation
    /// </summary>
    private async Task<(int RowCount, TimeSpan ExecutionTime)> ExecuteLimitedQueryAsync(
        string limitedSql,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        
        using var connection = new SqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        using var command = new SqlCommand(limitedSql, connection);
        command.CommandTimeout = 30;

        using var reader = await command.ExecuteReaderAsync(cancellationToken);
        
        int rowCount = 0;
        while (await reader.ReadAsync(cancellationToken))
        {
            rowCount++;
        }

        stopwatch.Stop();
        return (rowCount, stopwatch.Elapsed);
    }

    /// <summary>
    /// Apply execution limits to SQL for safe dry-run
    /// </summary>
    private string ApplyExecutionLimits(string sql, int maxRows)
    {
        // Add TOP clause if not present and it's a SELECT statement
        if (sql.TrimStart().StartsWith("SELECT", StringComparison.OrdinalIgnoreCase) &&
            !sql.Contains("TOP", StringComparison.OrdinalIgnoreCase))
        {
            var selectIndex = sql.IndexOf("SELECT", StringComparison.OrdinalIgnoreCase);
            var afterSelect = selectIndex + 6;
            
            return sql.Insert(afterSelect, $" TOP {maxRows}");
        }

        return sql;
    }

    /// <summary>
    /// Analyze query complexity for performance estimation
    /// </summary>
    private int AnalyzeQueryComplexity(string sql)
    {
        var complexity = 1; // Base complexity

        // Add complexity for various SQL features
        if (sql.Contains("JOIN", StringComparison.OrdinalIgnoreCase))
            complexity += CountJoinsInQuery(sql);

        if (sql.Contains("UNION", StringComparison.OrdinalIgnoreCase))
            complexity += 2;

        if (HasSubqueries(sql))
            complexity += 3;

        if (HasAggregation(sql))
            complexity += 2;

        if (sql.Contains("ORDER BY", StringComparison.OrdinalIgnoreCase))
            complexity += 1;

        if (sql.Contains("GROUP BY", StringComparison.OrdinalIgnoreCase))
            complexity += 2;

        return complexity;
    }

    /// <summary>
    /// Count tables referenced in the query
    /// </summary>
    private int CountTablesInQuery(string sql)
    {
        // Simple heuristic - count FROM and JOIN clauses
        var fromCount = CountOccurrences(sql, "FROM");
        var joinCount = CountJoinsInQuery(sql);
        
        return Math.Max(1, fromCount + joinCount);
    }

    /// <summary>
    /// Count JOIN operations in the query
    /// </summary>
    private int CountJoinsInQuery(string sql)
    {
        return CountOccurrences(sql, "JOIN");
    }

    /// <summary>
    /// Check if query has aggregation functions
    /// </summary>
    private bool HasAggregation(string sql)
    {
        var aggregateFunctions = new[] { "SUM(", "COUNT(", "AVG(", "MIN(", "MAX(", "GROUP BY" };
        return aggregateFunctions.Any(func => sql.Contains(func, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Check if query has subqueries
    /// </summary>
    private bool HasSubqueries(string sql)
    {
        // Simple heuristic - look for nested SELECT statements
        var selectCount = CountOccurrences(sql, "SELECT");
        return selectCount > 1;
    }

    /// <summary>
    /// Count occurrences of a keyword in SQL
    /// </summary>
    private int CountOccurrences(string sql, string keyword)
    {
        var count = 0;
        var index = 0;
        
        while ((index = sql.IndexOf(keyword, index, StringComparison.OrdinalIgnoreCase)) != -1)
        {
            count++;
            index += keyword.Length;
        }
        
        return count;
    }
}
