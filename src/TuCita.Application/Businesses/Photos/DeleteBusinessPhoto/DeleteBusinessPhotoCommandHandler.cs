using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.Photos.DeleteBusinessPhoto;

/// <summary>
/// Elimina una foto de negocio, tanto en el almacenamiento como en el
/// registro local.
/// </summary>
public sealed class DeleteBusinessPhotoCommandHandler
{
    private readonly IBusinessPhotoRepository _businessPhotoRepository;
    private readonly IObjectStorageService _objectStorageService;

    public DeleteBusinessPhotoCommandHandler(
        IBusinessPhotoRepository businessPhotoRepository,
        IObjectStorageService objectStorageService)
    {
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
        _objectStorageService = objectStorageService
            ?? throw new ArgumentNullException(nameof(objectStorageService));
    }

    public async Task HandleAsync(
        DeleteBusinessPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var photo = await _businessPhotoRepository.GetByIdAsync(command.BusinessPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        if (photo.BusinessId != command.RequestingBusinessId)
            throw new ForbiddenException("La foto no pertenece al negocio autenticado.");

        await _objectStorageService.DeleteAsync(photo.PublicId, cancellationToken);
        await _businessPhotoRepository.DeleteAsync(photo, cancellationToken);
    }
}
