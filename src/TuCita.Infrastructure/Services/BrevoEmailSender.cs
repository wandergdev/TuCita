using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Infrastructure.Options;

namespace TuCita.Infrastructure.Services;

/// <summary>
/// Envía correos usando la API HTTP de Brevo en vez de SMTP.
/// Varios hosts gratuitos (Render incluido) bloquean las conexiones
/// salientes por puertos SMTP para evitar spam, pero HTTPS siempre
/// está disponible.
/// </summary>
public sealed class BrevoEmailSender : IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly EmailOptions _options;

    public BrevoEmailSender(HttpClient httpClient, IOptions<EmailOptions> options)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string textBody,
        string? htmlBody = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("El correo no está configurado (Email:ApiKey vacío).");

        var payload = new
        {
            sender = new { name = _options.FromName, email = _options.FromAddress },
            to = new[] { new { email = to } },
            subject,
            textContent = textBody,
            htmlContent = htmlBody
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "smtp/email")
        {
            Content = JsonContent.Create(payload)
        };
        request.Headers.Add("api-key", _options.ApiKey);
        request.Headers.Add("accept", "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException($"Brevo respondió {(int)response.StatusCode}: {body}");
        }
    }
}
