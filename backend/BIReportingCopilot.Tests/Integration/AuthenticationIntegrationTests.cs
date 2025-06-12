using BIReportingCopilot.Core;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace BIReportingCopilot.Tests.Integration;

public class AuthenticationIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Test");
            builder.ConfigureServices(services =>
            {
                // Remove the existing DbContext registration
                var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<BICopilotContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Remove the database initialization service for tests
                var dbInitDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDatabaseInitializationService));
                if (dbInitDescriptor != null)
                {
                    services.Remove(dbInitDescriptor);
                }

                // Remove Redis distributed cache for tests
                var redisCacheDescriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IDistributedCache));
                if (redisCacheDescriptor != null)
                {
                    services.Remove(redisCacheDescriptor);
                }

                // Remove MemoryOptimizedCacheService that depends on Redis
                var cacheServiceDescriptors = services.Where(d => d.ServiceType.Name.Contains("CacheService")).ToList();
                foreach (var cacheDescriptor in cacheServiceDescriptors)
                {
                    services.Remove(cacheDescriptor);
                }

                // Add in-memory database for testing
                services.AddDbContext<BICopilotContext>(options =>
                {
                    options.UseInMemoryDatabase($"TestDb_{Guid.NewGuid()}");
                });

                // Add only in-memory caching for tests
                services.AddMemoryCache();
                services.AddDistributedMemoryCache(); // This provides IDistributedCache using memory

                // Add a test-specific cache service that only uses memory cache
                services.AddScoped<ICacheService>(provider =>
                {
                    var mockCache = new Mock<ICacheService>();
                    mockCache.Setup(x => x.GetAsync<object>(It.IsAny<string>()))
                        .ReturnsAsync((object?)null);
                    mockCache.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()))
                        .Returns(Task.CompletedTask);
                    mockCache.Setup(x => x.RemoveAsync(It.IsAny<string>()))
                        .Returns(Task.CompletedTask);
                    mockCache.Setup(x => x.ExistsAsync(It.IsAny<string>()))
                        .ReturnsAsync(false);
                    mockCache.Setup(x => x.IncrementAsync(It.IsAny<string>(), It.IsAny<long>()))
                        .ReturnsAsync(1L);
                    return mockCache.Object;
                });

                // Add a test-specific database initialization service
                services.AddScoped<IDatabaseInitializationService, TestDatabaseInitializationService>();
            });
        });

        _client = _factory.CreateClient();
    }

    private static void SeedTestData(BICopilotContext context, IPasswordHasher passwordHasher)
    {
        // Create test admin user
        var adminUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-admin-001",
            Username = "testadmin",
            Email = "testadmin@test.com",
            DisplayName = "Test Administrator",
            PasswordHash = passwordHasher.HashPassword("TestAdmin123!"),
            Roles = ApplicationConstants.Roles.Admin,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        // Create test regular user
        var regularUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-user-001",
            Username = "testuser",
            Email = "testuser@test.com",
            DisplayName = "Test User",
            PasswordHash = passwordHasher.HashPassword("TestUser123!"),
            Roles = ApplicationConstants.Roles.User,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        // Create inactive user
        var inactiveUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-inactive-001",
            Username = "inactiveuser",
            Email = "inactive@test.com",
            DisplayName = "Inactive User",
            PasswordHash = passwordHasher.HashPassword("InactiveUser123!"),
            Roles = ApplicationConstants.Roles.User,
            IsActive = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.Users.AddRange(adminUser, regularUser, inactiveUser);
        context.SaveChanges();
    }

    private async Task SeedTestDataAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BICopilotContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        // Check if data already exists
        if (await context.Users.AnyAsync())
        {
            return;
        }

        // Create test admin user
        var adminUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-admin-001",
            Username = "testadmin",
            Email = "testadmin@test.com",
            DisplayName = "Test Administrator",
            PasswordHash = passwordHasher.HashPassword("TestAdmin123!"),
            Roles = ApplicationConstants.Roles.Admin,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        // Create test regular user
        var regularUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-user-001",
            Username = "testuser",
            Email = "testuser@test.com",
            DisplayName = "Test User",
            PasswordHash = passwordHasher.HashPassword("TestUser123!"),
            Roles = ApplicationConstants.Roles.User,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        // Create inactive user
        var inactiveUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-inactive-001",
            Username = "inactiveuser",
            Email = "inactive@test.com",
            DisplayName = "Inactive User",
            PasswordHash = passwordHasher.HashPassword("InactiveUser123!"),
            Roles = ApplicationConstants.Roles.User,
            IsActive = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        context.Users.AddRange(adminUser, regularUser, inactiveUser);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsSuccessWithToken()
    {
        // Arrange - Seed test data first
        await SeedTestDataAsync();

        var loginRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "TestAdmin123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(authResult);
        Assert.True(authResult.Success);
        Assert.NotNull(authResult.AccessToken);
        Assert.NotNull(authResult.RefreshToken);
        Assert.NotNull(authResult.User);
        Assert.Equal("testadmin", authResult.User.Username);
        Assert.Contains(ApplicationConstants.Roles.Admin, authResult.User.Roles);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ReturnsUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testadmin",
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // API returns 200 with error in body

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Invalid username or password", authResult.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ReturnsError()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "inactiveuser",
            Password = "InactiveUser123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Account is disabled", authResult.ErrorMessage);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ReturnsError()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "nonexistent",
            Password = "Password123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Invalid username or password", authResult.ErrorMessage);
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("testuser", "")]
    [InlineData("", "")]
    [InlineData(null, "Password123!")]
    [InlineData("testuser", null)]
    public async Task Login_WithInvalidInput_ReturnsValidationError(string username, string password)
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = username,
            Password = password
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        Assert.NotNull(authResult);
        Assert.False(authResult.Success);
        Assert.Equal("Username and password are required", authResult.ErrorMessage);
    }

    [Fact]
    public async Task PasswordHashing_IsSecureAndVerifiable()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var password = "TestPassword123!";

        // Act
        var hash1 = passwordHasher.HashPassword(password);
        var hash2 = passwordHasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // Different salts should produce different hashes
        Assert.True(passwordHasher.VerifyPassword(password, hash1));
        Assert.True(passwordHasher.VerifyPassword(password, hash2));
        Assert.False(passwordHasher.VerifyPassword("WrongPassword", hash1));
    }

    [Fact]
    public async Task UserRepository_ValidateCredentials_WorksCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        // Act & Assert - Valid credentials
        var validUser = await userRepository.ValidateCredentialsAsync("testadmin", "TestAdmin123!");
        Assert.NotNull(validUser);
        Assert.Equal("testadmin", validUser.Username);

        // Act & Assert - Invalid password
        var invalidPasswordUser = await userRepository.ValidateCredentialsAsync("testadmin", "WrongPassword");
        Assert.Null(invalidPasswordUser);

        // Act & Assert - Non-existent user
        var nonExistentUser = await userRepository.ValidateCredentialsAsync("nonexistent", "Password123!");
        Assert.Null(nonExistentUser);

        // Act & Assert - Inactive user
        var inactiveUser = await userRepository.ValidateCredentialsAsync("inactiveuser", "InactiveUser123!");
        Assert.Null(inactiveUser); // Should return null for inactive users
    }

    [Fact]
    public async Task DatabaseInitialization_CreatesAdminUserCorrectly()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BICopilotContext>();

        // Act
        var adminUsers = await context.Users
            .Where(u => u.Roles.Contains(ApplicationConstants.Roles.Admin))
            .ToListAsync();

        // Assert
        Assert.NotEmpty(adminUsers);
        var adminUser = adminUsers.First();
        Assert.NotNull(adminUser.PasswordHash);
        Assert.NotEmpty(adminUser.PasswordHash);
        Assert.True(adminUser.IsActive);
    }

    public void Dispose()
    {
        _client?.Dispose();
    }
}



