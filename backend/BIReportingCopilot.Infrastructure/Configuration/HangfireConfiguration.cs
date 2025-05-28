using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BIReportingCopilot.Infrastructure.Jobs;

namespace BIReportingCopilot.Infrastructure.Configuration;

public static class HangfireConfiguration
{
    public static void ConfigureHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true
            }));

        services.AddHangfireServer(options =>
        {
            options.WorkerCount = Environment.ProcessorCount * 2;
            options.Queues = new[] { "critical", "default", "low" };
        });

        // Register job services
        services.AddScoped<ISchemaRefreshJob, SchemaRefreshJob>();
        services.AddScoped<ICleanupJob, CleanupJob>();
    }

    public static void ConfigureRecurringJobs(this IApplicationBuilder app)
    {
        var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

        // Schema refresh - daily at 2 AM
        recurringJobManager.AddOrUpdate<ISchemaRefreshJob>(
            "schema-refresh",
            job => job.RefreshAllSchemas(),
            Cron.Daily(2));

        // Cleanup job - daily at 4 AM
        recurringJobManager.AddOrUpdate<ICleanupJob>(
            "cleanup",
            job => job.PerformCleanup(),
            Cron.Daily(4));
    }
}
