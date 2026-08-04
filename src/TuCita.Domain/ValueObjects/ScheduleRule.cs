namespace TuCita.Domain.ValueObjects;

public sealed record ScheduleRule
{
    public WorkDays WorkDays { get; }

    public TimeRange WorkingHours { get; }

    /// <summary>
    /// Rango de almuerzo opcional dentro de <see cref="WorkingHours"/>.
    /// </summary>
    public TimeRange? LunchBreak { get; }

    public ScheduleRule(
        WorkDays workDays,
        TimeRange workingHours,
        TimeRange? lunchBreak = null)
    {
        WorkDays = workDays ??
                   throw new ArgumentNullException(nameof(workDays));

        WorkingHours = workingHours ??
                       throw new ArgumentNullException(nameof(workingHours));

        if (lunchBreak is not null
            && (lunchBreak.StartTime < workingHours.StartTime || lunchBreak.EndTime > workingHours.EndTime))
            throw new ArgumentException(
                "El horario de almuerzo debe estar dentro de la jornada laboral.",
                nameof(lunchBreak));

        LunchBreak = lunchBreak;
    }
}