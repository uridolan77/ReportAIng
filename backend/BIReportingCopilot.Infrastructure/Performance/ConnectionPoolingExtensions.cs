using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Data.SqlClient;
using BIReportingCopilot.Infrastructure.Data;

namespace BIReportingCopilot.Infrastructure.Performance;

public static class ConnectionPoolingExtensions
{
    public static IServiceCollection AddOptimizedSqlConnections(
        this IServiceCollection services,
        string connectionString,
        int maxPoolSize = 100,
        int minPoolSize = 5)
    {
        var builder = new SqlConnectionStringBuilder(connectionString)
        {
            MaxPoolSize = maxPoolSize,
            MinPoolSize = minPoolSize,
            Pooling = true,
            MultipleActiveResultSets = true,
            ConnectTimeout = 30,
            CommandTimeout = 30,
            TrustServerCertificate = true
        };

        services.AddDbContext<BICopilotContext>(options =>
        {
            options.UseSqlServer(builder.ConnectionString, sqlOptions =>
            {
                sqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorNumbersToAdd: null);
                
                sqlOptions.CommandTimeout(30);
                sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
            });

            // Enable query tracking behavior
            options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTrackingWithIdentityResolution);

            // Enable detailed errors in development
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                options.EnableDetailedErrors();
                options.EnableSensitiveDataLogging();
            }
        });

        return services;
    }
}
