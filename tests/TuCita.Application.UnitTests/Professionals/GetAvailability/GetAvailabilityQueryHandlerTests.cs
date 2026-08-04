using FluentAssertions;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Professionals.GetAvailability;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Professionals.GetAvailability;

public class GetAvailabilityQueryHandlerTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IScheduleExceptionRepository> _scheduleExceptionRepository = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly GetAvailabilityQueryHandler _handler;

    private static readonly DateOnly Monday = new(2026, 8, 3);

    public GetAvailabilityQueryHandlerTests()
    {
        _handler = new GetAvailabilityQueryHandler(
            _professionalRepository.Object,
            _appointmentRepository.Object,
            _scheduleExceptionRepository.Object,
            _dateTimeProvider.Object);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSlots_WhenProfessionalExists()
    {
        // Arrange: "ahora" son las 7:00, antes de que abra (8:00), así que nada está en el pasado.
        var professional = ProfessionalTestData.Create();
        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(7, 0)));
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);

        // Act
        var response = await _handler.HandleAsync(
            new GetAvailabilityQuery(professional.Slug.Value, Monday));

        // Assert
        response.AvailableSlots.Should().Contain(new TimeOnly(8, 0));
        response.IsWorkingDay.Should().BeTrue();
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNoSlotsAndIsWorkingDayFalse_WhenDateIsNotAWorkDay()
    {
        // Arrange: el profesional de prueba trabaja Lunes-Viernes; el sábado no.
        var saturday = new DateOnly(2026, 8, 8);
        var professional = ProfessionalTestData.Create();
        _dateTimeProvider.Setup(d => d.Now).Returns(saturday.ToDateTime(new TimeOnly(7, 0)));
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, saturday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);

        // Act
        var response = await _handler.HandleAsync(
            new GetAvailabilityQuery(professional.Slug.Value, saturday));

        // Assert
        response.AvailableSlots.Should().BeEmpty();
        response.IsWorkingDay.Should().BeFalse();
    }

    [Fact]
    public async Task HandleAsync_ShouldExcludePastSlots_WhenDateIsToday()
    {
        // Arrange: "ahora" son las 9:15, los horarios de 8:00 y 8:30 ya pasaron.
        var professional = ProfessionalTestData.Create();
        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(9, 15)));
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);

        // Act
        var response = await _handler.HandleAsync(
            new GetAvailabilityQuery(professional.Slug.Value, Monday));

        // Assert
        response.AvailableSlots.Should().NotContain(new TimeOnly(8, 0));
        response.AvailableSlots.Should().NotContain(new TimeOnly(8, 30));
        response.AvailableSlots.Should().Contain(new TimeOnly(9, 30));
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnNoSlotsAndMessage_WhenDateIsMarkedAsDayOffByException()
    {
        // Arrange: el horario habitual permite el lunes, pero una excepción
        // lo marca como no laborable con un mensaje para el cliente.
        var professional = ProfessionalTestData.Create();
        var dayOff = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            professional.Id,
            Monday,
            Monday,
            workingHours: null,
            message: "No estaré laborando por vacaciones.");

        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(7, 0)));
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);
        _scheduleExceptionRepository.Setup(r => r.GetForDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dayOff);

        // Act
        var response = await _handler.HandleAsync(
            new GetAvailabilityQuery(professional.Slug.Value, Monday));

        // Assert
        response.AvailableSlots.Should().BeEmpty();
        response.IsWorkingDay.Should().BeFalse();
        response.Message.Should().Be("No estaré laborando por vacaciones.");
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnSpecialHoursSlots_WhenScheduleExceptionOverridesHours()
    {
        // Arrange: el horario habitual termina a las 18:00, pero una excepción
        // habilita un horario especial de 19:00 a 20:00 para ese día.
        var professional = ProfessionalTestData.Create();
        var specialHours = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            professional.Id,
            Monday,
            Monday,
            workingHours: TimeRange.Create(new TimeOnly(19, 0), new TimeOnly(20, 0)),
            message: null);

        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(7, 0)));
        _professionalRepository.Setup(r => r.GetBySlugAsync(professional.Slug.Value, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);
        _scheduleExceptionRepository.Setup(r => r.GetForDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(specialHours);

        // Act
        var response = await _handler.HandleAsync(
            new GetAvailabilityQuery(professional.Slug.Value, Monday));

        // Assert
        response.IsWorkingDay.Should().BeTrue();
        response.AvailableSlots.Should().Contain(new TimeOnly(19, 0));
        response.AvailableSlots.Should().NotContain(new TimeOnly(8, 0));
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowNotFound_WhenProfessionalDoesNotExist()
    {
        // Arrange
        _professionalRepository.Setup(r => r.GetBySlugAsync("missing-slug", It.IsAny<CancellationToken>()))
            .ReturnsAsync((Domain.Entities.Professional?)null);

        // Act
        var action = () => _handler.HandleAsync(new GetAvailabilityQuery("missing-slug", Monday));

        // Assert
        await action.Should().ThrowAsync<NotFoundException>();
    }
}
