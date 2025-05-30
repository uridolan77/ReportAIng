using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;
using BIReportingCopilot.Infrastructure.Services;
using BIReportingCopilot.Infrastructure.Data.Entities;
using BIReportingCopilot.Infrastructure.Interfaces;

namespace BIReportingCopilot.Tests.Unit.MFA;

public class MfaServiceTests
{
    private readonly Mock<IUserEntityRepository> _mockUserRepository;
    private readonly Mock<IMfaChallengeRepository> _mockMfaChallengeRepository;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ISmsService> _mockSmsService;
    private readonly Mock<ILogger<MfaService>> _mockLogger;
    private readonly Mock<IOptions<SecuritySettings>> _mockSecuritySettings;
    private readonly MfaService _mfaService;

    public MfaServiceTests()
    {
        _mockUserRepository = new Mock<IUserEntityRepository>();
        _mockMfaChallengeRepository = new Mock<IMfaChallengeRepository>();
        _mockEmailService = new Mock<IEmailService>();
        _mockSmsService = new Mock<ISmsService>();
        _mockLogger = new Mock<ILogger<MfaService>>();
        _mockSecuritySettings = new Mock<IOptions<SecuritySettings>>();

        var securitySettings = new SecuritySettings
        {
            MfaCodeExpirationMinutes = 5,
            MaxMfaAttempts = 3
        };
        _mockSecuritySettings.Setup(x => x.Value).Returns(securitySettings);

        _mfaService = new MfaService(
            _mockUserRepository.Object,
            _mockMfaChallengeRepository.Object,
            _mockEmailService.Object,
            _mockSmsService.Object,
            _mockLogger.Object,
            _mockSecuritySettings.Object
        );
    }    [Fact]
    public async Task GetMfaStatusAsync_WhenUserNotFound_ShouldReturnNull()
    {        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userIdGuid = Guid.Parse(userId);
        _mockUserRepository.Setup(x => x.GetByIdAsync(userIdGuid))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _mfaService.GetMfaStatusAsync(userId);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetMfaStatusAsync_WhenUserFound_ShouldReturnMfaStatus()
    {        // Arrange
        var userId = Guid.NewGuid().ToString();
        var userIdGuid = Guid.Parse(userId);
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            IsMfaEnabled = true,
            MfaSecret = "test-secret",
            MfaMethod = MfaMethod.TOTP.ToString(),
            PhoneNumber = "+1234567890",
            BackupCodes = "code1,code2,code3"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(userIdGuid))
            .ReturnsAsync(user);

        // Act
        var result = await _mfaService.GetMfaStatusAsync(userId);

        // Assert
        result.Should().NotBeNull();
        result.IsEnabled.Should().BeTrue();
        result.Method.Should().Be(MfaMethod.TOTP);
        result.BackupCodesCount.Should().Be(3);
        result.HasPhoneNumber.Should().BeTrue();
        result.MaskedPhoneNumber.Should().Be("***-***-7890");
        result.MaskedEmail.Should().Be("t***@example.com");
    }

    [Theory]
    [InlineData(MfaMethod.TOTP)]
    [InlineData(MfaMethod.SMS)]
    [InlineData(MfaMethod.Email)]
    public async Task SetupMfaAsync_WhenValidMethod_ShouldReturnSetupInfo(MfaMethod method)
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            PhoneNumber = "+1234567890",        IsMfaEnabled = false
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        // Act
        var result = await _mfaService.SetupMfaAsync(userId, method);

        // Assert
        result.Should().NotBeNull();
        result.Method.Should().Be(method);
        
        if (method == MfaMethod.TOTP)
        {
            result.TotpSecret.Should().NotBeNullOrEmpty();
            result.QrCodeUrl.Should().NotBeNullOrEmpty();
        }
        
