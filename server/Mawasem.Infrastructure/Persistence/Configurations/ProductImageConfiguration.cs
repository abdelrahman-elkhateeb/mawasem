using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration
    : IEntityTypeConfiguration<ProductImage>
{
    public void Configure(
        EntityTypeBuilder<ProductImage> builder )
    {
        builder.ToTable("ProductImages" , tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_ProductImages_DisplayOrder_NonNegative" ,
                "[DisplayOrder] >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.StorageKey)
            .HasMaxLength(500)
            .IsRequired();

        builder.HasIndex(x => x.StorageKey)
            .IsUnique();

        builder.Property(x => x.IsPrimary)
            .HasDefaultValue(false);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductId ,
            x.ColorOptionValueId ,
            x.DisplayOrder
        })
            .IsUnique()
            .HasFilter(null)
            .HasDatabaseName(
                "UX_ProductImages_GalleryDisplayOrder");

        builder.HasIndex(x => new
        {
            x.ProductId ,
            x.ColorOptionValueId
        })
            .IsUnique()
            .HasFilter("[IsPrimary] = 1")
            .HasDatabaseName(
                "UX_ProductImages_GalleryPrimary");

        builder.HasOne(x => x.Product)
            .WithMany(x => x.Images)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ColorOptionValue)
            .WithMany(x => x.ColorImages)
            .HasForeignKey(x => x.ColorOptionValueId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}