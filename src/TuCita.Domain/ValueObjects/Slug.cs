using System.Text.RegularExpressions;

namespace TuCita.Domain.ValueObjects;

public sealed partial record Slug
{
    public string Value { get; }

    private Slug(string value)
    {
        Value = value;
    }

    public static Slug Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El slug no puede estar vacío.");

        value = value.Trim().ToLowerInvariant();

        if (!SlugFormat().IsMatch(value))
            throw new ArgumentException(
                "El slug solo puede contener letras sin tildes (a-z), números y guiones, sin espacios " +
                "ni empezar o terminar con guion.");

        return new Slug(value);
    }

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugFormat();
}
