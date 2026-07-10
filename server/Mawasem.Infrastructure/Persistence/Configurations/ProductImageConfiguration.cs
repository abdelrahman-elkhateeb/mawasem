using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
{
    public void Configure( EntityTypeBuilder<ProductImage> builder )
    {
        builder.ToTable("ProductImages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ImageUrl)
               .HasMaxLength(500)
               .IsRequired();

        builder.Property(x => x.IsPrimary)
               .HasDefaultValue(false);

        builder.HasOne(x => x.ProductVariant)
               .WithMany(x => x.Images)
               .HasForeignKey(x => x.ProductVariantId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}