using TuCita.Application.Abstractions;
using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.JoinBusiness;

/// <summary>
/// Asocia al profesional autenticado con el negocio de una invitación
/// existente. A diferencia del registro, aquí el profesional ya tiene
/// cuenta y solo se une a un negocio después de haberse registrado por
/// su cuenta.
/// </summary>
public sealed class JoinBusinessCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;
    private readonly IBusinessRepository _businessRepository;
    private readonly IBusinessInviteRepository _businessInviteRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public JoinBusinessCommandHandler(
        IProfessionalRepository professionalRepository,
        IBusinessRepository businessRepository,
        IBusinessInviteRepository businessInviteRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
        _businessRepository = businessRepository
            ?? throw new ArgumentNullException(nameof(businessRepository));
        _businessInviteRepository = businessInviteRepository
            ?? throw new ArgumentNullException(nameof(businessInviteRepository));
        _dateTimeProvider = dateTimeProvider
            ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task<JoinBusinessResponse> HandleAsync(
        JoinBusinessCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var invite = await _businessInviteRepository.GetByTokenAsync(command.InviteToken, cancellationToken);

        if (invite is null || !invite.IsValid(_dateTimeProvider.Now))
            throw new ArgumentException("La invitación no es válida o ya venció.");

        var professional = await _professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);

        if (professional is null)
            throw new NotFoundException("El profesional no existe.");

        professional.JoinBusiness(invite.BusinessId);

        await _professionalRepository.UpdateAsync(professional, cancellationToken);

        invite.Consume(_dateTimeProvider.Now);
        await _businessInviteRepository.UpdateAsync(invite, cancellationToken);

        var business = await _businessRepository.GetByIdAsync(invite.BusinessId, cancellationToken);

        return new JoinBusinessResponse(business?.Name ?? "");
    }
}
