namespace TuCita.Application.Businesses.GetMyRoster;

public sealed record GetMyRosterResponse(
    IReadOnlyCollection<RosterProfessionalSummary> Professionals);
