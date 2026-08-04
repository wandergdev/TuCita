using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.ScheduleExceptions.CreateScheduleException;

/// <summary>
/// Crea una excepción de horario para el profesional autenticado: un
/// horario especial para un rango de fechas (si vienen las horas) o
/// días no laborables (si no vienen).
/// </summary>
public sealed record CreateScheduleExceptionCommand(
    ProfessionalId ProfessionalId,
    DateOnly StartDate,
    DateOnly EndDate,
    TimeOnly? StartTime,
    TimeOnly? EndTime,
    TimeOnly? LunchBreakStartTime,
    TimeOnly? LunchBreakEndTime,
    string? Message);
