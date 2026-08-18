using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using TuCita.Application.Abstractions;
using TuCita.Application.Appointments.SendDueReminders;
using TuCita.Application.Options;
using TuCita.Application.UnitTests.TestData;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using Xunit;

namespace TuCita.Application.UnitTests.Appointments.SendDueReminders;

public class SendDueRemindersCommandHandlerTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepository = new();
    private readonly Mock<IProfessionalRepository> _professionalRepository = new();
    private readonly Mock<IWhatsAppSender> _whatsAppSender = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProvider = new();
    private readonly Mock<ILogger<SendDueRemindersCommandHandler>> _logger = new();

    private static readonly DateTime Now = new(2026, 7, 15, 8, 40, 0);
    private static readonly DateOnly Today = DateOnly.FromDateTime(Now);
    private static readonly DateOnly Tomorrow = Today.AddDays(1);

    private readonly SendDueRemindersCommandHandler _handler;

    public SendDueRemindersCommandHandlerTests()
    {
        _dateTimeProvider.Setup(d => d.Now).Returns(Now);

        _handler = new SendDueRemindersCommandHandler(
            _appointmentRepository.Object,
            _professionalRepository.Object,
            _whatsAppSender.Object,
            _dateTimeProvider.Object,
            Microsoft.Extensions.Options.Options.Create(new ReminderOptions { MinutesBefore = 20 }),
            _logger.Object);
    }

    private static Appointment CreateAppointment(TimeOnly startTime, DateOnly? date = null) => Appointment.Create(
        new AppointmentId($"appointment-{Guid.NewGuid()}"),
        new ProfessionalId("professional-001"),
        "Juan Pérez",
        PhoneNumber.Create("8095551234"),
        date ?? Today,
        startTime,
        startTime.AddMinutes(30));

    private void SetupCandidates(IReadOnlyCollection<Appointment> todayCandidates, IReadOnlyCollection<Appointment>? tomorrowCandidates = null)
    {
        _appointmentRepository.Setup(r => r.GetPendingReminderCandidatesAsync(Today, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todayCandidates);
        _appointmentRepository.Setup(r => r.GetPendingReminderCandidatesAsync(Tomorrow, It.IsAny<CancellationToken>()))
            .ReturnsAsync(tomorrowCandidates ?? []);
    }

    [Fact]
    public async Task HandleAsync_ShouldSendReminderAndMarkSent_WhenAppointmentIsDue()
    {
        // Arrange: la cita empieza en exactamente 20 minutos (el recordatorio vence ahora mismo)
        var appointment = CreateAppointment(TimeOnly.FromDateTime(Now.AddMinutes(20)));
        SetupCandidates([appointment]);
        _professionalRepository.Setup(r => r.GetByIdAsync(appointment.ProfessionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProfessionalTestData.Create());

        // Act
        await _handler.HandleAsync();

        // Assert
        _whatsAppSender.Verify(w => w.SendTemplateMessageAsync(
            "8095551234",
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<CancellationToken>()), Times.Once);
        appointment.ReminderSentAt.Should().Be(Now);
        _appointmentRepository.Verify(r => r.UpdateAsync(appointment, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task HandleAsync_ShouldNotSendReminder_WhenAppointmentIsNotYetDue()
    {
        // Arrange: la cita empieza en 2 horas, el recordatorio de 20 min todavía no aplica
        var appointment = CreateAppointment(TimeOnly.FromDateTime(Now.AddHours(2)));
        SetupCandidates([appointment]);

        // Act
        await _handler.HandleAsync();

        // Assert
        _whatsAppSender.Verify(w => w.SendTemplateMessageAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _appointmentRepository.Verify(r => r.UpdateAsync(It.IsAny<Appointment>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldSkip_WhenAppointmentAlreadyHasReminderSent()
    {
        // Arrange: el repositorio no debería devolver esta cita, pero por si acaso el handler
        // también la descarta si ya tiene ReminderSentAt.
        var appointment = CreateAppointment(TimeOnly.FromDateTime(Now.AddMinutes(20)));
        appointment.MarkReminderSent(Now.AddMinutes(-5));
        SetupCandidates([appointment]);

        // Act
        await _handler.HandleAsync();

        // Assert
        _whatsAppSender.Verify(w => w.SendTemplateMessageAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task HandleAsync_ShouldContinueWithOtherAppointments_WhenSendFailsForOne()
    {
        // Arrange
        var failing = CreateAppointment(TimeOnly.FromDateTime(Now.AddMinutes(20)));
        var succeeding = CreateAppointment(TimeOnly.FromDateTime(Now.AddMinutes(19)));
        SetupCandidates([failing, succeeding]);

        _professionalRepository.Setup(r => r.GetByIdAsync(It.IsAny<ProfessionalId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(ProfessionalTestData.Create());

        var callCount = 0;
        _whatsAppSender
            .Setup(w => w.SendTemplateMessageAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<IReadOnlyList<string>>(),
                It.IsAny<CancellationToken>()))
            .Returns(() =>
            {
                callCount++;
                return callCount == 1
                    ? throw new InvalidOperationException("Meta WhatsApp no disponible")
                    : Task.CompletedTask;
            });

        // Act
        await _handler.HandleAsync();

        // Assert: una de las dos citas quedó marcada, la otra no; se intentó con ambas.
        var markedCount = new[] { failing, succeeding }.Count(a => a.ReminderSentAt.HasValue);
        markedCount.Should().Be(1);
        _whatsAppSender.Verify(w => w.SendTemplateMessageAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<IReadOnlyList<string>>(),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }
}
