using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Infrastructure.Persistence.Configurations;

public sealed class BusinessPhotoConfiguration : IEntityTypeConfiguration<BusinessPhoto>
{
    public void Configure(EntityTypeBuilder<BusinessPhoto> builder)
    {
        builder.ToTable("business_photos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new BusinessPhotoId(value))
            .HasMaxLength(26)
            .ValueGeneratedNever();

        builder.Property(p => p.BusinessId)
            .HasConversion(id => id.Value, value => new BusinessId(value))
            .HasColumnName("business_id")
            .HasMaxLength(26)
            .IsRequired();

        builder.Property(p => p.Kind)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.ImageUrl)
            .HasColumnName("image_url")
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(p => p.PublicId)
            .HasColumnName("public_id")
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(p => p.Caption)
            .HasColumnName("caption")
            .HasMaxLength(200);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(p => new { p.BusinessId, p.Kind, p.Status });
    }
}
