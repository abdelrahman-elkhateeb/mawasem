using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure( EntityTypeBuilder<Product> builder )
    {
        builder.ToTable("Products");

        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Name , name =>
        {
            name.Property(x => x.English)
                .HasColumnName("NameEn")
                .HasMaxLength(200)
                .IsRequired();

            name.Property(x => x.Arabic)
                .HasColumnName("NameAr")
                .HasMaxLength(200)
                .IsRequired();
        });

        builder.OwnsOne(x => x.Description , description =>
        {
            description.Property(x => x.English)
                .HasColumnName("DescriptionEn")
                .HasMaxLength(2000);

            description.Property(x => x.Arabic)
                .HasColumnName("DescriptionAr")
                .HasMaxLength(2000);
        });

        builder.Property(x => x.OriginalPrice)
               .HasColumnType("decimal(18,2)");

        builder.Property(x => x.CurrentPrice)
               .HasColumnType("decimal(18,2)");

        builder.Property(x => x.Slug)
               .HasMaxLength(300);

        builder.HasIndex(x => x.Slug)
               .IsUnique();

        builder.HasOne(x => x.Brand)
               .WithMany(x => x.Products)
               .HasForeignKey(x => x.BrandId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Season)
               .WithMany(x => x.Products)
               .HasForeignKey(x => x.SeasonId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}