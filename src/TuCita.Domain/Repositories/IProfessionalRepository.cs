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