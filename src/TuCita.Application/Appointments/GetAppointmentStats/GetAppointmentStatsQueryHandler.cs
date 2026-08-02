using TuCita.Application.Abstractions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Appointments.GetAppointmentStats;

/// <summary>
/// Calcula las métricas del profesional autenticado (clientes atendidos,
/// no-shows y cancelaciones) para el rango de tiempo pedido.
/// </summary>
public sealed class GetAppointmentStatsQueryHandler
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetAppointmentStatsQueryHandler(
        IAppointmentRepository appointmentRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _appointmentRepository = appointmentRepository
            ?? throw new ArgumentNullException(nameof(appointmentRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<GetAppointmentStatsResponse> HandleAsync(
        GetAppointmentStatsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var today = DateOnly.FromDateTime(_dateTimeProvider.Now);
        var (from, to) = GetRangeBounds(query.Range, today);

        var stats = await _appointmentRepository.GetStatsByProfessionalAsync(
            query.ProfessionalId, from, to, cancellationToken);

        return new GetAppointmentStatsResponse(
            stats.CompletedCount,
            stats.NoShowCount,
            stats.CancelledCount,
            stats.DistinctCustomersCompleted,
            from,
            to);
    }

    private static (DateOnly From, DateOnly To) GetRangeBounds(StatsRange range, DateOnly today) => range switch
    {
        StatsRange.Day => (today, today),
        StatsRange.Week => GetWeekBounds(today),
        StatsRange.Month => (
            new DateOnly(today.Year, today.Month, 1),
            new DateOnly(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month))),
        _ => (today, today)
    };

    private static (DateOnly From, DateOnly To) GetWeekBounds(DateOnly today)
    {
        var daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
        var monday = today.AddDays(-daysSinceMonday);
        return (monday, monday.AddDays(6));
    }
}
