namespace TuCita.Application.Appointments.BookAppointment;

/// <summary>
/// Contiene la información necesaria para que un cliente reserve
/// una cita con un profesional, sin necesidad de crear una cuenta.
/// </summary>
public sealed record BookAppointmentCommand(
    string Slug,
    string CustomerName,
    string CustomerPhone,
    DateOnly Date,
    TimeOnly StartTime);
