using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Repositories;

public interface IPortfolioPhotoRepository
{
    Task<PortfolioPhoto?> GetByIdAsync(
        PortfolioPhotoId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Todas las fotos del profesional (cualquier estado), ordenadas por
    /// fecha de subida, para "mi portafolio" y para validar el límite.
    /// </summary>
    Task<IReadOnlyCollection<PortfolioPhoto>> GetByProfessionalIdAsync(
        ProfessionalId professionalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solo las fotos aprobadas del profesional, ordenadas por fecha de
    /// subida, para mostrar en perfiles públicos.
    /// </summary>
    Task<IReadOnlyCollection<PortfolioPhoto>> GetApprovedByProfessionalIdAsync(
        ProfessionalId professionalId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cola de moderación: todas las fotos pendientes de cualquier
    /// profesional, ordenadas por fecha de subida.
    /// </summary>
    Task<IReadOnlyCollection<PortfolioPhoto>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        PortfolioPhoto photo,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        PortfolioPhoto photo,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        PortfolioPhoto photo,
        CancellationToken cancellationToken = default);
}
