using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Repositories;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Tests.Unit.MFA;

public class MfaChallengeRepositoryTests : IDisposable
{
    private readonly DbContextOptions<BICopilotContext> _options;
    private readonly BICopilotContext _context;
    private readonly Mock<ILogger<MfaChallengeRepository>> _mockLogger;
    private readonly MfaChallengeRepository _repository;

    public MfaChallengeRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BICopilotContext(_options);
        _mockLogger = new Mock<ILogger<MfaChallengeRepository>>();
        _repository = new MfaChallengeRepository(_context, _mockLogger.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateAsync_WhenValidChallenge_ShouldCreateAndReturnChallenge()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var challenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = "123456",
            Method = MfaMethod.SMS,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        // Act
        var result = await _repository.CreateAsync(challenge);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(challenge.Id);
        result.UserId.Should().Be(userId);
        result.Code.Should().Be("123456");
        result.Method.Should().Be(MfaMethod.SMS);
        result.IsUsed.Should().BeFalse();

        // Verify it was saved to database
        var savedChallenge = await _context.MfaChallenges.FindAsync(challenge.Id);
        savedChallenge.Should().NotBeNull();
        savedChallenge.Code.Should().Be("123456");
    }

    [Fact]
    public async Task GetByIdAsync_WhenChallengeExists_ShouldReturnChallenge()
    {
        // Arrange
        var challengeId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var challenge = new MfaChallengeEntity
        {
            Id = challengeId,
            UserId = userId,
            Code = "123456",
            Method = MfaMethod.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.MfaChallenges.Add(challenge);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(challengeId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(challengeId);
        result.UserId.Should().Be(userId);
        result.Code.Should().Be("123456");
        result.Method.Should().Be(MfaMethod.Email);
    }

    [Fact]
    public async Task GetByIdAsync_WhenChallengeDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.GetByIdAsync(nonExistentId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByUserIdAndCodeAsync_WhenChallengeExists_ShouldReturnChallenge()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "123456";
        var challenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = code,
            Method = MfaMethod.TOTP,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.MfaChallenges.Add(challenge);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByUserIdAndCodeAsync(userId, code);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Code.Should().Be(code);
    }

    [Fact]
    public async Task GetByUserIdAndCodeAsync_WhenChallengeDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = "999999";

        // Act
        var result = await _repository.GetByUserIdAndCodeAsync(userId, code);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_WhenActiveChallengesExist_ShouldReturnActiveChallenges()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeChallenge1 = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = "123456",
            Method = MfaMethod.SMS,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var activeChallenge2 = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = "789012",
            Method = MfaMethod.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(3),
            IsUsed = false,
            AttemptCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        var expiredChallenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = "999999",
            Method = MfaMethod.SMS,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5), // Expired
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var usedChallenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = "888888",
            Method = MfaMethod.TOTP,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = true, // Used
            AttemptCount = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.MfaChallenges.AddRange(activeChallenge1, activeChallenge2, expiredChallenge, usedChallenge);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(c => c.Code == "123456");
        result.Should().Contain(c => c.Code == "789012");
        result.Should().NotContain(c => c.Code == "999999"); // Expired
        result.Should().NotContain(c => c.Code == "888888"); // Used
    }

    [Fact]
    public async Task GetActiveByUserIdAsync_WhenNoActiveChallenges_ShouldReturnEmptyList()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetActiveByUserIdAsync(userId);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAsync_WhenChallengeExists_ShouldUpdateChallenge()
    {
        // Arrange
        var challengeId = Guid.NewGuid();
        var challenge = new MfaChallengeEntity
        {
            Id = challengeId,
            UserId = Guid.NewGuid(),
            Code = "123456",
            Method = MfaMethod.SMS,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.MfaChallenges.Add(challenge);
        await _context.SaveChangesAsync();

        // Modify the challenge
        challenge.IsUsed = true;
        challenge.AttemptCount = 1;

        // Act
        var result = await _repository.UpdateAsync(challenge);

        // Assert
        result.Should().NotBeNull();
        result.IsUsed.Should().BeTrue();
        result.AttemptCount.Should().Be(1);

        // Verify it was updated in database
        var updatedChallenge = await _context.MfaChallenges.FindAsync(challengeId);
        updatedChallenge.Should().NotBeNull();
        updatedChallenge.IsUsed.Should().BeTrue();
        updatedChallenge.AttemptCount.Should().Be(1);
    }

    [Fact]
    public async Task DeleteAsync_WhenChallengeExists_ShouldDeleteChallenge()
    {
        // Arrange
        var challengeId = Guid.NewGuid();
        var challenge = new MfaChallengeEntity
        {
            Id = challengeId,
            UserId = Guid.NewGuid(),
            Code = "123456",
            Method = MfaMethod.TOTP,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.MfaChallenges.Add(challenge);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteAsync(challengeId);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from database
        var deletedChallenge = await _context.MfaChallenges.FindAsync(challengeId);
        deletedChallenge.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenChallengeDoesNotExist_ShouldReturnFalse()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid();

        // Act
        var result = await _repository.DeleteAsync(nonExistentId);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteExpiredChallengesAsync_WhenExpiredChallengesExist_ShouldDeleteOnlyExpired()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var activeChallenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = "123456",
            Method = MfaMethod.SMS,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // Active
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        var expiredChallenge1 = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = "789012",
            Method = MfaMethod.Email,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5), // Expired
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow.AddMinutes(-10)
        };

        var expiredChallenge2 = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Code = "345678",
            Method = MfaMethod.TOTP,
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10), // Expired
            IsUsed = false,
            AttemptCount = 2,
            CreatedAt = DateTime.UtcNow.AddMinutes(-15)
        };

        _context.MfaChallenges.AddRange(activeChallenge, expiredChallenge1, expiredChallenge2);
        await _context.SaveChangesAsync();

        // Act
        var deletedCount = await _repository.DeleteExpiredChallengesAsync();

        // Assert
        deletedCount.Should().Be(2);

        // Verify only expired challenges were deleted
        var remainingChallenges = await _context.MfaChallenges.ToListAsync();
        remainingChallenges.Should().HaveCount(1);
        remainingChallenges[0].Code.Should().Be("123456"); // Active challenge should remain
    }

    [Fact]
    public async Task DeleteExpiredChallengesAsync_WhenNoExpiredChallenges_ShouldReturnZero()
    {
        // Arrange
        var activeChallenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Code = "123456",
            Method = MfaMethod.SMS,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5), // Active
            IsUsed = false,
            AttemptCount = 0,
            CreatedAt = DateTime.UtcNow
        };

        _context.MfaChallenges.Add(activeChallenge);
        await _context.SaveChangesAsync();

        // Act
        var deletedCount = await _repository.DeleteExpiredChallengesAsync();

        // Assert
        deletedCount.Should().Be(0);

        // Verify no challenges were deleted
        var remainingChallenges = await _context.MfaChallenges.ToListAsync();
        remainingChallenges.Should().HaveCount(1);
    }
}
