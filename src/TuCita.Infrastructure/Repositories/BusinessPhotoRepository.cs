using Microsoft.EntityFrameworkCore;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Infrastructure.Persistence;

namespace TuCita.Infrastructure.Repositories;

public sealed class BusinessPhotoRepository : IBusinessPhotoRepository
{
    private readonly TuCitaDbContext _dbContext;

    public BusinessPhotoRepository(TuCitaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<BusinessPhoto?> GetByIdAsync(
        BusinessPhotoId id,
        CancellationToken cancellationToken = default)
        => _dbContext.BusinessPhotos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<BusinessPhoto>> GetByBusinessIdAsync(
        BusinessId businessId,
        CancellationToken cancellationToken = default)
        => await _dbContext.BusinessPhotos
            .Where(p => p.BusinessId == businessId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<BusinessPhoto>> GetApprovedByBusinessIdAsync(
        BusinessId businessId,
        BusinessPhotoKind kind,
        CancellationToken cancellationToken = default)
        => await _dbContext.BusinessPhotos
            .Where(p =>
                p.BusinessId == businessId
                && p.Kind == kind
                && p.Status == PortfolioPhotoStatus.Approved)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<BusinessPhoto>> GetPendingAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.BusinessPhotos
            .Where(p => p.Status == PortfolioPhotoStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        BusinessPhoto photo,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.BusinessPhotos.AddAsync(photo, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        BusinessPhoto photo,
        CancellationToken cancellationToken = default)
    {
        _dbContext.BusinessPhotos.Update(photo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        BusinessPhoto photo,
        CancellationToken cancellationToken = default)
    {
        _dbContext.BusinessPhotos.Remove(photo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
