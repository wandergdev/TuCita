using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Businesses.RemoveProfessional;

/// <summary>
/// Desvincula a un profesional del negocio autenticado. El profesional
/// sigue existiendo de forma independiente, solo pierde la asociación.
/// </summary>
public sealed class RemoveProfessionalCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;

    public RemoveProfessionalCommandHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
    }

    public async Task HandleAsync(
        RemoveProfessionalCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var professional = await _professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);

        if (professional is null)
            throw new NotFoundException("El profesional no existe.");

        if (professional.BusinessId != command.BusinessId)
            throw new ForbiddenException("El profesional no pertenece al negocio autenticado.");

        professional.LeaveBusiness();

        await _professionalRepository.UpdateAsync(professional, cancellationToken);
    }
}
