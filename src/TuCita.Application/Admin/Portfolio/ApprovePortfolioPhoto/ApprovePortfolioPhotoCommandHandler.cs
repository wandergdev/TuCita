using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.Portfolio.ApprovePortfolioPhoto;

public sealed class ApprovePortfolioPhotoCommandHandler
{
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;

    public ApprovePortfolioPhotoCommandHandler(IPortfolioPhotoRepository portfolioPhotoRepository)
    {
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
    }

    public async Task HandleAsync(
        ApprovePortfolioPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var photo = await _portfolioPhotoRepository.GetByIdAsync(command.PortfolioPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        photo.Approve();

        await _portfolioPhotoRepository.UpdateAsync(photo, cancellationToken);
    }
}
