namespace TuCita.Application.Options;

/// <summary>
/// Política de negocio de los recordatorios de cita (no confundir con
/// la cadencia técnica del proceso que los revisa, que vive en
/// Infrastructure).
/// </summary>
public sealed class ReminderOptions
{
    public const string SectionName = "Reminders";

    public int MinutesBefore { get; init; } = 20;
}
