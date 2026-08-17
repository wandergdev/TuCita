using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Appointments.MarkAppointmentAsNoShow;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Appointments.MarkAppointmentAsNoShow;

public class MarkAppointmentAsNoShowCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly MarkAppointmentAsNoShowCommandHandler _handler;
    private static readonly ProfessionalId OwnerId = new("professional-001");

    public MarkAppointmentAsNoShowCommandHandlerTests()
    {
        _handler = new MarkAppointmentAsNoShowCommandHandler(_appointmentRepository.Object, _dateTimeProvider.Object);
    }

    private static Appointment CreateScheduledAppointment() => Appointment.Create(
        new AppointmentId("appointment-001"),
        OwnerId,
        "Juan Pérez",
        PhoneNumber.Create("8095551234"),
        new DateOnly(2026, 8, 3),
        new TimeOnly(9, 0),
        new TimeOnly(9, 30));

    [Fact]
    public async Task HandleAsync_ShouldMarkAsNoShow_WhenAppointmentTimeAlreadyPassed()
    {
        // Arrange
        var appointment = CreateScheduledAppointment();
        _appointmentRepository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _dateTimeProvider.Setup(d => d.Now).Returns(new DateTime(2026, 8, 3, 10, 0, 0));

        // Act
        await _handler.HandleAsync(new MarkAppointmentAsNoShowCommand(appointment.Id, OwnerId));

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.NoShow);
        _appointmentRepository.Verify(r => r.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowInvalidOperation_WhenAppointmentTimeHasNotPassedYet()
    {
        // Arrange
        var appointment = CreateScheduledAppointment();
        _appointmentRepository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        _dateTimeProvider.Setup(d => d.Now).Returns(new DateTime(2026, 8, 3, 8, 0, 0));

        // Act
        var action = () => _handler.HandleAsync(new MarkAppointmentAsNoShowCommand(appointment.Id, OwnerId));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
        _appointmentRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowForbidden_WhenRequestedByAnotherProfessional()
    {
        // Arrange
        var appointment = CreateScheduledAppointment();
        _appointmentRepository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);
        var anotherProfessionalId = new ProfessionalId("professional-002");

        // Act
        var action = () => _handler.HandleAsync(
            new MarkAppointmentAsNoShowCommand(appointment.Id, anotherProfessionalId));

        // Assert
        await action.Should().ThrowAsync<ForbiddenException>();
        appointment.Status.Should().Be(AppointmentStatus.Scheduled);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenAppointmentDoesNotExist()
    {
        // Arrange
        var missingId = new AppointmentId("appointment-missing");
        _appointmentRepository.Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Appointment?)null);

        // Act
        var action = () => _handler.HandleAsync(new MarkAppointmentAsNoShowCommand(missingId, OwnerId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
