namespace TuCita.Domain.Identifiers;

/// <summary>
/// Identificador único de un negocio.
/// </summary>
public sealed record BusinessId
{
    public string Value { get; }

    public BusinessId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El identificador no puede estar vacío.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
