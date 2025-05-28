using System.Text.Json.Serialization;

namespace BIReportingCopilot.Core.Models;

/// <summary>
/// Standardized API response wrapper
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// Indicates whether the operation was successful
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// The response data (null if operation failed)
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error information (null if operation succeeded)
    /// </summary>
    public ApiError? Error { get; set; }

    /// <summary>
    /// Additional metadata about the response
    /// </summary>
    public ApiMetadata? Metadata { get; set; }

    /// <summary>
    /// Timestamp when the response was generated
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// API version used to generate this response
    /// </summary>
    public string ApiVersion { get; set; } = "1.0";

    /// <summary>
    /// Unique identifier for this request/response
    /// </summary>
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Create a successful response
    /// </summary>
    public static ApiResponse<T> Ok(T data, ApiMetadata? metadata = null)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data,
            Metadata = metadata
        };
    }

    /// <summary>
    /// Create a successful response with no data
    /// </summary>
    public static ApiResponse<object> Ok()
    {
        return new ApiResponse<object>
        {
            Success = true,
            Data = new { }
        };
    }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static ApiResponse<T> Error(string code, string message, object? details = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details
            }
        };
    }

    /// <summary>
    /// Create an error response with validation errors
    /// </summary>
    public static ApiResponse<T> ValidationError(Dictionary<string, string[]> validationErrors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = new ApiError
            {
                Code = "VALIDATION_ERROR",
                Message = "One or more validation errors occurred",
                Details = validationErrors
            }
        };
    }

    /// <summary>
    /// Create an error response from an exception
    /// </summary>
    public static ApiResponse<T> FromException(Exception exception, bool includeStackTrace = false)
    {
        var error = new ApiError
        {
            Code = exception.GetType().Name.Replace("Exception", "").ToUpperInvariant(),
            Message = exception.Message
        };

        if (includeStackTrace)
        {
            error.Details = new { StackTrace = exception.StackTrace };
        }

        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }
}

/// <summary>
/// Non-generic version for responses without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    /// <summary>
    /// Create a successful response with no data
    /// </summary>
    public static new ApiResponse Ok()
    {
        return new ApiResponse
        {
            Success = true,
            Data = new { }
        };
    }

    /// <summary>
    /// Create an error response
    /// </summary>
    public static new ApiResponse Error(string code, string message, object? details = null)
    {
        return new ApiResponse
        {
            Success = false,
            Error = new ApiError
            {
                Code = code,
                Message = message,
                Details = details
            }
        };
    }
}

/// <summary>
/// Error information for API responses
/// </summary>
public class ApiError
{
    /// <summary>
    /// Error code (e.g., "VALIDATION_ERROR", "NOT_FOUND", "UNAUTHORIZED")
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (validation errors, stack trace, etc.)
    /// </summary>
    public object? Details { get; set; }

    /// <summary>
    /// Link to documentation about this error
    /// </summary>
    public string? HelpUrl { get; set; }

    /// <summary>
    /// Unique identifier for this specific error occurrence
    /// </summary>
    public string ErrorId { get; set; } = Guid.NewGuid().ToString();
}

/// <summary>
/// Metadata for API responses
/// </summary>
public class ApiMetadata
{
    /// <summary>
    /// Total number of items (for paginated responses)
    /// </summary>
    public int? TotalCount { get; set; }

    /// <summary>
    /// Current page number (for paginated responses)
    /// </summary>
    public int? Page { get; set; }

    /// <summary>
    /// Number of items per page (for paginated responses)
    /// </summary>
    public int? PageSize { get; set; }

    /// <summary>
    /// Total number of pages (for paginated responses)
    /// </summary>
    public int? TotalPages { get; set; }

    /// <summary>
    /// Indicates if there are more pages available
    /// </summary>
    public bool? HasNextPage { get; set; }

    /// <summary>
    /// Indicates if there are previous pages available
    /// </summary>
    public bool? HasPreviousPage { get; set; }

    /// <summary>
    /// Time taken to process the request (in milliseconds)
    /// </summary>
    public long? ProcessingTimeMs { get; set; }

    /// <summary>
    /// Cache information
    /// </summary>
    public CacheInfo? Cache { get; set; }

    /// <summary>
    /// Additional custom metadata
    /// </summary>
    public Dictionary<string, object>? Custom { get; set; }
}

/// <summary>
/// Cache information for API responses
/// </summary>
public class CacheInfo
{
    /// <summary>
    /// Indicates if the response was served from cache
    /// </summary>
    public bool FromCache { get; set; }

    /// <summary>
    /// Cache key used for this response
    /// </summary>
    public string? CacheKey { get; set; }

    /// <summary>
    /// When the cached data expires
    /// </summary>
    public DateTime? ExpiresAt { get; set; }

