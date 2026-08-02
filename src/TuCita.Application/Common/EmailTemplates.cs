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
        TimeOnly endTime)
    {
        var formattedDate = SpanishFormat.Date(date);
        var formattedTime = $"{SpanishFormat.Time(startTime)} – {SpanishFormat.Time(endTime)}";

        var textBody =
            $"Tienes una nueva cita con {customerName}.\n" +
            $"Fecha: {formattedDate}\n" +
            $"Hora: {formattedTime}\n" +
            $"Teléfono: {customerPhone}";

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
              </div>
              <p style="max-width: 480px; margin: 16px auto 0; text-align: center; font-size: 12px; color: #94a3b8;">
                TuCita · agenda citas sin llamadas ni WhatsApp
              </p>
            </div>
            """;

        return (textBody, htmlBody);
    }
}
