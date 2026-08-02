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
        DateTime now)
    {
        ArgumentNullException.ThrowIfNull(workingSchedule);
        ArgumentNullException.ThrowIfNull(duration);
        ArgumentNullException.ThrowIfNull(existingAppointments);

        var workingHours = workingSchedule.GetWorkingHours(date.DayOfWeek);
        if (workingHours is null)
            return [];

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

            if (IsSlotAvailable(slotStart, slotEnd, existingAppointments))
                slots.Add(slotStart);
        }

        return slots;
    }

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
