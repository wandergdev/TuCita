namespace TuCita.Application.Businesses.LoginBusiness;

/// <summary>
/// Credenciales utilizadas por el dueño de un negocio para acceder a su
/// panel.
/// </summary>
public sealed record LoginBusinessCommand(
    string Email,
    string Password);
