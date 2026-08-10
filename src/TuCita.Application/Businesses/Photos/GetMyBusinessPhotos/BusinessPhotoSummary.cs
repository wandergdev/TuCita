using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Application.Businesses.Photos.GetMyBusinessPhotos;

public sealed record BusinessPhotoSummary(
    BusinessPhotoId Id,
    BusinessPhotoKind Kind,
    string ImageUrl,
    string? Caption,
    PortfolioPhotoStatus Status,
    DateTime CreatedAt);
