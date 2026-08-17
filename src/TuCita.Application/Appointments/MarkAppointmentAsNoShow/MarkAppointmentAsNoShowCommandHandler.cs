using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Appointments.MarkAppointmentAsNoShow;

/// <summary>
/// Marca una cita como ausencia del cliente.
/// </summary>
public sealed class MarkAppointmentAsNoShowCommandHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public MarkAppointmentAsNoShowCommandHandler(
        IAppointmentRepository appointmentRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task HandleAsync(
        MarkAppointmentAsNoShowCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var appointment = await _appointmentRepository.GetByIdAsync(command.AppointmentId, cancellationToken);

        if (appointment is null)
            throw new NotFoundException("La cita solicitada no existe.");

        if (appointment.ProfessionalId != command.RequestingProfessionalId)
            throw new ForbiddenException("La cita no pertenece al profesional autenticado.");

        appointment.MarkAsNoShow(_dateTimeProvider.Now);

        await _appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
