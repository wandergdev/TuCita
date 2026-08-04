using Microsoft.EntityFrameworkCore;
using TuCita.Domain.Entities;
using TuCita.Domain.Enums;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
using TuCita.Domain.ValueObjects;
using TuCita.Infrastructure.Persistence;

namespace TuCita.Infrastructure.Repositories;

public sealed class AppointmentRepository : IAppointmentRepository
{
    private readonly TuCitaDbContext _dbContext;

    public AppointmentRepository(TuCitaDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    public Task<Appointment?> GetByIdAsync(
        AppointmentId id,
        CancellationToken cancellationToken = default)
        => _dbContext.Appointments.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);

    public async Task<IReadOnlyCollection<Appointment>> GetByProfessionalAsync(
        ProfessionalId professionalId,
        AppointmentStatus? status,
        bool ascending,
        int skip,
        int take,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.Appointments
            .Where(a => a.ProfessionalId == professionalId);

        if (status is { } requestedStatus)
            query = query.Where(a => a.Status == requestedStatus);

        query = ascending
            ? query.OrderBy(a => a.Date).ThenBy(a => a.StartTime)
            : query.OrderByDescending(a => a.Date).ThenByDescending(a => a.StartTime);

        return await query.Skip(skip).Take(take).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyCollection<Appointment>> GetByProfessionalAndDateAsync(
        ProfessionalId professionalId,
        DateOnly date,
        CancellationToken cancellationToken = default)
        => await _dbContext.Appointments
            .Where(a => a.ProfessionalId == professionalId && a.Date == date)
            .ToListAsync(cancellationToken);

    public Task<bool> HasScheduledAppointmentAsync(
        ProfessionalId professionalId,
        PhoneNumber customerPhone,
        DateOnly date,
        CancellationToken cancellationToken = default)
        => _dbContext.Appointments.AnyAsync(a =>
            a.ProfessionalId == professionalId
            && a.CustomerPhone == customerPhone
            && a.Date == date
            && a.Status == AppointmentStatus.Scheduled,
            cancellationToken);

    public async Task<AppointmentStatsResult> GetStatsByProfessionalAsync(
        ProfessionalId professionalId,
        DateOnly from,
        DateOnly to,
        CancellationToken cancellationToken = default)
    {
        var rangeQuery = _dbContext.Appointments
            .Where(a => a.ProfessionalId == professionalId && a.Date >= from && a.Date <= to);

        var statusCounts = await rangeQuery
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        var distinctCustomers = await rangeQuery
            .Where(a => a.Status == AppointmentStatus.Completed)
            .Select(a => a.CustomerPhone)
            .Distinct()
            .CountAsync(cancellationToken);

        int CountFor(AppointmentStatus status)
            => statusCounts.FirstOrDefault(x => x.Status == status)?.Count ?? 0;

        return new AppointmentStatsResult(
            CountFor(AppointmentStatus.Completed),
            CountFor(AppointmentStatus.NoShow),
            CountFor(AppointmentStatus.Cancelled),
            distinctCustomers);
    }

    public async Task<IReadOnlyCollection<Appointment>> GetPendingReminderCandidatesAsync(
        DateOnly date,
        CancellationToken cancellationToken = default)
        => await _dbContext.Appointments
            .Where(a => a.Date == date
                && a.Status == AppointmentStatus.Scheduled
                && a.ReminderSentAt == null)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Appointments.AddAsync(appointment, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default)
    {
        _dbContext.Appointments.Update(appointment);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
