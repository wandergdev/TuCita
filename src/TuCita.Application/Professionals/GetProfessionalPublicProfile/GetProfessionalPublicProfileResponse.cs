using TuCita.Domain.Enums;

namespace TuCita.Application.Professionals.GetProfessionalPublicProfile;

/// <summary>
/// Datos públicos de un profesional, visibles para cualquier cliente
/// que visite su URL. Nunca incluye email, teléfono ni credenciales.
/// </summary>
public sealed record GetProfessionalPublicProfileResponse(
    string FullName,
    string Slug,
    Category Category,
    int AppointmentDurationMinutes,
    IReadOnlyCollection<PortfolioPhotoPublicSummary> Photos);

public sealed record PortfolioPhotoPublicSummary(string ImageUrl, string? Caption);
