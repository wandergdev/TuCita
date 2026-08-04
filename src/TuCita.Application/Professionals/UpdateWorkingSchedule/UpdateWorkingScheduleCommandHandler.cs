using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.UpdateWorkingSchedule;

/// <summary>
/// Se encarga de reemplazar el horario habitual de un profesional.
/// </summary>
public sealed class UpdateWorkingScheduleCommandHandler
{
    private readonly IProfessionalRepository _professionalRepository;

    public UpdateWorkingScheduleCommandHandler(IProfessionalRepository professionalRepository)
    {
        _professionalRepository = professionalRepository
            ?? throw new ArgumentNullException(nameof(professionalRepository));
    }

    public async Task HandleAsync(
        UpdateWorkingScheduleCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var professional = await _professionalRepository.GetByIdAsync(command.ProfessionalId, cancellationToken);

        if (professional is null)
            throw new NotFoundException("El profesional solicitado no existe.");

        professional.UpdateWorkingSchedule(command.WorkingSchedule);

        await _professionalRepository.UpdateAsync(professional, cancellationToken);
    }
}
