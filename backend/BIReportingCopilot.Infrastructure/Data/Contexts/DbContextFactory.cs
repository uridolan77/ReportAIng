using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Configuration;

namespace BIReportingCopilot.Infrastructure.Data.Contexts;

/// <summary>
/// Factory for managing bounded database contexts with proper lifecycle management
/// </summary>
public class DbContextFactory : IDbContextFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly UnifiedConfigurationService _configurationService;
    private readonly ILogger<DbContextFactory> _logger;

    public DbContextFactory(
        IServiceProvider serviceProvider,
        UnifiedConfigurationService configurationService,
        ILogger<DbContextFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _configurationService = configurationService;
        _logger = logger;
    }

    /// <summary>
    /// Create SecurityDbContext for user and authentication operations
    /// </summary>
    public SecurityDbContext CreateSecurityContext()
    {
        try
        {
            var context = _serviceProvider.GetRequiredService<SecurityDbContext>();
            _logger.LogDebug("Created SecurityDbContext");
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SecurityDbContext");
            throw;
        }
    }

    /// <summary>
    /// Create TuningDbContext for AI tuning and business intelligence configuration
    /// </summary>
    public TuningDbContext CreateTuningContext()
    {
        try
        {
            var context = _serviceProvider.GetRequiredService<TuningDbContext>();
            _logger.LogDebug("Created TuningDbContext");
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating TuningDbContext");
            throw;
        }
    }

    /// <summary>
    /// Create QueryDbContext for query execution and caching
    /// </summary>
    public QueryDbContext CreateQueryContext()
    {
        try
        {
            var context = _serviceProvider.GetRequiredService<QueryDbContext>();
            _logger.LogDebug("Created QueryDbContext");
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating QueryDbContext");
            throw;
        }
    }

    /// <summary>
    /// Create SchemaDbContext for schema management and metadata
    /// </summary>
    public SchemaDbContext CreateSchemaContext()
    {
        try
        {
            var context = _serviceProvider.GetRequiredService<SchemaDbContext>();
            _logger.LogDebug("Created SchemaDbContext");
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SchemaDbContext");
            throw;
        }
    }

    /// <summary>
    /// Create MonitoringDbContext for system monitoring and analytics
    /// </summary>
    public MonitoringDbContext CreateMonitoringContext()
    {
        try
        {
            var context = _serviceProvider.GetRequiredService<MonitoringDbContext>();
            _logger.LogDebug("Created MonitoringDbContext");
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MonitoringDbContext");
            throw;
        }
    }

    /// <summary>
    /// Create the legacy BICopilotContext for backward compatibility
    /// </summary>
    public BICopilotContext CreateLegacyContext()
    {
        try
        {
            var context = _serviceProvider.GetRequiredService<BICopilotContext>();
            _logger.LogDebug("Created legacy BICopilotContext");
            return context;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating legacy BICopilotContext");
            throw;
        }
    }

    /// <summary>
    /// Get appropriate context based on operation type
    /// </summary>
    public DbContext GetContextForOperation(ContextType contextType)
    {
        return contextType switch
        {
            ContextType.Security => CreateSecurityContext(),
            ContextType.Tuning => CreateTuningContext(),
            ContextType.Query => CreateQueryContext(),
            ContextType.Schema => CreateSchemaContext(),
            ContextType.Monitoring => CreateMonitoringContext(),
            ContextType.Legacy => CreateLegacyContext(),
            _ => throw new ArgumentException($"Unknown context type: {contextType}")
        };
    }

    /// <summary>
    /// Execute operation with appropriate context and automatic disposal
    /// </summary>
    public async Task<T> ExecuteWithContextAsync<T>(ContextType contextType, Func<DbContext, Task<T>> operation)
    {
        using var context = GetContextForOperation(contextType);
        try
        {
            return await operation(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing operation with {ContextType} context", contextType);
            throw;
        }
    }

    /// <summary>
    /// Execute operation with appropriate context and automatic disposal (void return)
    /// </summary>
    public async Task ExecuteWithContextAsync(ContextType contextType, Func<DbContext, Task> operation)
    {
        using var context = GetContextForOperation(contextType);
        try
        {
            await operation(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing operation with {ContextType} context", contextType);
            throw;
        }
    }

    /// <summary>
    /// Execute operation with multiple contexts (for cross-context operations)
    /// </summary>
    public async Task<T> ExecuteWithMultipleContextsAsync<T>(
        IEnumerable<ContextType> contextTypes, 
        Func<Dictionary<ContextType, DbContext>, Task<T>> operation)
    {
        var contexts = new Dictionary<ContextType, DbContext>();
        
        try
        {
            foreach (var contextType in contextTypes)
            {
                contexts[contextType] = GetContextForOperation(contextType);
            }

            return await operation(contexts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error executing multi-context operation");
            throw;
        }
        finally
        {
            foreach (var context in contexts.Values)
            {
                context.Dispose();
            }
        }
    }

    /// <summary>
    /// Validate all contexts can be created successfully
    /// </summary>
    public async Task<ContextValidationResult> ValidateAllContextsAsync()
    {
        var result = new ContextValidationResult();
        var contextTypes = Enum.GetValues<ContextType>();

        foreach (var contextType in contextTypes)
        {
            try
            {
                using var context = GetContextForOperation(contextType);
                await context.Database.CanConnectAsync();
                result.SuccessfulContexts.Add(contextType);
            }
            catch (Exception ex)
            {
                result.FailedContexts.Add(contextType, ex.Message);
                result.IsValid = false;
            }
        }

        return result;
    }

    /// <summary>
    /// Get connection health for all contexts
    /// </summary>
    public async Task<Dictionary<ContextType, bool>> GetConnectionHealthAsync()
    {
        var health = new Dictionary<ContextType, bool>();
        var contextTypes = Enum.GetValues<ContextType>();

        foreach (var contextType in contextTypes)
        {
            try
            {
                using var context = GetContextForOperation(contextType);
                health[contextType] = await context.Database.CanConnectAsync();
            }
            catch
            {
                health[contextType] = false;
            }
        }

        return health;
    }
}

/// <summary>
/// Interface for the DbContext factory
/// </summary>
public interface IDbContextFactory
{
    SecurityDbContext CreateSecurityContext();
    TuningDbContext CreateTuningContext();
    QueryDbContext CreateQueryContext();
    SchemaDbContext CreateSchemaContext();
    MonitoringDbContext CreateMonitoringContext();
    BICopilotContext CreateLegacyContext();
    DbContext GetContextForOperation(ContextType contextType);
    Task<T> ExecuteWithContextAsync<T>(ContextType contextType, Func<DbContext, Task<T>> operation);
    Task ExecuteWithContextAsync(ContextType contextType, Func<DbContext, Task> operation);
    Task<T> ExecuteWithMultipleContextsAsync<T>(IEnumerable<ContextType> contextTypes, Func<Dictionary<ContextType, DbContext>, Task<T>> operation);
    Task<ContextValidationResult> ValidateAllContextsAsync();
    Task<Dictionary<ContextType, bool>> GetConnectionHealthAsync();
}

/// <summary>
/// Context types for bounded contexts
/// </summary>
public enum ContextType
{
    Security,
    Tuning,
    Query,
    Schema,
    Monitoring,
    Legacy
}

/// <summary>
/// Result of context validation
/// </summary>
public class ContextValidationResult
{
    public bool IsValid { get; set; } = true;
    public List<ContextType> SuccessfulContexts { get; set; } = new();
    public Dictionary<ContextType, string> FailedContexts { get; set; } = new();
    public DateTime ValidatedAt { get; set; } = DateTime.UtcNow;
}
