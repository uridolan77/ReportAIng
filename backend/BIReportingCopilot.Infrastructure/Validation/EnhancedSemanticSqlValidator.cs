using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Validation;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Security;
using System.Text.Json;
using System.Text;

namespace BIReportingCopilot.Infrastructure.Validation;

/// <summary>
/// Enhanced SQL validator that combines traditional security validation with semantic validation
/// Phase 3: Enhanced SQL Validation Pipeline
/// </summary>
public class EnhancedSemanticSqlValidator : IEnhancedSemanticSqlValidator
{
    private readonly ILogger<EnhancedSemanticSqlValidator> _logger;
    private readonly IEnhancedSqlQueryValidator _securityValidator;
    private readonly IEnhancedSemanticLayerService _semanticLayerService;
    private readonly IAIService _aiService;
    private readonly IPromptService _promptService;
    private readonly ISemanticCacheService _cacheService;

    public EnhancedSemanticSqlValidator(
        ILogger<EnhancedSemanticSqlValidator> logger,
        IEnhancedSqlQueryValidator securityValidator,
        IEnhancedSemanticLayerService semanticLayerService,
        IAIService aiService,
        IPromptService promptService,
        ISemanticCacheService cacheService)
    {
        _logger = logger;
        _securityValidator = securityValidator;
        _semanticLayerService = semanticLayerService;
        _aiService = aiService;
        _promptService = promptService;
        _cacheService = cacheService;
    }

