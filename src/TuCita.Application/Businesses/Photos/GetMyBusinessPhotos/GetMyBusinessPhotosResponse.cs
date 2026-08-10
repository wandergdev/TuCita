namespace TuCita.Application.Businesses.Photos.GetMyBusinessPhotos;

public sealed record GetMyBusinessPhotosResponse(IReadOnlyCollection<BusinessPhotoSummary> Photos);
