using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Infrastructure.Services;

namespace BIReportingCopilot.Tests.Unit.MFA;

public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly Mock<IOptions<EmailSettings>> _mockEmailSettings;
    private readonly EmailService _emailService;

    public EmailServiceTests()
    {
        _mockLogger = new Mock<ILogger<EmailService>>();
        _mockEmailSettings = new Mock<IOptions<EmailSettings>>();

        var emailSettings = new EmailSettings
        {
            SmtpServer = "smtp.test.com",
            SmtpPort = 587,
            SmtpUsername = "test@example.com",
            SmtpPassword = "password",
            FromEmail = "noreply@example.com",
            FromName = "BI Reporting Copilot",
            EnableSsl = true
        };
        _mockEmailSettings.Setup(x => x.Value).Returns(emailSettings);

        _emailService = new EmailService(_mockLogger.Object, _mockEmailSettings.Object);
    }

    [Fact]
    public async Task SendMfaCodeAsync_WhenValidParameters_ShouldReturnTrue()
    {
        // Arrange
        var toEmail = "user@example.com";
        var mfaCode = "123456";

        // Act
        var result = await _emailService.SendMfaCodeAsync(toEmail, mfaCode);

        // Assert
        // Since we're using a stub implementation, it should always return true
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendMfaCodeAsync_WhenNullEmail_ShouldReturnFalse()
    {
        // Arrange
        string toEmail = null;
        var mfaCode = "123456";

        // Act
        var result = await _emailService.SendMfaCodeAsync(toEmail, mfaCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMfaCodeAsync_WhenEmptyEmail_ShouldReturnFalse()
    {
        // Arrange
        var toEmail = "";
        var mfaCode = "123456";

        // Act
        var result = await _emailService.SendMfaCodeAsync(toEmail, mfaCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMfaCodeAsync_WhenNullCode_ShouldReturnFalse()
    {
        // Arrange
        var toEmail = "user@example.com";
        string mfaCode = null;

        // Act
        var result = await _emailService.SendMfaCodeAsync(toEmail, mfaCode);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendMfaCodeAsync_WhenEmptyCode_ShouldReturnFalse()
    {
        // Arrange
        var toEmail = "user@example.com";
        var mfaCode = "";

        // Act
        var result = await _emailService.SendMfaCodeAsync(toEmail, mfaCode);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    [InlineData("user.example.com")]
    public async Task SendMfaCodeAsync_WhenInvalidEmailFormat_ShouldReturnFalse(string invalidEmail)
    {
        // Arrange
        var mfaCode = "123456";

        // Act
        var result = await _emailService.SendMfaCodeAsync(invalidEmail, mfaCode);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("test.user+tag@domain.co.uk")]
    [InlineData("simple@example.org")]
    public async Task SendMfaCodeAsync_WhenValidEmailFormat_ShouldReturnTrue(string validEmail)
    {
        // Arrange
        var mfaCode = "123456";

        // Act
        var result = await _emailService.SendMfaCodeAsync(validEmail, mfaCode);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("123456")]
    [InlineData("000000")]
    [InlineData("999999")]
    [InlineData("ABC123")]
    public async Task SendMfaCodeAsync_WhenValidCode_ShouldReturnTrue(string validCode)
    {
        // Arrange
        var toEmail = "user@example.com";

        // Act
        var result = await _emailService.SendMfaCodeAsync(toEmail, validCode);

        // Assert
        result.Should().BeTrue();
    }
}
