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
        IReadOnlyCollection<Appointment> existingAppointments)
    {
        ArgumentNullException.ThrowIfNull(workingSchedule);
        ArgumentNullException.ThrowIfNull(duration);
        ArgumentNullException.ThrowIfNull(existingAppointments);

        var workingHours = workingSchedule.GetWorkingHours(date.DayOfWeek);
        if (workingHours is null)
            return [];

        var slots = new List<TimeOnly>();
        var step = TimeSpan.FromMinutes(duration.Minutes);
        var slotStart = workingHours.StartTime;

        while (true)
        {
            var slotEnd = slotStart.Add(step);
            if (slotEnd > workingHours.EndTime)
                break;

            if (IsSlotAvailable(slotStart, slotEnd, existingAppointments))
                slots.Add(slotStart);

            slotStart = slotStart.Add(step);
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
}