/// <summary>
/// Test-specific database initialization service that doesn't use migrations
/// </summary>
public class TestDatabaseInitializationService : IDatabaseInitializationService
{
    private readonly BICopilotContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<TestDatabaseInitializationService> _logger;

    public TestDatabaseInitializationService(
        BICopilotContext context,
        IPasswordHasher passwordHasher,
        ILogger<TestDatabaseInitializationService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // For in-memory database, just ensure it's created
            await _context.Database.EnsureCreatedAsync();
            _logger.LogInformation("Test database created successfully");

            // Seed test data
            await SeedTestDataAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during test database initialization");
            throw;
        }
    }

    private async Task SeedTestDataAsync()
    {
        // Check if data already exists
        if (await _context.Users.AnyAsync())
        {
            return;
        }

        // Create test admin user
        var adminUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-admin-001",
            Username = "testadmin",
            Email = "testadmin@test.com",
            DisplayName = "Test Administrator",
            PasswordHash = _passwordHasher.HashPassword("TestAdmin123!"),
            Roles = ApplicationConstants.Roles.Admin,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        // Create test regular user
        var regularUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-user-001",
            Username = "testuser",
            Email = "testuser@test.com",
            DisplayName = "Test User",
            PasswordHash = _passwordHasher.HashPassword("TestUser123!"),
            Roles = ApplicationConstants.Roles.User,
            IsActive = true,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        // Create inactive user
        var inactiveUser = new BIReportingCopilot.Infrastructure.Data.Entities.UserEntity
        {
            Id = "test-inactive-001",
            Username = "inactiveuser",
            Email = "inactive@test.com",
            DisplayName = "Inactive User",
            PasswordHash = _passwordHasher.HashPassword("InactiveUser123!"),
            Roles = ApplicationConstants.Roles.User,
            IsActive = false,
            CreatedDate = DateTime.UtcNow,
            CreatedBy = "System"
        };

        _context.Users.AddRange(adminUser, regularUser, inactiveUser);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Test data seeded successfully");
    }
}
