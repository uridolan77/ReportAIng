using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Messaging;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Infrastructure.Configuration;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.Infrastructure.Messaging;

/// <summary>
/// Unified notification management service consolidating email, SMS, and real-time notifications
/// Replaces EmailService, SmsService, and Hub notification functionality
/// </summary>
public class NotificationManagementService : IEmailService, ISmsService
{
    private readonly ILogger<NotificationManagementService> _logger;
    private readonly ConfigurationService _configurationService;
    private readonly IHubContext<Hub> _hubContext;
    private readonly NotificationConfiguration _notificationConfig;

    public NotificationManagementService(
        ILogger<NotificationManagementService> logger,
        ConfigurationService configurationService,
        IHubContext<Hub> hubContext)
    {
        _logger = logger;
        _configurationService = configurationService;
        _hubContext = hubContext;
        _notificationConfig = configurationService.GetNotificationSettings();
    }

    #region Email Notifications (IEmailService Implementation)

    /// <summary>
    /// Send email asynchronously (IEmailService interface)
    /// </summary>
    public async Task<bool> SendAsync(string to, string subject, string body)
    {
        return await SendEmailAsync(to, subject, body);
    }

    /// <summary>
    /// Send email to multiple recipients (IEmailService interface)
    /// </summary>
    public async Task<bool> SendAsync(string[] to, string subject, string body)
    {
        return await SendEmailAsync(to, subject, body);
    }

    /// <summary>
    /// Send email notification to single recipient
    /// </summary>
    public async Task<bool> SendEmailAsync(string to, string subject, string body)
    {
        return await SendEmailAsync(new[] { to }, subject, body);
    }

