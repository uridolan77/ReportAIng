using Xunit;
using Moq;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using BIReportingCopilot.API.Controllers;
using BIReportingCopilot.Core.Interfaces;
using BIReportingCopilot.Core.Models;

namespace BIReportingCopilot.Tests.Unit.MFA;

public class MfaControllerTests
{
    private readonly Mock<IMfaService> _mockMfaService;
    private readonly Mock<ILogger<MfaController>> _mockLogger;
    private readonly MfaController _controller;
    private readonly Guid _testUserId = Guid.NewGuid();

    public MfaControllerTests()
    {
        _mockMfaService = new Mock<IMfaService>();
        _mockLogger = new Mock<ILogger<MfaController>>();
        _controller = new MfaController(_mockMfaService.Object, _mockLogger.Object);

        // Setup user context
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, _testUserId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = principal
            }
        };
    }

    [Fact]
    public async Task GetMfaStatus_WhenUserExists_ShouldReturnOkWithStatus()
    {
        // Arrange
        var mfaStatus = new MfaStatus
        {
            IsEnabled = true,
            Method = MfaMethod.TOTP,
            BackupCodesCount = 8,
            HasPhoneNumber = true,
            MaskedPhoneNumber = "***-***-7890",
            MaskedEmail = "t***@example.com"
        };

        _mockMfaService.Setup(x => x.GetMfaStatusAsync(_testUserId))
            .ReturnsAsync(mfaStatus);

        // Act
        var result = await _controller.GetMfaStatus();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MfaStatusResponse>().Subject;
        
        response.IsEnabled.Should().BeTrue();
        response.Method.Should().Be(MfaMethod.TOTP);
        response.BackupCodesCount.Should().Be(8);
        response.HasPhoneNumber.Should().BeTrue();
        response.MaskedPhoneNumber.Should().Be("***-***-7890");
        response.MaskedEmail.Should().Be("t***@example.com");
    }

    [Fact]
    public async Task GetMfaStatus_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockMfaService.Setup(x => x.GetMfaStatusAsync(_testUserId))
            .ReturnsAsync((MfaStatus)null);

        // Act
        var result = await _controller.GetMfaStatus();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task SetupMfa_WhenValidRequest_ShouldReturnOkWithSetupInfo()
    {
        // Arrange
        var request = new MfaSetupRequest { Method = MfaMethod.TOTP };
        var setupInfo = new MfaSetupInfo
        {
            Method = MfaMethod.TOTP,
            TotpSecret = "JBSWY3DPEHPK3PXP",
            QrCodeUrl = "otpauth://totp/test",
            BackupCodes = new[] { "ABC123", "DEF456", "GHI789" }
        };

        _mockMfaService.Setup(x => x.SetupMfaAsync(_testUserId, MfaMethod.TOTP))
            .ReturnsAsync(setupInfo);

        // Act
        var result = await _controller.SetupMfa(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<MfaSetupResponse>().Subject;
        
        response.Method.Should().Be(MfaMethod.TOTP);
        response.TotpSecret.Should().Be("JBSWY3DPEHPK3PXP");
        response.QrCodeUrl.Should().Be("otpauth://totp/test");
        response.BackupCodes.Should().HaveCount(3);
    }

    [Fact]
    public async Task SetupMfa_WhenInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("Method", "Method is required");

        // Act
        var result = await _controller.SetupMfa(new MfaSetupRequest());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task VerifySetup_WhenValidCode_ShouldReturnOk()
    {
        // Arrange
        var request = new MfaVerificationRequest { Code = "123456" };
        
        _mockMfaService.Setup(x => x.VerifySetupAsync(_testUserId, "123456"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.VerifySetup(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var successProperty = response.GetType().GetProperty("Success");
        successProperty.Should().NotBeNull();
        successProperty.GetValue(response).Should().Be(true);
    }

    [Fact]
    public async Task VerifySetup_WhenInvalidCode_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new MfaVerificationRequest { Code = "000000" };
        
        _mockMfaService.Setup(x => x.VerifySetupAsync(_testUserId, "000000"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.VerifySetup(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ValidateMfaCode_WhenValidCode_ShouldReturnOk()
    {
        // Arrange
        var request = new MfaVerificationRequest { Code = "123456" };
        
        _mockMfaService.Setup(x => x.ValidateMfaCodeAsync(_testUserId, "123456"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.ValidateMfaCode(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var validProperty = response.GetType().GetProperty("Valid");
        validProperty.Should().NotBeNull();
        validProperty.GetValue(response).Should().Be(true);
    }

    [Fact]
    public async Task ValidateMfaCode_WhenInvalidCode_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new MfaVerificationRequest { Code = "000000" };
        
        _mockMfaService.Setup(x => x.ValidateMfaCodeAsync(_testUserId, "000000"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.ValidateMfaCode(request);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task GenerateBackupCodes_WhenUserExists_ShouldReturnOkWithCodes()
    {
        // Arrange
        var backupCodes = new[] { "ABC123", "DEF456", "GHI789" };
        
        _mockMfaService.Setup(x => x.GenerateBackupCodesAsync(_testUserId))
            .ReturnsAsync(backupCodes);

        // Act
        var result = await _controller.GenerateBackupCodes();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeOfType<BackupCodesResponse>().Subject;
        
        response.BackupCodes.Should().HaveCount(3);
        response.BackupCodes.Should().Contain("ABC123");
        response.BackupCodes.Should().Contain("DEF456");
        response.BackupCodes.Should().Contain("GHI789");
    }

    [Fact]
    public async Task GenerateBackupCodes_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockMfaService.Setup(x => x.GenerateBackupCodesAsync(_testUserId))
            .ReturnsAsync((string[])null);

        // Act
        var result = await _controller.GenerateBackupCodes();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task GetBackupCodesCount_WhenUserExists_ShouldReturnOkWithCount()
    {
        // Arrange
        _mockMfaService.Setup(x => x.GetBackupCodesCountAsync(_testUserId))
            .ReturnsAsync(5);

        // Act
        var result = await _controller.GetBackupCodesCount();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var countProperty = response.GetType().GetProperty("Count");
        countProperty.Should().NotBeNull();
        countProperty.GetValue(response).Should().Be(5);
    }

    [Fact]
    public async Task DisableMfa_WhenUserExists_ShouldReturnOk()
    {
        // Arrange
        _mockMfaService.Setup(x => x.DisableMfaAsync(_testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.DisableMfa();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var successProperty = response.GetType().GetProperty("Success");
        successProperty.Should().NotBeNull();
        successProperty.GetValue(response).Should().Be(true);
    }

    [Fact]
    public async Task DisableMfa_WhenUserNotFound_ShouldReturnNotFound()
    {
        // Arrange
        _mockMfaService.Setup(x => x.DisableMfaAsync(_testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.DisableMfa();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task SendMfaChallenge_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        _mockMfaService.Setup(x => x.SendMfaChallengeAsync(_testUserId))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.SendMfaChallenge();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var sentProperty = response.GetType().GetProperty("Sent");
        sentProperty.Should().NotBeNull();
        sentProperty.GetValue(response).Should().Be(true);
    }

    [Fact]
    public async Task SendMfaChallenge_WhenFailed_ShouldReturnBadRequest()
    {
        // Arrange
        _mockMfaService.Setup(x => x.SendMfaChallengeAsync(_testUserId))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.SendMfaChallenge();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task TestSmsDelivery_WhenSuccessful_ShouldReturnOk()
    {
        // Arrange
        var request = new TestSmsRequest { PhoneNumber = "+1234567890" };
        
        _mockMfaService.Setup(x => x.TestSmsDeliveryAsync("+1234567890"))
            .ReturnsAsync(true);

        // Act
        var result = await _controller.TestSmsDelivery(request);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value.Should().BeAssignableTo<object>().Subject;
        
        var deliveredProperty = response.GetType().GetProperty("Delivered");
        deliveredProperty.Should().NotBeNull();
        deliveredProperty.GetValue(response).Should().Be(true);
    }

    [Fact]
    public async Task TestSmsDelivery_WhenFailed_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new TestSmsRequest { PhoneNumber = "+1234567890" };
        
        _mockMfaService.Setup(x => x.TestSmsDeliveryAsync("+1234567890"))
            .ReturnsAsync(false);

        // Act
        var result = await _controller.TestSmsDelivery(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task TestSmsDelivery_WhenInvalidModelState_ShouldReturnBadRequest()
    {
        // Arrange
        _controller.ModelState.AddModelError("PhoneNumber", "Phone number is required");

        // Act
        var result = await _controller.TestSmsDelivery(new TestSmsRequest());

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
