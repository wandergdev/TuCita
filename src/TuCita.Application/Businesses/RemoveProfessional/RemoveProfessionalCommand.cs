using TuCita.Domain.Identifiers;

namespace TuCita.Application.Businesses.RemoveProfessional;

public sealed record RemoveProfessionalCommand(BusinessId BusinessId, ProfessionalId ProfessionalId);
