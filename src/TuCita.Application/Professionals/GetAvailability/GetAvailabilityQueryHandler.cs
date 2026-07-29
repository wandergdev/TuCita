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

    public GetAvailabilityQueryHandler(
        IProfessionalRepository professionalRepository,
        IAppointmentRepository appointmentRepository)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
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

        var availableSlots = AvailabilitySlotCalculator.CalculateAvailableSlots(
            professional.WorkingSchedule,
            professional.AppointmentDuration,
            query.Date,
            existingAppointments);

        return new GetAvailabilityResponse(availableSlots);
    }
}
