using Microsoft.EntityFrameworkCore;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using TuCita.Infrastructure.Persistence;

namespace TuCita.Infrastructure.Repositories;

public sealed class ProfessionalRepository : IProfessionalRepository
{
    private readonly TuCitaDbContext _dbContext;

    public ProfessionalRepository(TuCitaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Professional?> GetByIdAsync(
        ProfessionalId id,
        CancellationToken cancellationToken = default)
        => _dbContext.Professionals.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public Task<Professional?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
        => _dbContext.Professionals.FirstOrDefaultAsync(p => p.Slug == Slug.Create(slug), cancellationToken);

    public Task<Professional?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => _dbContext.Professionals.FirstOrDefaultAsync(p => p.Email == Email.Create(email), cancellationToken);

    public Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
        => _dbContext.Professionals.AnyAsync(p => p.Slug == Slug.Create(slug), cancellationToken);

    public async Task AddAsync(
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Professionals.AddAsync(professional, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Professionals.Update(professional);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
