using Microsoft.EntityFrameworkCore;
using Npgsql;
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

    public Task<Professional?> GetByEmailVerificationTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
        => _dbContext.Professionals.FirstOrDefaultAsync(
            p => p.EmailVerificationToken == token, cancellationToken);

    public Task<Professional?> GetByPasswordResetTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
        => _dbContext.Professionals.FirstOrDefaultAsync(
            p => p.PasswordResetToken == token, cancellationToken);

    public Task<bool> ExistsBySlugAsync(
        string slug,
        CancellationToken cancellationToken = default)
        => _dbContext.Professionals.AnyAsync(p => p.Slug == Slug.Create(slug), cancellationToken);

    public async Task AddAsync(
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Professionals.AddAsync(professional, cancellationToken);

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
                    "IX_professionals_slug" => "El slug ya se encuentra registrado.",
                    "IX_professionals_email" => "El correo ya se encuentra registrado.",
                    _ => "Ya existe un profesional con esos datos.",
                },
                ex);
        }
    }

    public async Task UpdateAsync(
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Professionals.Update(professional);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
