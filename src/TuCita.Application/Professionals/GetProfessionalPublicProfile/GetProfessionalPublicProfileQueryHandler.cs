using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.GetProfessionalPublicProfile;

/// <summary>
/// Obtiene el perfil público de un profesional a partir de su slug.
/// </summary>
public sealed class GetProfessionalPublicProfileQueryHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;

    public GetProfessionalPublicProfileQueryHandler(
        IProfessionalRepository professionalRepository,
        IPortfolioPhotoRepository portfolioPhotoRepository)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
    }

    public async Task<GetProfessionalPublicProfileResponse> HandleAsync(
        GetProfessionalPublicProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var professional = await _professionalRepository.GetBySlugAsync(query.Slug, cancellationToken);

        if (professional is null || !professional.IsActive)
            throw new NotFoundException("El profesional solicitado no existe.");

        var approvedPhotos = await _portfolioPhotoRepository.GetApprovedByProfessionalIdAsync(
            professional.Id,
            cancellationToken);

        var photos = approvedPhotos
            .Select(p => new PortfolioPhotoPublicSummary(p.ImageUrl, p.Caption))
            .ToList();

        return new GetProfessionalPublicProfileResponse(
            professional.FullName,
            professional.Slug.Value,
            professional.Category,
            professional.AppointmentDuration.Minutes,
            photos);
    }
}
