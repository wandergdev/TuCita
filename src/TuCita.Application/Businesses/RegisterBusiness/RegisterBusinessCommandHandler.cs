using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Common;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Businesses.RegisterBusiness;

/// <summary>
/// Se encarga de registrar un nuevo negocio.
/// </summary>
public sealed class RegisterBusinessCommandHandler
{
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    private readonly IBusinessRepository _businessRepository;
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IIdentifierGenerator _identifierGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _frontendBaseUrl;
    private readonly ILogger<RegisterBusinessCommandHandler> _logger;

    public RegisterBusinessCommandHandler(
        IBusinessRepository businessRepository,
        IProfessionalRepository professionalRepository,
        IIdentifierGenerator identifierGenerator,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IDateTimeProvider dateTimeProvider,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<RegisterBusinessCommandHandler> logger)
    {
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
        _passwordHasher = passwordHasher
            ?? throw new ArgumentNullException(nameof(passwordHasher));
        _emailSender = emailSender
            ?? throw new ArgumentNullException(nameof(emailSender));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _frontendBaseUrl = frontendOptions?.Value.BaseUrl
            ?? throw new ArgumentNullException(nameof(frontendOptions));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<RegisterBusinessResponse> HandleAsync(
        RegisterBusinessCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var slug = Slug.Create(command.Slug);

        var slugExists = await _businessRepository.ExistsBySlugAsync(slug.Value, cancellationToken)
            || await _professionalRepository.ExistsBySlugAsync(slug.Value, cancellationToken);

        if (slugExists)
            throw new InvalidOperationException("El slug ya se encuentra registrado.");

        var business = Business.Create(
            _identifierGenerator.NewBusinessId(),
            command.Name,
            Email.Create(command.Email),
            slug,
            _passwordHasher.Hash(command.Password));

        var verificationToken = Guid.NewGuid().ToString("N");
        business.SetEmailVerificationToken(
            verificationToken,
            _dateTimeProvider.Now.Add(VerificationTokenLifetime));

        await _businessRepository.AddAsync(business, cancellationToken);

        await SendVerificationEmailAsync(business, verificationToken, cancellationToken);

        return new RegisterBusinessResponse(business.Id);
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
                "No se pudo enviar el correo de verificación al negocio {BusinessId}",
                business.Id);
        }
    }
}
