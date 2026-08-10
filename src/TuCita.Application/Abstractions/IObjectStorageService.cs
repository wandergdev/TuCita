namespace TuCita.Application.Abstractions;

/// <summary>
/// Almacenamiento de imágenes (Cloudinary) con subida directa desde el
/// navegador vía firma.
/// </summary>
public interface IObjectStorageService
{
    Task<UploadSignatureResult> CreateUploadSignatureAsync(
        string folder,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Borra la imagen en el almacenamiento. Idempotente: no falla si ya
    /// no existe.
    /// </summary>
    Task DeleteAsync(
        string publicId,
        CancellationToken cancellationToken = default);
}
