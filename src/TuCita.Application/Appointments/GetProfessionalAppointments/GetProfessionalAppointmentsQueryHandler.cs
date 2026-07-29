using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Appointments.GetProfessionalAppointments;

/// <summary>
/// Obtiene las citas del profesional autenticado para su panel.
/// </summary>
public sealed class GetProfessionalAppointmentsQueryHandler
{
    private readonly IAppointmentRepository _appointmentRepository;

    public GetProfessionalAppointmentsQueryHandler(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
    }

    public async Task<GetProfessionalAppointmentsResponse> HandleAsync(
        GetProfessionalAppointmentsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var appointments = query.Date is { } date
            ? await _appointmentRepository.GetByProfessionalAndDateAsync(query.ProfessionalId, date, cancellationToken)
            : await _appointmentRepository.GetByProfessionalAsync(query.ProfessionalId, cancellationToken);

        return new GetProfessionalAppointmentsResponse(appointments.Select(ToSummary).ToList());
    }

    private static AppointmentSummary ToSummary(Appointment appointment) => new(
        appointment.Id,
        appointment.CustomerName,
        appointment.CustomerPhone.Value,
        appointment.Date,
        appointment.StartTime,
        appointment.EndTime,
        appointment.Status);
}
