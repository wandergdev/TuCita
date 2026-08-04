using TuCita.Application.Abstractions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.ScheduleExceptions.GetScheduleExceptions;

/// <summary>
/// Lista las excepciones de horario vigentes/futuras de un profesional.
/// </summary>
public sealed class GetScheduleExceptionsQueryHandler
{
    private readonly IScheduleExceptionRepository _scheduleExceptionRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetScheduleExceptionsQueryHandler(
        IScheduleExceptionRepository scheduleExceptionRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _scheduleExceptionRepository = scheduleExceptionRepository
            ?? throw new ArgumentNullException(nameof(scheduleExceptionRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<GetScheduleExceptionsResponse> HandleAsync(
        GetScheduleExceptionsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var today = DateOnly.FromDateTime(_dateTimeProvider.Now);

        var exceptions = await _scheduleExceptionRepository.GetByProfessionalIdAsync(
            query.ProfessionalId,
            today,
            cancellationToken);

        var summaries = exceptions
            .OrderBy(exception => exception.StartDate)
            .Select(exception => new ScheduleExceptionSummary(
                exception.Id,
                exception.StartDate,
                exception.EndDate,
                exception.WorkingHours?.StartTime,
                exception.WorkingHours?.EndTime,
                exception.LunchBreak?.StartTime,
                exception.LunchBreak?.EndTime,
                exception.Message))
            .ToList();

        return new GetScheduleExceptionsResponse(summaries);
    }
}
