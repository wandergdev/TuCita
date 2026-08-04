using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Options;
using TuCita.Application.Professionals.ResendVerificationEmail;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.ResendVerificationEmail;

public class ResendVerificationEmailCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ILogger<ResendVerificationEmailCommandHandler>> _logger = new();
    private readonly ResendVerificationEmailCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public ResendVerificationEmailCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);

        var frontendOptions = Microsoft.Extensions.Options.Options.Create(
            new FrontendOptions { BaseUrl = "https://tucita.example" });

        _handler = new ResendVerificationEmailCommandHandler(
            _professionalRepository.Object,
            _emailSender.Object,
            _dateTimeProvider.Object,
            frontendOptions,
            _logger.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldIssueNewTokenAndSendEmail_WhenProfessionalIsUnverified()
    {
        // Arrange
        var professional = ProfessionalTestData.Create(emailVerified: false);
        professional.SetEmailVerificationToken("old-token", Now.AddHours(-1));
        _professionalRepository.Setup(r => r.GetByEmailAsync(professional.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        await _handler.HandleAsync(new ResendVerificationEmailCommand(professional.Email.Value));

        // Assert
        professional.EmailVerificationToken.Should().NotBe("old-token");
        professional.EmailVerificationTokenExpiresAt.Should().Be(Now.AddHours(24));
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
        await _handler.HandleAsync(new ResendVerificationEmailCommand("missing@email.com"));

        // Assert
        _professionalRepository.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldDoNothing_WhenProfessionalIsAlreadyVerified()
    {
        // Arrange
        var professional = ProfessionalTestData.Create(emailVerified: true);
        _professionalRepository.Setup(r => r.GetByEmailAsync(professional.Email.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        await _handler.HandleAsync(new ResendVerificationEmailCommand(professional.Email.Value));

        // Assert
        _professionalRepository.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
        _emailSender.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
