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

    public GetPendingPortfolioPhotosQueryHandler(
        IPortfolioPhotoRepository portfolioPhotoRepository,
        IProfessionalRepository professionalRepository)
    {
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
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

            summaries.Add(new PendingPortfolioPhotoSummary(
                photo.Id,
                photo.ImageUrl,
                photo.Caption,
                photo.CreatedAt,
                photo.ProfessionalId,
                professional?.FullName ?? "(profesional eliminado)",
                professional?.Slug.Value ?? ""));
        }

        return new GetPendingPortfolioPhotosResponse(summaries);
    }
}
