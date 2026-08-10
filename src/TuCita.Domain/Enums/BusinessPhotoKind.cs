namespace TuCita.Domain.Enums;

/// <summary>
/// Rol de una foto del negocio: <see cref="Cover"/> es el banner del hero
/// (máximo 1), <see cref="Space"/> son las fotos del local (máximo 4).
/// </summary>
public enum BusinessPhotoKind
{
    Cover,
    Space
}
