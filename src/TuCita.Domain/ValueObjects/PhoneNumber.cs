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
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Teléfono vacío");

        var digitsOnly = new string(value.Where(char.IsDigit).ToArray());

        if (digitsOnly.Length != 10)
            throw new ArgumentException("El teléfono debe tener 10 dígitos.");

        return new PhoneNumber(digitsOnly);
    }
}