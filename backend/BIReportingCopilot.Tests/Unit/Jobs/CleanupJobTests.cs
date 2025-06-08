using NUnit.Framework;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using FluentAssertions;
using BIReportingCopilot.Infrastructure.Jobs;
using BIReportingCopilot.Infrastructure.Data;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Models;
using Microsoft.AspNetCore.SignalR;

namespace BIReportingCopilot.Tests.Unit.Jobs;

[TestFixture]
public class CleanupJobTests
{
    private Mock<ILogger<BackgroundJobManagementService>> _mockLogger;
    private Mock<ICacheService> _mockCacheService;
    private Mock<IConfiguration> _mockConfiguration;
    private BICopilotContext _context;
    private BackgroundJobManagementService _cleanupJob;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<BackgroundJobManagementService>>();
        _mockCacheService = new Mock<ICacheService>();
        _mockConfiguration = new Mock<IConfiguration>();

        var options = new DbContextOptionsBuilder<BICopilotContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new BICopilotContext(options);

        // Create mocks for all required dependencies
        var mockSchemaService = new Mock<ISchemaService>();
        var mockAuditService = new Mock<IAuditService>();
        var mockQueryService = new Mock<IQueryService>();
        var mockConfigurationService = new Mock<UnifiedConfigurationService>();
        var mockHubContext = new Mock<IHubContext<Hub>>();

        // Setup configuration service to return performance settings
        mockConfigurationService.Setup(x => x.GetPerformanceSettings())
            .Returns(new PerformanceConfiguration { RetentionDays = 30 });

        _cleanupJob = new BackgroundJobManagementService(
            _context,
            mockSchemaService.Object,
            mockAuditService.Object,
            _mockCacheService.Object,
            mockQueryService.Object,
            mockConfigurationService.Object,
            _mockLogger.Object,
            mockHubContext.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task ExecuteAsync_CleansUpInactiveSessions()
    {
        // Arrange
        var cutoffTime = DateTime.UtcNow.AddHours(-24);
        var sessions = new[]
        {
            new UserSessionEntity
            {
                SessionId = "active-session",
                UserId = "user1",
                StartTime = DateTime.UtcNow.AddHours(-2),
                LastActivity = DateTime.UtcNow.AddHours(-1),
                IsActive = true
            },
            new UserSessionEntity
            {
                SessionId = "inactive-session-1",
                UserId = "user2",
                StartTime = DateTime.UtcNow.AddHours(-30),
                LastActivity = DateTime.UtcNow.AddHours(-25),
                IsActive = true
            },
            new UserSessionEntity
            {
                SessionId = "inactive-session-2",
                UserId = "user3",
                StartTime = DateTime.UtcNow.AddHours(-48),
                LastActivity = DateTime.UtcNow.AddHours(-26),
                IsActive = true
            }
        };

        _context.UserSessions.AddRange(sessions);
        await _context.SaveChangesAsync();

        // Act
        await _cleanupJob.PerformSystemCleanupAsync();

        // Assert
        var activeSessions = await _context.UserSessions.Where(s => s.IsActive).ToListAsync();
        var inactiveSessions = await _context.UserSessions.Where(s => !s.IsActive).ToListAsync();

        activeSessions.Should().HaveCount(1);
        activeSessions.First().SessionId.Should().Be("active-session");

        inactiveSessions.Should().HaveCount(2);
        inactiveSessions.Should().Contain(s => s.SessionId == "inactive-session-1");
        inactiveSessions.Should().Contain(s => s.SessionId == "inactive-session-2");
    }

    [Test]
    public async Task ExecuteAsync_CleansUpExpiredRefreshTokens()
    {
        // Arrange
        var tokens = new[]
        {
            new RefreshTokenEntity
            {
                Token = "valid-token",
                UserId = "user1",
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = false
            },
            new RefreshTokenEntity
            {
                Token = "expired-token",
                UserId = "user2",
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                IsRevoked = false
            },
            new RefreshTokenEntity
            {
                Token = "revoked-token",
                UserId = "user3",
                ExpiresAt = DateTime.UtcNow.AddDays(30),
                IsRevoked = true
            }
        };

        _context.RefreshTokens.AddRange(tokens);
        await _context.SaveChangesAsync();

        // Act
        await _cleanupJob.PerformSystemCleanupAsync();

        // Assert
        var remainingTokens = await _context.RefreshTokens.ToListAsync();
        remainingTokens.Should().HaveCount(1);
        remainingTokens.First().Token.Should().Be("valid-token");
    }

    [Test]
    public async Task ExecuteAsync_DeletesOldInactiveSessions()
    {
        // Arrange
        var oldCutoff = DateTime.UtcNow.AddDays(-30);
        var sessions = new[]
        {
            new UserSessionEntity
            {
                SessionId = "recent-inactive",
                UserId = "user1",
                StartTime = DateTime.UtcNow.AddDays(-2),
                LastActivity = DateTime.UtcNow.AddDays(-1),
                IsActive = false
            },
            new UserSessionEntity
            {
                SessionId = "old-inactive-1",
                UserId = "user2",
                StartTime = DateTime.UtcNow.AddDays(-35),
                LastActivity = DateTime.UtcNow.AddDays(-31),
                IsActive = false
            },
            new UserSessionEntity
            {
                SessionId = "old-inactive-2",
                UserId = "user3",
                StartTime = DateTime.UtcNow.AddDays(-40),
                LastActivity = DateTime.UtcNow.AddDays(-32),
                IsActive = false
            }
        };

        _context.UserSessions.AddRange(sessions);
        await _context.SaveChangesAsync();

        // Act
        await _cleanupJob.PerformSystemCleanupAsync();

        // Assert
        var remainingSessions = await _context.UserSessions.ToListAsync();
        remainingSessions.Should().HaveCount(1);
        remainingSessions.First().SessionId.Should().Be("recent-inactive");
    }

    [Test]
    public async Task ExecuteAsync_CleansUpOldAuditLogs()
    {
        // Arrange
        var retentionDays = 90;
        var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);

        var auditLogs = new[]
        {
            new AuditLogEntity
            {
                UserId = "user1",
                Action = "LOGIN",
                Timestamp = DateTime.UtcNow.AddDays(-30),
                Severity = "Info"
            },
            new AuditLogEntity
            {
                UserId = "user2",
                Action = "QUERY_EXECUTED",
                Timestamp = DateTime.UtcNow.AddDays(-100),
                Severity = "Info"
            },
            new AuditLogEntity
            {
                UserId = "user3",
                Action = "ERROR",
                Timestamp = DateTime.UtcNow.AddDays(-120),
                Severity = "Error"
            }
        };

        _context.AuditLog.AddRange(auditLogs);
        await _context.SaveChangesAsync();

        // Act
        await _cleanupJob.PerformSystemCleanupAsync();

        // Assert
        var remainingLogs = await _context.AuditLog.ToListAsync();
        remainingLogs.Should().HaveCount(1);
        remainingLogs.First().Action.Should().Be("LOGIN");
    }

