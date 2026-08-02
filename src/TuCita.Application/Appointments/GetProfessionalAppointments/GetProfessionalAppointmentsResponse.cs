namespace TuCita.Application.Appointments.GetProfessionalAppointments;

/// <summary>
/// Página de citas de un profesional para mostrar en su panel.
/// </summary>
public sealed record GetProfessionalAppointmentsResponse(
    IReadOnlyCollection<AppointmentSummary> Appointments,
    bool HasMore);
