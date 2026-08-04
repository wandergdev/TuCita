using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;
using TuCita.Domain.Services;

namespace TuCita.Application.Professionals.GetAvailability;

/// <summary>
/// Calcula los horarios disponibles de un profesional para una fecha.
/// </summary>
public sealed class GetAvailabilityQueryHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IScheduleExceptionRepository _scheduleExceptionRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetAvailabilityQueryHandler(
        IProfessionalRepository professionalRepository,
        IAppointmentRepository appointmentRepository,
        IScheduleExceptionRepository scheduleExceptionRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _scheduleExceptionRepository = scheduleExceptionRepository
            ?? throw new ArgumentNullException(nameof(scheduleExceptionRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<GetAvailabilityResponse> HandleAsync(
        GetAvailabilityQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var professional = await _professionalRepository.GetBySlugAsync(query.Slug, cancellationToken);

        if (professional is null || !professional.IsActive)
            throw new NotFoundException("El profesional solicitado no existe.");

        var existingAppointments = await _appointmentRepository.GetByProfessionalAndDateAsync(
            professional.Id,
            query.Date,
            cancellationToken);

        var exception = await _scheduleExceptionRepository.GetForDateAsync(
            professional.Id,
            query.Date,
            cancellationToken);

        var availableSlots = AvailabilitySlotCalculator.CalculateAvailableSlots(
            professional.WorkingSchedule,
            professional.AppointmentDuration,
            query.Date,
            existingAppointments,
            _dateTimeProvider.Now,
            exception);

        var isWorkingDay = AvailabilitySlotCalculator.GetEffectiveWorkingHours(
            professional.WorkingSchedule,
            query.Date,
            exception) is not null;

        return new GetAvailabilityResponse(availableSlots, isWorkingDay, exception?.Message);
    }
}
