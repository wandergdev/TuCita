using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.Portfolio.DeletePortfolioPhoto;

/// <summary>
/// Elimina una foto del portafolio, tanto en el almacenamiento como en el
/// registro local.
/// </summary>
public sealed class DeletePortfolioPhotoCommandHandler
{
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;
    private readonly IObjectStorageService _objectStorageService;

    public DeletePortfolioPhotoCommandHandler(
        IPortfolioPhotoRepository portfolioPhotoRepository,
        IObjectStorageService objectStorageService)
    {
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
        _objectStorageService = objectStorageService
            ?? throw new ArgumentNullException(nameof(objectStorageService));
    }

    public async Task HandleAsync(
        DeletePortfolioPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var photo = await _portfolioPhotoRepository.GetByIdAsync(command.PortfolioPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        if (photo.ProfessionalId != command.RequestingProfessionalId)
            throw new ForbiddenException("La foto no pertenece al profesional autenticado.");

        await _objectStorageService.DeleteAsync(photo.PublicId, cancellationToken);
        await _portfolioPhotoRepository.DeleteAsync(photo, cancellationToken);
    }
}
