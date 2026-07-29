using FluentAssertions;
using Moq;
using TuCita.Application.Appointments.CancelAppointment;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Appointments.CancelAppointment;

public class CancelAppointmentCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly CancelAppointmentCommandHandler _handler;
    private static readonly ProfessionalId OwnerId = new("professional-001");

    public CancelAppointmentCommandHandlerTests()
    {
        _handler = new CancelAppointmentCommandHandler(_appointmentRepository.Object);
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
    public async Task HandleAsync_ShouldCancelAppointment_WhenRequestedByOwner()
    {
        // Arrange
        var appointment = CreateScheduledAppointment();
        _appointmentRepository.Setup(r => r.GetByIdAsync(appointment.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointment);

        // Act
        await _handler.HandleAsync(new CancelAppointmentCommand(appointment.Id, OwnerId));

        // Assert
        appointment.Status.Should().Be(AppointmentStatus.Cancelled);
        _appointmentRepository.Verify(r => r.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
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
            new CancelAppointmentCommand(appointment.Id, anotherProfessionalId));

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
        var action = () => _handler.HandleAsync(new CancelAppointmentCommand(missingId, OwnerId));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
