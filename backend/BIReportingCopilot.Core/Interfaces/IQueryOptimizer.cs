using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces;

/// <summary>
/// Interface for optimizing SQL queries
/// </summary>
public interface IQueryOptimizer
{
    /// <summary>
    /// Generates SQL query candidates based on semantic analysis
    /// </summary>
    Task<List<SqlCandidate>> GenerateCandidatesAsync(SemanticAnalysis semanticAnalysis, SchemaContext schemaContext);

    /// <summary>
    /// Optimizes a list of SQL candidates and selects the best one
    /// </summary>
    Task<OptimizedQuery> OptimizeAsync(List<SqlCandidate> candidates, UserContext userContext);

    /// <summary>
    /// Analyzes query performance
    /// </summary>
    Task<QueryPerformanceAnalysis> AnalyzePerformanceAsync(string sql);

    /// <summary>
    /// Suggests query improvements
    /// </summary>
    Task<List<QueryImprovement>> SuggestImprovementsAsync(string sql);

    /// <summary>
    /// Validates SQL syntax and semantics
    /// </summary>
    Task<QueryValidationResult> ValidateQueryAsync(string sql, SchemaMetadata schema);
}
