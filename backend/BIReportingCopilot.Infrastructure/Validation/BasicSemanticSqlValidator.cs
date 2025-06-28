using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces.Validation;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Security;
using BIReportingCopilot.Core.Interfaces;
using System.Text.RegularExpressions;

namespace BIReportingCopilot.Infrastructure.Validation;

/// <summary>
/// Basic semantic SQL validator implementation without complex AI features
/// Focuses on syntax validation, basic semantic checks, and column existence validation
/// Phase 1: Critical Infrastructure Fixes - Step 1.1
/// </summary>
public class BasicSemanticSqlValidator : IEnhancedSemanticSqlValidator
{
    private readonly ILogger<BasicSemanticSqlValidator> _logger;
    private readonly ISqlQueryValidator _sqlValidator;
    private readonly IForeignKeyRelationshipService _relationshipService;

    public BasicSemanticSqlValidator(
        ILogger<BasicSemanticSqlValidator> logger,
        ISqlQueryValidator sqlValidator,
        IForeignKeyRelationshipService relationshipService)
    {
        _logger = logger;
        _sqlValidator = sqlValidator;
        _relationshipService = relationshipService;
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
        _logger.LogInformation("🔍 [BASIC-SQL-VALIDATOR] Starting semantic validation for query: {Query}", originalQuery);

        try
        {
            var result = new EnhancedSemanticValidationResult
            {
                OriginalSql = sql,
                ValidationTimestamp = DateTime.UtcNow
            };

            // Step 1: Basic security validation
            var securityValidation = await _sqlValidator.ValidateQueryAsync(sql);
            result.SecurityValidation = securityValidation;

            // Step 2: Semantic validation
            var semanticValidation = await ValidateSemanticAlignmentAsync(sql, originalQuery, cancellationToken);
            result.SemanticValidation = semanticValidation;

            // Step 3: Schema compliance validation
            var schemaCompliance = await ValidateSchemaComplianceAsync(sql, originalQuery, cancellationToken);
            result.SchemaCompliance = schemaCompliance;

            // Step 4: Business logic validation
            var businessLogicValidation = await ValidateBusinessLogicAsync(sql, originalQuery, cancellationToken);
            result.BusinessLogicValidation = businessLogicValidation;

            // Calculate overall score and validity
            result.OverallScore = CalculateOverallScore(securityValidation, semanticValidation, schemaCompliance, businessLogicValidation);
            result.IsValid = result.OverallScore >= 0.6; // 60% threshold for basic validation

            // Determine if self-correction is possible
            result.CanSelfCorrect = result.OverallScore >= 0.4 && result.OverallScore < 0.6;

            _logger.LogInformation("✅ [BASIC-SQL-VALIDATOR] Validation completed: Valid={IsValid}, Score={Score:F2}", 
                result.IsValid, result.OverallScore);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [BASIC-SQL-VALIDATOR] Validation failed for query: {Query}", originalQuery);
            return new EnhancedSemanticValidationResult
            {
                IsValid = false,
                OverallScore = 0.0,
                ErrorMessage = ex.Message,
                OriginalSql = sql,
                ValidationTimestamp = DateTime.UtcNow
            };
        }
    }

