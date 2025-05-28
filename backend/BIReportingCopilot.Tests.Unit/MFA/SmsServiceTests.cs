using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BIReportingCopilot.Core.Configuration;
using BIReportingCopilot.Infrastructure.Services;

namespace BIReportingCopilot.Tests.Unit.MFA;

public class SmsServiceTests
{
    private readonly Mock<ILogger<SmsService>> _mockLogger;
    private readonly Mock<IOptions<SmsSettings>> _mockSmsSettings;
    private readonly SmsService _smsService;

    public SmsServiceTests()
    {
        _mockLogger = new Mock<ILogger<SmsService>>();
        _mockSmsSettings = new Mock<IOptions<SmsSettings>>();

        var smsSettings = new SmsSettings
        {
            TwilioAccountSid = "test_account_sid",
            TwilioAuthToken = "test_auth_token",
            TwilioPhoneNumber = "+1234567890",
            MaxRetryAttempts = 3,
            RetryDelaySeconds = 5
        };
        _mockSmsSettings.Setup(x => x.Value).Returns(smsSettings);

        _smsService = new SmsService(_mockLogger.Object, _mockSmsSettings.Object);
    }

    [Fact]
    public async Task SendSmsAsync_WhenValidParameters_ShouldReturnTrue()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var message = "Your MFA code is: 123456";

        // Act
        var result = await _smsService.SendSmsAsync(phoneNumber, message);

        // Assert
        // Since we're using a stub implementation, it should always return true
        result.Should().BeTrue();
    }

    [Fact]
    public async Task SendSmsAsync_WhenNullPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        string phoneNumber = null;
        var message = "Your MFA code is: 123456";

        // Act
        var result = await _smsService.SendSmsAsync(phoneNumber, message);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_WhenEmptyPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = "";
        var message = "Your MFA code is: 123456";

        // Act
        var result = await _smsService.SendSmsAsync(phoneNumber, message);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_WhenNullMessage_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        string message = null;

        // Act
        var result = await _smsService.SendSmsAsync(phoneNumber, message);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task SendSmsAsync_WhenEmptyMessage_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = "+1234567890";
        var message = "";

        // Act
        var result = await _smsService.SendSmsAsync(phoneNumber, message);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("1234567890")] // Missing country code
    [InlineData("12345")] // Too short
    [InlineData("+1234567890123456789")] // Too long
    [InlineData("invalid-phone")]
    [InlineData("+1-invalid-phone")]
    public async Task SendSmsAsync_WhenInvalidPhoneFormat_ShouldReturnFalse(string invalidPhone)
    {
        // Arrange
        var message = "Your MFA code is: 123456";

        // Act
        var result = await _smsService.SendSmsAsync(invalidPhone, message);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+447123456789")]
    [InlineData("+33123456789")]
    [InlineData("+491234567890")]
    public async Task SendSmsAsync_WhenValidPhoneFormat_ShouldReturnTrue(string validPhone)
    {
        // Arrange
        var message = "Your MFA code is: 123456";

        // Act
        var result = await _smsService.SendSmsAsync(validPhone, message);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [InlineData("Your MFA code is: 123456")]
    [InlineData("Code: 000000")]
    [InlineData("Security code: ABC123")]
    public async Task SendSmsAsync_WhenValidMessage_ShouldReturnTrue(string validMessage)
    {
        // Arrange
        var phoneNumber = "+1234567890";

        // Act
        var result = await _smsService.SendSmsAsync(phoneNumber, validMessage);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestDeliveryAsync_WhenValidPhoneNumber_ShouldReturnTrue()
    {
        // Arrange
        var phoneNumber = "+1234567890";

        // Act
        var result = await _smsService.TestDeliveryAsync(phoneNumber);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task TestDeliveryAsync_WhenInvalidPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        var phoneNumber = "invalid-phone";

        // Act
        var result = await _smsService.TestDeliveryAsync(phoneNumber);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task TestDeliveryAsync_WhenNullPhoneNumber_ShouldReturnFalse()
    {
        // Arrange
        string phoneNumber = null;

        // Act
        var result = await _smsService.TestDeliveryAsync(phoneNumber);

        // Assert
        result.Should().BeFalse();
    }
}
