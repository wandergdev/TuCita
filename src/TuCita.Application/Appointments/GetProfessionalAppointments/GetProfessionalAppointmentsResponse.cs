namespace TuCita.Application.Appointments.GetProfessionalAppointments;

/// <summary>
/// Citas de un profesional para mostrar en su panel.
/// </summary>
public sealed record GetProfessionalAppointmentsResponse(
    IReadOnlyCollection<AppointmentSummary> Appointments);
