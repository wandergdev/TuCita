namespace TuCita.Domain.ValueObjects;

public sealed record TimeRange
{
    public TimeOnly StartTime { get; }
    public TimeOnly EndTime { get; }

    private TimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (startTime > endTime)
            throw new ArgumentException("La fecha de inicio debe ser menor que la fecha de cierre");
        
        StartTime = startTime;
        EndTime = endTime;
    }
    
    public static TimeRange Create(TimeOnly startTime, TimeOnly endTime)
        => new(startTime, endTime);
}