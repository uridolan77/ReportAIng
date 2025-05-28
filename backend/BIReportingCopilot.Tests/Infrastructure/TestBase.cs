using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Xunit;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Tests.Infrastructure.Builders;
using BIReportingCopilot.Tests.Infrastructure.Fixtures;

namespace BIReportingCopilot.Tests.Infrastructure;

/// <summary>
/// Base class for all unit tests with common setup and utilities
/// </summary>
public abstract class TestBase : IDisposable
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly ILogger Logger;
    protected readonly IConfiguration Configuration;
    protected readonly IMemoryCache MemoryCache;
    private readonly ServiceCollection _services;
    private bool _disposed;

    protected TestBase()
    {
        _services = new ServiceCollection();
        Configuration = CreateTestConfiguration();

        ConfigureServices(_services);
        ServiceProvider = _services.BuildServiceProvider();

        Logger = ServiceProvider.GetRequiredService<ILogger<TestBase>>();
        MemoryCache = ServiceProvider.GetRequiredService<IMemoryCache>();
    }

    protected virtual void ConfigureServices(IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Add configuration
        services.AddSingleton(Configuration);

        // Add memory cache
        services.AddMemoryCache();

        // Add test-specific services
        services.AddScoped(typeof(TestDataBuilder<>));
        services.AddScoped<MockServiceBuilder>();
    }

    protected virtual IConfiguration CreateTestConfiguration()
    {
        var configBuilder = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                ["AI:Provider"] = "Mock",
                ["AI:OpenAI:ApiKey"] = "test-key",
                ["AI:OpenAI:Model"] = "gpt-3.5-turbo",
                ["Caching:DefaultExpiryMinutes"] = "60",
                ["EventBus:ConnectionString"] = "memory://localhost",
                ["Logging:LogLevel:Default"] = "Debug"
            });

        return configBuilder.Build();
    }

    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }

    protected T? GetOptionalService<T>() where T : class
    {
        return ServiceProvider.GetService<T>();
    }

    protected ILogger<T> GetLogger<T>()
    {
        return ServiceProvider.GetRequiredService<ILogger<T>>();
    }

    /// <summary>
    /// Create a test data builder for the specified entity type
    /// </summary>
    protected TestDataBuilder<T> CreateBuilder<T>() where T : class, new()
    {
        return new TestDataBuilder<T>();
    }

    /// <summary>
    /// Create multiple test entities using a builder
    /// </summary>
    protected List<T> CreateMany<T>(int count, Action<TestDataBuilder<T>>? configure = null) where T : class, new()
    {
        var builder = CreateBuilder<T>();
        configure?.Invoke(builder);

        return Enumerable.Range(0, count)
            .Select(_ => builder.Build())
            .ToList();
    }

    /// <summary>
    /// Assert that an async operation throws a specific exception
    /// </summary>
    protected async Task<TException> AssertThrowsAsync<TException>(Func<Task> operation)
        where TException : Exception
    {
        try
        {
            await operation();
            throw new Xunit.Sdk.XunitException($"Expected {typeof(TException).Name} but no exception was thrown");
        }
        catch (TException ex)
        {
            return ex;
        }
        catch (Exception ex)
        {
            throw new Xunit.Sdk.XunitException($"Expected {typeof(TException).Name} but got {ex.GetType().Name}: {ex.Message}");
        }
    }

    /// <summary>
    /// Assert that a condition becomes true within a timeout period
    /// </summary>
    protected async Task AssertEventuallyAsync(Func<bool> condition, TimeSpan? timeout = null, TimeSpan? interval = null)
    {
        var timeoutValue = timeout ?? TimeSpan.FromSeconds(5);
        var intervalValue = interval ?? TimeSpan.FromMilliseconds(100);
        var endTime = DateTime.UtcNow.Add(timeoutValue);

        while (DateTime.UtcNow < endTime)
        {
            if (condition())
                return;

            await Task.Delay(intervalValue);
        }

        throw new Xunit.Sdk.XunitException($"Condition was not met within {timeoutValue.TotalSeconds} seconds");
    }

    /// <summary>
    /// Assert that an async condition becomes true within a timeout period
    /// </summary>
    protected async Task AssertEventuallyAsync(Func<Task<bool>> condition, TimeSpan? timeout = null, TimeSpan? interval = null)
    {
        var timeoutValue = timeout ?? TimeSpan.FromSeconds(5);
        var intervalValue = interval ?? TimeSpan.FromMilliseconds(100);
        var endTime = DateTime.UtcNow.Add(timeoutValue);

        while (DateTime.UtcNow < endTime)
        {
            if (await condition())
                return;

            await Task.Delay(intervalValue);
        }

        throw new Xunit.Sdk.XunitException($"Async condition was not met within {timeoutValue.TotalSeconds} seconds");
    }

    /// <summary>
    /// Create a cancellation token that cancels after the specified timeout
    /// </summary>
    protected CancellationToken CreateTimeoutToken(TimeSpan timeout)
    {
        var cts = new CancellationTokenSource(timeout);
        return cts.Token;
    }

    /// <summary>
    /// Capture logs during test execution
    /// </summary>
    protected TestLogCapture CaptureLogsFor<T>()
    {
        return new TestLogCapture(ServiceProvider.GetRequiredService<ILoggerFactory>(), typeof(T));
    }

    public virtual void Dispose()
    {
        if (_disposed)
            return;

        try
        {
            if (ServiceProvider is IDisposable disposableProvider)
            {
                disposableProvider.Dispose();
            }
        }
        catch (Exception ex)
        {
            // Log disposal errors but don't throw
            Console.WriteLine($"Error disposing test base: {ex.Message}");
        }

        _disposed = true;
    }
}

