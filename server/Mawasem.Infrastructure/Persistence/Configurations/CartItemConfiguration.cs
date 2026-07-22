using Mawasem.Domain.Carts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public sealed class CartItemConfiguration :
    IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.ToTable(
            "CartItems",
            tableBuilder =>
            {
                tableBuilder.HasCheckConstraint(
                    "CK_CartItems_Quantity",
                    "[Quantity] > 0");

                tableBuilder.HasCheckConstraint(
                    "CK_CartItems_UnitPriceSnapshot",
                    "[UnitPriceSnapshot] >= 0");
            });

        builder.HasKey(item => item.Id);

        builder.Property(item => item.UnitPriceSnapshot)
            .HasPrecision(18, 2);

        builder.HasIndex(item => new
        {
            item.CartId,
            item.ProductVariantId
        })
            .IsUnique();

        builder.HasOne(item => item.ProductVariant)
            .WithMany()
            .HasForeignKey(item => item.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
