using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Enums;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.GetBusinessPublicProfile;

/// <summary>
/// Obtiene el perfil público de un negocio (y su lista de profesionales)
/// a partir de su slug.
/// </summary>
public sealed class GetBusinessPublicProfileQueryHandler
{
    private readonly IBusinessRepository _businessRepository;
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;
    private readonly IBusinessPhotoRepository _businessPhotoRepository;

    public GetBusinessPublicProfileQueryHandler(
        IBusinessRepository businessRepository,
        IProfessionalRepository professionalRepository,
        IPortfolioPhotoRepository portfolioPhotoRepository,
        IBusinessPhotoRepository businessPhotoRepository)
    {
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
    }

    public async Task<GetBusinessPublicProfileResponse> HandleAsync(
        GetBusinessPublicProfileQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var business = await _businessRepository.GetBySlugAsync(query.Slug, cancellationToken);

        if (business is null)
            throw new NotFoundException("El negocio solicitado no existe.");

        var professionals = await _professionalRepository.GetByBusinessIdAsync(business.Id, cancellationToken);

        var summaries = new List<BusinessProfessionalSummary>();
        foreach (var professional in professionals)
        {
            var approvedPhotos = await _portfolioPhotoRepository.GetApprovedByProfessionalIdAsync(
                professional.Id,
                cancellationToken);

            var coverPhotoUrls = approvedPhotos
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => p.ImageUrl)
                .ToList();

            summaries.Add(new BusinessProfessionalSummary(
                professional.FullName,
                professional.Slug.Value,
                professional.Category,
                coverPhotoUrls));
        }

        var coverPhotos = await _businessPhotoRepository.GetApprovedByBusinessIdAsync(
            business.Id,
            BusinessPhotoKind.Cover,
            cancellationToken);

        var spacePhotos = await _businessPhotoRepository.GetApprovedByBusinessIdAsync(
            business.Id,
            BusinessPhotoKind.Space,
            cancellationToken);

        return new GetBusinessPublicProfileResponse(
            business.Name,
            business.Slug.Value,
            coverPhotos.FirstOrDefault()?.ImageUrl,
            spacePhotos.Select(p => p.ImageUrl).ToList(),
            summaries);
    }
}
