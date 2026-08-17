using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.GetMyRoster;

/// <summary>
/// Lista los profesionales asociados al negocio autenticado.
/// </summary>
public sealed class GetMyRosterQueryHandler
{
    private readonly IProfessionalRepository _professionalRepository;

    public GetMyRosterQueryHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
    }

    public async Task<GetMyRosterResponse> HandleAsync(
        GetMyRosterQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var professionals = await _professionalRepository.GetByBusinessIdAsync(
            query.BusinessId,
            cancellationToken);

        var summaries = professionals
            .Select(p => new RosterProfessionalSummary(p.Id, p.FullName, p.Slug.Value, p.Category))
            .ToList();

        return new GetMyRosterResponse(summaries);
    }
}
