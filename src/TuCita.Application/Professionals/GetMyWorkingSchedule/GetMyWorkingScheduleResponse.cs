namespace TuCita.Application.Professionals.GetMyWorkingSchedule;

/// <summary>
/// Horario habitual del profesional autenticado.
/// </summary>
public sealed record GetMyWorkingScheduleResponse(
    IReadOnlyCollection<WorkingScheduleRuleResponse> Rules);
