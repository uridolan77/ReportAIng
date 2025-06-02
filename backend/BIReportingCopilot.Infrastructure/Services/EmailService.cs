using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Email service implementation
/// </summary>
public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Send email asynchronously (interface implementation)
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendAsync(string to, string subject, string body)
    {
        return await SendEmailAsync(to, subject, body);
    }

    /// <summary>
    /// Send email to multiple recipients (interface implementation)
    /// </summary>
    /// <param name="to">Recipient email addresses</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendAsync(string[] to, string subject, string body)
    {
        var successCount = await SendBulkEmailAsync(to, subject, body);
        return successCount == to.Length;
    }

    /// <summary>
    /// Send MFA code via email (interface implementation)
    /// </summary>
    /// <param name="toEmail">Recipient email address</param>
    /// <param name="mfaCode">MFA code</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendMfaCodeAsync(string toEmail, string mfaCode)
    {
        var subject = "Your Verification Code";
        var body = $"Your verification code is: {mfaCode}. This code will expire in 10 minutes.";
        return await SendEmailAsync(toEmail, subject, body);
    }

    /// <summary>
    /// Send email asynchronously
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="isHtml">Whether body is HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = false, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email to {To} with subject: {Subject}", to, subject);

            // Simulate email sending
            await Task.Delay(100, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
            return false;
        }
    }

    /// <summary>
    /// Send email with attachments
    /// </summary>
    /// <param name="to">Recipient email address</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="attachments">Email attachments</param>
    /// <param name="isHtml">Whether body is HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendEmailWithAttachmentsAsync(
        string to,
        string subject,
        string body,
        IEnumerable<EmailAttachment> attachments,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email with attachments to {To} with subject: {Subject}", to, subject);

            // Simulate email sending with attachments
            await Task.Delay(200, cancellationToken);

            _logger.LogInformation("Email with attachments sent successfully to {To}", to);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email with attachments to {To}", to);
            return false;
        }
    }

    /// <summary>
    /// Send bulk emails
    /// </summary>
    /// <param name="recipients">List of recipients</param>
    /// <param name="subject">Email subject</param>
    /// <param name="body">Email body</param>
    /// <param name="isHtml">Whether body is HTML</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of emails sent successfully</returns>
    public async Task<int> SendBulkEmailAsync(
        IEnumerable<string> recipients,
        string subject,
        string body,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        var successCount = 0;
        var recipientList = recipients.ToList();

        _logger.LogInformation("Sending bulk email to {Count} recipients", recipientList.Count);

        foreach (var recipient in recipientList)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (await SendEmailAsync(recipient, subject, body, isHtml, cancellationToken))
            {
                successCount++;
            }

            // Small delay to prevent overwhelming the email service
            await Task.Delay(50, cancellationToken);
        }

        _logger.LogInformation("Bulk email completed: {Success}/{Total} emails sent", successCount, recipientList.Count);
        return successCount;
    }
}

/// <summary>
/// Email attachment model
/// </summary>
public class EmailAttachment
{
    public string FileName { get; set; } = string.Empty;
    public byte[] Content { get; set; } = Array.Empty<byte>();
    public string ContentType { get; set; } = "application/octet-stream";
}
