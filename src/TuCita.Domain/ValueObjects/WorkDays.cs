namespace TuCita.Domain.ValueObjects;

public sealed class WorkDays : IEquatable<WorkDays>
{
    private readonly HashSet<DayOfWeek> _daysOfWeek;
    public IReadOnlyCollection<DayOfWeek> Days => _daysOfWeek;
    
    public WorkDays(IEnumerable<DayOfWeek> daysOfWeek)
    {
        ArgumentNullException.ThrowIfNull(daysOfWeek);
        
        _daysOfWeek = new HashSet<DayOfWeek>(daysOfWeek);

        if (_daysOfWeek.Count == 0)
            throw new ArgumentException("Debe tener al menos un día laboral");
    }
    
    public bool Contains(DayOfWeek dayOfWeek) => _daysOfWeek.Contains(dayOfWeek);

    public bool Equals(WorkDays? other)
    {
        if (other is null)
            return false;

        return _daysOfWeek.SetEquals(other._daysOfWeek);
    }

    public override int GetHashCode()
    {
        HashCode hashCode = new();

        foreach (var dayOfWeek in _daysOfWeek.OrderBy(d => d))
        {
            hashCode.Add(dayOfWeek);
        }

        return hashCode.ToHashCode();
    }
    
    
}