using TuCita.Domain.Identifiers;

namespace TuCita.Application.Businesses.LoginBusiness;

/// <summary>
/// Resultado del inicio de sesión de un negocio.
/// </summary>
public sealed record LoginBusinessResponse(
    string Token,
    BusinessId BusinessId,
    string Slug,
    string Name);
