using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TuCita.Domain.Entities;
using TuCita.Domain.Identifiers;

namespace TuCita.Infrastructure.Persistence.Configurations;

public sealed class PortfolioPhotoConfiguration : IEntityTypeConfiguration<PortfolioPhoto>
{
    public void Configure(EntityTypeBuilder<PortfolioPhoto> builder)
    {
        builder.ToTable("portfolio_photos");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .HasConversion(id => id.Value, value => new PortfolioPhotoId(value))
            .HasMaxLength(26)
            .ValueGeneratedNever();

        builder.Property(p => p.ProfessionalId)
            .HasConversion(id => id.Value, value => new ProfessionalId(value))
            .HasColumnName("professional_id")
            .HasMaxLength(26)
            .IsRequired();

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

        builder.HasIndex(p => new { p.ProfessionalId, p.Status });
    }
}
