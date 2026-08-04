namespace TuCita.Domain.Identifiers;

/// <summary>
/// Identificador único de una excepción de horario.
/// </summary>
public sealed record ScheduleExceptionId
{
    public string Value { get; }

    public ScheduleExceptionId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El identificador no puede estar vacío.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
