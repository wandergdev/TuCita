using FluentAssertions;
using Moq;
using TuCita.Application.Appointments.GetProfessionalAppointments;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Appointments.GetProfessionalAppointments;

public class GetProfessionalAppointmentsQueryHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly GetProfessionalAppointmentsQueryHandler _handler;
    private static readonly ProfessionalId OwnerId = new("professional-001");

    public GetProfessionalAppointmentsQueryHandlerTests()
    {
        _handler = new GetProfessionalAppointmentsQueryHandler(_appointmentRepository.Object);
    }

    private static Appointment CreateAppointment(string customerName = "Juan Pérez") => Appointment.Create(
        new AppointmentId($"appointment-{Guid.NewGuid()}"),
        OwnerId,
        customerName,
        PhoneNumber.Create("8095551234"),
        new DateOnly(2026, 8, 3),
        new TimeOnly(9, 0),
        new TimeOnly(9, 30));

    [Fact]
    public async Task HandleAsync_ShouldRequestAscendingOrder_WhenStatusIsScheduled()
    {
        // Arrange
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, AppointmentStatus.Scheduled, true, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, AppointmentStatus.Scheduled, 0, 50));

        // Assert
        _appointmentRepository.Verify(r => r.GetByProfessionalAsync(
            OwnerId, AppointmentStatus.Scheduled, true, 0, 51, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.NoShow)]
    public async Task HandleAsync_ShouldRequestDescendingOrder_WhenStatusIsNotScheduled(AppointmentStatus status)
    {
        // Arrange
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, status, false, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, status, 0, 50));

        // Assert
        _appointmentRepository.Verify(r => r.GetByProfessionalAsync(
            OwnerId, status, false, 0, 51, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldRequestDescendingOrder_WhenStatusIsNull()
    {
        // Arrange
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, null, false, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, null, 0, 50));

        // Assert
        _appointmentRepository.Verify(r => r.GetByProfessionalAsync(
            OwnerId, null, false, 0, 51, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetHasMoreTrue_WhenRepositoryReturnsExtraRow()
    {
        // Arrange: se piden 2 (take+1 = 3), el repositorio devuelve 3 -> hay más páginas.
        var appointments = new[] { CreateAppointment(), CreateAppointment(), CreateAppointment() };
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, null, false, 0, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        // Act
        var response = await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, null, 0, 2));

        // Assert
        response.HasMore.Should().BeTrue();
        response.Appointments.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_ShouldSetHasMoreFalse_WhenRepositoryReturnsExactlyRequestedPage()
    {
        // Arrange: se piden 2 (take+1 = 3), el repositorio solo devuelve 2 -> no hay más páginas.
        var appointments = new[] { CreateAppointment(), CreateAppointment() };
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, null, false, 0, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointments);

        // Act
        var response = await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, null, 0, 2));

        // Assert
        response.HasMore.Should().BeFalse();
        response.Appointments.Should().HaveCount(2);
    }

    [Fact]
    public async Task HandleAsync_ShouldClampNegativeSkipToZero()
    {
        // Arrange
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, null, false, 0, It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, null, -10, 50));

        // Assert
        _appointmentRepository.Verify(r => r.GetByProfessionalAsync(
            OwnerId, null, false, 0, It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldDefaultTakeTo50_WhenTakeIsZeroOrNegative()
    {
        // Arrange
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, null, false, It.IsAny<int>(), 51, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, null, 0, 0));

        // Assert
        _appointmentRepository.Verify(r => r.GetByProfessionalAsync(
            OwnerId, null, false, It.IsAny<int>(), 51, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldClampTakeTo100_WhenTakeExceedsMax()
    {
        // Arrange
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, null, false, It.IsAny<int>(), 101, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, null, 0, 500));

        // Assert
        _appointmentRepository.Verify(r => r.GetByProfessionalAsync(
            OwnerId, null, false, It.IsAny<int>(), 101, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldMapAppointmentToSummary()
    {
        // Arrange
        var appointment = CreateAppointment("Ana Gómez");
        _appointmentRepository.Setup(r => r.GetByProfessionalAsync(
                OwnerId, null, false, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync([appointment]);

        // Act
        var response = await _handler.HandleAsync(new GetProfessionalAppointmentsQuery(OwnerId, null, 0, 50));

        // Assert
        var summary = response.Appointments.Single();
        summary.Id.Should().Be(appointment.Id);
        summary.CustomerName.Should().Be("Ana Gómez");
        summary.CustomerPhone.Should().Be(appointment.CustomerPhone.Value);
        summary.Date.Should().Be(appointment.Date);
        summary.StartTime.Should().Be(appointment.StartTime);
        summary.EndTime.Should().Be(appointment.EndTime);
        summary.Status.Should().Be(appointment.Status);
    }
}
