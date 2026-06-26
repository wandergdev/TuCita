namespace TuCita.Domain.ValueObjects;

public sealed record PhoneNumber
{
    public string Value { get; }

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public static PhoneNumber Create(string value)
    {
        if (string.IsNullOrWhiteSpace((value)))
            throw new ArgumentException("Teléfono vacío");

        value = value.Replace(" ", "").Replace("-", "");

        if (value.Length < 10)
            throw new ArgumentException("Teléfono inválido");
        
        return new PhoneNumber(value);

    }
}