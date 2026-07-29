using TuCita.Domain.Identifiers;

namespace TuCita.Application.Appointments.BookAppointment;

/// <summary>
/// Resultado de la reserva de una cita.
/// </summary>
public sealed record BookAppointmentResponse(AppointmentId AppointmentId);
