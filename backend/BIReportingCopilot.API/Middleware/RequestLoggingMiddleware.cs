using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Constants;
using System.Diagnostics;

namespace BIReportingCopilot.API.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with performance metrics
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        // Add request ID to response headers using constants
        context.Response.Headers[ApplicationConstants.Headers.RequestId] = requestId;

        _logger.LogInformation("Request {RequestId} started: {Method} {Path}",
            requestId, context.Request.Method, context.Request.Path);

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();
            _logger.LogInformation("Request {RequestId} completed in {ElapsedMs}ms with status {StatusCode}",
                requestId, stopwatch.ElapsedMilliseconds, context.Response.StatusCode);
        }
    }
}
