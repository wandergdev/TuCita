namespace TuCita.Domain.Identifiers;

/// <summary>
/// Identificador único de una foto de portafolio.
/// </summary>
public sealed record PortfolioPhotoId
{
    public string Value { get; }

    public PortfolioPhotoId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El identificador no puede estar vacío.", nameof(value));

        Value = value.Trim();
    }

    public override string ToString() => Value;
}
