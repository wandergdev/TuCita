using TuCita.Domain.Common;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.Entities;

/// <summary>
/// Excepción puntual al horario habitual de un profesional para un
/// rango de fechas: un horario especial para ese rango (<see cref="WorkingHours"/>
/// distinto al de la semana) o, si <see cref="WorkingHours"/> es null,
/// días no laborables (p. ej. vacaciones).
/// </summary>
public sealed class ScheduleException : Entity<ScheduleExceptionId>
{
    private const int MaxMessageLength = 300;

    public ProfessionalId ProfessionalId { get; private set; }

    public DateOnly StartDate { get; private set; }

    public DateOnly EndDate { get; private set; }

    /// <summary>
    /// Horario especial para este rango. Null significa día(s) no laborable(s).
    /// </summary>
    public TimeRange? WorkingHours { get; private set; }

    /// <summary>
    /// Almuerzo opcional dentro de <see cref="WorkingHours"/>. Solo tiene
    /// sentido cuando <see cref="WorkingHours"/> no es null.
    /// </summary>
    public TimeRange? LunchBreak { get; private set; }

    /// <summary>
    /// Mensaje opcional que se muestra al cliente (p. ej. motivo de las
    /// vacaciones).
    /// </summary>
    public string? Message { get; private set; }

    private ScheduleException(
        ScheduleExceptionId id,
        ProfessionalId professionalId,
        DateOnly startDate,
        DateOnly endDate,
        TimeRange? workingHours,
        TimeRange? lunchBreak,
        string? message)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        ProfessionalId = professionalId ?? throw new ArgumentNullException(nameof(professionalId));

        if (endDate < startDate)
            throw new ArgumentException(
                "La fecha de fin debe ser mayor o igual a la fecha de inicio.",
                nameof(endDate));

        StartDate = startDate;
        EndDate = endDate;
        WorkingHours = workingHours;

        if (lunchBreak is not null)
        {
            if (workingHours is null)
                throw new ArgumentException(
                    "No se puede definir un horario de almuerzo en un día no laborable.",
                    nameof(lunchBreak));

            if (lunchBreak.StartTime < workingHours.StartTime || lunchBreak.EndTime > workingHours.EndTime)
                throw new ArgumentException(
                    "El horario de almuerzo debe estar dentro del horario especial.",
                    nameof(lunchBreak));
        }

        LunchBreak = lunchBreak;

        var trimmedMessage = message?.Trim();
        if (!string.IsNullOrEmpty(trimmedMessage) && trimmedMessage.Length > MaxMessageLength)
            throw new ArgumentException(
                $"El mensaje no puede superar los {MaxMessageLength} caracteres.",
                nameof(message));

        Message = string.IsNullOrEmpty(trimmedMessage) ? null : trimmedMessage;
    }

    public static ScheduleException Create(
        ScheduleExceptionId id,
        ProfessionalId professionalId,
        DateOnly startDate,
        DateOnly endDate,
        TimeRange? workingHours,
        TimeRange? lunchBreak = null,
        string? message = null)
    {
        return new ScheduleException(id, professionalId, startDate, endDate, workingHours, lunchBreak, message);
    }

    public bool CoversDate(DateOnly date) => date >= StartDate && date <= EndDate;

    public bool OverlapsWith(DateOnly otherStartDate, DateOnly otherEndDate)
        => StartDate <= otherEndDate && EndDate >= otherStartDate;
}
