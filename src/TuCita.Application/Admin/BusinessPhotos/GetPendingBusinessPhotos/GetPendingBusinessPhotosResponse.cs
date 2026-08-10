namespace TuCita.Application.Admin.BusinessPhotos.GetPendingBusinessPhotos;

public sealed record GetPendingBusinessPhotosResponse(
    IReadOnlyCollection<PendingBusinessPhotoSummary> Photos);
