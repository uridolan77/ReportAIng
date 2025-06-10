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

        // Register unified background job management service
        services.AddScoped<BackgroundJobManagementService>();
    }

    public static void ConfigureRecurringJobs(this IApplicationBuilder app)
    {
        var recurringJobManager = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();

        // Schema refresh - hourly using unified service
        recurringJobManager.AddOrUpdate<BackgroundJobManagementService>(
            "schema-refresh",
            service => service.RefreshAllSchemasAsync(),
            Cron.Hourly());

        // System cleanup - daily at 2 AM using unified service
        recurringJobManager.AddOrUpdate<BackgroundJobManagementService>(
            "system-cleanup",
            service => service.PerformSystemCleanupAsync(),
            Cron.Daily(2));
    }
}
