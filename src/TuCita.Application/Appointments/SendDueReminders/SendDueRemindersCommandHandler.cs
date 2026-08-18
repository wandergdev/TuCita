using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Common;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Appointments.SendDueReminders;

/// <summary>
/// Envía el recordatorio de WhatsApp a las citas cuya hora de aviso ya
/// llegó. Pensado para ejecutarse periódicamente desde un proceso en
/// segundo plano, no desde un endpoint HTTP.
/// </summary>
public sealed class SendDueRemindersCommandHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly int _minutesBefore;
    private readonly ILogger<SendDueRemindersCommandHandler> _logger;

    public SendDueRemindersCommandHandler(
        IAppointmentRepository appointmentRepository,
        IProfessionalRepository professionalRepository,
        IWhatsAppSender whatsAppSender,
        IDateTimeProvider dateTimeProvider,
        IOptions<ReminderOptions> reminderOptions,
        ILogger<SendDueRemindersCommandHandler> logger)
    {
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _whatsAppSender = whatsAppSender
            ?? throw new ArgumentNullException(nameof(whatsAppSender));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _minutesBefore = reminderOptions?.Value.MinutesBefore
            ?? throw new ArgumentNullException(nameof(reminderOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider.Now;
        var today = DateOnly.FromDateTime(now);

        var candidates = new List<Appointment>();
        candidates.AddRange(await _appointmentRepository.GetPendingReminderCandidatesAsync(today, cancellationToken));
        candidates.AddRange(await _appointmentRepository.GetPendingReminderCandidatesAsync(today.AddDays(1), cancellationToken));

        foreach (var appointment in candidates)
        {
            if (appointment.ReminderSentAt is not null)
                continue;

            var appointmentAt = appointment.Date.ToDateTime(appointment.StartTime);
            var reminderDueAt = appointmentAt.AddMinutes(-_minutesBefore);

            if (now < reminderDueAt || now >= appointmentAt)
                continue;

            try
            {
                var professional = await _professionalRepository.GetByIdAsync(
                    appointment.ProfessionalId,
                    cancellationToken);

                if (professional is null)
                    continue;

                var parameters = WhatsAppTemplates.AppointmentReminderParameters(
                    appointment.CustomerName,
                    professional.FullName,
                    appointment.StartTime,
                    professional.PhoneNumber.Value);

                await _whatsAppSender.SendTemplateMessageAsync(
                    appointment.CustomerPhone.Value,
                    WhatsAppTemplates.AppointmentReminderName,
                    WhatsAppTemplates.LanguageCode,
                    parameters,
                    cancellationToken);

                appointment.MarkReminderSent(now);
                await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "No se pudo enviar el recordatorio de la cita {AppointmentId}",
                    appointment.Id);
            }
        }
    }
}
