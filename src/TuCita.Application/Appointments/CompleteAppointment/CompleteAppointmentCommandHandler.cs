using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Appointments.CompleteAppointment;

/// <summary>
/// Marca una cita como completada.
/// </summary>
public sealed class CompleteAppointmentCommandHandler
{
    private readonly IAppointmentRepository _appointmentRepository;

    public CompleteAppointmentCommandHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
    }

    public async Task HandleAsync(
        CompleteAppointmentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var appointment = await _appointmentRepository.GetByIdAsync(command.AppointmentId, cancellationToken);

        if (appointment is null)
            throw new NotFoundException("La cita solicitada no existe.");

        if (appointment.ProfessionalId != command.RequestingProfessionalId)
            throw new ForbiddenException("La cita no pertenece al profesional autenticado.");

        appointment.Complete();

        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
