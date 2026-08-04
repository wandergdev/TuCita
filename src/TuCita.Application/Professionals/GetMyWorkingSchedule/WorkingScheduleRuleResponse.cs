namespace TuCita.Application.Professionals.GetMyWorkingSchedule;

/// <summary>
/// Una regla del horario habitual: los días que cubre y su rango de hora.
/// </summary>
public sealed record WorkingScheduleRuleResponse(
    IReadOnlyCollection<DayOfWeek> Days,
    TimeOnly StartTime,
    TimeOnly EndTime,
    TimeOnly? LunchBreakStartTime,
    TimeOnly? LunchBreakEndTime);
