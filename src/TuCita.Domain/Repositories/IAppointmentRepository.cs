using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(
        AppointmentId id,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Appointment>> GetByProfessionalAsync(
        ProfessionalId professionalId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Appointment>> GetByProfessionalAndDateAsync(
        ProfessionalId professionalId,
        DateOnly date,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default);
}