namespace TuCita.Application.Common;

/// <summary>
/// Valores del claim de rol (<see cref="System.Security.Claims.ClaimTypes.Role"/>)
/// que distingue si un token de acceso pertenece a un profesional o a un
/// negocio, para que las políticas de autorización puedan separar sus
/// endpoints.
/// </summary>
public static class AccountTypeClaims
{
    public const string Professional = "professional";

    public const string Business = "business";

    public const string Admin = "admin";
}
