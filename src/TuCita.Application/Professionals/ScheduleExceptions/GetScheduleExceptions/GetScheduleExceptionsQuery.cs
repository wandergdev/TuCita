using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.ScheduleExceptions.GetScheduleExceptions;

/// <summary>
/// Solicita las excepciones de horario vigentes/futuras del profesional
/// autenticado.
/// </summary>
public sealed record GetScheduleExceptionsQuery(ProfessionalId ProfessionalId);
