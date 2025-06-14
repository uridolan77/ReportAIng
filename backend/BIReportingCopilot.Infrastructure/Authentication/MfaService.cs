using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QRCoder;
using System.Security.Cryptography;
using System.Text;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Interfaces.Security;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Interfaces;
using BIReportingCopilot.Infrastructure.Data.Entities;
using OtpNet;

namespace BIReportingCopilot.Infrastructure.Authentication;

/// <summary>
/// Multi-factor authentication service supporting TOTP, SMS, and Email
/// </summary>
public class MfaService : IMfaService
{
    private readonly IUserRepository _userRepository;
    private readonly BIReportingCopilot.Core.Interfaces.Security.IMfaChallengeRepository _mfaChallengeRepository;
    private readonly Core.Interfaces.Security.IEmailService _emailService;
    private readonly Core.Interfaces.Security.ISmsService _smsService;
    private readonly ILogger<MfaService> _logger;
    private readonly SecurityConfiguration _securitySettings;

    public MfaService(
        IUserRepository userRepository,
        BIReportingCopilot.Core.Interfaces.Security.IMfaChallengeRepository mfaChallengeRepository,
        Core.Interfaces.Security.IEmailService emailService,
        Core.Interfaces.Security.ISmsService smsService,
        ILogger<MfaService> logger,
        IOptions<SecurityConfiguration> securitySettings)
    {
        _userRepository = userRepository;
        _mfaChallengeRepository = mfaChallengeRepository;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
        _securitySettings = securitySettings.Value;
    }

    // Status and Setup Methods
    public async Task<MfaStatus?> GetMfaStatusAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            var backupCodesCount = user.BackupCodes?.Length ?? 0;

