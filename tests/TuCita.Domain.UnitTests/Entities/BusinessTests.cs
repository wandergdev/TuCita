using FluentAssertions;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.Entities;

public class BusinessTests
{
    private static Business CreateBusiness() => Business.Create(
        new BusinessId("business-001"),
        "Peluquería Lisbeth",
        Email.Create("peluqueria@email.com"),
        Slug.Create("peluqueria-lisbeth"),
        "hashed-password");

    [Fact]
    public void Create_ShouldCreateBusiness_WhenDataIsValid()
    {
        // Act
        var business = CreateBusiness();

        // Assert
        business.Name.Should().Be("Peluquería Lisbeth");
        business.EmailVerified.Should().BeFalse();
    }

    [Fact]
    public void Create_ShouldThrowException_WhenNameIsEmpty()
    {
        // Act
        Action action = () => Business.Create(
            new BusinessId("business-001"),
            "",
            Email.Create("peluqueria@email.com"),
            Slug.Create("peluqueria-lisbeth"),
            "hashed-password");

        // Assert
        action.Should().Throw<ArgumentException>().WithMessage("*nombre*");
    }

    [Fact]
    public void SetEmailVerificationToken_ShouldStoreTokenAndExpiry()
    {
        // Arrange
        var business = CreateBusiness();
        var expiresAt = new DateTime(2026, 8, 4, 8, 0, 0);

        // Act
        business.SetEmailVerificationToken("some-token", expiresAt);

        // Assert
        business.EmailVerificationToken.Should().Be("some-token");
        business.EmailVerificationTokenExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void VerifyEmail_ShouldMarkVerifiedAndClearToken()
    {
        // Arrange
        var business = CreateBusiness();
        business.SetEmailVerificationToken("some-token", new DateTime(2026, 8, 4, 8, 0, 0));

        // Act
        business.VerifyEmail();

        // Assert
        business.EmailVerified.Should().BeTrue();
        business.EmailVerificationToken.Should().BeNull();
        business.EmailVerificationTokenExpiresAt.Should().BeNull();
    }

    [Fact]
    public void SetPasswordResetToken_ShouldStoreTokenAndExpiry()
    {
        // Arrange
        var business = CreateBusiness();
        var expiresAt = new DateTime(2026, 8, 4, 8, 0, 0);

        // Act
        business.SetPasswordResetToken("some-token", expiresAt);

        // Assert
        business.PasswordResetToken.Should().Be("some-token");
        business.PasswordResetTokenExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void ResetPassword_ShouldReplaceHashAndClearToken()
    {
        // Arrange
        var business = CreateBusiness();
        business.SetPasswordResetToken("some-token", new DateTime(2026, 8, 4, 8, 0, 0));

        // Act
        business.ResetPassword("new-hashed-password");

        // Assert
        business.PasswordHash.Should().Be("new-hashed-password");
        business.PasswordResetToken.Should().BeNull();
        business.PasswordResetTokenExpiresAt.Should().BeNull();
    }
}
