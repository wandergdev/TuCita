using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Repositories;

public interface IBusinessInviteRepository
{
    Task<BusinessInvite?> GetByIdAsync(
        BusinessInviteId id,
        CancellationToken cancellationToken = default);

    Task<BusinessInvite?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BusinessInvite invite,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BusinessInvite invite,
        CancellationToken cancellationToken = default);
}
