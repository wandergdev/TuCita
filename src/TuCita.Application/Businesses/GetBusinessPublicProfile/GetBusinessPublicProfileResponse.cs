namespace TuCita.Application.Businesses.GetBusinessPublicProfile;

/// <summary>
/// Datos públicos de un negocio, visibles para cualquier cliente que
/// visite su URL.
/// </summary>
public sealed record GetBusinessPublicProfileResponse(
    string Name,
    string Slug,
    string? CoverImageUrl,
    IReadOnlyCollection<string> SpacePhotos,
    IReadOnlyCollection<BusinessProfessionalSummary> Professionals);
