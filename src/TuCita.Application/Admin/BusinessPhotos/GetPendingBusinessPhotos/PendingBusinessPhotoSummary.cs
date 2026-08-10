using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Application.Admin.BusinessPhotos.GetPendingBusinessPhotos;

public sealed record PendingBusinessPhotoSummary(
    BusinessPhotoId Id,
    BusinessPhotoKind Kind,
    string ImageUrl,
    string? Caption,
    DateTime CreatedAt,
    BusinessId BusinessId,
    string BusinessName,
    string BusinessSlug);
