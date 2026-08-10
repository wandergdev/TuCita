namespace TuCita.Application.Businesses.ResetPassword;

public sealed record ResetPasswordCommand(string Token, string NewPassword);
