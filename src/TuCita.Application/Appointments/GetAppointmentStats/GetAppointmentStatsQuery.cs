using TuCita.Domain.Identifiers;

namespace TuCita.Application.Appointments.GetAppointmentStats;

/// <summary>
/// Solicita las métricas de un profesional autenticado para un rango
/// de tiempo relativo a hoy.
/// </summary>
public sealed record GetAppointmentStatsQuery(
    ProfessionalId ProfessionalId,
    StatsRange Range);
