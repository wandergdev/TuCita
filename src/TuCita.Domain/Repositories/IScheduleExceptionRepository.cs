using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Repositories;

public interface IScheduleExceptionRepository
{
    Task<ScheduleException?> GetByIdAsync(
        ScheduleExceptionId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Excepción vigente para esa fecha (si la hay).
    /// </summary>
    Task<ScheduleException?> GetForDateAsync(
        ProfessionalId professionalId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Excepciones del profesional que sigan vigentes desde <paramref name="fromDate"/>
    /// en adelante, ordenadas por fecha de inicio.
    /// </summary>
    Task<IReadOnlyCollection<ScheduleException>> GetByProfessionalIdAsync(
        ProfessionalId professionalId,
        DateOnly fromDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Excepciones existentes que se solapen con el rango indicado, para
    /// evitar crear excepciones duplicadas/contradictorias.
    /// </summary>
    Task<IReadOnlyCollection<ScheduleException>> GetOverlappingAsync(
        ProfessionalId professionalId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        ScheduleException scheduleException,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        ScheduleException scheduleException,
        CancellationToken cancellationToken = default);
}
