using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.GetBusinessInvite;

/// <summary>
/// Valida una invitación de negocio y devuelve su nombre, para mostrarlo
/// en la pantalla de registro del profesional invitado.
/// </summary>
public sealed class GetBusinessInviteQueryHandler
{
    private readonly IBusinessInviteRepository _businessInviteRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public GetBusinessInviteQueryHandler(
        IBusinessInviteRepository businessInviteRepository,
        IBusinessRepository businessRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _businessInviteRepository = businessInviteRepository
            ?? throw new ArgumentNullException(nameof(businessInviteRepository));
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<GetBusinessInviteResponse> HandleAsync(
        GetBusinessInviteQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);

        var invite = await _businessInviteRepository.GetByTokenAsync(query.Token, cancellationToken);

        if (invite is null || !invite.IsValid(_dateTimeProvider.Now))
            throw new NotFoundException("La invitación no es válida o ya venció.");

        var business = await _businessRepository.GetByIdAsync(invite.BusinessId, cancellationToken);

        if (business is null)
            throw new NotFoundException("La invitación no es válida o ya venció.");

        return new GetBusinessInviteResponse(business.Name);
    }
}
