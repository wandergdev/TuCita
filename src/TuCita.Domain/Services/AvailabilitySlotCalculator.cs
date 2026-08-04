using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.Services;

/// <summary>
/// Calcula los horarios disponibles de un profesional para una fecha,
/// respetando su jornada laboral y las citas ya programadas.
/// </summary>
public static class AvailabilitySlotCalculator
{
    public static IReadOnlyCollection<TimeOnly> CalculateAvailableSlots(
        WorkingSchedule workingSchedule,
        AppointmentDuration duration,
        DateOnly date,
        IReadOnlyCollection<Appointment> existingAppointments,
        DateTime now,
        ScheduleException? exception = null)
    {
        ArgumentNullException.ThrowIfNull(workingSchedule);
        ArgumentNullException.ThrowIfNull(duration);
        ArgumentNullException.ThrowIfNull(existingAppointments);

        var workingHours = GetEffectiveWorkingHours(workingSchedule, date, exception);
        if (workingHours is null)
            return [];

        var lunchBreak = GetEffectiveLunchBreak(workingSchedule, date, exception);

        var slots = new List<TimeOnly>();
        var stepMinutes = duration.Minutes;

        // Se trabaja en minutos desde medianoche (enteros, sin "envolver")
        // en vez de sumar TimeOnly directamente: TimeOnly.Add da vuelta al
        // llegar a medianoche, lo que puede formar un ciclo que nunca
        // cumple la condición de corte cuando la jornada termina cerca de
        // las 00:00.
        var startMinutes = ToMinutes(workingHours.StartTime);
        var endMinutes = ToMinutes(workingHours.EndTime);

        for (var minutes = startMinutes; minutes + stepMinutes <= endMinutes; minutes += stepMinutes)
        {
            var slotStart = new TimeOnly(minutes / 60, minutes % 60);
            var slotEnd = new TimeOnly((minutes + stepMinutes) / 60, (minutes + stepMinutes) % 60);

            if (date.ToDateTime(slotStart) <= now)
                continue;

            if (OverlapsLunchBreak(slotStart, slotEnd, lunchBreak))
                continue;

            if (IsSlotAvailable(slotStart, slotEnd, existingAppointments))
                slots.Add(slotStart);
        }

        return slots;
    }

    /// <summary>
    /// Devuelve el horario laboral efectivo para una fecha: el de la
    /// excepción vigente si la hay (null si ese rango es no laborable),
    /// o si no el del horario semanal habitual.
    /// </summary>
    public static TimeRange? GetEffectiveWorkingHours(
        WorkingSchedule workingSchedule,
        DateOnly date,
        ScheduleException? exception)
    {
        ArgumentNullException.ThrowIfNull(workingSchedule);

        if (exception is not null && exception.CoversDate(date))
            return exception.WorkingHours;

        return workingSchedule.GetWorkingHours(date.DayOfWeek);
    }

    /// <summary>
    /// Devuelve el horario de almuerzo efectivo para una fecha: el de la
    /// excepción vigente ese día (su propio almuerzo, si tiene horario
    /// especial), o si no el de la regla semanal habitual.
    /// </summary>
    public static TimeRange? GetEffectiveLunchBreak(
        WorkingSchedule workingSchedule,
        DateOnly date,
        ScheduleException? exception)
    {
        ArgumentNullException.ThrowIfNull(workingSchedule);

        if (exception is not null && exception.CoversDate(date))
            return exception.LunchBreak;

        return workingSchedule.GetLunchBreak(date.DayOfWeek);
    }

    public static bool OverlapsLunchBreak(TimeOnly start, TimeOnly end, TimeRange? lunchBreak)
        => lunchBreak is not null && Overlaps(start, end, lunchBreak.StartTime, lunchBreak.EndTime);

    public static bool IsSlotAvailable(
        TimeOnly start,
        TimeOnly end,
        IReadOnlyCollection<Appointment> existingAppointments)
    {
        ArgumentNullException.ThrowIfNull(existingAppointments);

        return existingAppointments
            .Where(appointment => appointment.Status == AppointmentStatus.Scheduled)
            .All(appointment => !Overlaps(start, end, appointment.StartTime, appointment.EndTime));
    }

    private static bool Overlaps(TimeOnly startA, TimeOnly endA, TimeOnly startB, TimeOnly endB)
        => startA < endB && startB < endA;

    private static int ToMinutes(TimeOnly time) => time.Hour * 60 + time.Minute;
}
