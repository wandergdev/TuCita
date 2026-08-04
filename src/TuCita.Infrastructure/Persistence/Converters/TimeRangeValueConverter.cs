using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TuCita.Domain.ValueObjects;

namespace TuCita.Infrastructure.Persistence.Converters;

/// <summary>
/// Serializa un <see cref="TimeRange"/> opcional como JSON en una sola
/// columna, igual que <see cref="WorkingScheduleValueConverter"/> hace
/// con <see cref="WorkingSchedule"/>.
/// </summary>
public sealed class TimeRangeValueConverter : ValueConverter<TimeRange?, string?>
{
    public TimeRangeValueConverter()
        : base(
            range => Serialize(range),
            json => Deserialize(json))
    {
    }

    private static string? Serialize(TimeRange? range)
        => range is null
            ? null
            : JsonSerializer.Serialize(new TimeRangeJson(range.StartTime, range.EndTime));

    private static TimeRange? Deserialize(string? json)
    {
        if (string.IsNullOrEmpty(json))
            return null;

        var data = JsonSerializer.Deserialize<TimeRangeJson>(json)
            ?? throw new InvalidOperationException("No fue posible deserializar el horario.");

        return TimeRange.Create(data.Start, data.End);
    }

    private sealed record TimeRangeJson(TimeOnly Start, TimeOnly End);
}
