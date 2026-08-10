namespace TuCita.Api.Contracts.Businesses;

public sealed record ResetPasswordRequest(string Token, string NewPassword);
