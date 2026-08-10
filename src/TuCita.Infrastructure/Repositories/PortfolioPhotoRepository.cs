using Microsoft.EntityFrameworkCore;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Infrastructure.Persistence;

namespace TuCita.Infrastructure.Repositories;

public sealed class PortfolioPhotoRepository : IPortfolioPhotoRepository
{
    private readonly TuCitaDbContext _dbContext;

    public PortfolioPhotoRepository(TuCitaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<PortfolioPhoto?> GetByIdAsync(
        PortfolioPhotoId id,
        CancellationToken cancellationToken = default)
        => _dbContext.PortfolioPhotos.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<PortfolioPhoto>> GetByProfessionalIdAsync(
        ProfessionalId professionalId,
        CancellationToken cancellationToken = default)
        => await _dbContext.PortfolioPhotos
            .Where(p => p.ProfessionalId == professionalId)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PortfolioPhoto>> GetApprovedByProfessionalIdAsync(
        ProfessionalId professionalId,
        CancellationToken cancellationToken = default)
        => await _dbContext.PortfolioPhotos
            .Where(p => p.ProfessionalId == professionalId && p.Status == PortfolioPhotoStatus.Approved)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<PortfolioPhoto>> GetPendingAsync(
        CancellationToken cancellationToken = default)
        => await _dbContext.PortfolioPhotos
            .Where(p => p.Status == PortfolioPhotoStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        PortfolioPhoto photo,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.PortfolioPhotos.AddAsync(photo, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        PortfolioPhoto photo,
        CancellationToken cancellationToken = default)
    {
        _dbContext.PortfolioPhotos.Update(photo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        PortfolioPhoto photo,
        CancellationToken cancellationToken = default)
    {
        _dbContext.PortfolioPhotos.Remove(photo);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
