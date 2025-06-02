using BIReportingCopilot.Core.Constants;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Infrastructure.Repositories;
using BIReportingCopilot.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace BIReportingCopilot.Tests.Integration;

public class SimpleAuthenticationTests : IDisposable
{
    private readonly BICopilotContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IUserRepository _userRepository;
    private readonly IAuthenticationService _authenticationService;
    private readonly Mock<ILogger<UserRepository>> _userRepoLogger;
    private readonly Mock<ILogger<AuthenticationService>> _authLogger;
    private readonly Mock<ITokenRepository> _tokenRepository;
    private readonly Mock<IAuditService> _auditService;
    private readonly Mock<ICacheService> _cacheService;
    private readonly Mock<IMfaService> _mfaService;
    private readonly Mock<IMfaChallengeRepository> _mfaChallengeRepository;

    public SimpleAuthenticationTests()
    {
        // Create in-memory database
        var options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new BICopilotContext(options);
        _context.Database.EnsureCreated();

        // Create real services
        var passwordHasherLogger = new Mock<ILogger<PasswordHasher>>();
        _passwordHasher = new PasswordHasher(passwordHasherLogger.Object);
        _userRepoLogger = new Mock<ILogger<UserRepository>>();
        _authLogger = new Mock<ILogger<AuthenticationService>>();
        _tokenRepository = new Mock<ITokenRepository>();
        _auditService = new Mock<IAuditService>();
        _cacheService = new Mock<ICacheService>();
        _mfaService = new Mock<IMfaService>();
        _mfaChallengeRepository = new Mock<IMfaChallengeRepository>();

        // Setup JWT settings
        var securitySettings = new BIReportingCopilot.Core.Configuration.SecurityConfiguration
        {
            JwtSecret = "test-super-secret-jwt-key-that-is-at-least-32-characters-long-for-testing",
            JwtIssuer = "TestIssuer",
            JwtAudience = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 43200,
            MaxLoginAttempts = 5,
            LockoutDurationMinutes = 15,
            MinPasswordLength = 8,
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
            RequireNonAlphanumeric = true
        };

        _userRepository = new UserRepository(_context, _userRepoLogger.Object, _passwordHasher);
        _authenticationService = new AuthenticationService(
            _authLogger.Object,
            _userRepository,
            _tokenRepository.Object,
            _passwordHasher,
            _auditService.Object,
            Options.Create(securitySettings),
            _cacheService.Object,
            _mfaService.Object,
            _mfaChallengeRepository.Object);

        // Seed test data
        SeedTestData();
    }

    private void SeedTestData()
    {
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
        _context.SaveChanges();
    }

    [Fact]
    public async Task PasswordHasher_HashAndVerify_WorksCorrectly()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var hash1 = _passwordHasher.HashPassword(password);
        var hash2 = _passwordHasher.HashPassword(password);

        // Assert
        Assert.NotEqual(hash1, hash2); // Different salts should produce different hashes
        Assert.True(_passwordHasher.VerifyPassword(password, hash1));
        Assert.True(_passwordHasher.VerifyPassword(password, hash2));
        Assert.False(_passwordHasher.VerifyPassword("WrongPassword", hash1));
    }

    [Fact]
    public async Task UserRepository_ValidateCredentials_WithValidUser_ReturnsUser()
    {
        // Act
        var user = await _userRepository.ValidateCredentialsAsync("testadmin", "TestAdmin123!");

        // Assert
        Assert.NotNull(user);
        Assert.Equal("testadmin", user.Username);
        Assert.Equal("testadmin@test.com", user.Email);
        Assert.Contains(ApplicationConstants.Roles.Admin, user.Roles);
        Assert.True(user.IsActive);
    }

    [Fact]
    public async Task UserRepository_ValidateCredentials_WithInvalidPassword_ReturnsNull()
    {
        // Act
        var user = await _userRepository.ValidateCredentialsAsync("testadmin", "WrongPassword");

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task UserRepository_ValidateCredentials_WithInactiveUser_ReturnsNull()
    {
        // Act
        var user = await _userRepository.ValidateCredentialsAsync("inactiveuser", "InactiveUser123!");

        // Assert
        Assert.Null(user); // Should return null for inactive users
    }

    [Fact]
    public async Task UserRepository_ValidateCredentials_WithNonExistentUser_ReturnsNull()
    {
        // Act
        var user = await _userRepository.ValidateCredentialsAsync("nonexistent", "Password123!");

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task AuthenticationService_AuthenticateAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var request = new LoginRequest { Username = "testadmin", Password = "TestAdmin123!" };

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("testadmin", result.User.Username);
        Assert.Contains(ApplicationConstants.Roles.Admin, result.User.Roles);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task AuthenticationService_AuthenticateAsync_WithInvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest { Username = "testadmin", Password = "WrongPassword" };

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Equal("Invalid username or password.", result.ErrorMessage);
    }

    [Fact]
    public async Task AuthenticationService_AuthenticateAsync_WithInactiveUser_ReturnsFailure()
    {
        // Arrange
        var request = new LoginRequest { Username = "inactiveuser", Password = "InactiveUser123!" };

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.User);
        // UserRepository returns null for inactive users, so authentication service treats it as "user not found"
        Assert.Equal("Invalid username or password.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("testuser", "")]
    [InlineData("", "")]
    [InlineData(null, "Password123!")]
    [InlineData("testuser", null)]
    public async Task AuthenticationService_AuthenticateAsync_WithInvalidInput_ReturnsFailure(string username, string password)
    {
        // Arrange
        var request = new LoginRequest { Username = username, Password = password };

        // Act
        var result = await _authenticationService.AuthenticateAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.False(result.Success);
        Assert.Null(result.User);
        // The authentication service returns "Invalid username or password" for security reasons
        Assert.Equal("Invalid username or password.", result.ErrorMessage);
    }

    [Fact]
    public async Task UserRepository_GetByUsernameAsync_WithExistingUser_ReturnsUser()
    {
        // Act
        var user = await _userRepository.GetByUsernameAsync("testadmin");

        // Assert
        Assert.NotNull(user);
        Assert.Equal("testadmin", user.Username);
        Assert.Equal("testadmin@test.com", user.Email);
    }

    [Fact]
    public async Task UserRepository_GetByUsernameAsync_WithNonExistentUser_ReturnsNull()
    {
        // Act
        var user = await _userRepository.GetByUsernameAsync("nonexistent");

        // Assert
        Assert.Null(user);
    }

    [Fact]
    public async Task Database_ContainsSeededUsers()
    {
        // Act
        var users = await _context.Users.ToListAsync();

        // Assert
        Assert.True(users.Count >= 3); // May include admin user from database initialization
        Assert.Contains(users, u => u.Username == "testadmin" && u.IsActive);
        Assert.Contains(users, u => u.Username == "testuser" && u.IsActive);
        Assert.Contains(users, u => u.Username == "inactiveuser" && !u.IsActive);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}
