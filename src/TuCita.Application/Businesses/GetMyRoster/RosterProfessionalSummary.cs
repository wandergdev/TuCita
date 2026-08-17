using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Application.Businesses.GetMyRoster;

public sealed record RosterProfessionalSummary(
    ProfessionalId ProfessionalId,
    string FullName,
    string Slug,
    Category Category);
