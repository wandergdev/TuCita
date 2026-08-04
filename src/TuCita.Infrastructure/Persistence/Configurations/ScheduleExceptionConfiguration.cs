using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Infrastructure.Persistence.Converters;

namespace TuCita.Infrastructure.Persistence.Configurations;

public sealed class ScheduleExceptionConfiguration : IEntityTypeConfiguration<ScheduleException>
{
    public void Configure(EntityTypeBuilder<ScheduleException> builder)
    {
        builder.ToTable("schedule_exceptions");

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(id => id.Value, value => new ScheduleExceptionId(value))
            .HasMaxLength(26)
            .ValueGeneratedNever();

        builder.Property(e => e.ProfessionalId)
            .HasConversion(id => id.Value, value => new ProfessionalId(value))
            .HasColumnName("professional_id")
            .HasMaxLength(26)
            .IsRequired();

        builder.Property(e => e.StartDate)
            .HasColumnName("start_date")
            .IsRequired();

        builder.Property(e => e.EndDate)
            .HasColumnName("end_date")
            .IsRequired();

        builder.Property(e => e.WorkingHours)
            .HasConversion<TimeRangeValueConverter>()
            .HasColumnName("working_hours");

        builder.Property(e => e.LunchBreak)
            .HasConversion<TimeRangeValueConverter>()
            .HasColumnName("lunch_break");

        builder.Property(e => e.Message)
            .HasColumnName("message")
            .HasMaxLength(300);

        builder.HasIndex(e => new { e.ProfessionalId, e.StartDate, e.EndDate });
    }
}
