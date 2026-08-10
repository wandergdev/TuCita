using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Repositories;

public interface IProfessionalRepository
{
    Task<Professional?> GetByIdAsync(
        ProfessionalId id,
        CancellationToken cancellationToken = default);

    Task<Professional?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<Professional?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<Professional?> GetByEmailVerificationTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<Professional?> GetByPasswordResetTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Profesionales activos asociados a un negocio (ej. los barberos de
    /// una peluquería), para el roster del dueño y la página pública del
    /// negocio.
    /// </summary>
    Task<IReadOnlyCollection<Professional>> GetByBusinessIdAsync(
        BusinessId businessId,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Professional professional,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Professional professional,
        CancellationToken cancellationToken = default);
}