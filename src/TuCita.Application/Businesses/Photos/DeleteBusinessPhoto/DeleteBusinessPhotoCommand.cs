using TuCita.Domain.Identifiers;

namespace TuCita.Application.Businesses.Photos.DeleteBusinessPhoto;

public sealed record DeleteBusinessPhotoCommand(
    BusinessPhotoId BusinessPhotoId,
    BusinessId RequestingBusinessId);
