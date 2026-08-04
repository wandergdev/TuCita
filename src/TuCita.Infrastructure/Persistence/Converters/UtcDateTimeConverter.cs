using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TuCita.Infrastructure.Persistence.Converters;

/// <summary>
/// Etiqueta un <see cref="DateTime"/> como Kind=Utc sin alterar su valor.
/// Npgsql rechaza escribir un DateTime con Kind=Local o Unspecified (como
/// el que produce <c>DateTime.Now</c>) en una columna "timestamp with time
/// zone".
/// </summary>
public sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            value => DateTime.SpecifyKind(value, DateTimeKind.Utc))
    {
    }
}

public sealed class UtcNullableDateTimeConverter : ValueConverter<DateTime?, DateTime?>
{
    public UtcNullableDateTimeConverter()
        : base(
            value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value,
            value => value.HasValue ? DateTime.SpecifyKind(value.Value, DateTimeKind.Utc) : value)
    {
    }
}
