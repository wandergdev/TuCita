namespace TuCita.Api.Contracts.Businesses;

public sealed record LoginBusinessRequest(
    string Email,
    string Password);
