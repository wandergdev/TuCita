using FluentAssertions;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Services;
using TuCita.Domain.UnitTests.TestData;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.UnitTests.Services;

public class AvailabilitySlotCalculatorTests
{
    private static readonly DateOnly Monday = new(2026, 8, 3);
    private static readonly DateOnly Sunday = new(2026, 8, 2);

    [Fact]
    public void CalculateAvailableSlots_ShouldReturnEmpty_WhenProfessionalDoesNotWorkThatDay()
    {
        // Arrange
        var workingSchedule = ProfessionalTestData.CreateWorkingSchedule();
        var duration = AppointmentDuration.Create(30);

        // Act
        var slots = AvailabilitySlotCalculator.CalculateAvailableSlots(
            workingSchedule, duration, Sunday, []);

        // Assert
        slots.Should().BeEmpty();
    }

    [Fact]
    public void CalculateAvailableSlots_ShouldReturnAllSlots_WhenNoAppointmentsExist()
    {
        // Arrange
        var workingSchedule = new WorkingSchedule(
        [
            new ScheduleRule(
                new WorkDays([DayOfWeek.Monday]),
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)))
        ]);
        var duration = AppointmentDuration.Create(30);

        // Act
        var slots = AvailabilitySlotCalculator.CalculateAvailableSlots(
            workingSchedule, duration, Monday, []);

        // Assert
        slots.Should().BeEquivalentTo([new TimeOnly(9, 0), new TimeOnly(9, 30)]);
    }

    [Fact]
    public void CalculateAvailableSlots_ShouldExcludeSlot_WhenAlreadyBookedByScheduledAppointment()
    {
        // Arrange
        var workingSchedule = new WorkingSchedule(
        [
            new ScheduleRule(
                new WorkDays([DayOfWeek.Monday]),
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(10, 0)))
        ]);
        var duration = AppointmentDuration.Create(30);

        var existingAppointment = Appointment.Create(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            Monday,
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        // Act
        var slots = AvailabilitySlotCalculator.CalculateAvailableSlots(
            workingSchedule, duration, Monday, [existingAppointment]);

        // Assert
        slots.Should().BeEquivalentTo([new TimeOnly(9, 30)]);
    }

    [Fact]
    public void CalculateAvailableSlots_ShouldIncludeSlot_WhenExistingAppointmentIsCancelled()
    {
        // Arrange
        var workingSchedule = new WorkingSchedule(
        [
            new ScheduleRule(
                new WorkDays([DayOfWeek.Monday]),
                TimeRange.Create(new TimeOnly(9, 0), new TimeOnly(9, 30)))
        ]);
        var duration = AppointmentDuration.Create(30);

        var cancelledAppointment = Appointment.Create(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            Monday,
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));
        cancelledAppointment.Cancel();

        // Act
        var slots = AvailabilitySlotCalculator.CalculateAvailableSlots(
            workingSchedule, duration, Monday, [cancelledAppointment]);

        // Assert
        slots.Should().BeEquivalentTo([new TimeOnly(9, 0)]);
    }

    [Fact]
    public void IsSlotAvailable_ShouldReturnFalse_WhenSlotOverlapsScheduledAppointment()
    {
        // Arrange
        var existingAppointment = Appointment.Create(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            Monday,
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        // Act
        var isAvailable = AvailabilitySlotCalculator.IsSlotAvailable(
            new TimeOnly(9, 15), new TimeOnly(9, 45), [existingAppointment]);

        // Assert
        isAvailable.Should().BeFalse();
    }

    [Fact]
    public void IsSlotAvailable_ShouldReturnTrue_WhenSlotDoesNotOverlap()
    {
        // Arrange
        var existingAppointment = Appointment.Create(
            new AppointmentId("appointment-001"),
            new ProfessionalId("professional-001"),
            "Juan Pérez",
            PhoneNumber.Create("8095551234"),
            Monday,
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        // Act
        var isAvailable = AvailabilitySlotCalculator.IsSlotAvailable(
            new TimeOnly(9, 30), new TimeOnly(10, 0), [existingAppointment]);

        // Assert
        isAvailable.Should().BeTrue();
    }
}
