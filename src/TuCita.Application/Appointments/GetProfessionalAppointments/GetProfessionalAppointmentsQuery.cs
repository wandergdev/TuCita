using TuCita.Domain.Identifiers;

namespace TuCita.Application.Appointments.GetProfessionalAppointments;

/// <summary>
/// Solicita las citas de un profesional autenticado, opcionalmente
/// filtradas por fecha.
/// </summary>
public sealed record GetProfessionalAppointmentsQuery(
    ProfessionalId ProfessionalId,
    DateOnly? Date);
