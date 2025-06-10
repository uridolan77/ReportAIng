using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Infrastructure.Data.Configurations;

public class BusinessSchemaConfiguration : IEntityTypeConfiguration<BusinessSchema>
{
    public void Configure(EntityTypeBuilder<BusinessSchema> builder)
    {
        builder.ToTable("BusinessSchemas");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.Description)
            .HasMaxLength(500);
            
        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(e => e.UpdatedBy)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);
            
        builder.Property(e => e.IsDefault)
            .HasDefaultValue(false);
            
        builder.Property(e => e.Tags)
            .HasMaxLength(500);
            
        builder.HasIndex(e => e.Name)
            .IsUnique()
            .HasDatabaseName("UK_BusinessSchemas_Name");
            
        builder.HasIndex(e => e.IsActive)
            .HasDatabaseName("IX_BusinessSchemas_IsActive");
            
        builder.HasIndex(e => e.IsDefault)
            .HasDatabaseName("IX_BusinessSchemas_IsDefault");
            
        // Relationships
        builder.HasMany(e => e.Versions)
            .WithOne(e => e.Schema)
            .HasForeignKey(e => e.SchemaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class BusinessSchemaVersionConfiguration : IEntityTypeConfiguration<BusinessSchemaVersion>
{
    public void Configure(EntityTypeBuilder<BusinessSchemaVersion> builder)
    {
        builder.ToTable("BusinessSchemaVersions");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(e => e.VersionNumber)
            .IsRequired();
            
        builder.Property(e => e.VersionName)
            .HasMaxLength(50);
            
        builder.Property(e => e.Description)
            .HasMaxLength(1000);
            
        builder.Property(e => e.CreatedBy)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(e => e.IsActive)
            .HasDefaultValue(true);
            
        builder.Property(e => e.IsCurrent)
            .HasDefaultValue(false);
            
        builder.HasIndex(e => new { e.SchemaId, e.VersionNumber })
            .IsUnique()
            .HasDatabaseName("UK_BusinessSchemaVersions_SchemaVersion");
            
        builder.HasIndex(e => new { e.SchemaId, e.IsCurrent })
            .HasDatabaseName("IX_BusinessSchemaVersions_SchemaId_IsCurrent");
            
        // Relationships
        builder.HasMany(e => e.TableContexts)
            .WithOne(e => e.SchemaVersion)
            .HasForeignKey(e => e.SchemaVersionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(e => e.GlossaryTerms)
            .WithOne(e => e.SchemaVersion)
            .HasForeignKey(e => e.SchemaVersionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(e => e.Relationships)
            .WithOne(e => e.SchemaVersion)
            .HasForeignKey(e => e.SchemaVersionId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(e => e.UserPreferences)
            .WithOne(e => e.SchemaVersion)
            .HasForeignKey(e => e.SchemaVersionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SchemaTableContextConfiguration : IEntityTypeConfiguration<SchemaTableContext>
{
    public void Configure(EntityTypeBuilder<SchemaTableContext> builder)
    {
        builder.ToTable("SchemaTableContexts");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(e => e.TableName)
            .IsRequired()
            .HasMaxLength(128);
            
        builder.Property(e => e.SchemaName)
            .HasMaxLength(128)
            .HasDefaultValue("dbo");
            
        builder.Property(e => e.BusinessPurpose)
            .HasMaxLength(500);
            
        builder.Property(e => e.PrimaryUseCase)
            .HasMaxLength(500);
            
        builder.Property(e => e.ConfidenceScore)
            .HasColumnType("decimal(3,2)");
            
        builder.Property(e => e.IsAutoGenerated)
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.HasIndex(e => new { e.SchemaVersionId, e.SchemaName, e.TableName })
            .IsUnique()
            .HasDatabaseName("UK_SchemaTableContexts_VersionTable");
            
        builder.HasIndex(e => e.SchemaVersionId)
            .HasDatabaseName("IX_SchemaTableContexts_SchemaVersionId");
            
        // Relationships
        builder.HasMany(e => e.ColumnContexts)
            .WithOne(e => e.TableContext)
            .HasForeignKey(e => e.TableContextId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SchemaColumnContextConfiguration : IEntityTypeConfiguration<SchemaColumnContext>
{
    public void Configure(EntityTypeBuilder<SchemaColumnContext> builder)
    {
        builder.ToTable("SchemaColumnContexts");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(e => e.ColumnName)
            .IsRequired()
            .HasMaxLength(128);
            
        builder.Property(e => e.BusinessName)
            .HasMaxLength(200);
            
        builder.Property(e => e.BusinessDescription)
            .HasMaxLength(1000);
            
        builder.Property(e => e.BusinessDataType)
            .HasMaxLength(50);
            
        builder.Property(e => e.IsKeyColumn)
            .HasDefaultValue(false);
            
        builder.Property(e => e.IsPrimaryKey)
            .HasDefaultValue(false);
            
        builder.Property(e => e.IsForeignKey)
            .HasDefaultValue(false);
            
        builder.Property(e => e.ConfidenceScore)
            .HasColumnType("decimal(3,2)");
            
        builder.Property(e => e.IsAutoGenerated)
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.HasIndex(e => new { e.TableContextId, e.ColumnName })
            .IsUnique()
            .HasDatabaseName("UK_SchemaColumnContexts_TableColumn");
            
        builder.HasIndex(e => e.TableContextId)
            .HasDatabaseName("IX_SchemaColumnContexts_TableContextId");
    }
}

public class SchemaGlossaryTermConfiguration : IEntityTypeConfiguration<SchemaGlossaryTerm>
{
    public void Configure(EntityTypeBuilder<SchemaGlossaryTerm> builder)
    {
        builder.ToTable("SchemaGlossaryTerms");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(e => e.Term)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(e => e.Definition)
            .IsRequired();
            
        builder.Property(e => e.BusinessContext)
            .HasMaxLength(1000);
            
        builder.Property(e => e.Category)
            .HasMaxLength(100);
            
        builder.Property(e => e.ConfidenceScore)
            .HasColumnType("decimal(3,2)");
            
        builder.Property(e => e.IsAutoGenerated)
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.HasIndex(e => new { e.SchemaVersionId, e.Term })
            .IsUnique()
            .HasDatabaseName("UK_SchemaGlossaryTerms_VersionTerm");
            
        builder.HasIndex(e => e.SchemaVersionId)
            .HasDatabaseName("IX_SchemaGlossaryTerms_SchemaVersionId");
    }
}

public class SchemaRelationshipConfiguration : IEntityTypeConfiguration<SchemaRelationship>
{
    public void Configure(EntityTypeBuilder<SchemaRelationship> builder)
    {
        builder.ToTable("SchemaRelationships");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(e => e.FromTable)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(e => e.ToTable)
            .IsRequired()
            .HasMaxLength(256);
            
        builder.Property(e => e.RelationshipType)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(e => e.BusinessDescription)
            .HasMaxLength(1000);
            
        builder.Property(e => e.ConfidenceScore)
            .HasColumnType("decimal(3,2)");
            
        builder.Property(e => e.IsAutoGenerated)
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.HasIndex(e => e.SchemaVersionId)
            .HasDatabaseName("IX_SchemaRelationships_SchemaVersionId");
    }
}

public class UserSchemaPreferenceConfiguration : IEntityTypeConfiguration<UserSchemaPreference>
{
    public void Configure(EntityTypeBuilder<UserSchemaPreference> builder)
    {
        builder.ToTable("UserSchemaPreferences");
        
        builder.HasKey(e => e.Id);
        
        builder.Property(e => e.Id)
            .HasDefaultValueSql("NEWID()");
            
        builder.Property(e => e.UserId)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(e => e.IsDefault)
            .HasDefaultValue(false);
            
        builder.Property(e => e.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.Property(e => e.UpdatedAt)
            .HasDefaultValueSql("GETUTCDATE()");
            
        builder.HasIndex(e => new { e.UserId, e.SchemaVersionId })
            .IsUnique()
            .HasDatabaseName("UK_UserSchemaPreferences_User");
            
        builder.HasIndex(e => e.UserId)
            .HasDatabaseName("IX_UserSchemaPreferences_UserId");
    }
}
