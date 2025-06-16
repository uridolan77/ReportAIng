using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Data.Configurations;
using System.Text.Json;
using InfraUserPreferencesEntity = BIReportingCopilot.Infrastructure.Data.Entities.UserPreferencesEntity;
using InfraAuditLogEntity = BIReportingCopilot.Infrastructure.Data.Entities.AuditLogEntity;
using InfraUserEntity = BIReportingCopilot.Infrastructure.Data.Entities.UserEntity;
using InfraUserSessionEntity = BIReportingCopilot.Infrastructure.Data.Entities.UserSessionEntity;
using InfraRefreshTokenEntity = BIReportingCopilot.Infrastructure.Data.Entities.RefreshTokenEntity;
using InfraMfaChallengeEntity = BIReportingCopilot.Infrastructure.Data.Entities.MfaChallengeEntity;

namespace BIReportingCopilot.Infrastructure.Data;

public class BICopilotContext : DbContext
{
    public BICopilotContext(DbContextOptions<BICopilotContext> options) : base(options)
    {
    }

    // Core entities
    public DbSet<SchemaMetadataEntity> SchemaMetadata { get; set; }
    public DbSet<Core.Models.UnifiedQueryHistoryEntity> QueryHistory { get; set; } // Query history
    public DbSet<PromptTemplateEntity> PromptTemplates { get; set; }
    public DbSet<PromptLogEntity> PromptLogs { get; set; }
    public DbSet<AITuningSettingsEntity> AITuningSettings { get; set; }
    public DbSet<QueryCacheEntity> QueryCache { get; set; }
    public DbSet<InfraUserPreferencesEntity> UserPreferences { get; set; }
    public DbSet<SystemConfigurationEntity> SystemConfiguration { get; set; }
    public DbSet<InfraAuditLogEntity> AuditLog { get; set; }

    // AI Tuning entities
    public DbSet<BusinessTableInfoEntity> BusinessTableInfo { get; set; }
    public DbSet<BusinessColumnInfoEntity> BusinessColumnInfo { get; set; }
    public DbSet<QueryPatternEntity> QueryPatterns { get; set; }
    public DbSet<BusinessGlossaryEntity> BusinessGlossary { get; set; }

    // User and session management
    public DbSet<InfraUserEntity> Users { get; set; }
    public DbSet<InfraUserSessionEntity> UserSessions { get; set; }
    public DbSet<InfraRefreshTokenEntity> RefreshTokens { get; set; }
    public DbSet<InfraMfaChallengeEntity> MfaChallenges { get; set; }

    // Analytics and monitoring
    public DbSet<QueryPerformanceEntity> QueryPerformance { get; set; }
    public DbSet<SystemMetricsEntity> SystemMetrics { get; set; }

    // AI Learning and Semantic Cache
    public DbSet<Core.Models.AIGenerationAttempt> AIGenerationAttempts { get; set; }
    public DbSet<Core.Models.UnifiedAIFeedbackEntry> AIFeedbackEntries { get; set; }
    public DbSet<Core.Models.UnifiedSemanticCacheEntry> SemanticCacheEntries { get; set; }

    // Additional missing DbSets
    public DbSet<SystemMetricsEntity> PerformanceMetrics { get; set; }
    public DbSet<Core.Models.TempFile> TempFiles { get; set; }

    // Query Suggestions System
    public DbSet<Core.Models.QuerySuggestions.SuggestionCategory> SuggestionCategories { get; set; }
    public DbSet<Core.Models.QuerySuggestions.QuerySuggestion> QuerySuggestions { get; set; }
    public DbSet<Core.Models.QuerySuggestions.SuggestionUsageAnalytics> SuggestionUsageAnalytics { get; set; }
    public DbSet<Core.Models.QuerySuggestions.TimeFrameDefinition> TimeFrameDefinitions { get; set; }

