namespace TuCita.Application.Abstractions;

/// <summary>
/// Envía mensajes de WhatsApp desde el número compartido de TuCita usando
/// la Cloud API de Meta. Fuera de la ventana de 24h de servicio al cliente,
/// todo mensaje que inicia el negocio debe usar una plantilla ya aprobada
/// por Meta — por eso no existe un "enviar texto libre", solo plantillas.
/// </summary>
public interface IWhatsAppSender
{
    Task SendTemplateMessageAsync(
        string toPhoneNumber,
        string templateName,
        string languageCode,
        IReadOnlyList<string> parameters,
        CancellationToken cancellationToken = default);
}
