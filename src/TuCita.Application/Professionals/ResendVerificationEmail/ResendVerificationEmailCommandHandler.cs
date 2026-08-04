using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Common;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.ResendVerificationEmail;

/// <summary>
/// Reenvía el correo de verificación con un token nuevo. No revela si
/// el correo existe o ya está verificado: siempre responde igual, para
/// no filtrar qué correos están registrados.
/// </summary>
public sealed class ResendVerificationEmailCommandHandler
{
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    private readonly IProfessionalRepository _professionalRepository;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _frontendBaseUrl;
    private readonly ILogger<ResendVerificationEmailCommandHandler> _logger;

    public ResendVerificationEmailCommandHandler(
        IProfessionalRepository professionalRepository,
        IEmailSender emailSender,
        IDateTimeProvider dateTimeProvider,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<ResendVerificationEmailCommandHandler> logger)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _emailSender = emailSender
            ?? throw new ArgumentNullException(nameof(emailSender));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _frontendBaseUrl = frontendOptions?.Value.BaseUrl
            ?? throw new ArgumentNullException(nameof(frontendOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(
        ResendVerificationEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var professional = await _professionalRepository.GetByEmailAsync(
            command.Email.Trim().ToLower(),
            cancellationToken);

        if (professional is null || professional.EmailVerified)
            return;

        var token = Guid.NewGuid().ToString("N");
        professional.SetEmailVerificationToken(
            token,
            _dateTimeProvider.Now.Add(VerificationTokenLifetime));

        await _professionalRepository.UpdateAsync(professional, cancellationToken);

        await SendVerificationEmailAsync(professional, token, cancellationToken);
    }

    private async Task SendVerificationEmailAsync(
        Professional professional,
        string verificationToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var verifyUrl = $"{_frontendBaseUrl}/verificar-correo?token={verificationToken}";
            var (textBody, htmlBody) = EmailTemplates.VerifyEmail(professional.FullName, verifyUrl);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(8));

            await _emailSender.SendAsync(
                professional.Email.Value,
                "Confirma tu correo en TuCita",
                textBody,
                htmlBody,
                timeoutCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo reenviar el correo de verificación al profesional {ProfessionalId}",
                professional.Id);
        }
    }
}
