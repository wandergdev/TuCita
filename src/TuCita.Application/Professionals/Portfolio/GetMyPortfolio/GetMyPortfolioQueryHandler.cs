using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.Portfolio.GetMyPortfolio;

/// <summary>
/// Devuelve todas las fotos del profesional (cualquier estado) para que
/// pueda ver qué está pendiente, aprobado o rechazado.
/// </summary>
public sealed class GetMyPortfolioQueryHandler
{
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;

    public GetMyPortfolioQueryHandler(IPortfolioPhotoRepository portfolioPhotoRepository)
    {
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
    }

    public async Task<GetMyPortfolioResponse> HandleAsync(
        GetMyPortfolioQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var photos = await _portfolioPhotoRepository.GetByProfessionalIdAsync(
            query.ProfessionalId,
            cancellationToken);

        var summaries = photos
            .Select(p => new PortfolioPhotoSummary(p.Id, p.ImageUrl, p.Caption, p.Status, p.CreatedAt))
            .ToList();

        return new GetMyPortfolioResponse(summaries);
    }
}
