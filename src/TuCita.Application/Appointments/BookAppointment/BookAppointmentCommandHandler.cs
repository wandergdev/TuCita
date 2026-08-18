using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Common;
using TuCita.Application.Common.Exceptions;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using TuCita.Domain.Services;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Appointments.BookAppointment;

/// <summary>
/// Se encarga de reservar una nueva cita para un profesional.
/// </summary>
public sealed class BookAppointmentCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleExceptionRepository _scheduleExceptionRepository;
    private readonly IIdentifierGenerator _identifierGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IWhatsAppSender _whatsAppSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _frontendBaseUrl;
    private readonly ILogger<BookAppointmentCommandHandler> _logger;

    public BookAppointmentCommandHandler(
        IProfessionalRepository professionalRepository,
        IAppointmentRepository appointmentRepository,
        IScheduleExceptionRepository scheduleExceptionRepository,
        IIdentifierGenerator identifierGenerator,
        IEmailSender emailSender,
        IWhatsAppSender whatsAppSender,
        IDateTimeProvider dateTimeProvider,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<BookAppointmentCommandHandler> logger)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _scheduleExceptionRepository = scheduleExceptionRepository
            ?? throw new ArgumentNullException(nameof(scheduleExceptionRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
        _emailSender = emailSender
            ?? throw new ArgumentNullException(nameof(emailSender));
        _whatsAppSender = whatsAppSender
            ?? throw new ArgumentNullException(nameof(whatsAppSender));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _frontendBaseUrl = frontendOptions?.Value.BaseUrl
            ?? throw new ArgumentNullException(nameof(frontendOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BookAppointmentResponse> HandleAsync(
        BookAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var professional = await _professionalRepository.GetBySlugAsync(command.Slug, cancellationToken);

        if (professional is null || !professional.IsActive)
            throw new NotFoundException("El profesional solicitado no existe.");

        var endTime = command.StartTime.Add(
            TimeSpan.FromMinutes(professional.AppointmentDuration.Minutes));

        var exception = await _scheduleExceptionRepository.GetForDateAsync(
            professional.Id,
            command.Date,
            cancellationToken);

        var workingHours = AvailabilitySlotCalculator.GetEffectiveWorkingHours(
            professional.WorkingSchedule,
            command.Date,
            exception);
        if (workingHours is null
            || command.StartTime < workingHours.StartTime
            || endTime > workingHours.EndTime)
            throw new ArgumentException("El horario seleccionado está fuera de la jornada laboral.");

        var lunchBreak = AvailabilitySlotCalculator.GetEffectiveLunchBreak(
            professional.WorkingSchedule,
            command.Date,
            exception);
        if (AvailabilitySlotCalculator.OverlapsLunchBreak(command.StartTime, endTime, lunchBreak))
            throw new ArgumentException("El horario seleccionado coincide con la hora de almuerzo.");

        if (command.Date.ToDateTime(command.StartTime) <= _dateTimeProvider.Now)
            throw new ArgumentException("El horario seleccionado ya pasó.");

        var existingAppointments = await _appointmentRepository.GetByProfessionalAndDateAsync(
            professional.Id,
            command.Date,
            cancellationToken);

        if (!AvailabilitySlotCalculator.IsSlotAvailable(command.StartTime, endTime, existingAppointments))
            throw new InvalidOperationException("El horario seleccionado ya no está disponible.");

        var appointment = Appointment.Create(
            _identifierGenerator.NewAppointmentId(),
            professional.Id,
            command.CustomerName,
            PhoneNumber.Create(command.CustomerPhone),
            command.Date,
            command.StartTime,
            endTime);

        await _appointmentRepository.AddAsync(appointment, cancellationToken);

        // Correo y WhatsApp corren en paralelo e independientes: que uno
        // falle o se cuelgue no debe afectar al otro ni retrasar el doble
        // la respuesta de la reserva (el cliente ya la completó en este punto).
        await Task.WhenAll(
            NotifyProfessionalByEmailAsync(professional, appointment, cancellationToken),
            NotifyProfessionalByWhatsAppAsync(professional, appointment, cancellationToken));

        return new BookAppointmentResponse(appointment.Id);
    }

    private async Task NotifyProfessionalByEmailAsync(
        Professional professional,
        Appointment appointment,
        CancellationToken cancellationToken)
    {
        try
        {
            var (textBody, htmlBody) = EmailTemplates.NewAppointmentNotification(
                appointment.CustomerName,
                appointment.CustomerPhone.Value,
                appointment.Date,
                appointment.StartTime,
                appointment.EndTime,
                $"{_frontendBaseUrl}/panel");

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(8));

            await _emailSender.SendAsync(
                professional.Email.Value,
                "Nueva cita reservada",
                textBody,
                htmlBody,
                timeoutCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo notificar por correo al profesional {ProfessionalId} sobre la cita {AppointmentId}",
                professional.Id,
                appointment.Id);
        }
    }

    private async Task NotifyProfessionalByWhatsAppAsync(
        Professional professional,
        Appointment appointment,
        CancellationToken cancellationToken)
    {
        try
        {
            var parameters = WhatsAppTemplates.NewAppointmentNotificationParameters(
                appointment.CustomerName,
                appointment.CustomerPhone.Value,
                appointment.Date,
                appointment.StartTime);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(8));

            await _whatsAppSender.SendTemplateMessageAsync(
                professional.PhoneNumber.Value,
                WhatsAppTemplates.NewAppointmentNotificationName,
                WhatsAppTemplates.LanguageCode,
                parameters,
                timeoutCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo notificar por WhatsApp al profesional {ProfessionalId} sobre la cita {AppointmentId}",
                professional.Id,
                appointment.Id);
        }
    }
}
