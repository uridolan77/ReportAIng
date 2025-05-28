using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Data;
using Xunit;

namespace BIReportingCopilot.Tests.Integration.Authentication;

public class AuthenticationIntegrationTests : IClassFixture<TestWebApplicationFactory<Program>>
{
    private readonly TestWebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public AuthenticationIntegrationTests(TestWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnSuccessWithTokens()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        authResult.Should().NotBeNull();
        authResult!.Success.Should().BeTrue();
        authResult.AccessToken.Should().NotBeNullOrEmpty();
        authResult.RefreshToken.Should().NotBeNullOrEmpty();
        authResult.User.Should().NotBeNull();
        authResult.User!.Username.Should().Be("testuser");
        authResult.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Login_WithInvalidUsername_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "nonexistentuser",
            Password = "testpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        authResult.Should().NotBeNull();
        authResult!.Success.Should().BeFalse();
        authResult.ErrorMessage.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithInactiveUser_ShouldReturnUnauthorized()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "inactiveuser",
            Password = "testpassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        authResult.Should().NotBeNull();
        authResult!.Success.Should().BeFalse();
        authResult.ErrorMessage.Should().Contain("not active");
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewTokens()
    {
        // Arrange - First login to get tokens
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<AuthenticationResult>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = loginResult!.RefreshToken!
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        authResult.Should().NotBeNull();
        authResult!.Success.Should().BeTrue();
        authResult.AccessToken.Should().NotBeNullOrEmpty();
        authResult.RefreshToken.Should().NotBeNullOrEmpty();
        authResult.AccessToken.Should().NotBe(loginResult.AccessToken);
        authResult.RefreshToken.Should().NotBe(loginResult.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        // Arrange
        var refreshRequest = new RefreshTokenRequest
        {
            RefreshToken = "invalid-refresh-token"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/refresh", refreshRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_WithValidToken_ShouldReturnSuccess()
    {
        // Arrange - First login to get tokens
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<AuthenticationResult>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Add authorization header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act
        var response = await _client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUserInfo_WithValidToken_ShouldReturnUserInfo()
    {
        // Arrange - First login to get tokens
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "testpassword"
        };

        var loginResponse = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        var loginContent = await loginResponse.Content.ReadAsStringAsync();
        var loginResult = JsonSerializer.Deserialize<AuthenticationResult>(loginContent, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        // Add authorization header
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", loginResult!.AccessToken);

        // Act
        var response = await _client.GetAsync("/api/auth/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var userInfo = JsonSerializer.Deserialize<UserInfo>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        userInfo.Should().NotBeNull();
        userInfo!.Username.Should().Be("testuser");
        userInfo.Email.Should().Be("test@example.com");
        userInfo.Roles.Should().Contain("User");
    }

    [Fact]
    public async Task GetUserInfo_WithoutToken_ShouldReturnUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/auth/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task AccountLockout_AfterMultipleFailedAttempts_ShouldLockAccount()
    {
        // Arrange
        var loginRequest = new LoginRequest
        {
            Username = "testuser",
            Password = "wrongpassword"
        };

        // Act - Make multiple failed login attempts
        for (int i = 0; i < 6; i++) // Exceed the limit of 5
        {
            await _client.PostAsJsonAsync("/api/auth/login", loginRequest);
        }

        // Try with correct password after lockout
        loginRequest.Password = "testpassword";
        var response = await _client.PostAsJsonAsync("/api/auth/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var content = await response.Content.ReadAsStringAsync();
        var authResult = JsonSerializer.Deserialize<AuthenticationResult>(content, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        authResult.Should().NotBeNull();
        authResult!.Success.Should().BeFalse();
        authResult.ErrorMessage.Should().Contain("locked");
    }

    [Fact]
    public async Task TokenValidation_WithExpiredToken_ShouldReturnUnauthorized()
    {
        // This test would require manipulating token expiration
        // For now, we'll test with an obviously invalid token

        // Arrange
        _client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", "invalid.jwt.token");

        // Act
        var response = await _client.GetAsync("/api/auth/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
