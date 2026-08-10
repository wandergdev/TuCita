using TuCita.Domain.Identifiers;

namespace TuCita.Application.Admin.BusinessPhotos.ReplaceBusinessPhotoImage;

public sealed record ReplaceBusinessPhotoImageCommand(
    BusinessPhotoId BusinessPhotoId,
    string ImageUrl,
    string PublicId);
