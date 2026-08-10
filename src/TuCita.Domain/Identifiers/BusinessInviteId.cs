namespace TuCita.Domain.Identifiers;

/// <summary>
/// Identificador único de una invitación de negocio.
/// </summary>
public sealed record BusinessInviteId
{
    public string Value { get; }

    public BusinessInviteId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El identificador no puede estar vacío.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
