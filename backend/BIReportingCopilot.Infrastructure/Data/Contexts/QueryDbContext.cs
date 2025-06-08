using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Models;
using CoreQuerySuggestion = BIReportingCopilot.Core.Models.QuerySuggestion;
using BIReportingCopilot.Core.Models.QuerySuggestions;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Data.Contexts;

/// <summary>
/// Bounded context for query execution, caching, and suggestions
/// </summary>
public class QueryDbContext : DbContext
{
    public QueryDbContext(DbContextOptions<QueryDbContext> options) : base(options)
    {
    }

    // Query execution and history
    public DbSet<Entities.QueryHistoryEntity> QueryHistory { get; set; }
    public DbSet<Core.Models.QueryHistoryEntity> QueryHistories { get; set; } // Core version for compatibility
    public DbSet<Core.Models.QueryHistoryEntity> QueryExecutionLogs { get; set; }

    // Query caching
    public DbSet<QueryCacheEntity> QueryCache { get; set; }
    public DbSet<SemanticCacheEntry> SemanticCacheEntries { get; set; }

    // Query performance
    public DbSet<QueryPerformanceEntity> QueryPerformance { get; set; }

    // Query suggestions system
    public DbSet<SuggestionCategory> SuggestionCategories { get; set; }
    public DbSet<Core.Models.QuerySuggestions.QuerySuggestion> QuerySuggestions { get; set; }
    public DbSet<SuggestionUsageAnalytics> SuggestionUsageAnalytics { get; set; }
    public DbSet<TimeFrameDefinition> TimeFrameDefinitions { get; set; }

    // Temporary files
    public DbSet<TempFile> TempFiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure QueryHistory (Infrastructure version)
        modelBuilder.Entity<Entities.QueryHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.QueryTimestamp });
            entity.HasIndex(e => e.SessionId);
            entity.HasIndex(e => e.QueryTimestamp);
            entity.HasIndex(e => new { e.IsSuccessful, e.QueryTimestamp });
            entity.Property(e => e.NaturalLanguageQuery).HasMaxLength(2000);
            entity.Property(e => e.ErrorMessage).HasMaxLength(1000);
            entity.Property(e => e.UserId).HasMaxLength(256);
            entity.Property(e => e.SessionId).HasMaxLength(50);
        });

        // Configure QueryHistory (Core version)
        modelBuilder.Entity<Core.Models.QueryHistoryEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.ExecutedAt);
            entity.HasIndex(e => new { e.UserId, e.ExecutedAt });
            entity.Property(e => e.UserId).HasMaxLength(500);
            entity.Property(e => e.Query).HasMaxLength(2000);
        });

        // Configure QueryCache
        modelBuilder.Entity<QueryCacheEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QueryHash).IsUnique();
            entity.HasIndex(e => e.ExpiryTimestamp);
            entity.HasIndex(e => e.LastAccessedDate);
            entity.HasIndex(e => new { e.ExpiryTimestamp, e.LastAccessedDate });
            entity.Property(e => e.QueryHash).HasMaxLength(64);
            entity.Property(e => e.ResultMetadata).HasMaxLength(2000);
        });

        // Configure SemanticCacheEntry
        modelBuilder.Entity<SemanticCacheEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QueryHash).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.LastAccessedAt);
            entity.HasIndex(e => new { e.ExpiresAt, e.LastAccessedAt });
            entity.Property(e => e.QueryHash).HasMaxLength(100);
        });

        // Configure QueryPerformance
        modelBuilder.Entity<QueryPerformanceEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QueryHash);
            entity.HasIndex(e => e.Timestamp);
            entity.HasIndex(e => new { e.QueryHash, e.Timestamp });
            entity.Property(e => e.QueryHash).HasMaxLength(64);
            entity.Property(e => e.UserId).HasMaxLength(256);
        });

        // Configure TempFile
        modelBuilder.Entity<TempFile>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ExpiresAt);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => new { e.ExpiresAt, e.CreatedAt });
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.ContentType).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(256);
        });

        // Configure Query Suggestions entities
        ConfigureQuerySuggestions(modelBuilder);
    }

    private static void ConfigureQuerySuggestions(ModelBuilder modelBuilder)
    {
        // SuggestionCategory configuration
        modelBuilder.Entity<SuggestionCategory>(entity =>
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
            entity.Property(e => e.Description).IsRequired().HasMaxLength(200);
            entity.Property(e => e.DefaultTimeFrame).HasMaxLength(50);
            entity.Property(e => e.TargetTables).HasMaxLength(500);
            entity.Property(e => e.RequiredPermissions).HasMaxLength(200);
            entity.Property(e => e.Tags).HasMaxLength(300);
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
        modelBuilder.Entity<SuggestionUsageAnalytics>(entity =>
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
        modelBuilder.Entity<TimeFrameDefinition>(entity =>
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
    }
}
