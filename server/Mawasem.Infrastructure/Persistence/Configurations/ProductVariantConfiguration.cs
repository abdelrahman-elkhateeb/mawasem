using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure( EntityTypeBuilder<ProductVariant> builder )
    {
        builder.ToTable("ProductVariants");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SKU)
               .HasMaxLength(100)
               .IsRequired();

        builder.HasIndex(x => x.SKU)
               .IsUnique();

        builder.Property(x => x.StockQuantity)
               .IsRequired();

        builder.Property(x => x.IsAvailable)
               .HasDefaultValue(true);

        builder.HasOne(x => x.Product)
               .WithMany(x => x.Variants)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}