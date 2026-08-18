using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Appointments.BookAppointment;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Options;
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
    private readonly Mock<IScheduleExceptionRepository> _scheduleExceptionRepository = new();
    private readonly Mock<IIdentifierGenerator> _identifierGenerator = new();
    private readonly Mock<IEmailSender> _emailSender = new();
    private readonly Mock<IWhatsAppSender> _whatsAppSender = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ILogger<BookAppointmentCommandHandler>> _logger = new();
    private readonly BookAppointmentCommandHandler _handler;
    private static readonly DateOnly Monday = new(2026, 8, 3);

    public BookAppointmentCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Monday.ToDateTime(new TimeOnly(8, 30)));

        var frontendOptions = Microsoft.Extensions.Options.Options.Create(
            new FrontendOptions { BaseUrl = "https://tucita.example" });

        _handler = new BookAppointmentCommandHandler(
            _professionalRepository.Object,
            _appointmentRepository.Object,
            _scheduleExceptionRepository.Object,
            _identifierGenerator.Object,
            _emailSender.Object,
            _whatsAppSender.Object,
            _dateTimeProvider.Object,
            frontendOptions,
            _logger.Object);
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
    public async Task HandleAsync_ShouldThrowException_WhenSlotAlreadyPassed()
    {
        // Arrange: "ahora" son las 8:30 (ver constructor), la cita solicitada es a las 8:00,
        // dentro de la jornada laboral pero ya pasada.
        var professional = ProfessionalTestData.Create();
        _professionalRepository.Setup(r => r.GetBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(new TimeOnly(8, 0)));

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

    [Fact]
    public async Task HandleAsync_ShouldStillBookAppointment_WhenEmailNotificationFails()
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
        _emailSender
            .Setup(e => e.SendAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP no disponible"));

        // Act
        var response = await _handler.HandleAsync(CreateCommand(new TimeOnly(9, 0)));

        // Assert
        response.AppointmentId.Should().Be(appointmentId);
        _appointmentRepository.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldStillBookAppointment_WhenWhatsAppNotificationFails()
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
        _whatsAppSender
            .Setup(w => w.SendTemplateMessageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Meta WhatsApp no disponible"));

        // Act
        var response = await _handler.HandleAsync(CreateCommand(new TimeOnly(9, 0)));

        // Assert
        response.AppointmentId.Should().Be(appointmentId);
        _appointmentRepository.Verify(r => r.AddAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenDateIsMarkedAsDayOffByException()
    {
        // Arrange: el horario habitual permite las 9:00, pero hay una excepción
        // que marca ese día como no laborable.
        var professional = ProfessionalTestData.Create();
        var dayOff = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            professional.Id,
            Monday,
            Monday,
            workingHours: null,
            message: "Vacaciones");

        _professionalRepository.Setup(r => r.GetBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _scheduleExceptionRepository.Setup(r => r.GetForDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(dayOff);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(new TimeOnly(9, 0)));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldBookAppointment_WhenScheduleExceptionOverridesHours()
    {
        // Arrange: el horario habitual termina a las 18:00, pero una excepción
        // habilita un horario especial de 19:00 a 21:00 para ese día.
        var professional = ProfessionalTestData.Create();
        var appointmentId = new AppointmentId("appointment-001");
        var specialHours = ScheduleException.Create(
            new ScheduleExceptionId("exception-001"),
            professional.Id,
            Monday,
            Monday,
            workingHours: TimeRange.Create(new TimeOnly(19, 0), new TimeOnly(21, 0)),
            message: null);

        _professionalRepository.Setup(r => r.GetBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);
        _scheduleExceptionRepository.Setup(r => r.GetForDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync(specialHours);
        _appointmentRepository.Setup(r => r.GetByProfessionalAndDateAsync(
                professional.Id, Monday, It.IsAny<CancellationToken>()))
            .ReturnsAsync((IReadOnlyCollection<Appointment>)[]);
        _identifierGenerator.Setup(g => g.NewAppointmentId()).Returns(appointmentId);

        // Act
        var response = await _handler.HandleAsync(CreateCommand(new TimeOnly(19, 0)));

        // Assert
        response.AppointmentId.Should().Be(appointmentId);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrowException_WhenSlotOverlapsLunchBreak()
    {
        // Arrange: el profesional de prueba trabaja 8:00-18:00; se le agrega
        // un almuerzo de 12:00 a 13:00.
        var professional = ProfessionalTestData.Create();
        professional.UpdateWorkingSchedule(new WorkingSchedule(
        [
            new ScheduleRule(
                new WorkDays([
                    DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
                    DayOfWeek.Thursday, DayOfWeek.Friday,
                ]),
                TimeRange.Create(new TimeOnly(8, 0), new TimeOnly(18, 0)),
                TimeRange.Create(new TimeOnly(12, 0), new TimeOnly(13, 0)))
        ]));

        _professionalRepository.Setup(r => r.GetBySlugAsync("wander-genao", It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var action = () => _handler.HandleAsync(CreateCommand(new TimeOnly(12, 0)));

        // Assert
        await action.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HandleAsync_ShouldNotifyProfessionalByWhatsApp_WhenBookingSucceeds()
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
        await _handler.HandleAsync(CreateCommand(new TimeOnly(9, 0)));

        // Assert
        _whatsAppSender.Verify(w => w.SendTemplateMessageAsync(
            professional.PhoneNumber.Value,
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
