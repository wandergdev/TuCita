using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Application.Appointments.GetProfessionalAppointments;

/// <summary>
/// Solicita una página de citas de un profesional autenticado,
/// opcionalmente filtradas por estado.
/// </summary>
public sealed record GetProfessionalAppointmentsQuery(
    ProfessionalId ProfessionalId,
    AppointmentStatus? Status,
    int Skip,
    int Take);
