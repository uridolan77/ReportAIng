using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.AI.Management;
using BIReportingCopilot.Infrastructure.AI.Providers;

// Simple test to check if circular dependency is resolved
var services = new ServiceCollection();

// Add logging
services.AddLogging(builder => builder.AddConsole());

// Add required services
services.AddScoped<ICacheService, MockCacheService>();
services.AddScoped<IDbContextFactory, MockDbContextFactory>();

// Try to register the services that were causing circular dependency
try 
{
    services.AddScoped<ILLMManagementService, LLMManagementService>();
    services.AddScoped<IAIProviderFactory, AIProviderFactory>();
    
    var provider = services.BuildServiceProvider();
    
    // Try to resolve the services
    var llmService = provider.GetRequiredService<ILLMManagementService>();
    var aiFactory = provider.GetRequiredService<IAIProviderFactory>();
    
    Console.WriteLine("✅ Circular dependency resolved! Services can be created successfully.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Circular dependency still exists: {ex.Message}");
}

// Mock implementations for testing
public class MockCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key) => Task.FromResult(default(T));
    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) => Task.CompletedTask;
    public Task RemoveAsync(string key) => Task.CompletedTask;
    public Task RemoveByPatternAsync(string pattern) => Task.CompletedTask;
}

public class MockDbContextFactory : IDbContextFactory
{
    // Implement required methods as no-ops for testing
    public SecurityDbContext CreateSecurityContext() => throw new NotImplementedException();
    public TuningDbContext CreateTuningContext() => throw new NotImplementedException();
    public QueryDbContext CreateQueryContext() => throw new NotImplementedException();
    public SchemaDbContext CreateSchemaContext() => throw new NotImplementedException();
    public MonitoringDbContext CreateMonitoringContext() => throw new NotImplementedException();
    public BICopilotContext CreateLegacyContext() => throw new NotImplementedException();
    public DbContext GetContextForOperation(ContextType contextType) => throw new NotImplementedException();
    public Task<T> ExecuteWithContextAsync<T>(ContextType contextType, Func<DbContext, Task<T>> operation) => Task.FromResult(default(T)!);
    public Task ExecuteWithContextAsync(ContextType contextType, Func<DbContext, Task> operation) => Task.CompletedTask;
    public Task<T> ExecuteWithMultipleContextsAsync<T>(IEnumerable<ContextType> contextTypes, Func<Dictionary<ContextType, DbContext>, Task<T>> operation) => Task.FromResult(default(T)!);
    public Task<ContextValidationResult> ValidateAllContextsAsync() => Task.FromResult(new ContextValidationResult());
    public Task<Dictionary<ContextType, bool>> GetConnectionHealthAsync() => Task.FromResult(new Dictionary<ContextType, bool>());
}
