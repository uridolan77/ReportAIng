using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Models.BusinessContext;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for generating SQL aggregations, grouping, and metrics
/// </summary>
public interface ISqlAggregationService
{
    /// <summary>
    /// Generate SQL aggregation clauses based on business intent
    /// </summary>
    /// <param name="profile">Business context profile with intent and terms</param>
    /// <param name="availableColumns">Available columns for aggregation</param>
    /// <param name="strategy">Aggregation strategy</param>
    /// <returns>SQL aggregation result with SELECT, GROUP BY, and ORDER BY clauses</returns>
    Task<SqlAggregationResult> GenerateAggregationAsync(
        BusinessContextProfile profile,
        List<string> availableColumns,
        AggregationStrategy strategy = AggregationStrategy.Optimal);

    /// <summary>
    /// Generate multiple aggregation options for different strategies
    /// </summary>
    /// <param name="profile">Business context profile with intent and terms</param>
    /// <param name="availableColumns">Available columns for aggregation</param>
    /// <returns>List of aggregation results ordered by quality</returns>
    Task<List<SqlAggregationResult>> GenerateMultipleAggregationsAsync(
        BusinessContextProfile profile,
        List<string> availableColumns);

    /// <summary>
    /// Validate aggregation compatibility with available columns
    /// </summary>
    /// <param name="metrics">Metrics to validate</param>
    /// <param name="dimensions">Dimensions to validate</param>
    /// <param name="availableColumns">Available columns for validation</param>
    /// <returns>Validation result with compatibility information</returns>
    Task<AggregationValidationResult> ValidateAggregationAsync(
        List<SqlMetric> metrics,
        List<SqlDimension> dimensions,
        List<string> availableColumns);
}


