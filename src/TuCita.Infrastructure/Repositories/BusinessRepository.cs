using Microsoft.EntityFrameworkCore;
using Npgsql;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using TuCita.Infrastructure.Persistence;

namespace TuCita.Infrastructure.Repositories;

public sealed class BusinessRepository : IBusinessRepository
{
    private readonly TuCitaDbContext _dbContext;

    public BusinessRepository(TuCitaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Business?> GetByIdAsync(
        BusinessId id,
        CancellationToken cancellationToken = default)
        => _dbContext.Businesses.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

    public Task<Business?> GetBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
        => _dbContext.Businesses.FirstOrDefaultAsync(b => b.Slug == Slug.Create(slug), cancellationToken);

    public Task<Business?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
        => _dbContext.Businesses.FirstOrDefaultAsync(b => b.Email == Email.Create(email), cancellationToken);

    public Task<Business?> GetByEmailVerificationTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
        => _dbContext.Businesses.FirstOrDefaultAsync(
            b => b.EmailVerificationToken == token, cancellationToken);

    public Task<Business?> GetByPasswordResetTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
        => _dbContext.Businesses.FirstOrDefaultAsync(
            b => b.PasswordResetToken == token, cancellationToken);

    public Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
        => _dbContext.Businesses.AnyAsync(b => b.Slug == Slug.Create(slug), cancellationToken);

    public async Task AddAsync(
        Business business,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Businesses.AddAsync(business, cancellationToken);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException postgresException
            && postgresException.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            throw new InvalidOperationException(
                postgresException.ConstraintName switch
                {
                    "IX_businesses_slug" => "El slug ya se encuentra registrado.",
                    "IX_businesses_email" => "El correo ya se encuentra registrado.",
                    _ => "Ya existe un negocio con esos datos.",
                },
                ex);
        }
    }

    public async Task UpdateAsync(
        Business business,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Businesses.Update(business);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
