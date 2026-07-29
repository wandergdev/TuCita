using TuCita.Domain.Entities;

namespace TuCita.Application.Abstractions;

/// <summary>
/// Genera el token de autenticación utilizado por el profesional
/// para acceder a su panel.
/// </summary>
public interface IAuthTokenGenerator
{
    string Generate(Professional professional);
}