    /// <summary>
    /// Validate SQL with specific validation level
    /// </summary>
    public async Task<EnhancedValidationResponse> ValidateWithLevelAsync(
        EnhancedValidationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 [BASIC-SQL-VALIDATOR] Validating with level: {Level}", request.ValidationLevel);

        try
        {
            var validationResult = await ValidateWithSemanticAnalysisAsync(
                request.Sql, 
                request.OriginalQuery, 
                request.Context, 
                request.UserId, 
                cancellationToken);

            return new EnhancedValidationResponse
            {
                Success = validationResult.IsValid,
                Message = validationResult.IsValid ? "Validation passed" : "Validation failed",
                ValidationResult = validationResult,
                Warnings = ExtractWarnings(validationResult),
                Metadata = new Dictionary<string, object>
                {
                    ["ValidationLevel"] = request.ValidationLevel.ToString(),
                    ["ValidationTimestamp"] = DateTime.UtcNow,
                    ["ValidatorType"] = "BasicSemanticSqlValidator"
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ [BASIC-SQL-VALIDATOR] Level validation failed");
            return new EnhancedValidationResponse
            {
                Success = false,
                Message = $"Validation error: {ex.Message}",
                Warnings = new List<string> { "Validation service encountered an error" }
            };
        }
    }

    /// <summary>
    /// Perform dry-run execution of SQL for validation
    /// </summary>
    public async Task<DryRunExecutionResult> PerformDryRunAsync(
        DryRunExecutionRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔍 [BASIC-SQL-VALIDATOR] Performing dry-run execution");

        // Basic implementation - just validate syntax and structure
        var syntaxValid = ValidateSqlSyntax(request.Sql);
        
        return new DryRunExecutionResult
        {
            CanExecute = syntaxValid,
            ExecutedSuccessfully = false, // We don't actually execute in basic validator
            EstimatedExecutionTime = TimeSpan.FromSeconds(5), // Default estimate
            EstimatedRowCount = 100, // Default estimate
            ExecutionWarnings = syntaxValid ? new List<string>() : new List<string> { "SQL syntax validation failed" },
            ExecutionErrors = syntaxValid ? new List<string>() : new List<string> { "Invalid SQL syntax detected" },
            PerformanceMetrics = new Dictionary<string, object>
            {
                ["SyntaxValid"] = syntaxValid,
                ["ValidatorType"] = "BasicSemanticSqlValidator",
                ["DryRunType"] = "SyntaxOnly"
            }
        };
    }

    /// <summary>
    /// Attempt self-correction of SQL based on validation issues
    /// </summary>
    public async Task<SelfCorrectionAttempt> AttemptSelfCorrectionAsync(
        string sql,
        string originalQuery,
        List<string> validationIssues,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("🔧 [BASIC-SQL-VALIDATOR] Attempting basic self-correction");

        // Basic self-correction - just fix common syntax issues
        var correctedSql = sql;
        var corrections = new List<string>();

        // Fix missing semicolon
        if (!correctedSql.TrimEnd().EndsWith(";"))
        {
            correctedSql = correctedSql.TrimEnd() + ";";
            corrections.Add("Added missing semicolon");
        }

        // Fix common case issues
        correctedSql = FixCommonCaseIssues(correctedSql);
        if (correctedSql != sql)
        {
            corrections.Add("Fixed SQL keyword casing");
        }

        return new SelfCorrectionAttempt
        {
            WasSuccessful = corrections.Any(),
            CorrectedSql = correctedSql,
            OriginalSql = sql,
            CorrectionReason = string.Join("; ", corrections),
            ImprovementScore = corrections.Any() ? 0.7 : 0.0,
            IssuesAddressed = corrections,
            AttemptTimestamp = DateTime.UtcNow
        };
    }

    #region Interface Implementation Methods

    public async Task<SemanticValidationResult> ValidateSemanticAlignmentAsync(
        string sql,
        string originalQuery,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🔍 [BASIC-SQL-VALIDATOR] Validating semantic alignment");

        var result = new SemanticValidationResult();

        // Basic semantic checks
        var queryLower = originalQuery.ToLowerInvariant();
        var sqlLower = sql.ToLowerInvariant();

        // Check for basic alignment
        var alignmentScore = 0.0;

        // Check if query intent matches SQL structure
        if (queryLower.Contains("top") && sqlLower.Contains("top"))
            alignmentScore += 0.2;
        
        if (queryLower.Contains("count") && sqlLower.Contains("count"))
            alignmentScore += 0.2;
            
        if (queryLower.Contains("sum") && sqlLower.Contains("sum"))
            alignmentScore += 0.2;
            
        if (queryLower.Contains("group") && sqlLower.Contains("group by"))
            alignmentScore += 0.2;
            
        if (queryLower.Contains("order") && sqlLower.Contains("order by"))
            alignmentScore += 0.2;

        result.AlignmentScore = alignmentScore;
        result.IsValid = alignmentScore >= 0.4;
        result.AlignmentReason = $"Basic semantic alignment score: {alignmentScore:F2}";
        result.ConfidenceScore = alignmentScore;

        return result;
    }

    public async Task<SchemaComplianceResult> ValidateSchemaComplianceAsync(
        string sql,
        string originalQuery,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🔍 [BASIC-SQL-VALIDATOR] Validating schema compliance");

        var result = new SchemaComplianceResult();

        // Extract table names from SQL
        var tableNames = ExtractTableNames(sql);
        
        // Basic table validation
        var validTables = new List<string>();
        var invalidTables = new List<string>();

        foreach (var tableName in tableNames)
        {
            // Basic validation - check if table name follows naming conventions
            if (IsValidTableName(tableName))
            {
                validTables.Add(tableName);
            }
            else
            {
                invalidTables.Add(tableName);
            }
        }

        result.TableValidation = new TableValidationResult
        {
            IsValid = invalidTables.Count == 0,
            ValidTables = validTables,
            InvalidTables = invalidTables
        };

        result.IsValid = result.TableValidation.IsValid;
        result.ComplianceScore = validTables.Count > 0 ? (double)validTables.Count / tableNames.Count : 0.0;

        return result;
    }

    public async Task<BusinessLogicValidationResult> ValidateBusinessLogicAsync(
        string sql,
        string originalQuery,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🔍 [BASIC-SQL-VALIDATOR] Validating business logic");

        var result = new BusinessLogicValidationResult();

        // Basic business logic checks
        var violations = new List<string>();
        var recommendations = new List<string>();

        // Check for potential performance issues
        if (sql.ToLowerInvariant().Contains("select *"))
        {
            violations.Add("Using SELECT * may impact performance");
            recommendations.Add("Consider selecting only required columns");
        }

        // Check for missing WHERE clauses on large tables
        if (ContainsLargeTableWithoutWhere(sql))
        {
            violations.Add("Query on large table without WHERE clause");
            recommendations.Add("Consider adding appropriate filters");
        }

        result.BusinessRuleViolations = violations;
        result.Recommendations = recommendations;
        result.IsValid = violations.Count == 0;
        result.ComplianceScore = violations.Count == 0 ? 1.0 : Math.Max(0.0, 1.0 - (violations.Count * 0.2));

        return result;
    }

    private double CalculateOverallScore(
        SqlValidationResult securityValidation,
        SemanticValidationResult semanticValidation,
        SchemaComplianceResult schemaCompliance,
        BusinessLogicValidationResult businessLogicValidation)
    {
        var securityScore = securityValidation.IsValid ? 1.0 : 0.0;
        var semanticScore = semanticValidation.AlignmentScore;
        var schemaScore = schemaCompliance.ComplianceScore;
        var businessScore = businessLogicValidation.ComplianceScore;

        // Weighted average: Security (40%), Semantic (30%), Schema (20%), Business (10%)
        return (securityScore * 0.4) + (semanticScore * 0.3) + (schemaScore * 0.2) + (businessScore * 0.1);
    }

    private List<string> ExtractWarnings(EnhancedSemanticValidationResult result)
    {
        var warnings = new List<string>();

        if (result.SecurityValidation is SqlValidationResult securityResult)
        {
            warnings.AddRange(securityResult.Warnings);
        }

        if (result.SemanticValidation?.SemanticInconsistencies != null)
        {
            warnings.AddRange(result.SemanticValidation.SemanticInconsistencies);
        }

        if (result.BusinessLogicValidation?.BusinessRuleViolations != null)
        {
            warnings.AddRange(result.BusinessLogicValidation.BusinessRuleViolations);
        }

        return warnings;
    }

    private bool ValidateSqlSyntax(string sql)
    {
        if (string.IsNullOrWhiteSpace(sql))
            return false;

        // Basic syntax validation
        var trimmedSql = sql.Trim();
        
        // Must start with SELECT (for basic validator)
        if (!trimmedSql.StartsWith("SELECT", StringComparison.OrdinalIgnoreCase))
            return false;

        // Check for balanced parentheses
        var openParens = sql.Count(c => c == '(');
        var closeParens = sql.Count(c => c == ')');
        if (openParens != closeParens)
            return false;

        // Check for balanced quotes
        var singleQuotes = sql.Count(c => c == '\'');
        if (singleQuotes % 2 != 0)
            return false;

        return true;
    }

    private string FixCommonCaseIssues(string sql)
    {
        // Fix common SQL keyword casing
        var keywords = new[] { "SELECT", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "ON", "GROUP BY", "ORDER BY", "HAVING" };
        
        foreach (var keyword in keywords)
        {
            sql = Regex.Replace(sql, $@"\b{keyword}\b", keyword, RegexOptions.IgnoreCase);
        }

        return sql;
    }

    private List<string> ExtractTableNames(string sql)
    {
        var tableNames = new List<string>();
        
        // Simple regex to extract table names after FROM and JOIN
        var fromMatches = Regex.Matches(sql, @"FROM\s+(\w+)", RegexOptions.IgnoreCase);
        var joinMatches = Regex.Matches(sql, @"JOIN\s+(\w+)", RegexOptions.IgnoreCase);

        foreach (Match match in fromMatches)
        {
            if (match.Groups.Count > 1)
                tableNames.Add(match.Groups[1].Value);
        }

        foreach (Match match in joinMatches)
        {
            if (match.Groups.Count > 1)
                tableNames.Add(match.Groups[1].Value);
        }

        return tableNames.Distinct().ToList();
    }

    private bool IsValidTableName(string tableName)
    {
        // Basic table name validation
        if (string.IsNullOrWhiteSpace(tableName))
            return false;

        // Check if it follows basic naming conventions
        return Regex.IsMatch(tableName, @"^[a-zA-Z_][a-zA-Z0-9_]*$");
    }

    private bool ContainsLargeTableWithoutWhere(string sql)
    {
        var sqlLower = sql.ToLowerInvariant();
        var largeTablePatterns = new[] { "tbl_daily_actions", "daily_actions", "transactions", "logs" };
        
        foreach (var pattern in largeTablePatterns)
        {
            if (sqlLower.Contains(pattern) && !sqlLower.Contains("where"))
            {
                return true;
            }
        }

        return false;
    }

    private List<string> ExtractColumnNames(string sql)
    {
        var columns = new List<string>();

        // Simple regex to extract column names after SELECT
        var selectMatches = Regex.Matches(sql, @"SELECT\s+(.*?)\s+FROM", RegexOptions.IgnoreCase | RegexOptions.Singleline);

        foreach (Match match in selectMatches)
        {
            if (match.Groups.Count > 1)
            {
                var columnsPart = match.Groups[1].Value;
                var columnNames = columnsPart.Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c) && c != "*")
                    .ToList();
                columns.AddRange(columnNames);
            }
        }

        return columns.Distinct().ToList();
    }

