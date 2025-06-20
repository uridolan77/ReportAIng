using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Validation;

/// <summary>
/// Interface for enhanced semantic SQL validation with self-correction capabilities
/// Phase 3: Enhanced SQL Validation Pipeline
/// </summary>
public interface IEnhancedSemanticSqlValidator
{
    /// <summary>
    /// Comprehensive SQL validation with semantic analysis and self-correction
    /// </summary>
    /// <param name="sql">The SQL query to validate</param>
    /// <param name="originalQuery">The original natural language query</param>
    /// <param name="context">Additional context for validation</param>
    /// <param name="userId">User identifier for access control</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced semantic validation result</returns>
    Task<EnhancedSemanticValidationResult> ValidateWithSemanticAnalysisAsync(
        string sql,
        string originalQuery,
        string? context = null,
        string? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate SQL with specific validation level
    /// </summary>
    /// <param name="request">Enhanced validation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Enhanced validation response</returns>
    Task<EnhancedValidationResponse> ValidateWithLevelAsync(
        EnhancedValidationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Perform dry-run execution of SQL for validation
    /// </summary>
    /// <param name="request">Dry-run execution request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dry-run execution result</returns>
    Task<DryRunExecutionResult> PerformDryRunAsync(
        DryRunExecutionRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempt self-correction of SQL based on validation issues
    /// </summary>
    /// <param name="sql">Original SQL query</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="validationIssues">Validation issues to address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Self-correction attempt result</returns>
    Task<SelfCorrectionAttempt> AttemptSelfCorrectionAsync(
        string sql,
        string originalQuery,
        List<string> validationIssues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate business logic compliance
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business logic validation result</returns>
    Task<BusinessLogicValidationResult> ValidateBusinessLogicAsync(
        string sql,
        string originalQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate semantic alignment between SQL and business intent
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Semantic validation result</returns>
    Task<SemanticValidationResult> ValidateSemanticAlignmentAsync(
        string sql,
        string originalQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate schema compliance and accessibility
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Schema compliance result</returns>
    Task<SchemaComplianceResult> ValidateSchemaComplianceAsync(
        string sql,
        string originalQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extract and analyze SQL elements for validation
    /// </summary>
    /// <param name="sql">SQL query to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>SQL elements extraction result</returns>
    Task<SqlElementsResult> ExtractSqlElementsAsync(
        string sql,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get validation metrics for monitoring and optimization
    /// </summary>
    /// <param name="timeRange">Time range for metrics</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation metrics</returns>
    Task<ValidationMetrics> GetValidationMetricsAsync(
        TimeSpan timeRange,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Configure self-correction behavior
    /// </summary>
    /// <param name="configuration">Self-correction configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    Task<bool> ConfigureSelfCorrectionAsync(
        SelfCorrectionConfiguration configuration,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for dry-run SQL execution service
/// </summary>
public interface IDryRunExecutionService
{
    /// <summary>
    /// Execute SQL in dry-run mode for validation
    /// </summary>
    /// <param name="sql">SQL query to execute</param>
    /// <param name="maxRows">Maximum rows to analyze</param>
    /// <param name="maxExecutionTime">Maximum execution time</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dry-run execution result</returns>
    Task<DryRunExecutionResult> ExecuteDryRunAsync(
        string sql,
        int maxRows = 1000,
        TimeSpan? maxExecutionTime = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Analyze SQL execution plan without executing
    /// </summary>
    /// <param name="sql">SQL query to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Execution plan analysis</returns>
    Task<string> AnalyzeExecutionPlanAsync(
        string sql,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Estimate query performance metrics
    /// </summary>
    /// <param name="sql">SQL query to analyze</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Performance metrics</returns>
    Task<Dictionary<string, object>> EstimatePerformanceMetricsAsync(
        string sql,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate SQL syntax without execution
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Syntax validation result</returns>
    Task<bool> ValidateSyntaxAsync(
        string sql,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for SQL self-correction service
/// </summary>
public interface ISqlSelfCorrectionService
{
    /// <summary>
    /// Attempt to correct SQL based on validation issues
    /// </summary>
    /// <param name="sql">Original SQL query</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="validationIssues">Issues to address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Correction attempt result</returns>
    Task<SelfCorrectionAttempt> CorrectSqlAsync(
        string sql,
        string originalQuery,
        List<string> validationIssues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate correction suggestions without applying them
    /// </summary>
    /// <param name="sql">SQL query to analyze</param>
    /// <param name="validationIssues">Issues to address</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of correction suggestions</returns>
    Task<List<string>> GenerateCorrectionSuggestionsAsync(
        string sql,
        List<string> validationIssues,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Learn from successful corrections for future improvements
    /// </summary>
    /// <param name="correctionAttempt">Successful correction attempt</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success indicator</returns>
    Task<bool> LearnFromCorrectionAsync(
        SelfCorrectionAttempt correctionAttempt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get correction patterns and statistics
    /// </summary>
    /// <param name="timeRange">Time range for analysis</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Correction patterns and statistics</returns>
    Task<Dictionary<string, object>> GetCorrectionPatternsAsync(
        TimeSpan timeRange,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for business logic validation service
/// </summary>
public interface IBusinessLogicValidationService
{
    /// <summary>
    /// Validate business rules compliance
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="originalQuery">Original natural language query</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Business logic validation result</returns>
    Task<BusinessLogicValidationResult> ValidateBusinessRulesAsync(
        string sql,
        string originalQuery,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Check data access permissions
    /// </summary>
    /// <param name="sql">SQL query to check</param>
    /// <param name="userId">User identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Access validation result</returns>
    Task<AccessValidationResult> ValidateDataAccessAsync(
        string sql,
        string? userId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate sensitive data handling
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Sensitivity validation result</returns>
    Task<SensitivityValidationResult> ValidateSensitiveDataAsync(
        string sql,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate aggregation logic and grouping
    /// </summary>
    /// <param name="sql">SQL query to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Aggregation validation result</returns>
    Task<AggregationValidationResult> ValidateAggregationLogicAsync(
        string sql,
        CancellationToken cancellationToken = default);
}
