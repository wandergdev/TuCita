namespace TuCita.Application.Appointments.GetAppointmentStats;

/// <summary>
/// Métricas de un profesional para mostrar en el panel de métricas.
/// </summary>
public sealed record GetAppointmentStatsResponse(
    int CompletedCount,
    int NoShowCount,
    int CancelledCount,
    int DistinctCustomersCount,
    DateOnly From,
    DateOnly To);
