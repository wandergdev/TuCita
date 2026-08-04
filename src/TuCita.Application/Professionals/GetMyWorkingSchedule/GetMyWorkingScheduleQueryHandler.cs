using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.GetMyWorkingSchedule;

/// <summary>
/// Obtiene el horario habitual del profesional autenticado.
/// </summary>
public sealed class GetMyWorkingScheduleQueryHandler
{
    private readonly IProfessionalRepository _professionalRepository;

    public GetMyWorkingScheduleQueryHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
    }

    public async Task<GetMyWorkingScheduleResponse> HandleAsync(
        GetMyWorkingScheduleQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var professional = await _professionalRepository.GetByIdAsync(query.ProfessionalId, cancellationToken);

        if (professional is null)
            throw new NotFoundException("El profesional solicitado no existe.");

        var rules = professional.WorkingSchedule.Rules
            .Select(rule => new WorkingScheduleRuleResponse(
                rule.WorkDays.Days,
                rule.WorkingHours.StartTime,
                rule.WorkingHours.EndTime,
                rule.LunchBreak?.StartTime,
                rule.LunchBreak?.EndTime))
            .ToList();

        return new GetMyWorkingScheduleResponse(rules);
    }
}
