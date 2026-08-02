namespace TuCita.Infrastructure.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string Host { get; init; } = "";

    public int Port { get; init; } = 587;

    public string Username { get; init; } = "";

    public string Password { get; init; } = "";

    public string FromAddress { get; init; } = "";

    public string FromName { get; init; } = "TuCita";

    public bool UseSsl { get; init; } = true;
}
