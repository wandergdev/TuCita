using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.BusinessPhotos.RejectBusinessPhoto;

public sealed class RejectBusinessPhotoCommandHandler
{
    private readonly IBusinessPhotoRepository _businessPhotoRepository;

    public RejectBusinessPhotoCommandHandler(IBusinessPhotoRepository businessPhotoRepository)
    {
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
    }

    public async Task HandleAsync(
        RejectBusinessPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var photo = await _businessPhotoRepository.GetByIdAsync(command.BusinessPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        photo.Reject();

        await _businessPhotoRepository.UpdateAsync(photo, cancellationToken);
    }
}
