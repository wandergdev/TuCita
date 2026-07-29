using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.LoginProfessional;

/// <summary>
/// Resultado del inicio de sesión de un profesional.
/// </summary>
public sealed record LoginProfessionalResponse(
    string Token,
    ProfessionalId ProfessionalId,
    string Slug);
