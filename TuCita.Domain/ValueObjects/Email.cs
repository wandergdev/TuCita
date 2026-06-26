namespace TuCita.Domain.ValueObjects;

public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email vacío");

        if (!value.Contains("@"))
            throw new ArgumentException("Email invalido");
        
        return new Email(value.Trim().ToLower());
    }
}