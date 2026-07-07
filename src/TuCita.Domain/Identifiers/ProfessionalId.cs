namespace TuCita.Domain.Identifiers;

public sealed record ProfessionalId
{
    public string Value { get; }

    public ProfessionalId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("El identificador no puede estar vacio.", nameof(value));
        
        Value = value;
    }
    
    public override string ToString() => Value;
}