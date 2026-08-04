using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Appointments.CheckExistingAppointment;

/// <summary>
/// Verifica si un cliente ya tiene una cita programada ese día, para
/// advertirle antes de que reserve una duplicada por accidente.
/// </summary>
public sealed class CheckExistingAppointmentQueryHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IAppointmentRepository _appointmentRepository;

    public CheckExistingAppointmentQueryHandler(
        IProfessionalRepository professionalRepository,
        IAppointmentRepository appointmentRepository)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
    }

    public async Task<CheckExistingAppointmentResponse> HandleAsync(
        CheckExistingAppointmentQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var professional = await _professionalRepository.GetBySlugAsync(query.Slug, cancellationToken);

        if (professional is null || !professional.IsActive)
            throw new NotFoundException("El profesional solicitado no existe.");

        var hasExisting = await _appointmentRepository.HasScheduledAppointmentAsync(
            professional.Id,
            PhoneNumber.Create(query.CustomerPhone),
            query.Date,
            cancellationToken);

        return new CheckExistingAppointmentResponse(hasExisting);
    }
}
