using TuCita.Domain.Identifiers;

namespace TuCita.Application.Appointments.CancelAppointment;

/// <summary>
/// Solicita la cancelación de una cita por parte del profesional dueño.
/// </summary>
public sealed record CancelAppointmentCommand(
    AppointmentId AppointmentId,
    ProfessionalId RequestingProfessionalId);
