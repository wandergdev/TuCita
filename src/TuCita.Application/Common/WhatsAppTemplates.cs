namespace TuCita.Application.Common;

/// <summary>
/// Contenido de los mensajes de WhatsApp que envía TuCita.
/// </summary>
public static class WhatsAppTemplates
{
    public static string NewAppointmentNotification(
        string customerName,
        string customerPhone,
        DateOnly date,
        TimeOnly startTime)
    {
        return $"Tienes una nueva cita en TuCita.\n" +
               $"Cliente: {customerName}\n" +
               $"Teléfono: {customerPhone}\n" +
               $"Fecha: {SpanishFormat.Date(date)}\n" +
               $"Hora: {SpanishFormat.Time(startTime)}";
    }
}
