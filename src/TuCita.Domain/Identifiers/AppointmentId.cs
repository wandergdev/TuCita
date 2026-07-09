namespace TuCita.Domain.Identifiers;

/// <summary>
/// Identificador único de una cita.
/// </summary>
public sealed record AppointmentId
{
    public string Value { get; }

    public AppointmentId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El identificador no puede estar vacío.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}