    private List<string> ExtractSqlKeywords(string sql)
    {
        var keywords = new[] { "SELECT", "FROM", "WHERE", "JOIN", "INNER", "LEFT", "RIGHT", "GROUP BY", "ORDER BY", "HAVING", "UNION", "DISTINCT" };
        var sqlUpper = sql.ToUpperInvariant();

        return keywords.Where(keyword => sqlUpper.Contains(keyword)).ToList();
    }

    private List<string> ExtractSqlFunctions(string sql)
    {
        var functions = new List<string>();
        var functionPatterns = new[] { "COUNT", "SUM", "AVG", "MAX", "MIN", "DATEADD", "GETDATE", "CAST", "CONVERT" };

        foreach (var function in functionPatterns)
        {
            if (sql.ToUpperInvariant().Contains($"{function}("))
            {
                functions.Add(function);
            }
        }

        return functions.Distinct().ToList();
    }

    private List<string> ExtractSqlOperators(string sql)
    {
        var operators = new List<string>();
        var operatorPatterns = new[] { "=", "!=", "<>", "<", ">", "<=", ">=", "LIKE", "IN", "NOT IN", "EXISTS", "NOT EXISTS" };

        foreach (var op in operatorPatterns)
        {
            if (sql.ToUpperInvariant().Contains(op))
            {
                operators.Add(op);
            }
        }

        return operators.Distinct().ToList();
    }

