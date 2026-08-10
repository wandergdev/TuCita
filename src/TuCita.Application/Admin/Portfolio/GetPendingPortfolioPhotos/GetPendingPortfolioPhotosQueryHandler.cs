using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.Portfolio.GetPendingPortfolioPhotos;

/// <summary>
/// Cola de moderación: fotos pendientes de todos los profesionales, con
/// contexto suficiente para revisarlas por curl/Postman.
/// </summary>
public sealed class GetPendingPortfolioPhotosQueryHandler
{
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IBusinessRepository _businessRepository;

    public GetPendingPortfolioPhotosQueryHandler(
        IPortfolioPhotoRepository portfolioPhotoRepository,
        IProfessionalRepository professionalRepository,
        IBusinessRepository businessRepository)
    {
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
    }

    public async Task<GetPendingPortfolioPhotosResponse> HandleAsync(
        GetPendingPortfolioPhotosQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pendingPhotos = await _portfolioPhotoRepository.GetPendingAsync(cancellationToken);

        var summaries = new List<PendingPortfolioPhotoSummary>();
        foreach (var photo in pendingPhotos)
        {
            var professional = await _professionalRepository.GetByIdAsync(photo.ProfessionalId, cancellationToken);

            string? businessName = null;
            string? businessSlug = null;
            if (professional?.BusinessId is { } businessId)
            {
                var business = await _businessRepository.GetByIdAsync(businessId, cancellationToken);
                businessName = business?.Name;
                businessSlug = business?.Slug.Value;
            }

            summaries.Add(new PendingPortfolioPhotoSummary(
                photo.Id,
                photo.ImageUrl,
                photo.Caption,
                photo.CreatedAt,
                photo.ProfessionalId,
                professional?.FullName ?? "(profesional eliminado)",
                professional?.Slug.Value ?? "",
                professional?.Category ?? default,
                businessName,
                businessSlug));
        }

        return new GetPendingPortfolioPhotosResponse(summaries);
    }
}
