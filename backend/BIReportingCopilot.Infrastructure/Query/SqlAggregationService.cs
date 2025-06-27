using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BIReportingCopilot.Infrastructure.Query;

/// <summary>
/// Service for generating SQL aggregations, grouping, and metrics
/// </summary>
public class SqlAggregationService : ISqlAggregationService
{
    private readonly ILogger<SqlAggregationService> _logger;

    public SqlAggregationService(ILogger<SqlAggregationService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate SQL aggregation clauses based on business intent
    /// </summary>
    public async Task<SqlAggregationResult> GenerateAggregationAsync(
        BusinessContextProfile profile,
        List<string> availableColumns,
        AggregationStrategy strategy = AggregationStrategy.Optimal)
    {
        try
        {
            _logger.LogDebug("📊 Generating aggregation for intent: {Intent}", profile.Intent.Type);

            var metrics = ExtractMetricsFromIntent(profile, availableColumns);
            var dimensions = ExtractDimensionsFromIntent(profile, availableColumns);
            
            if (!metrics.Any() && !dimensions.Any())
            {
                return new SqlAggregationResult
                {
                    Success = true,
                    SelectClause = "SELECT *",
                    GroupByClause = string.Empty,
                    OrderByClause = string.Empty,
                    Strategy = strategy
                };
            }

            var selectClause = GenerateSelectClause(metrics, dimensions, strategy);
            var groupByClause = GenerateGroupByClause(dimensions, strategy);
            var orderByClause = GenerateOrderByClause(metrics, dimensions, profile.Intent, strategy);

            var result = new SqlAggregationResult
            {
                Success = true,
                SelectClause = selectClause,
                GroupByClause = groupByClause,
                OrderByClause = orderByClause,
                Metrics = metrics,
                Dimensions = dimensions,
                Strategy = strategy,
                Metadata = new Dictionary<string, object>
                {
                    ["MetricCount"] = metrics.Count,
                    ["DimensionCount"] = dimensions.Count,
                    ["IntentType"] = profile.Intent.Type.ToString(),
                    ["BusinessTerms"] = profile.BusinessTerms.Count
                }
            };

            _logger.LogDebug("✅ Generated aggregation with {MetricCount} metrics and {DimensionCount} dimensions", 
                metrics.Count, dimensions.Count);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating aggregation");
            return new SqlAggregationResult
            {
                Success = false,
                Error = ex.Message,
                SelectClause = string.Empty,
                GroupByClause = string.Empty,
                OrderByClause = string.Empty
            };
        }
    }

    /// <summary>
    /// Generate multiple aggregation options for different strategies
    /// </summary>
    public async Task<List<SqlAggregationResult>> GenerateMultipleAggregationsAsync(
        BusinessContextProfile profile,
        List<string> availableColumns)
    {
        var results = new List<SqlAggregationResult>();
        var strategies = Enum.GetValues<AggregationStrategy>();

        foreach (var strategy in strategies)
        {
            var result = await GenerateAggregationAsync(profile, availableColumns, strategy);
            if (result.Success)
            {
                results.Add(result);
            }
        }

        return results.OrderByDescending(r => CalculateAggregationQuality(r)).ToList();
    }

    /// <summary>
    /// Validate aggregation compatibility with available columns
    /// </summary>
    public async Task<AggregationValidationResult> ValidateAggregationAsync(
        List<SqlMetric> metrics,
        List<SqlDimension> dimensions,
        List<string> availableColumns)
    {
        try
        {
            var validMetrics = new List<SqlMetric>();
            var invalidMetrics = new List<SqlMetric>();
            var validDimensions = new List<SqlDimension>();
            var invalidDimensions = new List<SqlDimension>();
            var warnings = new List<string>();

            // Validate metrics
            foreach (var metric in metrics)
            {
                if (availableColumns.Contains(metric.ColumnName, StringComparer.OrdinalIgnoreCase))
                {
                    validMetrics.Add(metric);
                }
                else
                {
                    invalidMetrics.Add(metric);
                    warnings.Add($"Metric column '{metric.ColumnName}' not found in available columns");
                }
            }

            // Validate dimensions
            foreach (var dimension in dimensions)
            {
                if (availableColumns.Contains(dimension.ColumnName, StringComparer.OrdinalIgnoreCase))
                {
                    validDimensions.Add(dimension);
                }
                else
                {
                    invalidDimensions.Add(dimension);
                    warnings.Add($"Dimension column '{dimension.ColumnName}' not found in available columns");
                }
            }

            return new AggregationValidationResult
            {
                IsValid = validMetrics.Any() || validDimensions.Any(),
                ValidMetrics = validMetrics,
                InvalidMetrics = invalidMetrics,
                ValidDimensions = validDimensions,
                InvalidDimensions = invalidDimensions,
                Warnings = warnings
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating aggregation");
            return new AggregationValidationResult
            {
                IsValid = false,
                Warnings = new List<string> { $"Validation error: {ex.Message}" }
            };
        }
    }

    #region Private Helper Methods

    private List<SqlMetric> ExtractMetricsFromIntent(BusinessContextProfile profile, List<string> availableColumns)
    {
        var metrics = new List<SqlMetric>();

        // Extract metrics based on business terms and intent
        var businessTerms = profile.BusinessTerms.Select(bt => bt.ToLower()).ToList();
        var intentType = profile.Intent.Type;

        // Common metric patterns
        var metricPatterns = new Dictionary<string, (string Function, string[] Keywords)>
        {
            ["SUM"] = ("SUM", new[] { "total", "sum", "amount", "revenue", "deposit", "bet", "win" }),
            ["COUNT"] = ("COUNT", new[] { "count", "number", "how many", "depositor", "player" }),
            ["AVG"] = ("AVG", new[] { "average", "avg", "mean" }),
            ["MAX"] = ("MAX", new[] { "maximum", "max", "highest", "top" }),
            ["MIN"] = ("MIN", new[] { "minimum", "min", "lowest", "bottom" })
        };

        foreach (var (function, keywords) in metricPatterns.Values)
        {
            if (businessTerms.Any(term => keywords.Any(keyword => term.Contains(keyword))))
            {
                var metricColumns = FindMetricColumns(availableColumns, keywords);
                foreach (var column in metricColumns)
                {
                    metrics.Add(new SqlMetric
                    {
                        ColumnName = column,
                        Function = function,
                        Alias = GenerateMetricAlias(function, column),
                        Priority = CalculateMetricPriority(function, column, businessTerms)
                    });
                }
            }
        }

        // If no specific metrics found, add default based on intent
        if (!metrics.Any() && intentType == IntentType.Analytical)
        {
            var defaultMetricColumn = FindDefaultMetricColumn(availableColumns);
            if (!string.IsNullOrEmpty(defaultMetricColumn))
            {
                metrics.Add(new SqlMetric
                {
                    ColumnName = defaultMetricColumn,
                    Function = "SUM",
                    Alias = GenerateMetricAlias("SUM", defaultMetricColumn),
                    Priority = 0.5
                });
            }
        }

        return metrics.OrderByDescending(m => m.Priority).Take(5).ToList();
    }

    private List<SqlDimension> ExtractDimensionsFromIntent(BusinessContextProfile profile, List<string> availableColumns)
    {
        var dimensions = new List<SqlDimension>();
        var businessTerms = profile.BusinessTerms.Select(bt => bt.ToLower()).ToList();

        // Common dimension patterns
        var dimensionKeywords = new[] { "country", "currency", "date", "game", "provider", "label", "type", "category" };

        foreach (var keyword in dimensionKeywords)
        {
            if (businessTerms.Any(term => term.Contains(keyword)))
            {
                var dimensionColumns = FindDimensionColumns(availableColumns, keyword);
                foreach (var column in dimensionColumns)
                {
                    dimensions.Add(new SqlDimension
                    {
                        ColumnName = column,
                        Alias = GenerateDimensionAlias(column),
                        Priority = CalculateDimensionPriority(column, businessTerms)
                    });
                }
            }
        }

        // Add time dimension if time context exists
        if (profile.TimeContext != null)
        {
            var dateColumns = availableColumns.Where(col => 
                col.ToLower().Contains("date") || col.ToLower().Contains("time")).ToList();
            
            foreach (var dateCol in dateColumns.Take(1)) // Take only the first date column
            {
                dimensions.Add(new SqlDimension
                {
                    ColumnName = dateCol,
                    Alias = GenerateDimensionAlias(dateCol),
                    Priority = 0.9 // High priority for time dimensions
                });
            }
        }

        return dimensions.OrderByDescending(d => d.Priority).Take(3).ToList();
    }

    private string GenerateSelectClause(List<SqlMetric> metrics, List<SqlDimension> dimensions, AggregationStrategy strategy)
    {
        var selectItems = new List<string>();

        // Add dimensions first
        foreach (var dimension in dimensions)
        {
            selectItems.Add($"{dimension.ColumnName} AS {dimension.Alias}");
        }

        // Add metrics
        foreach (var metric in metrics)
        {
            selectItems.Add($"{metric.Function}({metric.ColumnName}) AS {metric.Alias}");
        }

        // If no specific items, return basic select
        if (!selectItems.Any())
        {
            return "SELECT TOP 100 *";
        }

        var selectClause = strategy == AggregationStrategy.Performance 
            ? "SELECT TOP 100 " + string.Join(", ", selectItems)
            : "SELECT " + string.Join(", ", selectItems);

        return selectClause;
    }

    private string GenerateGroupByClause(List<SqlDimension> dimensions, AggregationStrategy strategy)
    {
        if (!dimensions.Any()) return string.Empty;

        var groupByItems = dimensions.Select(d => d.ColumnName).ToList();
        return "GROUP BY " + string.Join(", ", groupByItems);
    }

    private string GenerateOrderByClause(List<SqlMetric> metrics, List<SqlDimension> dimensions,
        BIReportingCopilot.Core.Models.BusinessContext.QueryIntent intent, AggregationStrategy strategy)
    {
        var orderByItems = new List<string>();

        // Determine ordering based on intent
        if (intent.Type == IntentType.Analytical && metrics.Any())
        {
            // Order by the first metric descending for analytics
            var primaryMetric = metrics.First();
            orderByItems.Add($"{primaryMetric.Alias} DESC");
        }
        else if (dimensions.Any(d => d.ColumnName.ToLower().Contains("date")))
        {
            // Order by date if available
            var dateColumn = dimensions.First(d => d.ColumnName.ToLower().Contains("date"));
            orderByItems.Add($"{dateColumn.Alias} DESC");
        }
        else if (dimensions.Any())
        {
            // Order by first dimension
            orderByItems.Add($"{dimensions.First().Alias}");
        }

        return orderByItems.Any() ? "ORDER BY " + string.Join(", ", orderByItems) : string.Empty;
    }

    private List<string> FindMetricColumns(List<string> availableColumns, string[] keywords)
    {
        return availableColumns.Where(col =>
            keywords.Any(keyword => col.ToLower().Contains(keyword)) ||
            col.ToLower().Contains("amount") ||
            col.ToLower().Contains("value") ||
            col.ToLower().Contains("count") ||
            col.ToLower().Contains("sum")).ToList();
    }

    private List<string> FindDimensionColumns(List<string> availableColumns, string keyword)
    {
        return availableColumns.Where(col =>
            col.ToLower().Contains(keyword.ToLower())).ToList();
    }

    private string? FindDefaultMetricColumn(List<string> availableColumns)
    {
        var defaultMetricKeywords = new[] { "amount", "value", "total", "sum", "count" };
        
        foreach (var keyword in defaultMetricKeywords)
        {
            var column = availableColumns.FirstOrDefault(col => 
                col.ToLower().Contains(keyword));
            if (column != null) return column;
        }

        return null;
    }

    private string GenerateMetricAlias(string function, string columnName)
    {
        var cleanColumnName = columnName.Replace("_", "").Replace(" ", "");
        return $"{function}{cleanColumnName}";
    }

    private string GenerateDimensionAlias(string columnName)
    {
        return columnName.Replace("_", "").Replace(" ", "");
    }

    private double CalculateMetricPriority(string function, string columnName, List<string> businessTerms)
    {
        var priority = 0.5;
        
        // Boost priority if column name matches business terms
        if (businessTerms.Any(term => columnName.ToLower().Contains(term)))
            priority += 0.3;
            
        // Boost priority for common business metrics
        if (columnName.ToLower().Contains("amount") || columnName.ToLower().Contains("revenue"))
            priority += 0.2;
            
        return Math.Min(priority, 1.0);
    }

    private double CalculateDimensionPriority(string columnName, List<string> businessTerms)
    {
        var priority = 0.5;
        
        // Boost priority if column name matches business terms
        if (businessTerms.Any(term => columnName.ToLower().Contains(term)))
            priority += 0.3;
            
        // Boost priority for common dimensions
        if (columnName.ToLower().Contains("country") || columnName.ToLower().Contains("date"))
            priority += 0.2;
            
        return Math.Min(priority, 1.0);
    }

    private double CalculateAggregationQuality(SqlAggregationResult result)
    {
        var quality = 1.0;

        // Prefer results with both metrics and dimensions
        if (result.Metrics.Any() && result.Dimensions.Any()) quality += 0.3;
        
        // Prefer optimal strategy
        if (result.Strategy == AggregationStrategy.Optimal) quality += 0.2;
        
        // Prefer results with reasonable number of items
        if (result.Metrics.Count + result.Dimensions.Count <= 8) quality += 0.1;

        return quality;
    }

    #endregion
}
