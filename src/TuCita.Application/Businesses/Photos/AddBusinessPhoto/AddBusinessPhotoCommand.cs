using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Application.Businesses.Photos.AddBusinessPhoto;

public sealed record AddBusinessPhotoCommand(
    BusinessId BusinessId,
    BusinessPhotoKind Kind,
    string ImageUrl,
    string PublicId,
    string? Caption);
