namespace TuCita.Application.Professionals.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword);
