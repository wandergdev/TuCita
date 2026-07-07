namespace TuCita.Domain.ValueObjects;

public sealed record ScheduleRule
{
    public WorkDays WorkDays { get; }
    
    public TimeRange WorkingHours { get; }

    public ScheduleRule(
        WorkDays workDays,
        TimeRange workingHours)
    {
        WorkDays = workDays ?? 
                   throw new ArgumentNullException(nameof(workDays));
        
        WorkingHours = workingHours ?? 
                       throw new ArgumentNullException(nameof(workingHours));
        
    }
}