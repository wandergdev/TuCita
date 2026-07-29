using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Professionals.RegisterProfessional;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.RegisterProfessional;

public class RegisterProfessionalCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IIdentifierGenerator> _identifierGenerator = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly RegisterProfessionalCommandHandler _handler;

    public RegisterProfessionalCommandHandlerTests()
    {
        _handler = new RegisterProfessionalCommandHandler(
            _professionalRepository.Object,
            _identifierGenerator.Object,
            _passwordHasher.Object);
    }

    private static RegisterProfessionalCommand CreateCommand() => new(
        "Wander Genao",
        "wander@email.com",
        "8095551234",
        "wander-genao",
        Category.Barber,
        30,
        WorkingScheduleTestData.Create(),
        "Sup3rSecret!");

    [Fact]
    public async Task HandleAsync_ShouldRegisterProfessional_WhenSlugIsAvailable()
    {
        // Arrange
        var id = new ProfessionalId("professional-001");
        _professionalRepository.Setup(r => r.ExistsBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _identifierGenerator.Setup(g => g.NewProfessionalId()).Returns(id);
        _passwordHasher.Setup(h => h.Hash("Sup3rSecret!")).Returns("hashed-password");

        // Act
        var response = await _handler.HandleAsync(CreateCommand());

        // Assert
        response.ProfessionalId.Should().Be(id);
        _professionalRepository.Verify(r => r.AddAsync(
            It.Is<Domain.Entities.Professional>(p => p.PasswordHash == "hashed-password"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenSlugAlreadyExists()
    {
        // Arrange
        _professionalRepository.Setup(r => r.ExistsBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand());

        // Assert
        await action.Should()
            .ThrowAsync<InvalidOperationException>()
            .WithMessage("*slug*");
        _professionalRepository.Verify(
            r => r.AddAsync(It.IsAny<Domain.Entities.Professional>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