    /// <summary>
    /// Comprehensive SQL validation with semantic analysis and self-correction
    /// </summary>
    public async Task<EnhancedSemanticValidationResult> ValidateWithSemanticAnalysisAsync(
        string sql,
        string originalQuery,
        string? context = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("🔍 Starting enhanced semantic SQL validation");

            // Step 1: Traditional security validation
            var securityResult = await _securityValidator.ValidateQueryAsync(sql, context, userId);

            // Step 2: Semantic validation
            var semanticResult = await PerformSemanticValidationAsync(sql, originalQuery, cancellationToken);

            // Step 3: Schema compliance validation
            var schemaResult = await ValidateSchemaComplianceAsync(sql, originalQuery, cancellationToken);

            // Step 4: Business logic validation
            var businessResult = await ValidateBusinessLogicAsync(sql, originalQuery, cancellationToken);

            // Step 5: Combine results and determine overall validity
            var combinedResult = CombineValidationResults(securityResult, semanticResult, schemaResult, businessResult);

            // Step 6: Self-correction if needed
            if (!combinedResult.IsValid && combinedResult.CanSelfCorrect)
            {
                _logger.LogInformation("🔧 Attempting self-correction for SQL validation issues");
                var correctedResult = await AttemptSelfCorrectionAsync(sql, originalQuery, combinedResult, cancellationToken);
                if (correctedResult != null)
                {
                    combinedResult = correctedResult;
                }
            }

            _logger.LogInformation("✅ Enhanced semantic validation completed: {IsValid}", combinedResult.IsValid);
            return combinedResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in enhanced semantic SQL validation");
            return CreateErrorResult(sql, originalQuery, ex.Message);
        }
    }

    /// <summary>
    /// Perform semantic validation of SQL against business intent
    /// </summary>
    private async Task<SemanticValidationResult> PerformSemanticValidationAsync(
        string sql, 
        string originalQuery, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("🧠 Performing semantic validation");

        try
        {
            // Get enhanced schema context for the original query
            var schemaContext = await _semanticLayerService.GetEnhancedSchemaAsync(
                originalQuery, 0.7, 10, cancellationToken);

            // Analyze SQL semantic alignment with business intent
            var semanticAlignment = await AnalyzeSqlSemanticAlignment(sql, originalQuery, schemaContext, cancellationToken);

            // Check for semantic inconsistencies
            var inconsistencies = await DetectSemanticInconsistencies(sql, schemaContext, cancellationToken);

            // Validate business term usage
            var termUsage = await ValidateBusinessTermUsage(sql, schemaContext, cancellationToken);

            return new SemanticValidationResult
            {
                IsValid = semanticAlignment.Score >= 0.7 && inconsistencies.Count == 0,
                AlignmentScore = semanticAlignment.Score,
                AlignmentReason = semanticAlignment.Reason,
                SemanticInconsistencies = inconsistencies,
                BusinessTermValidation = termUsage,
                ConfidenceScore = CalculateSemanticConfidence(semanticAlignment, inconsistencies, termUsage)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error in semantic validation, using fallback");
            return new SemanticValidationResult
            {
                IsValid = true, // Fallback to allow execution
                AlignmentScore = 0.5,
                AlignmentReason = "Semantic validation failed, using fallback",
                ConfidenceScore = 0.3
            };
        }
    }

    /// <summary>
    /// Validate SQL compliance with schema metadata and business rules
    /// </summary>
    private async Task<SchemaComplianceResult> ValidateSchemaComplianceAsync(
        string sql, 
        string originalQuery, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("📋 Validating schema compliance");

        try
        {
            // Extract tables and columns from SQL
            var sqlElements = await ExtractSqlElementsAsync(sql, cancellationToken);

            // Get relevant schema information
            var schemaContext = await _semanticLayerService.GetEnhancedSchemaAsync(
                originalQuery, 0.6, 15, cancellationToken);

            // Validate table existence and accessibility
            var tableValidation = ValidateTableAccess(sqlElements.Tables, schemaContext);

            // Validate column existence and data types
            var columnValidation = ValidateColumnUsage(sqlElements.Columns, schemaContext);

            // Validate join relationships
            var joinValidation = ValidateJoinRelationships(sqlElements.Joins, schemaContext);

            // Check for missing required business context
            var contextValidation = ValidateBusinessContext(sql, schemaContext);

            return new SchemaComplianceResult
            {
                IsCompliant = tableValidation.IsValid && columnValidation.IsValid && joinValidation.IsValid,
                TableValidation = tableValidation,
                ColumnValidation = columnValidation,
                JoinValidation = joinValidation,
                ContextValidation = contextValidation,
                ComplianceScore = CalculateComplianceScore(tableValidation, columnValidation, joinValidation, contextValidation)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error in schema compliance validation");
            return new SchemaComplianceResult
            {
                IsCompliant = true, // Fallback to allow execution
                ComplianceScore = 0.5
            };
        }
    }

    /// <summary>
    /// Validate business logic and rules compliance
    /// </summary>
    private async Task<BusinessLogicValidationResult> ValidateBusinessLogicAsync(
        string sql, 
        string originalQuery, 
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("💼 Validating business logic compliance");

        try
        {
            // Check for business rule violations
            var ruleViolations = await DetectBusinessRuleViolations(sql, originalQuery, cancellationToken);

            // Validate data access permissions
            var accessValidation = await ValidateDataAccessPermissions(sql, cancellationToken);

            // Check for sensitive data exposure
            var sensitivityValidation = await ValidateSensitiveDataHandling(sql, cancellationToken);

            // Validate aggregation logic
            var aggregationValidation = ValidateAggregationLogic(sql);

            return new BusinessLogicValidationResult
            {
                IsValid = ruleViolations.Count == 0 && accessValidation.IsValid && sensitivityValidation.IsValid,
                RuleViolations = ruleViolations,
                AccessValidation = accessValidation,
                SensitivityValidation = sensitivityValidation,
                AggregationValidation = aggregationValidation,
                BusinessLogicScore = CalculateBusinessLogicScore(ruleViolations, accessValidation, sensitivityValidation, aggregationValidation)
            };
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "⚠️ Error in business logic validation");
            return new BusinessLogicValidationResult
            {
                IsValid = true, // Fallback to allow execution
                BusinessLogicScore = 0.5
            };
        }
    }

    /// <summary>
    /// Attempt self-correction using LLM feedback loops
    /// </summary>
    private async Task<EnhancedSemanticValidationResult?> AttemptSelfCorrectionAsync(
        string sql,
        string originalQuery,
        EnhancedSemanticValidationResult validationResult,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("🔧 Attempting SQL self-correction");

        try
        {
            // Build correction prompt with validation issues
            var correctionPrompt = await BuildCorrectionPromptAsync(sql, originalQuery, validationResult, cancellationToken);

            // Generate corrected SQL using AI
            var correctedSql = await _aiService.GenerateSQLAsync(correctionPrompt, cancellationToken);

            if (string.IsNullOrWhiteSpace(correctedSql) || correctedSql.Equals(sql, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("⚠️ Self-correction did not produce a different SQL");
                return null;
            }

            // Validate the corrected SQL
            var correctedValidation = await ValidateWithSemanticAnalysisAsync(
                correctedSql, originalQuery, null, null, cancellationToken);

            // Check if correction improved the validation
            if (correctedValidation.OverallScore > validationResult.OverallScore)
            {
                _logger.LogInformation("✅ Self-correction improved SQL validation score from {OldScore:F2} to {NewScore:F2}",
                    validationResult.OverallScore, correctedValidation.OverallScore);

                correctedValidation.IsSelfCorrected = true;
                correctedValidation.OriginalSql = sql;
                correctedValidation.CorrectionReason = "Improved validation score through self-correction";

                return correctedValidation;
            }
            else
            {
                _logger.LogWarning("⚠️ Self-correction did not improve validation score");
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error in SQL self-correction");
            return null;
        }
    }

    /// <summary>
    /// Build correction prompt for LLM self-correction
    /// </summary>
    private async Task<string> BuildCorrectionPromptAsync(
        string sql,
        string originalQuery,
        EnhancedSemanticValidationResult validationResult,
        CancellationToken cancellationToken)
    {
        var issues = new List<string>();

        if (validationResult.SemanticValidation != null && !validationResult.SemanticValidation.IsValid)
        {
            issues.Add($"Semantic alignment issue: {validationResult.SemanticValidation.AlignmentReason}");
        }

        if (validationResult.SchemaCompliance != null && !validationResult.SchemaCompliance.IsCompliant)
        {
            issues.Add("Schema compliance issues detected");
        }

        if (validationResult.BusinessLogicValidation != null && !validationResult.BusinessLogicValidation.IsValid)
        {
            issues.Add($"Business logic violations: {validationResult.BusinessLogicValidation.RuleViolations.Count} issues");
        }

        var correctionPrompt = $@"
You are an expert SQL analyst. The following SQL query has validation issues and needs correction.

Original Business Question: {originalQuery}

Generated SQL: {sql}

Validation Issues:
{string.Join("\n", issues.Select(i => $"- {i}"))}

Please provide a corrected SQL query that:
1. Addresses all validation issues
2. Maintains the original business intent
3. Follows best practices for SQL generation
4. Uses appropriate table and column names from the schema

Corrected SQL:";

        return correctionPrompt;
    }

    // Helper methods for validation logic
    private EnhancedSemanticValidationResult CombineValidationResults(
        EnhancedSqlValidationResult security,
        SemanticValidationResult semantic,
        SchemaComplianceResult schema,
        BusinessLogicValidationResult business)
    {
        var isValid = security.IsValid && semantic.IsValid && schema.IsCompliant && business.IsValid;
        var overallScore = (semantic.AlignmentScore + schema.ComplianceScore + business.BusinessLogicScore) / 3.0;
        var canSelfCorrect = !security.IsValid || semantic.AlignmentScore < 0.7 || schema.ComplianceScore < 0.7;

        return new EnhancedSemanticValidationResult
        {
            IsValid = isValid,
            OverallScore = overallScore,
            CanSelfCorrect = canSelfCorrect,
            SecurityValidation = security,
            SemanticValidation = semantic,
            SchemaCompliance = schema,
            BusinessLogicValidation = business,
            ValidationTimestamp = DateTime.UtcNow
        };
    }

    private EnhancedSemanticValidationResult CreateErrorResult(string sql, string originalQuery, string errorMessage)
    {
        return new EnhancedSemanticValidationResult
        {
            IsValid = false,
            OverallScore = 0.0,
            CanSelfCorrect = false,
            ErrorMessage = errorMessage,
            ValidationTimestamp = DateTime.UtcNow
        };
    }

    // Placeholder methods for detailed validation logic (to be implemented)
    private async Task<(double Score, string Reason)> AnalyzeSqlSemanticAlignment(string sql, string query, EnhancedSchemaResult schema, CancellationToken ct) => (0.8, "Good alignment");
    private async Task<List<string>> DetectSemanticInconsistencies(string sql, EnhancedSchemaResult schema, CancellationToken ct) => new();
    private async Task<BusinessTermValidationResult> ValidateBusinessTermUsage(string sql, EnhancedSchemaResult schema, CancellationToken ct) => new() { IsValid = true };
    private double CalculateSemanticConfidence(dynamic alignment, List<string> inconsistencies, BusinessTermValidationResult termUsage) => 0.8;
    private async Task<SqlElementsResult> ExtractSqlElementsAsync(string sql, CancellationToken ct) => new();
    private TableValidationResult ValidateTableAccess(List<string> tables, EnhancedSchemaResult schema) => new() { IsValid = true };
    private ColumnValidationResult ValidateColumnUsage(List<string> columns, EnhancedSchemaResult schema) => new() { IsValid = true };
    private JoinValidationResult ValidateJoinRelationships(List<string> joins, EnhancedSchemaResult schema) => new() { IsValid = true };
    private ContextValidationResult ValidateBusinessContext(string sql, EnhancedSchemaResult schema) => new() { IsValid = true };
    private double CalculateComplianceScore(params dynamic[] validations) => 0.8;
    private async Task<List<string>> DetectBusinessRuleViolations(string sql, string query, CancellationToken ct) => new();
    private async Task<AccessValidationResult> ValidateDataAccessPermissions(string sql, CancellationToken ct) => new() { IsValid = true };
    private async Task<SensitivityValidationResult> ValidateSensitiveDataHandling(string sql, CancellationToken ct) => new() { IsValid = true };
    private AggregationValidationResult ValidateAggregationLogic(string sql) => new() { IsValid = true };
    private double CalculateBusinessLogicScore(params dynamic[] validations) => 0.8;
}
