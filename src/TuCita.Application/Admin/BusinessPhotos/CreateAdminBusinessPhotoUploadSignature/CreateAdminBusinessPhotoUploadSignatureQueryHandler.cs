using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.BusinessPhotos.CreateAdminBusinessPhotoUploadSignature;

/// <summary>
/// Genera la firma que el panel de admin necesita para subir una versión
/// editada de una foto de negocio pendiente, reutilizando la misma
/// carpeta de Cloudinary que el negocio dueño de la foto.
/// </summary>
public sealed class CreateAdminBusinessPhotoUploadSignatureQueryHandler
{
    private readonly IBusinessPhotoRepository _businessPhotoRepository;
    private readonly IObjectStorageService _objectStorageService;

    public CreateAdminBusinessPhotoUploadSignatureQueryHandler(
        IBusinessPhotoRepository businessPhotoRepository,
        IObjectStorageService objectStorageService)
    {
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
        _objectStorageService = objectStorageService
            ?? throw new ArgumentNullException(nameof(objectStorageService));
    }

    public async Task<UploadSignatureResult> HandleAsync(
        CreateAdminBusinessPhotoUploadSignatureQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var photo = await _businessPhotoRepository.GetByIdAsync(query.BusinessPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        var folder = $"businesses/{photo.BusinessId.Value}";

        return await _objectStorageService.CreateUploadSignatureAsync(folder, cancellationToken);
    }
}
