using TuCita.Application.Abstractions;

namespace TuCita.Infrastructure.Services;

/// <summary>
/// TuCita opera únicamente en República Dominicana (UTC-4, sin horario de
/// verano), así que se ancla a UTC y se convierte explícitamente a esa zona
/// en vez de usar <see cref="DateTime.Now"/>: ese valor depende de la zona
/// horaria del sistema operativo del servidor, que en producción corre en
/// UTC, y eso hacía que los recordatorios de cita (y las validaciones de
/// horario al reservar) se dispararan 4 horas antes de lo debido.
/// </summary>
public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    private static readonly TimeZoneInfo BusinessTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("America/Santo_Domingo");

    public DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, BusinessTimeZone);
}
