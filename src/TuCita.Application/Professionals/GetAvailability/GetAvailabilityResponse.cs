namespace TuCita.Application.Professionals.GetAvailability;

/// <summary>
/// Horarios disponibles de un profesional para la fecha solicitada.
/// </summary>
public sealed record GetAvailabilityResponse(
    IReadOnlyCollection<TimeOnly> AvailableSlots,
    bool IsWorkingDay,
    string? Message);
