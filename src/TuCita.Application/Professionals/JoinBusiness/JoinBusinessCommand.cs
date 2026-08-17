using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.JoinBusiness;

public sealed record JoinBusinessCommand(ProfessionalId ProfessionalId, string InviteToken);
