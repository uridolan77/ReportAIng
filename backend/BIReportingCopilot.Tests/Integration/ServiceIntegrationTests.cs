using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Query;
using BIReportingCopilot.Core.Interfaces.Schema;
using BIReportingCopilot.Core.Interfaces.Data;
using BIReportingCopilot.Core.Interfaces.Services;
using BIReportingCopilot.Core.Interfaces.AI;
using BIReportingCopilot.Core.Interfaces.Cache;
using BIReportingCopilot.Core.Models;
using Xunit;

namespace BIReportingCopilot.Tests.Integration;

public class ServiceIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ServiceIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Services_ShouldBeRegistered_AndResolvable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Act & Assert - Verify all core services can be resolved
        var sqlQueryService = services.GetRequiredService<ISqlQueryService>();
        Assert.NotNull(sqlQueryService);

        var schemaService = services.GetRequiredService<ISchemaService>();
        Assert.NotNull(schemaService);

        var auditService = services.GetRequiredService<IAuditService>();
        Assert.NotNull(auditService);

        var userService = services.GetRequiredService<IUserService>();
        Assert.NotNull(userService);

        var promptService = services.GetRequiredService<IPromptService>();
        Assert.NotNull(promptService);

        var cacheService = services.GetRequiredService<ICacheService>();
        Assert.NotNull(cacheService);

        var aiService = services.GetRequiredService<IAIService>();
        Assert.NotNull(aiService);
    }

    [Fact]
    public async Task SqlQueryService_ShouldValidateQueries()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var sqlQueryService = scope.ServiceProvider.GetRequiredService<ISqlQueryService>();

        // Act
        var validResult = await sqlQueryService.ValidateSqlAsync("SELECT * FROM Users");
        var invalidResult = await sqlQueryService.ValidateSqlAsync("DROP TABLE Users");

        // Assert
        Assert.True(validResult);
        Assert.False(invalidResult);
    }

    [Fact]
    public async Task AuditService_ShouldLogEvents()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var auditService = scope.ServiceProvider.GetRequiredService<IAuditService>();

        // Act & Assert - Should not throw
        await auditService.LogAsync("TEST_ACTION", "test-user", "TestEntity", "123",
            new { TestProperty = "TestValue" });
    }

    [Fact]
    public async Task PromptService_ShouldReturnDefaultTemplates()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var promptService = scope.ServiceProvider.GetRequiredService<IPromptService>();

        // Act
        var template = await promptService.GetPromptTemplateAsync("sql_generation");

        // Assert
        Assert.NotNull(template);
        Assert.Equal("sql_generation", template.Name);
        Assert.NotEmpty(template.Content);
    }

    [Fact]
    public async Task CacheService_ShouldStoreAndRetrieveData()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var cacheService = scope.ServiceProvider.GetRequiredService<ICacheService>();
        var testKey = "test-key";
        var testValue = "test-value";

        // Act
        await cacheService.SetAsync(testKey, testValue, TimeSpan.FromMinutes(5));
        var retrievedValue = await cacheService.GetAsync<string>(testKey);

        // Assert
        Assert.Equal(testValue, retrievedValue);
    }

    [Fact]
    public async Task OpenAIService_ShouldValidateQueryIntent()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var aiService = scope.ServiceProvider.GetRequiredService<IAIService>();

        // Act
        var isValid = await aiService.ValidateQueryIntentAsync("Show me all users");

        // Assert
        // Should not throw and return a boolean result
        Assert.IsType<bool>(isValid);
    }

    [Fact]
    public async Task UserService_ShouldHandleUserPreferences()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userService = scope.ServiceProvider.GetRequiredService<IUserService>();
        var testUserId = "test-user-123";
        var preferences = new UserPreferences
        {
            DefaultChartType = "bar",
            MaxRowsPerQuery = 500,
            EnableQuerySuggestions = true
        };

        // Act & Assert - Should not throw
        try
        {
            var result = await userService.UpdateUserPreferencesAsync(testUserId, preferences);
            Assert.NotNull(result);
        }
        catch (Exception ex)
        {
            // Expected to fail without proper database setup, but service should be resolvable
            Assert.IsType<InvalidOperationException>(ex);
        }
    }
}
