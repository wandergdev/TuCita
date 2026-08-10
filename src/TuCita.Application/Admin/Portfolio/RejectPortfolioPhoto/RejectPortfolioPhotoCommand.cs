using TuCita.Domain.Identifiers;

namespace TuCita.Application.Admin.Portfolio.RejectPortfolioPhoto;

public sealed record RejectPortfolioPhotoCommand(PortfolioPhotoId PortfolioPhotoId);
