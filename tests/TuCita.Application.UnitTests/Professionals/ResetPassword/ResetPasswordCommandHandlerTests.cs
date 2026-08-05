using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.ResetPassword;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly ResetPasswordCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public ResetPasswordCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);
        _handler = new ResetPasswordCommandHandler(
            _professionalRepository.Object,
            _passwordHasher.Object,
            _dateTimeProvider.Object);
    }

    private static Professional CreateProfessionalWithResetToken(DateTime expiresAt)
    {
        var professional = ProfessionalTestData.Create();
        professional.SetPasswordResetToken("valid-token", expiresAt);
        return professional;
    }

    [Fact]
    public async Task HandleAsync_ShouldResetPassword_WhenTokenIsValidAndNotExpired()
    {
        // Arrange
        var professional = CreateProfessionalWithResetToken(Now.AddHours(1));
        _professionalRepository.Setup(r => r.GetByPasswordResetTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _passwordHasher.Setup(h => h.Hash("NewPassword123")).Returns("hashed-new-password");

        // Act
        await _handler.HandleAsync(new ResetPasswordCommand("valid-token", "NewPassword123"));

        // Assert
        professional.PasswordHash.Should().Be("hashed-new-password");
        professional.PasswordResetToken.Should().BeNull();
        professional.PasswordResetTokenExpiresAt.Should().BeNull();
        _professionalRepository.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenTokenDoesNotMatchAnyProfessional()
    {
        // Arrange
        _professionalRepository.Setup(r => r.GetByPasswordResetTokenAsync("missing-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(new ResetPasswordCommand("missing-token", "NewPassword123"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenTokenHasExpired()
    {
        // Arrange: el token venció hace una hora.
        var professional = CreateProfessionalWithResetToken(Now.AddHours(-1));
        _professionalRepository.Setup(r => r.GetByPasswordResetTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var action = () => _handler.HandleAsync(new ResetPasswordCommand("valid-token", "NewPassword123"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        _professionalRepository.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenNewPasswordIsTooShort()
    {
        // Act
        var action = () => _handler.HandleAsync(new ResetPasswordCommand("valid-token", "short"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        _professionalRepository.Verify(
            r => r.GetByPasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
