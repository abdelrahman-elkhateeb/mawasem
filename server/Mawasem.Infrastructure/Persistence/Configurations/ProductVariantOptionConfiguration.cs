using Mawasem.Domain.Catalog;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class ProductVariantOptionConfiguration : IEntityTypeConfiguration<ProductVariantOption>
{
    public void Configure( EntityTypeBuilder<ProductVariantOption> builder )
    {
        builder.ToTable("ProductVariantOptions");

        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new
        {
            x.ProductVariantId ,
            x.ProductOptionValueId
        })
               .IsUnique();

        builder.HasOne(x => x.ProductVariant)
               .WithMany(x => x.Options)
               .HasForeignKey(x => x.ProductVariantId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ProductOptionValue)
               .WithMany(x => x.ProductVariantOptions)
               .HasForeignKey(x => x.ProductOptionValueId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}