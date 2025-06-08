using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data.Contexts;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Data.Migration;

/// <summary>
/// Helper service to assist with migrating services from legacy BICopilotContext to bounded contexts
/// </summary>
public class ServiceMigrationHelper
{
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<ServiceMigrationHelper> _logger;

    public ServiceMigrationHelper(
        IDbContextFactory contextFactory,
        ILogger<ServiceMigrationHelper> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get the appropriate context type for a given entity type
    /// </summary>
    public ContextType GetContextTypeForEntity(string entityName)
    {
        return entityName.ToLowerInvariant() switch
        {
            // Security entities
            "user" or "userentity" or "users" => ContextType.Security,
            "usersession" or "usersessionentity" or "usersessions" => ContextType.Security,
            "refreshtoken" or "refreshtokenentity" or "refreshtokens" => ContextType.Security,
            "mfachallenge" or "mfachallengeentity" or "mfachallenges" => ContextType.Security,
            "userpreferences" or "userpreferencesentity" => ContextType.Security,
            "auditlog" or "auditlogentity" => ContextType.Security,

            // Tuning entities
            "businesstableinfo" or "businesstableinfoentity" => ContextType.Tuning,
            "businesscolumninfo" or "businesscolumninfoentity" => ContextType.Tuning,
            "querypattern" or "querypatternentity" or "querypatterns" => ContextType.Tuning,
            "businessglossary" or "businessglossaryentity" => ContextType.Tuning,
            "aituningsettings" or "aituningsettingsentity" => ContextType.Tuning,
            "prompttemplate" or "prompttemplateentity" or "prompttemplates" => ContextType.Tuning,
            "promptlog" or "promptlogentity" or "promptlogs" => ContextType.Tuning,
            "aigenerationattempt" or "aigenerationattempts" => ContextType.Tuning,
            "aifeedbackentry" or "aifeedbackentries" => ContextType.Tuning,
            "systemconfiguration" or "systemconfigurationentity" => ContextType.Tuning,

            // Query entities
            "queryhistory" or "queryhistoryentity" => ContextType.Query,
            "queryhistories" => ContextType.Query,
            "queryexecutionlogs" => ContextType.Query,
            "querycache" or "querycacheentity" => ContextType.Query,
            "semanticcacheentry" or "semanticcacheentries" => ContextType.Query,
            "queryperformance" or "queryperformanceentity" => ContextType.Query,
            "suggestioncategory" or "suggestioncategories" => ContextType.Query,
            "querysuggestion" or "querysuggestions" => ContextType.Query,
            "suggestionusageanalytics" => ContextType.Query,
            "timeframedefinition" or "timeframedefinitions" => ContextType.Query,
            "tempfile" or "tempfiles" => ContextType.Query,

            // Schema entities
            "schemametadata" or "schemametadataentity" => ContextType.Schema,
            "businessschema" or "businessschemas" => ContextType.Schema,
            "businessschemaversion" or "businessschemaversions" => ContextType.Schema,
            "schematablecontext" or "schematablecontexts" => ContextType.Schema,
            "schemacolumncontext" or "schemacolumncontexts" => ContextType.Schema,
            "schemaglossaryterm" or "schemaglossaryterms" => ContextType.Schema,
            "schemarelationship" or "schemarelationships" => ContextType.Schema,
            "userschemapreference" or "userschemapreferences" => ContextType.Schema,

            // Monitoring entities
            "systemmetrics" or "systemmetricsentity" => ContextType.Monitoring,
            "performancemetrics" or "performancemetricsentity" => ContextType.Monitoring,
            "errorlog" or "errorlogentity" or "errorlogs" => ContextType.Monitoring,
            "healthchecklog" or "healthchecklogentity" or "healthchecklogs" => ContextType.Monitoring,
            "useractivity" or "useractivityentity" => ContextType.Monitoring,
            "featureusage" or "featureusageentity" => ContextType.Monitoring,
            "apiusage" or "apiusageentity" => ContextType.Monitoring,
            "resourceusage" or "resourceusageentity" => ContextType.Monitoring,
            "databaseconnection" or "databaseconnectionentity" => ContextType.Monitoring,

            // Default to legacy for unknown entities
            _ => ContextType.Legacy
        };
    }

    /// <summary>
    /// Get migration recommendations for a service
    /// </summary>
    public ServiceMigrationRecommendation GetMigrationRecommendation(string serviceName, string[] entityTypes)
    {
        var recommendation = new ServiceMigrationRecommendation
        {
            ServiceName = serviceName,
            EntityTypes = entityTypes.ToList()
        };

        // Determine primary context type
        var contextTypes = entityTypes.Select(GetContextTypeForEntity).Distinct().ToList();
        
        if (contextTypes.Count == 1)
        {
            recommendation.PrimaryContextType = contextTypes.First();
            recommendation.MigrationComplexity = MigrationComplexity.Simple;
            recommendation.RecommendedApproach = "Direct migration to single bounded context";
        }
        else if (contextTypes.Count == 2 && contextTypes.Contains(ContextType.Legacy))
        {
            recommendation.PrimaryContextType = contextTypes.First(c => c != ContextType.Legacy);
            recommendation.MigrationComplexity = MigrationComplexity.Medium;
            recommendation.RecommendedApproach = "Migrate to primary context, handle legacy entities separately";
        }
        else if (contextTypes.Count <= 3)
        {
            recommendation.PrimaryContextType = contextTypes.GroupBy(c => c)
                .OrderByDescending(g => g.Count())
                .First().Key;
            recommendation.MigrationComplexity = MigrationComplexity.Medium;
            recommendation.RecommendedApproach = "Use DbContextFactory with multiple contexts";
        }
        else
        {
            recommendation.PrimaryContextType = ContextType.Legacy;
            recommendation.MigrationComplexity = MigrationComplexity.Complex;
            recommendation.RecommendedApproach = "Refactor service to use focused services or keep legacy context";
        }

        recommendation.RequiredContextTypes = contextTypes.Where(c => c != ContextType.Legacy).ToList();

        return recommendation;
    }

    /// <summary>
    /// Generate migration code template for a service
    /// </summary>
    public string GenerateMigrationTemplate(ServiceMigrationRecommendation recommendation)
    {
        var template = $@"
// Migration template for {recommendation.ServiceName}
// Complexity: {recommendation.MigrationComplexity}
// Approach: {recommendation.RecommendedApproach}

public class {recommendation.ServiceName}
{{
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<{recommendation.ServiceName}> _logger;

    public {recommendation.ServiceName}(
        IDbContextFactory contextFactory,
        ILogger<{recommendation.ServiceName}> logger)
    {{
        _contextFactory = contextFactory;
        _logger = logger;
    }}

    // Example method using primary context ({recommendation.PrimaryContextType})
    public async Task<T> ExampleMethodAsync<T>()
    {{
        return await _contextFactory.ExecuteWithContextAsync(ContextType.{recommendation.PrimaryContextType}, async context =>
        {{
            var typedContext = ({recommendation.PrimaryContextType}DbContext)context;
            // Your logic here
            return default(T);
        }});
    }}";

        if (recommendation.RequiredContextTypes.Count > 1)
        {
            template += $@"

    // Example method using multiple contexts
    public async Task<T> ExampleMultiContextMethodAsync<T>()
    {{
        var contextTypes = new[] {{ {string.Join(", ", recommendation.RequiredContextTypes.Select(c => $"ContextType.{c}"))} }};
        return await _contextFactory.ExecuteWithMultipleContextsAsync(contextTypes, async contexts =>
        {{
            // Access different contexts as needed
            {string.Join("\n            ", recommendation.RequiredContextTypes.Select(c => $"var {c.ToString().ToLower()}Context = ({c}DbContext)contexts[ContextType.{c}];"))}
            
            // Your logic here
            return default(T);
        }});
    }}";
        }

        template += "\n}";

        return template;
    }

    /// <summary>
    /// Validate that a service migration is ready
    /// </summary>
    public async Task<MigrationValidationResult> ValidateServiceMigrationAsync(string serviceName, ContextType[] requiredContexts)
    {
        var result = new MigrationValidationResult();

        try
        {
            // Check if all required contexts are available
            foreach (var contextType in requiredContexts)
            {
                try
                {
                    using var context = _contextFactory.GetContextForOperation(contextType);
                    await context.Database.CanConnectAsync();
                    result.SuccessfulMigrations.Add($"{serviceName}_{contextType}");
                }
                catch (Exception ex)
                {
                    result.FailedMigrations.Add($"{serviceName}_{contextType}", ex.Message);
                    result.IsValid = false;
                }
            }

            _logger.LogInformation("Service migration validation for {ServiceName}: Success={IsValid}, Required contexts: {RequiredContexts}",
                serviceName, result.IsValid, string.Join(", ", requiredContexts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating service migration for {ServiceName}", serviceName);
            result.IsValid = false;
            result.FailedMigrations.Add($"{serviceName}_validation", ex.Message);
        }

        return result;
    }

    /// <summary>
    /// Get migration status for all services
    /// </summary>
    public async Task<ServiceMigrationStatus> GetMigrationStatusAsync()
    {
        var status = new ServiceMigrationStatus();

        try
        {
            // Check context factory health
            var contextHealth = await _contextFactory.GetConnectionHealthAsync();
            status.ContextFactoryHealthy = contextHealth.Values.All(h => h);
            status.AvailableContexts = contextHealth.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            status.UnavailableContexts = contextHealth.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();

            // Determine migration readiness
            status.MigrationReady = status.ContextFactoryHealthy && status.UnavailableContexts.Count == 0;

            _logger.LogInformation("Service migration status: Ready={MigrationReady}, Available contexts: {AvailableContexts}",
                status.MigrationReady, string.Join(", ", status.AvailableContexts));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service migration status");
            status.MigrationReady = false;
        }

        return status;
    }
}

/// <summary>
/// Service migration recommendation
/// </summary>
public class ServiceMigrationRecommendation
{
    public string ServiceName { get; set; } = string.Empty;
    public List<string> EntityTypes { get; set; } = new();
    public ContextType PrimaryContextType { get; set; }
    public List<ContextType> RequiredContextTypes { get; set; } = new();
    public MigrationComplexity MigrationComplexity { get; set; }
    public string RecommendedApproach { get; set; } = string.Empty;
}

/// <summary>
/// Migration complexity levels
/// </summary>
public enum MigrationComplexity
{
    Simple,
    Medium,
    Complex
}

/// <summary>
/// Service migration status
/// </summary>
public class ServiceMigrationStatus
{
    public bool MigrationReady { get; set; }
    public bool ContextFactoryHealthy { get; set; }
    public List<ContextType> AvailableContexts { get; set; } = new();
    public List<ContextType> UnavailableContexts { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}
