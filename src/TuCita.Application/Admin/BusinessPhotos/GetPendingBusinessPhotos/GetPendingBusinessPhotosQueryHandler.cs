using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.BusinessPhotos.GetPendingBusinessPhotos;

/// <summary>
/// Cola de moderación: fotos de negocio pendientes (portada o espacio) de
/// todos los negocios, con contexto suficiente para revisarlas.
/// </summary>
public sealed class GetPendingBusinessPhotosQueryHandler
{
    private readonly IBusinessPhotoRepository _businessPhotoRepository;
    private readonly IBusinessRepository _businessRepository;

    public GetPendingBusinessPhotosQueryHandler(
        IBusinessPhotoRepository businessPhotoRepository,
        IBusinessRepository businessRepository)
    {
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
    }

    public async Task<GetPendingBusinessPhotosResponse> HandleAsync(
        GetPendingBusinessPhotosQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var pendingPhotos = await _businessPhotoRepository.GetPendingAsync(cancellationToken);

        var summaries = new List<PendingBusinessPhotoSummary>();
        foreach (var photo in pendingPhotos)
        {
            var business = await _businessRepository.GetByIdAsync(photo.BusinessId, cancellationToken);

            summaries.Add(new PendingBusinessPhotoSummary(
                photo.Id,
                photo.Kind,
                photo.ImageUrl,
                photo.Caption,
                photo.CreatedAt,
                photo.BusinessId,
                business?.Name ?? "(negocio eliminado)",
                business?.Slug.Value ?? ""));
        }

        return new GetPendingBusinessPhotosResponse(summaries);
    }
}
