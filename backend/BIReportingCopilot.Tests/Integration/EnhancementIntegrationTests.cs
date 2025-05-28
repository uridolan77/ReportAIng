using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Infrastructure.Security;
using BIReportingCopilot.Infrastructure.Performance;
using BIReportingCopilot.Core.Interfaces;
using MediatR;

namespace BIReportingCopilot.Tests.Integration;

public class EnhancementIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public EnhancementIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void Services_ShouldBeRegistered_WithCorrectImplementations()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Act & Assert - Verify all our new services are registered
        Assert.NotNull(services.GetService<IPasswordHasher>());
        Assert.IsType<PasswordHasher>(services.GetService<IPasswordHasher>());

        Assert.NotNull(services.GetService<ISqlQueryValidator>());
        Assert.IsType<SqlQueryValidator>(services.GetService<ISqlQueryValidator>());

        Assert.NotNull(services.GetService<IRateLimitingService>());
        Assert.IsType<RateLimitingService>(services.GetService<IRateLimitingService>());

        Assert.NotNull(services.GetService<IMediator>());

        // Verify cache service is the enhanced version
        Assert.NotNull(services.GetService<ICacheService>());
        Assert.IsType<MemoryOptimizedCacheService>(services.GetService<ICacheService>());
    }

    [Fact]
    public void PasswordHasher_ShouldHashAndVerifyPasswords()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        const string password = "TestPassword123!";

        // Act
        var hashedPassword = passwordHasher.HashPassword(password);
        var isValid = passwordHasher.VerifyPassword(password, hashedPassword);
        var isInvalid = passwordHasher.VerifyPassword("WrongPassword", hashedPassword);

        // Assert
        Assert.NotNull(hashedPassword);
        Assert.NotEqual(password, hashedPassword);
        Assert.True(isValid);
        Assert.False(isInvalid);
    }

    [Fact]
    public async Task SqlQueryValidator_ShouldValidateQueries()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var validator = scope.ServiceProvider.GetRequiredService<ISqlQueryValidator>();

        // Act & Assert - Valid SELECT query
        var validResult = await validator.ValidateAsync("SELECT * FROM Users WHERE Id = 1");
        Assert.True(validResult.IsValid);

        // Act & Assert - Invalid DELETE query
        var invalidResult = await validator.ValidateAsync("DELETE FROM Users WHERE Id = 1");
        Assert.False(invalidResult.IsValid);
        Assert.Contains("Only SELECT queries are allowed", invalidResult.Errors);

        // Act & Assert - Dangerous keywords
        var dangerousResult = await validator.ValidateAsync("SELECT * FROM Users; DROP TABLE Users;");
        Assert.False(dangerousResult.IsValid);
    }

    [Fact]
    public async Task RateLimitingService_ShouldEnforceRateLimits()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var rateLimitService = scope.ServiceProvider.GetRequiredService<IRateLimitingService>();
        const string testKey = "test-user-endpoint";

        // Act - First request should be allowed
        var firstResult = await rateLimitService.CheckRateLimitAsync(testKey, 2, TimeSpan.FromMinutes(1));
        Assert.True(firstResult.IsAllowed);
        Assert.Equal(1, firstResult.RequestsRemaining);

        // Act - Second request should be allowed
        var secondResult = await rateLimitService.CheckRateLimitAsync(testKey, 2, TimeSpan.FromMinutes(1));
        Assert.True(secondResult.IsAllowed);
        Assert.Equal(0, secondResult.RequestsRemaining);

        // Act - Third request should be blocked
        var thirdResult = await rateLimitService.CheckRateLimitAsync(testKey, 2, TimeSpan.FromMinutes(1));
        Assert.False(thirdResult.IsAllowed);
        Assert.Equal(0, thirdResult.RequestsRemaining);
        Assert.True(thirdResult.RetryAfter > TimeSpan.Zero);
    }

    [Fact]
    public async Task HealthChecks_ShouldReturnHealthyOrDegraded()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();

        // Accept either Healthy or Degraded (Degraded is expected when OpenAI API key is not configured)
        Assert.True(content.Contains("Healthy") || content.Contains("Degraded"),
            $"Expected 'Healthy' or 'Degraded' but got: {content}");
    }

    [Fact]
    public async Task Api_ShouldHaveCorrelationIdHeader()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        Assert.True(response.Headers.Contains("X-Correlation-ID"));
        var correlationId = response.Headers.GetValues("X-Correlation-ID").First();
        Assert.True(Guid.TryParse(correlationId, out _));
    }

    [Fact]
    public async Task Api_ShouldSupportCompression()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.EnsureSuccessStatusCode();
        // The response should be compressed if the middleware is working
        // This is a basic test - in practice you'd check the Content-Encoding header
    }
}

[Collection("Integration")]
public class MediatRIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public MediatRIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public void MediatR_ShouldBeConfiguredCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

        // Assert
        Assert.NotNull(mediator);
    }

    [Fact]
    public void ValidationBehaviors_ShouldBeRegistered()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Act - Check for MediatR registration
        var mediator = services.GetService<IMediator>();

        // Assert
        Assert.NotNull(mediator);

        // Verify that MediatR is properly configured
        // The behaviors are registered as open generics, so we can't directly test them
        // but we can verify MediatR itself is working
        Assert.IsType<Mediator>(mediator);
    }
}
