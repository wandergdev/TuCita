using TuCita.Domain.Identifiers;

namespace TuCita.Application.Admin.Portfolio.ReplacePortfolioPhotoImage;

public sealed record ReplacePortfolioPhotoImageCommand(
    PortfolioPhotoId PortfolioPhotoId,
    string ImageUrl,
    string PublicId);
