using Microsoft.EntityFrameworkCore;
using BIReportingCopilot.Infrastructure.Data.Entities;

namespace BIReportingCopilot.Infrastructure.Data.Contexts
{
    /// <summary>
    /// Bounded context for AI transparency and explainability features
    /// Handles all transparency-related data persistence
    /// </summary>
    public class TransparencyDbContext : DbContext
    {
        public TransparencyDbContext(DbContextOptions<TransparencyDbContext> options) : base(options)
        {
        }

        // AI Transparency entities
        public DbSet<PromptConstructionTraceEntity> PromptConstructionTraces { get; set; }
        public DbSet<PromptConstructionStepEntity> PromptConstructionSteps { get; set; }
        public DbSet<TokenBudgetEntity> TokenBudgets { get; set; }
        public DbSet<BusinessContextProfileEntity> BusinessContextProfiles { get; set; }
        public DbSet<BusinessEntityEntity> BusinessEntities { get; set; }
        public DbSet<TransparencyReportEntity> TransparencyReports { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure PromptConstructionTrace relationships
            modelBuilder.Entity<PromptConstructionTraceEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TraceId).IsUnique();
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                
                entity.Property(e => e.OverallConfidence)
                    .HasPrecision(5, 4);
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
                
                entity.Property(e => e.UpdatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure PromptConstructionStep relationships
            modelBuilder.Entity<PromptConstructionStepEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TraceId);
                entity.HasIndex(e => e.CreatedAt);
                
                entity.Property(e => e.Confidence)
                    .HasPrecision(5, 4);
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");

                // Configure foreign key relationship
                entity.HasOne(e => e.Trace)
                    .WithMany(t => t.Steps)
                    .HasForeignKey(e => e.TraceId)
                    .HasPrincipalKey(t => t.TraceId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Configure TokenBudget
            modelBuilder.Entity<TokenBudgetEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                
                entity.Property(e => e.EstimatedCost)
                    .HasPrecision(10, 6);
                
                entity.Property(e => e.ActualCost)
                    .HasPrecision(10, 6);
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure BusinessContextProfile
            modelBuilder.Entity<BusinessContextProfileEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.CreatedAt);
                
                entity.Property(e => e.IntentConfidence)
                    .HasPrecision(5, 4);
                
                entity.Property(e => e.DomainConfidence)
                    .HasPrecision(5, 4);
                
                entity.Property(e => e.OverallConfidence)
                    .HasPrecision(5, 4);
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure BusinessEntity
            modelBuilder.Entity<BusinessEntityEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.ProfileId);
                entity.HasIndex(e => e.EntityType);
                entity.HasIndex(e => e.CreatedAt);
                
                entity.Property(e => e.Confidence)
                    .HasPrecision(5, 4);
                
                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });

            // Configure TransparencyReport
            modelBuilder.Entity<TransparencyReportEntity>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.TraceId);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.GeneratedAt);
                
                entity.Property(e => e.GeneratedAt)
                    .HasDefaultValueSql("GETUTCDATE()");
            });
        }

        /// <summary>
        /// Override SaveChanges for any future timestamp logic
        /// </summary>
        public override int SaveChanges()
        {
            return base.SaveChanges();
        }

        /// <summary>
        /// Override SaveChangesAsync for any future timestamp logic
        /// </summary>
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
