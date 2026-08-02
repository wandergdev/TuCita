namespace TuCita.Domain.Repositories;

/// <summary>
/// Conteos agregados de citas de un profesional para un rango de fechas.
/// </summary>
public sealed record AppointmentStatsResult(
    int CompletedCount,
    int NoShowCount,
    int CancelledCount,
    int DistinctCustomersCompleted);
