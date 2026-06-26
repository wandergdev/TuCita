namespace TuCita.Domain.ValueObjects;

public sealed record Slug
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Slug no puede estar vacio");

        value = value.Trim().ToLower();

        if (value.Contains(" "))
            throw new ArgumentException(("Slug no puede contener espacios"));
        
        return new Slug(value);
    }
}