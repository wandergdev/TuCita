using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Application.Appointments.GetProfessionalAppointments;

/// <summary>
/// Representación de una cita para el panel del profesional.
/// </summary>
public sealed record AppointmentSummary(
    AppointmentId Id,
    string CustomerName,
    string CustomerPhone,
    DateOnly Date,
    TimeOnly StartTime,
    TimeOnly EndTime,
    AppointmentStatus Status);
