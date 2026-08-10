namespace TuCita.Application.Options;

/// <summary>
/// Credenciales de la única cuenta de administración (moderación de
/// portafolio). No hay sistema de roles/usuarios admin todavía, así que
/// esto reemplaza temporalmente a uno.
/// </summary>
public sealed class AdminOptions
{
    public const string SectionName = "Admin";

    public string Username { get; init; } = "";

    public string PasswordHash { get; init; } = "";
}
