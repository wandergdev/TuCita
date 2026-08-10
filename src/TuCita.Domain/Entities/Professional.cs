using TuCita.Domain.Common;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.Entities;

public sealed class Professional : Entity<ProfessionalId>
{
    /// <summary>
    /// Nombre completo del profesional.
    /// </summary>
    public string FullName { get; private set; }

    /// <summary>
    /// Correo electrónico del profesional.
    /// </summary>
    public Email Email { get; private set; }

    /// <summary>
    /// Número telefónico del profesional.
    /// </summary>
    public PhoneNumber PhoneNumber { get; private set; }

    /// <summary>
    /// Identificador público utilizado en la URL.
    /// Ejemplo:
    /// tucita.com/jose-barber
    /// </summary>
    public Slug Slug { get; private set; }

    /// <summary>
    /// Categoría del profesional.
    /// En la V1 solamente tiene fines informativos.
    /// </summary>
    public Category Category { get; private set; }

    /// <summary>
    /// Duración de las citas del profesional.
    /// </summary>
    public AppointmentDuration AppointmentDuration { get; private set; }

    /// <summary>
    /// Horario habitual del profesional.
    /// </summary>
    public WorkingSchedule WorkingSchedule { get; private set; }

    /// <summary>
    /// Indica si el profesional puede recibir nuevas citas.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Hash de la contraseña utilizada para autenticarse en el panel.
    /// </summary>
    public string PasswordHash { get; private set; }

    /// <summary>
    /// Indica si el profesional confirmó su correo. No puede iniciar
    /// sesión hasta que esto sea true.
    /// </summary>
    public bool EmailVerified { get; private set; }

    /// <summary>
    /// Token de un solo uso enviado por correo para verificar la
    /// cuenta. Null una vez verificada.
    /// </summary>
    public string? EmailVerificationToken { get; private set; }

    public DateTime? EmailVerificationTokenExpiresAt { get; private set; }

    /// <summary>
    /// Token de un solo uso enviado por correo para restablecer la
    /// contraseña olvidada. Null cuando no hay un reseteo en curso.
    /// </summary>
    public string? PasswordResetToken { get; private set; }

    public DateTime? PasswordResetTokenExpiresAt { get; private set; }

    /// <summary>
    /// Negocio al que pertenece este profesional (ej. la peluquería donde
    /// trabaja). Null si opera de forma independiente. Su página y horario
    /// públicos siguen funcionando igual en ambos casos.
    /// </summary>
    public BusinessId? BusinessId { get; private set; }

    private Professional(
        ProfessionalId id,
        string fullName,
        Email email,
        PhoneNumber phoneNumber,
        Slug slug,
        Category category,
        AppointmentDuration appointmentDuration,
        WorkingSchedule workingSchedule,
        string passwordHash)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("El nombre del profesional es obligatorio.", nameof(fullName));
        FullName = fullName.Trim();
        Email = email ?? throw new ArgumentNullException(nameof(email));
        PhoneNumber = phoneNumber ?? throw new ArgumentNullException(nameof(phoneNumber));
        Slug = slug ?? throw new ArgumentNullException(nameof(slug));
        Category = category;
        AppointmentDuration = appointmentDuration
            ?? throw new ArgumentNullException(nameof(appointmentDuration));
        WorkingSchedule = workingSchedule
            ?? throw new ArgumentNullException(nameof(workingSchedule));

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de la contraseña es obligatorio.", nameof(passwordHash));
        PasswordHash = passwordHash;

        // Todo profesional inicia activo al momento de registrarse
        IsActive = true;

        // El correo empieza sin verificar; se confirma con el enlace
        // que se le manda al registrarse.
        EmailVerified = false;
    }

    public static Professional Create(
        ProfessionalId id,
        string fullName,
        Email email,
        PhoneNumber phoneNumber,
        Slug slug,
        Category category,
        AppointmentDuration appointmentDuration,
        WorkingSchedule workingSchedule,
        string passwordHash)
    {
        return new Professional(
            id,
            fullName,
            email,
            phoneNumber,
            slug,
            category,
            appointmentDuration,
            workingSchedule,
            passwordHash);
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
    /// Confirma el correo del profesional e invalida el token usado.
    /// </summary>
    public void VerifyEmail()
    {
        EmailVerified = true;
        EmailVerificationToken = null;
        EmailVerificationTokenExpiresAt = null;
    }

    /// <summary>
    /// Reemplaza el horario habitual del profesional.
    /// </summary>
    public void UpdateWorkingSchedule(WorkingSchedule workingSchedule)
    {
        WorkingSchedule = workingSchedule ?? throw new ArgumentNullException(nameof(workingSchedule));
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

    /// <summary>
    /// Establece una nueva contraseña estando autenticado (tras verificar
    /// la contraseña actual).
    /// </summary>
    public void ChangePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new ArgumentException("El hash de la contraseña es obligatorio.", nameof(newPasswordHash));

        PasswordHash = newPasswordHash;
    }

    /// <summary>
    /// Asocia al profesional con un negocio (ej. al registrarse mediante
    /// una invitación). Solo puede hacerse una vez.
    /// </summary>
    public void JoinBusiness(BusinessId businessId)
    {
        ArgumentNullException.ThrowIfNull(businessId);

        if (BusinessId is not null)
            throw new InvalidOperationException("El profesional ya pertenece a un negocio.");

        BusinessId = businessId;
    }
}