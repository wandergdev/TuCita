using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.GetMyWorkingSchedule;

/// <summary>
/// Solicita el horario habitual del profesional autenticado.
/// </summary>
public sealed record GetMyWorkingScheduleQuery(ProfessionalId ProfessionalId);
