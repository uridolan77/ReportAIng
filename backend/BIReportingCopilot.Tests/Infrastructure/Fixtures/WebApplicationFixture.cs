using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.API;
using Xunit;

namespace BIReportingCopilot.Tests.Infrastructure.Fixtures;

/// <summary>
/// Web application factory for integration testing
/// </summary>
public class WebApplicationFixture : WebApplicationFactory<Program>, IDisposable
{
    private readonly string _databaseName;
    private readonly Dictionary<string, string> _testConfiguration;

    public WebApplicationFixture()
    {
        _databaseName = $"TestDb_{Guid.NewGuid():N}";
        _testConfiguration = CreateTestConfiguration();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Clear existing configuration
            config.Sources.Clear();

            // Add test configuration
            config.AddInMemoryCollection(_testConfiguration);
        });

        builder.ConfigureServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BICopilotContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add in-memory database for testing
            services.AddDbContext<BICopilotContext>(options =>
            {
                options.UseInMemoryDatabase(_databaseName);
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
            });

            // Override services for testing
            ConfigureTestServices(services);
        });

        builder.UseEnvironment("Testing");

        // Configure logging for tests
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Warning); // Reduce noise in tests
        });
    }

    protected virtual void ConfigureTestServices(IServiceCollection services)
    {
        // Override services here if needed for specific test scenarios
        // For example, replace external dependencies with mocks
    }

    protected virtual Dictionary<string, string> CreateTestConfiguration()
    {
        return new Dictionary<string, string>
        {
            ["ConnectionStrings:DefaultConnection"] = $"Data Source=:memory:;Database={_databaseName}",
            ["AI:Provider"] = "Mock",
            ["AI:OpenAI:ApiKey"] = "test-api-key",
            ["AI:OpenAI:Model"] = "gpt-3.5-turbo",
            ["AI:OpenAI:BaseUrl"] = "https://api.openai.com/v1",
            ["AI:AzureOpenAI:ApiKey"] = "test-azure-key",
            ["AI:AzureOpenAI:Endpoint"] = "https://test.openai.azure.com",
            ["AI:AzureOpenAI:DeploymentName"] = "test-deployment",
            ["Authentication:Jwt:Key"] = "test-jwt-key-that-is-long-enough-for-hmac-sha256",
            ["Authentication:Jwt:Issuer"] = "test-issuer",
            ["Authentication:Jwt:Audience"] = "test-audience",
            ["Authentication:Jwt:ExpiryMinutes"] = "60",
            ["Caching:DefaultExpiryMinutes"] = "60",
            ["Caching:Redis:ConnectionString"] = "",
            ["EventBus:ConnectionString"] = "memory://localhost",
            ["EventBus:ExchangeName"] = "test-events",
            ["Logging:LogLevel:Default"] = "Warning",
            ["Logging:LogLevel:Microsoft"] = "Warning",
            ["Logging:LogLevel:Microsoft.Hosting.Lifetime"] = "Information"
        };
    }

    /// <summary>
    /// Create an authenticated HTTP client for testing
    /// </summary>
    public async Task<HttpClient> CreateAuthenticatedClientAsync(string userId = "test-user", string[] roles = null!)
    {
        var client = CreateClient();
        var token = await AuthenticateAsync(userId, roles);
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    /// <summary>
    /// Generate a JWT token for testing
    /// </summary>
    public async Task<string> AuthenticateAsync(string userId = "test-user", string[] roles = null!)
    {
        roles ??= new[] { "User" };

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_testConfiguration["Authentication:Jwt:Key"]);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, $"Test User {userId}"),
            new(ClaimTypes.Email, $"{userId}@test.com"),
            new("sub", userId),
            new("jti", Guid.NewGuid().ToString())
        };

        // Add role claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            Issuer = _testConfiguration["Authentication:Jwt:Issuer"],
            Audience = _testConfiguration["Authentication:Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    /// <summary>
    /// Seed the test database with initial data
    /// </summary>
    public async Task SeedDatabaseAsync(Action<BICopilotContext> seedAction)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BICopilotContext>();

        await context.Database.EnsureCreatedAsync();
        seedAction(context);
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Clear all data from the test database
    /// </summary>
    public async Task ClearDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BICopilotContext>();

        // Remove all entities in reverse dependency order
        context.AIFeedbackEntries.RemoveRange(context.AIFeedbackEntries);
        context.AIGenerationAttempts.RemoveRange(context.AIGenerationAttempts);
        context.SemanticCacheEntries.RemoveRange(context.SemanticCacheEntries);
        context.QueryHistories.RemoveRange(context.QueryHistories);
        context.BusinessColumnInfo.RemoveRange(context.BusinessColumnInfo);
        context.BusinessTableInfo.RemoveRange(context.BusinessTableInfo);
        context.QueryPatterns.RemoveRange(context.QueryPatterns);
        context.BusinessGlossary.RemoveRange(context.BusinessGlossary);
        context.UserSessions.RemoveRange(context.UserSessions);
        context.RefreshTokens.RemoveRange(context.RefreshTokens);
        context.MfaChallenges.RemoveRange(context.MfaChallenges);
        context.Users.RemoveRange(context.Users);
        context.QueryPerformance.RemoveRange(context.QueryPerformance);
        context.SystemMetrics.RemoveRange(context.SystemMetrics);
        context.AuditLog.RemoveRange(context.AuditLog);
        context.SystemConfiguration.RemoveRange(context.SystemConfiguration);

        await context.SaveChangesAsync();
    }

    /// <summary>
    /// Execute a test within a database transaction that gets rolled back
    /// </summary>
    public async Task ExecuteInTransactionAsync(Func<BICopilotContext, Task> testAction)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BICopilotContext>();

        using var transaction = await context.Database.BeginTransactionAsync();
        try
        {
            await testAction(context);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }

    /// <summary>
    /// Get a service from the test application's DI container
    /// </summary>
    public T GetService<T>() where T : notnull
    {
        return Services.GetRequiredService<T>();
    }

    /// <summary>
    /// Get a scoped service from the test application's DI container
    /// </summary>
    public T GetScopedService<T>() where T : notnull
    {
        using var scope = Services.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Clean up test database
            try
            {
                using var scope = Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<BICopilotContext>();
                context.Database.EnsureDeleted();
            }
            catch (Exception ex)
            {
                // Log but don't throw during disposal
                Console.WriteLine($"Error cleaning up test database: {ex.Message}");
            }
        }

        base.Dispose(disposing);
    }
}

/// <summary>
/// Specialized fixture for testing with specific mock configurations
/// </summary>
public class MockedWebApplicationFixture : WebApplicationFixture
{
    private readonly Action<IServiceCollection> _serviceConfiguration;

    public MockedWebApplicationFixture(Action<IServiceCollection> serviceConfiguration)
    {
        _serviceConfiguration = serviceConfiguration;
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
        base.ConfigureTestServices(services);
        _serviceConfiguration(services);
    }
}

/// <summary>
/// Collection definition for sharing web application fixture across test classes
/// </summary>
[CollectionDefinition("WebApplication")]
public class WebApplicationCollection : ICollectionFixture<WebApplicationFixture>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

/// <summary>
/// Helper for creating test HTTP requests
/// </summary>
public static class HttpRequestHelper
{
    public static StringContent CreateJsonContent<T>(T content)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(content, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });

        return new StringContent(json, Encoding.UTF8, "application/json");
    }

    public static async Task<T?> ReadJsonResponseAsync<T>(HttpResponseMessage response)
    {
        var json = await response.Content.ReadAsStringAsync();

        return System.Text.Json.JsonSerializer.Deserialize<T>(json, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
    }

    public static void AddAuthorizationHeader(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }
}
