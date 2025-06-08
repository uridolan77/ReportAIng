using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BIReportingCopilot.Infrastructure.Data.Migration;
using BIReportingCopilot.Infrastructure.Data.Contexts;

namespace BIReportingCopilot.API.Controllers;

/// <summary>
/// Controller for managing database context migration from monolithic to bounded contexts
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class MigrationController : ControllerBase
{
    private readonly ContextMigrationService _contextMigrationService;
    private readonly ServiceMigrationHelper _serviceMigrationHelper;
    private readonly MigrationStatusTracker _statusTracker;
    private readonly IDbContextFactory _contextFactory;
    private readonly ILogger<MigrationController> _logger;

    public MigrationController(
        ContextMigrationService contextMigrationService,
        ServiceMigrationHelper serviceMigrationHelper,
        MigrationStatusTracker statusTracker,
        IDbContextFactory contextFactory,
        ILogger<MigrationController> logger)
    {
        _contextMigrationService = contextMigrationService;
        _serviceMigrationHelper = serviceMigrationHelper;
        _statusTracker = statusTracker;
        _contextFactory = contextFactory;
        _logger = logger;
    }

    /// <summary>
    /// Get migration readiness status
    /// </summary>
    [HttpGet("readiness")]
    public async Task<ActionResult<object>> GetMigrationReadiness()
    {
        try
        {
            var contextReadiness = await _contextMigrationService.ValidateMigrationReadinessAsync();
            var serviceStatus = await _serviceMigrationHelper.GetMigrationStatusAsync();
            var contextValidation = await _contextFactory.ValidateAllContextsAsync();

            return Ok(new
            {
                ContextMigration = contextReadiness,
                ServiceMigration = serviceStatus,
                ContextValidation = contextValidation,
                OverallReady = contextReadiness.IsReady && serviceStatus.MigrationReady && contextValidation.IsValid,
                CheckedAt = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking migration readiness");
            return StatusCode(500, "Error checking migration readiness");
        }
    }

    /// <summary>
    /// Validate all bounded contexts
    /// </summary>
    [HttpGet("contexts/validate")]
    public async Task<ActionResult<object>> ValidateContexts()
    {
        try
        {
            var validation = await _contextFactory.ValidateAllContextsAsync();
            var connectionHealth = await _contextFactory.GetConnectionHealthAsync();

            return Ok(new
            {
                Validation = validation,
                ConnectionHealth = connectionHealth,
                Summary = new
                {
                    AllValid = validation.IsValid,
                    SuccessfulContexts = validation.SuccessfulContexts.Count,
                    FailedContexts = validation.FailedContexts.Count,
                    HealthyConnections = connectionHealth.Count(kvp => kvp.Value),
                    UnhealthyConnections = connectionHealth.Count(kvp => !kvp.Value)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating contexts");
            return StatusCode(500, "Error validating contexts");
        }
    }

    /// <summary>
    /// Perform dry run of data migration
    /// </summary>
    [HttpPost("data/dry-run")]
    public async Task<ActionResult<MigrationResult>> PerformDataMigrationDryRun()
    {
        try
        {
            _logger.LogInformation("Starting data migration dry run");
            var result = await _contextMigrationService.MigrateToContextsAsync(dryRun: true);
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data migration dry run");
            return StatusCode(500, "Error during data migration dry run");
        }
    }

    /// <summary>
    /// Perform actual data migration
    /// </summary>
    [HttpPost("data/execute")]
    public async Task<ActionResult<MigrationResult>> ExecuteDataMigration()
    {
        try
        {
            _logger.LogInformation("Starting actual data migration");
            
            // Validate readiness first
            var readiness = await _contextMigrationService.ValidateMigrationReadinessAsync();
            if (!readiness.IsReady)
            {
                return BadRequest(new
                {
                    Error = "Migration not ready",
                    Readiness = readiness
                });
            }

            var result = await _contextMigrationService.MigrateToContextsAsync(dryRun: false);
            
            if (result.IsSuccessful)
            {
                _logger.LogInformation("Data migration completed successfully");
            }
            else
            {
                _logger.LogError("Data migration failed with {ErrorCount} errors", result.Errors.Count);
            }
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during data migration execution");
            return StatusCode(500, "Error during data migration execution");
        }
    }

    /// <summary>
    /// Get service migration recommendations
    /// </summary>
    [HttpGet("services/recommendations")]
    public ActionResult<object> GetServiceMigrationRecommendations()
    {
        try
        {
            var services = new[]
            {
                new { Name = "QueryService", Entities = new[] { "QueryHistory", "QueryCache", "QueryPerformance" } },
                new { Name = "TuningService", Entities = new[] { "BusinessTableInfo", "BusinessColumnInfo", "QueryPatterns", "BusinessGlossary" } },
                new { Name = "UserService", Entities = new[] { "Users", "UserSessions", "UserPreferences" } },
                new { Name = "AuditService", Entities = new[] { "AuditLog" } },
                new { Name = "SchemaService", Entities = new[] { "SchemaMetadata", "BusinessSchemas" } },
                new { Name = "PromptManagementService", Entities = new[] { "PromptTemplates", "PromptLogs" } }
            };

            var recommendations = services.Select(s => 
                _serviceMigrationHelper.GetMigrationRecommendation(s.Name, s.Entities)).ToList();

            return Ok(new
            {
                Recommendations = recommendations,
                Summary = new
                {
                    TotalServices = recommendations.Count,
                    SimpleMigrations = recommendations.Count(r => r.MigrationComplexity == MigrationComplexity.Simple),
                    MediumMigrations = recommendations.Count(r => r.MigrationComplexity == MigrationComplexity.Medium),
                    ComplexMigrations = recommendations.Count(r => r.MigrationComplexity == MigrationComplexity.Complex)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting service migration recommendations");
            return StatusCode(500, "Error getting service migration recommendations");
        }
    }

    /// <summary>
    /// Generate migration template for a service
    /// </summary>
    [HttpGet("services/{serviceName}/template")]
    public ActionResult<object> GenerateMigrationTemplate(string serviceName, [FromQuery] string[] entities)
    {
        try
        {
            if (entities == null || entities.Length == 0)
            {
                return BadRequest("Entities parameter is required");
            }

            var recommendation = _serviceMigrationHelper.GetMigrationRecommendation(serviceName, entities);
            var template = _serviceMigrationHelper.GenerateMigrationTemplate(recommendation);

            return Ok(new
            {
                ServiceName = serviceName,
                Recommendation = recommendation,
                Template = template
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating migration template for service {ServiceName}", serviceName);
            return StatusCode(500, "Error generating migration template");
        }
    }

    /// <summary>
    /// Get comprehensive migration progress and status
    /// </summary>
    [HttpGet("status")]
    public async Task<ActionResult<ComprehensiveMigrationStatus>> GetMigrationStatus()
    {
        try
        {
            var status = await _statusTracker.GetComprehensiveMigrationStatusAsync();
            return Ok(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting comprehensive migration status");
            return StatusCode(500, "Error getting migration status");
        }
    }

    /// <summary>
    /// Get recommended next actions for migration
    /// </summary>
    [HttpGet("recommendations")]
    public async Task<ActionResult<object>> GetRecommendations()
    {
        try
        {
            var actions = await _statusTracker.GetRecommendedActionsAsync();
            var status = await _statusTracker.GetComprehensiveMigrationStatusAsync();

            return Ok(new
            {
                CurrentPhase = status.CurrentPhase,
                Progress = status.OverallProgress,
                RecommendedActions = actions,
                IsComplete = status.IsComplete,
                NextMilestone = GetNextMilestone(status)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration recommendations");
            return StatusCode(500, "Error getting recommendations");
        }
    }

    /// <summary>
    /// Test bounded context operations
    /// </summary>
    [HttpPost("test/contexts")]
    public async Task<ActionResult<object>> TestBoundedContexts()
    {
        try
        {
            var results = new Dictionary<string, object>();

            foreach (ContextType contextType in Enum.GetValues<ContextType>())
            {
                if (contextType == ContextType.Legacy) continue;

                try
                {
                    var testResult = await _contextFactory.ExecuteWithContextAsync(contextType, async context =>
                    {
                        var canConnect = await context.Database.CanConnectAsync();
                        var entityCount = context.Model.GetEntityTypes().Count();
                        
                        return new
                        {
                            CanConnect = canConnect,
                            EntityCount = entityCount,
                            ContextType = context.GetType().Name
                        };
                    });

                    results[contextType.ToString()] = new
                    {
                        Success = true,
                        Result = testResult
                    };
                }
                catch (Exception ex)
                {
                    results[contextType.ToString()] = new
                    {
                        Success = false,
                        Error = ex.Message
                    };
                }
            }

            return Ok(new
            {
                TestResults = results,
                Summary = new
                {
                    TotalContexts = results.Count,
                    SuccessfulTests = results.Count(kvp => ((dynamic)kvp.Value).Success),
                    FailedTests = results.Count(kvp => !((dynamic)kvp.Value).Success)
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing bounded contexts");
            return StatusCode(500, "Error testing bounded contexts");
        }
    }

    private string DetermineMigrationPhase(Dictionary<ContextType, bool> contextHealth, ServiceMigrationStatus serviceStatus)
    {
        if (!serviceStatus.ContextFactoryHealthy || contextHealth.Values.Any(h => !h))
        {
            return "Phase 0 - Setup";
        }

        if (serviceStatus.MigrationReady && contextHealth.Values.All(h => h))
        {
            return "Phase 2 - Service Migration";
        }

        return "Phase 1 - Bounded Contexts Ready";
    }

    private string GetNextMilestone(ComprehensiveMigrationStatus status)
    {
        if (!status.Phase1Status.IsComplete)
            return "Complete bounded contexts setup";

        if (!status.Phase2Status.IsComplete)
            return $"Migrate remaining {status.Phase2Status.RemainingServices.Count} services";

        if (!status.Phase3Status.IsComplete)
            return "Deprecate legacy context";

        return "Migration complete - optimize and monitor";
    }
}
