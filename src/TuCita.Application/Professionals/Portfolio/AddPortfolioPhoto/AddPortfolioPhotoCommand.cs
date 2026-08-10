using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.Portfolio.AddPortfolioPhoto;

public sealed record AddPortfolioPhotoCommand(
    ProfessionalId ProfessionalId,
    string ImageUrl,
    string PublicId,
    string? Caption);
