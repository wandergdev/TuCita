using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Repositories;

public interface IBusinessRepository
{
    Task<Business?> GetByIdAsync(
        BusinessId id,
        CancellationToken cancellationToken = default);

    Task<Business?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task<Business?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default);

    Task<Business?> GetByEmailVerificationTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<Business?> GetByPasswordResetTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Business business,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Business business,
        CancellationToken cancellationToken = default);
}
