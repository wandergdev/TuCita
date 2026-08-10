using TuCita.Domain.Identifiers;

namespace TuCita.Application.Admin.Portfolio.GetPendingPortfolioPhotos;

public sealed record PendingPortfolioPhotoSummary(
    PortfolioPhotoId Id,
    string ImageUrl,
    string? Caption,
    DateTime CreatedAt,
    ProfessionalId ProfessionalId,
    string ProfessionalFullName,
    string ProfessionalSlug);
