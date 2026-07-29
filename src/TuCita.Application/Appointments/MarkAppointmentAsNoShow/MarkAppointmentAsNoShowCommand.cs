using TuCita.Domain.Identifiers;

namespace TuCita.Application.Appointments.MarkAppointmentAsNoShow;

/// <summary>
/// Solicita marcar una cita como ausencia del cliente (No Show).
/// </summary>
public sealed record MarkAppointmentAsNoShowCommand(
    AppointmentId AppointmentId,
    ProfessionalId RequestingProfessionalId);