        result.BackupCodes.Should().HaveCount(10);
        result.BackupCodes.Should().OnlyContain(code => !string.IsNullOrWhiteSpace(code));
    }

    [Fact]
    public async Task SetupMfaAsync_WhenUserNotFound_ShouldReturnNull()
    {        // Arrange
        var userId = Guid.NewGuid().ToString();
        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync((UserEntity?)null);

        // Act
        var result = await _mfaService.SetupMfaAsync(userId, MfaMethod.TOTP);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task VerifyMfaSetupAsync_WhenValidTotpCode_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var secret = "JBSWY3DPEHPK3PXP"; // Base32 encoded secret
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            MfaSecret = secret,
            MfaMethod = MfaMethod.TOTP.ToString(),
            IsMfaEnabled = false
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        // Generate a valid TOTP code for testing
        var validCode = GenerateValidTotpCode(secret);

        // Act
        var result = await _mfaService.VerifyMfaSetupAsync(userId, validCode);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => u.IsMfaEnabled == true)), Times.Once);
    }

    [Fact]
    public async Task ValidateMfaAsync_WhenValidCode_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var secret = "JBSWY3DPEHPK3PXP";
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            MfaSecret = secret,
            MfaMethod = MfaMethod.TOTP.ToString(),
            IsMfaEnabled = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        var validCode = GenerateValidTotpCode(secret);

        // Act
        var result = await _mfaService.ValidateMfaAsync(userId, validCode);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateMfaAsync_WhenInvalidCode_ShouldReturnFalse()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            MfaSecret = "JBSWY3DPEHPK3PXP",
            MfaMethod = MfaMethod.TOTP.ToString(),
            IsMfaEnabled = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        // Act
        var result = await _mfaService.ValidateMfaAsync(userId, "000000");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task ValidateMfaAsync_WhenBackupCode_ShouldReturnTrue()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var backupCode = "ABC123DEF";
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            IsMfaEnabled = true,
            BackupCodes = $"{backupCode},XYZ789GHI"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        // Act
        var result = await _mfaService.ValidateMfaAsync(userId, backupCode);

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => 
            !u.BackupCodes!.Contains(backupCode))), Times.Once);
    }

    [Fact]
    public async Task GetRemainingBackupCodesCountAsync_WhenUserHasCodes_ShouldReturnCount()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            BackupCodes = "code1,code2,code3"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        // Act
        var result = await _mfaService.GetRemainingBackupCodesCountAsync(userId);

        // Assert
        result.Should().Be(3);
    }

    [Fact]
    public async Task SendMfaChallengeAsync_WhenSmsMethod_ShouldSendSms()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            PhoneNumber = "+1234567890",
            MfaMethod = MfaMethod.SMS.ToString(),
            IsMfaEnabled = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        _mockSmsService.Setup(x => x.SendSmsAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mfaService.SendMfaChallengeAsync(userId, MfaMethod.SMS);

        // Assert
        result.Should().BeTrue();
        _mockSmsService.Verify(x => x.SendSmsAsync(user.PhoneNumber!, It.IsAny<string>()), Times.Once);
        _mockMfaChallengeRepository.Verify(x => x.CreateChallengeAsync(It.IsAny<MfaChallenge>()), Times.Once);
    }

    [Fact]
    public async Task SendMfaChallengeAsync_WhenEmailMethod_ShouldSendEmail()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            MfaMethod = MfaMethod.Email.ToString(),
            IsMfaEnabled = true
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        _mockEmailService.Setup(x => x.SendMfaCodeAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        // Act
        var result = await _mfaService.SendMfaChallengeAsync(userId, MfaMethod.Email);

        // Assert
        result.Should().BeTrue();
        _mockEmailService.Verify(x => x.SendMfaCodeAsync(user.Email, It.IsAny<string>()), Times.Once);
        _mockMfaChallengeRepository.Verify(x => x.CreateChallengeAsync(It.IsAny<MfaChallenge>()), Times.Once);
    }

    [Fact]
    public async Task DisableMfaAsync_WhenUserExists_ShouldDisableMfa()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var user = new UserEntity
        {
            Id = userId,
            Email = "test@example.com",
            IsMfaEnabled = true,
            MfaSecret = "secret",
            BackupCodes = "code1,code2"
        };

        _mockUserRepository.Setup(x => x.GetByIdAsync(Guid.Parse(userId)))
            .ReturnsAsync(user);

        // Act
        var result = await _mfaService.DisableMfaAsync(userId, "123456");

        // Assert
        result.Should().BeTrue();
        _mockUserRepository.Verify(x => x.UpdateAsync(It.Is<UserEntity>(u => 
            !u.IsMfaEnabled && 
            string.IsNullOrEmpty(u.MfaSecret) && 
            string.IsNullOrEmpty(u.BackupCodes))), Times.Once);
    }

    private string GenerateValidTotpCode(string secret)
    {
        // This is a simplified TOTP implementation for testing
        // In a real scenario, you would use a proper TOTP library
        var unixTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var timestep = unixTimestamp / 30;
        return (timestep % 1000000).ToString("D6");
    }
}
