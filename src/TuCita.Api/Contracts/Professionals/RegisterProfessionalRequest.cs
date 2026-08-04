using TuCita.Domain.Enums;

namespace TuCita.Api.Contracts.Professionals;

public sealed record RegisterProfessionalRequest(
    string FullName,
    string Email,
    string PhoneNumber,
    string Slug,
    Category Category,
    int AppointmentDurationMinutes,
    List<WorkingScheduleRuleRequest> WorkingSchedule,
    string Password);

public sealed record WorkingScheduleRuleRequest(
    List<DayOfWeek> Days,
    TimeOnly StartTime,
    TimeOnly EndTime,
    TimeOnly? LunchBreakStartTime = null,
    TimeOnly? LunchBreakEndTime = null);
