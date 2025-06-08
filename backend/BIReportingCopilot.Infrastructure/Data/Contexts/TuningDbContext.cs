using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Data.Contexts;

/// <summary>
/// Bounded context for AI tuning and business intelligence configuration
/// </summary>
public class TuningDbContext : DbContext
{
    public TuningDbContext(DbContextOptions<TuningDbContext> options) : base(options)
    {
    }

    // AI Tuning entities
    public DbSet<BusinessTableInfoEntity> BusinessTableInfo { get; set; }
    public DbSet<BusinessColumnInfoEntity> BusinessColumnInfo { get; set; }
    public DbSet<QueryPatternEntity> QueryPatterns { get; set; }
    public DbSet<BusinessGlossaryEntity> BusinessGlossary { get; set; }
    public DbSet<AITuningSettingsEntity> AITuningSettings { get; set; }
    public DbSet<PromptTemplateEntity> PromptTemplates { get; set; }
    public DbSet<PromptLogEntity> PromptLogs { get; set; }

    // AI Learning and feedback (Unified Models)
    public DbSet<UnifiedAIGenerationAttempt> AIGenerationAttempts { get; set; }
    public DbSet<UnifiedAIFeedbackEntry> AIFeedbackEntries { get; set; }

    // System configuration
    public DbSet<SystemConfigurationEntity> SystemConfiguration { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure BusinessTableInfo
        modelBuilder.Entity<BusinessTableInfoEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.SchemaName, e.TableName })
                .HasDatabaseName("IX_BusinessTableInfo_Schema_Table")
                .IsUnique();
            entity.HasIndex(e => e.IsActive)
                .HasDatabaseName("IX_BusinessTableInfo_IsActive");
            entity.HasIndex(e => new { e.CreatedDate, e.IsActive });
            entity.Property(e => e.SchemaName).HasMaxLength(128);
            entity.Property(e => e.TableName).HasMaxLength(128);
            entity.Property(e => e.BusinessPurpose).HasMaxLength(1000);
            entity.Property(e => e.BusinessContext).HasMaxLength(2000);
            entity.Property(e => e.PrimaryUseCase).HasMaxLength(500);
            entity.Property(e => e.BusinessRules).HasMaxLength(2000);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
        });

        // Configure BusinessColumnInfo
        modelBuilder.Entity<BusinessColumnInfoEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TableInfoId, e.ColumnName })
                .HasDatabaseName("IX_BusinessColumnInfo_Table_Column")
                .IsUnique();
            entity.HasIndex(e => e.IsKeyColumn)
                .HasDatabaseName("IX_BusinessColumnInfo_IsKeyColumn");
            entity.HasIndex(e => new { e.IsActive, e.TableInfoId });
            entity.Property(e => e.ColumnName).HasMaxLength(128);
            entity.Property(e => e.BusinessMeaning).HasMaxLength(500);
            entity.Property(e => e.BusinessContext).HasMaxLength(1000);
            entity.Property(e => e.ValidationRules).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);

            // Foreign key relationship
            entity.HasOne(c => c.TableInfo)
                .WithMany(t => t.Columns)
                .HasForeignKey(c => c.TableInfoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure QueryPatterns
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
            entity.HasIndex(e => new { e.IsActive, e.LastUsedDate });
            entity.Property(e => e.PatternName).HasMaxLength(200);
            entity.Property(e => e.NaturalLanguagePattern).HasMaxLength(1000);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.BusinessContext).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
        });

        // Configure BusinessGlossary
        modelBuilder.Entity<BusinessGlossaryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Term)
                .HasDatabaseName("IX_BusinessGlossary_Term")
                .IsUnique();
            entity.HasIndex(e => e.Category)
                .HasDatabaseName("IX_BusinessGlossary_Category");
            entity.HasIndex(e => new { e.IsActive, e.Category });
            entity.Property(e => e.Term).HasMaxLength(200);
            entity.Property(e => e.Definition).HasMaxLength(1000);
            entity.Property(e => e.BusinessContext).HasMaxLength(1000);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
        });

        // Configure AITuningSettings
        modelBuilder.Entity<AITuningSettingsEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SettingKey)
                .HasDatabaseName("IX_AITuningSettings_SettingKey")
                .IsUnique();
            entity.HasIndex(e => e.Category)
                .HasDatabaseName("IX_AITuningSettings_Category");
            entity.Property(e => e.SettingKey).HasMaxLength(100);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(256);
        });

        // Configure PromptTemplates
        modelBuilder.Entity<PromptTemplateEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.Name, e.Version }).IsUnique();
            entity.HasIndex(e => new { e.IsActive, e.Name });
            entity.HasIndex(e => e.UsageCount);
            entity.Property(e => e.Name).HasMaxLength(100);
            entity.Property(e => e.Version).HasMaxLength(20);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
        });

        // Configure PromptLogs
        modelBuilder.Entity<PromptLogEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.TemplateId, e.Timestamp });
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.PromptHash).HasMaxLength(64);
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

        // Configure Unified AI Learning entities
        modelBuilder.Entity<UnifiedAIGenerationAttempt>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.AttemptedAt);
            entity.HasIndex(e => new { e.IsSuccessful, e.AttemptedAt });
            entity.Property(e => e.UserId).HasMaxLength(500);
            entity.Property(e => e.AIProvider).HasMaxLength(100);
            entity.Property(e => e.ModelVersion).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(500);
        });

        modelBuilder.Entity<UnifiedAIFeedbackEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.CreatedAt });
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.FeedbackType, e.CreatedAt });
            entity.Property(e => e.UserId).HasMaxLength(500);
            entity.Property(e => e.FeedbackType).HasMaxLength(50);
            entity.Property(e => e.Category).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(500);
            entity.Property(e => e.UpdatedBy).HasMaxLength(500);
        });

        // Seed default data
        SeedTuningData(modelBuilder);
    }

    private static void SeedTuningData(ModelBuilder modelBuilder)
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

        // Seed default prompt template
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
    }
}
