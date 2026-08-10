namespace TuCita.Api.Contracts.Businesses;

public sealed record RegisterBusinessRequest(
    string Name,
    string Email,
    string Slug,
    string Password);
