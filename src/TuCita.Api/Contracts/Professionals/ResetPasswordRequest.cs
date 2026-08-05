namespace TuCita.Api.Contracts.Professionals;

public sealed record ResetPasswordRequest(string Token, string NewPassword);
