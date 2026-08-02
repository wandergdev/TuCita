namespace TuCita.Application.Abstractions;

/// <summary>
/// Envía correos transaccionales (ej. notificar al profesional de una
/// nueva cita).
/// </summary>
public interface IEmailSender
{
    /// <param name="htmlBody">
    /// Versión en HTML del correo. Si se omite, se envía solo texto plano.
    /// </param>
    Task SendAsync(
        string to,
        string subject,
        string textBody,
        string? htmlBody = null,
        CancellationToken cancellationToken = default);
}
