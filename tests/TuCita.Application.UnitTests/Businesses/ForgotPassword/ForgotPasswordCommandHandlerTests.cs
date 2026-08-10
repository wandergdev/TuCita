using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.ForgotPassword;
using TuCita.Application.Options;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.ForgotPassword;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _logger = new();
    private readonly ForgotPasswordCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public ForgotPasswordCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);

        var frontendOptions = Microsoft.Extensions.Options.Options.Create(
            new FrontendOptions { BaseUrl = "https://tucita.example" });

        _handler = new ForgotPasswordCommandHandler(
            _businessRepository.Object,
            _emailSender.Object,
            _dateTimeProvider.Object,
            frontendOptions,
            _logger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldIssueResetTokenAndSendEmail_WhenBusinessExists()
    {
        // Arrange
        var business = BusinessTestData.Create();
        _businessRepository.Setup(r => r.GetByEmailAsync(business.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        // Act
        await _handler.HandleAsync(new ForgotPasswordCommand(business.Email.Value));

        // Assert
        business.PasswordResetToken.Should().NotBeNullOrEmpty();
        business.PasswordResetTokenExpiresAt.Should().Be(Now.AddHours(1));
        _businessRepository.Verify(r => r.UpdateAsync(business, It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(e => e.SendAsync(
            business.Email.Value, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldDoNothing_WhenBusinessDoesNotExist()
    {
        // Arrange
        _businessRepository.Setup(r => r.GetByEmailAsync("missing@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        // Act
        await _handler.HandleAsync(new ForgotPasswordCommand("missing@email.com"));

        // Assert
        _businessRepository.Verify(r => r.UpdateAsync(It.IsAny<Business>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
