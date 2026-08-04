namespace TuCita.Application.Options;

/// <summary>
/// URL base del sitio (Next.js) para construir enlaces dentro de
/// correos y otras notificaciones.
/// </summary>
public sealed class FrontendOptions
{
    public const string SectionName = "Frontend";

    public string BaseUrl { get; init; } = "";
}
