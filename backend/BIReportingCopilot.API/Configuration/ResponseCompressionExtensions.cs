using Microsoft.AspNetCore.ResponseCompression;
using System.IO.Compression;

namespace BIReportingCopilot.API.Configuration;

public static class ResponseCompressionExtensions
{
    public static IServiceCollection AddOptimizedResponseCompression(
        this IServiceCollection services)
    {
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[]
            {
                "application/json",
                "application/json; charset=utf-8",
                "text/plain",
                "text/csv",
                "application/octet-stream"
            });
        });

        services.Configure<BrotliCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Fastest;
        });

        services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        return services;
    }
}