    // Schema Management entities
    public DbSet<BusinessSchema> BusinessSchemas { get; set; }
    public DbSet<BusinessSchemaVersion> BusinessSchemaVersions { get; set; }
    public DbSet<SchemaTableContext> SchemaTableContexts { get; set; }
    public DbSet<SchemaColumnContext> SchemaColumnContexts { get; set; }
    public DbSet<SchemaGlossaryTerm> SchemaGlossaryTerms { get; set; }
    public DbSet<SchemaRelationship> SchemaRelationships { get; set; }
    public DbSet<UserSchemaPreference> UserSchemaPreferences { get; set; }

    // LLM Management entities
    public DbSet<LLMProviderConfig> LLMProviderConfigs { get; set; }
    public DbSet<LLMModelConfig> LLMModelConfigs { get; set; }
    public DbSet<LLMUsageLog> LLMUsageLogs { get; set; }

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

        // QueryHistory configuration moved to unified model configuration below (line 330)

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
            // Fix decimal precision warning
            entity.Property(e => e.SuccessRate).HasPrecision(5, 2);
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
        modelBuilder.Entity<InfraUserPreferencesEntity>(entity =>
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
        modelBuilder.Entity<InfraAuditLogEntity>(entity =>
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
        modelBuilder.Entity<InfraUserEntity>(entity =>
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
        modelBuilder.Entity<InfraUserSessionEntity>(entity =>
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
        modelBuilder.Entity<InfraRefreshTokenEntity>(entity =>
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
        modelBuilder.Entity<InfraMfaChallengeEntity>(entity =>
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
        modelBuilder.Entity<Core.Models.UnifiedAIGenerationAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AttemptedAt);
            entity.Property(e => e.UserId).HasMaxLength(500);
            entity.Property(e => e.AIProvider).HasMaxLength(100);
            entity.Property(e => e.ModelVersion).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(500);
        });

        modelBuilder.Entity<Core.Models.UnifiedAIFeedbackEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.UserId).HasMaxLength(500);
            entity.Property(e => e.FeedbackType).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(500);
        });

        modelBuilder.Entity<Core.Models.UnifiedSemanticCacheEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QueryHash).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.LastAccessedAt);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.QueryHash).HasMaxLength(64);
            entity.Property(e => e.OriginalQuery).HasMaxLength(4000);
            entity.Property(e => e.NormalizedQuery).HasMaxLength(4000);
            entity.Property(e => e.CreatedBy).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(500);
            
            // Ignore the ResultData navigation property as it's a complex type
            entity.Ignore(e => e.ResultData);
        });

        modelBuilder.Entity<Core.Models.UnifiedQueryHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExecutedAt);
            entity.HasIndex(e => e.SessionId);
            entity.Property(e => e.UserId).HasMaxLength(500);
            entity.Property(e => e.Query).HasMaxLength(2000);
            entity.Property(e => e.SessionId).HasMaxLength(100);
            entity.Property(e => e.DatabaseName).HasMaxLength(100);
            entity.Property(e => e.SchemaName).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(500);
        });

        // Apply schema management configurations
        modelBuilder.ApplyConfiguration(new BusinessSchemaConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessSchemaVersionConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaTableContextConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaColumnContextConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaGlossaryTermConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaRelationshipConfiguration());
        modelBuilder.ApplyConfiguration(new UserSchemaPreferenceConfiguration());

        // Configure Query Suggestions entities
        ConfigureQuerySuggestions(modelBuilder);

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
- Always add WITH (NOLOCK) hint to all table references for better read performance.
- Format table hints as: FROM TableName alias WITH (NOLOCK) - never use AS keyword with table hints.

