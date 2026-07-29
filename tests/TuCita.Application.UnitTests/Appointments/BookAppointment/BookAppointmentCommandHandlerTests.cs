using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Appointments.BookAppointment;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Appointments.BookAppointment;

public class BookAppointmentCommandHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IIdentifierGenerator> _identifierGenerator = new();
    private readonly BookAppointmentCommandHandler _handler;
    private static readonly DateOnly Monday = new(2026, 8, 3);

    public BookAppointmentCommandHandlerTests()
    {
        _handler = new BookAppointmentCommandHandler(
            _professionalRepository.Object,
            _appointmentRepository.Object,
            _identifierGenerator.Object);
    }

    private static BookAppointmentCommand CreateCommand(TimeOnly startTime) => new(
        "wander-genao",
        "Juan Pérez",
        "8095551234",
        Monday,
        startTime);

    [Fact]
    public async Task HandleAsync_ShouldBookAppointment_WhenSlotIsAvailable()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var appointmentId = new AppointmentId("appointment-001");

        _professionalRepository.Setup(r => r.GetBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);
        _identifierGenerator.Setup(g => g.NewAppointmentId()).Returns(appointmentId);

        // Act
        var response = await _handler.HandleAsync(CreateCommand(new TimeOnly(9, 0)));

        // Assert
        response.AppointmentId.Should().Be(appointmentId);
        _appointmentRepository.Verify(r => r.AddAsync(
            It.Is<Appointment>(a => a.StartTime == new TimeOnly(9, 0) && a.EndTime == new TimeOnly(9, 30)),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenProfessionalDoesNotExist()
    {
        // Arrange
        _professionalRepository.Setup(r => r.GetBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(new TimeOnly(9, 0)));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenSlotIsOutsideWorkingHours()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(new TimeOnly(19, 0)));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenSlotOverlapsExistingAppointment()
    {
        // Arrange
        var professional = ProfessionalTestData.Create();
        var existingAppointment = Appointment.Create(
            new AppointmentId("appointment-existing"),
            professional.Id,
            "Otro Cliente",
            PhoneNumber.Create("8095559999"),
            Monday,
            new TimeOnly(9, 0),
            new TimeOnly(9, 30));

        _professionalRepository.Setup(r => r.GetBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[existingAppointment]);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(new TimeOnly(9, 0)));

        // Assert
        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}
