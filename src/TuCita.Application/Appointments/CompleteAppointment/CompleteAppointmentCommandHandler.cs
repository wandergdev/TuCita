using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Appointments.CompleteAppointment;

/// <summary>
/// Marca una cita como completada.
/// </summary>
public sealed class CompleteAppointmentCommandHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CompleteAppointmentCommandHandler(
        IAppointmentRepository appointmentRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
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

        appointment.Complete(_dateTimeProvider.Now);

        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
