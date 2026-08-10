namespace TuCita.Api.Contracts.Professionals;

public sealed record AddPortfolioPhotoRequest(string ImageUrl, string PublicId, string? Caption);
