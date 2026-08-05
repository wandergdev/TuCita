namespace TuCita.Api.Contracts.Professionals;

public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
