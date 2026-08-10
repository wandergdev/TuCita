using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.ResendBusinessVerificationEmail;
using TuCita.Application.Options;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.ResendBusinessVerificationEmail;

public class ResendBusinessVerificationEmailCommandHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ILogger<ResendBusinessVerificationEmailCommandHandler>> _logger = new();
    private readonly ResendBusinessVerificationEmailCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public ResendBusinessVerificationEmailCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);

        var frontendOptions = Microsoft.Extensions.Options.Options.Create(
            new FrontendOptions { BaseUrl = "https://tucita.example" });

        _handler = new ResendBusinessVerificationEmailCommandHandler(
            _businessRepository.Object,
            _emailSender.Object,
            _dateTimeProvider.Object,
            frontendOptions,
            _logger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldIssueNewTokenAndSendEmail_WhenBusinessIsUnverified()
    {
        // Arrange
        var business = BusinessTestData.Create(emailVerified: false);
        business.SetEmailVerificationToken("old-token", Now.AddHours(-1));
        _businessRepository.Setup(r => r.GetByEmailAsync(business.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        // Act
        await _handler.HandleAsync(new ResendBusinessVerificationEmailCommand(business.Email.Value));

        // Assert
        business.EmailVerificationToken.Should().NotBe("old-token");
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
        await _handler.HandleAsync(new ResendBusinessVerificationEmailCommand("missing@email.com"));

        // Assert
        _businessRepository.Verify(r => r.UpdateAsync(It.IsAny<Business>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldDoNothing_WhenBusinessIsAlreadyVerified()
    {
        // Arrange
        var business = BusinessTestData.Create(emailVerified: true);
        _businessRepository.Setup(r => r.GetByEmailAsync(business.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        // Act
        await _handler.HandleAsync(new ResendBusinessVerificationEmailCommand(business.Email.Value));

        // Assert
        _businessRepository.Verify(r => r.UpdateAsync(It.IsAny<Business>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
