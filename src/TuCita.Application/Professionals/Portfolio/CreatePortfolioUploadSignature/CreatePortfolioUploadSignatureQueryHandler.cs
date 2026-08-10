using TuCita.Application.Abstractions;

namespace TuCita.Application.Professionals.Portfolio.CreatePortfolioUploadSignature;

/// <summary>
/// Genera la firma que el navegador necesita para subir una foto directo
/// a Cloudinary.
/// </summary>
public sealed class CreatePortfolioUploadSignatureQueryHandler
{
    private readonly IObjectStorageService _objectStorageService;

    public CreatePortfolioUploadSignatureQueryHandler(IObjectStorageService objectStorageService)
    {
        _objectStorageService = objectStorageService
            ?? throw new ArgumentNullException(nameof(objectStorageService));
    }

    public Task<UploadSignatureResult> HandleAsync(
        CreatePortfolioUploadSignatureQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var folder = $"professionals/{query.ProfessionalId.Value}";

        return _objectStorageService.CreateUploadSignatureAsync(folder, cancellationToken);
    }
}
