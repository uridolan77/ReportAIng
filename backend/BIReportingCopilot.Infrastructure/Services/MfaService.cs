using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRCoder;
using System.Security.Cryptography;
using System.Text;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Configuration;
using OtpNet;

namespace BIReportingCopilot.Infrastructure.Services;

/// <summary>
/// Multi-factor authentication service supporting TOTP, SMS, and Email
/// </summary>
public class MfaService : IMfaService
{
    private readonly ILogger<MfaService> _logger;
    private readonly SecuritySettings _securitySettings;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;

    public MfaService(
        ILogger<MfaService> logger,
        IOptions<SecuritySettings> securitySettings,
        IEmailService emailService,
        ISmsService smsService)
    {
        _logger = logger;
        _securitySettings = securitySettings.Value;
        _emailService = emailService;
        _smsService = smsService;
    }

    public async Task<string> GenerateSecretAsync()
    {
        try
        {
            var key = KeyGeneration.GenerateRandomKey(20);
            var secret = Base32Encoding.ToString(key);
            
            _logger.LogInformation("Generated TOTP secret");
            return secret;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating TOTP secret");
            throw;
        }
    }

    public async Task<string> GenerateQrCodeAsync(string secret, string userEmail, string issuer)
    {
        try
        {
            var totpUri = $"otpauth://totp/{Uri.EscapeDataString(issuer)}:{Uri.EscapeDataString(userEmail)}?secret={secret}&issuer={Uri.EscapeDataString(issuer)}";
            
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(totpUri, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new Base64QRCode(qrCodeData);
            
            var qrCodeImageAsBase64 = qrCode.GetGraphic(20);
            
            _logger.LogInformation("Generated QR code for user: {UserEmail}", userEmail);
            return $"data:image/png;base64,{qrCodeImageAsBase64}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating QR code for user: {UserEmail}", userEmail);
            throw;
        }
    }

    public async Task<bool> ValidateTotpAsync(string secret, string code)
    {
        try
        {
            if (string.IsNullOrEmpty(secret) || string.IsNullOrEmpty(code))
            {
                return false;
            }

            var secretBytes = Base32Encoding.ToBytes(secret);
            var totp = new Totp(secretBytes);
            
            // Allow for time drift - check current window and ±1 window
            var currentTime = DateTime.UtcNow;
            var windows = new[]
            {
                currentTime.AddSeconds(-30),
                currentTime,
                currentTime.AddSeconds(30)
            };

            foreach (var window in windows)
            {
                var expectedCode = totp.ComputeTotp(window);
                if (expectedCode == code)
                {
                    _logger.LogInformation("TOTP validation successful");
                    return true;
                }
            }

            _logger.LogWarning("TOTP validation failed - invalid code");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating TOTP code");
            return false;
        }
    }

    public async Task<string> GenerateSmsCodeAsync()
    {
        try
        {
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            
            _logger.LogInformation("Generated SMS code");
            return code;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating SMS code");
            throw;
        }
    }

    public async Task<bool> SendSmsAsync(string phoneNumber, string code)
    {
        try
        {
            var message = $"Your verification code is: {code}. This code will expire in 5 minutes.";
            var success = await _smsService.SendAsync(phoneNumber, message);
            
            if (success)
            {
                _logger.LogInformation("SMS code sent successfully to: {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            }
            else
            {
                _logger.LogWarning("Failed to send SMS code to: {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending SMS to: {PhoneNumber}", MaskPhoneNumber(phoneNumber));
            return false;
        }
    }

    public async Task<string> GenerateEmailCodeAsync()
    {
        try
        {
            var random = new Random();
            var code = random.Next(100000, 999999).ToString();
            
            _logger.LogInformation("Generated email code");
            return code;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating email code");
            throw;
        }
    }

    public async Task<bool> SendEmailCodeAsync(string email, string code)
    {
        try
        {
            var subject = "Verification Code";
            var body = $@"
                <h2>Verification Code</h2>
                <p>Your verification code is: <strong>{code}</strong></p>
                <p>This code will expire in 5 minutes.</p>
                <p>If you didn't request this code, please ignore this email.</p>
            ";
            
            var success = await _emailService.SendAsync(email, subject, body);
            
            if (success)
            {
                _logger.LogInformation("Email code sent successfully to: {Email}", MaskEmail(email));
            }
            else
            {
                _logger.LogWarning("Failed to send email code to: {Email}", MaskEmail(email));
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to: {Email}", MaskEmail(email));
            return false;
        }
    }

    public async Task<string[]> GenerateBackupCodesAsync(int count = 8)
    {
        try
        {
            var codes = new string[count];
            
            for (int i = 0; i < count; i++)
            {
                using var rng = RandomNumberGenerator.Create();
                var bytes = new byte[4];
                rng.GetBytes(bytes);
                
                // Generate 8-character alphanumeric code
                var code = Convert.ToBase64String(bytes)
                    .Replace("+", "")
                    .Replace("/", "")
                    .Replace("=", "")
                    .ToUpper();
                
                if (code.Length >= 8)
                {
                    codes[i] = code.Substring(0, 8);
                }
                else
                {
                    // Fallback: generate random alphanumeric string
                    codes[i] = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
                }
            }
            
            _logger.LogInformation("Generated {Count} backup codes", count);
            return codes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes");
            throw;
        }
    }

    private static string MaskPhoneNumber(string phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return "****";
        
        return phoneNumber.Substring(0, 3) + "****" + phoneNumber.Substring(phoneNumber.Length - 4);
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
