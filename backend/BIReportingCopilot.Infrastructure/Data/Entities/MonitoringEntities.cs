using System.ComponentModel.DataAnnotations;

namespace BIReportingCopilot.Infrastructure.Data.Entities;

/// <summary>
/// Enhanced performance metrics entity for detailed monitoring
/// </summary>
public class PerformanceMetricsEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string OperationType { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? CorrelationId { get; set; }
    public bool IsSuccessful { get; set; }
    public string? AdditionalData { get; set; }
    public DateTime? RetentionDate { get; set; }
}

/// <summary>
/// Error logging entity for centralized error tracking
/// </summary>
public class ErrorLogEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? CorrelationId { get; set; }
    public string? ErrorCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
    public DateTime? RetentionDate { get; set; }
}

/// <summary>
/// Health check logging entity
/// </summary>
public class HealthCheckLogEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string CheckName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long DurationMs { get; set; }
    public string? Description { get; set; }
    public string? Data { get; set; }
    public DateTime? RetentionDate { get; set; }
}

/// <summary>
/// User activity tracking entity
/// </summary>
public class UserActivityEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? SessionId { get; set; }
    public string ActivityType { get; set; } = string.Empty;
    public string ActivityName { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? AdditionalData { get; set; }
    public DateTime? RetentionDate { get; set; }
}

/// <summary>
/// Feature usage analytics entity
/// </summary>
public class FeatureUsageEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string FeatureName { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? SessionId { get; set; }
    public string? FeatureVersion { get; set; }
    public long UsageDurationMs { get; set; }
    public bool IsSuccessful { get; set; }
    public string? Context { get; set; }
    public DateTime? RetentionDate { get; set; }
}

/// <summary>
/// API usage tracking entity
/// </summary>
public class ApiUsageEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string Method { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public long ResponseTimeMs { get; set; }
    public string? UserId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? RequestId { get; set; }
    public long RequestSize { get; set; }
    public long ResponseSize { get; set; }
    public DateTime? RetentionDate { get; set; }
}

/// <summary>
/// Resource usage monitoring entity
/// </summary>
public class ResourceUsageEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string ResourceType { get; set; } = string.Empty;
    public string ResourceName { get; set; } = string.Empty;
    public double UsageValue { get; set; }
    public double MaxValue { get; set; }
    public double PercentageUsed { get; set; }
    public string Unit { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public DateTime? RetentionDate { get; set; }
}

/// <summary>
/// Database connection monitoring entity
/// </summary>
public class DatabaseConnectionEntity
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public string DatabaseName { get; set; } = string.Empty;
    public string ConnectionString { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = string.Empty;
    public long ConnectionTimeMs { get; set; }
    public int ActiveConnections { get; set; }
    public int MaxConnections { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? RetentionDate { get; set; }
}
