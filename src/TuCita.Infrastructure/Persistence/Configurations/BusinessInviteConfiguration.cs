using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;
using TuCita.Infrastructure.Persistence.Converters;

namespace TuCita.Infrastructure.Persistence.Configurations;

public sealed class BusinessInviteConfiguration : IEntityTypeConfiguration<BusinessInvite>
{
    public void Configure(EntityTypeBuilder<BusinessInvite> builder)
    {
        builder.ToTable("business_invites");

        builder.HasKey(i => i.Id);
        builder.Property(i => i.Id)
            .HasConversion(id => id.Value, value => new BusinessInviteId(value))
            .HasMaxLength(26)
            .ValueGeneratedNever();

        builder.Property(i => i.BusinessId)
            .HasConversion(id => id.Value, value => new BusinessId(value))
            .HasColumnName("business_id")
            .HasMaxLength(26)
            .IsRequired();
        builder.HasIndex(i => i.BusinessId);

        builder.Property(i => i.Token)
            .HasColumnName("token")
            .IsRequired()
            .HasMaxLength(64);
        builder.HasIndex(i => i.Token).IsUnique();

        builder.Property(i => i.ExpiresAt)
            .HasConversion<UtcDateTimeConverter>()
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(i => i.UsedAt)
            .HasConversion<UtcNullableDateTimeConverter>()
            .HasColumnName("used_at");
    }
}
