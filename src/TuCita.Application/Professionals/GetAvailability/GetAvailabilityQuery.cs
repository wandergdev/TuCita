namespace TuCita.Application.Professionals.GetAvailability;

/// <summary>
/// Solicita los horarios disponibles de un profesional para una fecha.
/// </summary>
public sealed record GetAvailabilityQuery(
    string Slug,
    DateOnly Date);
