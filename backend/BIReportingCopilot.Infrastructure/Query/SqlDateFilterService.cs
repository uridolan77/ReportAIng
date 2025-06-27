using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;
using Microsoft.Extensions.Logging;
using System.Text;

namespace BIReportingCopilot.Infrastructure.Query;

/// <summary>
/// Service for generating SQL date filters and WHERE clauses based on time context
/// </summary>
public class SqlDateFilterService : ISqlDateFilterService
{
    private readonly ILogger<SqlDateFilterService> _logger;

    public SqlDateFilterService(ILogger<SqlDateFilterService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate SQL WHERE clause for date filtering based on time context
    /// </summary>
    public async Task<SqlDateFilterResult> GenerateDateFilterAsync(
        TimeRange? timeRange,
        List<string> availableDateColumns,
        DateFilterStrategy strategy = DateFilterStrategy.Optimal)
    {
        try
        {
            _logger.LogDebug("🗓️ Generating date filter for time range: {TimeRange}", 
                timeRange?.RelativeExpression ?? "None");

            if (timeRange == null)
            {
                return new SqlDateFilterResult
                {
                    Success = true,
                    WhereClause = string.Empty,
                    DateColumns = new List<string>(),
                    Strategy = strategy
                };
            }

            // Determine the best date column to use
            var selectedDateColumn = SelectOptimalDateColumn(availableDateColumns, timeRange);
            
            if (string.IsNullOrEmpty(selectedDateColumn))
            {
                _logger.LogWarning("⚠️ No suitable date column found for filtering");
                return new SqlDateFilterResult
                {
                    Success = false,
                    Error = "No suitable date column found for time filtering",
                    WhereClause = string.Empty,
                    DateColumns = new List<string>()
                };
            }

            // Generate the WHERE clause based on strategy
            var whereClause = strategy switch
            {
                DateFilterStrategy.Optimal => GenerateOptimalDateFilter(timeRange, selectedDateColumn),
                DateFilterStrategy.Inclusive => GenerateInclusiveDateFilter(timeRange, selectedDateColumn),
                DateFilterStrategy.Exclusive => GenerateExclusiveDateFilter(timeRange, selectedDateColumn),
                DateFilterStrategy.Performance => GeneratePerformanceDateFilter(timeRange, selectedDateColumn),
                _ => GenerateOptimalDateFilter(timeRange, selectedDateColumn)
            };

            var result = new SqlDateFilterResult
            {
                Success = true,
                WhereClause = whereClause,
                DateColumns = new List<string> { selectedDateColumn },
                Strategy = strategy,
                TimeRange = timeRange,
                Metadata = new Dictionary<string, object>
                {
                    ["SelectedDateColumn"] = selectedDateColumn,
                    ["AvailableColumns"] = availableDateColumns.Count,
                    ["Granularity"] = timeRange.Granularity.ToString(),
                    ["RelativeExpression"] = timeRange.RelativeExpression ?? "None"
                }
            };

            _logger.LogDebug("✅ Generated date filter: {WhereClause}", whereClause);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error generating date filter");
            return new SqlDateFilterResult
            {
                Success = false,
                Error = ex.Message,
                WhereClause = string.Empty,
                DateColumns = new List<string>()
            };
        }
    }

    /// <summary>
    /// Generate multiple date filter options for different strategies
    /// </summary>
    public async Task<List<SqlDateFilterResult>> GenerateMultipleDateFiltersAsync(
        TimeRange? timeRange,
        List<string> availableDateColumns)
    {
        var results = new List<SqlDateFilterResult>();
        var strategies = Enum.GetValues<DateFilterStrategy>();

        foreach (var strategy in strategies)
        {
            var result = await GenerateDateFilterAsync(timeRange, availableDateColumns, strategy);
            if (result.Success)
            {
                results.Add(result);
            }
        }

        return results.OrderByDescending(r => CalculateFilterQuality(r)).ToList();
    }

    /// <summary>
    /// Validate date column compatibility with time range
    /// </summary>
    public async Task<DateColumnValidationResult> ValidateDateColumnsAsync(
        List<string> dateColumns,
        TimeRange? timeRange)
    {
        try
        {
            var validColumns = new List<string>();
            var invalidColumns = new List<string>();
            var warnings = new List<string>();

            foreach (var column in dateColumns)
            {
                if (IsValidDateColumn(column))
                {
                    validColumns.Add(column);
                }
                else
                {
                    invalidColumns.Add(column);
                    warnings.Add($"Column '{column}' may not be a valid date column");
                }
            }

            // Check time range compatibility
            if (timeRange != null && !validColumns.Any())
            {
                warnings.Add("No valid date columns found for time range filtering");
            }

            return new DateColumnValidationResult
            {
                IsValid = validColumns.Any(),
                ValidColumns = validColumns,
                InvalidColumns = invalidColumns,
                Warnings = warnings,
                RecommendedColumn = SelectOptimalDateColumn(validColumns, timeRange)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error validating date columns");
            return new DateColumnValidationResult
            {
                IsValid = false,
                Warnings = new List<string> { $"Validation error: {ex.Message}" }
            };
        }
    }

    #region Private Helper Methods

    private string SelectOptimalDateColumn(List<string> availableColumns, TimeRange? timeRange)
    {
        if (!availableColumns.Any()) return string.Empty;

        // Priority order for date columns
        var priorityColumns = new[]
        {
            "Date", "GameDate", "TransactionDate", "CreatedDate", "UpdatedDate",
            "ActionDate", "ProcessDate", "EventDate", "Timestamp", "DateTime"
        };

        // First, try to find exact matches
        foreach (var priority in priorityColumns)
        {
            var match = availableColumns.FirstOrDefault(col => 
                col.Equals(priority, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;
        }

        // Then try partial matches
        foreach (var priority in priorityColumns)
        {
            var match = availableColumns.FirstOrDefault(col => 
                col.Contains(priority, StringComparison.OrdinalIgnoreCase));
            if (match != null) return match;
        }

        // Finally, return the first column that looks like a date
        var dateColumn = availableColumns.FirstOrDefault(col => 
            col.ToLower().Contains("date") || 
            col.ToLower().Contains("time") ||
            col.ToLower().Contains("created") ||
            col.ToLower().Contains("updated"));

        return dateColumn ?? availableColumns.First();
    }

    private bool IsValidDateColumn(string columnName)
    {
        var lowerColumn = columnName.ToLower();
        var dateIndicators = new[] { "date", "time", "created", "updated", "modified", "timestamp" };
        
        return dateIndicators.Any(indicator => lowerColumn.Contains(indicator));
    }

    private string GenerateOptimalDateFilter(TimeRange timeRange, string dateColumn)
    {
        var startDate = timeRange.StartDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        var endDate = timeRange.EndDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");

        // Use different formats based on granularity
        return timeRange.Granularity switch
        {
            TimeGranularity.Day => $"{dateColumn} >= '{startDate}' AND {dateColumn} < '{endDate}'",
            TimeGranularity.Week => $"{dateColumn} >= '{startDate}' AND {dateColumn} <= '{endDate}'",
            TimeGranularity.Month => $"{dateColumn} >= '{startDate}' AND {dateColumn} <= '{endDate}'",
            TimeGranularity.Quarter => $"{dateColumn} >= '{startDate}' AND {dateColumn} <= '{endDate}'",
            TimeGranularity.Year => $"{dateColumn} >= '{startDate}' AND {dateColumn} <= '{endDate}'",
            _ => $"{dateColumn} >= '{startDate}' AND {dateColumn} <= '{endDate}'"
        };
    }

    private string GenerateInclusiveDateFilter(TimeRange timeRange, string dateColumn)
    {
        var startDate = timeRange.StartDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        var endDate = timeRange.EndDate?.ToString("yyyy-MM-dd 23:59:59") ?? DateTime.Today.ToString("yyyy-MM-dd 23:59:59");

        return $"{dateColumn} >= '{startDate}' AND {dateColumn} <= '{endDate}'";
    }

    private string GenerateExclusiveDateFilter(TimeRange timeRange, string dateColumn)
    {
        var startDate = timeRange.StartDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        var endDate = timeRange.EndDate?.AddDays(1).ToString("yyyy-MM-dd") ?? DateTime.Today.AddDays(1).ToString("yyyy-MM-dd");

        return $"{dateColumn} >= '{startDate}' AND {dateColumn} < '{endDate}'";
    }

    private string GeneratePerformanceDateFilter(TimeRange timeRange, string dateColumn)
    {
        // Use CAST for better index usage
        var startDate = timeRange.StartDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");
        var endDate = timeRange.EndDate?.ToString("yyyy-MM-dd") ?? DateTime.Today.ToString("yyyy-MM-dd");

        return $"CAST({dateColumn} AS DATE) >= '{startDate}' AND CAST({dateColumn} AS DATE) <= '{endDate}'";
    }

    private double CalculateFilterQuality(SqlDateFilterResult result)
    {
        var quality = 1.0;

        // Prefer shorter, more efficient WHERE clauses
        if (result.WhereClause.Length < 100) quality += 0.2;
        
        // Prefer strategies that use indexes well
        if (result.Strategy == DateFilterStrategy.Performance) quality += 0.3;
        if (result.Strategy == DateFilterStrategy.Optimal) quality += 0.2;

        // Prefer filters with good date columns
        if (result.DateColumns.Any(col => col.ToLower().Contains("date"))) quality += 0.1;

        return quality;
    }

    #endregion
}
