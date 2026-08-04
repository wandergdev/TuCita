using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.ScheduleExceptions.DeleteScheduleException;

/// <summary>
/// Elimina una excepción de horario propia del profesional autenticado.
/// </summary>
public sealed record DeleteScheduleExceptionCommand(
    ScheduleExceptionId ScheduleExceptionId,
    ProfessionalId RequestingProfessionalId);
