using TuCita.Domain.Enums;

namespace TuCita.Application.Businesses.GetMyRoster;

public sealed record RosterProfessionalSummary(
    string FullName,
    string Slug,
    Category Category);
