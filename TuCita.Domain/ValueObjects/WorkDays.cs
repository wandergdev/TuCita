namespace TuCita.Domain.ValueObjects;

public sealed record WorkDays
{
    private readonly HashSet<DayOfWeek> _daysOfWeek;

    public WorkDays(IEnumerable<DayOfWeek> daysOfWeek)
    {
        _daysOfWeek = new HashSet<DayOfWeek>(daysOfWeek);

        if (_daysOfWeek.Count == 0)
            throw new ArgumentException("Debe tener al menos un día laboral");
    }
    
    public bool Contains(DayOfWeek dayOfWeek) => _daysOfWeek.Contains(dayOfWeek);
    
    public IReadOnlyCollection<DayOfWeek> GetAll() => _daysOfWeek.ToList();
}