using TuCita.Domain.Enums;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Professionals.RegisterProfessional;

/// <summary>
/// Contiene toda la información necesaria para registrar
/// un nuevo profesional en el sistema.
/// </summary>
public sealed record RegisterProfessionalCommand(
    string FullName,
    string Email,
    string PhoneNumber,
    string Slug,
    Category Category,
    int AppointmentDuration,
    WorkingSchedule WorkingSchedule,
    string Password,
    string? InviteToken = null);