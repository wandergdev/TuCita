using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.GetProfessionalPublicProfile;

/// <summary>
/// Obtiene el perfil público de un profesional a partir de su slug.
/// </summary>
public sealed class GetProfessionalPublicProfileQueryHandler
{
    private readonly IProfessionalRepository _professionalRepository;

    public GetProfessionalPublicProfileQueryHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
    }

    public async Task<GetProfessionalPublicProfileResponse> HandleAsync(
        GetProfessionalPublicProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var professional = await _professionalRepository.GetBySlugAsync(query.Slug, cancellationToken);

        if (professional is null || !professional.IsActive)
            throw new NotFoundException("El profesional solicitado no existe.");

        return new GetProfessionalPublicProfileResponse(
            professional.FullName,
            professional.Slug.Value,
            professional.Category,
            professional.AppointmentDuration.Minutes);
    }
}