Return only the SQL query without any explanation or markdown formatting.",
                Description = "Basic template for generating SQL queries from natural language",
                IsActive = true,
                CreatedBy = "System",
                CreatedDate = DateTime.UtcNow,
                UsageCount = 0
            }
        );

        // Seed default admin user (password will be set during application startup)
        modelBuilder.Entity<InfraUserEntity>().HasData(
            new InfraUserEntity
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

    private static void ConfigureQuerySuggestions(ModelBuilder modelBuilder)
    {
        // SuggestionCategory configuration
        modelBuilder.Entity<Core.Models.QuerySuggestions.SuggestionCategory>(entity =>
        {
            entity.ToTable("SuggestionCategories");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CategoryKey).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Icon).HasMaxLength(10);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
            entity.HasIndex(e => e.CategoryKey).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.SortOrder });
        });

        // QuerySuggestion configuration
        modelBuilder.Entity<Core.Models.QuerySuggestions.QuerySuggestion>(entity =>
        {
            entity.ToTable("QuerySuggestions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.QueryText).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DefaultTimeFrame).HasMaxLength(50);
            entity.Property(e => e.TargetTables).HasMaxLength(500);
            entity.Property(e => e.RequiredPermissions).HasMaxLength(200);
            entity.Property(e => e.Tags).HasMaxLength(300);
            entity.Property(e => e.Relevance).HasDefaultValue(0.8).HasPrecision(3, 2);
            entity.Property(e => e.Confidence).HasDefaultValue(0.8).HasPrecision(3, 2);
            entity.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);

            entity.HasOne(e => e.Category)
                .WithMany(c => c.Suggestions)
                .HasForeignKey(e => e.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.CategoryId, e.IsActive, e.SortOrder });
            entity.HasIndex(e => new { e.UsageCount, e.LastUsed }).IsDescending();
        });

        // SuggestionUsageAnalytics configuration
        modelBuilder.Entity<Core.Models.QuerySuggestions.SuggestionUsageAnalytics>(entity =>
        {
            entity.ToTable("SuggestionUsageAnalytics");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.SessionId).HasMaxLength(50);
            entity.Property(e => e.TimeFrameUsed).HasMaxLength(50);

            entity.HasOne(e => e.Suggestion)
                .WithMany(s => s.UsageAnalytics)
                .HasForeignKey(e => e.SuggestionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.SuggestionId, e.UsedAt }).IsDescending();
            entity.HasIndex(e => new { e.UserId, e.UsedAt }).IsDescending();
        });

        // TimeFrameDefinition configuration
        modelBuilder.Entity<Core.Models.QuerySuggestions.TimeFrameDefinition>(entity =>
        {
            entity.ToTable("TimeFrameDefinitions");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TimeFrameKey).IsRequired().HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.SqlExpression).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.TimeFrameKey).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.SortOrder });
        });

        // LLM Management entity configurations
        modelBuilder.Entity<LLMProviderConfig>(entity =>
        {
            entity.ToTable("LLMProviderConfigs");
            entity.HasKey(e => e.ProviderId);
            entity.Property(e => e.ProviderId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Type).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ApiKey).HasMaxLength(500);
            entity.Property(e => e.Endpoint).HasMaxLength(500);
            entity.Property(e => e.Organization).HasMaxLength(100);
            entity.Property(e => e.Settings).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => new { e.IsEnabled, e.IsDefault });
        });

        modelBuilder.Entity<LLMModelConfig>(entity =>
        {
            entity.ToTable("LLMModelConfigs");
            entity.HasKey(e => e.ModelId);
            entity.Property(e => e.ModelId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ProviderId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.UseCase).HasMaxLength(50);
            entity.Property(e => e.CostPerToken).HasPrecision(18, 8);
            entity.Property(e => e.Capabilities).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            entity.HasIndex(e => e.ProviderId);
            entity.HasIndex(e => new { e.IsEnabled, e.UseCase });
        });

        modelBuilder.Entity<LLMUsageLog>(entity =>
        {
            entity.ToTable("LLMUsageLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ProviderId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ModelId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.RequestType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.RequestText).IsRequired();
            entity.Property(e => e.ResponseText).IsRequired();
            entity.Property(e => e.Cost).HasPrecision(18, 8);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.Metadata).HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions?)null) ?? new Dictionary<string, object>());
            entity.HasIndex(e => e.RequestId);
            entity.HasIndex(e => new { e.UserId, e.Timestamp }).IsDescending();
            entity.HasIndex(e => new { e.ProviderId, e.Timestamp }).IsDescending();
            entity.HasIndex(e => new { e.ModelId, e.Timestamp }).IsDescending();
            entity.HasIndex(e => new { e.RequestType, e.Timestamp }).IsDescending();
        });
    }
}
