using Microsoft.Extensions.Logging;
using BIReportingCopilot.Core.Interfaces;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// SMS service implementation
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;

    public SmsService(ILogger<SmsService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Send SMS asynchronously (interface implementation)
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="message">SMS message</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        return await SendSmsAsync(phoneNumber, message);
    }

    /// <summary>
    /// Test SMS delivery (interface implementation)
    /// </summary>
    /// <param name="phoneNumber">Phone number to test</param>
    /// <returns>True if delivery test successful</returns>
    public async Task<bool> TestDeliveryAsync(string phoneNumber)
    {
        return await SendSmsAsync(phoneNumber, "Test message - please ignore");
    }

    /// <summary>
    /// Send SMS asynchronously (interface implementation)
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="message">SMS message</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        return await SendSmsAsync(phoneNumber, message, CancellationToken.None);
    }

    /// <summary>
    /// Send SMS asynchronously
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="message">SMS message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendSmsAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}", phoneNumber);

            // Simulate SMS sending
            await Task.Delay(100, cancellationToken);

            _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SMS to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    /// <summary>
    /// Send bulk SMS messages
    /// </summary>
    /// <param name="phoneNumbers">List of phone numbers</param>
    /// <param name="message">SMS message</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Number of SMS messages sent successfully</returns>
    public async Task<int> SendBulkSmsAsync(
        IEnumerable<string> phoneNumbers,
        string message,
        CancellationToken cancellationToken = default)
    {
        var successCount = 0;
        var phoneNumberList = phoneNumbers.ToList();

        _logger.LogInformation("Sending bulk SMS to {Count} recipients", phoneNumberList.Count);

        foreach (var phoneNumber in phoneNumberList)
        {
            if (cancellationToken.IsCancellationRequested)
                break;

            if (await SendSmsAsync(phoneNumber, message, cancellationToken))
            {
                successCount++;
            }

            // Small delay to prevent overwhelming the SMS service
            await Task.Delay(50, cancellationToken);
        }

        _logger.LogInformation("Bulk SMS completed: {Success}/{Total} messages sent", successCount, phoneNumberList.Count);
        return successCount;
    }

    /// <summary>
    /// Send verification code via SMS
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="code">Verification code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendVerificationCodeAsync(string phoneNumber, string code, CancellationToken cancellationToken = default)
    {
        var message = $"Your verification code is: {code}. This code will expire in 10 minutes.";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }

    /// <summary>
    /// Send alert message via SMS
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number</param>
    /// <param name="alertMessage">Alert message</param>
    /// <param name="priority">Message priority</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if sent successfully</returns>
    public async Task<bool> SendAlertAsync(
        string phoneNumber,
        string alertMessage,
        SmsPriority priority = SmsPriority.Normal,
        CancellationToken cancellationToken = default)
    {
        var priorityPrefix = priority switch
        {
            SmsPriority.High => "[URGENT] ",
            SmsPriority.Critical => "[CRITICAL] ",
            _ => ""
        };

        var message = $"{priorityPrefix}{alertMessage}";
        return await SendSmsAsync(phoneNumber, message, cancellationToken);
    }
}

/// <summary>
/// SMS priority levels
/// </summary>
public enum SmsPriority
{
    Low,
    Normal,
    High,
    Critical
}
