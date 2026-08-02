using Microsoft.Extensions.Options;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using TuCita.Application.Abstractions;
using TuCita.Infrastructure.Options;

namespace TuCita.Infrastructure.Services;

public sealed class TwilioWhatsAppSender : IWhatsAppSender
{
    private readonly TwilioOptions _options;

    public TwilioWhatsAppSender(IOptions<TwilioOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendTextMessageAsync(
        string toPhoneNumber,
        string message,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.AccountSid) || string.IsNullOrWhiteSpace(_options.AuthToken))
            throw new InvalidOperationException("Twilio no está configurado (AccountSid/AuthToken vacíos).");

        TwilioClient.Init(_options.AccountSid, _options.AuthToken);

        await MessageResource.CreateAsync(
            body: message,
            from: new PhoneNumber(_options.FromWhatsAppNumber),
            to: new PhoneNumber(ToWhatsAppAddress(toPhoneNumber)));
    }

    private string ToWhatsAppAddress(string phoneNumber)
    {
        var e164 = phoneNumber.StartsWith('+')
            ? phoneNumber
            : $"{_options.DefaultCountryCode}{phoneNumber}";

        return $"whatsapp:{e164}";
    }
}
