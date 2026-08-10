using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;
using TuCita.Infrastructure.Persistence.Converters;

namespace TuCita.Infrastructure.Persistence.Configurations;

public sealed class ProfessionalConfiguration : IEntityTypeConfiguration<Professional>
{
    public void Configure(EntityTypeBuilder<Professional> builder)
    {
        builder.ToTable("professionals");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new ProfessionalId(value))
            .HasMaxLength(26)
            .ValueGeneratedNever();

        builder.Property(p => p.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(p => p.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(320);
        builder.HasIndex(p => p.Email).IsUnique();

        builder.Property(p => p.PhoneNumber)
            .HasConversion(phone => phone.Value, value => PhoneNumber.Create(value))
            .HasColumnName("phone_number")
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value))
            .HasColumnName("slug")
            .IsRequired()
            .HasMaxLength(150);
        builder.HasIndex(p => p.Slug).IsUnique();

        builder.Property(p => p.Category)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(p => p.AppointmentDuration)
            .HasConversion(duration => duration.Minutes, minutes => AppointmentDuration.Create(minutes))
            .HasColumnName("appointment_duration_minutes")
            .IsRequired();

        builder.Property(p => p.WorkingSchedule)
            .HasConversion<WorkingScheduleValueConverter>()
            .HasColumnName("working_schedule")
            .HasColumnType("jsonb")
            .IsRequired();

        builder.Property(p => p.PasswordHash)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .IsRequired();

        builder.Property(p => p.EmailVerified)
            .HasColumnName("email_verified")
            .IsRequired();

        builder.Property(p => p.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(64);
        builder.HasIndex(p => p.EmailVerificationToken);

        builder.Property(p => p.EmailVerificationTokenExpiresAt)
            .HasConversion<UtcNullableDateTimeConverter>()
            .HasColumnName("email_verification_token_expires_at");

        builder.Property(p => p.PasswordResetToken)
            .HasColumnName("password_reset_token")
            .HasMaxLength(64);
        builder.HasIndex(p => p.PasswordResetToken);

        builder.Property(p => p.PasswordResetTokenExpiresAt)
            .HasConversion<UtcNullableDateTimeConverter>()
            .HasColumnName("password_reset_token_expires_at");

        builder.Property(p => p.BusinessId)
            .HasConversion(
                id => id == null ? null : id.Value,
                value => value == null ? null : new BusinessId(value))
            .HasColumnName("business_id")
            .HasMaxLength(26);
        builder.HasIndex(p => p.BusinessId);
    }
}