/// <summary>
/// Base class for integration tests with database context
/// </summary>
public abstract class IntegrationTestBase : TestBase
{
    protected readonly BICopilotContext DbContext;
    private readonly string _databaseName;

    protected IntegrationTestBase()
    {
        _databaseName = $"TestDb_{Guid.NewGuid():N}";
        DbContext = CreateDbContext();
        DbContext.Database.EnsureCreated();
    }

    protected override void ConfigureServices(IServiceCollection services)
    {
        base.ConfigureServices(services);

        // Add Entity Framework with in-memory database
        services.AddDbContext<BICopilotContext>(options =>
        {
            options.UseInMemoryDatabase(_databaseName);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });
    }

    protected virtual BICopilotContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(_databaseName)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        return new BICopilotContext(options);
    }

    /// <summary>
    /// Seed the database with test data
    /// </summary>
    protected async Task SeedDatabaseAsync(Action<BICopilotContext> seedAction)
    {
        seedAction(DbContext);
        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Clear all data from the database
    /// </summary>
    protected async Task ClearDatabaseAsync()
    {
        // Remove all entities in reverse dependency order
        DbContext.AIFeedbackEntries.RemoveRange(DbContext.AIFeedbackEntries);
        DbContext.AIGenerationAttempts.RemoveRange(DbContext.AIGenerationAttempts);
        DbContext.SemanticCacheEntries.RemoveRange(DbContext.SemanticCacheEntries);
        DbContext.QueryHistories.RemoveRange(DbContext.QueryHistories);
        DbContext.BusinessColumnInfo.RemoveRange(DbContext.BusinessColumnInfo);
        DbContext.BusinessTableInfo.RemoveRange(DbContext.BusinessTableInfo);
        DbContext.QueryPatterns.RemoveRange(DbContext.QueryPatterns);
        DbContext.BusinessGlossary.RemoveRange(DbContext.BusinessGlossary);
        DbContext.UserSessions.RemoveRange(DbContext.UserSessions);
        DbContext.RefreshTokens.RemoveRange(DbContext.RefreshTokens);
        DbContext.MfaChallenges.RemoveRange(DbContext.MfaChallenges);
        DbContext.Users.RemoveRange(DbContext.Users);
        DbContext.QueryPerformance.RemoveRange(DbContext.QueryPerformance);
        DbContext.SystemMetrics.RemoveRange(DbContext.SystemMetrics);
        DbContext.AuditLog.RemoveRange(DbContext.AuditLog);
        DbContext.SystemConfiguration.RemoveRange(DbContext.SystemConfiguration);

        await DbContext.SaveChangesAsync();
    }

    /// <summary>
    /// Execute a test within a database transaction that gets rolled back
    /// </summary>
    protected async Task ExecuteInTransactionAsync(Func<BICopilotContext, Task> testAction)
    {
        using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            await testAction(DbContext);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    public override void Dispose()
    {
        try
        {
            DbContext?.Database.EnsureDeleted();
            DbContext?.Dispose();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error disposing integration test base: {ex.Message}");
        }

        base.Dispose();
    }
}

/// <summary>
/// Base class for API integration tests
/// </summary>
public abstract class ApiIntegrationTestBase : IntegrationTestBase, IClassFixture<WebApplicationFixture>
{
    protected readonly WebApplicationFixture WebAppFixture;
    protected readonly HttpClient HttpClient;

    protected ApiIntegrationTestBase(WebApplicationFixture webAppFixture)
    {
        WebAppFixture = webAppFixture;
        HttpClient = webAppFixture.CreateClient();
    }

    /// <summary>
    /// Authenticate as a test user and return the authorization header value
    /// </summary>
    protected async Task<string> AuthenticateAsTestUserAsync(string userId = "test-user", string[] roles = null!)
    {
        roles ??= new[] { "User" };
        return await WebAppFixture.AuthenticateAsync(userId, roles);
    }

    /// <summary>
    /// Set authorization header for HTTP client
    /// </summary>
    protected void SetAuthorizationHeader(string token)
    {
        HttpClient.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Send a JSON POST request and return the response
    /// </summary>
    protected async Task<HttpResponseMessage> PostJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await HttpClient.PostAsync(requestUri, stringContent);
    }

    /// <summary>
    /// Send a JSON PUT request and return the response
    /// </summary>
    protected async Task<HttpResponseMessage> PutJsonAsync<T>(string requestUri, T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content);
        var stringContent = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        return await HttpClient.PutAsync(requestUri, stringContent);
    }

    /// <summary>
    /// Get and deserialize JSON response
    /// </summary>
    protected async Task<T?> GetJsonAsync<T>(string requestUri)
    {
        var response = await HttpClient.GetAsync(requestUri);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public override void Dispose()
    {
        HttpClient?.Dispose();
        base.Dispose();
    }
}
