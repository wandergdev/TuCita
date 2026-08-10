using TuCita.Domain.Common;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.Entities;

/// <summary>
/// Negocio con varios profesionales (ej. una peluquería con varios
/// barberos). El dueño se autentica con esta cuenta para invitar
/// profesionales y ver su lista; el horario, las citas y la página pública
/// de cada profesional siguen siendo de ellos, no del negocio.
/// </summary>
public sealed class Business : Entity<BusinessId>
{
    public string Name { get; private set; }

    public Email Email { get; private set; }

    /// <summary>
    /// Identificador público utilizado en la URL, en el mismo espacio de
    /// nombres que el slug de los profesionales (ej. tucita.com/mi-negocio).
    /// </summary>
    public Slug Slug { get; private set; }

    public string PasswordHash { get; private set; }

    public bool EmailVerified { get; private set; }

    public string? EmailVerificationToken { get; private set; }

    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    /// <summary>
    /// Token de un solo uso enviado por correo para restablecer la
    /// contraseña olvidada. Null cuando no hay un reseteo en curso.
    /// </summary>
    public string? PasswordResetToken { get; private set; }

    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    private Business(
        BusinessId id,
        string name,
        Email email,
        Slug slug,
        string passwordHash)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del negocio es obligatorio.", nameof(name));
        Name = name.Trim();

        Email = email ?? throw new ArgumentNullException(nameof(email));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de la contraseña es obligatorio.", nameof(passwordHash));
        PasswordHash = passwordHash;

        EmailVerified = false;
    }

    public static Business Create(
        BusinessId id,
        string name,
        Email email,
        Slug slug,
        string passwordHash)
    {
        return new Business(id, name, email, slug, passwordHash);
    }

    /// <summary>
    /// Genera un nuevo token de verificación de correo, invalidando
    /// cualquier token anterior (permite reenviar el correo).
    /// </summary>
    public void SetEmailVerificationToken(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("El token de verificación es obligatorio.", nameof(token));

        EmailVerificationToken = token;
        EmailVerificationTokenExpiresAt = expiresAt;
    }

    /// <summary>
    /// Confirma el correo del negocio e invalida el token usado.
    /// </summary>
    public void VerifyEmail()
    {
        EmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
    }

    /// <summary>
    /// Genera un nuevo token para restablecer la contraseña, invalidando
    /// cualquier token anterior (permite reenviar el correo).
    /// </summary>
    public void SetPasswordResetToken(string token, DateTime expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("El token de reseteo es obligatorio.", nameof(token));

        PasswordResetToken = token;
        PasswordResetTokenExpiresAt = expiresAt;
    }

    /// <summary>
    /// Establece una nueva contraseña a partir de un token de reseteo
    /// válido, invalidando el token usado.
    /// </summary>
    public void ResetPassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("El hash de la contraseña es obligatorio.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
        PasswordResetToken = null;
        PasswordResetTokenExpiresAt = null;
    }
}
