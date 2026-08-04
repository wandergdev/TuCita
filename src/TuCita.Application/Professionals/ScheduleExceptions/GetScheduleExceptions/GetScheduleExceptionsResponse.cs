namespace TuCita.Application.Professionals.ScheduleExceptions.GetScheduleExceptions;

public sealed record GetScheduleExceptionsResponse(
    IReadOnlyCollection<ScheduleExceptionSummary> Exceptions);
