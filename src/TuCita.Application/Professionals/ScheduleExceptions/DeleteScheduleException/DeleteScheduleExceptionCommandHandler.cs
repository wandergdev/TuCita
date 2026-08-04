using TuCita.Application.Common.Exceptions;
using TuCita.Domain.Repositories;

namespace TuCita.Application.Professionals.ScheduleExceptions.DeleteScheduleException;

/// <summary>
/// Se encarga de eliminar una excepción de horario.
/// </summary>
public sealed class DeleteScheduleExceptionCommandHandler
{
    private readonly IScheduleExceptionRepository _scheduleExceptionRepository;

    public DeleteScheduleExceptionCommandHandler(IScheduleExceptionRepository scheduleExceptionRepository)
    {
        _scheduleExceptionRepository = scheduleExceptionRepository
            ?? throw new ArgumentNullException(nameof(scheduleExceptionRepository));
    }

    public async Task HandleAsync(
        DeleteScheduleExceptionCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var scheduleException = await _scheduleExceptionRepository.GetByIdAsync(
            command.ScheduleExceptionId,
            cancellationToken);

        if (scheduleException is null)
            throw new NotFoundException("La excepción de horario solicitada no existe.");

        if (scheduleException.ProfessionalId != command.RequestingProfessionalId)
            throw new ForbiddenException("La excepción de horario no pertenece al profesional autenticado.");

        await _scheduleExceptionRepository.DeleteAsync(scheduleException, cancellationToken);
    }
}