            return new MfaStatus
            {
                IsEnabled = user.IsMfaEnabled,
                Method = user.MfaMethod,
                BackupCodesCount = backupCodesCount,
                HasBackupCodes = backupCodesCount > 0,
                HasPhoneNumber = !string.IsNullOrEmpty(user.PhoneNumber),
                IsPhoneNumberVerified = user.IsPhoneNumberVerified,
                MaskedPhoneNumber = MaskPhoneNumber(user.PhoneNumber),
                MaskedEmail = MaskEmail(user.Email),
                LastValidationDate = user.LastMfaValidationDate
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting MFA status for user {UserId}", userId);
            return null;
        }
    }

    public async Task<MfaSetupInfo?> SetupMfaAsync(string userId, MfaMethod method)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            var setupInfo = new MfaSetupInfo
            {
                Method = method,
                BackupCodes = await GenerateBackupCodesInternalAsync()
            };

            if (method == MfaMethod.TOTP)
            {
                setupInfo.TotpSecret = await GenerateSecretAsync();
                setupInfo.QrCodeUrl = await GenerateQrCodeAsync(setupInfo.TotpSecret, user.Email, "BIReportingCopilot");
            }

            // Store the secret temporarily (user needs to verify before enabling)
            user.MfaSecret = setupInfo.TotpSecret;
            user.MfaMethod = method;
            // Don't enable MFA yet - wait for verification

            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("MFA setup initiated for user {UserId} with method {Method}", userId, method);
            return setupInfo;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting up MFA for user {UserId}", userId);
            return new MfaSetupInfo
            {
                Method = method,
                Success = false,
                ErrorMessage = "Failed to setup MFA"
            };
        }
    }

    public async Task<bool> VerifyMfaSetupAsync(string userId, string verificationCode)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.MfaSecret))
                return false;

            var isValid = false;
            var method = user.MfaMethod;

            switch (method)
            {
                case MfaMethod.TOTP:
                    isValid = await ValidateTotpAsync(user.MfaSecret, verificationCode);
                    break;
                case MfaMethod.SMS:
                case MfaMethod.Email:
                    // For SMS/Email, verification should come through challenge system
                    var challenge = await _mfaChallengeRepository.GetActiveChallengeAsync(userId, method);
                    if (challenge != null && challenge.Challenge == verificationCode && !challenge.IsUsed && challenge.ExpiresAt > DateTime.UtcNow)
                    {
                        isValid = true;
                        await _mfaChallengeRepository.MarkChallengeAsUsedAsync(challenge.ChallengeId);
                    }
                    break;
            }

            if (isValid)
            {
                user.IsMfaEnabled = true;
                user.LastMfaValidationDate = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);

                _logger.LogInformation("MFA setup verified and enabled for user {UserId}", userId);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying MFA setup for user {UserId}", userId);
            return false;
        }
    }

    public async Task<bool> DisableMfaAsync(string userId, string verificationCode)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled)
                return false;

            // Verify the code before disabling
            var isValid = await ValidateMfaCodeAsync(userId, verificationCode);
            if (!isValid)
                return false;

            user.IsMfaEnabled = false;
            user.MfaSecret = null;
            user.MfaMethod = MfaMethod.None;
            user.BackupCodes = Array.Empty<string>();
            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("MFA disabled for user {UserId}", userId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disabling MFA for user {UserId}", userId);
            return false;
        }
    }

    // Validation and Challenge Methods
    public async Task<bool> ValidateMfaCodeAsync(string userId, string code)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || !user.IsMfaEnabled)
                return false;

            // Check if it's a backup code
            if (user.BackupCodes != null && user.BackupCodes.Length > 0)
            {
                if (user.BackupCodes.Contains(code))
                {
                    // Remove used backup code
                    var remainingCodes = user.BackupCodes.Where(c => c != code).ToArray();
                    user.BackupCodes = remainingCodes;
                    user.LastMfaValidationDate = DateTime.UtcNow;
                    await _userRepository.UpdateUserAsync(user);

                    _logger.LogInformation("Backup code used for user {UserId}", userId);
                    return true;
                }
            }

            // Validate based on MFA method
            var method = user.MfaMethod;
            var isValid = method switch
            {
                MfaMethod.TOTP => await ValidateTotpAsync(user.MfaSecret ?? "", code),
                MfaMethod.SMS or MfaMethod.Email => await ValidateChallengeCodeAsync(userId, method, code),
                _ => false
            };

            if (isValid)
            {
                user.LastMfaValidationDate = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MFA code for user {UserId}", userId);
            return false;
        }
    }    public async Task<bool> ValidateMfaAsync(MfaChallengeRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.ChallengeId) || string.IsNullOrEmpty(request.Code))
                return false;

            var challenge = await _mfaChallengeRepository.GetChallengeAsync(request.ChallengeId);
            if (challenge == null || challenge.IsUsed || challenge.ExpiresAt <= DateTime.UtcNow)
                return false;

            if (challenge.Challenge != request.Code)
                return false;

            await _mfaChallengeRepository.MarkChallengeAsUsedAsync(challenge.ChallengeId);

            var user = await _userRepository.GetByIdAsync(challenge.UserId);
            if (user != null)
            {
                user.LastMfaValidationDate = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MFA challenge {ChallengeId}", request.ChallengeId);
            return false;
        }
    }

    public async Task<MfaChallengeResponse?> SendMfaChallengeAsync(string userId, MfaMethod method)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new MfaChallengeResponse
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            var challengeCode = method switch
            {
                MfaMethod.SMS => await GenerateSmsCodeAsync(),
                MfaMethod.Email => await GenerateEmailCodeAsync(),
                _ => throw new NotSupportedException($"Challenge method {method} not supported")
            };

            var challenge = new MfaChallenge
            {
                ChallengeId = Guid.NewGuid().ToString(),
                UserId = userId,
                Method = method.ToString(),
                Challenge = challengeCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_securitySettings.MfaCodeExpirationMinutes)
            };

            await _mfaChallengeRepository.CreateChallengeAsync(challenge);

            var success = method switch
            {
                MfaMethod.SMS => await _smsService.SendAsync(user.PhoneNumber ?? "", challengeCode),
                MfaMethod.Email => await _emailService.SendMfaCodeAsync(user.Email, challengeCode),
                _ => false
            };

            return new MfaChallengeResponse
            {
                ChallengeId = challenge.ChallengeId,
                Method = method,
                Success = success,
                ExpiresAt = challenge.ExpiresAt,
                MaskedDeliveryAddress = method == MfaMethod.SMS ? MaskPhoneNumber(user.PhoneNumber) : MaskEmail(user.Email),
                ErrorMessage = success ? null : "Failed to send challenge"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending MFA challenge for user {UserId}", userId);
            return new MfaChallengeResponse
            {
                Success = false,
                ErrorMessage = "Failed to send challenge"
            };
        }
    }

    // Backup Code Methods
    public async Task<string[]> GenerateBackupCodesAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return Array.Empty<string>();

            var backupCodesArray = await GenerateBackupCodesInternalAsync();
            user.BackupCodes = backupCodesArray;
            await _userRepository.UpdateUserAsync(user);

            _logger.LogInformation("Generated new backup codes for user {UserId}", userId);
            return backupCodesArray;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating backup codes for user {UserId}", userId);
            return Array.Empty<string>();
        }
    }

    public async Task<int> GetBackupCodesCountAsync(string userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.BackupCodes == null)
                return 0;

            return user.BackupCodes.Length;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting backup codes count for user {UserId}", userId);
            return 0;
        }
    }

    public async Task<int> GetRemainingBackupCodesCountAsync(string userId)
    {
        return await GetBackupCodesCountAsync(userId);
    }

    // Testing/Utility Methods
    public async Task<bool> TestSmsDeliveryAsync(string phoneNumber)
    {
        try
        {
            return await _smsService.TestDeliveryAsync(phoneNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing SMS delivery to {PhoneNumber}", phoneNumber);
            return false;
        }
    }

    // Legacy Methods for Backward Compatibility
    public async Task<string> GenerateSecretAsync()
    {
        return await Task.Run(() =>
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
        });
    }

    public async Task<string> GenerateQrCodeAsync(string secret, string userEmail, string issuer)
    {
        return await Task.Run(() =>
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
        });
    }

    public async Task<bool> ValidateTotpAsync(string secret, string code)
    {
        return await Task.Run(() =>
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
        });
    }

    public async Task<string> GenerateSmsCodeAsync()
    {
        return await Task.Run(() =>
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
        });
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
        return await Task.Run(() =>
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
        });
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
        return await Task.Run(() =>
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
        });
    }

    // Helper Methods
    private Task<string[]> GenerateBackupCodesInternalAsync()
    {
        var codes = new string[10];
        using var rng = RandomNumberGenerator.Create();

        for (int i = 0; i < codes.Length; i++)
        {
            var bytes = new byte[4];
            rng.GetBytes(bytes);
            codes[i] = Convert.ToHexString(bytes);
        }

        return Task.FromResult(codes);
    }

    private async Task<bool> ValidateChallengeCodeAsync(string userId, MfaMethod method, string code)
    {
        var challenge = await _mfaChallengeRepository.GetActiveChallengeAsync(userId, method);
        if (challenge == null || challenge.IsUsed || challenge.ExpiresAt <= DateTime.UtcNow)
            return false;

        if (challenge.Challenge != code)
            return false;

        await _mfaChallengeRepository.MarkChallengeAsUsedAsync(challenge.ChallengeId);
        return true;
    }

    private static string MaskPhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrEmpty(phoneNumber) || phoneNumber.Length < 4)
            return phoneNumber ?? "";

        return phoneNumber.Length > 10
            ? "***-***-" + phoneNumber.Substring(phoneNumber.Length - 4)
            : "***-" + phoneNumber.Substring(phoneNumber.Length - 4);
    }

    private static string MaskEmail(string email)
    {
        if (string.IsNullOrEmpty(email) || !email.Contains('@'))
            return email;

        var parts = email.Split('@');
        var username = parts[0];
        var domain = parts[1];

        var maskedUsername = username.Length <= 1
            ? username
            : username.Substring(0, 1) + "***";

        return $"{maskedUsername}@{domain}";
    }

    // Interface implementations
    public async Task<MfaChallengeResult> CreateChallengeAsync(string userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new MfaChallengeResult
                {
                    Success = false,
                    ErrorMessage = "User not found"
                };
            }

            // Determine the best MFA method for the user
            var method = !string.IsNullOrEmpty(user.PhoneNumber) ? MfaMethod.SMS : MfaMethod.Email;

            var challengeCode = method switch
            {
                MfaMethod.SMS => await GenerateSmsCodeAsync(),
                MfaMethod.Email => await GenerateEmailCodeAsync(),
                _ => throw new NotSupportedException($"Challenge method {method} not supported")
            };

            var challenge = new MfaChallenge
            {
                ChallengeId = Guid.NewGuid().ToString(),
                UserId = userId,
                Method = method.ToString(),
                Challenge = challengeCode,
                ExpiresAt = DateTime.UtcNow.AddMinutes(_securitySettings.MfaCodeExpirationMinutes)
            };

            await _mfaChallengeRepository.CreateChallengeAsync(challenge, cancellationToken);

            var success = method switch
            {
                MfaMethod.SMS => await SendSmsAsync(user.PhoneNumber ?? "", challengeCode),
                MfaMethod.Email => await SendEmailCodeAsync(user.Email, challengeCode),
                _ => false
            };

            return new MfaChallengeResult
            {
                ChallengeId = challenge.ChallengeId,
                Method = method.ToString(),
                Success = success,
                ExpiresAt = challenge.ExpiresAt,
                MaskedDeliveryAddress = method == MfaMethod.SMS ? MaskPhoneNumber(user.PhoneNumber) : MaskEmail(user.Email),
                ErrorMessage = success ? null : "Failed to send challenge"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MFA challenge for user {UserId}", userId);
            return new MfaChallengeResult
            {
                Success = false,
                ErrorMessage = "Failed to create MFA challenge"
            };
        }
    }

    public async Task<bool> ValidateChallengeAsync(string challengeId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrEmpty(challengeId) || string.IsNullOrEmpty(code))
                return false;

            var challenge = await _mfaChallengeRepository.GetChallengeAsync(challengeId, cancellationToken);
            if (challenge == null || challenge.IsUsed || challenge.ExpiresAt <= DateTime.UtcNow)
                return false;

            if (challenge.Challenge != code)
                return false;

            await _mfaChallengeRepository.MarkChallengeAsUsedAsync(challenge.ChallengeId);

            var user = await _userRepository.GetByIdAsync(challenge.UserId);
            if (user != null)
            {
                user.LastMfaValidationDate = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating MFA challenge {ChallengeId}", challengeId);
            return false;
        }
    }

    /// <summary>
    /// Validate TOTP code
    /// </summary>
    public async Task<bool> ValidateTotpAsync(string userId, string code, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || string.IsNullOrEmpty(user.MfaSecret))
                return false;

            // Basic TOTP validation (in a real implementation, use a proper TOTP library)
            var isValid = await ValidateTotpAsync(user.MfaSecret, code);

            if (isValid && user != null)
            {
                user.LastMfaValidationDate = DateTime.UtcNow;
                await _userRepository.UpdateUserAsync(user);
            }

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating TOTP for user {UserId}", userId);
            return false;
        }
    }

    // Additional interface implementations
    public async Task<string[]> GenerateBackupCodesAsync()
    {
        return await GenerateBackupCodesInternalAsync();
    }
}
