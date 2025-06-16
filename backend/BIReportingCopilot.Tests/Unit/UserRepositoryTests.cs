using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using BIReportingCopilot.Infrastructure.Repositories;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using Moq;

namespace BIReportingCopilot.Tests.Unit;

[TestFixture]
public class UserRepositoryTests
{
    private Mock<ILogger<UserRepository>> _mockLogger;
    private Mock<IPasswordHasher> _mockPasswordHasher;
    private BICopilotContext _context;
    private UserRepository _userRepository;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<UserRepository>>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        var options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new BICopilotContext(options);

        _userRepository = new UserRepository(_context, _mockLogger.Object, _mockPasswordHasher.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ValidateCredentialsAsync_WithValidActiveUser_ReturnsUser()
    {
        // Arrange
        var username = "testuser";
        var password = "testpassword";
        var userEntity = new UserEntity
        {
            Id = "test-user-id",
            Username = username,
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = "User,Analyst",
            IsActive = true,
            CreatedDate = DateTime.UtcNow.AddDays(-30),
            LastLoginDate = DateTime.UtcNow.AddDays(-1)
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.ValidateCredentialsAsync(username, password);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be("test-user-id");
        result.Username.Should().Be(username);
        result.Email.Should().Be("test@example.com");
        result.DisplayName.Should().Be("Test User");
        result.Roles.Should().BeEquivalentTo(new[] { "User", "Analyst" });
        result.IsActive.Should().BeTrue();

        // Verify last login was updated
        var updatedUser = await _context.Users.FindAsync("test-user-id");
        updatedUser!.LastLoginDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task ValidateCredentialsAsync_WithInactiveUser_ReturnsNull()
    {
        // Arrange
        var username = "inactiveuser";
        var password = "testpassword";
        var userEntity = new UserEntity
        {
            Id = "inactive-user-id",
            Username = username,
            Email = "inactive@example.com",
            DisplayName = "Inactive User",
            Roles = "User",
            IsActive = false // Inactive user
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.ValidateCredentialsAsync(username, password);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ValidateCredentialsAsync_WithNonExistentUser_ReturnsNull()
    {
        // Arrange
        var username = "nonexistentuser";
        var password = "testpassword";

        // Act
        var result = await _userRepository.ValidateCredentialsAsync(username, password);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task ValidateCredentialsAsync_WithEmptyPassword_ReturnsNull()
    {
        // Arrange
        var username = "testuser";
        var password = "";
        var userEntity = new UserEntity
        {
            Id = "test-user-id",
            Username = username,
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = "User",
            IsActive = true
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.ValidateCredentialsAsync(username, password);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetUserPermissionsAsync_WithValidUser_ReturnsCorrectPermissions()
    {
        // Arrange
        var userId = "test-user-id";
        var userEntity = new UserEntity
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = "Admin,Analyst",
            IsActive = true
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("bi_copilot_access");
        result.Should().Contain("read_data");
        result.Should().Contain("query_data");
        result.Should().Contain("export_data");
        result.Should().Contain("manage_users"); // Admin permission
        result.Should().Contain("view_audit_logs"); // Admin permission
        result.Should().Contain("create_visualizations"); // Analyst permission
    }

    [Test]
    public async Task GetUserPermissionsAsync_WithViewerRole_ReturnsLimitedPermissions()
    {
        // Arrange
        var userId = "viewer-user-id";
        var userEntity = new UserEntity
        {
            Id = userId,
            Username = "vieweruser",
            Email = "viewer@example.com",
            DisplayName = "Viewer User",
            Roles = "Viewer",
            IsActive = true
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("bi_copilot_access");
        result.Should().Contain("read_data");
        result.Should().Contain("query_data");
        result.Should().NotContain("export_data");
        result.Should().NotContain("manage_users");
        result.Should().NotContain("view_audit_logs");
    }

    [Test]
    public async Task GetUserPermissionsAsync_WithInactiveUser_ReturnsEmptyList()
    {
        // Arrange
        var userId = "inactive-user-id";
        var userEntity = new UserEntity
        {
            Id = userId,
            Username = "inactiveuser",
            Email = "inactive@example.com",
            DisplayName = "Inactive User",
            Roles = "Admin",
            IsActive = false
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserPermissionsAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task GetUserRolesAsync_WithValidUser_ReturnsRoles()
    {
        // Arrange
        var userId = "test-user-id";
        var userEntity = new UserEntity
        {
            Id = userId,
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = "Admin,Analyst,Viewer",
            IsActive = true
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _userRepository.GetUserRolesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new[] { "Admin", "Analyst", "Viewer" });
    }

    [Test]
    public async Task GetUserRolesAsync_WithNonExistentUser_ReturnsEmptyList()
    {
        // Arrange
        var userId = "non-existent-user-id";

        // Act
        var result = await _userRepository.GetUserRolesAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Test]
    public async Task CreateUserAsync_WithValidUser_CreatesSuccessfully()
    {
        // Arrange
        var user = new User
        {
            Id = "new-user-id",
            Username = "newuser",
            Email = "newuser@example.com",
            DisplayName = "New User",
            Roles = new[] { "User" },
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        // Act
        var result = await _userRepository.CreateUserAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be("new-user-id");

        var createdUser = await _context.Users.FindAsync("new-user-id");
        createdUser.Should().NotBeNull();
        createdUser!.Username.Should().Be("newuser");
        createdUser.Email.Should().Be("newuser@example.com");
        createdUser.Roles.Should().Be("User");
    }

    [Test]
    public async Task UpdateUserAsync_WithValidUser_UpdatesSuccessfully()
    {
        // Arrange
        var userEntity = new UserEntity
        {
            Id = "test-user-id",
            Username = "testuser",
            Email = "test@example.com",
            DisplayName = "Test User",
            Roles = "User",
            IsActive = true
        };

        _context.Users.Add(userEntity);
        await _context.SaveChangesAsync();

        var updatedUser = new User
        {
            Id = "test-user-id",
            Username = "testuser",
            Email = "updated@example.com",
            DisplayName = "Updated Test User",
            Roles = new[] { "User", "Analyst" },
            IsActive = true
        };

        // Act
        var result = await _userRepository.UpdateUserAsync(updatedUser);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be("updated@example.com");
        result.DisplayName.Should().Be("Updated Test User");
        result.Roles.Should().BeEquivalentTo(new[] { "User", "Analyst" });

        var dbUser = await _context.Users.FindAsync("test-user-id");
        dbUser!.Email.Should().Be("updated@example.com");
        dbUser.DisplayName.Should().Be("Updated Test User");
        dbUser.Roles.Should().Be("User,Analyst");
    }
}
