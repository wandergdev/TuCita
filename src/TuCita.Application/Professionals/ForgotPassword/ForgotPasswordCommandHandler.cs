using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Common;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.ForgotPassword;

/// <summary>
/// Envía un enlace para restablecer la contraseña con un token nuevo. No
/// revela si el correo existe: siempre responde igual, para no filtrar
/// qué correos están registrados.
/// </summary>
public sealed class ForgotPasswordCommandHandler
{
    private static readonly TimeSpan ResetTokenLifetime = TimeSpan.FromHours(1);

    private readonly IProfessionalRepository _professionalRepository;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _frontendBaseUrl;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IProfessionalRepository professionalRepository,
        IEmailSender emailSender,
        IDateTimeProvider dateTimeProvider,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<ForgotPasswordCommandHandler> logger)
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
        ForgotPasswordCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var professional = await _professionalRepository.GetByEmailAsync(
            command.Email.Trim().ToLower(),
            cancellationToken);

        if (professional is null)
            return;

        var token = Guid.NewGuid().ToString("N");
        professional.SetPasswordResetToken(
            token,
            _dateTimeProvider.Now.Add(ResetTokenLifetime));

        await _professionalRepository.UpdateAsync(professional, cancellationToken);

        await SendResetEmailAsync(professional, token, cancellationToken);
    }

    private async Task SendResetEmailAsync(
        Professional professional,
        string resetToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var resetUrl = $"{_frontendBaseUrl}/restablecer-password?token={resetToken}";
            var (textBody, htmlBody) = EmailTemplates.ResetPassword(professional.FullName, resetUrl);

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(TimeSpan.FromSeconds(8));

            await _emailSender.SendAsync(
                professional.Email.Value,
                "Restablece tu contraseña en TuCita",
                textBody,
                htmlBody,
                timeoutCts.Token);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "No se pudo enviar el correo de reseteo de contraseña al profesional {ProfessionalId}",
                professional.Id);
        }
    }
}
