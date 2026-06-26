namespace TuCita.Domain.ValueObjects;

public sealed record AppointmentDuration
{
    public int Minutes { get; }

    private static readonly int[] AllowedMinutes = [30, 45, 60, 90];
    
    private AppointmentDuration(int minutes)
    {
        Minutes = minutes;
    }

    public static AppointmentDuration Create(int minutes)
    {
        if (!AllowedMinutes.Contains(minutes))
            throw new ArgumentException("Duración inválida");

        return new AppointmentDuration(minutes);
    }
}