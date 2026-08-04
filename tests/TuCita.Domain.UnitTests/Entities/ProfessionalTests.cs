using FluentAssertions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.UnitTests.TestData;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.Entities;

public class ProfessionalTests
{
    [Fact]
    public void Constructor_ShouldCreateProfessional_WhenDataIsValid()
    {
        // Arrange & Act
        var professional = ProfessionalTestData.Create();

        // Assert
        professional.Should().NotBeNull();
        professional.FullName.Should().Be("Wander Genao");
        professional.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenNameIsEmpty()
    {
        // Arrange
        var id = new ProfessionalId("professional-001");

        // Act
        Action action = () => Professional.Create(
            id,
            "",
            Email.Create("wander@email.com"),
            PhoneNumber.Create("8095551234"),
            Slug.Create("wander-genao"),
            Category.Barber,
            AppointmentDuration.Create(30),
            ProfessionalTestData.CreateWorkingSchedule(),
            "hashed-password");

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*nombre*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenIdIsNull()
    {
        // Act
        Action action = () => Professional.Create(
            null!,
            "Wander Genao",
            Email.Create("wander@email.com"),
            PhoneNumber.Create("8095551234"),
            Slug.Create("wander-genao"),
            Category.Barber,
            AppointmentDuration.Create(30),
            ProfessionalTestData.CreateWorkingSchedule(),
            "hashed-password");

        // Assert
        action.Should()
            .Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldInitializeProfessionalAsActive()
    {
        // Arrange & Act
        var professional = ProfessionalTestData.Create();

        // Assert
        professional.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Constructor_ShouldTrimName_WhenNameContainsLeadingOrTrailingSpaces()
    {
        // Arrange & Act
        var professional = Professional.Create(
            new ProfessionalId("professional-001"),
            "   Wander Genao   ",
            Email.Create("wander@email.com"),
            PhoneNumber.Create("8095551234"),
            Slug.Create("wander-genao"),
            Category.Barber,
            AppointmentDuration.Create(30),
            ProfessionalTestData.CreateWorkingSchedule(),
            "hashed-password");

        // Assert
        professional.FullName.Should().Be("Wander Genao");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenPasswordHashIsEmpty()
    {
        // Act
        Action action = () => Professional.Create(
            new ProfessionalId("professional-001"),
            "Wander Genao",
            Email.Create("wander@email.com"),
            PhoneNumber.Create("8095551234"),
            Slug.Create("wander-genao"),
            Category.Barber,
            AppointmentDuration.Create(30),
            ProfessionalTestData.CreateWorkingSchedule(),
            "");

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*contraseña*");
    }

    [Fact]
    public void Constructor_ShouldStartWithEmailUnverified()
    {
        // Arrange & Act
        var professional = ProfessionalTestData.Create();

        // Assert
        professional.EmailVerified.Should().BeFalse();
        professional.EmailVerificationToken.Should().BeNull();
    }

    [Fact]
    public void SetEmailVerificationToken_ShouldStoreTokenAndExpiry()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var expiresAt = new DateTime(2026, 8, 4, 8, 0, 0);

        // Act
        professional.SetEmailVerificationToken("some-token", expiresAt);

        // Assert
        professional.EmailVerificationToken.Should().Be("some-token");
        professional.EmailVerificationTokenExpiresAt.Should().Be(expiresAt);
    }

    [Fact]
    public void VerifyEmail_ShouldMarkVerifiedAndClearToken()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        professional.SetEmailVerificationToken("some-token", new DateTime(2026, 8, 4, 8, 0, 0));

        // Act
        professional.VerifyEmail();

        // Assert
        professional.EmailVerified.Should().BeTrue();
        professional.EmailVerificationToken.Should().BeNull();
        professional.EmailVerificationTokenExpiresAt.Should().BeNull();
    }
}