using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;

namespace TuCita.Domain.Repositories;

public interface IBusinessPhotoRepository
{
    Task<BusinessPhoto?> GetByIdAsync(
        BusinessPhotoId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Todas las fotos del negocio (cualquier tipo y estado), ordenadas
    /// por fecha de subida, para "mis fotos" y para validar los límites.
    /// </summary>
    Task<IReadOnlyCollection<BusinessPhoto>> GetByBusinessIdAsync(
        BusinessId businessId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Solo las fotos aprobadas de un tipo, ordenadas por fecha de
    /// subida, para mostrar en el perfil público.
    /// </summary>
    Task<IReadOnlyCollection<BusinessPhoto>> GetApprovedByBusinessIdAsync(
        BusinessId businessId,
        BusinessPhotoKind kind,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cola de moderación: todas las fotos pendientes de cualquier
    /// negocio, ordenadas por fecha de subida.
    /// </summary>
    Task<IReadOnlyCollection<BusinessPhoto>> GetPendingAsync(
        CancellationToken cancellationToken = default);

    Task AddAsync(
        BusinessPhoto photo,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        BusinessPhoto photo,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        BusinessPhoto photo,
        CancellationToken cancellationToken = default);
}
