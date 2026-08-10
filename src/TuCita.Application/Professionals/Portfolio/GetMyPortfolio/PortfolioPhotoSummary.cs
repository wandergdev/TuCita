using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.Portfolio.GetMyPortfolio;

public sealed record PortfolioPhotoSummary(
    PortfolioPhotoId Id,
    string ImageUrl,
    string? Caption,
    PortfolioPhotoStatus Status,
    DateTime CreatedAt);
