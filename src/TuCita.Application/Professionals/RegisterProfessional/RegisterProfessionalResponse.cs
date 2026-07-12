using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.RegisterProfessional;

/// <summary>
/// Resultado del registro de un profesional.
/// </summary>
public sealed record RegisterProfessionalResponse(
    ProfessionalId ProfessionalId);