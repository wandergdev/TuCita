using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.Portfolio.DeletePortfolioPhoto;

public sealed record DeletePortfolioPhotoCommand(
    PortfolioPhotoId PortfolioPhotoId,
    ProfessionalId RequestingProfessionalId);
