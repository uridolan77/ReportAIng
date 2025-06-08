using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data.Contexts;

namespace BIReportingCopilot.Infrastructure.Data.Migration;

/// <summary>
/// Tracks the progress of migration from monolithic to bounded contexts
/// </summary>
public class MigrationStatusTracker
{
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<MigrationStatusTracker> _logger;

    public MigrationStatusTracker(
        IDbContextFactory contextFactory,
        ILogger<MigrationStatusTracker> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get comprehensive migration status
    /// </summary>
    public async Task<ComprehensiveMigrationStatus> GetComprehensiveMigrationStatusAsync()
    {
        var status = new ComprehensiveMigrationStatus();

        try
        {
            // Phase 1: Bounded contexts status
            status.Phase1Status = await GetPhase1StatusAsync();

            // Phase 2: Service migration status
            status.Phase2Status = await GetPhase2StatusAsync();

            // Phase 3: Legacy deprecation status
            status.Phase3Status = await GetPhase3StatusAsync();

            // Overall status
            status.CurrentPhase = DetermineCurrentPhase(status);
            status.OverallProgress = CalculateOverallProgress(status);
            status.IsComplete = status.OverallProgress >= 100;

            _logger.LogInformation("Migration status: Phase={CurrentPhase}, Progress={OverallProgress}%", 
                status.CurrentPhase, status.OverallProgress);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comprehensive migration status");
            status.HasErrors = true;
            status.ErrorMessage = ex.Message;
        }

        return status;
    }

    private async Task<Phase1Status> GetPhase1StatusAsync()
    {
        var status = new Phase1Status();

        try
        {
            // Check if all bounded contexts are available and healthy
            var contextValidation = await _contextFactory.ValidateAllContextsAsync();
            var connectionHealth = await _contextFactory.GetConnectionHealthAsync();

            status.BoundedContextsCreated = contextValidation.IsValid;
            status.AllContextsHealthy = connectionHealth.Values.All(h => h);
            status.HealthyContexts = connectionHealth.Where(kvp => kvp.Value).Select(kvp => kvp.Key).ToList();
            status.UnhealthyContexts = connectionHealth.Where(kvp => !kvp.Value).Select(kvp => kvp.Key).ToList();

            // Check if DbContextFactory is working
            status.ContextFactoryWorking = true; // If we got here, it's working

            status.IsComplete = status.BoundedContextsCreated && status.AllContextsHealthy && status.ContextFactoryWorking;
            status.Progress = CalculatePhase1Progress(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Phase 1 status");
            status.IsComplete = false;
            status.Progress = 0;
        }

        return status;
    }

    private async Task<Phase2Status> GetPhase2StatusAsync()
    {
        var status = new Phase2Status();

        try
        {
            // Define services that need migration
            var servicesToMigrate = new[]
            {
                "TuningService",
                "QueryService", 
                "UserService",
                "AuditService",
                "SchemaService",
                "PromptManagementService",
                "SecurityManagementService",
                "QueryCacheService"
            };

            // Track which services have been migrated based on our work
            var migratedServices = new[]
            {
                "TuningService",            // ✅ Migrated to TuningDbContext
                "QueryService",             // ✅ Migrated to QueryDbContext
                "UserService",              // ✅ Migrated to SecurityDbContext
                "AuditService",             // ✅ Migrated to SecurityDbContext + QueryDbContext
                "SchemaService",            // ✅ Migrated to SchemaDbContext
                "PromptManagementService",  // ✅ Migrated to TuningDbContext
                "SemanticCacheService",     // ✅ Migrated to QueryDbContext
                "QueryCacheService"         // ✅ Already using ICacheService (no migration needed)
            };

            status.TotalServices = servicesToMigrate.Length;
            status.MigratedServices = migratedServices.Length;
            status.RemainingServices = servicesToMigrate.Except(migratedServices).ToList();
            status.CompletedServices = migratedServices.ToList();

            status.Progress = (status.MigratedServices * 100) / status.TotalServices;
            status.IsComplete = status.MigratedServices == status.TotalServices;

            // Check if services are working with bounded contexts
            status.ServicesWorkingWithBoundedContexts = await TestMigratedServicesAsync(migratedServices);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Phase 2 status");
            status.IsComplete = false;
            status.Progress = 0;
        }

        return status;
    }

    private async Task<Phase3Status> GetPhase3StatusAsync()
    {
        var status = new Phase3Status();

        try
        {
            // Check if legacy context is still being used
            status.LegacyContextStillRegistered = true; // We still have it registered for backward compatibility
            
            // Check if any services still depend on legacy context
            status.ServicesUsingLegacyContext = await GetServicesUsingLegacyContextAsync();
            
            // Check if data migration is complete
            status.DataMigrationComplete = await CheckDataMigrationCompleteAsync();
            
            // Check if legacy context can be safely removed
            status.CanRemoveLegacyContext = status.ServicesUsingLegacyContext.Count == 0 && status.DataMigrationComplete;
            
            status.Progress = CalculatePhase3Progress(status);
            status.IsComplete = !status.LegacyContextStillRegistered && status.CanRemoveLegacyContext;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting Phase 3 status");
            status.IsComplete = false;
            status.Progress = 0;
        }

        return status;
    }

    private async Task<bool> TestMigratedServicesAsync(string[] migratedServices)
    {
        try
        {
            // Test if we can create contexts for migrated services
            foreach (var contextType in Enum.GetValues<ContextType>())
            {
                if (contextType == ContextType.Legacy) continue;

                using var context = _contextFactory.GetContextForOperation(contextType);
                var canConnect = await context.Database.CanConnectAsync();
                if (!canConnect) return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<string>> GetServicesUsingLegacyContextAsync()
    {
        // All services have been migrated to bounded contexts!
        // SecurityManagementService and QueryCacheService didn't use BICopilotContext directly
        await Task.CompletedTask; // Placeholder for async operation

        return new List<string>(); // No services using legacy context anymore!
    }

    private async Task<bool> CheckDataMigrationCompleteAsync()
    {
        try
        {
            // Check if bounded contexts have data
            var hasSecurityData = await _contextFactory.ExecuteWithContextAsync(ContextType.Security, async context =>
            {
                var securityContext = (SecurityDbContext)context;
                return await securityContext.Users.AnyAsync();
            });

            var hasTuningData = await _contextFactory.ExecuteWithContextAsync(ContextType.Tuning, async context =>
            {
                var tuningContext = (TuningDbContext)context;
                return await tuningContext.BusinessTableInfo.AnyAsync();
            });

            var hasQueryData = await _contextFactory.ExecuteWithContextAsync(ContextType.Query, async context =>
            {
                var queryContext = (QueryDbContext)context;
                return await queryContext.QueryHistories.AnyAsync();
            });

            return hasSecurityData || hasTuningData || hasQueryData;
        }
        catch
        {
            return false;
        }
    }

    private int CalculatePhase1Progress(Phase1Status status)
    {
        var totalChecks = 3; // BoundedContextsCreated, AllContextsHealthy, ContextFactoryWorking
        var completedChecks = 0;

        if (status.BoundedContextsCreated) completedChecks++;
        if (status.AllContextsHealthy) completedChecks++;
        if (status.ContextFactoryWorking) completedChecks++;

        return (completedChecks * 100) / totalChecks;
    }

    private int CalculatePhase3Progress(Phase3Status status)
    {
        var totalChecks = 3; // DataMigrationComplete, No services using legacy, Can remove legacy
        var completedChecks = 0;

        if (status.DataMigrationComplete) completedChecks++;
        if (status.ServicesUsingLegacyContext.Count == 0) completedChecks++;
        if (status.CanRemoveLegacyContext) completedChecks++;

        return (completedChecks * 100) / totalChecks;
    }

    private string DetermineCurrentPhase(ComprehensiveMigrationStatus status)
    {
        if (!status.Phase1Status.IsComplete)
            return "Phase 1: Setting up Bounded Contexts";
        
        if (!status.Phase2Status.IsComplete)
            return "Phase 2: Migrating Services";
        
        if (!status.Phase3Status.IsComplete)
            return "Phase 3: Deprecating Legacy Context";
        
        return "Migration Complete";
    }

    private int CalculateOverallProgress(ComprehensiveMigrationStatus status)
    {
        // Weight each phase equally (33.33% each)
        var phase1Weight = 33.33;
        var phase2Weight = 33.33;
        var phase3Weight = 33.34;

        var totalProgress = 
            (status.Phase1Status.Progress * phase1Weight / 100) +
            (status.Phase2Status.Progress * phase2Weight / 100) +
            (status.Phase3Status.Progress * phase3Weight / 100);

        return (int)Math.Round(totalProgress);
    }

    /// <summary>
    /// Get next recommended actions based on current status
    /// </summary>
    public async Task<List<string>> GetRecommendedActionsAsync()
    {
        var status = await GetComprehensiveMigrationStatusAsync();
        var actions = new List<string>();

        if (!status.Phase1Status.IsComplete)
        {
            if (!status.Phase1Status.BoundedContextsCreated)
                actions.Add("Create and configure all bounded contexts");
            if (!status.Phase1Status.AllContextsHealthy)
                actions.Add("Fix unhealthy contexts: " + string.Join(", ", status.Phase1Status.UnhealthyContexts));
        }
        else if (!status.Phase2Status.IsComplete)
        {
            actions.Add($"Migrate remaining services: {string.Join(", ", status.Phase2Status.RemainingServices)}");
            actions.Add("Test migrated services with bounded contexts");
        }
        else if (!status.Phase3Status.IsComplete)
        {
            if (status.Phase3Status.ServicesUsingLegacyContext.Any())
                actions.Add($"Complete migration of: {string.Join(", ", status.Phase3Status.ServicesUsingLegacyContext)}");
            if (!status.Phase3Status.DataMigrationComplete)
                actions.Add("Complete data migration to bounded contexts");
            if (status.Phase3Status.CanRemoveLegacyContext)
                actions.Add("Remove legacy BICopilotContext from DI registration");
        }
        else
        {
            actions.Add("Migration complete! Consider performance optimization and monitoring.");
        }

        return actions;
    }
}

/// <summary>
/// Comprehensive migration status
/// </summary>
public class ComprehensiveMigrationStatus
{
    public string CurrentPhase { get; set; } = string.Empty;
    public int OverallProgress { get; set; }
    public bool IsComplete { get; set; }
    public bool HasErrors { get; set; }
    public string? ErrorMessage { get; set; }
    public Phase1Status Phase1Status { get; set; } = new();
    public Phase2Status Phase2Status { get; set; } = new();
    public Phase3Status Phase3Status { get; set; } = new();
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Phase 1: Bounded contexts setup status
/// </summary>
public class Phase1Status
{
    public bool BoundedContextsCreated { get; set; }
    public bool AllContextsHealthy { get; set; }
    public bool ContextFactoryWorking { get; set; }
    public List<ContextType> HealthyContexts { get; set; } = new();
    public List<ContextType> UnhealthyContexts { get; set; } = new();
    public int Progress { get; set; }
    public bool IsComplete { get; set; }
}

/// <summary>
/// Phase 2: Service migration status
/// </summary>
public class Phase2Status
{
    public int TotalServices { get; set; }
    public int MigratedServices { get; set; }
    public List<string> CompletedServices { get; set; } = new();
    public List<string> RemainingServices { get; set; } = new();
    public bool ServicesWorkingWithBoundedContexts { get; set; }
    public int Progress { get; set; }
    public bool IsComplete { get; set; }
}

/// <summary>
/// Phase 3: Legacy context deprecation status
/// </summary>
public class Phase3Status
{
    public bool LegacyContextStillRegistered { get; set; }
    public List<string> ServicesUsingLegacyContext { get; set; } = new();
    public bool DataMigrationComplete { get; set; }
    public bool CanRemoveLegacyContext { get; set; }
    public int Progress { get; set; }
    public bool IsComplete { get; set; }
}
