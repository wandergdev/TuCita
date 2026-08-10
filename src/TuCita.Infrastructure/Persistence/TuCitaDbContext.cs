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

    public DbSet<ScheduleException> ScheduleExceptions => Set<ScheduleException>();

    public DbSet<Business> Businesses => Set<Business>();

    public DbSet<BusinessInvite> BusinessInvites => Set<BusinessInvite>();

    public DbSet<PortfolioPhoto> PortfolioPhotos => Set<PortfolioPhoto>();

    public DbSet<BusinessPhoto> BusinessPhotos => Set<BusinessPhoto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TuCitaDbContext).Assembly);
    }
}
