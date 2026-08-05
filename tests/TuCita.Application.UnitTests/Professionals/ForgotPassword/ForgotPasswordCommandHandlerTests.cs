using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Options;
using TuCita.Application.Professionals.ForgotPassword;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.ForgotPassword;

public class ForgotPasswordCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
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
            _professionalRepository.Object,
            _emailSender.Object,
            _dateTimeProvider.Object,
            frontendOptions,
            _logger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldIssueResetTokenAndSendEmail_WhenProfessionalExists()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetByEmailAsync(professional.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        await _handler.HandleAsync(new ForgotPasswordCommand(professional.Email.Value));

        // Assert
        professional.PasswordResetToken.Should().NotBeNullOrEmpty();
        professional.PasswordResetTokenExpiresAt.Should().Be(Now.AddHours(1));
        _professionalRepository.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(e => e.SendAsync(
            professional.Email.Value, It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldDoNothing_WhenProfessionalDoesNotExist()
    {
        // Arrange
        _professionalRepository.Setup(r => r.GetByEmailAsync("missing@email.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act
        await _handler.HandleAsync(new ForgotPasswordCommand("missing@email.com"));

        // Assert
        _professionalRepository.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
