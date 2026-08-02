namespace TuCita.Application.Abstractions;

/// <summary>
/// Envía mensajes de WhatsApp desde el número compartido de TuCita.
/// </summary>
public interface IWhatsAppSender
{
    Task SendTextMessageAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default);
}
