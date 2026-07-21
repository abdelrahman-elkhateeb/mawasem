using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
{
    public void Configure( EntityTypeBuilder<ProductVariant> builder )
    {
        builder.ToTable("ProductVariants" , tableBuilder =>
        {
            tableBuilder.HasCheckConstraint(
                "CK_ProductVariants_StockQuantity_NonNegative" ,
                "[StockQuantity] >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.Property(x => x.SKU)
               .HasMaxLength(100)
               .IsRequired();

        builder.HasIndex(x => x.SKU)
               .IsUnique();

        builder.Property(x => x.OptionCombinationKey)
               .HasMaxLength(450)
               .IsRequired();

        builder.HasIndex(x => new
        {
            x.ProductId ,
            x.OptionCombinationKey
        })
               .IsUnique();

        builder.Property(x => x.StockQuantity)
               .IsRequired();

        builder.Property(x => x.IsAvailable)
               .HasDefaultValue(true);

        builder.Property(x => x.RowVersion)
               .IsRowVersion();

        builder.HasOne(x => x.Product)
               .WithMany(x => x.Variants)
               .HasForeignKey(x => x.ProductId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}