using TuCita.Domain.Identifiers;

namespace TuCita.Application.Professionals.ChangePassword;

public sealed record ChangePasswordCommand(
    ProfessionalId ProfessionalId,
    string CurrentPassword,
    string NewPassword);
