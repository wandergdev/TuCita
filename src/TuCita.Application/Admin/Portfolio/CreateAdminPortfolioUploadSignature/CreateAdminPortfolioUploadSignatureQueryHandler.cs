using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.Portfolio.CreateAdminPortfolioUploadSignature;

/// <summary>
/// Genera la firma que el panel de admin necesita para subir una versión
/// editada de una foto pendiente, reutilizando la misma carpeta de
/// Cloudinary que el profesional dueño de la foto.
/// </summary>
public sealed class CreateAdminPortfolioUploadSignatureQueryHandler
{
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;
    private readonly IObjectStorageService _objectStorageService;

    public CreateAdminPortfolioUploadSignatureQueryHandler(
        IPortfolioPhotoRepository portfolioPhotoRepository,
        IObjectStorageService objectStorageService)
    {
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
        _objectStorageService = objectStorageService
            ?? throw new ArgumentNullException(nameof(objectStorageService));
    }

    public async Task<UploadSignatureResult> HandleAsync(
        CreateAdminPortfolioUploadSignatureQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var photo = await _portfolioPhotoRepository.GetByIdAsync(query.PortfolioPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        var folder = $"professionals/{photo.ProfessionalId.Value}";

        return await _objectStorageService.CreateUploadSignatureAsync(folder, cancellationToken);
    }
}
