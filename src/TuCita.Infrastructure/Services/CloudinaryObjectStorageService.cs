using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Infrastructure.Options;

namespace TuCita.Infrastructure.Services;

/// <summary>
/// Sube fotos de portafolio directo desde el navegador a Cloudinary
/// (signed upload): el backend solo firma los parámetros, nunca recibe
/// el binario de la imagen.
/// </summary>
public sealed class CloudinaryObjectStorageService : IObjectStorageService
{
    /// <summary>
    /// Limita el tamaño y optimiza calidad/formato automáticamente al
    /// subir, para que las fotos se vean bien tanto en mobile como en
    /// desktop sin que el profesional tenga que redimensionarlas.
    /// </summary>
    private const string Transformation = "c_limit,w_1600,h_1600,q_auto,f_auto";

    private readonly Cloudinary _cloudinary;

    public CloudinaryObjectStorageService(IOptions<CloudinaryOptions> options)
    {
        var cloudinaryOptions = options.Value;
        var account = new Account(cloudinaryOptions.CloudName, cloudinaryOptions.ApiKey, cloudinaryOptions.ApiSecret);
        _cloudinary = new Cloudinary(account);
    }

    public Task<UploadSignatureResult> CreateUploadSignatureAsync(
        string folder,
        CancellationToken cancellationToken = default)
    {
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var parametersToSign = new SortedDictionary<string, object>
        {
            { "timestamp", timestamp },
            { "folder", folder },
            { "transformation", Transformation }
        };

        var signature = _cloudinary.Api.SignParameters(parametersToSign);

        var result = new UploadSignatureResult(
            signature,
            timestamp,
            _cloudinary.Api.Account.ApiKey,
            _cloudinary.Api.Account.Cloud,
            folder,
            Transformation);

        return Task.FromResult(result);
    }

    public async Task DeleteAsync(string publicId, CancellationToken cancellationToken = default)
    {
        var result = await _cloudinary.DestroyAsync(new DeletionParams(publicId));

        if (result.Result is not ("ok" or "not found"))
            throw new InvalidOperationException($"Cloudinary respondió al borrar la imagen: {result.Result}");
    }
}
