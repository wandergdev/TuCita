namespace TuCita.Infrastructure.Options;

public sealed class MetaWhatsAppOptions
{
    public const string SectionName = "Meta:WhatsApp";

    /// <summary>
    /// Access token permanente del System User de Meta Business, usado
    /// para llamar a la Graph API y enviar mensajes.
    /// </summary>
    public string AccessToken { get; init; } = "";

    /// <summary>
    /// ID del número de teléfono de WhatsApp Business registrado en
    /// Meta (no es el número en sí, es el identificador que asigna la
    /// Graph API).
    /// </summary>
    public string PhoneNumberId { get; init; } = "";

    /// <summary>
    /// Valor secreto que TuCita define (no lo da Meta) y que se registra
    /// también en la consola de Meta al configurar el webhook. Meta lo
    /// reenvía en la verificación inicial (GET) para confirmar que el
    /// callback URL nos pertenece.
    /// </summary>
    public string VerifyToken { get; init; } = "";

    public string ApiVersion { get; init; } = "v21.0";

    /// <summary>
    /// Código de país anteponer a los teléfonos guardados sin él, ya que
    /// la Graph API exige el número completo (sin "+").
    /// </summary>
    public string DefaultCountryCode { get; init; } = "1";
}
