namespace TuCita.Application.Appointments.CheckExistingAppointment;

/// <summary>
/// Pregunta si un teléfono ya tiene una cita programada con un
/// profesional en una fecha dada.
/// </summary>
public sealed record CheckExistingAppointmentQuery(
    string Slug,
    string CustomerPhone,
    DateOnly Date);
