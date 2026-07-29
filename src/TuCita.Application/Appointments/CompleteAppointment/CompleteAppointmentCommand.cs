using TuCita.Domain.Identifiers;

namespace TuCita.Application.Appointments.CompleteAppointment;

/// <summary>
/// Solicita marcar una cita como completada.
/// </summary>
public sealed record CompleteAppointmentCommand(
    AppointmentId AppointmentId,
    ProfessionalId RequestingProfessionalId);