    /// <summary>
    /// How long the data should be cached (in seconds)
    /// </summary>
    public int? MaxAge { get; set; }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
/// <typeparam name="T">The type of items in the collection</typeparam>
public class PagedApiResponse<T> : ApiResponse<IEnumerable<T>>
{
    /// <summary>
    /// Create a paginated response
    /// </summary>
    public static PagedApiResponse<T> Create(
        IEnumerable<T> items,
        int totalCount,
        int page,
        int pageSize,
        long? processingTimeMs = null)
    {
        var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
        
        return new PagedApiResponse<T>
        {
            Success = true,
            Data = items,
            Metadata = new ApiMetadata
            {
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                HasNextPage = page < totalPages,
                HasPreviousPage = page > 1,
                ProcessingTimeMs = processingTimeMs
            }
        };
    }
}

/// <summary>
/// Standard error codes used throughout the API
/// </summary>
public static class ApiErrorCodes
{
    // Authentication & Authorization
    public const string UNAUTHORIZED = "UNAUTHORIZED";
    public const string FORBIDDEN = "FORBIDDEN";
    public const string INVALID_TOKEN = "INVALID_TOKEN";
    public const string TOKEN_EXPIRED = "TOKEN_EXPIRED";
    public const string INSUFFICIENT_PERMISSIONS = "INSUFFICIENT_PERMISSIONS";

    // Validation
    public const string VALIDATION_ERROR = "VALIDATION_ERROR";
    public const string INVALID_INPUT = "INVALID_INPUT";
    public const string MISSING_REQUIRED_FIELD = "MISSING_REQUIRED_FIELD";
    public const string INVALID_FORMAT = "INVALID_FORMAT";

    // Resource Management
    public const string NOT_FOUND = "NOT_FOUND";
    public const string ALREADY_EXISTS = "ALREADY_EXISTS";
    public const string CONFLICT = "CONFLICT";
    public const string RESOURCE_LOCKED = "RESOURCE_LOCKED";

    // Business Logic
    public const string BUSINESS_RULE_VIOLATION = "BUSINESS_RULE_VIOLATION";
    public const string OPERATION_NOT_ALLOWED = "OPERATION_NOT_ALLOWED";
    public const string INVALID_STATE = "INVALID_STATE";
    public const string QUOTA_EXCEEDED = "QUOTA_EXCEEDED";

    // System Errors
    public const string INTERNAL_ERROR = "INTERNAL_ERROR";
    public const string SERVICE_UNAVAILABLE = "SERVICE_UNAVAILABLE";
    public const string TIMEOUT = "TIMEOUT";
    public const string RATE_LIMITED = "RATE_LIMITED";

    // AI/ML Specific
    public const string AI_SERVICE_ERROR = "AI_SERVICE_ERROR";
    public const string QUERY_GENERATION_FAILED = "QUERY_GENERATION_FAILED";
    public const string CONFIDENCE_TOO_LOW = "CONFIDENCE_TOO_LOW";
    public const string UNSAFE_QUERY = "UNSAFE_QUERY";

    // Database Specific
    public const string DATABASE_ERROR = "DATABASE_ERROR";
    public const string QUERY_EXECUTION_FAILED = "QUERY_EXECUTION_FAILED";
    public const string SCHEMA_NOT_FOUND = "SCHEMA_NOT_FOUND";
    public const string TABLE_NOT_FOUND = "TABLE_NOT_FOUND";
}

/// <summary>
/// Extension methods for creating common API responses
/// </summary>
public static class ApiResponseExtensions
{
    /// <summary>
    /// Create a not found response
    /// </summary>
    public static ApiResponse<T> NotFound<T>(string resource, string identifier)
    {
        return ApiResponse<T>.Error(
            ApiErrorCodes.NOT_FOUND,
            $"{resource} with identifier '{identifier}' was not found");
    }

    /// <summary>
    /// Create an unauthorized response
    /// </summary>
    public static ApiResponse<T> Unauthorized<T>(string? message = null)
    {
        return ApiResponse<T>.Error(
            ApiErrorCodes.UNAUTHORIZED,
            message ?? "Authentication is required to access this resource");
    }

    /// <summary>
    /// Create a forbidden response
    /// </summary>
    public static ApiResponse<T> Forbidden<T>(string? message = null)
    {
        return ApiResponse<T>.Error(
            ApiErrorCodes.FORBIDDEN,
            message ?? "You do not have permission to access this resource");
    }

    /// <summary>
    /// Create a rate limited response
    /// </summary>
    public static ApiResponse<T> RateLimited<T>(TimeSpan retryAfter)
    {
        return ApiResponse<T>.Error(
            ApiErrorCodes.RATE_LIMITED,
            "Rate limit exceeded. Please try again later.",
            new { RetryAfterSeconds = (int)retryAfter.TotalSeconds });
    }

    /// <summary>
    /// Create a service unavailable response
    /// </summary>
    public static ApiResponse<T> ServiceUnavailable<T>(string? message = null)
    {
        return ApiResponse<T>.Error(
            ApiErrorCodes.SERVICE_UNAVAILABLE,
            message ?? "The service is temporarily unavailable. Please try again later.");
    }
}
