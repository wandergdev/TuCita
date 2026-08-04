using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.ScheduleExceptions.GetScheduleExceptions;

public sealed record ScheduleExceptionSummary(
    ScheduleExceptionId Id,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    TimeOnly? LunchBreakStartTime,
    TimeOnly? LunchBreakEndTime,
    string? Message);
