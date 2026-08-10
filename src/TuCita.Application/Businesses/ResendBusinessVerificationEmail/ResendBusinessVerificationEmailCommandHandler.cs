using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Common;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.ResendBusinessVerificationEmail;

/// <summary>
/// Reenvía el correo de verificación de un negocio con un token nuevo. No
/// revela si el correo existe: siempre responde igual.
/// </summary>
public sealed class ResendBusinessVerificationEmailCommandHandler
{
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    private readonly IBusinessRepository _businessRepository;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _frontendBaseUrl;
    private readonly ILogger<ResendBusinessVerificationEmailCommandHandler> _logger;

    public ResendBusinessVerificationEmailCommandHandler(
        IBusinessRepository businessRepository,
        IEmailSender emailSender,
        IDateTimeProvider dateTimeProvider,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<ResendBusinessVerificationEmailCommandHandler> logger)
    {
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _emailSender = emailSender
            ?? throw new ArgumentNullException(nameof(emailSender));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _frontendBaseUrl = frontendOptions?.Value.BaseUrl
            ?? throw new ArgumentNullException(nameof(frontendOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(
        ResendBusinessVerificationEmailCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var business = await _businessRepository.GetByEmailAsync(
            command.Email.Trim().ToLower(),
            cancellationToken);

        if (business is null || business.EmailVerified)
            return;

        var token = Guid.NewGuid().ToString("N");
        business.SetEmailVerificationToken(
            token,
            _dateTimeProvider.Now.Add(VerificationTokenLifetime));

        await _businessRepository.UpdateAsync(business, cancellationToken);

        await SendVerificationEmailAsync(business, token, cancellationToken);
    }

    private async Task SendVerificationEmailAsync(
        Business business,
        string verificationToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var verifyUrl = $"{_frontendBaseUrl}/verificar-correo?token={verificationToken}&type=business";
            var (textBody, htmlBody) = EmailTemplates.VerifyEmail(business.Name, verifyUrl);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(8));

            await _emailSender.SendAsync(
                business.Email.Value,
                "Confirma tu correo en TuCita",
                textBody,
                htmlBody,
                timeoutCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo reenviar el correo de verificación al negocio {BusinessId}",
                business.Id);
        }
    }
}
