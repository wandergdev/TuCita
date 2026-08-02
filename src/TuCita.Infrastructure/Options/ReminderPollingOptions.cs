namespace TuCita.Infrastructure.Options;

/// <summary>
/// Cadencia técnica con la que se revisan citas pendientes de
/// recordatorio. La política de negocio (cuántos minutos antes avisar)
/// vive en <see cref="TuCita.Application.Options.ReminderOptions"/>.
/// </summary>
public sealed class ReminderPollingOptions
{
    public const string SectionName = "Reminders";

    public int PollIntervalSeconds { get; init; } = 60;
}
