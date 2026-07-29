using Microsoft.EntityFrameworkCore;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.Repositories;
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
        CancellationToken cancellationToken = default)
        => await _dbContext.Appointments
            .Where(a => a.ProfessionalId == professionalId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyCollection<Appointment>> GetByProfessionalAndDateAsync(
        ProfessionalId professionalId,
        DateOnly date,
        CancellationToken cancellationToken = default)
        => await _dbContext.Appointments
            .Where(a => a.ProfessionalId == professionalId && a.Date == date)
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
