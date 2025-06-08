using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Data.Contexts;

/// <summary>
/// Bounded context for system monitoring, performance metrics, and analytics
/// </summary>
public class MonitoringDbContext : DbContext
{
    public MonitoringDbContext(DbContextOptions<MonitoringDbContext> options) : base(options)
    {
    }

    // System monitoring
    public DbSet<SystemMetricsEntity> SystemMetrics { get; set; }
    public DbSet<PerformanceMetricsEntity> PerformanceMetrics { get; set; }
    public DbSet<ErrorLogEntity> ErrorLogs { get; set; }
    public DbSet<HealthCheckLogEntity> HealthCheckLogs { get; set; }

    // Usage analytics
    public DbSet<UserActivityEntity> UserActivity { get; set; }
    public DbSet<FeatureUsageEntity> FeatureUsage { get; set; }
    public DbSet<ApiUsageEntity> ApiUsage { get; set; }

    // Resource monitoring
    public DbSet<ResourceUsageEntity> ResourceUsage { get; set; }
    public DbSet<DatabaseConnectionEntity> DatabaseConnections { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SystemMetrics
        modelBuilder.Entity<SystemMetricsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.MetricName, e.Timestamp });
            entity.HasIndex(e => new { e.Timestamp, e.MetricName });
            entity.Property(e => e.MetricName).HasMaxLength(100);
            entity.Property(e => e.MetricCategory).HasMaxLength(50);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.Tags).HasMaxLength(500);
        });

        // Configure PerformanceMetrics
        modelBuilder.Entity<PerformanceMetricsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.OperationType, e.Timestamp });
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.Property(e => e.OperationType).HasMaxLength(50);
            entity.Property(e => e.OperationName).HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.SessionId).HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasMaxLength(50);
            entity.Property(e => e.AdditionalData).HasMaxLength(2000);
        });

        // Configure ErrorLogs
        modelBuilder.Entity<ErrorLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.Severity, e.Timestamp });
            entity.HasIndex(e => new { e.Source, e.Timestamp });
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.Property(e => e.Source).HasMaxLength(100);
            entity.Property(e => e.Severity).HasMaxLength(20);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.SessionId).HasMaxLength(50);
            entity.Property(e => e.CorrelationId).HasMaxLength(50);
            entity.Property(e => e.ErrorCode).HasMaxLength(50);
            entity.Property(e => e.Message).HasMaxLength(1000);
        });

        // Configure HealthCheckLogs
        modelBuilder.Entity<HealthCheckLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.CheckName, e.Timestamp });
            entity.HasIndex(e => new { e.Status, e.Timestamp });
            entity.Property(e => e.CheckName).HasMaxLength(100);
            entity.Property(e => e.Status).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Data).HasMaxLength(2000);
        });

        // Configure UserActivity
        modelBuilder.Entity<UserActivityEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.ActivityType, e.Timestamp });
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.SessionId).HasMaxLength(50);
            entity.Property(e => e.ActivityType).HasMaxLength(50);
            entity.Property(e => e.ActivityName).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.AdditionalData).HasMaxLength(1000);
        });

        // Configure FeatureUsage
        modelBuilder.Entity<FeatureUsageEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.FeatureName, e.Timestamp });
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.FeatureName).HasMaxLength(100);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.SessionId).HasMaxLength(50);
            entity.Property(e => e.FeatureVersion).HasMaxLength(20);
            entity.Property(e => e.Context).HasMaxLength(500);
        });

        // Configure ApiUsage
        modelBuilder.Entity<ApiUsageEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.Endpoint, e.Timestamp });
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.StatusCode, e.Timestamp });
            entity.Property(e => e.Endpoint).HasMaxLength(200);
            entity.Property(e => e.Method).HasMaxLength(10);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.RequestId).HasMaxLength(50);
        });

        // Configure ResourceUsage
        modelBuilder.Entity<ResourceUsageEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.ResourceType, e.Timestamp });
            entity.Property(e => e.ResourceType).HasMaxLength(50);
            entity.Property(e => e.ResourceName).HasMaxLength(100);
            entity.Property(e => e.Unit).HasMaxLength(20);
            entity.Property(e => e.Tags).HasMaxLength(500);
        });

        // Configure DatabaseConnections
        modelBuilder.Entity<DatabaseConnectionEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.DatabaseName, e.Timestamp });
            entity.HasIndex(e => new { e.ConnectionStatus, e.Timestamp });
            entity.Property(e => e.DatabaseName).HasMaxLength(100);
            entity.Property(e => e.ConnectionString).HasMaxLength(500);
            entity.Property(e => e.ConnectionStatus).HasMaxLength(20);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        });

        // Configure data retention policies
        ConfigureDataRetention(modelBuilder);
    }

    private static void ConfigureDataRetention(ModelBuilder modelBuilder)
    {
        // Add computed columns for data retention
        modelBuilder.Entity<SystemMetricsEntity>()
            .Property(e => e.RetentionDate)
            .HasComputedColumnSql("DATEADD(day, 90, [Timestamp])");

        modelBuilder.Entity<PerformanceMetricsEntity>()
            .Property(e => e.RetentionDate)
            .HasComputedColumnSql("DATEADD(day, 30, [Timestamp])");

        modelBuilder.Entity<ErrorLogEntity>()
            .Property(e => e.RetentionDate)
            .HasComputedColumnSql("DATEADD(day, 365, [Timestamp])");

        modelBuilder.Entity<UserActivityEntity>()
            .Property(e => e.RetentionDate)
            .HasComputedColumnSql("DATEADD(day, 180, [Timestamp])");

        modelBuilder.Entity<FeatureUsageEntity>()
            .Property(e => e.RetentionDate)
            .HasComputedColumnSql("DATEADD(day, 365, [Timestamp])");

        modelBuilder.Entity<ApiUsageEntity>()
            .Property(e => e.RetentionDate)
            .HasComputedColumnSql("DATEADD(day, 90, [Timestamp])");

        modelBuilder.Entity<ResourceUsageEntity>()
            .Property(e => e.RetentionDate)
            .HasComputedColumnSql("DATEADD(day, 30, [Timestamp])");

        modelBuilder.Entity<DatabaseConnectionEntity>()
            .Property(e => e.RetentionDate)
            .HasComputedColumnSql("DATEADD(day, 30, [Timestamp])");
    }
}
