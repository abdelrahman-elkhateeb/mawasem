using Mawasem.Domain.Orders;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mawasem.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure( EntityTypeBuilder<OrderItem> builder )
    {
        builder.ToTable("OrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProductNameAr)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.ProductNameEn)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.SKU)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.UnitPrice)
            .HasPrecision(18 , 2)
            .IsRequired();

        builder.Property(x => x.DiscountAmount)
            .HasPrecision(18 , 2)
            .HasDefaultValue(0m)
            .IsRequired();

        builder.Property(x => x.Quantity)
            .IsRequired();

        builder.Property(x => x.TotalPrice)
            .HasPrecision(18 , 2)
            .IsRequired();

        builder.Property(x => x.RefundedQuantity)
            .HasDefaultValue(0)
            .IsRequired();

        // Deleting an order deletes its order items.
        builder.HasOne(x => x.Order)
            .WithMany(x => x.OrderItems)
            .HasForeignKey(x => x.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Product variants must not be deleted when historical
        // order items reference them.
        builder.HasOne(x => x.ProductVariant)
            .WithMany()
            .HasForeignKey(x => x.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.OrderId);

        builder.HasIndex(x => x.ProductVariantId);

        builder.HasIndex(x => x.SKU);

        builder.HasIndex(x => new
        {
            x.OrderId ,
            x.ProductVariantId
        });
    }
}