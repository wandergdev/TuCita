using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Infrastructure.Options;

namespace TuCita.Infrastructure.Services;

public sealed class MetaWhatsAppSender : IWhatsAppSender
{
    private readonly HttpClient _httpClient;
    private readonly MetaWhatsAppOptions _options;

    public MetaWhatsAppSender(HttpClient httpClient, IOptions<MetaWhatsAppOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task SendTemplateMessageAsync(
        string toPhoneNumber,
        string templateName,
        string languageCode,
        IReadOnlyList<string> parameters,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AccessToken) || string.IsNullOrWhiteSpace(_options.PhoneNumberId))
            throw new InvalidOperationException("Meta WhatsApp no está configurado (AccessToken/PhoneNumberId vacíos).");

        var payload = new
        {
            messaging_product = "whatsapp",
            to = ToWhatsAppNumber(toPhoneNumber),
            type = "template",
            template = new
            {
                name = templateName,
                language = new { code = languageCode },
                components = new object[]
                {
                    new
                    {
                        type = "body",
                        parameters = parameters
                            .Select(value => new { type = "text", text = value })
                            .ToArray(),
                    },
                },
            },
        };

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"{_options.ApiVersion}/{_options.PhoneNumberId}/messages")
        {
            Content = JsonContent.Create(payload),
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.AccessToken);

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Meta WhatsApp respondió {(int)response.StatusCode}: {body}");
        }
    }

    private string ToWhatsAppNumber(string phoneNumber)
    {
        return phoneNumber.StartsWith('+')
            ? phoneNumber[1..]
            : $"{_options.DefaultCountryCode}{phoneNumber}";
    }
}
