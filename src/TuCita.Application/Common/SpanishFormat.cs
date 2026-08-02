using System.Globalization;

namespace TuCita.Application.Common;

/// <summary>
/// Formatea fechas y horas en español para mensajes al cliente y al
/// profesional (recordatorios de WhatsApp, notificaciones por correo).
/// </summary>
public static class SpanishFormat
{
    private static readonly CultureInfo Culture = new("es-DO");

    public static string Time(TimeOnly time) => time.ToString("h:mm tt", Culture);

    public static string Date(DateOnly date) => date.ToString("dddd, d 'de' MMMM", Culture);
}
