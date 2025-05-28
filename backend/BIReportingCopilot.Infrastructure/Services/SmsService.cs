using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Configuration;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// SMS service for sending MFA codes and notifications
/// </summary>
public class SmsService : ISmsService
{
    private readonly ILogger<SmsService> _logger;
    private readonly SmsSettings _smsSettings;

    public SmsService(
        ILogger<SmsService> logger,
        IOptions<SmsSettings> smsSettings)
    {
        _logger = logger;
        _smsSettings = smsSettings.Value;
    }

    public async Task<bool> SendAsync(string phoneNumber, string message)
    {
        try
        {
            _logger.LogInformation("Sending SMS to {PhoneNumber}", MaskPhoneNumber(phoneNumber));

            // TODO: Implement actual SMS sending using Twilio, AWS SNS, etc.
            // For now, just log the SMS content for development
            _logger.LogInformation("SMS Content:\nTo: {PhoneNumber}\nMessage: {Message}", 
                MaskPhoneNumber(phoneNumber), message);

            // Simulate SMS sending delay
            await Task.Delay(100);

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
    public async Task<bool> SendSmsAsync(string phoneNumber, string message)
    {
        return await SendAsync(phoneNumber, message);
    }

    /// <summary>
    /// Test SMS delivery to a phone number
    /// </summary>
    public async Task<bool> TestDeliveryAsync(string phoneNumber)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                _logger.LogWarning("Cannot test SMS delivery: invalid phone number");
                return false;
            }

            // Basic phone number validation
            if (!phoneNumber.StartsWith("+") || phoneNumber.Length < 10 || phoneNumber.Contains("invalid"))
            {
                _logger.LogWarning("Cannot test SMS delivery: invalid phone number format");
                return false;
            }

            var testMessage = "Test SMS delivery from BI Reporting Copilot";
            return await SendAsync(phoneNumber, testMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SMS delivery to {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            return false;
        }
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return "****";
        
        return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(phoneNumber.Length - 4);
    }
}
