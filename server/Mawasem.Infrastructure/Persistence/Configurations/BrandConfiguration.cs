using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class BrandConfiguration : IEntityTypeConfiguration<Brand>
{
    public void Configure( EntityTypeBuilder<Brand> builder )
    {
        builder.ToTable("Brands");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Name , name =>
        {
            name.Property(x => x.English)
                .HasColumnName("NameEn")
                .HasMaxLength(100)
                .IsRequired();

            name.Property(x => x.Arabic)
                .HasColumnName("NameAr")
                .HasMaxLength(100)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Description , description =>
        {
            description.Property(x => x.English)
                .HasColumnName("DescriptionEn")
                .HasMaxLength(500);

            description.Property(x => x.Arabic)
                .HasColumnName("DescriptionAr")
                .HasMaxLength(500);
        });

        builder.Property(x => x.LogoUrl)
               .HasMaxLength(500);

        builder.Property(x => x.IsActive)
               .HasDefaultValue(true);
    }
}