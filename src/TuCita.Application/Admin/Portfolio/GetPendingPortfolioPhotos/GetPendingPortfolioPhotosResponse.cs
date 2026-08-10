namespace TuCita.Application.Admin.Portfolio.GetPendingPortfolioPhotos;

public sealed record GetPendingPortfolioPhotosResponse(
    IReadOnlyCollection<PendingPortfolioPhotoSummary> Photos);
