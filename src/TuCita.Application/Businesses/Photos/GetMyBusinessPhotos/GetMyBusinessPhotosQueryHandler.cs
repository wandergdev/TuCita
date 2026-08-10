using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.Photos.GetMyBusinessPhotos;

/// <summary>
/// Devuelve todas las fotos del negocio (cualquier estado) para que el
/// dueño vea qué está pendiente, aprobado o rechazado.
/// </summary>
public sealed class GetMyBusinessPhotosQueryHandler
{
    private readonly IBusinessPhotoRepository _businessPhotoRepository;

    public GetMyBusinessPhotosQueryHandler(IBusinessPhotoRepository businessPhotoRepository)
    {
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
    }

    public async Task<GetMyBusinessPhotosResponse> HandleAsync(
        GetMyBusinessPhotosQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var photos = await _businessPhotoRepository.GetByBusinessIdAsync(query.BusinessId, cancellationToken);

        var summaries = photos
            .Select(p => new BusinessPhotoSummary(p.Id, p.Kind, p.ImageUrl, p.Caption, p.Status, p.CreatedAt))
            .ToList();

        return new GetMyBusinessPhotosResponse(summaries);
    }
}
