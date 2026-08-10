using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.RegisterBusiness;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.RegisterBusiness;

public class RegisterBusinessCommandHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IIdentifierGenerator> _identifierGenerator = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ILogger<RegisterBusinessCommandHandler>> _logger = new();
    private readonly RegisterBusinessCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public RegisterBusinessCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);

        var frontendOptions = Microsoft.Extensions.Options.Options.Create(
            new FrontendOptions { BaseUrl = "https://tucita.example" });

        _handler = new RegisterBusinessCommandHandler(
            _businessRepository.Object,
            _professionalRepository.Object,
            _identifierGenerator.Object,
            _passwordHasher.Object,
            _emailSender.Object,
            _dateTimeProvider.Object,
            frontendOptions,
            _logger.Object);
    }

    private static RegisterBusinessCommand CreateCommand() => new(
        "Peluquería Lisbeth",
        "peluqueria@email.com",
        "peluqueria-lisbeth",
        "Sup3rSecret!");

    [Fact]
    public async Task HandleAsync_ShouldRegisterBusiness_WhenSlugIsAvailable()
    {
        // Arrange
        var id = new BusinessId("business-001");
        _businessRepository.Setup(r => r.ExistsBySlugAsync("peluqueria-lisbeth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _professionalRepository.Setup(r => r.ExistsBySlugAsync("peluqueria-lisbeth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _identifierGenerator.Setup(g => g.NewBusinessId()).Returns(id);
        _passwordHasher.Setup(h => h.Hash("Sup3rSecret!")).Returns("hashed-password");

        // Act
        var response = await _handler.HandleAsync(CreateCommand());

        // Assert
        response.BusinessId.Should().Be(id);
        _businessRepository.Verify(r => r.AddAsync(
            It.Is<Business>(b => b.PasswordHash == "hashed-password" && !b.EmailVerified),
            It.IsAny<CancellationToken>()), Times.Once);
        _emailSender.Verify(e => e.SendAsync(
            "peluqueria@email.com", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenSlugAlreadyExistsAsBusiness()
    {
        // Arrange
        _businessRepository.Setup(r => r.ExistsBySlugAsync("peluqueria-lisbeth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand());

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*slug*");
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenSlugAlreadyExistsAsProfessional()
    {
        // Arrange: el slug se comparte entre negocios y profesionales.
        _businessRepository.Setup(r => r.ExistsBySlugAsync("peluqueria-lisbeth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _professionalRepository.Setup(r => r.ExistsBySlugAsync("peluqueria-lisbeth", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand());

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>().WithMessage("*slug*");
        _businessRepository.Verify(
            r => r.AddAsync(It.IsAny<Business>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
