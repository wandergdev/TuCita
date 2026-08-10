namespace TuCita.Domain.Identifiers;

/// <summary>
/// Identificador único de una foto de negocio (portada o del espacio).
/// </summary>
public sealed record BusinessPhotoId
{
    public string Value { get; }

    public BusinessPhotoId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El identificador no puede estar vacío.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
