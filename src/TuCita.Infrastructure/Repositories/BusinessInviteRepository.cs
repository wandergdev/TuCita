using Microsoft.EntityFrameworkCore;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Infrastructure.Persistence;

namespace TuCita.Infrastructure.Repositories;

public sealed class BusinessInviteRepository : IBusinessInviteRepository
{
    private readonly TuCitaDbContext _dbContext;

    public BusinessInviteRepository(TuCitaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<BusinessInvite?> GetByIdAsync(
        BusinessInviteId id,
        CancellationToken cancellationToken = default)
        => _dbContext.BusinessInvites.FirstOrDefaultAsync(i => i.Id == id, cancellationToken);

    public Task<BusinessInvite?> GetByTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
        => _dbContext.BusinessInvites.FirstOrDefaultAsync(i => i.Token == token, cancellationToken);

    public async Task AddAsync(
        BusinessInvite invite,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.BusinessInvites.AddAsync(invite, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        BusinessInvite invite,
        CancellationToken cancellationToken = default)
    {
        _dbContext.BusinessInvites.Update(invite);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
