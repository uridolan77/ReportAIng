using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for generating SQL date filters and WHERE clauses based on time context
/// </summary>
public interface ISqlDateFilterService
{
    /// <summary>
    /// Generate SQL WHERE clause for date filtering based on time context
    /// </summary>
    /// <param name="timeRange">Time range to filter by</param>
    /// <param name="availableDateColumns">Available date columns in the query</param>
    /// <param name="strategy">Date filtering strategy</param>
    /// <returns>SQL date filter result with WHERE clause</returns>
    Task<SqlDateFilterResult> GenerateDateFilterAsync(
        TimeRange? timeRange,
        List<string> availableDateColumns,
        DateFilterStrategy strategy = DateFilterStrategy.Optimal);

    /// <summary>
    /// Generate multiple date filter options for different strategies
    /// </summary>
    /// <param name="timeRange">Time range to filter by</param>
    /// <param name="availableDateColumns">Available date columns in the query</param>
    /// <returns>List of date filter results ordered by quality</returns>
    Task<List<SqlDateFilterResult>> GenerateMultipleDateFiltersAsync(
        TimeRange? timeRange,
        List<string> availableDateColumns);

    /// <summary>
    /// Validate date column compatibility with time range
    /// </summary>
    /// <param name="dateColumns">Date columns to validate</param>
    /// <param name="timeRange">Time range for compatibility check</param>
    /// <returns>Validation result with recommendations</returns>
    Task<DateColumnValidationResult> ValidateDateColumnsAsync(
        List<string> dateColumns,
        TimeRange? timeRange);
}


