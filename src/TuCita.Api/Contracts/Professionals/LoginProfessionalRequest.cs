namespace TuCita.Api.Contracts.Professionals;

public sealed record LoginProfessionalRequest(
    string Email,
    string Password);
