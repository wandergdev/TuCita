using TuCita.Domain.ValueObjects;

namespace TuCita.Application.UnitTests.TestData;

public static class WorkingScheduleTestData
{
    public static WorkingSchedule Create() => new(
    [
        new ScheduleRule(
            new WorkDays(
            [
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday
            ]),
            TimeRange.Create(new TimeOnly(8, 0), new TimeOnly(18, 0)))
    ]);
}
