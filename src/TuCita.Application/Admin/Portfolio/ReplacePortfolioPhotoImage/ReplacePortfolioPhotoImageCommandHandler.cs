using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.Portfolio.ReplacePortfolioPhotoImage;

/// <summary>
/// Reemplaza la imagen de una foto ya subida (ej. el admin la descargó,
/// la mejoró y la volvió a subir) y borra el asset viejo en Cloudinary.
/// </summary>
public sealed class ReplacePortfolioPhotoImageCommandHandler
{
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;
    private readonly IObjectStorageService _objectStorageService;

    public ReplacePortfolioPhotoImageCommandHandler(
        IPortfolioPhotoRepository portfolioPhotoRepository,
        IObjectStorageService objectStorageService)
    {
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
        _objectStorageService = objectStorageService
            ?? throw new ArgumentNullException(nameof(objectStorageService));
    }

    public async Task HandleAsync(
        ReplacePortfolioPhotoImageCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var photo = await _portfolioPhotoRepository.GetByIdAsync(command.PortfolioPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        var oldPublicId = photo.PublicId;

        photo.ReplaceImage(command.ImageUrl, command.PublicId);
        await _portfolioPhotoRepository.UpdateAsync(photo, cancellationToken);

        await _objectStorageService.DeleteAsync(oldPublicId, cancellationToken);
    }
}
