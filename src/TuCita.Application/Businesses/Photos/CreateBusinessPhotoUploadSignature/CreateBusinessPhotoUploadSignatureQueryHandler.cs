using TuCita.Application.Abstractions;

namespace TuCita.Application.Businesses.Photos.CreateBusinessPhotoUploadSignature;

/// <summary>
/// Genera la firma que el navegador necesita para subir una foto de
/// negocio (portada o del espacio) directo a Cloudinary.
/// </summary>
public sealed class CreateBusinessPhotoUploadSignatureQueryHandler
{
    private readonly IObjectStorageService _objectStorageService;

    public CreateBusinessPhotoUploadSignatureQueryHandler(IObjectStorageService objectStorageService)
    {
        _objectStorageService = objectStorageService
            ?? throw new ArgumentNullException(nameof(objectStorageService));
    }

    public Task<UploadSignatureResult> HandleAsync(
        CreateBusinessPhotoUploadSignatureQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var folder = $"businesses/{query.BusinessId.Value}";

        return _objectStorageService.CreateUploadSignatureAsync(folder, cancellationToken);
    }
}
