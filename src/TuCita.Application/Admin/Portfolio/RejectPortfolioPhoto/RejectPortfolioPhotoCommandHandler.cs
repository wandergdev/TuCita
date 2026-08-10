using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.Portfolio.RejectPortfolioPhoto;

public sealed class RejectPortfolioPhotoCommandHandler
{
    private readonly IPortfolioPhotoRepository _portfolioPhotoRepository;

    public RejectPortfolioPhotoCommandHandler(IPortfolioPhotoRepository portfolioPhotoRepository)
    {
        _portfolioPhotoRepository = portfolioPhotoRepository
            ?? throw new ArgumentNullException(nameof(portfolioPhotoRepository));
    }

    public async Task HandleAsync(
        RejectPortfolioPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var photo = await _portfolioPhotoRepository.GetByIdAsync(command.PortfolioPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        photo.Reject();

        await _portfolioPhotoRepository.UpdateAsync(photo, cancellationToken);
    }
}
