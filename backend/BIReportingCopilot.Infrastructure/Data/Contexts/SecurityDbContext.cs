using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Data.Contexts;

/// <summary>
/// Bounded context for security-related entities: users, sessions, authentication, and audit logs
/// </summary>
public class SecurityDbContext : DbContext
{
    public SecurityDbContext(DbContextOptions<SecurityDbContext> options) : base(options)
    {
    }

    // User and authentication entities
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserSessionEntity> UserSessions { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    public DbSet<MfaChallengeEntity> MfaChallenges { get; set; }
    public DbSet<UserPreferencesEntity> UserPreferences { get; set; }

    // Audit and security logging
    public DbSet<AuditLogEntity> AuditLog { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Users
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.CreatedDate });
            entity.Property(e => e.Id).HasMaxLength(256);
            entity.Property(e => e.Username).HasMaxLength(100);
            entity.Property(e => e.Email).HasMaxLength(256);
            entity.Property(e => e.DisplayName).HasMaxLength(200);
            entity.Property(e => e.PasswordHash).HasMaxLength(500);
            entity.Property(e => e.Roles).HasMaxLength(500);
            entity.Property(e => e.MfaSecret).HasMaxLength(500);
            entity.Property(e => e.MfaMethod).HasMaxLength(50);
            entity.Property(e => e.PhoneNumber).HasMaxLength(20);
            entity.Property(e => e.BackupCodes).HasMaxLength(2000);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
        });

        // Configure UserSessions
        modelBuilder.Entity<UserSessionEntity>(entity =>
        {
            entity.HasKey(e => e.SessionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LastActivity);
            entity.HasIndex(e => new { e.IsActive, e.LastActivity });
            entity.Property(e => e.SessionId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);

            // Foreign key relationship
            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure RefreshTokens
        modelBuilder.Entity<RefreshTokenEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.IsRevoked, e.ExpiresAt });
            entity.Property(e => e.Token).HasMaxLength(256);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(45);

            // Foreign key relationship
            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure MfaChallenges
        modelBuilder.Entity<MfaChallengeEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ExpiresAt });
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => new { e.IsUsed, e.ExpiresAt });
            entity.Property(e => e.Id).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.ChallengeCode).HasMaxLength(10);
            entity.Property(e => e.MfaMethod).HasMaxLength(50);
            entity.Property(e => e.DeliveryAddress).HasMaxLength(256);

            // Foreign key relationship
            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure UserPreferences
        modelBuilder.Entity<UserPreferencesEntity>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.PreferredChartTypes).HasMaxLength(500);
            entity.Property(e => e.DefaultDateRange).HasMaxLength(50);
            entity.Property(e => e.NotificationSettings).HasMaxLength(1000);

            // Foreign key relationship
            entity.HasOne<UserEntity>()
                .WithOne()
                .HasForeignKey<UserPreferencesEntity>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure AuditLog
        modelBuilder.Entity<AuditLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.Action, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp });
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Severity).HasMaxLength(20);

            // Foreign key relationship (optional - user might be deleted)
            entity.HasOne<UserEntity>()
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Seed default admin user
        SeedSecurityData(modelBuilder);
    }

    private static void SeedSecurityData(ModelBuilder modelBuilder)
    {
        // Seed default admin user (password will be set during application startup)
        modelBuilder.Entity<UserEntity>().HasData(
            new UserEntity
            {
                Id = "admin-user-001",
                Username = "admin",
                Email = "admin@bireporting.local",
                DisplayName = "System Administrator",
                PasswordHash = "", // Will be set during application startup
                Roles = "Admin",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            },
            new UserEntity
            {
                Id = "analyst-user-001",
                Username = "analyst",
                Email = "analyst@bireporting.local",
                DisplayName = "Data Analyst",
                PasswordHash = "", // Will be set during application startup
                Roles = "Analyst",
                IsActive = true,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System"
            }
        );
    }
}
