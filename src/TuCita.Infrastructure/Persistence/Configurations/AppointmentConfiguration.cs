using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;
using TuCita.Infrastructure.Persistence.Converters;

namespace TuCita.Infrastructure.Persistence.Configurations;

public sealed class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("appointments");

        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id)
            .HasConversion(id => id.Value, value => new AppointmentId(value))
            .HasMaxLength(26)
            .ValueGeneratedNever();

        builder.Property(a => a.ProfessionalId)
            .HasConversion(id => id.Value, value => new ProfessionalId(value))
            .HasColumnName("professional_id")
            .HasMaxLength(26)
            .IsRequired();

        builder.Property(a => a.CustomerName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.CustomerPhone)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value))
            .HasColumnName("customer_phone")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Date)
            .IsRequired();

        builder.Property(a => a.StartTime)
            .HasColumnName("start_time")
            .IsRequired();

        builder.Property(a => a.EndTime)
            .HasColumnName("end_time")
            .IsRequired();

        builder.Property(a => a.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.ReminderSentAt)
            .HasConversion<UtcNullableDateTimeConverter>()
            .HasColumnName("reminder_sent_at");

        builder.HasIndex(a => new { a.ProfessionalId, a.Date });
        builder.HasIndex(a => new { a.ProfessionalId, a.Status, a.Date, a.StartTime });
    }
}
