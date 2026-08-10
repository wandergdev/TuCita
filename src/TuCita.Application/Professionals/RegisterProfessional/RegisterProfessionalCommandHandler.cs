using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Common;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;

namespace TuCita.Application.Professionals.RegisterProfessional;

/// <summary>
/// Se encarga de registrar un nuevo profesional.
/// </summary>
public sealed class RegisterProfessionalCommandHandler
{
    private static readonly TimeSpan VerificationTokenLifetime = TimeSpan.FromHours(24);

    private readonly IProfessionalRepository _professionalRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessInviteRepository _businessInviteRepository;
    private readonly IIdentifierGenerator _identifierGenerator;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IEmailSender _emailSender;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _frontendBaseUrl;
    private readonly ILogger<RegisterProfessionalCommandHandler> _logger;

    public RegisterProfessionalCommandHandler(
        IProfessionalRepository professionalRepository,
        IBusinessRepository businessRepository,
        IBusinessInviteRepository businessInviteRepository,
        IIdentifierGenerator identifierGenerator,
        IPasswordHasher passwordHasher,
        IEmailSender emailSender,
        IDateTimeProvider dateTimeProvider,
        IOptions<FrontendOptions> frontendOptions,
        ILogger<RegisterProfessionalCommandHandler> logger)
    {
        _professionalRepository = professionalRepository
                                  ?? throw new ArgumentNullException(nameof(professionalRepository));
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _businessInviteRepository = businessInviteRepository
            ?? throw new ArgumentNullException(nameof(businessInviteRepository));
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

    public async Task<RegisterProfessionalResponse> HandleAsync(
        RegisterProfessionalCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var slug = Slug.Create(command.Slug);

        var slugExists = await _professionalRepository.ExistsBySlugAsync(slug.Value, cancellationToken)
            || await _businessRepository.ExistsBySlugAsync(slug.Value, cancellationToken);

        if (slugExists)
            throw new InvalidOperationException(
                "El slug ya se encuentra registrado.");

        BusinessInvite? invite = null;
        if (!string.IsNullOrWhiteSpace(command.InviteToken))
        {
            invite = await _businessInviteRepository.GetByTokenAsync(command.InviteToken, cancellationToken);

            if (invite is null || !invite.IsValid(_dateTimeProvider.Now))
                throw new ArgumentException("La invitación no es válida o ya venció.");
        }

        var professional = Professional.Create(
            _identifierGenerator.NewProfessionalId(),
            command.FullName,
            Email.Create(command.Email),
            PhoneNumber.Create(command.PhoneNumber),
            slug,
            command.Category,
            AppointmentDuration.Create(command.AppointmentDuration),
            command.WorkingSchedule,
            _passwordHasher.Hash(command.Password));

        if (invite is not null)
            professional.JoinBusiness(invite.BusinessId);

        var verificationToken = Guid.NewGuid().ToString("N");
        professional.SetEmailVerificationToken(
            verificationToken,
            _dateTimeProvider.Now.Add(VerificationTokenLifetime));

        await _professionalRepository.AddAsync(
            professional,
            cancellationToken);

        if (invite is not null)
        {
            invite.Consume(_dateTimeProvider.Now);
            await _businessInviteRepository.UpdateAsync(invite, cancellationToken);
        }

        await SendVerificationEmailAsync(professional, verificationToken, cancellationToken);

        return new RegisterProfessionalResponse(
            professional.Id);
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
                "No se pudo enviar el correo de verificación al profesional {ProfessionalId}",
                professional.Id);
        }
    }
}
