using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.VerifyEmail;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.VerifyEmail;

public class VerifyEmailCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly VerifyEmailCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public VerifyEmailCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);
        _handler = new VerifyEmailCommandHandler(_professionalRepository.Object, _dateTimeProvider.Object);
    }

    private static Professional CreateUnverifiedProfessional(DateTime expiresAt)
    {
        var professional = ProfessionalTestData.Create(emailVerified: false);
        professional.SetEmailVerificationToken("valid-token", expiresAt);
        return professional;
    }

    [Fact]
    public async Task HandleAsync_ShouldVerifyEmail_WhenTokenIsValidAndNotExpired()
    {
        // Arrange
        var professional = CreateUnverifiedProfessional(Now.AddHours(1));
        _professionalRepository.Setup(r => r.GetByEmailVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        await _handler.HandleAsync(new VerifyEmailCommand("valid-token"));

        // Assert
        professional.EmailVerified.Should().BeTrue();
        _professionalRepository.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenTokenDoesNotMatchAnyProfessional()
    {
        // Arrange
        _professionalRepository.Setup(r => r.GetByEmailVerificationTokenAsync("missing-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(new VerifyEmailCommand("missing-token"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenTokenHasExpired()
    {
        // Arrange: el token venció hace una hora.
        var professional = CreateUnverifiedProfessional(Now.AddHours(-1));
        _professionalRepository.Setup(r => r.GetByEmailVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var action = () => _handler.HandleAsync(new VerifyEmailCommand("valid-token"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        professional.EmailVerified.Should().BeFalse();
        _professionalRepository.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
