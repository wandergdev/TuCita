using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Admin.BusinessPhotos.ApproveBusinessPhoto;

public sealed class ApproveBusinessPhotoCommandHandler
{
    private readonly IBusinessPhotoRepository _businessPhotoRepository;

    public ApproveBusinessPhotoCommandHandler(IBusinessPhotoRepository businessPhotoRepository)
    {
        _businessPhotoRepository = businessPhotoRepository
            ?? throw new ArgumentNullException(nameof(businessPhotoRepository));
    }

    public async Task HandleAsync(
        ApproveBusinessPhotoCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var photo = await _businessPhotoRepository.GetByIdAsync(command.BusinessPhotoId, cancellationToken);

        if (photo is null)
            throw new NotFoundException("La foto solicitada no existe.");

        photo.Approve();

        await _businessPhotoRepository.UpdateAsync(photo, cancellationToken);
    }
}
