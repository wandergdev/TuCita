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
    private readonly IIdentifierGenerator _identifierGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _frontendBaseUrl;
    private readonly ILogger<BookAppointmentCommandHandler> _logger;

    public BookAppointmentCommandHandler(
        IProfessionalRepository professionalRepository,
        IAppointmentRepository appointmentRepository,
        IIdentifierGenerator identifierGenerator,
        IEmailSender emailSender,
        IDateTimeProvider dateTimeProvider,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<BookAppointmentCommandHandler> logger)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
        _emailSender = emailSender
            ?? throw new ArgumentNullException(nameof(emailSender));
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

        var workingHours = professional.WorkingSchedule.GetWorkingHours(command.Date.DayOfWeek);
        if (workingHours is null
            || command.StartTime < workingHours.StartTime
            || endTime > workingHours.EndTime)
            throw new ArgumentException("El horario seleccionado está fuera de la jornada laboral.");

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

        await NotifyProfessionalAsync(professional, appointment, cancellationToken);

        return new BookAppointmentResponse(appointment.Id);
    }

    private async Task NotifyProfessionalAsync(
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

            // Un envío de correo colgado (ej. una conexión SMTP que nunca
            // responde) no debe bloquear indefinidamente la respuesta de la
            // reserva; el cliente ya la completó exitosamente en ese punto.
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
}
