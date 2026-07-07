namespace TuCita.Domain.ValueObjects;

public sealed class WorkingSchedule
{
    public IReadOnlyCollection<ScheduleRule> Rules { get; }

    public WorkingSchedule(IEnumerable<ScheduleRule> rules)
    {
        ArgumentNullException.ThrowIfNull(rules);

        var scheduleRules = rules.ToList();

        if (!scheduleRules.Any())
            throw new ArgumentException("El horario debe contener al menos una regla.",
                nameof(rules));

        ValidateDuplicateDays(scheduleRules);

        Rules = scheduleRules.AsReadOnly();
    }

    public bool WorksOn(DayOfWeek dayOfWeek)
    {
        return Rules.Any(rule => rule.WorkDays.Contains(dayOfWeek));
    }

    public TimeRange? GetWorkingHours(DayOfWeek dayOfWeek)
    {
        return Rules.FirstOrDefault(rule => rule.WorkDays.Contains(dayOfWeek))
            ?.WorkingHours;
    }

    private static void ValidateDuplicateDays(IEnumerable<ScheduleRule> rules)
    {
        var usedDays = new HashSet<DayOfWeek>();

        foreach (var rule in rules)
        {
            foreach (var dayOfWeek in rule.WorkDays.Days)
            {
                if (!usedDays.Add(dayOfWeek))
                {
                    throw new ArgumentException($"El día '{dayOfWeek}' está repetido en más de una regla.");
                }
            }
        }
    }
}