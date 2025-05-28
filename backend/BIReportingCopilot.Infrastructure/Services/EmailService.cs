using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Email service for sending notifications and MFA codes
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;
    private readonly EmailSettings _emailSettings;

    public EmailService(
        ILogger<EmailService> logger,
        IOptions<EmailSettings> emailSettings)
    {
        _logger = logger;
        _emailSettings = emailSettings.Value;
    }

    public async Task<bool> SendAsync(string to, string subject, string body)
    {
        return await SendAsync(new[] { to }, subject, body);
    }

    public async Task<bool> SendAsync(string[] to, string subject, string body)
    {
        try
        {
            _logger.LogInformation("Sending email to {Recipients} with subject: {Subject}", 
                string.Join(", ", to.Select(MaskEmail)), subject);

            // TODO: Implement actual email sending using SMTP, SendGrid, etc.
            // For now, just log the email content for development
            _logger.LogInformation("Email Content:\nTo: {To}\nSubject: {Subject}\nBody: {Body}", 
                string.Join(", ", to), subject, body);

            // Simulate email sending delay
            await Task.Delay(100);

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
            if (!toEmail.Contains("@") || !toEmail.Contains("."))
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

            return await SendAsync(toEmail, subject, body);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending MFA code to {Email}", MaskEmail(toEmail));
            return false;
        }
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
}
