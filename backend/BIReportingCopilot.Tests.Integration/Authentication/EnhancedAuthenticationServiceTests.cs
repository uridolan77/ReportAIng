using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Services;
using Xunit;

namespace BIReportingCopilot.Tests.Integration.Authentication;

public class EnhancedAuthenticationServiceTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITokenRepository> _mockTokenRepository;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IAuditService> _mockAuditService;
    private readonly Mock<ICacheService> _mockCacheService;
    private readonly EnhancedAuthenticationService _authService;

    public EnhancedAuthenticationServiceTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;

        // Create mocks
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenRepository = new Mock<ITokenRepository>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockAuditService = new Mock<IAuditService>();
        _mockCacheService = new Mock<ICacheService>();

        // Setup configuration
        var jwtSettings = new JwtSettings
        {
            Secret = "test-super-secret-jwt-key-that-is-at-least-32-characters-long-for-testing",
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            AccessTokenExpirationMinutes = 60,
            RefreshTokenExpirationMinutes = 43200,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };

        var securitySettings = new SecuritySettings
        {
            MaxLoginAttempts = 5,
            LockoutDurationMinutes = 15,
            MinPasswordLength = 8,
            RequireDigit = true,
            RequireLowercase = true,
            RequireUppercase = true,
            RequireNonAlphanumeric = true
        };

        var logger = new Mock<ILogger<EnhancedAuthenticationService>>();

        // Create service instance
        _authService = new EnhancedAuthenticationService(
            logger.Object,
            _mockUserRepository.Object,
            _mockTokenRepository.Object,
            _mockPasswordHasher.Object,
            _mockAuditService.Object,
            Options.Create(jwtSettings),
            Options.Create(securitySettings),
            _mockCacheService.Object
        );
    }

    [Fact]
    public async Task AuthenticateAsync_WithValidCredentials_ShouldReturnSuccess()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user-1",
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = new[] { "User" },
            IsActive = true
        };

        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        _mockUserRepository.Setup(x => x.ValidateCredentialsAsync("testuser", "testpassword"))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserPermissionsAsync("test-user-1"))
            .ReturnsAsync(new List<string> { "read_data", "query_data" });
        _mockCacheService.Setup(x => x.GetAsync<object>(It.IsAny<string>()))
            .ReturnsAsync((object?)null);

        // Act
        var result = await _authService.AuthenticateAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.User.Should().NotBeNull();
        result.User!.Username.Should().Be("testuser");
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        // Verify interactions
        _mockTokenRepository.Verify(x => x.StoreRefreshTokenAsync(
            It.Is<string>(s => s == "test-user-1"),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Once);
        _mockAuditService.Verify(x => x.LogAsync(
            It.IsAny<string>(),
            It.Is<string>(s => s == "test-user-1"),
            It.IsAny<string>(),
            It.Is<string>(s => s == "test-user-1"),
            It.IsAny<object>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task AuthenticateAsync_WithInvalidCredentials_ShouldReturnFailure()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        _mockUserRepository.Setup(x => x.ValidateCredentialsAsync("testuser", "wrongpassword"))
            .ReturnsAsync((User?)null);
        _mockCacheService.Setup(x => x.GetAsync<object>(It.IsAny<string>()))
            .ReturnsAsync((object?)null);

        // Act
        var result = await _authService.AuthenticateAsync(loginRequest);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().NotBeNullOrEmpty();
        result.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();

        // Verify failed login handling
        _mockCacheService.Verify(x => x.SetAsync(
            It.IsAny<string>(),
            It.IsAny<object>(),
            It.IsAny<TimeSpan?>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var userId = "test-user-1";
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = new[] { "User" },
            IsActive = true
        };

        var tokenInfo = new TokenInfo
        {
            Token = refreshToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(1),
            IsRevoked = false
        };

        _mockTokenRepository.Setup(x => x.GetRefreshTokenAsync(refreshToken))
            .ReturnsAsync(tokenInfo);
        _mockTokenRepository.Setup(x => x.GetUserIdFromRefreshTokenAsync(refreshToken))
            .ReturnsAsync(userId);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserPermissionsAsync(userId))
            .ReturnsAsync(new List<string> { "read_data", "query_data" });

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.AccessToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBeNullOrEmpty();
        result.RefreshToken.Should().NotBe(refreshToken); // Should be a new token

        // Verify old token was revoked and new token stored
        _mockTokenRepository.Verify(x => x.RevokeRefreshTokenAsync(refreshToken), Times.Once);
        _mockTokenRepository.Verify(x => x.StoreRefreshTokenAsync(
            It.Is<string>(s => s == userId),
            It.IsAny<string>(),
            It.IsAny<DateTime>()), Times.Once);
    }

    [Fact]
    public async Task RefreshTokenAsync_WithExpiredToken_ShouldReturnFailure()
    {
        // Arrange
        var refreshToken = "expired-refresh-token";
        var tokenInfo = new TokenInfo
        {
            Token = refreshToken,
            UserId = "test-user-1",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            IsRevoked = false
        };

        _mockTokenRepository.Setup(x => x.GetRefreshTokenAsync(refreshToken))
            .ReturnsAsync(tokenInfo);

        // Act
        var result = await _authService.RefreshTokenAsync(refreshToken);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("expired");
    }

    [Fact]
    public async Task ValidateTokenAsync_WithValidToken_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Id = "test-user-1",
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = new[] { "User" },
            IsActive = true
        };

        // First authenticate to get a valid token
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        _mockUserRepository.Setup(x => x.ValidateCredentialsAsync("testuser", "testpassword"))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.UpdateUserAsync(It.IsAny<User>()))
            .ReturnsAsync(user);
        _mockUserRepository.Setup(x => x.GetUserPermissionsAsync("test-user-1"))
            .ReturnsAsync(new List<string> { "read_data", "query_data" });
        _mockCacheService.Setup(x => x.GetAsync<object>(It.IsAny<string>()))
            .ReturnsAsync((object?)null);

        var authResult = await _authService.AuthenticateAsync(loginRequest);
        var token = authResult.AccessToken!;

        // Act
        var isValid = await _authService.ValidateTokenAsync(token);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateTokenAsync_WithInvalidToken_ShouldReturnFalse()
    {
        // Arrange
        var invalidToken = "invalid.jwt.token";

        // Act
        var isValid = await _authService.ValidateTokenAsync(invalidToken);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserInfoAsync_WithValidUserId_ShouldReturnUserInfo()
    {
        // Arrange
        var userId = "test-user-1";
        var user = new User
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = new[] { "User" },
            IsActive = true,
            LastLoginDate = DateTime.UtcNow.AddHours(-1)
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var userInfo = await _authService.GetUserInfoAsync(userId);

        // Assert
        userInfo.Should().NotBeNull();
        userInfo.Id.Should().Be(userId);
        userInfo.Username.Should().Be("testuser");
        userInfo.Email.Should().Be("test@example.com");
        userInfo.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task GetUserInfoAsync_WithInvalidUserId_ShouldThrowException()
    {
        // Arrange
        var userId = "invalid-user-id";
        _mockUserRepository.Setup(x => x.GetByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        await _authService.Invoking(x => x.GetUserInfoAsync(userId))
            .Should().ThrowAsync<ArgumentException>()
            .WithMessage("*not found*");
    }
}
