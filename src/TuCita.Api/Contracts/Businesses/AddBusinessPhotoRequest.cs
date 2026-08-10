using TuCita.Domain.Enums;

namespace TuCita.Api.Contracts.Businesses;

public sealed record AddBusinessPhotoRequest(
    BusinessPhotoKind Kind,
    string ImageUrl,
    string PublicId,
    string? Caption);
