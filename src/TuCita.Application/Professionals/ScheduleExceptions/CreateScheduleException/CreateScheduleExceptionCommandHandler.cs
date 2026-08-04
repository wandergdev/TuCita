using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Professionals.ScheduleExceptions.CreateScheduleException;

/// <summary>
/// Se encarga de crear una excepción de horario para un profesional.
/// </summary>
public sealed class CreateScheduleExceptionCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IScheduleExceptionRepository _scheduleExceptionRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IIdentifierGenerator _identifierGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CreateScheduleExceptionCommandHandler(
        IProfessionalRepository professionalRepository,
        IScheduleExceptionRepository scheduleExceptionRepository,
        IAppointmentRepository appointmentRepository,
        IIdentifierGenerator identifierGenerator,
        IDateTimeProvider dateTimeProvider)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _scheduleExceptionRepository = scheduleExceptionRepository
            ?? throw new ArgumentNullException(nameof(scheduleExceptionRepository));
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<CreateScheduleExceptionResponse> HandleAsync(
        CreateScheduleExceptionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.StartTime.HasValue != command.EndTime.HasValue)
            throw new ArgumentException(
                "Debe indicar hora de inicio y de fin, o ninguna de las dos.");

        if (command.LunchBreakStartTime.HasValue != command.LunchBreakEndTime.HasValue)
            throw new ArgumentException(
                "Debe indicar hora de inicio y de fin de almuerzo, o ninguna de las dos.");

        if (command.LunchBreakStartTime.HasValue && !command.StartTime.HasValue)
            throw new ArgumentException(
                "No se puede definir un horario de almuerzo en un día no laborable.");

        var professional = await _professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);

        if (professional is null)
            throw new NotFoundException("El profesional solicitado no existe.");

        var today = DateOnly.FromDateTime(_dateTimeProvider.Now);
        if (command.StartDate < today)
            throw new ArgumentException("No se pueden crear excepciones en fechas pasadas.");

        var overlapping = await _scheduleExceptionRepository.GetOverlappingAsync(
            professional.Id,
            command.StartDate,
            command.EndDate,
            cancellationToken);

        if (overlapping.Count > 0)
            throw new InvalidOperationException(
                "Ya existe una excepción de horario que se superpone con estas fechas.");

        var workingHours = command.StartTime.HasValue && command.EndTime.HasValue
            ? TimeRange.Create(command.StartTime.Value, command.EndTime.Value)
            : null;

        var lunchBreak = command.LunchBreakStartTime.HasValue && command.LunchBreakEndTime.HasValue
            ? TimeRange.Create(command.LunchBreakStartTime.Value, command.LunchBreakEndTime.Value)
            : null;

        if (workingHours is null)
        {
            var existingAppointments = await _appointmentRepository.GetByProfessionalAndDateRangeAsync(
                professional.Id,
                command.StartDate,
                command.EndDate,
                cancellationToken);

            if (existingAppointments.Any(appointment => appointment.Status == AppointmentStatus.Scheduled))
                throw new InvalidOperationException(
                    "Tienes citas programadas en ese rango. Cancélalas antes de marcar estos días como no laborables.");
        }

        var scheduleException = ScheduleException.Create(
            _identifierGenerator.NewScheduleExceptionId(),
            professional.Id,
            command.StartDate,
            command.EndDate,
            workingHours,
            lunchBreak,
            command.Message);

        await _scheduleExceptionRepository.AddAsync(scheduleException, cancellationToken);

        return new CreateScheduleExceptionResponse(scheduleException.Id);
    }
}
