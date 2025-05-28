using Xunit;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace BIReportingCopilot.Tests.Unit.MFA;

public class MfaChallengeRepositoryTests : IDisposable
{
    private readonly BICopilotContext _context;
    private readonly MfaChallengeRepository _repository;
    private readonly Mock<ILogger<MfaChallengeRepository>> _mockLogger;

    public MfaChallengeRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new BICopilotContext(options);
        _mockLogger = new Mock<ILogger<MfaChallengeRepository>>();
        _repository = new MfaChallengeRepository(_context, _mockLogger.Object);
    }

    [Fact]
    public async Task CreateChallengeAsync_WhenValidChallenge_ShouldCreateAndReturnChallenge()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var challenge = new MfaChallenge
        {
            UserId = userId,
            Challenge = "123456",
            Method = MfaMethod.SMS,
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false
        };

        // Act
        var result = await _repository.CreateChallengeAsync(challenge);

        // Assert
        result.Should().NotBeNull();
        result.UserId.Should().Be(userId);
        result.Challenge.Should().Be("123456");
        result.Method.Should().Be(MfaMethod.SMS);
        result.ChallengeId.Should().NotBeNullOrEmpty();
        
        // Verify entity was saved to database
        var entity = await _context.MfaChallenges.FirstOrDefaultAsync(x => x.Id == result.ChallengeId);
        entity.Should().NotBeNull();
        entity!.ChallengeCode.Should().Be("123456");
    }

    [Fact]
    public async Task GetActiveChallengeAsync_WhenChallengeExists_ShouldReturnChallenge()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var entity = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ChallengeCode = "123456",
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow
        };

        _context.MfaChallenges.Add(entity);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveChallengeAsync(userId, "123456");

        // Assert
        result.Should().NotBeNull();
        result!.ChallengeId.Should().Be(entity.Id);
        result.UserId.Should().Be(userId);
        result.Challenge.Should().Be("123456");
    }

    [Fact]
    public async Task GetActiveChallengeAsync_WhenChallengeNotExists_ShouldReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();

        // Act
        var result = await _repository.GetActiveChallengeAsync(userId, "nonexistent");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetActiveChallengeAsync_WhenMultipleChallenges_ShouldReturnMostRecent()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var code = "123456";

        var activeChallenge1 = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ChallengeCode = code,
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow.AddMinutes(-10)
        };

        var activeChallenge2 = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ChallengeCode = code,
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow.AddMinutes(-5)
        };

        var expiredChallenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ChallengeCode = code,
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-1),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow.AddMinutes(-15)
        };

        var usedChallenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            ChallengeCode = code,
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = true,
            AttemptCount = 1,
            CreatedDate = DateTime.UtcNow.AddMinutes(-2)
        };

        _context.MfaChallenges.AddRange(activeChallenge1, activeChallenge2, expiredChallenge, usedChallenge);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetActiveChallengeAsync(userId, code);

        // Assert
        result.Should().NotBeNull();
        result!.ChallengeId.Should().Be(activeChallenge2.Id); // Most recent active challenge
    }

    [Fact]
    public async Task MarkChallengeAsUsedAsync_WhenChallengeExists_ShouldMarkAsUsed()
    {
        // Arrange
        var entity = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            ChallengeCode = "123456",
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow
        };

        _context.MfaChallenges.Add(entity);
        await _context.SaveChangesAsync();

        // Act
        await _repository.MarkChallengeAsUsedAsync(entity.Id);

        // Assert
        var updatedEntity = await _context.MfaChallenges.FindAsync(entity.Id);
        updatedEntity.Should().NotBeNull();
        updatedEntity!.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task MarkChallengeAsUsedAsync_WhenChallengeNotExists_ShouldNotThrow()
    {
        // Arrange
        var nonExistentId = Guid.NewGuid().ToString();

        // Act & Assert
        var act = async () => await _repository.MarkChallengeAsUsedAsync(nonExistentId);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task CleanupExpiredChallengesAsync_ShouldRemoveExpiredChallenges()
    {
        // Arrange
        var activeChallenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            ChallengeCode = "123456",
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow
        };

        var expiredChallenge1 = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            ChallengeCode = "789012",
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-10),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow.AddMinutes(-15)
        };

        var expiredChallenge2 = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            ChallengeCode = "345678",
            MfaMethod = MfaMethod.Email.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(-5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow.AddMinutes(-10)
        };

        _context.MfaChallenges.AddRange(activeChallenge, expiredChallenge1, expiredChallenge2);
        await _context.SaveChangesAsync();

        // Act
        var deletedCount = await _repository.CleanupExpiredChallengesAsync();

        // Assert
        deletedCount.Should().Be(2);
        
        var remainingChallenges = await _context.MfaChallenges.ToListAsync();
        remainingChallenges.Should().HaveCount(1);
        remainingChallenges[0].ChallengeCode.Should().Be("123456");
    }

    [Fact]
    public async Task CleanupExpiredChallengesAsync_WhenNoExpiredChallenges_ShouldReturnZero()
    {
        // Arrange
        var activeChallenge = new MfaChallengeEntity
        {
            Id = Guid.NewGuid().ToString(),
            UserId = Guid.NewGuid().ToString(),
            ChallengeCode = "123456",
            MfaMethod = MfaMethod.SMS.ToString(),
            ExpiresAt = DateTime.UtcNow.AddMinutes(5),
            IsUsed = false,
            AttemptCount = 0,
            CreatedDate = DateTime.UtcNow
        };

        _context.MfaChallenges.Add(activeChallenge);
        await _context.SaveChangesAsync();

        // Act
        var deletedCount = await _repository.CleanupExpiredChallengesAsync();

        // Assert
        deletedCount.Should().Be(0);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
