using TuCita.Domain.Enums;

namespace TuCita.Application.Businesses.GetBusinessPublicProfile;

public sealed record BusinessProfessionalSummary(
    string FullName,
    string Slug,
    Category Category,
    IReadOnlyCollection<string> CoverPhotoUrls);
