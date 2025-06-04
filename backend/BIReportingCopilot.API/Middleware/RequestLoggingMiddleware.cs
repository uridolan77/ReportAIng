using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Constants;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace BIReportingCopilot.API.Middleware;

/// <summary>
/// Middleware for logging HTTP requests and responses with performance metrics and request body content
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly IConfiguration _configuration;
    private readonly bool _enableRequestBodyLogging;
    private readonly int _maxRequestBodySize;
    private readonly HashSet<string> _queryEndpoints;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        _configuration = configuration;
        _enableRequestBodyLogging = _configuration.GetValue<bool>("Logging:EnableRequestBodyLogging", true);
        _maxRequestBodySize = _configuration.GetValue<int>("Logging:MaxRequestBodySize", 8192); // 8KB default

        // Define endpoints that should have their request body logged
        _queryEndpoints = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "/api/unifiedquery/execute",
            "/api/unifiedquery/enhanced",
            "/api/unifiedquery/streaming",
            "/api/unifiedquery/analyze",
            "/api/unifiedquery/classify",
            "/api/tuning/auto-generate",
            "/api/schema/analyze"
        };
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString();

        // Add request ID to response headers using constants
        context.Response.Headers[ApplicationConstants.Headers.RequestId] = requestId;

        // Log basic request information
        _logger.LogInformation("Request {RequestId} started: {Method} {Path}",
            requestId, context.Request.Method, context.Request.Path);

        // Log request body for query endpoints
        await LogRequestBodyIfApplicable(context, requestId);

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

    private async Task LogRequestBodyIfApplicable(HttpContext context, string requestId)
    {
        if (!_enableRequestBodyLogging ||
            context.Request.Method != "POST" ||
            !_queryEndpoints.Contains(context.Request.Path.Value ?? string.Empty))
        {
            return;
        }

        try
        {
            // Enable buffering to allow multiple reads of the request body
            context.Request.EnableBuffering();

            // Read the request body
            var buffer = new byte[_maxRequestBodySize];
            var bytesRead = await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);

            // Reset the stream position for the next middleware
            context.Request.Body.Position = 0;

            if (bytesRead > 0)
            {
                var requestBody = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                // Try to parse and extract query information
                var queryInfo = ExtractQueryInformation(requestBody);

                if (!string.IsNullOrEmpty(queryInfo))
                {
                    _logger.LogInformation("Request {RequestId} query content: {QueryContent}",
                        requestId, queryInfo);
                }
                else
                {
                    // Log raw body if we can't extract query info (truncated for safety)
                    var truncatedBody = requestBody.Length > 500 ? requestBody.Substring(0, 500) + "..." : requestBody;
                    _logger.LogInformation("Request {RequestId} body: {RequestBody}",
                        requestId, truncatedBody);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to log request body for request {RequestId}", requestId);
        }
    }

    private string ExtractQueryInformation(string requestBody)
    {
        try
        {
            using var document = JsonDocument.Parse(requestBody);
            var root = document.RootElement;

            // Try different property names for query content
            var queryProperties = new[] { "question", "query", "naturalLanguageQuery", "prompt" };

            foreach (var property in queryProperties)
            {
                if (root.TryGetProperty(property, out var queryElement) &&
                    queryElement.ValueKind == JsonValueKind.String)
                {
                    var queryText = queryElement.GetString();
                    if (!string.IsNullOrWhiteSpace(queryText))
                    {
                        // Truncate long queries for logging
                        return queryText.Length > 1000 ? queryText.Substring(0, 1000) + "..." : queryText;
                    }
                }
            }

            return string.Empty;
        }
        catch
        {
            // If JSON parsing fails, return empty string
            return string.Empty;
        }
    }
}
