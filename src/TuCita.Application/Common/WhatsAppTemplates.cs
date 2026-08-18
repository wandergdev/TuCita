namespace TuCita.Application.Common;

/// <summary>
/// Nombres y orden de parámetros de las plantillas de WhatsApp aprobadas
/// en Meta (WhatsApp Manager → Message Templates). El orden de cada lista
/// devuelta tiene que coincidir exactamente con los {{1}}, {{2}}... del
/// cuerpo aprobado, porque la Graph API los llena por posición, no por
/// nombre.
/// </summary>
public static class WhatsAppTemplates
{
    public const string LanguageCode = "es";

    public const string NewAppointmentNotificationName = "nueva_cita_profesional";

    public const string AppointmentReminderName = "recordatorio_cita";

    /// <summary>
    /// Aviso al profesional cuando un cliente reserva una cita nueva.
    /// Plantilla: "Tienes una nueva cita en TuCita. Cliente: {{1}}
    /// Teléfono: {{2}} Fecha: {{3}} Hora: {{4}}".
    /// </summary>
    public static IReadOnlyList<string> NewAppointmentNotificationParameters(
        string customerName,
        string customerPhone,
        DateOnly date,
        TimeOnly startTime)
    {
        return
        [
            customerName,
            customerPhone,
            SpanishFormat.Date(date),
            SpanishFormat.Time(startTime),
        ];
    }

    /// <summary>
    /// Recordatorio al cliente antes de su cita, con el teléfono del
    /// profesional para que pueda cancelar o reprogramar directamente.
    /// Plantilla: "Hola {{1}}, te recordamos tu cita con {{2}} hoy a las
    /// {{3}}. ... contacta directamente al profesional al {{4}}...".
    /// </summary>
    public static IReadOnlyList<string> AppointmentReminderParameters(
        string customerName,
        string professionalFullName,
        TimeOnly startTime,
        string professionalPhone)
    {
        return
        [
            customerName,
            professionalFullName,
            SpanishFormat.Time(startTime),
            professionalPhone,
        ];
    }
}
