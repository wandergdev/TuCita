namespace TuCita.Api.Contracts.Professionals;

public sealed record UpdateWorkingScheduleRequest(
    List<WorkingScheduleRuleRequest> WorkingSchedule);
