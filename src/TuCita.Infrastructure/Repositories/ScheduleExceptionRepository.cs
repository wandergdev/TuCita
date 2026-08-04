using Microsoft.EntityFrameworkCore;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Infrastructure.Persistence;

namespace TuCita.Infrastructure.Repositories;

public sealed class ScheduleExceptionRepository : IScheduleExceptionRepository
{
    private readonly TuCitaDbContext _dbContext;

    public ScheduleExceptionRepository(TuCitaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<ScheduleException?> GetByIdAsync(
        ScheduleExceptionId id,
        CancellationToken cancellationToken = default)
        => _dbContext.ScheduleExceptions.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public Task<ScheduleException?> GetForDateAsync(
        ProfessionalId professionalId,
        DateOnly date,
        CancellationToken cancellationToken = default)
        => _dbContext.ScheduleExceptions.FirstOrDefaultAsync(
            e => e.ProfessionalId == professionalId && e.StartDate <= date && e.EndDate >= date,
            cancellationToken);

    public async Task<IReadOnlyCollection<ScheduleException>> GetByProfessionalIdAsync(
        ProfessionalId professionalId,
        DateOnly fromDate,
        CancellationToken cancellationToken = default)
        => await _dbContext.ScheduleExceptions
            .Where(e => e.ProfessionalId == professionalId && e.EndDate >= fromDate)
            .OrderBy(e => e.StartDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<ScheduleException>> GetOverlappingAsync(
        ProfessionalId professionalId,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken = default)
        => await _dbContext.ScheduleExceptions
            .Where(e =>
                e.ProfessionalId == professionalId
                && e.StartDate <= endDate
                && e.EndDate >= startDate)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        ScheduleException scheduleException,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.ScheduleExceptions.AddAsync(scheduleException, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(
        ScheduleException scheduleException,
        CancellationToken cancellationToken = default)
    {
        _dbContext.ScheduleExceptions.Remove(scheduleException);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
