using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;

namespace TuCita.Domain.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(
        AppointmentId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Citas de un profesional, opcionalmente filtradas por estado,
    /// ordenadas por fecha/hora y paginadas.
    /// </summary>
    Task<IReadOnlyCollection<Appointment>> GetByProfessionalAsync(
        ProfessionalId professionalId,
        AppointmentStatus? status,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Appointment>> GetByProfessionalAndDateAsync(
        ProfessionalId professionalId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Citas de un profesional dentro de un rango de fechas (inclusive),
    /// para detectar conflictos al marcar días como no laborables.
    /// </summary>
    Task<IReadOnlyCollection<Appointment>> GetByProfessionalAndDateRangeAsync(
        ProfessionalId professionalId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indica si ese teléfono ya tiene una cita programada con este
    /// profesional en esa fecha, para advertir antes de una reserva
    /// duplicada.
    /// </summary>
    Task<bool> HasScheduledAppointmentAsync(
        ProfessionalId professionalId,
        PhoneNumber customerPhone,
        DateOnly date,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Conteos agregados de citas de un profesional dentro de un rango
    /// de fechas, para el panel de métricas.
    /// </summary>
    Task<AppointmentStatsResult> GetStatsByProfessionalAsync(
        ProfessionalId professionalId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Citas programadas de esa fecha a las que aún no se les ha
    /// enviado el recordatorio.
    /// </summary>
    Task<IReadOnlyCollection<Appointment>> GetPendingReminderCandidatesAsync(
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default);
}