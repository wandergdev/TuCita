namespace TuCita.Infrastructure.Options;

public sealed class TwilioOptions
{
    public const string SectionName = "Twilio";

    public string AccountSid { get; init; } = "";

    public string AuthToken { get; init; } = "";

    /// <summary>
    /// Número compartido de TuCita en formato "whatsapp:+1415...".
    /// </summary>
    public string FromWhatsAppNumber { get; init; } = "";

    /// <summary>
    /// Código de país anteponer a los teléfonos guardados sin "+",
    /// ya que Twilio exige formato E.164.
    /// </summary>
    public string DefaultCountryCode { get; init; } = "+1";
}
