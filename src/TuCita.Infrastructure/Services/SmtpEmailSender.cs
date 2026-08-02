using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using TuCita.Application.Abstractions;
using TuCita.Infrastructure.Options;

namespace TuCita.Infrastructure.Services;

public sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;

    public SmtpEmailSender(IOptions<EmailOptions> options)
    {
        _options = options.Value;
    }

    public async Task SendAsync(
        string to,
        string subject,
        string textBody,
        string? htmlBody = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.Host))
            throw new InvalidOperationException("El correo no está configurado (Email:Host vacío).");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { TextBody = textBody };
        if (htmlBody is not null)
            bodyBuilder.HtmlBody = htmlBody;

        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient
        {
            // Algunos entornos (contenedores, redes con salida restringida) no
            // pueden completar la verificación de revocación en línea del
            // certificado TLS, y MailKit rechaza la conexión por eso aunque el
            // certificado sea válido. Se valida igual la cadena y el host,
            // solo se omite el chequeo de revocación.
            CheckCertificateRevocation = false,
        };

        await client.ConnectAsync(
            _options.Host,
            _options.Port,
            _options.UseSsl ? SecureSocketOptions.Auto : SecureSocketOptions.None,
            cancellationToken);

        await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
