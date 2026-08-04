namespace TuCita.Api.Contracts.Professionals;

/// <summary>
/// StartTime/EndTime ambos ausentes = día(s) no laborable(s); ambos
/// presentes = horario especial para ese rango.
/// </summary>
public sealed record CreateScheduleExceptionRequest(
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    TimeOnly? LunchBreakStartTime,
    TimeOnly? LunchBreakEndTime,
    string? Message);
