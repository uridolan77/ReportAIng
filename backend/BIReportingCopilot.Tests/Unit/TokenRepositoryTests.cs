using NUnit.Framework;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using BIReportingCopilot.Infrastructure.Repositories;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using Moq;

namespace BIReportingCopilot.Tests.Unit;

[TestFixture]
public class TokenRepositoryTests
{
    private Mock<ILogger<TokenRepository>> _mockLogger;
    private BICopilotContext _context;
    private TokenRepository _tokenRepository;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<TokenRepository>>();

        var options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new BICopilotContext(options);

        _tokenRepository = new TokenRepository(_context, _mockLogger.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task StoreRefreshTokenAsync_WithValidData_StoresTokenSuccessfully()
    {
        // Arrange
        var userId = "test-user-id";
        var token = "test-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(30);

        // Act
        await _tokenRepository.StoreRefreshTokenAsync(userId, token, expiresAt);

        // Assert
        var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        storedToken.Should().NotBeNull();
        storedToken!.UserId.Should().Be(userId);
        storedToken.Token.Should().Be(token);
        storedToken.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
        storedToken.IsRevoked.Should().BeFalse();
        storedToken.CreatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task GetRefreshTokenAsync_WithValidToken_ReturnsTokenInfo()
    {
        // Arrange
        var userId = "test-user-id";
        var token = "test-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(30);
        var createdDate = DateTime.UtcNow.AddDays(-1);

        var tokenEntity = new RefreshTokenEntity
        {
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
            CreatedDate = createdDate,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetRefreshTokenAsync(token);

        // Assert
        result.Should().NotBeNull();
        result!.Token.Should().Be(token);
        result.UserId.Should().Be(userId);
        result.ExpiresAt.Should().BeCloseTo(expiresAt, TimeSpan.FromSeconds(1));
        result.CreatedAt.Should().BeCloseTo(createdDate, TimeSpan.FromSeconds(1));
        result.IsRevoked.Should().BeFalse();
    }

    [Test]
    public async Task GetRefreshTokenAsync_WithExpiredToken_ReturnsNull()
    {
        // Arrange
        var userId = "test-user-id";
        var token = "expired-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(-1); // Expired

        var tokenEntity = new RefreshTokenEntity
        {
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
            IsRevoked = false
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetRefreshTokenAsync(token);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetRefreshTokenAsync_WithRevokedToken_ReturnsNull()
    {
        // Arrange
        var userId = "test-user-id";
        var token = "revoked-refresh-token";
        var expiresAt = DateTime.UtcNow.AddDays(30);

        var tokenEntity = new RefreshTokenEntity
        {
            Token = token,
            UserId = userId,
            ExpiresAt = expiresAt,
            IsRevoked = true // Revoked
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _tokenRepository.GetRefreshTokenAsync(token);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task UpdateRefreshTokenAsync_WithValidTokens_UpdatesSuccessfully()
    {
        // Arrange
        var userId = "test-user-id";
        var oldToken = "old-refresh-token";
        var newToken = "new-refresh-token";
        var newExpiresAt = DateTime.UtcNow.AddDays(30);

        var oldTokenEntity = new RefreshTokenEntity
        {
            Token = oldToken,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(15),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(oldTokenEntity);
        await _context.SaveChangesAsync();

        // Act
        await _tokenRepository.UpdateRefreshTokenAsync(oldToken, newToken, newExpiresAt);

        // Assert
        var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == oldToken);
        revokedToken.Should().NotBeNull();
        revokedToken!.IsRevoked.Should().BeTrue();
        revokedToken.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        var newTokenEntity = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == newToken);
        newTokenEntity.Should().NotBeNull();
        newTokenEntity!.UserId.Should().Be(userId);
        newTokenEntity.ExpiresAt.Should().BeCloseTo(newExpiresAt, TimeSpan.FromSeconds(1));
        newTokenEntity.IsRevoked.Should().BeFalse();
    }

    [Test]
    public async Task RevokeRefreshTokenAsync_WithValidToken_RevokesSuccessfully()
    {
        // Arrange
        var userId = "test-user-id";
        var token = "test-refresh-token";

        var tokenEntity = new RefreshTokenEntity
        {
            Token = token,
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            IsRevoked = false
        };

        _context.RefreshTokens.Add(tokenEntity);
        await _context.SaveChangesAsync();

        // Act
        await _tokenRepository.RevokeRefreshTokenAsync(token);

        // Assert
        var revokedToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);
        revokedToken.Should().NotBeNull();
        revokedToken!.IsRevoked.Should().BeTrue();
        revokedToken.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Test]
    public async Task RevokeAllUserTokensAsync_WithValidUserId_RevokesAllTokens()
    {
        // Arrange
        var userId = "test-user-id";
        var tokens = new[]
        {
            new RefreshTokenEntity { Token = "token1", UserId = userId, ExpiresAt = DateTime.UtcNow.AddDays(30), IsRevoked = false },
            new RefreshTokenEntity { Token = "token2", UserId = userId, ExpiresAt = DateTime.UtcNow.AddDays(30), IsRevoked = false },
            new RefreshTokenEntity { Token = "token3", UserId = "other-user", ExpiresAt = DateTime.UtcNow.AddDays(30), IsRevoked = false }
        };

        _context.RefreshTokens.AddRange(tokens);
        await _context.SaveChangesAsync();

        // Act
        await _tokenRepository.RevokeAllUserTokensAsync(userId);

        // Assert
        var userTokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
        userTokens.Should().AllSatisfy(t => t.IsRevoked.Should().BeTrue());
        userTokens.Should().AllSatisfy(t => t.UpdatedDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5)));

        var otherUserToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.UserId == "other-user");
        otherUserToken.Should().NotBeNull();
        otherUserToken!.IsRevoked.Should().BeFalse();
    }

    [Test]
    public async Task CleanupExpiredTokensAsync_WithExpiredTokens_RemovesExpiredTokens()
    {
        // Arrange
        var tokens = new[]
        {
            new RefreshTokenEntity { Token = "expired1", UserId = "user1", ExpiresAt = DateTime.UtcNow.AddDays(-1), IsRevoked = false },
            new RefreshTokenEntity { Token = "expired2", UserId = "user2", ExpiresAt = DateTime.UtcNow.AddDays(-5), IsRevoked = false },
            new RefreshTokenEntity { Token = "revoked", UserId = "user3", ExpiresAt = DateTime.UtcNow.AddDays(30), IsRevoked = true },
            new RefreshTokenEntity { Token = "valid", UserId = "user4", ExpiresAt = DateTime.UtcNow.AddDays(30), IsRevoked = false }
        };

        _context.RefreshTokens.AddRange(tokens);
        await _context.SaveChangesAsync();

        // Act
        await _tokenRepository.CleanupExpiredTokensAsync();

        // Assert
        var remainingTokens = await _context.RefreshTokens.ToListAsync();
        remainingTokens.Should().HaveCount(1);
        remainingTokens.First().Token.Should().Be("valid");
    }
}
