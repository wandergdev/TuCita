namespace TuCita.Application.Professionals.Portfolio.GetMyPortfolio;

public sealed record GetMyPortfolioResponse(IReadOnlyCollection<PortfolioPhotoSummary> Photos);
