namespace TuCita.Application.Businesses.RegisterBusiness;

/// <summary>
/// Contiene la información necesaria para registrar un nuevo negocio.
/// </summary>
public sealed record RegisterBusinessCommand(
    string Name,
    string Email,
    string Slug,
    string Password);