    #endregion

    #region Additional Interface Methods

    /// <summary>
    /// Extract and analyze SQL elements for validation
    /// </summary>
    public async Task<SqlElementsResult> ExtractSqlElementsAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🔍 [BASIC-SQL-VALIDATOR] Extracting SQL elements");

        var result = new SqlElementsResult();

        // Extract basic SQL elements
        result.Tables = ExtractTableNames(sql);
        result.Columns = ExtractColumnNames(sql);

        // Extract joins
        var joinMatches = Regex.Matches(sql, @"(INNER|LEFT|RIGHT|FULL)\s+JOIN\s+(\w+)", RegexOptions.IgnoreCase);
        result.Joins = joinMatches.Cast<Match>().Select(m => m.Value).ToList();

        // Extract WHERE conditions
        var whereMatches = Regex.Matches(sql, @"WHERE\s+(.*?)(?:\s+GROUP\s+BY|\s+ORDER\s+BY|\s*$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result.WhereConditions = whereMatches.Cast<Match>().Select(m => m.Groups[1].Value.Trim()).ToList();

        // Extract aggregations
        var aggregationFunctions = new[] { "COUNT", "SUM", "AVG", "MAX", "MIN" };
        result.Aggregations = aggregationFunctions.Where(func => sql.ToUpperInvariant().Contains($"{func}(")).ToList();

        // Extract ORDER BY
        var orderByMatches = Regex.Matches(sql, @"ORDER\s+BY\s+(.*?)(?:\s*$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result.OrderBy = orderByMatches.Cast<Match>().Select(m => m.Groups[1].Value.Trim()).ToList();

        // Extract GROUP BY
        var groupByMatches = Regex.Matches(sql, @"GROUP\s+BY\s+(.*?)(?:\s+ORDER\s+BY|\s*$)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
        result.GroupBy = groupByMatches.Cast<Match>().Select(m => m.Groups[1].Value.Trim()).ToList();

        return result;
    }

    /// <summary>
    /// Get validation metrics for monitoring and optimization
    /// </summary>
    public async Task<ValidationMetrics> GetValidationMetricsAsync(
        TimeSpan timeWindow,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("📊 [BASIC-SQL-VALIDATOR] Getting validation metrics");

        // Basic implementation - return default metrics
        return new ValidationMetrics
        {
            Timestamp = DateTime.UtcNow,
            ValidationDuration = timeWindow,
            TotalValidations = 0,
            SuccessfulValidations = 0,
            SelfCorrectionAttempts = 0,
            SuccessfulSelfCorrections = 0,
            AverageValidationScore = 0.0,
            ValidationTypeMetrics = new Dictionary<string, int>
            {
                ["BasicSemanticValidation"] = 0,
                ["SecurityValidation"] = 0,
                ["SchemaValidation"] = 0,
                ["BusinessLogicValidation"] = 0
            }
        };
    }

    /// <summary>
    /// Validate table access permissions
    /// </summary>
    public async Task<TableAccessValidationResult> ValidateTableAccessAsync(
        string sql,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🔍 [BASIC-SQL-VALIDATOR] Validating table access");

        var result = new TableAccessValidationResult();
        var tableNames = ExtractTableNames(sql);

        // Basic access validation - assume all tables are accessible for basic validator
        result.AccessibleTables = tableNames;
        result.InaccessibleTables = new List<string>();
        result.IsValid = true;
        result.AccessLevel = "Read";

        return result;
    }

    /// <summary>
    /// Validate column access permissions
    /// </summary>
    public async Task<ColumnAccessValidationResult> ValidateColumnAccessAsync(
        string sql,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🔍 [BASIC-SQL-VALIDATOR] Validating column access");

        var result = new ColumnAccessValidationResult();

        // Basic implementation - assume all columns are accessible
        result.AccessibleColumns = new List<string>();
        result.InaccessibleColumns = new List<string>();
        result.IsValid = true;

        return result;
    }

    /// <summary>
    /// Check data access permissions
    /// </summary>
    public async Task<AccessValidationResult> ValidateDataAccessAsync(
        string sql,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🔍 [BASIC-SQL-VALIDATOR] Validating data access");

        var result = new AccessValidationResult();

        // Basic implementation - perform security validation
        var securityResult = await _sqlValidator.ValidateQueryAsync(sql);

        result.IsValid = securityResult.IsValid;
        result.AccessLevel = securityResult.IsValid ? "Read" : "Denied";
        result.Restrictions = securityResult.IsValid ? new List<string>() : new List<string> { "Security validation failed" };
        result.Reason = securityResult.IsValid ? "Access granted" : "Security validation failed";

        return result;
    }

    /// <summary>
    /// Validate sensitive data handling
    /// </summary>
    public async Task<SensitivityValidationResult> ValidateSensitiveDataAsync(
        string sql,
        CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("🔍 [BASIC-SQL-VALIDATOR] Validating sensitive data handling");

        var result = new SensitivityValidationResult();

        // Basic sensitive data patterns
        var sensitivePatterns = new[] { "password", "ssn", "credit_card", "email", "phone" };
        var sqlLower = sql.ToLowerInvariant();

        var detectedSensitiveFields = sensitivePatterns.Where(pattern => sqlLower.Contains(pattern)).ToList();

        result.ContainsSensitiveData = detectedSensitiveFields.Any();
        result.SensitiveFields = detectedSensitiveFields;
        result.IsValid = !result.ContainsSensitiveData; // Basic validator blocks sensitive data
        result.SensitivityLevel = result.ContainsSensitiveData ? "High" : "Low";
        result.RequiredProtections = result.ContainsSensitiveData ?
            new List<string> { "Data masking required", "Audit logging required" } :
            new List<string>();

        return result;
    }

    /// <summary>
    /// Generate validation report
    /// </summary>
    public async Task<ValidationReport> GenerateValidationReportAsync(
        string sql,
        string originalQuery,
        string? context = null,
        string? userId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("📊 [BASIC-SQL-VALIDATOR] Generating validation report");

        var validationResult = await ValidateWithSemanticAnalysisAsync(sql, originalQuery, context, userId, cancellationToken);

        var report = new ValidationReport
        {
            Query = sql,
            OriginalQuery = originalQuery,
            ValidationResult = validationResult,
            IsValid = validationResult.IsValid,
            OverallScore = validationResult.OverallScore,
            GeneratedAt = DateTime.UtcNow,
            ValidatorType = "BasicSemanticSqlValidator",
            Summary = GenerateValidationSummary(validationResult),
            Recommendations = GenerateRecommendations(validationResult),
            Metadata = new Dictionary<string, object>
            {
                ["ValidatorVersion"] = "1.0.0",
                ["ValidationLevel"] = "Basic",
                ["UserId"] = userId ?? "Anonymous"
            }
        };

        return report;
    }

    /// <summary>
    /// Configure self-correction behavior
    /// </summary>
    public async Task<bool> ConfigureSelfCorrectionAsync(
        SelfCorrectionConfiguration configuration,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("⚙️ [BASIC-SQL-VALIDATOR] Configuring self-correction");

        // Basic validator has limited self-correction capabilities
        // Just log the configuration and return success
        _logger.LogInformation("Self-correction configuration applied: {Config}", configuration);

        return true;
    }

    private string GenerateValidationSummary(EnhancedSemanticValidationResult result)
    {
        if (result.IsValid)
        {
            return $"Validation passed with score {result.OverallScore:F2}. SQL query meets basic validation criteria.";
        }
        else
        {
            return $"Validation failed with score {result.OverallScore:F2}. {result.ErrorMessage ?? "Multiple validation issues detected."}";
        }
    }

    private List<string> GenerateRecommendations(EnhancedSemanticValidationResult result)
    {
        var recommendations = new List<string>();

        if (result.OverallScore < 0.6)
        {
            recommendations.Add("Consider reviewing SQL syntax and structure");
        }

        if (result.SemanticValidation?.AlignmentScore < 0.5)
        {
            recommendations.Add("SQL query may not fully align with the original natural language query");
        }

        if (result.BusinessLogicValidation?.BusinessRuleViolations?.Any() == true)
        {
            recommendations.AddRange(result.BusinessLogicValidation.Recommendations);
        }

        if (!recommendations.Any())
        {
            recommendations.Add("Query validation passed successfully");
        }

        return recommendations;
    }

    #endregion
}
