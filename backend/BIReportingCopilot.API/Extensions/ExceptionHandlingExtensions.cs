using Microsoft.AspNetCore.Builder;

namespace BIReportingCopilot.API.Extensions;

/// <summary>
/// Exception handling extension methods
/// </summary>
public static class ExceptionHandlingExtensions
{
    /// <summary>
    /// Use global exception handler
    /// </summary>
    /// <param name="app">Application builder</param>
    /// <returns>Application builder</returns>
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        // Use the built-in exception handler for now
        app.UseExceptionHandler("/error");
        
        return app;
    }
}
