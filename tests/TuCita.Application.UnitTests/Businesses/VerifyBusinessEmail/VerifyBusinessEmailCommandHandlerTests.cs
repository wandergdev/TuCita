using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.VerifyBusinessEmail;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.VerifyBusinessEmail;

public class VerifyBusinessEmailCommandHandlerTests
{
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly VerifyBusinessEmailCommandHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public VerifyBusinessEmailCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);
        _handler = new VerifyBusinessEmailCommandHandler(_businessRepository.Object, _dateTimeProvider.Object);
    }

    private static Business CreateUnverifiedBusiness(DateTime expiresAt)
    {
        var business = BusinessTestData.Create(emailVerified: false);
        business.SetEmailVerificationToken("valid-token", expiresAt);
        return business;
    }

    [Fact]
    public async Task HandleAsync_ShouldVerifyEmail_WhenTokenIsValidAndNotExpired()
    {
        // Arrange
        var business = CreateUnverifiedBusiness(Now.AddHours(1));
        _businessRepository.Setup(r => r.GetByEmailVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        // Act
        await _handler.HandleAsync(new VerifyBusinessEmailCommand("valid-token"));

        // Assert
        business.EmailVerified.Should().BeTrue();
        _businessRepository.Verify(r => r.UpdateAsync(business, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenTokenDoesNotMatchAnyBusiness()
    {
        // Arrange
        _businessRepository.Setup(r => r.GetByEmailVerificationTokenAsync("missing-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Business?)null);

        // Act
        var action = () => _handler.HandleAsync(new VerifyBusinessEmailCommand("missing-token"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowArgumentException_WhenTokenHasExpired()
    {
        // Arrange
        var business = CreateUnverifiedBusiness(Now.AddHours(-1));
        _businessRepository.Setup(r => r.GetByEmailVerificationTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        // Act
        var action = () => _handler.HandleAsync(new VerifyBusinessEmailCommand("valid-token"));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
        business.EmailVerified.Should().BeFalse();
    }
}
