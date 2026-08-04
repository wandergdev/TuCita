namespace TuCita.Infrastructure.Options;

public sealed class EmailOptions
{
    public const string SectionName = "Email";

    public string ApiKey { get; init; } = "";

    public string FromAddress { get; init; } = "";

    public string FromName { get; init; } = "TuCita";
}
