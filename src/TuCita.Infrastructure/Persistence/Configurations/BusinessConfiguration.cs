using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Domain.ValueObjects;
using TuCita.Infrastructure.Persistence.Converters;

namespace TuCita.Infrastructure.Persistence.Configurations;

public sealed class BusinessConfiguration : IEntityTypeConfiguration<Business>
{
    public void Configure(EntityTypeBuilder<Business> builder)
    {
        builder.ToTable("businesses");

        builder.HasKey(b => b.Id);
        builder.Property(b => b.Id)
            .HasConversion(id => id.Value, value => new BusinessId(value))
            .HasMaxLength(26)
            .ValueGeneratedNever();

        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(b => b.Email)
            .HasConversion(email => email.Value, value => Email.Create(value))
            .HasColumnName("email")
            .IsRequired()
            .HasMaxLength(320);
        builder.HasIndex(b => b.Email).IsUnique();

        builder.Property(b => b.Slug)
            .HasConversion(slug => slug.Value, value => Slug.Create(value))
            .HasColumnName("slug")
            .IsRequired()
            .HasMaxLength(150);
        builder.HasIndex(b => b.Slug).IsUnique();

        builder.Property(b => b.PasswordHash)
            .IsRequired();

        builder.Property(b => b.EmailVerified)
            .HasColumnName("email_verified")
            .IsRequired();

        builder.Property(b => b.EmailVerificationToken)
            .HasColumnName("email_verification_token")
            .HasMaxLength(64);
        builder.HasIndex(b => b.EmailVerificationToken);

        builder.Property(b => b.EmailVerificationTokenExpiresAt)
            .HasConversion<UtcNullableDateTimeConverter>()
            .HasColumnName("email_verification_token_expires_at");

        builder.Property(b => b.PasswordResetToken)
            .HasColumnName("password_reset_token")
            .HasMaxLength(64);
        builder.HasIndex(b => b.PasswordResetToken);

        builder.Property(b => b.PasswordResetTokenExpiresAt)
            .HasConversion<UtcNullableDateTimeConverter>()
            .HasColumnName("password_reset_token_expires_at");
    }
}
