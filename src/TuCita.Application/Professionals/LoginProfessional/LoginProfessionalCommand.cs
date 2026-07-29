namespace TuCita.Application.Professionals.LoginProfessional;

/// <summary>
/// Credenciales utilizadas por un profesional para acceder a su panel.
/// </summary>
public sealed record LoginProfessionalCommand(
    string Email,
    string Password);
