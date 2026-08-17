using FluentAssertions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.UnitTests.TestData;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.Entities;

public class AppointmentTests
{
    [Fact]
    public void Constructor_ShouldCreateAppointment_WhenDataIsValid()
    {
        // Arrange & Act
        var appointment = AppointmentTestData.Create();

        // Assert
        appointment.Should().NotBeNull();
        appointment.CustomerName.Should().Be("Juan Pérez");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenIdIsNull()
    {
        // Arrange & Act
        Action action = () =>  Appointment.Create(
            null!,
            new ProfessionalId("professional-001"),
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            new DateOnly(2026, 7, 15),
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenProfessionalIdIsNull()
    {
        // Arrange & Act
        Action action = () => Appointment.Create(
            new AppointmentId("appointment-001"),
            null!,
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            new DateOnly(2026, 7, 15),
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenCustomerNameIsEmpty()
    {
        // Arrange & Act
        Action action = () =>  Appointment.Create(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "",
            PhoneNumber.Create("8095551234"),
            new DateOnly(2026, 7, 15),
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*nombre*");
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenCustomerPhoneIsNull()
    {
        // Arrange & Act
        Action action = () => Appointment.Create(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "Juan Pérez",
            null!,
            new DateOnly(2026, 7, 15),
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Constructor_ShouldThrowException_WhenStartTimeIsGreaterThanEndTime()
    {
        // Arrange & Act
        Action action = () => Appointment.Create(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            new DateOnly(2026, 7, 15),
            new TimeOnly(10, 0),
            new TimeOnly(9, 30));

        // Assert
        action.Should()
            .Throw<ArgumentException>()
            .WithMessage("*hora de inicio*");
    }

    [Fact]
    public void Constructor_ShouldInitializeStatusAsScheduled()
    {
        // Arrange & Act
        var appointment = AppointmentTestData.Create();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Constructor_ShouldInitializeCreatedAt()
    {
        // Arrange
        var before = DateTime.UtcNow;

        // Act
        var appointment = AppointmentTestData.Create();

        var after = DateTime.UtcNow;

        // Assert
        appointment.CreatedAt.Should().BeOnOrAfter(before);
        appointment.CreatedAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public void Constructor_ShouldTrimCustomerName()
    {
        // Arrange & Act
        var appointment = Appointment.Create(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "   Juan Pérez   ",
            PhoneNumber.Create("8095551234"),
            new DateOnly(2026, 7, 15),
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        // Assert
        appointment.CustomerName.Should().Be("Juan Pérez");
    }
    
    [Fact]
    public void Cancel_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var appointment = AppointmentTestData.Create();

        // Act
        appointment.Cancel();

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
    }

    [Fact]
    public void Complete_ShouldChangeStatusToCompleted_WhenAppointmentTimeAlreadyPassed()
    {
        // Arrange
        var appointment = AppointmentTestData.Create();
        var now = new DateTime(2026, 7, 15, 10, 0, 0);

        // Act
        appointment.Complete(now);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Completed);
    }

    [Fact]
    public void Complete_ShouldThrowException_WhenAppointmentTimeHasNotPassedYet()
    {
        // Arrange
        var appointment = AppointmentTestData.Create();
        var now = new DateTime(2026, 7, 15, 8, 0, 0);

        // Act
        Action action = () => appointment.Complete(now);

        // Assert
        action.Should().Throw<InvalidOperationException>();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void MarkAsNoShow_ShouldChangeStatusToNoShow_WhenAppointmentTimeAlreadyPassed()
    {
        // Arrange
        var appointment = AppointmentTestData.Create();
        var now = new DateTime(2026, 7, 15, 10, 0, 0);

        // Act
        appointment.MarkAsNoShow(now);

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
    }

    [Fact]
    public void MarkAsNoShow_ShouldThrowException_WhenAppointmentTimeHasNotPassedYet()
    {
        // Arrange
        var appointment = AppointmentTestData.Create();
        var now = new DateTime(2026, 7, 15, 8, 0, 0);

        // Act
        Action action = () => appointment.MarkAsNoShow(now);

        // Assert
        action.Should().Throw<InvalidOperationException>();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public void Cancel_ShouldThrowException_WhenAppointmentIsAlreadyCancelled()
    {
        var appointment = AppointmentTestData.Create();

        appointment.Cancel();

        Action action = appointment.Cancel;

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Complete_ShouldThrowException_WhenAppointmentIsCancelled()
    {
        var appointment = AppointmentTestData.Create();
        var now = new DateTime(2026, 7, 15, 10, 0, 0);

        appointment.Cancel();

        Action action = () => appointment.Complete(now);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkAsNoShow_ShouldThrowException_WhenAppointmentIsCompleted()
    {
        var appointment = AppointmentTestData.Create();
        var now = new DateTime(2026, 7, 15, 10, 0, 0);

        appointment.Complete(now);

        Action action = () => appointment.MarkAsNoShow(now);

        action.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void MarkReminderSent_ShouldSetReminderSentAt()
    {
        // Arrange
        var appointment = AppointmentTestData.Create();
        var sentAt = new DateTime(2026, 7, 15, 8, 40, 0);

        // Act
        appointment.MarkReminderSent(sentAt);

        // Assert
        appointment.ReminderSentAt.Should().Be(sentAt);
    }
}