    [Test]
    public async Task ExecuteAsync_CleansUpOldQueryCache()
    {
        // Arrange
        var cacheEntries = new[]
        {
            new QueryCacheEntity
            {
                QueryHash = "hash1",
                NormalizedQuery = "SELECT * FROM table1",
                ResultData = "{}",
                CacheTimestamp = DateTime.UtcNow.AddHours(-1),
                ExpiryTimestamp = DateTime.UtcNow.AddHours(1),
                HitCount = 5
            },
            new QueryCacheEntity
            {
                QueryHash = "hash2",
                NormalizedQuery = "SELECT * FROM table2",
                ResultData = "{}",
                CacheTimestamp = DateTime.UtcNow.AddDays(-2),
                ExpiryTimestamp = DateTime.UtcNow.AddDays(-1), // Expired
                HitCount = 2
            },
            new QueryCacheEntity
            {
                QueryHash = "hash3",
                NormalizedQuery = "SELECT * FROM table3",
                ResultData = "{}",
                CacheTimestamp = DateTime.UtcNow.AddDays(-8),
                ExpiryTimestamp = DateTime.UtcNow.AddDays(1),
                HitCount = 0, // Never accessed
                LastAccessedDate = DateTime.UtcNow.AddDays(-8)
            }
        };

        _context.QueryCache.AddRange(cacheEntries);
        await _context.SaveChangesAsync();

        // Act
        await _cleanupJob.PerformSystemCleanupAsync();

        // Assert
        var remainingCache = await _context.QueryCache.ToListAsync();
        remainingCache.Should().HaveCount(1);
        remainingCache.First().QueryHash.Should().Be("hash1");
    }

    [Test]
    public async Task ExecuteAsync_WithException_LogsErrorAndContinues()
    {
        // Arrange
        // Create a scenario that might cause an exception
        _context.Dispose(); // Dispose context to simulate database error

        // Act & Assert
        await _cleanupJob.Invoking(job => job.PerformSystemCleanupAsync())
            .Should().NotThrowAsync();

        // Verify error was logged
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error during cleanup")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    [Test]
    public async Task PerformCleanup_WithDisposedContext_HandlesGracefully()
    {
        // Arrange
        _context.Dispose(); // Dispose context to simulate error

        // Act & Assert
        await _cleanupJob.Invoking(job => job.PerformSystemCleanupAsync())
            .Should().NotThrowAsync();
    }

    [Test]
    public async Task ExecuteAsync_LogsCleanupStatistics()
    {
        // Arrange
        var sessions = new[]
        {
            new UserSessionEntity
            {
                SessionId = "inactive-session",
                UserId = "user1",
                StartTime = DateTime.UtcNow.AddHours(-30),
                LastActivity = DateTime.UtcNow.AddHours(-25),
                IsActive = true
            }
        };

        var tokens = new[]
        {
            new RefreshTokenEntity
            {
                Token = "expired-token",
                UserId = "user1",
                ExpiresAt = DateTime.UtcNow.AddDays(-1),
                IsRevoked = false
            }
        };

        _context.UserSessions.AddRange(sessions);
        _context.RefreshTokens.AddRange(tokens);
        await _context.SaveChangesAsync();

        // Act
        await _cleanupJob.PerformSystemCleanupAsync();

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Marked") && v.ToString()!.Contains("sessions as inactive")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Deleted") && v.ToString()!.Contains("expired refresh tokens")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}
