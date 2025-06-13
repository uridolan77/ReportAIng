using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Core.Interfaces.Security;

/// <summary>
/// Authentication service interface for user authentication and authorization
/// </summary>
public interface IAuthenticationService
{
    Task<AuthenticationResult> AuthenticateAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<UserInfo?> GetUserFromTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> RevokeTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> RevokeAllUserTokensAsync(string userId, CancellationToken cancellationToken = default);
    Task<List<UserSession>> GetActiveSessionsAsync(string userId, CancellationToken cancellationToken = default);
    Task<bool> TerminateSessionAsync(string sessionId, CancellationToken cancellationToken = default);
    Task<AuthenticationResult> LoginWithMfaAsync(LoginWithMfaRequest request, CancellationToken cancellationToken = default);
    Task<MfaChallenge> InitiateMfaChallengeAsync(string userId, CancellationToken cancellationToken = default);
    Task<MfaValidationResult> ValidateMfaAsync(MfaValidationRequest request, CancellationToken cancellationToken = default);
}

/// <summary>
/// Password hasher interface for secure password hashing (moved from root)
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    bool NeedsRehash(string hashedPassword);
}

// =============================================================================
// MISSING SECURITY INTERFACES
// =============================================================================

/// <summary>
/// Metrics collector interface
/// </summary>
public interface IMetricsCollector
{
    Task RecordMetricAsync(string metricName, double value, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);
    Task IncrementCounterAsync(string counterName, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);
    Task RecordTimingAsync(string timerName, TimeSpan duration, Dictionary<string, string>? tags = null, CancellationToken cancellationToken = default);
}

/// <summary>
/// Audit service interface
/// </summary>
public interface IAuditService
{
    Task LogActionAsync(string userId, string action, string entityType, string entityId, Dictionary<string, object>? metadata = null);
    Task LogErrorAsync(string userId, string error, string? stackTrace = null, Dictionary<string, object>? metadata = null);
    Task<IEnumerable<object>> GetAuditLogsAsync(string? userId = null, DateTime? fromDate = null, DateTime? toDate = null);
}

/// <summary>
/// Email service interface
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
    Task SendEmailAsync(List<string> to, string subject, string body, bool isHtml = false);
    Task SendTemplateEmailAsync(string to, string templateId, Dictionary<string, object> templateData);
}

/// <summary>
/// SMS service interface
/// </summary>
public interface ISmsService
{
    Task SendSmsAsync(string phoneNumber, string message);
    Task SendBulkSmsAsync(List<string> phoneNumbers, string message);
    Task<bool> ValidatePhoneNumberAsync(string phoneNumber);
}
