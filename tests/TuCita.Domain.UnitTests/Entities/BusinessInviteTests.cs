using FluentAssertions;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.UnitTests.Entities;

public class BusinessInviteTests
{
    private static readonly DateTime Now = new(2026, 8, 3, 8, 0, 0);

    private static BusinessInvite CreateInvite(DateTime expiresAt) => BusinessInvite.Create(
        new BusinessInviteId("invite-001"),
        new BusinessId("business-001"),
        "some-token",
        expiresAt);

    [Fact]
    public void IsValid_ShouldReturnTrue_WhenNotExpiredAndNotUsed()
    {
        // Arrange
        var invite = CreateInvite(Now.AddDays(1));

        // Act & Assert
        invite.IsValid(Now).Should().BeTrue();
    }

    [Fact]
    public void IsValid_ShouldReturnFalse_WhenExpired()
    {
        // Arrange
        var invite = CreateInvite(Now.AddDays(-1));

        // Act & Assert
        invite.IsValid(Now).Should().BeFalse();
    }

    [Fact]
    public void Consume_ShouldMarkAsUsed_WhenValid()
    {
        // Arrange
        var invite = CreateInvite(Now.AddDays(1));

        // Act
        invite.Consume(Now);

        // Assert
        invite.UsedAt.Should().Be(Now);
        invite.IsValid(Now).Should().BeFalse();
    }

    [Fact]
    public void Consume_ShouldThrowException_WhenAlreadyUsed()
    {
        // Arrange
        var invite = CreateInvite(Now.AddDays(1));
        invite.Consume(Now);

        // Act
        Action action = () => invite.Consume(Now);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Consume_ShouldThrowException_WhenExpired()
    {
        // Arrange
        var invite = CreateInvite(Now.AddDays(-1));

        // Act
        Action action = () => invite.Consume(Now);

        // Assert
        action.Should().Throw<InvalidOperationException>();
    }
}
