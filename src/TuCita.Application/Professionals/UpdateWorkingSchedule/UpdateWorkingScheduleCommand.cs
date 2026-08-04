using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Professionals.UpdateWorkingSchedule;

/// <summary>
/// Reemplaza el horario habitual del profesional autenticado.
/// </summary>
public sealed record UpdateWorkingScheduleCommand(
    ProfessionalId ProfessionalId,
    WorkingSchedule WorkingSchedule);
