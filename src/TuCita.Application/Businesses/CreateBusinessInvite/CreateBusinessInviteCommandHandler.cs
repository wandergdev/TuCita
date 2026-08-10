using Microsoft.Extensions.Options;
using TuCita.Application.Abstractions;
using TuCita.Application.Options;
using TuCita.Domain.Entities;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.CreateBusinessInvite;

/// <summary>
/// Genera un enlace de invitación de un solo uso para que un profesional
/// se una al negocio autenticado.
/// </summary>
public sealed class CreateBusinessInviteCommandHandler
{
    private static readonly TimeSpan InviteLifetime = TimeSpan.FromDays(7);

    private readonly IBusinessInviteRepository _businessInviteRepository;
    private readonly IIdentifierGenerator _identifierGenerator;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly string _frontendBaseUrl;

    public CreateBusinessInviteCommandHandler(
        IBusinessInviteRepository businessInviteRepository,
        IIdentifierGenerator identifierGenerator,
        IDateTimeProvider dateTimeProvider,
        IOptions<FrontendOptions> frontendOptions)
    {
        _businessInviteRepository = businessInviteRepository
            ?? throw new ArgumentNullException(nameof(businessInviteRepository));
        _identifierGenerator = identifierGenerator
            ?? throw new ArgumentNullException(nameof(identifierGenerator));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
        _frontendBaseUrl = frontendOptions?.Value.BaseUrl
            ?? throw new ArgumentNullException(nameof(frontendOptions));
    }

    public async Task<CreateBusinessInviteResponse> HandleAsync(
        CreateBusinessInviteCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var token = Guid.NewGuid().ToString("N");

        var invite = BusinessInvite.Create(
            _identifierGenerator.NewBusinessInviteId(),
            command.BusinessId,
            token,
            _dateTimeProvider.Now.Add(InviteLifetime));

        await _businessInviteRepository.AddAsync(invite, cancellationToken);

        return new CreateBusinessInviteResponse($"{_frontendBaseUrl}/registro?invite={token}");
    }
}
