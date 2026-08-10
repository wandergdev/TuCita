using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.BusinessPhotos.ReplaceBusinessPhotoImage;

/// <summary>
/// Reemplaza la imagen de una foto de negocio ya subida (ej. el admin la
/// descargó, la mejoró y la volvió a subir) y borra el asset viejo en
/// Cloudinary.
/// </summary>
public sealed class ReplaceBusinessPhotoImageCommandHandler
{
    private readonly IBusinessPhotoRepository _businessPhotoRepository;
    private readonly IObjectStorageService _objectStorageService;

    public ReplaceBusinessPhotoImageCommandHandler(
        IBusinessPhotoRepository businessPhotoRepository,
        IObjectStorageService objectStorageService)
    {
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
        _objectStorageService = objectStorageService
            ?? throw new ArgumentNullException(nameof(objectStorageService));
    }

    public async Task HandleAsync(
        ReplaceBusinessPhotoImageCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var photo = await _businessPhotoRepository.GetByIdAsync(command.BusinessPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        var oldPublicId = photo.PublicId;

        photo.ReplaceImage(command.ImageUrl, command.PublicId);
        await _businessPhotoRepository.UpdateAsync(photo, cancellationToken);

        await _objectStorageService.DeleteAsync(oldPublicId, cancellationToken);
    }
}
