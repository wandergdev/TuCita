using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.ChangePassword;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.ChangePassword;

public class ChangePasswordCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly ChangePasswordCommandHandler _handler;

    public ChangePasswordCommandHandlerTests()
    {
        _handler = new ChangePasswordCommandHandler(_professionalRepository.Object, _passwordHasher.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldChangePassword_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _passwordHasher.Setup(h => h.Verify("CurrentPass1", professional.PasswordHash)).Returns(true);
        _passwordHasher.Setup(h => h.Hash("NewPassword123")).Returns("hashed-new-password");

        // Act
        await _handler.HandleAsync(new ChangePasswordCommand(professional.Id, "CurrentPass1", "NewPassword123"));

        // Assert
        professional.PasswordHash.Should().Be("hashed-new-password");
        _professionalRepository.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenCurrentPasswordIsIncorrect()
    {
        // Arrange: verificar que sea ArgumentException (400) y no
        // UnauthorizedException (401) — un 401 con un token presente
        // dispara el cierre de sesión automático en el frontend, lo cual
        // sería incorrecto para un simple error de tipeo.
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetByIdAsync(professional.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _passwordHasher.Setup(h => h.Verify("WrongPassword", professional.PasswordHash)).Returns(false);

        // Act
        var action = () => _handler.HandleAsync(
            new ChangePasswordCommand(professional.Id, "WrongPassword", "NewPassword123"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        _professionalRepository.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenNewPasswordIsTooShort()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();

        // Act
        var action = () => _handler.HandleAsync(
            new ChangePasswordCommand(professional.Id, "CurrentPass1", "short"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        _professionalRepository.Verify(r => r.GetByIdAsync(It.IsAny<ProfessionalId>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenProfessionalDoesNotExist()
    {
        // Arrange
        var missingId = new ProfessionalId("professional-missing");
        _professionalRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(
            new ChangePasswordCommand(missingId, "CurrentPass1", "NewPassword123"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