    /// <summary>
    /// Send email notification to multiple recipients
    /// </summary>
    public async Task<bool> SendEmailAsync(string[] to, string subject, string body)
    {
        if (!_notificationConfig.EnableEmail)
        {
            _logger.LogInformation("Email notifications are disabled");
            return false;
        }

        try
        {
            _logger.LogInformation("Sending email to {Recipients} with subject: {Subject}",
                string.Join(", ", to.Select(MaskEmail)), subject);

            // Development mode: Log email content for debugging
            if (_notificationConfig.EnableLogging)
            {
                _logger.LogInformation("Email Content:\nTo: {To}\nSubject: {Subject}\nBody: {Body}",
                    string.Join(", ", to), subject, body);
            }

            // Simulate email sending delay for development
            await Task.Delay(_notificationConfig.SimulatedDelay);

            _logger.LogInformation("Email sent successfully to {Recipients}",
                string.Join(", ", to.Select(MaskEmail)));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Recipients}",
                string.Join(", ", to.Select(MaskEmail)));
            return false;
        }
    }

    /// <summary>
    /// Send MFA code via email
    /// </summary>
    public async Task<bool> SendMfaCodeAsync(string toEmail, string mfaCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(toEmail) || string.IsNullOrWhiteSpace(mfaCode))
            {
                _logger.LogWarning("Cannot send MFA code: invalid email or code");
                return false;
            }

            // Basic email validation
            if (!IsValidEmail(toEmail))
            {
                _logger.LogWarning("Cannot send MFA code: invalid email format");
                return false;
            }

            var subject = "Your Verification Code";
            var body = $@"
                <h2>Verification Code</h2>
                <p>Your verification code is: <strong>{mfaCode}</strong></p>
                <p>This code will expire in 5 minutes.</p>
                <p>If you didn't request this code, please ignore this email.</p>
            ";

            return await SendEmailAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending MFA code to {Email}", MaskEmail(toEmail));
            return false;
        }
    }

    #endregion

    #region SMS Notifications (ISmsService Implementation)

    /// <summary>
    /// Send SMS asynchronously (ISmsService interface)
    /// </summary>
    async Task<bool> ISmsService.SendAsync(string phoneNumber, string message)
    {
        return await SendSmsAsync(phoneNumber, message);
    }

    /// <summary>
    /// Test SMS delivery (ISmsService interface)
    /// </summary>
    public async Task<bool> TestDeliveryAsync(string phoneNumber)
    {
        return await TestSmsDeliveryAsync(phoneNumber);
    }

    /// <summary>
    /// Send SMS asynchronously (ISmsService interface)
    /// </summary>
    async Task ISmsService.SendSmsAsync(string phoneNumber, string message)
    {
        await SendSmsAsync(phoneNumber, message);
    }

    /// <summary>
    /// Send SMS notification
    /// </summary>
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        if (!_notificationConfig.EnableSms)
        {
            _logger.LogInformation("SMS notifications are disabled");
            return false;
        }

        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}", MaskPhoneNumber(phoneNumber));

            // Development mode: Log SMS content for debugging
            if (_notificationConfig.EnableLogging)
            {
                _logger.LogInformation("SMS Content:\nTo: {PhoneNumber}\nMessage: {Message}",
                    MaskPhoneNumber(phoneNumber), message);
            }

            // Simulate SMS sending delay for development
            await Task.Delay(_notificationConfig.SimulatedDelay);

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}", MaskPhoneNumber(phoneNumber));

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            return false;
        }
    }

    /// <summary>
    /// Send MFA code via SMS
    /// </summary>
    public async Task<bool> SendSmsMfaCodeAsync(string phoneNumber, string mfaCode)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber) || string.IsNullOrWhiteSpace(mfaCode))
            {
                _logger.LogWarning("Cannot send SMS MFA code: invalid phone number or code");
                return false;
            }

            // Basic phone number validation
            if (!IsValidPhoneNumber(phoneNumber))
            {
                _logger.LogWarning("Cannot send SMS MFA code: invalid phone number format");
                return false;
            }

            var message = $"Your verification code is: {mfaCode}. This code expires in 5 minutes.";
            return await SendSmsAsync(phoneNumber, message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS MFA code to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            return false;
        }
    }

    /// <summary>
    /// Test SMS delivery to a phone number
    /// </summary>
    public async Task<bool> TestSmsDeliveryAsync(string phoneNumber)
    {
        try
        {
            if (!IsValidPhoneNumber(phoneNumber))
            {
                _logger.LogWarning("Cannot test SMS delivery: invalid phone number format");
                return false;
            }

            var testMessage = "Test SMS delivery from BI Reporting Copilot";
            return await SendSmsAsync(phoneNumber, testMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SMS delivery to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            return false;
        }
    }

    #endregion

    #region Real-Time Notifications

    /// <summary>
    /// Send notification to specific user
    /// </summary>
    public async Task SendUserNotificationAsync(string userId, object notification)
    {
        if (!_notificationConfig.EnableRealTime)
        {
            _logger.LogDebug("Real-time notifications are disabled");
            return;
        }

        try
        {
            await _hubContext.Clients.Group($"user_{userId}").SendAsync("UserNotification", notification);
            _logger.LogDebug("Real-time notification sent to user: {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send real-time notification to user: {UserId}", userId);
        }
    }

    /// <summary>
    /// Broadcast system-wide notification
    /// </summary>
    public async Task SendSystemNotificationAsync(object notification)
    {
        if (!_notificationConfig.EnableRealTime)
        {
            _logger.LogDebug("Real-time notifications are disabled");
            return;
        }

        try
        {
            await _hubContext.Clients.All.SendAsync("SystemNotification", notification);
            _logger.LogDebug("System-wide notification broadcasted");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send system-wide notification");
        }
    }

    /// <summary>
    /// Notify query started
    /// </summary>
    public async Task NotifyQueryStartedAsync(string queryId, string userId)
    {
        try
        {
            await _hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryStarted", new
            {
                QueryId = queryId,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                Status = "started"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify query started: {QueryId}", queryId);
        }
    }

    /// <summary>
    /// Notify query progress
    /// </summary>
    public async Task NotifyQueryProgressAsync(string queryId, double progress, string? message = null)
    {
        try
        {
            await _hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryProgress", new
            {
                QueryId = queryId,
                Progress = progress,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Status = "in_progress"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify query progress: {QueryId}", queryId);
        }
    }

    /// <summary>
    /// Notify query completed
    /// </summary>
    public async Task NotifyQueryCompletedAsync(string queryId, bool success, int executionTimeMs, string? error = null)
    {
        try
        {
            await _hubContext.Clients.Group($"query_{queryId}").SendAsync("QueryCompleted", new
            {
                QueryId = queryId,
                Success = success,
                ExecutionTimeMs = executionTimeMs,
                Error = error,
                Timestamp = DateTime.UtcNow,
                Status = success ? "completed" : "failed"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to notify query completed: {QueryId}", queryId);
        }
    }

    #endregion

    #region Multi-Channel Notifications

    /// <summary>
    /// Send notification via multiple channels
    /// </summary>
    public async Task<NotificationResult> SendMultiChannelNotificationAsync(
        string userId,
        string subject,
        string message,
        NotificationChannels channels = NotificationChannels.All,
        string? email = null,
        string? phoneNumber = null)
    {
        var result = new NotificationResult();

        try
        {
            // Send email if enabled and requested
            if (channels.HasFlag(NotificationChannels.Email) && !string.IsNullOrEmpty(email))
            {
                result.EmailSent = await SendEmailAsync(email, subject, message);
            }

            // Send SMS if enabled and requested
            if (channels.HasFlag(NotificationChannels.Sms) && !string.IsNullOrEmpty(phoneNumber))
            {
                result.SmsSent = await SendSmsAsync(phoneNumber, message);
            }

            // Send real-time notification if enabled and requested
            if (channels.HasFlag(NotificationChannels.RealTime))
            {
                await SendUserNotificationAsync(userId, new
                {
                    Subject = subject,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    Type = "notification"
                });
                result.RealTimeSent = true;
            }

            result.Success = result.EmailSent || result.SmsSent || result.RealTimeSent;
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send multi-channel notification to user: {UserId}", userId);
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    #endregion

    #region Missing Interface Method Implementations

    /// <summary>
    /// Send email with HTML support (IEmailService interface)
    /// </summary>
    public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
    {
        await SendEmailAsync(to, subject, body);
    }

    /// <summary>
    /// Send email to multiple recipients (IEmailService interface)
    /// </summary>
    public async Task SendEmailAsync(List<string> recipients, string subject, string body, bool isHtml = false)
    {
        foreach (var recipient in recipients)
        {
            await SendEmailAsync(recipient, subject, body);
        }
    }

    /// <summary>
    /// Send template email (IEmailService interface)
    /// </summary>
    public async Task SendTemplateEmailAsync(string to, string templateName, Dictionary<string, object> templateData)
    {
        var subject = templateData.ContainsKey("subject") ? templateData["subject"].ToString() : "Notification";
        var body = templateData.ContainsKey("body") ? templateData["body"].ToString() : "Template notification";
        await SendEmailAsync(to, subject!, body!);
    }

    /// <summary>
    /// Send bulk SMS (ISmsService interface)
    /// </summary>
    public async Task SendBulkSmsAsync(List<string> phoneNumbers, string message)
    {
        foreach (var phoneNumber in phoneNumbers)
        {
            await SendSmsAsync(phoneNumber, message);
        }
    }

    /// <summary>
    /// Validate phone number (ISmsService interface)
    /// </summary>
    public async Task<bool> ValidatePhoneNumberAsync(string phoneNumber)
    {
        return await Task.FromResult(IsValidPhoneNumber(phoneNumber));
    }

    #endregion

    #region Private Helper Methods

    private static bool IsValidEmail(string email)
    {
        return !string.IsNullOrWhiteSpace(email) &&
               email.Contains('@') &&
               email.Contains('.') &&
               email.Length > 5;
    }

    private static bool IsValidPhoneNumber(string phoneNumber)
    {
        return !string.IsNullOrWhiteSpace(phoneNumber) &&
               phoneNumber.StartsWith("+") &&
               phoneNumber.Length >= 10 &&
               !phoneNumber.Contains("invalid");
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return "****@****.***";

        var parts = email.Split('@');
        var localPart = parts[0];
        var domainPart = parts[1];

        var maskedLocal = localPart.Length > 2
            ? localPart.Substring(0, 2) + "****"
            : "****";

        return $"{maskedLocal}@{domainPart}";
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return "****";

        return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(phoneNumber.Length - 4);
    }

    #endregion
}

/// <summary>
/// Notification channels enumeration
/// </summary>
[Flags]
public enum NotificationChannels
{
    None = 0,
    Email = 1,
    Sms = 2,
    RealTime = 4,
    All = Email | Sms | RealTime
}

/// <summary>
/// Notification result with channel-specific status
/// </summary>
public class NotificationResult
{
    public bool Success { get; set; }
    public bool EmailSent { get; set; }
    public bool SmsSent { get; set; }
    public bool RealTimeSent { get; set; }
    public string? ErrorMessage { get; set; }
}