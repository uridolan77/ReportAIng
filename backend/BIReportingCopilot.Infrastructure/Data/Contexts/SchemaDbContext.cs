using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Data.Configurations;

namespace BIReportingCopilot.Infrastructure.Data.Contexts;

/// <summary>
/// Bounded context for schema management and metadata
/// </summary>
public class SchemaDbContext : DbContext
{
    public SchemaDbContext(DbContextOptions<SchemaDbContext> options) : base(options)
    {
    }

    // Schema metadata
    public DbSet<SchemaMetadataEntity> SchemaMetadata { get; set; }

    // Schema management entities
    public DbSet<BusinessSchema> BusinessSchemas { get; set; }
    public DbSet<BusinessSchemaVersion> BusinessSchemaVersions { get; set; }
    public DbSet<SchemaTableContext> SchemaTableContexts { get; set; }
    public DbSet<SchemaColumnContext> SchemaColumnContexts { get; set; }
    public DbSet<SchemaGlossaryTerm> SchemaGlossaryTerms { get; set; }
    public DbSet<SchemaRelationship> SchemaRelationships { get; set; }
    public DbSet<UserSchemaPreference> UserSchemaPreferences { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure SchemaMetadata
        modelBuilder.Entity<SchemaMetadataEntity>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.DatabaseName, e.SchemaName, e.TableName });
            entity.HasIndex(e => new { e.TableName, e.ColumnName });
            entity.HasIndex(e => new { e.SchemaName, e.TableName, e.ColumnName });
            entity.HasIndex(e => e.LastUpdated);
            entity.Property(e => e.DatabaseName).HasMaxLength(128);
            entity.Property(e => e.SchemaName).HasMaxLength(128);
            entity.Property(e => e.TableName).HasMaxLength(128);
            entity.Property(e => e.ColumnName).HasMaxLength(128);
            entity.Property(e => e.DataType).HasMaxLength(50);
            entity.Property(e => e.SemanticTags).HasMaxLength(1000);
            entity.Property(e => e.SampleValues).HasMaxLength(2000);
            entity.Property(e => e.BusinessDescription).HasMaxLength(500);
        });

        // Apply schema management configurations
        modelBuilder.ApplyConfiguration(new BusinessSchemaConfiguration());
        modelBuilder.ApplyConfiguration(new BusinessSchemaVersionConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaTableContextConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaColumnContextConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaGlossaryTermConfiguration());
        modelBuilder.ApplyConfiguration(new SchemaRelationshipConfiguration());
        modelBuilder.ApplyConfiguration(new UserSchemaPreferenceConfiguration());

        // Additional indexes for performance
        ConfigureAdditionalIndexes(modelBuilder);
    }

    private static void ConfigureAdditionalIndexes(ModelBuilder modelBuilder)
    {
        // BusinessSchema additional indexes
        modelBuilder.Entity<BusinessSchema>(entity =>
        {
            entity.HasIndex(e => new { e.IsActive, e.CreatedAt });
            entity.HasIndex(e => new { e.Name, e.IsActive });
        });

        // BusinessSchemaVersion additional indexes
        modelBuilder.Entity<BusinessSchemaVersion>(entity =>
        {
            entity.HasIndex(e => new { e.SchemaId, e.VersionNumber });
            entity.HasIndex(e => new { e.IsActive, e.CreatedAt });
        });

        // SchemaTableContext additional indexes
        modelBuilder.Entity<SchemaTableContext>(entity =>
        {
            entity.HasIndex(e => new { e.SchemaVersionId, e.TableName });
            entity.HasIndex(e => new { e.IsActive, e.TableName });
        });

        // SchemaColumnContext additional indexes
        modelBuilder.Entity<SchemaColumnContext>(entity =>
        {
            entity.HasIndex(e => new { e.TableContextId, e.ColumnName });
            entity.HasIndex(e => new { e.IsActive, e.ColumnName });
        });

        // SchemaGlossaryTerm additional indexes
        modelBuilder.Entity<SchemaGlossaryTerm>(entity =>
        {
            entity.HasIndex(e => new { e.SchemaVersionId, e.Term });
            entity.HasIndex(e => new { e.IsActive, e.Category });
        });

        // SchemaRelationship additional indexes
        modelBuilder.Entity<SchemaRelationship>(entity =>
        {
            entity.HasIndex(e => new { e.SchemaVersionId, e.SourceTableId });
            entity.HasIndex(e => new { e.SchemaVersionId, e.TargetTableId });
            entity.HasIndex(e => new { e.IsActive, e.RelationshipType });
        });

        // UserSchemaPreference additional indexes
        modelBuilder.Entity<UserSchemaPreference>(entity =>
        {
            entity.HasIndex(e => new { e.UserId, e.SchemaId });
            entity.HasIndex(e => new { e.IsActive, e.LastUsed });
        });
    }
}
