using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.ResetPassword;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.ResetPassword;

public class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly ResetPasswordCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public ResetPasswordCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);
        _handler = new ResetPasswordCommandHandler(
            _businessRepository.Object,
            _passwordHasher.Object,
            _dateTimeProvider.Object);
    }

    private static Business CreateBusinessWithResetToken(DateTime expiresAt)
    {
        var business = BusinessTestData.Create();
        business.SetPasswordResetToken("valid-token", expiresAt);
        return business;
    }

    [Fact]
    public async Task HandleAsync_ShouldResetPassword_WhenTokenIsValidAndNotExpired()
    {
        // Arrange
        var business = CreateBusinessWithResetToken(Now.AddHours(1));
        _businessRepository.Setup(r => r.GetByPasswordResetTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);
        _passwordHasher.Setup(h => h.Hash("NewPassword123")).Returns("hashed-new-password");

        // Act
        await _handler.HandleAsync(new ResetPasswordCommand("valid-token", "NewPassword123"));

        // Assert
        business.PasswordHash.Should().Be("hashed-new-password");
        business.PasswordResetToken.Should().BeNull();
        business.PasswordResetTokenExpiresAt.Should().BeNull();
        _businessRepository.Verify(r => r.UpdateAsync(business, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenTokenDoesNotMatchAnyBusiness()
    {
        // Arrange
        _businessRepository.Setup(r => r.GetByPasswordResetTokenAsync("missing-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        // Act
        var action = () => _handler.HandleAsync(new ResetPasswordCommand("missing-token", "NewPassword123"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenTokenHasExpired()
    {
        // Arrange: el token venció hace una hora.
        var business = CreateBusinessWithResetToken(Now.AddHours(-1));
        _businessRepository.Setup(r => r.GetByPasswordResetTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        // Act
        var action = () => _handler.HandleAsync(new ResetPasswordCommand("valid-token", "NewPassword123"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        _businessRepository.Verify(r => r.UpdateAsync(It.IsAny<Business>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenNewPasswordIsTooShort()
    {
        // Act
        var action = () => _handler.HandleAsync(new ResetPasswordCommand("valid-token", "short"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        _businessRepository.Verify(
            r => r.GetByPasswordResetTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
