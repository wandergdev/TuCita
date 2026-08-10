using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Businesses.GetBusinessInvite;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using Xunit;

namespace TuCita.Application.UnitTests.Businesses.GetBusinessInvite;

public class GetBusinessInviteQueryHandlerTests
{
    private readonly Mock<IBusinessInviteRepository> _businessInviteRepository = new();
    private readonly Mock<IBusinessRepository> _businessRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly GetBusinessInviteQueryHandler _handler;

    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    public GetBusinessInviteQueryHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);
        _handler = new GetBusinessInviteQueryHandler(
            _businessInviteRepository.Object,
            _businessRepository.Object,
            _dateTimeProvider.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnBusinessName_WhenInviteIsValid()
    {
        // Arrange
        var business = BusinessTestData.Create();
        var invite = BusinessInvite.Create(
            new BusinessInviteId("invite-001"), business.Id, "valid-token", Now.AddDays(1));
        _businessInviteRepository.Setup(r => r.GetByTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);
        _businessRepository.Setup(r => r.GetByIdAsync(business.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(business);

        // Act
        var response = await _handler.HandleAsync(new GetBusinessInviteQuery("valid-token"));

        // Assert
        response.BusinessName.Should().Be(business.Name);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenTokenDoesNotExist()
    {
        // Arrange
        _businessInviteRepository.Setup(r => r.GetByTokenAsync("missing-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync((BusinessInvite?)null);

        // Act
        var action = () => _handler.HandleAsync(new GetBusinessInviteQuery("missing-token"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenInviteExpired()
    {
        // Arrange
        var invite = BusinessInvite.Create(
            new BusinessInviteId("invite-001"), new BusinessId("business-001"), "valid-token", Now.AddDays(-1));
        _businessInviteRepository.Setup(r => r.GetByTokenAsync("valid-token", It.IsAny<CancellationToken>()))
            .ReturnsAsync(invite);

        // Act
        var action = () => _handler.HandleAsync(new GetBusinessInviteQuery("valid-token"));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
