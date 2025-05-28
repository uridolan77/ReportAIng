using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Data;

public class BICopilotContext : DbContext
{
    public BICopilotContext(DbContextOptions<BICopilotContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<SchemaMetadataEntity> SchemaMetadata { get; set; }
    public DbSet<QueryHistoryEntity> QueryHistory { get; set; }
    public DbSet<PromptTemplateEntity> PromptTemplates { get; set; }
    public DbSet<PromptLogEntity> PromptLogs { get; set; }
    public DbSet<AITuningSettingsEntity> AITuningSettings { get; set; }
    public DbSet<QueryCacheEntity> QueryCache { get; set; }
    public DbSet<UserPreferencesEntity> UserPreferences { get; set; }
    public DbSet<SystemConfigurationEntity> SystemConfiguration { get; set; }
    public DbSet<AuditLogEntity> AuditLog { get; set; }

    // AI Tuning entities
    public DbSet<BusinessTableInfoEntity> BusinessTableInfo { get; set; }
    public DbSet<BusinessColumnInfoEntity> BusinessColumnInfo { get; set; }
    public DbSet<QueryPatternEntity> QueryPatterns { get; set; }
    public DbSet<BusinessGlossaryEntity> BusinessGlossary { get; set; }

    // User and session management
    public DbSet<UserEntity> Users { get; set; }
    public DbSet<UserSessionEntity> UserSessions { get; set; }
    public DbSet<RefreshTokenEntity> RefreshTokens { get; set; }
    public DbSet<MfaChallengeEntity> MfaChallenges { get; set; }

    // Analytics and monitoring
    public DbSet<QueryPerformanceEntity> QueryPerformance { get; set; }
    public DbSet<SystemMetricsEntity> SystemMetrics { get; set; }

    // AI Learning and Semantic Cache
    public DbSet<AIGenerationAttempt> AIGenerationAttempts { get; set; }
    public DbSet<AIFeedbackEntry> AIFeedbackEntries { get; set; }
    public DbSet<SemanticCacheEntry> SemanticCacheEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SchemaMetadata
        modelBuilder.Entity<SchemaMetadataEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DatabaseName, e.SchemaName, e.TableName });
            entity.HasIndex(e => new { e.TableName, e.ColumnName });
            entity.Property(e => e.SemanticTags).HasMaxLength(1000);
            entity.Property(e => e.SampleValues).HasMaxLength(2000);
            entity.Property(e => e.BusinessDescription).HasMaxLength(500);
        });

        // Configure QueryHistory
        modelBuilder.Entity<QueryHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.QueryTimestamp });
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.QueryTimestamp);
            entity.Property(e => e.NaturalLanguageQuery).HasMaxLength(2000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
        });

        // Configure PromptTemplates
        modelBuilder.Entity<PromptTemplateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.Version }).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.Name });
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Version).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
        });

        // Configure QueryCache
        modelBuilder.Entity<QueryCacheEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QueryHash).IsUnique();
            entity.HasIndex(e => e.ExpiryTimestamp);
            entity.HasIndex(e => e.LastAccessedDate);
            entity.Property(e => e.QueryHash).HasMaxLength(64);
            entity.Property(e => e.ResultMetadata).HasMaxLength(2000);
        });

        // Configure UserPreferences
        modelBuilder.Entity<UserPreferencesEntity>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.PreferredChartTypes).HasMaxLength(500);
            entity.Property(e => e.DefaultDateRange).HasMaxLength(50);
            entity.Property(e => e.NotificationSettings).HasMaxLength(1000);
        });

        // Configure SystemConfiguration
        modelBuilder.Entity<SystemConfigurationEntity>(entity =>
        {
            entity.HasKey(e => e.Key);
            entity.Property(e => e.Key).HasMaxLength(100);
            entity.Property(e => e.DataType).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
        });

        // Configure AuditLog
        modelBuilder.Entity<AuditLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => new { e.Action, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.Action).HasMaxLength(100);
            entity.Property(e => e.EntityType).HasMaxLength(50);
            entity.Property(e => e.EntityId).HasMaxLength(100);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
            entity.Property(e => e.Severity).HasMaxLength(20);
        });

        // Configure Users
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
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
        });

        // Configure UserSessions
        modelBuilder.Entity<UserSessionEntity>(entity =>
        {
            entity.HasKey(e => e.SessionId);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.LastActivity);
            entity.Property(e => e.SessionId).HasMaxLength(50);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });

        // Configure RefreshTokens
        modelBuilder.Entity<RefreshTokenEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExpiresAt);
            entity.Property(e => e.Token).HasMaxLength(256);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.DeviceInfo).HasMaxLength(500);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
        });

        // Configure QueryPerformance
        modelBuilder.Entity<QueryPerformanceEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QueryHash);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.QueryHash).HasMaxLength(64);
            entity.Property(e => e.UserId).HasMaxLength(256);
        });

        // Configure SystemMetrics
        modelBuilder.Entity<SystemMetricsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.MetricName).HasMaxLength(100);
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
        });

        // Configure AI Tuning entities
        // BusinessTableInfo indexes
        modelBuilder.Entity<BusinessTableInfoEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SchemaName, e.TableName })
                .HasDatabaseName("IX_BusinessTableInfo_Schema_Table")
                .IsUnique();
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_BusinessTableInfo_IsActive");
        });

        // BusinessColumnInfo indexes
        modelBuilder.Entity<BusinessColumnInfoEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TableInfoId, e.ColumnName })
                .HasDatabaseName("IX_BusinessColumnInfo_Table_Column")
                .IsUnique();
            entity.HasIndex(e => e.IsKeyColumn)
                .HasDatabaseName("IX_BusinessColumnInfo_IsKeyColumn");
        });

        // QueryPatterns indexes
        modelBuilder.Entity<QueryPatternEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PatternName)
                .HasDatabaseName("IX_QueryPatterns_PatternName")
                .IsUnique();
            entity.HasIndex(e => new { e.Priority, e.IsActive })
                .HasDatabaseName("IX_QueryPatterns_Priority_Active");
            entity.HasIndex(e => e.UsageCount)
                .HasDatabaseName("IX_QueryPatterns_UsageCount");
        });

        // BusinessGlossary indexes
        modelBuilder.Entity<BusinessGlossaryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Term)
                .HasDatabaseName("IX_BusinessGlossary_Term")
                .IsUnique();
            entity.HasIndex(e => e.Category)
                .HasDatabaseName("IX_BusinessGlossary_Category");
        });

        // AITuningSettings indexes
        modelBuilder.Entity<AITuningSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SettingKey)
                .HasDatabaseName("IX_AITuningSettings_SettingKey")
                .IsUnique();
            entity.HasIndex(e => e.Category)
                .HasDatabaseName("IX_AITuningSettings_Category");
        });

        // Configure BusinessTableInfo relationships
        modelBuilder.Entity<BusinessColumnInfoEntity>()
            .HasOne(c => c.TableInfo)
            .WithMany(t => t.Columns)
            .HasForeignKey(c => c.TableInfoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure AI Learning entities
        modelBuilder.Entity<AIGenerationAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PromptHash);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.PromptHash).HasMaxLength(32);
            entity.Property(e => e.SQLHash).HasMaxLength(32);
            entity.Property(e => e.UserId).HasMaxLength(256);
        });

        modelBuilder.Entity<AIFeedbackEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.PromptPattern, e.IsSuccessful });
            entity.HasIndex(e => new { e.UserId, e.Timestamp });
            entity.HasIndex(e => e.Timestamp);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.PromptPattern).HasMaxLength(50);
            entity.Property(e => e.SQLPattern).HasMaxLength(50);
        });

        modelBuilder.Entity<SemanticCacheEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QuerySignature).IsUnique();
            entity.HasIndex(e => e.ExpiryTime);
            entity.HasIndex(e => e.LastAccessTime);
            entity.Property(e => e.QuerySignature).HasMaxLength(32);
        });

        // Seed default data
        SeedDefaultData(modelBuilder);
    }

    private static void SeedDefaultData(ModelBuilder modelBuilder)
    {
        // Seed default system configuration
        modelBuilder.Entity<SystemConfigurationEntity>().HasData(
            new SystemConfigurationEntity
            {
                Key = "MaxQueryExecutionTimeSeconds",
                Value = "30",
                DataType = "int",
                Description = "Maximum time allowed for query execution",
                UpdatedBy = "System",
                LastUpdated = DateTime.UtcNow
            },
            new SystemConfigurationEntity
            {
                Key = "MaxResultRows",
                Value = "10000",
                DataType = "int",
                Description = "Maximum number of rows returned per query",
                UpdatedBy = "System",
                LastUpdated = DateTime.UtcNow
            },
            new SystemConfigurationEntity
            {
                Key = "EnableQueryCaching",
                Value = "true",
                DataType = "bool",
                Description = "Enable caching of query results",
                UpdatedBy = "System",
                LastUpdated = DateTime.UtcNow
            },
            new SystemConfigurationEntity
            {
                Key = "CacheExpiryHours",
                Value = "24",
                DataType = "int",
                Description = "Default cache expiry time in hours",
                UpdatedBy = "System",
                LastUpdated = DateTime.UtcNow
            },
            new SystemConfigurationEntity
            {
                Key = "EnableAuditLogging",
                Value = "true",
                DataType = "bool",
                Description = "Enable comprehensive audit logging",
                UpdatedBy = "System",
                LastUpdated = DateTime.UtcNow
            }
        );

        // Seed default prompt templates
        modelBuilder.Entity<PromptTemplateEntity>().HasData(
            new PromptTemplateEntity
            {
                Id = 1,
                Name = "BasicQueryGeneration",
                Version = "1.0",
                Content = @"You are a SQL Server expert helping generate business intelligence reports.

Database schema:
{schema}

When the user asks: '{question}'
- Write a T-SQL SELECT query to answer the question.
- Only use SELECT statements, never write/alter data.
- Use proper table and column names from the schema.
- Include appropriate WHERE clauses for filtering.
- Use JOINs when data from multiple tables is needed.
- Format the query for readability.

Return only the SQL query without any explanation or markdown formatting.",
                Description = "Basic template for generating SQL queries from natural language",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow,
                UsageCount = 0
            }
        );

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
            }
        );
    }
}
