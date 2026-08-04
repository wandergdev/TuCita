namespace TuCita.Application.Common;

/// <summary>
/// Contenido de los correos transaccionales que envía TuCita.
/// </summary>
public static class EmailTemplates
{
    public static (string TextBody, string HtmlBody) NewAppointmentNotification(
        string customerName,
        string customerPhone,
        DateOnly date,
        TimeOnly startTime,
        TimeOnly endTime,
        string panelUrl)
    {
        var formattedDate = SpanishFormat.Date(date);
        var formattedTime = $"{SpanishFormat.Time(startTime)} – {SpanishFormat.Time(endTime)}";

        var textBody =
            $"Tienes una nueva cita con {customerName}.\n" +
            $"Fecha: {formattedDate}\n" +
            $"Hora: {formattedTime}\n" +
            $"Teléfono: {customerPhone}\n\n" +
            $"Ve tu panel: {panelUrl}";

        var htmlBody = $$"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif; background-color: #f5f6f8; padding: 32px 16px;">
              <div style="max-width: 480px; margin: 0 auto; background: #ffffff; border: 1px solid #e4e7ec; border-radius: 12px; padding: 32px;">
                <p style="margin: 0 0 4px; font-size: 12px; font-weight: 600; letter-spacing: 0.05em; text-transform: uppercase; color: #0d9488;">
                  Nueva cita
                </p>
                <h1 style="margin: 0 0 24px; font-size: 20px; line-height: 1.3; color: #1e293b;">
                  Tienes una nueva reserva
                </h1>
                <table style="width: 100%; border-collapse: collapse; font-size: 14px; color: #1e293b;">
                  <tr>
                    <td style="padding: 10px 0; color: #64748b;">Cliente</td>
                    <td style="padding: 10px 0; text-align: right; font-weight: 600;">{{customerName}}</td>
                  </tr>
                  <tr style="border-top: 1px solid #e4e7ec;">
                    <td style="padding: 10px 0; color: #64748b;">Teléfono</td>
                    <td style="padding: 10px 0; text-align: right; font-weight: 600;">{{customerPhone}}</td>
                  </tr>
                  <tr style="border-top: 1px solid #e4e7ec;">
                    <td style="padding: 10px 0; color: #64748b;">Fecha</td>
                    <td style="padding: 10px 0; text-align: right; font-weight: 600;">{{formattedDate}}</td>
                  </tr>
                  <tr style="border-top: 1px solid #e4e7ec;">
                    <td style="padding: 10px 0; color: #64748b;">Hora</td>
                    <td style="padding: 10px 0; text-align: right; font-weight: 600;">{{formattedTime}}</td>
                  </tr>
                </table>
                <div style="margin-top: 24px; text-align: center;">
                  <a href="{{panelUrl}}" style="display: inline-block; padding: 12px 28px; background-color: #0d9488; color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 14px;">
                    Ver mi panel
                  </a>
                </div>
              </div>
              <p style="max-width: 480px; margin: 16px auto 0; text-align: center; font-size: 12px; color: #94a3b8;">
                TuCita · agenda citas sin llamadas ni WhatsApp
              </p>
            </div>
            """;

        return (textBody, htmlBody);
    }

    public static (string TextBody, string HtmlBody) VerifyEmail(
        string fullName,
        string verifyUrl)
    {
        var textBody =
            $"Hola {fullName}, confirma tu correo para activar tu cuenta de TuCita:\n" +
            $"{verifyUrl}\n\n" +
            "Este enlace vence en 24 horas.";

        var htmlBody = $$"""
            <div style="font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Arial, sans-serif; background-color: #f5f6f8; padding: 32px 16px;">
              <div style="max-width: 480px; margin: 0 auto; background: #ffffff; border: 1px solid #e4e7ec; border-radius: 12px; padding: 32px;">
                <p style="margin: 0 0 4px; font-size: 12px; font-weight: 600; letter-spacing: 0.05em; text-transform: uppercase; color: #0d9488;">
                  Confirma tu cuenta
                </p>
                <h1 style="margin: 0 0 16px; font-size: 20px; line-height: 1.3; color: #1e293b;">
                  Hola {{fullName}}, ya casi terminas
                </h1>
                <p style="margin: 0 0 24px; font-size: 14px; line-height: 1.5; color: #64748b;">
                  Confirma tu correo para activar tu cuenta y empezar a recibir reservas. Este
                  enlace vence en 24 horas.
                </p>
                <div style="text-align: center;">
                  <a href="{{verifyUrl}}" style="display: inline-block; padding: 12px 28px; background-color: #0d9488; color: #ffffff; text-decoration: none; border-radius: 8px; font-weight: 600; font-size: 14px;">
                    Verificar mi correo
                  </a>
                </div>
              </div>
              <p style="max-width: 480px; margin: 16px auto 0; text-align: center; font-size: 12px; color: #94a3b8;">
                TuCita · agenda citas sin llamadas ni WhatsApp
              </p>
            </div>
            """;

        return (textBody, htmlBody);
    }
}
