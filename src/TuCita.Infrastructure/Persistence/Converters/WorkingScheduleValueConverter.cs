using System.Text.Json;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TuCita.Domain.ValueObjects;

namespace TuCita.Infrastructure.Persistence.Converters;

/// <summary>
/// Serializa <see cref="WorkingSchedule"/> como JSON para almacenarlo
/// en una única columna, evitando modelar una tabla relacional para
/// algo que siempre se lee/escribe como una unidad.
/// </summary>
public sealed class WorkingScheduleValueConverter : ValueConverter<WorkingSchedule, string>
{
    public WorkingScheduleValueConverter()
        : base(
            schedule => Serialize(schedule),
            json => Deserialize(json))
    {
    }

    private static string Serialize(WorkingSchedule schedule)
    {
        var rules = schedule.Rules.Select(rule => new ScheduleRuleJson(
            rule.WorkDays.Days.Select(day => (int)day).ToArray(),
            rule.WorkingHours.StartTime,
            rule.WorkingHours.EndTime));

        return JsonSerializer.Serialize(rules);
    }

    private static WorkingSchedule Deserialize(string json)
    {
        var rules = JsonSerializer.Deserialize<List<ScheduleRuleJson>>(json)
            ?? throw new InvalidOperationException("No fue posible deserializar el horario de trabajo.");

        return new WorkingSchedule(rules.Select(rule => new ScheduleRule(
            new WorkDays(rule.Days.Select(day => (DayOfWeek)day)),
            TimeRange.Create(rule.Start, rule.End))));
    }

    private sealed record ScheduleRuleJson(int[] Days, TimeOnly Start, TimeOnly End);
}
