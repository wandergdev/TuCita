using Microsoft.EntityFrameworkCore;
using TuCita.Domain.Entities;

namespace TuCita.Infrastructure.Persistence;

public sealed class TuCitaDbContext : DbContext
{
    public TuCitaDbContext(DbContextOptions<TuCitaDbContext> options) : base(options)
    {
    }

    public DbSet<Professional> Professionals => Set<Professional>();

    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TuCitaDbContext).Assembly);
    }
}
